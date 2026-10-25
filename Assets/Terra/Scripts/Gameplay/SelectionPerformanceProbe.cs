using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terra.Contracts;
using Terra.Core;
using Terra.UI;
using Unity.Profiling;
using UnityEngine;

namespace Terra.Gameplay
{
    // Opt-in native-player diagnostic. Never queues projects or writes a player's save.
    public sealed class SelectionPerformanceProbe : MonoBehaviour
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        readonly List<float> frames = new List<float>();
        readonly List<float> cpu = new List<float>(), gpu = new List<float>();
        readonly List<Event> events = new List<Event>();
        readonly List<Sample> samples = new List<Sample>();
        ProfilerRecorder cpuRecorder, gpuRecorder;
        string phase, output;
        int unfocused;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return;
#else
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "--terra-selection-perf");
            if (i < 0) return;
            var probe = new GameObject("Selection performance diagnostic").AddComponent<SelectionPerformanceProbe>();
            probe.output = i + 1 < args.Length ? args[i + 1] : Path.Combine(Application.persistentDataPath, "selection-performance.json");
#endif
        }

        static T Field<T>(object value, string name) => (T)value.GetType().GetField(name, Private).GetValue(value);
        static void Invoke(object value, string name, params object[] args) => value.GetType().GetMethod(name, Private).Invoke(value, args);
        IEnumerator Start()
        {
            yield return null;
            var main = FindAnyObjectByType<TerraMain>();
            // Set this diagnostic instance only, preserving the user's motion preference.
            typeof(WorldAtlas).GetProperty("ReducedMotion").SetValue(WorldAtlas.Instance, true);
            var args = Environment.GetCommandLineArgs();
            int locArg = Array.IndexOf(args, "--terra-location");
            int location = locArg >= 0 ? int.Parse(args[locArg + 1]) : 0;
            WorldAtlas.Instance.TravelTo(location);
            float timeout = Time.realtimeSinceStartup + 180;
            while ((!main.LandscapeIsReady || WorldAtlas.Instance.Traveling) && Time.realtimeSinceStartup < timeout) yield return null;
            if (!main.LandscapeIsReady || WorldAtlas.Instance.Traveling) { Debug.LogError("Selection diagnostic: landscape entry timed out"); Application.Quit(2); yield break; }
            var hud = Field<TerraHud>(main, "hud");
            hud.CloseModal(); hud.CloseAnnualOverview();
            Invoke(main, "Cancel");
            if(args.Contains("--terra-placement-stages"))
            {
                ProfilePlacement(main);
                Application.Quit();yield break;
            }
            yield return new WaitForSecondsRealtime(5);
            cpuRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Main Thread Frame Time");
            gpuRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GPU Frame Time");
            phase = "idle";
            yield return new WaitForSecondsRealtime(8);
            SavePhase();
            var world = Field<WorldData>(main, "world");
            var rules = Field<BuildActionRules>(main, "buildActionRules");
            var available = world.sites.Where(s => !s.protectedSite).ToArray();
            // Largest parcel exercises the costly surrounding-vegetation path.
            var large = available.OrderByDescending(s => (s.polygon.Max(p => p.x) - s.polygon.Min(p => p.x)) * (s.polygon.Max(p => p.z) - s.polygon.Min(p => p.z))).First();
            phase = "tool-selection";
            foreach (var rule in rules.actions)
            {
                FieldInfo hover = main.GetType().GetField("hoverId", Private);
                hover.SetValue(main, large.id);
                Measure("choose/" + rule.id, () => Invoke(main, "Choose", rule.id));
                yield return new WaitForSecondsRealtime(.65f);
            }
            SavePhase();
            phase = "building-hover";
            Invoke(main, "Choose", "low-density-housing");
            foreach (var site in available.Take(16))
            {
                Measure("hover/" + site.id, () => Invoke(main, "SetHover", site));
                yield return new WaitForSecondsRealtime(.5f);
            }
            SavePhase();
            phase = "camera";
            Invoke(main, "Cancel");
            float end = Time.realtimeSinceStartup + 6;
            var yaw = main.GetType().GetField("yaw", Private);
            while (Time.realtimeSinceStartup < end)
            {
                yaw.SetValue(main, (float)yaw.GetValue(main) + Time.unscaledDeltaTime * 8);
                Invoke(main, "ApplyView"); yield return null;
            }
            SavePhase();
            // Exercise the real placement/undo callbacks against disposable save keys.
            // The player's regular town/chapter keys are never changed by this stage.
            var session = Field<AnnualTownSession>(main, "annual");
            var chapters = Field<AnnualChapterSession>(main, "chapters");
            session.saveKey = "myterrain.selection-diagnostic.town";
            chapters.saveKey = "myterrain.selection-diagnostic.chapters";
            session.Initialize(session.Definition, "selection-diagnostic", false);
            chapters.ResetForRun();
            Invoke(main, "RefreshLandscape"); Invoke(main, "Refresh", new object[] { null });
            phase = "placement-and-undo";
            foreach (var site in available.Where(s => rules.Rejection(s, "low-density-housing") == null).Take(3))
            {
                Invoke(main, "SetHover", site); Invoke(main, "Choose", "low-density-housing");
                Measure("place/" + site.id, () => Invoke(main, "ConfirmEdit"));
                yield return new WaitForSecondsRealtime(.8f);
                Measure("undo/" + site.id, () => Invoke(main, "Undo"));
                yield return new WaitForSecondsRealtime(.8f);
            }
            SavePhase();
            // History makes repeated deep snapshots more expensive. Check the same
            // selection path after six accepted years in the disposable run too.
            for(int year=0;year<6;year++)
            {
                var request=session.BeginYear("selection-diagnostic/year-"+year,session.State.revision);
                session.CompleteYear(new AuthoredAnnualConsequenceProvider().Resolve(request));
                session.ContinueYear(session.State.revision);
            }
            Invoke(main, "RefreshLandscape"); Invoke(main, "Refresh", new object[] { null });
            yield return new WaitForSecondsRealtime(2);
            phase="year7-tool-selection";
            foreach(var rule in rules.actions)
            {
                main.GetType().GetField("hoverId",Private).SetValue(main,large.id);
                Measure("year7-choose/"+rule.id,()=>Invoke(main,"Choose",rule.id));
                yield return new WaitForSecondsRealtime(.5f);
            }
            SavePhase();
            Invoke(main, "SetHover", large); Invoke(main, "Choose", "low-density-housing");
            Invoke(main, "FocusSite");
            yield return new WaitForSecondsRealtime(1);
            ScreenCapture.CaptureScreenshot(Path.ChangeExtension(output, ".png"));
            yield return new WaitForSecondsRealtime(.5f);
            PlayerPrefs.DeleteKey(session.saveKey); PlayerPrefs.DeleteKey(chapters.saveKey); PlayerPrefs.Save();
            cpuRecorder.Dispose(); gpuRecorder.Dispose();
            var report = new Report { gpu = SystemInfo.graphicsDeviceName, api = SystemInfo.graphicsDeviceType.ToString(),
                unity = Application.unityVersion, width = Screen.width, height = Screen.height, sites = world.sites.Length,
                largeSite = large.id, samples = samples.ToArray(), events = events.ToArray() };
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            Debug.Log("SELECTION_PERFORMANCE_COMPLETE: " + output);
            Application.Quit();
        }

        void Update()
        {
            if (phase == null) return;
            frames.Add(Time.unscaledDeltaTime * 1000);
            if (!Application.isFocused) unfocused++;
            if (cpuRecorder.Valid && cpuRecorder.LastValue > 0) cpu.Add(cpuRecorder.LastValue / 1000000f);
            if (gpuRecorder.Valid && gpuRecorder.LastValue > 0) gpu.Add(gpuRecorder.LastValue / 1000000f);
        }
        void Measure(string name, Action action)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew(); action();
            events.Add(new Event { name = name, milliseconds = timer.Elapsed.TotalMilliseconds });
        }
        void ProfilePlacement(TerraMain main)
        {
            var session=Field<AnnualTownSession>(main,"annual");
            var chapters=Field<AnnualChapterSession>(main,"chapters");
            session.saveKey="myterrain.selection-diagnostic.town";chapters.saveKey="myterrain.selection-diagnostic.chapters";
            session.Initialize(session.Definition,"placement-stage-profile",false);chapters.ResetForRun();
            var world=Field<WorldData>(main,"world");var rules=Field<BuildActionRules>(main,"buildActionRules");
            foreach(var site in world.sites.Where(s=>!s.protectedSite&&rules.Rejection(s,"low-density-housing")==null).Take(3))
            {
                Measure("queue/"+site.id,()=>session.QueueAction(site.id,"low-density-housing",session.State.revision));
                Measure("save/"+site.id,()=>Invoke(main,"SaveAnnual"));
                Measure("render/"+site.id,()=>Invoke(main,"RefreshLandscape"));
                Measure("hud-refresh/"+site.id,()=>Invoke(main,"Refresh",new object[]{null}));
                Measure("undo-model/"+site.id,()=>session.UndoPlan(session.State.revision));
                Measure("undo-save/"+site.id,()=>Invoke(main,"SaveAnnual"));
                Measure("undo-render/"+site.id,()=>Invoke(main,"RefreshLandscape"));
                Measure("undo-hud/"+site.id,()=>Invoke(main,"Refresh",new object[]{null}));
            }
            PlayerPrefs.DeleteKey(session.saveKey);PlayerPrefs.DeleteKey(chapters.saveKey);PlayerPrefs.Save();
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
            File.WriteAllText(output,JsonUtility.ToJson(new Report{gpu=SystemInfo.graphicsDeviceName,api=SystemInfo.graphicsDeviceType.ToString(),width=Screen.width,height=Screen.height,events=events.ToArray()},true));
            Debug.Log("SELECTION_STAGE_PROFILE_COMPLETE: "+output);
        }
        void SavePhase()
        {
            samples.Add(new Sample { phase = phase, frames = frames.Count, averageFps = frames.Count * 1000 / frames.Sum(),
                p50 = P(frames, .5f), p95 = P(frames, .95f), p99 = P(frames, .99f), max = frames.DefaultIfEmpty().Max(),
                over33 = frames.Count(f => f > 33.334f), over100 = frames.Count(f => f > 100),
                cpuP95 = P(cpu, .95f), gpuP95 = P(gpu, .95f), unfocused = unfocused });
            phase = null; frames.Clear(); cpu.Clear(); gpu.Clear(); unfocused = 0;
        }
        static float P(List<float> values, float p) { if (values.Count == 0) return -1; var a = values.OrderBy(x => x).ToArray(); return a[Mathf.CeilToInt(a.Length * p) - 1]; }
        [Serializable] public sealed class Report { public string gpu, api, unity, largeSite; public int width, height, sites; public Sample[] samples; public Event[] events; }
        [Serializable] public sealed class Sample { public string phase; public int frames, over33, over100, unfocused; public float averageFps, p50, p95, p99, max, cpuP95, gpuP95; }
        [Serializable] public sealed class Event { public string name; public double milliseconds; }
    }
}
