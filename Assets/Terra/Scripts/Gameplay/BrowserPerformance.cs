using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
namespace Terra.Gameplay
{
    // Opt-in diagnostic capture of the real player loop. F9 starts a 30-second run.
    public sealed class BrowserPerformance
    {
        readonly List<float> frames=new List<float>(2400),preview=new List<float>(),placement=new List<float>();
        float start;bool running;int cameraEvents,unfocusedFrames;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void TerraLandscapeReady();
        [DllImport("__Internal")] static extern void TerraMetricsStart();
        [DllImport("__Internal")] static extern void TerraMetricsEnd();
        [DllImport("__Internal")] static extern void TerraFeedbackReady(int kind);
#endif
        public static void Memory(string checkpoint)
        {
            Debug.Log("TERRA_MEMORY: "+JsonUtility.ToJson(new MemorySample{checkpoint=checkpoint,allocatedBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),reservedBytes=UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong(),managedBytes=GC.GetTotalMemory(false),meshes=Resources.FindObjectsOfTypeAll<Mesh>().Length,materials=Resources.FindObjectsOfTypeAll<Material>().Length}));
        }
        [Serializable] sealed class MemorySample{public string checkpoint;public long allocatedBytes,reservedBytes,managedBytes;public int meshes,materials;}
        public static void Ready(){
#if UNITY_WEBGL && !UNITY_EDITOR
            TerraLandscapeReady();
#endif
        }
        public static void Feedback(int kind){
#if UNITY_WEBGL && !UNITY_EDITOR
            TerraFeedbackReady(kind);
#endif
        }
        public void Start(){frames.Clear();preview.Clear();placement.Clear();cameraEvents=unfocusedFrames=0;start=Time.realtimeSinceStartup;running=true;Debug.Log("TERRA_PERFORMANCE_STARTED: 30-second actual camera/placement path");
#if UNITY_WEBGL && !UNITY_EDITOR
            TerraMetricsStart();
#endif
        }
        public void Tick()
        {
            if(!running)return;frames.Add(Time.unscaledDeltaTime*1000);if(!Application.isFocused)unfocusedFrames++;
            if(Time.realtimeSinceStartup-start<30)return;running=false;
            float seconds=Time.realtimeSinceStartup-start;
            var report=new Report{seconds=seconds,frames=frames.Count,averageFps=frames.Count/seconds,frameP50Ms=Percentile(frames,.5f),frameP95Ms=Percentile(frames,.95f),frameP99Ms=Percentile(frames,.99f),maxFrameMs=frames.Max(),framesOver33Ms=frames.Count(f=>f>33.34f),framesOver200Ms=frames.Count(f=>f>200),previewEvents=preview.Count,maxPreviewCpuMs=preview.Count>0?preview.Max():-1,placementEvents=placement.Count,maxPlacementCpuMs=placement.Count>0?placement.Max():-1,cameraEvents=cameraEvents,unfocusedFrames=unfocusedFrames};
            Debug.Log("TERRA_PERFORMANCE: "+JsonUtility.ToJson(report));
            var nature=UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(FindObjectsSortMode.None);
            Debug.Log($"TERRA_RENDER_SUBMISSIONS: submitted={nature.Sum(n=>n.LastSubmittedBatches)}; culled={nature.Sum(n=>n.LastCulledBatches)}; groundcoverCulled={nature.Sum(n=>n.LastCulledGroundcoverBatches)}; groups={nature.Length}");
            var combined=UnityEngine.Object.FindFirstObjectByType<Terra.Rendering.NatureSubmission>();
            if(combined!=null)Debug.Log($"TERRA_RENDER_CONSOLIDATION: sourceBatches={combined.LastSourceBatches}; drawCalls={combined.LastDrawCalls}; savedDrawCalls={combined.LastSourceBatches-combined.LastDrawCalls}");
#if UNITY_WEBGL && !UNITY_EDITOR
            TerraMetricsEnd();
#endif
        }
        public void Preview(double milliseconds){if(running)preview.Add((float)milliseconds);}
        public void Placement(double milliseconds){if(running)placement.Add((float)milliseconds);}
        public void Camera(){if(running)cameraEvents++;}
        static float Percentile(List<float> input,float p){var a=input.OrderBy(x=>x).ToArray();return a[Mathf.Clamp(Mathf.CeilToInt(a.Length*p)-1,0,a.Length-1)];}
        [Serializable] public sealed class Report
        {
            public float seconds,averageFps,frameP50Ms,frameP95Ms,frameP99Ms,maxFrameMs,maxPreviewCpuMs,maxPlacementCpuMs;
            public int frames,framesOver33Ms,framesOver200Ms,previewEvents,placementEvents,cameraEvents,unfocusedFrames;
        }
    }
}
