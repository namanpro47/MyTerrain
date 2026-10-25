using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Terra.Gameplay;
public static class Phase1Setup
{
    public static void CreateScene()
    {
        Directory.CreateDirectory("Assets/Terra/Scenes");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
        var main=new GameObject("TerraMain").AddComponent<TerraMain>();
        main.buildActionCatalog=AssetDatabase.LoadAssetAtPath<Terra.Art.BuildActionCatalog>("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
        main.terrainMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Phase0Material.mat");
        main.treePrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Terra/Toolkit/Prefabs/tree_oak.prefab");
        main.cropPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Terra/Toolkit/Prefabs/crops_wheatStageB.prefab");
        main.grassPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Terra/Toolkit/Prefabs/grass_large.prefab");
        main.shrubPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Terra/Toolkit/Prefabs/plant_bush.prefab");
        var sounds=AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/ThirdParty"});if(sounds.Length>0)main.placementSound=AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(sounds[0]));
        var light=Object.FindFirstObjectByType<Light>();light.transform.rotation=Quaternion.Euler(48,-35,0);light.color=new Color(1,.93f,.78f);light.intensity=1.8f;light.shadows=LightShadows.Soft;
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.66f,.72f,.61f);
        EditorSceneManager.SaveScene(scene,"Assets/Terra/Scenes/TerraMain.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Terra/Scenes/TerraMain.unity",true),new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity",false)};
        AssetDatabase.SaveAssets();
    }
    public static void PackageFixture()
    {
        foreach(var location in Terra.Core.LocationProfile.All)
        {
            string source="worlds/"+location.PackId,target="Assets/StreamingAssets/WorldPacks/"+location.PackId;
            Directory.CreateDirectory(target);foreach(var name in new[]{"manifest.json","world.json","scenario.json","hydrology.json"})File.Copy(Path.Combine(source,name),Path.Combine(target,name),true);
        }
        AssetDatabase.Refresh();
    }
}
