var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var world=(Terra.Contracts.WorldData)main.GetType().GetField("world",flags).GetValue(main);
var landscape=(Terra.Rendering.LandscapeRenderer)main.GetType().GetField("landscape",flags).GetValue(main);
var prefab=UnityEngine.Resources.Load<UnityEngine.GameObject>("Nature/Rock_Medium_2");
var bounds=System.Linq.Enumerable.First(prefab.GetComponentsInChildren<UnityEngine.MeshFilter>()).sharedMesh.bounds;
var group=new UnityEngine.GameObject("Illustrative submerged stone study");
var random=new System.Random(4658);int count=0;
foreach(var f in world.features){
 if(f.kind!="water")continue;float at=8;
 for(int i=1;i<f.points.Length;i++){
  var a=new UnityEngine.Vector2(f.points[i-1].x,f.points[i-1].z);var d=new UnityEngine.Vector2(f.points[i].x,f.points[i].z)-a;var n=new UnityEngine.Vector2(-d.y,d.x).normalized;
  for(;at<d.magnitude;at+=19){
   var center=a+d.normalized*at+n*((float)random.NextDouble()-.5f)*f.widthM*.5f;
   for(int j=0;j<5;j++){
    var p=center+new UnityEngine.Vector2((float)random.NextDouble()-.5f,(float)random.NextDouble()-.5f)*2.5f;
    float height=.32f+(float)random.NextDouble()*.5f;
    float scale=height/bounds.size.y;
    var stone=UnityEngine.Object.Instantiate(prefab,group.transform);
    stone.transform.localScale=UnityEngine.Vector3.one*scale;
    stone.transform.rotation=UnityEngine.Quaternion.Euler(0,(float)random.NextDouble()*360,0);
    stone.transform.position=new UnityEngine.Vector3(p.x,landscape.Height(p.x,p.y)-1.94f-bounds.min.y*scale,p.y);count++;
   }
  }
  at-=d.magnitude;
 }
}
return new{count,bounds};
