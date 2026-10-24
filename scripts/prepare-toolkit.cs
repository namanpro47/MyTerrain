// Run with: unity command script eval_file scripts/prepare-toolkit.cs
// Builds an isolated compatibility gallery; never replaces the main game scene.

var scenePath = "Assets/Terra/Toolkit/ToolkitPreview.unity";
if (System.IO.File.Exists(scenePath)) throw new InvalidOperationException("Toolkit preview already exists; open it instead of regenerating over edits.");
foreach (var folder in new[] { "Assets/Terra/Toolkit/Materials", "Assets/Terra/Toolkit/Prefabs" }) System.IO.Directory.CreateDirectory(folder);
AssetDatabase.Refresh();
var materials = new Dictionary<string, Material>();
var paths = AssetDatabase.FindAssets("t:Model", new[] {"Assets/ThirdParty/Kenney/NatureKit/Models"}).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
foreach (var path in paths)
{
    var importer = (ModelImporter)AssetImporter.GetAtPath(path);
    foreach (var original in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Material>())
    {
        var key = original.name + "_" + ColorUtility.ToHtmlStringRGBA(original.color);
        if (!materials.TryGetValue(key, out var shared))
        {
            shared = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            shared.name = key;
            shared.SetColor("_BaseColor", original.color);
            shared.SetFloat("_Smoothness", 0.15f);
            shared.enableInstancing = true;
            AssetDatabase.CreateAsset(shared, "Assets/Terra/Toolkit/Materials/" + key + ".mat");
            materials.Add(key, shared);
        }
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), original.name), shared);
    }
    importer.importAnimation = false;
    importer.isReadable = false;
    importer.SaveAndReimport();
}
var soundPaths = AssetDatabase.FindAssets("t:AudioClip", new[] {"Assets/ThirdParty/Kenney/InterfaceSounds/Audio"}).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
foreach (var path in soundPaths)
{
    var importer = (AudioImporter)AssetImporter.GetAtPath(path);
    importer.forceToMono = true;
    var settings = importer.defaultSampleSettings;
    settings.loadType = AudioClipLoadType.DecompressOnLoad;
    settings.compressionFormat = AudioCompressionFormat.Vorbis;
    settings.quality = 0.7f;
    importer.defaultSampleSettings = settings;
    importer.SaveAndReimport();
}
var oldScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var gallery = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Additive);
UnityEngine.SceneManagement.SceneManager.SetActiveScene(gallery);
var cameraObject = new GameObject("Toolkit Camera", typeof(Camera), typeof(AudioListener), typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData));
var camera = cameraObject.GetComponent<Camera>();
camera.tag = "MainCamera";
camera.orthographic = true;
camera.orthographicSize = 11.5f;
camera.backgroundColor = new Color(0.10f, 0.16f, 0.18f);
camera.clearFlags = CameraClearFlags.SolidColor;
camera.transform.position = new Vector3(14, 23, -28);
camera.transform.LookAt(new Vector3(0, 0, 0));
camera.farClipPlane = 100f;
var lightObject = new GameObject("Sun", typeof(Light));
var sun = lightObject.GetComponent<Light>();
sun.type = LightType.Directional;
sun.intensity = 1.5f;
sun.shadows = LightShadows.Soft;
sun.transform.rotation = Quaternion.Euler(45, -35, 0);
RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
RenderSettings.ambientLight = new Color(0.55f, 0.62f, 0.66f);
var floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
floorMat.color = new Color(0.22f, 0.29f, 0.26f);
floorMat.SetFloat("_Smoothness", 0.05f);
AssetDatabase.CreateAsset(floorMat, "Assets/Terra/Toolkit/Materials/DisplayBase.mat");
var specimens = new List<Transform>();
for (int i = 0; i < paths.Length; i++)
{
    var model = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
    var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, gallery);
    instance.name = System.IO.Path.GetFileNameWithoutExtension(paths[i]);
    var renderers = instance.GetComponentsInChildren<Renderer>();
    var bounds = renderers[0].bounds;
    foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
    var nativePrefab = "Assets/Terra/Toolkit/Prefabs/" + instance.name + ".prefab";
    PrefabUtility.SaveAsPrefabAsset(instance, nativePrefab);
    float factor = 2.8f / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
    instance.transform.localScale *= factor;
    var pos = new Vector3((i % 6 - 2.5f) * 4.0f, 0.25f, (i / 6 - 1.5f) * 4.2f);
    instance.transform.position = pos - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z) * factor;
    specimens.Add(instance.transform);
    var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
    tile.name = "Display Plinth " + instance.name;
    tile.transform.position = new Vector3(pos.x, 0, pos.z);
    tile.transform.localScale = new Vector3(3.7f, 0.45f, 3.9f);
    tile.GetComponent<Renderer>().sharedMaterial = floorMat;
    UnityEngine.Object.DestroyImmediate(tile.GetComponent<Collider>());
}
var controller = new GameObject("Toolkit Preview").AddComponent<Terra.Toolkit.TerraToolkitPreview>();
controller.specimens = specimens.ToArray();
controller.sounds = soundPaths.Select(p => AssetDatabase.LoadAssetAtPath<AudioClip>(p)).ToArray();
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gallery, scenePath);
AssetDatabase.SaveAssets();
UnityEngine.SceneManagement.SceneManager.SetActiveScene(oldScene);
UnityEditor.SceneManagement.EditorSceneManager.CloseScene(gallery, true);
var palette = new Dictionary<string, string> {
    {"leafsGreen", "7EA95D"}, {"leafsDark", "416B4A"}, {"grass", "9BB96A"},
    {"woodBark", "927054"}, {"woodBarkDark", "705440"}, {"wood", "C09A6B"},
    {"woodDark", "8A6C4B"}, {"woodInner", "D8BF8C"}, {"dirt", "9D7958"},
    {"dirtDark", "725541"}, {"stone", "929E94"}, {"colorYellow", "EDC865"},
    {"colorPurple", "B28AC1"}, {"_defaultMat", "DDC281"}
};
foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:Material", new[] {"Assets/Terra/Toolkit/Materials"})) {
    var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
    int last = material.name.LastIndexOf('_');
    if (last < 0 || !palette.TryGetValue(material.name.Substring(0, last), out var hex)) continue;
    ColorUtility.TryParseHtmlString("#" + hex, out var color);
    material.SetColor("_BaseColor", color);
    UnityEditor.EditorUtility.SetDirty(material);
}
UnityEditor.AssetDatabase.SaveAssets();

return $"Prepared {paths.Length} prefabs, {materials.Count} shared URP materials, {soundPaths.Length} sounds, and {scenePath}";
