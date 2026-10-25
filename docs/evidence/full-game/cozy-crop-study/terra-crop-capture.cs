var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();main.enabled=false;
var world=(Terra.Contracts.WorldData)main.GetType().GetField("world",flags).GetValue(main);
var rules=(Terra.Core.BuildActionRules)main.GetType().GetField("buildActionRules",flags).GetValue(main);
var layout=rules.Layout(world.sites.First(s=>s.id=="south-01"),rules.Find("cover-crop"));
foreach(var line in UnityEngine.Object.FindObjectsByType<UnityEngine.LineRenderer>(UnityEngine.FindObjectsSortMode.None))line.enabled=false;
foreach(var label in UnityEngine.Object.FindObjectsByType<TMPro.TextMeshPro>(UnityEngine.FindObjectsSortMode.None))label.enabled=false;
UnityEditor.EditorApplication.isPaused=true;
var camera=UnityEngine.Camera.main;camera.orthographic=false;camera.fieldOfView=42;
foreach(var distance in new[]{75f,175f,550f}){
 camera.transform.rotation=UnityEngine.Quaternion.Euler(42,-32,0);camera.transform.position=new UnityEngine.Vector3(layout.x,layout.groundY,layout.z)-camera.transform.forward*distance;
 var rt=new UnityEngine.RenderTexture(1600,900,24);var old=camera.targetTexture;var active=UnityEngine.RenderTexture.active;camera.targetTexture=rt;
 typeof(Terra.Rendering.InstancedNature).GetField("cameraFrame",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,-1);
 foreach(var plants in UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(UnityEngine.FindObjectsSortMode.None))typeof(Terra.Rendering.InstancedNature).GetMethod("LateUpdate",flags).Invoke(plants,null);
 camera.Render();UnityEngine.RenderTexture.active=rt;var tex=new UnityEngine.Texture2D(1600,900,UnityEngine.TextureFormat.RGB24,false);tex.ReadPixels(new UnityEngine.Rect(0,0,1600,900),0,0);tex.Apply();System.IO.File.WriteAllBytes("/tmp/terra-crop-"+distance+".png",tex.EncodeToPNG());camera.targetTexture=old;UnityEngine.RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
}
return new {frame=UnityEngine.Time.frameCount,center=new[]{layout.x,layout.groundY,layout.z}};
