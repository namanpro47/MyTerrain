Terra.Rendering.NatureSubmission.ComparisonCellSize=192;
if(!UnityEngine.Application.isPlaying)throw new System.Exception("Play required");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;
var type=typeof(Terra.Rendering.NatureSubmission);var go=new UnityEngine.GameObject("Submission test fixture");var fixture=go.AddComponent<Terra.Rendering.NatureSubmission>();fixture.enabled=false;
var template=UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Material>().First(m=>m.shader!=null&&m.enableInstancing);
var material=new UnityEngine.Material(template);var other=new UnityEngine.Material(template);
var mesh=new UnityEngine.Mesh();var mesh2=new UnityEngine.Mesh();
var checks=new System.Collections.Generic.List<string>();void Check(bool value,string label){if(!value)throw new System.Exception(label);checks.Add(label);}
var add=type.GetMethod("Add",flags);var clear=type.GetMethod("Clear",flags);var pool=(System.Collections.IList)type.GetField("pool",flags).GetValue(fixture);
T Field<T>(object o,string name)=>(T)o.GetType().GetField(name,flags).GetValue(o);
int Used()=>Field<int>(fixture,"used");
void Add(int count,float x=0,int sub=0,UnityEngine.Material mat=null,UnityEngine.Mesh geometry=null,UnityEngine.Rendering.ShadowCastingMode shadow=UnityEngine.Rendering.ShadowCastingMode.On){
 var matrices=Enumerable.Range(0,count).Select(i=>UnityEngine.Matrix4x4.TRS(new UnityEngine.Vector3(x+i*.01f,i*.1f,3),UnityEngine.Quaternion.Euler(0,i,0),new UnityEngine.Vector3(1+i*.001f,2,3))).ToArray();
 var p=new UnityEngine.RenderParams(mat??material){worldBounds=new UnityEngine.Bounds(new UnityEngine.Vector3(x,0,0),UnityEngine.Vector3.one*10),shadowCastingMode=shadow,receiveShadows=true};
 add.Invoke(fixture,new object[]{p,geometry??mesh,sub,matrices});
}
try{
 Add(7);Add(9,20);Check(Used()==1,"Nearby compatible submissions share one draw");var d=pool[0];Check(Field<int>(d,"count")==16,"All submitted instances retained");
 var matrices=Field<UnityEngine.Matrix4x4[]>(d,"matrices");Check(matrices[0].m13==0&&matrices[6].m13==.6f&&matrices[7].m03==20&&matrices[15].m03==20.08f,"Matrix ordering and translation retained across inputs");
 Check(matrices[15].m11==2&&matrices[15].m33==1,"Nonuniform scale and homogeneous coordinate retained");
 var p=Field<UnityEngine.RenderParams>(d,"parameters");Check(p.worldBounds.min.x==-5&&p.worldBounds.max.x==25,"Union bounds contain both source bounds");Check(p.receiveShadows&&p.shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.On,"Render shadow properties retained");
 Add(3,Terra.Rendering.NatureSubmission.ComparisonCellSize+4);Add(3,-1);Check(Used()==3,"Neighbouring positive/negative grid cells remain separate");
 Add(3,0,1);Add(3,0,0,other);Add(3,0,0,null,mesh2);Add(3,0,0,null,null,UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly);Check(Used()==7,"Submesh, material identity, mesh identity and shadow mode do not alias");
 clear.Invoke(fixture,null);Check(Used()==0,"Clear releases active draw count");Check(pool.Cast<object>().All(o=>Field<UnityEngine.Mesh>(o,"mesh")==null&&Field<UnityEngine.RenderParams>(o,"parameters").material==null),"Pool releases source mesh/material references");
 Add(127);Add(127);Add(127);Add(127);Check(Used()==2&&Field<int>(pool[0],"count")==381&&Field<int>(pool[1],"count")==127,"Capacity rollover never exceeds 500");
 Add(127);Check(Used()==2&&Field<int>(pool[1],"count")==254,"Later compatible batches append to rollover draw");
 int capacity=pool.Count;clear.Invoke(fixture,null);Add(2);Check(pool.Count==capacity,"Frame pool reuses allocated matrix buffers");
 type.GetField("frame",flags).SetValue(fixture,UnityEngine.Time.frameCount-1);Add(5);Check(Used()==1&&Field<int>(pool[0],"count")==5&&fixture.LastSourceBatches==1,"New frame discards stale unflushed submissions");
 fixture.enabled=true;fixture.enabled=false;Check(Used()==0,"Disable clears pending submissions");
 return new{count=checks.Count,checks,frame=UnityEngine.Time.frameCount};
}finally{clear.Invoke(fixture,null);UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(other);UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(mesh2);}
