using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using Terra.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Read-only asset/runtime census. Does not save scenes, prefabs or player progress.
public static class TreeRenderingAudit
{
    const BindingFlags Fields=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    const float CssHeight=876,Aspect=1707.3333f/876;
    [Serializable] public class MeshRow {public string name;public int vertices,submeshes;public long triangles;}
    [Serializable] public class PrefabRow {public string name;public int lodGroups;public string[] meshFilters;}
    [Serializable] public class CameraRow
    {
        public string name;public float zoom;public Vector3 position;
        public int visibleBatches,submittedInstances,offscreenInstancesInVisibleBatches;
        public int[] selectedLodInstances=new int[3],perTreeLodInstances=new int[3];
        public long batchSelectedTriangles,perTreeLodTriangles;
        public int depthClampedToOneBatches;
        public float averageBatchDiameter;
    }
    [Serializable] public class Result
    {
        public string sourceCommit,unityVersion,scope;
        public MeshRow[] importedMeshes;public PrefabRow[] prefabs;
        public int forestInstances,forestBatches,forestMaterials,forestSubmeshes;
        public long allForestLod0Triangles;
        public CameraRow[] cameras;
        public string[] limits;
    }
    sealed class Batch
    {
        public Mesh[] lods;public int submesh;public Material material;
        public Matrix4x4[] transforms;public Bounds bounds;public float height;
    }
    public static void RunBatch()
    {
        try {Run();EditorApplication.Exit(0);}
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    public static void BuildExperiment()
    {
        try
        {
            Run();
            // Discard the census's temporary objects and camera before packaging the real scene.
            EditorSceneManager.OpenScene("Assets/Terra/Scenes/TerraMain.unity");
            if(UnityEngine.Object.FindObjectsByType<InstancedNature>(FindObjectsInactive.Include).Length!=0)throw new Exception("Temporary census forest leaked into the build scene.");
            Phase1Setup.PackageFixture();
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=true;PlayerSettings.WebGL.nameFilesAsHashes=true;
            PlayerSettings.WebGL.template="PROJECT:Terra";
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/Terra/Scenes/TerraMain.unity"},locationPathName="Builds/TreeAudit",target=BuildTarget.WebGL,
                extraScriptingDefines=new[]{"TERRA_TREE_AUDIT"},options=BuildOptions.None
            });
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Tree audit Web build failed.");
            string htmlPath="Builds/TreeAudit/index.html";
            string html=File.ReadAllText(htmlPath);
            string panel="<aside style='position:fixed;left:350px;top:130px;z-index:10000;background:#fff9dc;color:#172e24;padding:12px;border:2px solid #172e24;font:15px system-ui'><b>LOCAL TREE A/B TEST</b><br>Same scene and camera. Samples: 2 s warmup + 12 s.<br><button data-tree-mode='normal'>Trees on</button> <button data-tree-mode='off'>Trees off</button> <button data-tree-mode='shadows'>Shadows only</button><div id='tree-test-status'>Choose a mode after the landscape loads.</div></aside><script>document.querySelectorAll('[data-tree-mode]').forEach(b=>b.onclick=()=>{if(!player)return;player.SendMessage('TreeAuditControl','SetMode',b.dataset.treeMode);document.getElementById('tree-test-status').textContent=b.textContent+' · sample started; result in console';canvas.focus()});</script>";
            File.WriteAllText(htmlPath,html.Replace("</body>",panel+"</body>"));
            Debug.Log("TREE_AUDIT_WEB_BUILD_PASSED");EditorApplication.Exit(0);
        }
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static object Field(object o,string name)=>o.GetType().GetField(name,Fields).GetValue(o);
    static string SourceCommit()
    {
        try
        {
            using(var process=System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("git","rev-parse HEAD") {
                WorkingDirectory=Directory.GetParent(Application.dataPath).FullName,
                UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true
            }))
            {
                if(!process.WaitForExit(5000)){process.Kill();return "unknown";}
                return process.ExitCode==0?process.StandardOutput.ReadToEnd().Trim():"unknown";
            }
        }
        catch{return "unknown";}
    }
    static long Triangles(Mesh mesh,int submesh)=>checked((long)mesh.GetIndexCount(submesh)/3);
    static Bounds TransformBounds(Bounds local,Matrix4x4 matrix)
    {
        var e=local.extents;var x=matrix.MultiplyVector(new Vector3(e.x,0,0));
        var y=matrix.MultiplyVector(new Vector3(0,e.y,0));var z=matrix.MultiplyVector(new Vector3(0,0,e.z));
        var ext=new Vector3(Mathf.Abs(x.x)+Mathf.Abs(y.x)+Mathf.Abs(z.x),Mathf.Abs(x.y)+Mathf.Abs(y.y)+Mathf.Abs(z.y),Mathf.Abs(x.z)+Mathf.Abs(y.z)+Mathf.Abs(z.z));
        return new Bounds(matrix.MultiplyPoint3x4(local.center),ext*2);
    }
    static int Lod(float pixels,Mesh[] meshes)=>pixels<18&&meshes[2]!=null?2:pixels<42&&meshes[1]!=null?1:0;
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Terra/Scenes/TerraMain.unity");
        var meshes=AssetDatabase.LoadAllAssetsAtPath("Assets/Terra/Art/SymphonieOak/Model/M_Oak01.fbx").OfType<Mesh>().ToArray();
        if(meshes.Length!=3)throw new Exception("Expected three source oak LOD meshes.");
        var prefabs=Enumerable.Range(1,4).Select(i=>Resources.Load<GameObject>("Nature/SolidOak0"+i)).ToArray();
        var sceneMain=UnityEngine.Object.FindAnyObjectByType<TerraMain>();
        var world=JsonUtility.FromJson<WorldData>(File.ReadAllText("worlds/meadowbrook-v1/world.json"));
        var scenario=JsonUtility.FromJson<Scenario>(File.ReadAllText("worlds/meadowbrook-v1/scenario.json"));
        var hydro=JsonUtility.FromJson<RunoffConfiguration>(File.ReadAllText("worlds/meadowbrook-v1/hydrology.json"));
        AnnualSpatialOpportunities.AddWind(world,hydro);
        BuildActionRuntime.Expand(sceneMain.buildActionCatalog,ref world,ref scenario);
        var root=new GameObject("Tree audit temporary landscape");
        var landscape=new LandscapeRenderer(world,sceneMain.terrainMaterial,root.transform,sceneMain.treePrefab,sceneMain.cropPrefab,sceneMain.grassPrefab,sceneMain.shrubPrefab);
        landscape.Build();
        var forest=root.transform.Find("Textured mesh forest study").GetComponent<InstancedNature>();
        var batches=new List<Batch>();
        foreach(object b in (IEnumerable)typeof(InstancedNature).GetField("batches",Fields).GetValue(forest))
            batches.Add(new Batch {lods=new[]{(Mesh)Field(b,"mesh"),(Mesh)Field(b,"medium"),(Mesh)Field(b,"distant")},
                submesh=(int)Field(b,"submesh"),material=(Material)Field(b,"material"),transforms=(Matrix4x4[])Field(b,"transforms"),
                bounds=(Bounds)Field(b,"bounds"),height=(float)Field(b,"maxHeight")});
        if(batches.Count==0||batches.Any(b=>b.lods.Any(m=>m==null)))throw new Exception("Custom forest renderer did not bind all three LOD meshes.");
        var camera=Camera.main;camera.orthographic=false;camera.fieldOfView=36;camera.aspect=Aspect;camera.nearClipPlane=1;camera.farClipPlane=6000;
        var result=new Result {
            sourceCommit=SourceCommit(),unityVersion=Application.unityVersion,
            scope="Current baseline forest, same world, prefab transforms and custom batch LOD math as production. Fresh run; no projects or game saves changed.",
            importedMeshes=meshes.Select(m=>new MeshRow{name=m.name,vertices=m.vertexCount,submeshes=m.subMeshCount,triangles=Enumerable.Range(0,m.subMeshCount).Sum(i=>Triangles(m,i))}).OrderByDescending(m=>m.triangles).ToArray(),
            prefabs=prefabs.Select(p=>new PrefabRow{name=p.name,lodGroups=p.GetComponentsInChildren<LODGroup>(true).Length,meshFilters=p.GetComponentsInChildren<MeshFilter>(true).Select(f=>f.name+" -> "+f.sharedMesh.name).ToArray()}).ToArray(),
            forestBatches=batches.Count,forestInstances=batches.Sum(b=>b.transforms.Length),forestMaterials=batches.Select(b=>b.material).Distinct().Count(),
            forestSubmeshes=batches.Select(b=>b.submesh).Distinct().Count(),allForestLod0Triangles=batches.Sum(b=>Triangles(b.lods[0],b.submesh)*b.transforms.Length),
            cameras=new[]{Sample("Opening view",115,new Vector3(-193,25,-43)),Sample("Close view",72,new Vector3(-193,25,-43)),Sample("Wide view",580,new Vector3(0,30,0)),Sample("Maximum zoom out",1000,new Vector3(0,30,0))},
            limits=new[]{"Triangle counts are color-pass candidate submissions from batch/frustum tests, not a captured GPU command stream or final on-screen triangles.","Per-tree estimate applies the same screen-pixel LOD thresholds to each individual tree. It does not reduce tree count or change source meshes.","Real-time shadow passes can add work beyond these totals. No FPS improvement or causal tree attribution is claimed by this census.","Meshes are imported Unity buffers. FBX control-point counts differ where import splits normals/UVs. Sizes are CPU vertex counts, not unique shaded screen pixels."}
        };
        Directory.CreateDirectory("Logs/TreeAudit");
        File.WriteAllText("Logs/TreeAudit/tree-rendering-audit.json",JsonUtility.ToJson(result,true));
        Debug.Log("TERRA_TREE_AUDIT: "+JsonUtility.ToJson(result));
        // The process exits without saving; Unity tears down these diagnostic objects.
        CameraRow Sample(string name,float zoom,Vector3 target)
        {
            var rotation=Quaternion.Euler(37,-32,0);var forward=rotation*Vector3.forward;
            target.y=landscape.Height(target.x,target.z)+8;
            var offset=forward*(zoom/Mathf.Tan(18*Mathf.Deg2Rad));var proposed=target-offset;
            target.y=Mathf.Max(target.y,landscape.Height(proposed.x,proposed.z)+35+offset.y);
            var position=target-offset;position.y=Mathf.Max(position.y,landscape.Height(position.x,position.z)+35);
            camera.transform.SetPositionAndRotation(position,rotation);
            var planes=GeometryUtility.CalculateFrustumPlanes(camera);
            var row=new CameraRow{name=name,zoom=zoom,position=position};
            float pixelFactor=CssHeight/2,span=Mathf.Tan(18*Mathf.Deg2Rad),diameter=0;
            foreach(var b in batches)
            {
                if(!GeometryUtility.TestPlanesAABB(planes,b.bounds))continue;
                row.visibleBatches++;diameter+=b.bounds.size.magnitude;
                float rawDepth=Vector3.Dot(b.bounds.center-position,forward)-b.bounds.extents.magnitude;
                if(rawDepth<=1)row.depthClampedToOneBatches++;
                int selected=Lod(b.height*pixelFactor/(Mathf.Max(1,rawDepth)*span),b.lods);
                row.selectedLodInstances[selected]+=b.transforms.Length;row.submittedInstances+=b.transforms.Length;
                row.batchSelectedTriangles+=Triangles(b.lods[selected],b.submesh)*b.transforms.Length;
                foreach(var matrix in b.transforms)
                {
                    var bounds=TransformBounds(b.lods[0].bounds,matrix);
                    if(!GeometryUtility.TestPlanesAABB(planes,bounds))row.offscreenInstancesInVisibleBatches++;
                    float depth=Mathf.Max(1,Vector3.Dot(bounds.center-position,forward)-bounds.extents.magnitude);
                    int perTree=Lod(bounds.size.y*pixelFactor/(depth*span),b.lods);
                    row.perTreeLodInstances[perTree]++;row.perTreeLodTriangles+=Triangles(b.lods[perTree],b.submesh);
                }
            }
            row.averageBatchDiameter=row.visibleBatches==0?0:diameter/row.visibleBatches;
            return row;
        }
    }
}
