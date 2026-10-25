if(!UnityEngine.Application.isPlaying)throw new System.Exception("Play required");
var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;
var type=typeof(Terra.Rendering.NatureSubmission);var go=new UnityEngine.GameObject("Submission cell audit");var fixture=go.AddComponent<Terra.Rendering.NatureSubmission>();fixture.enabled=false;
var add=type.GetMethod("Add",f);var clear=type.GetMethod("Clear",f);var pool=(System.Collections.IList)type.GetField("pool",f).GetValue(fixture);
T Get<T>(object o,string n)=>(T)o.GetType().GetField(n,f).GetValue(o);
var inputs=new System.Collections.Generic.Dictionary<(UnityEngine.Mesh,UnityEngine.Material,int,int,UnityEngine.Matrix4x4),int>();
var bounds=new System.Collections.Generic.List<(UnityEngine.RenderParams p,UnityEngine.Mesh mesh,int sub,UnityEngine.Matrix4x4[] transforms)>();
foreach(var nature in UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>())
foreach(var b in (System.Collections.IEnumerable)nature.GetType().GetField("batches",f).GetValue(nature)){
 var matrices=Get<UnityEngine.Matrix4x4[]>(b,"transforms");var material=Get<UnityEngine.Material>(b,"material");
 if(matrices.Length>=128||material.renderQueue>2500)continue;
 var mesh=Get<UnityEngine.Mesh>(b,"mesh");int sub=Get<int>(b,"submesh");var shadows=nature.ShadowsOnly?UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly:UnityEngine.Rendering.ShadowCastingMode.On;
 var p=new UnityEngine.RenderParams(material){worldBounds=Get<UnityEngine.Bounds>(b,"bounds"),shadowCastingMode=shadows,receiveShadows=true};bounds.Add((p,mesh,sub,matrices));
 foreach(var m in matrices){var key=(mesh,material,sub,(int)shadows,m);inputs.TryGetValue(key,out int count);inputs[key]=count+1;}
}
var rows=new System.Collections.Generic.List<object>();float originalSize=Terra.Rendering.NatureSubmission.ComparisonCellSize;
try{
 foreach(float size in new[]{96f,192f}){
  Terra.Rendering.NatureSubmission.ComparisonCellSize=size;clear.Invoke(fixture,null);type.GetField("frame",f).SetValue(fixture,-1);
  foreach(var b in bounds)add.Invoke(fixture,new object[]{b.p,b.mesh,b.sub,b.transforms});
  var remaining=new System.Collections.Generic.Dictionary<(UnityEngine.Mesh,UnityEngine.Material,int,int,UnityEngine.Matrix4x4),int>(inputs);int used=Get<int>(fixture,"used"),instances=0,missingBounds=0;
  for(int i=0;i<used;i++){
   var d=pool[i];var p=Get<UnityEngine.RenderParams>(d,"parameters");var mesh=Get<UnityEngine.Mesh>(d,"mesh");int sub=Get<int>(d,"submesh"),count=Get<int>(d,"count");if(count>500)throw new System.Exception("Capacity exceeded");
   var matrices=Get<UnityEngine.Matrix4x4[]>(d,"source")??Get<UnityEngine.Matrix4x4[]>(d,"matrices");
   for(int j=0;j<count;j++){
    var key=(mesh,p.material,sub,(int)p.shadowCastingMode,matrices[j]);
    if(!remaining.TryGetValue(key,out int n)||n<=0)throw new System.Exception("Lost identity or changed matrix");remaining[key]=n-1;instances++;
   }
  }
  if(remaining.Values.Any(n=>n!=0))throw new System.Exception("Missing instance");
  foreach(var b in bounds){bool contained=false;for(int i=0;i<used;i++){
   var d=pool[i];var p=Get<UnityEngine.RenderParams>(d,"parameters");
   if(Get<UnityEngine.Mesh>(d,"mesh")!=b.mesh||p.material!=b.p.material||Get<int>(d,"submesh")!=b.sub||p.shadowCastingMode!=b.p.shadowCastingMode)continue;
   var outer=p.worldBounds;outer.Expand(.001f);if(outer.Contains(b.p.worldBounds.min)&&outer.Contains(b.p.worldBounds.max)){contained=true;break;}
  }if(!contained)missingBounds++;}
  if(missingBounds!=0)throw new System.Exception("Lost source bounds");
  rows.Add(new{size,sourceBatches=bounds.Count,draws=used,instances,uniqueIdentityMatrices=inputs.Count,missingBounds});
 }

}finally{Terra.Rendering.NatureSubmission.ComparisonCellSize=originalSize;clear.Invoke(fixture,null);UnityEngine.Object.DestroyImmediate(go);}
 return new{rows,scope="All active scene small-batch full-detail collector replay; not visibility/LOD selection or GPU timing. Exact identity/matrix multiplicity, capacity and containing bounds are checked."};
