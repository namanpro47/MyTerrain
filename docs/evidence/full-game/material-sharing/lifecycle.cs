if(!UnityEngine.Application.isPlaying)throw new System.Exception("Play required");
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
if(main==null||string.IsNullOrEmpty(main.Diagnostic))throw new System.Exception("Initialized game required");
string beforeState=main.Diagnostic;
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var cache=(System.Collections.IDictionary)typeof(Terra.Rendering.InstancedNature).GetField("materialCache",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).GetValue(null);
int cacheBefore=cache.Count;var checks=new System.Collections.Generic.List<string>();
void Check(bool yes,string label){if(!yes)throw new System.Exception(label);checks.Add(label);}
object[] Batches(Terra.Rendering.InstancedNature n)=>((System.Collections.IEnumerable)n.GetType().GetField("batches",flags).GetValue(n)).Cast<object>().ToArray();
UnityEngine.Material Mat(Terra.Rendering.InstancedNature n,int index=0,string slot="material"){var b=Batches(n)[index];return (UnityEngine.Material)b.GetType().GetField(slot,flags).GetValue(b);}
int Leases(Terra.Rendering.InstancedNature n)=>((System.Collections.IDictionary)n.GetType().GetField("borrowedMaterials",flags).GetValue(n)).Count;
bool Near(UnityEngine.Color a,UnityEngine.Color b)=>System.Math.Abs(a.r-b.r)<.000001f&&System.Math.Abs(a.g-b.g)<.000001f&&System.Math.Abs(a.b-b.b)<.000001f&&a.a==b.a;
var root=new UnityEngine.GameObject("Material lease regression fixture");
var source=new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit"));source.SetColor("_BaseColor",new UnityEngine.Color(.3f,.6f,.2f,.7f));
var otherSource=new UnityEngine.Material(source);
var mesh=new UnityEngine.Mesh();mesh.vertices=new[]{new UnityEngine.Vector3(-1,0,0),new UnityEngine.Vector3(1,0,0),new UnityEngine.Vector3(0,2,0)};mesh.triangles=new[]{0,1,2};mesh.RecalculateBounds();mesh.RecalculateNormals();
UnityEngine.GameObject Prefab(string name,UnityEngine.Material material){var go=new UnityEngine.GameObject(name);go.transform.SetParent(root.transform);go.SetActive(false);go.AddComponent<UnityEngine.MeshFilter>().sharedMesh=mesh;go.AddComponent<UnityEngine.MeshRenderer>().sharedMaterial=material;return go;}
var prefab=Prefab("Grass_Fixture",source);var distinct=Prefab("Grass_Distinct",otherSource);
var matrices=new System.Collections.Generic.List<UnityEngine.Matrix4x4>{UnityEngine.Matrix4x4.TRS(new UnityEngine.Vector3(100000,0,100000),UnityEngine.Quaternion.identity,UnityEngine.Vector3.one)};
Terra.Rendering.InstancedNature New(string name,UnityEngine.GameObject p,UnityEngine.Color tint,bool soft=false){var go=new UnityEngine.GameObject(name);go.transform.SetParent(root.transform);var n=go.AddComponent<Terra.Rendering.InstancedNature>();n.enabled=false;n.Add(p,matrices,tint,soft);return n;}
try{
int sourceCrc=source.ComputeCRC();
var a=New("A",prefab,UnityEngine.Color.white);var b=New("B",prefab,UnityEngine.Color.white);
Check(Mat(a)==Mat(b),"Identical construction shares one material");Check(Mat(a)!=source,"Source resource remains separate");Check(Mat(a).enableInstancing,"Shared material retains instancing");
a.Add(prefab,matrices,UnityEngine.Color.white);Check(Leases(a)==1&&Batches(a).Length==2,"Repeated Add uses one component lease");
a.SetTint(UnityEngine.Color.red,0);Check(Mat(a)==Mat(b)&&Leases(a)==1,"Untinted operating site stays shared");
var original=Mat(b).GetColor("_BaseColor");a.SetTint(UnityEngine.Color.red,.6f);
Check(Mat(a)!=Mat(b)&&Mat(a,1)==Mat(a),"Tint detaches only this site and remaps every batch");Check(Leases(a)==0,"Detached color material releases its lease");
Check(Mat(b).GetColor("_BaseColor").Equals(original),"Neighbor remains unchanged");var expected=UnityEngine.Color.Lerp(original,UnityEngine.Color.red,.6f);expected.a=original.a;
Check(Near(Mat(a).GetColor("_BaseColor"),expected),"Tint matches original blend and preserves alpha: actual="+Mat(a).GetColor("_BaseColor").ToString("R")+" expected="+expected.ToString("R"));var first=Mat(a);a.SetTint(UnityEngine.Color.red,.6f);Check(Mat(a)==first&&Near(Mat(a).GetColor("_BaseColor"),expected),"Repeated tint neither allocates nor compounds");
a.SetTint(UnityEngine.Color.blue,.2f);expected=UnityEngine.Color.Lerp(original,UnityEngine.Color.blue,.2f);expected.a=original.a;Check(Near(Mat(a).GetColor("_BaseColor"),expected),"Status changes blend from original color");a.SetTint(UnityEngine.Color.red,0);Check(Near(Mat(a).GetColor("_BaseColor"),original),"Operating status restores original within 1e-6 RGB storage tolerance");
var c=New("C",prefab,UnityEngine.Color.white);Check(Mat(c)==Mat(b),"Later sites receive pristine shared material");
var d=New("D",distinct,UnityEngine.Color.white);Check(Mat(d)!=Mat(b),"Distinct artist material identity does not alias");
var green=new UnityEngine.Color(.4f,.8f,.3f,1);var nearGreen=green;nearGreen.r+=.000001f;var e=New("E",prefab,green);var f=New("F",prefab,nearGreen);Check(Mat(e)!=Mat(f),"Exact color key distinguishes near-equal tint");
var s1=New("S1",prefab,green,true);var s2=New("S2",prefab,green,true);Check(Mat(s1)==Mat(s2),"Equivalent soft cover shares");Check(Mat(s1)!=Mat(e),"Soft and artist materials stay separate");
var oldBounds=mesh.bounds;mesh.bounds=new UnityEngine.Bounds(new UnityEngine.Vector3(0,2,0),new UnityEngine.Vector3(2,4,0));var s3=New("S3",prefab,green,true);mesh.bounds=oldBounds;Check(Mat(s3)!=Mat(s1),"Soft cover plant bounds remain independent");
var template=Mat(s2);s1.SetTint(UnityEngine.Color.gray,.5f);Check(Mat(s1)!=template&&Mat(s2)==template,"Soft-cover lifecycle tint is isolated");
b.gameObject.SetActive(false);Check(Mat(c)!=null&&Mat(c).GetColor("_BaseColor").Equals(original),"Disabling a borrower preserves other users");b.gameObject.SetActive(true);UnityEngine.Object.DestroyImmediate(b.gameObject);Check(Mat(c)!=null&&Mat(c).enableInstancing,"Destroying one borrower preserves remaining lease");
a.Add(prefab,matrices,UnityEngine.Color.white);a.SetTint(UnityEngine.Color.blue,.4f);Check(Leases(a)==0&&Mat(a,2)!=Mat(c),"Add after tint detaches new batches on next status update");
var lodPrefab=new UnityEngine.GameObject("LodFixture");lodPrefab.transform.SetParent(root.transform);lodPrefab.SetActive(false);
var near=Prefab("Near",source);near.transform.SetParent(lodPrefab.transform);var far=Prefab("Far",otherSource);far.transform.SetParent(lodPrefab.transform);
lodPrefab.AddComponent<UnityEngine.LODGroup>().SetLODs(new[]{new UnityEngine.LOD(.4f,new UnityEngine.Renderer[]{near.GetComponent<UnityEngine.MeshRenderer>()}),new UnityEngine.LOD(.1f,new UnityEngine.Renderer[]{far.GetComponent<UnityEngine.MeshRenderer>()})});
var l1=New("L1",lodPrefab,UnityEngine.Color.white);var l2=New("L2",lodPrefab,UnityEngine.Color.white);
Check(Mat(l1)==Mat(l2)&&Mat(l1,0,"mediumMaterial")==Mat(l2,0,"mediumMaterial")&&Mat(l1)!=Mat(l1,0,"mediumMaterial"),"Artist LOD materials share by identity but remain distinct across levels");
l1.SetTint(UnityEngine.Color.yellow,.7f);Check(Mat(l1)!=Mat(l2)&&Mat(l1,0,"mediumMaterial")!=Mat(l2,0,"mediumMaterial"),"Tint detaches both near and medium LOD materials");
Check(Mat(l1,0,"distantMaterial")==Mat(l1),"Fallback distant slot follows copied near material");
Check(source.ComputeCRC()==sourceCrc,"Source material unchanged after all tints");Check(main.Diagnostic==beforeState,"Live annual state unchanged");
}finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(otherSource);UnityEngine.Object.DestroyImmediate(mesh);}
Check(cache.Count==cacheBefore,"All fixture leases released without disturbing live scene cache");
return new{frame=UnityEngine.Time.frameCount,state=main.Diagnostic,count=checks.Count,checks,cacheBefore,cacheAfter=cache.Count};
