// Check the retained candidate against independent source queries and actual generated geometry.
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
object F(string name)=>main.GetType().GetField(name,flags).GetValue(main);
var land=(Terra.Rendering.LandscapeRenderer)F("landscape");var world=(Terra.Contracts.WorldData)F("world");
var rules=(Terra.Core.BuildActionRules)F("buildActionRules");var annual=(Terra.Gameplay.AnnualTownSession)F("annual");
string sourceBefore=UnityEngine.JsonUtility.ToJson(world),stateBefore=UnityEngine.JsonUtility.ToJson(annual.State);
int checks=0,samples=0;float sourceError=0,layoutShift=0,overlayError=0;var results=new System.Collections.Generic.List<object>();
void Check(bool ok,string label){checks++;if(!ok)throw new System.Exception(label);}
var terrain=world.terrain;var rng=new System.Random(4851);
for(int i=0;i<1000;i++)
{
 float x=terrain.xMin+(float)rng.NextDouble()*(terrain.columns-1)*terrain.cellSizeM,z=terrain.zMin+(float)rng.NextDouble()*(terrain.rows-1)*terrain.cellSizeM;
 sourceError=UnityEngine.Mathf.Max(sourceError,UnityEngine.Mathf.Abs(land.SourceHeight(x,z)-rules.Height(x,z)));
}
Check(sourceError<.0001f,"Renderer source sampler agrees with analytical rules");
foreach(var site in world.sites.Where(s=>!s.protectedSite))
{
 foreach(var rule in rules.actions.Where(a=>!a.landscape&&rules.Rejection(site,a.id)==null))
 {
  var source=rules.Layout(site,rule);string before=UnityEngine.JsonUtility.ToJson(source);
  var timer=System.Diagnostics.Stopwatch.StartNew();var visual=land.VisualLayout(source);timer.Stop();
  Check(!object.ReferenceEquals(source,visual),site.id+"/"+rule.id+": independent presentation layout");
  Check(UnityEngine.JsonUtility.ToJson(source)==before,site.id+"/"+rule.id+": cached analytical layout unchanged");
  Check(visual.x==source.x&&visual.z==source.z&&visual.yaw==source.yaw,site.id+"/"+rule.id+": metric position preserved");
  float left=visual.foundation[0].x,right=visual.foundation[2].x,bottom=visual.foundation[0].z,top=visual.foundation[2].z;
  float highest=float.MinValue,lowest=float.MaxValue;
  // Different sampling grid from the implementation, including all four edges.
  for(int iz=0;iz<=40;iz++)for(int ix=0;ix<=40;ix++)
  {float h=land.Height(UnityEngine.Mathf.Lerp(left,right,ix/40f),UnityEngine.Mathf.Lerp(bottom,top,iz/40f));highest=UnityEngine.Mathf.Max(highest,h);lowest=UnityEngine.Mathf.Min(lowest,h);samples++;}
  Check(highest<=visual.groundY+.001f,site.id+"/"+rule.id+": foundation is above sampled display surface");
  Check(UnityEngine.Mathf.Abs(visual.groundY-highest)<.12f,site.id+"/"+rule.id+": foundation has no unsupported global lift");
  layoutShift=UnityEngine.Mathf.Max(layoutShift,UnityEngine.Mathf.Abs(visual.groundY-source.groundY));
  results.Add(new{kind="layout",site=site.id,action=rule.id,sourceY=source.groundY,displayY=visual.groundY,sourceRelief=source.relief,displayRelief=visual.relief,sampledFill=visual.groundY-lowest,milliseconds=timer.Elapsed.TotalMilliseconds});
 }
 var watch=System.Diagnostics.Stopwatch.StartNew();land.Ghost(site,false,"cultivate-standard");watch.Stop();
 var ghost=(UnityEngine.Transform)land.GetType().GetField("ghost",flags).GetValue(land);int count=0,triangles=0;float error=0;
 foreach(var filter in ghost.GetComponentsInChildren<UnityEngine.MeshFilter>())
 {
  var mesh=filter.sharedMesh;var vertices=mesh.vertices;var indices=mesh.triangles;count+=vertices.Length;triangles+=indices.Length/3;
  for(int j=0;j<indices.Length;j+=3)
  {var p=(vertices[indices[j]]+vertices[indices[j+1]]+vertices[indices[j+2]])/3f;p=filter.transform.TransformPoint(p);error=UnityEngine.Mathf.Max(error,UnityEngine.Mathf.Abs(p.y-land.Height(p.x,p.z)-.22f));}
 }
 overlayError=UnityEngine.Mathf.Max(overlayError,error);Check(error<.002f,site.id+": generated overlay follows display triangulation");
 results.Add(new{kind="overlay",site=site.id,vertices=count,triangles,maxHeightError=error,milliseconds=watch.Elapsed.TotalMilliseconds});land.ClearGhost();
}
Check(UnityEngine.JsonUtility.ToJson(world)==sourceBefore,"Entire runtime source world byte-identical");
Check(UnityEngine.JsonUtility.ToJson(annual.State)==stateBefore,"Annual state byte-identical");
return new{checks,samples,sourceError,layoutShift,overlayError,results,frame=UnityEngine.Time.frameCount,annual=main.Diagnostic};
