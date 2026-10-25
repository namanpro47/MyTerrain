using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class SelectionPerformanceChecks
{

    const System.Reflection.BindingFlags Private=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
    static T Field<T>(object value,string name)=>(T)value.GetType().GetField(name,Private).GetValue(value);
    static void Call(object value,string name,params object[] args)=>value.GetType().GetMethod(name,Private).Invoke(value,args);
    public static void Run()
    {
        int checks=0;
        void Check(bool ok,string why){checks++;if(!ok)throw new Exception("Preview lifecycle: "+why);}
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Terra/Scenes/TerraMain.unity");
        var main=UnityEngine.Object.FindAnyObjectByType<Terra.Gameplay.TerraMain>();main.enabled=false;
        var world=JsonUtility.FromJson<Terra.Contracts.WorldData>(File.ReadAllText("worlds/meadowbrook-v1/world.json"));
        var scenario=JsonUtility.FromJson<Terra.Contracts.Scenario>(File.ReadAllText("worlds/meadowbrook-v1/scenario.json"));
        var hydro=JsonUtility.FromJson<Terra.Core.RunoffConfiguration>(File.ReadAllText("worlds/meadowbrook-v1/hydrology.json"));
        var pack=JsonUtility.FromJson<Terra.Core.UniformParcelPack>(Resources.Load<TextAsset>("Parcels/meadowbrook-v1").text);
        Terra.Core.UniformParcels.Apply(world,hydro,pack,"meadowbrook-v1");
        var rules=Terra.Gameplay.BuildActionRuntime.Expand(main.buildActionCatalog,ref world,ref scenario);
        var root=new GameObject("Preview performance lifecycle checks").transform;
        var renderer=new Terra.Rendering.LandscapeRenderer(world,main.terrainMaterial,root,main.treePrefab,main.cropPrefab,main.grassPrefab,main.shrubPrefab,true);
        renderer.ConfigureBuildActions(main.buildActionCatalog,rules);
        renderer.Build();
        var baselines=Field<System.Collections.Generic.Dictionary<string,Transform>>(renderer,"baselines");
        var site=world.sites.Where(s=>!s.protectedSite&&rules.Rejection(s,"low-density-housing")==null)
            .OrderByDescending(s=>(s.polygon.Max(p=>p.x)-s.polygon.Min(p=>p.x))*(s.polygon.Max(p=>p.z)-s.polygon.Min(p=>p.z))).First();
        var baseline=baselines[site.id];
        var sourceFilters=baseline.GetComponentsInChildren<MeshFilter>(true);
        var sourceMeshes=sourceFilters.Select(f=>f.sharedMesh).ToArray();
        var instances=baseline.GetComponentsInChildren<Terra.Rendering.InstancedNature>(true);
        var sourceJson=JsonUtility.ToJson(world);
        Check(instances.Length>0,"fixture has source vegetation");
        renderer.Ghost(site,true,"low-density-housing");
        var ghost=Field<Transform>(renderer,"ghost");
        Check(baseline.gameObject.activeSelf,"building preview retains the source field");
        Check(ghost.GetComponentsInChildren<Terra.Rendering.InstancedNature>(true).Length==0,"building preview does not regenerate the surrounding field");
        var meshes=Field<System.Collections.Generic.List<Mesh>>(renderer,"meshes");
        int owned=meshes.Count,children=root.childCount;
        renderer.Ghost(site,true,"low-density-housing");
        Check(Field<Transform>(renderer,"ghost")==ghost&&meshes.Count==owned&&root.childCount==children,"unchanged preview is reused");
        foreach(var nature in instances)
        {
            var batches=(System.Collections.IEnumerable)Field<object>(nature,"batches");
            foreach(var batch in batches)
            {
                var type=batch.GetType();
                var source=(Matrix4x4[])type.GetField("transforms").GetValue(batch);
                var filtered=(Matrix4x4[])type.GetField("filtered").GetValue(batch);
                int count=(int)type.GetField("filteredCount").GetValue(batch);
                Check(count>=0&&count<=source.Length&&filtered.Length==source.Length,"filtered instance count is bounded");
                Check(filtered!=source,"source matrices have a separate immutable buffer");
                for(int i=0;i<count;i++)Check(System.Array.IndexOf(source,filtered[i])>=0,"every surviving plant comes from the source field");
            }
        }
        renderer.ClearGhost();
        Check(baseline.gameObject.activeSelf,"cancel restores baseline visibility");
        renderer.Ghost(site,true,"restore-grass");
        Check(baseline.gameObject.activeSelf,"land preview keeps the landscape visible");
        Check(Field<Transform>(renderer,"ghost").GetComponentsInChildren<Terra.Rendering.InstancedNature>(true).Length==0,"land preview uses an exact boundary without synchronous replanting");
        renderer.ClearGhost();
        for(int i=0;i<sourceFilters.Length;i++)Check(sourceFilters[i].sharedMesh==sourceMeshes[i],"cancel restores the exact original soil mesh");
        renderer.Apply(site,"low-density-housing");
        Check(baseline.gameObject.activeSelf,"accepted building keeps the surrounding source vegetation");
        renderer.PreviewBaseline(site);
        for(int i=0;i<sourceFilters.Length;i++)Check(sourceFilters[i].sharedMesh==sourceMeshes[i],"removal preview restores the original field");
        renderer.ClearGhost();
        string replacement=rules.actions.FirstOrDefault(a=>!a.landscape&&a.id!="wind"&&a.id!="low-density-housing"&&rules.Rejection(site,a.id)==null)?.id??"low-density-housing";
        renderer.Ghost(site,true,replacement);renderer.ClearGhost();
        Check(baseline.gameObject.activeSelf,"cancel replacement restores the committed building field");
        renderer.Apply(site,"restore-grass");
        Check(!baseline.gameObject.activeSelf,"land treatment replaces the source field");
        renderer.Ghost(site,true,"low-density-housing");
        Check(baseline.gameObject.activeSelf,"building replacement previews the original surrounding cover");
        renderer.ClearGhost();
        Check(!baseline.gameObject.activeSelf,"cancel replacement restores accepted land treatment");
        renderer.Apply(site,null);
        Check(baseline.gameObject.activeSelf,"undo/remove restores the source field");
        for(int i=0;i<sourceFilters.Length;i++)Check(sourceFilters[i].sharedMesh==sourceMeshes[i],"undo restores source mesh identity");
        Check(JsonUtility.ToJson(world)==sourceJson,"preview/commit rendering never mutates analytical world data");
        renderer.Dispose();UnityEngine.Object.DestroyImmediate(root.gameObject);
        Directory.CreateDirectory("docs/evidence/selection-performance");
        File.WriteAllText("docs/evidence/selection-performance/lifecycle-checks.json","{\"passed\":true,\"assertions\":"+checks+"}");
        Debug.Log("SELECTION_LIFECYCLE_PASS: "+checks);
    }

    public static void BuildNative()
    {
        Phase1Setup.PackageFixture();
        PlayerSettings.enableFrameTimingStats = true;
        var output = Path.GetFullPath("Builds/SelectionPerformance/MyTerrain.exe");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "Assets/Terra/Scenes/TerraMain.unity" },
            locationPathName = output, target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Native performance build failed");
        Debug.Log("SELECTION_NATIVE_BUILD_PASS: " + output);
    }
}
