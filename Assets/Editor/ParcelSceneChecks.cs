using System;
using System.IO;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using Terra.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Editor-only verification stays with the generated scenario overlay, outside player builds.
public static class ParcelSceneChecks
{
    public static void BuildAndCheck(){Run();TerraBuild.BuildWeb();}
    public static void Run()
    {
        int checks=0;
        void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
        EditorSceneManager.OpenScene("Assets/Terra/Scenes/TerraMain.unity");
        var main=Object.FindFirstObjectByType<TerraMain>();
        main.enabled=false;
        Directory.CreateDirectory("docs/evidence/map-parcels");
        foreach(var location in LocationProfile.All)
        {
            string root="worlds/"+location.PackId+"/";
            var world=JsonUtility.FromJson<WorldData>(File.ReadAllText(root+"world.json"));
            var scenario=JsonUtility.FromJson<Scenario>(File.ReadAllText(root+"scenario.json"));
            var hydro=JsonUtility.FromJson<RunoffConfiguration>(File.ReadAllText(root+"hydrology.json"));
            var pack=JsonUtility.FromJson<UniformParcelPack>(Resources.Load<TextAsset>("Parcels/"+location.PackId).text);
            UniformParcels.Apply(world,hydro,pack,location.PackId);
            var rules=BuildActionRuntime.Expand(main.buildActionCatalog,ref world,ref scenario);
            var definition=FullAnnualGameplay.Create(world,scenario,location.PackId,rules,new LandscapeRunoff(world,scenario,hydro));
            Check(definition.version=="4.0.0","New layout definition");
            Check(location.ParcelAnnualSaveKey!=location.AnnualSaveKey && location.ParcelChapterSaveKey!=location.ChapterSaveKey,"Prior saves isolated");
            var scenery=new GameObject("Parcel scene check "+location.Name);
            var renderer=new LandscapeRenderer(world,main.terrainMaterial,scenery.transform,main.treePrefab,main.cropPrefab,main.grassPrefab,main.shrubPrefab);
            renderer.ConfigureBuildActions(main.buildActionCatalog,rules);renderer.Build();renderer.BuildStartingSettlement(pack);
            foreach(var initial in pack.initialBuildings)
            {
                var site=world.sites.Single(s=>s.id==initial.siteId);
                var inherited=scenery.transform.Find(initial.title+" · inherited baseline");
                Check(inherited!=null && inherited.GetComponentsInChildren<MeshRenderer>().Length>0,"Inherited model is rendered: "+initial.title);
                Check(rules.Layout(site,rules.Find(initial.actionId)).Valid,"Starting foundation fits: "+initial.title);
                renderer.Apply(site,null);
                Check(inherited.gameObject.activeSelf,"Baseline refresh preserves initial building");
            }
            // Captures inspect metric buildings/ground in Edit Mode. Instanced vegetation's
            // Update loop is not running; these are not playable-scene or browser acceptance.
            var camera=Camera.main;camera.orthographic=true;camera.orthographicSize=150;
            camera.transform.rotation=Quaternion.Euler(55,-25,0);
            var initialSites=pack.initialBuildings.Select(b=>world.sites.Single(s=>s.id==b.siteId)).ToArray();
            float x=initialSites.Average(s=>s.polygon.Average(p=>p.x)),z=initialSites.Average(s=>s.polygon.Average(p=>p.z));
            camera.transform.position=new Vector3(x,rules.Height(x,z),z)-camera.transform.forward*900;
            camera.farClipPlane=3000;
            var target=new RenderTexture(1280,900,24);camera.targetTexture=target;
            // Warm the URP material/property upload before recording the first location.
            for(int warm=0;warm<3;warm++)camera.Render();
            var old=RenderTexture.active;RenderTexture.active=target;
            var pixels=new Texture2D(1280,900,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1280,900),0,0);pixels.Apply();
            File.WriteAllBytes("docs/evidence/map-parcels/"+location.PackId+"-editor.png",pixels.EncodeToPNG());
            RenderTexture.active=old;camera.targetTexture=null;Object.DestroyImmediate(pixels);Object.DestroyImmediate(target);
            Object.DestroyImmediate(scenery);
        }
        File.WriteAllText("docs/evidence/map-parcels/unity-checks.json","{\"checks\":"+checks+",\"scope\":\"Unity import, real prefab fit, baseline refresh and Edit Mode captures; no browser acceptance\"}");
        Debug.Log("PARCEL_SCENE_CHECKS_PASS: "+checks);
    }
}
