var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var land=(Terra.Rendering.LandscapeRenderer)main.GetType().GetField("landscape",flags).GetValue(main);var world=(Terra.Contracts.WorldData)main.GetType().GetField("world",flags).GetValue(main);
var shapeMethod=land.GetType().GetMethod("SwaleFor",flags);var cutMethod=land.GetType().GetMethod("SwaleCut",flags);var distanceMethod=land.GetType().GetMethod("SwaleDistanceSquared",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
float OriginalDistance(Terra.Contracts.Point[] route,float x,float z){var p=new UnityEngine.Vector2(x,z);float distance=float.MaxValue;for(int i=1;i<route.Length;i++){var a=new UnityEngine.Vector2(route[i-1].x,route[i-1].z);var d=new UnityEngine.Vector2(route[i].x,route[i].z)-a;float t=d.sqrMagnitude>0?UnityEngine.Mathf.Clamp01(UnityEngine.Vector2.Dot(p-a,d)/d.sqrMagnitude):0;distance=UnityEngine.Mathf.Min(distance,UnityEngine.Vector2.Distance(p,a+d*t));}return distance;}
var rng=new System.Random(5381);float maxCutError=0,maxDistanceError=0,maxGradientError=0;int samples=0;var results=new System.Collections.Generic.List<object>();
foreach(var site in world.sites)
{
 var shape=shapeMethod.Invoke(land,new object[]{site});var shapeType=shape.GetType();var route=(Terra.Contracts.Point[])shapeType.GetField("route").GetValue(shape);float half=(float)shapeType.GetField("halfWidth").GetValue(shape);var bounds=(UnityEngine.Bounds)shapeType.GetField("bounds").GetValue(shape);
 float Original(float x,float z){if(x<bounds.min.x||x>bounds.max.x||z<bounds.min.z||z>bounds.max.z||!Terra.Rendering.LandscapeRenderer.Contains(site.polygon,x,z))return 0;float d=OriginalDistance(route,x,z)/half;return 1.15f*UnityEngine.Mathf.Pow(UnityEngine.Mathf.Max(0,1-d*d),2);}
 float Current(float x,float z)=>(float)cutMethod.Invoke(land,new object[]{shape,x,z});
 int positive=0;
 for(int i=0;i<6000;i++)
 {
  float x=UnityEngine.Mathf.Lerp(bounds.min.x-10,bounds.max.x+10,(float)rng.NextDouble()),z=UnityEngine.Mathf.Lerp(bounds.min.z-10,bounds.max.z+10,(float)rng.NextDouble());
  float oldCut=Original(x,z),newCut=Current(x,z);if(oldCut>0)positive++;
  maxCutError=UnityEngine.Mathf.Max(maxCutError,UnityEngine.Mathf.Abs(oldCut-newCut));
  float distance=UnityEngine.Mathf.Sqrt((float)distanceMethod.Invoke(null,new object[]{shape,x,z,float.MaxValue}));maxDistanceError=UnityEngine.Mathf.Max(maxDistanceError,UnityEngine.Mathf.Abs(distance-OriginalDistance(route,x,z)));
  float ox=(Original(x+.75f,z)-Original(x-.75f,z))/1.5f,nx=(Current(x+.75f,z)-Current(x-.75f,z))/1.5f;
  float oz=(Original(x,z+.75f)-Original(x,z-.75f))/1.5f,nz=(Current(x,z+.75f)-Current(x,z-.75f))/1.5f;
  maxGradientError=UnityEngine.Mathf.Max(maxGradientError,UnityEngine.Mathf.Abs(ox-nx),UnityEngine.Mathf.Abs(oz-nz));samples++;
 }
 if(positive==0)throw new System.Exception("No basin samples: "+site.id);results.Add(new{site=site.id,positive});
}
if(maxCutError>.00001f||maxGradientError>.00001f||maxDistanceError>.00001f)throw new System.Exception("Profile differs: "+maxCutError+"/"+maxGradientError+"/"+maxDistanceError);
return new{samples,maxCutError,maxGradientError,maxDistanceError,results,frame=UnityEngine.Time.frameCount};
