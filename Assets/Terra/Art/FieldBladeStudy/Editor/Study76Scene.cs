using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terra.Contracts;
using Terra.Gameplay;
using Terra.Rendering;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// Play-only comparison. No production scene, source pack or annual state is saved.
public static class Study76Scene
{
    const string Assets = "Assets/Terra/Art/FieldBladeStudy/";
    const string Output = "Builds/ArtResearch/Study76/";
    const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    static GameObject root, template;
    static Material material;
    static Transform baseline;
    static string sourceBefore, annualBefore;
    static TerraMain main;
    static WorldData world;
    static LandscapeRenderer landscape;

    public static object Inspect()
    {
        Connect();
        return new { frame = Time.frameCount, annual = main.Diagnostic,
            atlas = WorldAtlas.Instance.Diagnostic,
            sites = world.sites.Select(s => new { s.id, s.protectedSite,
                centerX = s.polygon.Average(p => p.x), centerZ = s.polygon.Average(p => p.z),
                s.analysisAreaM2, s.baselineCover }).ToArray() };
    }

    public static object Apply(string siteId = "grid-07-05", float spacing = .4f)
    {
        Connect();
        if(root != null)throw new InvalidOperationException("Remove the previous study before applying another.");
        if(spacing < .25f || spacing > 1)throw new ArgumentOutOfRangeException(nameof(spacing));
        var site = world.sites.Single(s => s.id == siteId);
        if(site.protectedSite)throw new InvalidOperationException("Protected site cannot be replaced.");
        sourceBefore = JsonUtility.ToJson(world); annualBefore = main.Diagnostic;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Assets + "FieldCluster.prefab");
        if(prefab == null)throw new InvalidOperationException("Build the study assets first.");
        template = Object.Instantiate(prefab); template.name = "FieldCluster"; template.SetActive(false);
        material = new Material(AssetDatabase.LoadAssetAtPath<Material>(Assets + "NativeGradient.mat"));
        foreach(var renderer in template.GetComponentsInChildren<MeshRenderer>(true))renderer.sharedMaterial = material;
        root = new GameObject("Study76 source-derived field");
        var crop = landscape.GetType().GetMethod("VisualCropAt", Fields);
        var route = landscape.GetType().GetMethod("NearLine", Fields);
        bool Inside(float x, float z)
        {
            bool inside = false;
            for(int i = 0, j = site.polygon.Length - 1; i < site.polygon.Length; j = i++)
            {
                var a = site.polygon[i]; var b = site.polygon[j];
                if((a.z > z) != (b.z > z) && x < (b.x-a.x)*(z-a.z)/(b.z-a.z)+a.x)inside = !inside;
            }
            return inside;
        }
        float left=site.polygon.Min(p=>p.x), right=site.polygon.Max(p=>p.x);
        float bottom=site.polygon.Min(p=>p.z), top=site.polygon.Max(p=>p.z);
        var random = new System.Random(7601);
        var cells = new Dictionary<Vector2Int,List<Matrix4x4>>();
        int count=0, rejected=0;
        for(float z=bottom+spacing*.5f;z<top;z+=spacing)
        for(float x=left+spacing*.5f;x<right;x+=spacing)
        {
            float px=x+((float)random.NextDouble()-.5f)*spacing*.6f;
            float pz=z+((float)random.NextDouble()-.5f)*spacing*.6f;
            // Authored cultivation lanes; eligibility still comes from the real source cover.
            bool lane=Mathf.Abs(Mathf.Repeat(px-left+12,24)-12)<.45f;
            if(!Inside(px,pz)||lane||!(bool)crop.Invoke(landscape,new object[]{px,pz})
                ||(bool)route.Invoke(landscape,new object[]{px,pz,5f})){rejected++;continue;}
            float scale=Mathf.Lerp(.85f,1.12f,(float)random.NextDouble());
            var matrix=Matrix4x4.TRS(new Vector3(px,landscape.Height(px,pz)+.015f,pz),
                Quaternion.Euler(0,(float)random.NextDouble()*360,0),Vector3.one*scale);
            var key=new Vector2Int(Mathf.FloorToInt(px/12),Mathf.FloorToInt(pz/12));
            if(!cells.TryGetValue(key,out var list)){list=new List<Matrix4x4>();cells.Add(key,list);}
            list.Add(matrix);count++;
        }
        foreach(var cell in cells)
        {
            var go=new GameObject("Study76 cell "+cell.Key);go.transform.SetParent(root.transform,false);
            go.AddComponent<InstancedNature>().Add(template,cell.Value,Color.white);
        }
        baseline=((Dictionary<string,Transform>)landscape.GetType().GetField("baselines",Fields).GetValue(landscape))[siteId];
        baseline.gameObject.SetActive(false);
        Check();
        var report=new {siteId,spacing,plants=count,cells=cells.Count,rejected,
            maximumNearTriangles=count*7,maximumMediumTriangles=count*3,maximumFarTriangles=count,
            sourceUnchanged=true,annualUnchanged=true,annual=annualBefore,
            scope="Actual Play-only setup; geometry totals are upper bounds, not GPU timings or art acceptance."};
        Directory.CreateDirectory(Output);File.WriteAllText(Output+"application.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
        return report;
    }

    public static void Palette(bool native)
    {
        if(material==null)throw new InvalidOperationException("Apply the study first.");
        material.SetColor("_RootColor",native?new Color(.054902f,.556863f,.439216f):new Color(.24f,.29f,.06f));
        material.SetColor("_MidColor",native?new Color(.321569f,.890196f,.341176f):new Color(.53f,.59f,.16f));
        material.SetColor("_TipColor",native?new Color(.4991f,.92f,.3588f):new Color(.70f,.74f,.30f));
        Check();
    }

    public static void Wind(bool running)
    {
        if(material==null)throw new InvalidOperationException("Apply the study first.");
        material.SetFloat("_StudyTime",running?-1:0);
    }

    public static void Check()
    {
        if(sourceBefore!=JsonUtility.ToJson(world)||annualBefore!=main.Diagnostic)
            throw new InvalidOperationException("Source world or annual state changed during the study.");
    }

    public static void Remove()
    {
        if(root!=null)Check();
        if(baseline!=null)baseline.gameObject.SetActive(true);
        if(root!=null)Object.DestroyImmediate(root);
        if(template!=null)Object.DestroyImmediate(template);
        if(material!=null)Object.DestroyImmediate(material);
        root=null;template=null;material=null;baseline=null;
    }

    static void Connect()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Play mode is required.");
        main=Object.FindAnyObjectByType<TerraMain>();
        if(main==null||main.Diagnostic=="loading")throw new InvalidOperationException("Choose Wisconsin and await arrival first.");
        world=(WorldData)main.GetType().GetField("world",Fields).GetValue(main);
        landscape=(LandscapeRenderer)main.GetType().GetField("landscape",Fields).GetValue(main);
    }
}
