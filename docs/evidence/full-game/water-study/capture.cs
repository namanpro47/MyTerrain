var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
if(main==null)throw new System.Exception("Main scene is not running");
main.enabled=false;
foreach(var line in UnityEngine.Object.FindObjectsByType<UnityEngine.LineRenderer>(UnityEngine.FindObjectsSortMode.None))line.enabled=false;
foreach(var label in UnityEngine.Object.FindObjectsByType<TMPro.TextMeshPro>(UnityEngine.FindObjectsSortMode.None))label.enabled=false;
UnityEditor.EditorApplication.isPaused=true;
var camera=UnityEngine.Camera.main;camera.orthographic=false;camera.fieldOfView=42;
var world=(Terra.Contracts.WorldData)main.GetType().GetField("world",flags).GetValue(main);
var landscape=(Terra.Rendering.LandscapeRenderer)main.GetType().GetField("landscape",flags).GetValue(main);
var feature=System.Linq.Enumerable.First(world.features,f=>f.kind=="water");
var point=feature.points[feature.points.Length/2];
var center=new UnityEngine.Vector3(point.x,landscape.Height(point.x,point.z),point.z);
foreach(var distance in new[]{55f,145f,450f}){
 camera.transform.rotation=UnityEngine.Quaternion.Euler(42,-32,0);camera.transform.position=center-camera.transform.forward*distance;
 var rt=new UnityEngine.RenderTexture(1600,900,24);var old=camera.targetTexture;var active=UnityEngine.RenderTexture.active;camera.targetTexture=rt;
 typeof(Terra.Rendering.InstancedNature).GetField("cameraFrame",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,-1);
 foreach(var plants in UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(UnityEngine.FindObjectsSortMode.None))typeof(Terra.Rendering.InstancedNature).GetMethod("LateUpdate",flags).Invoke(plants,null);
 camera.Render();UnityEngine.RenderTexture.active=rt;var tex=new UnityEngine.Texture2D(1600,900,UnityEngine.TextureFormat.RGB24,false);tex.ReadPixels(new UnityEngine.Rect(0,0,1600,900),0,0);tex.Apply();System.IO.File.WriteAllBytes("/tmp/terra-stream-"+distance+".jpg",tex.EncodeToJPG(93));camera.targetTexture=old;UnityEngine.RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
}
return new {frame=UnityEngine.Time.frameCount,center=new[]{center.x,center.y,center.z},feature.widthM};
