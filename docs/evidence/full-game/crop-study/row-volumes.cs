// Temporary rendering study: reconstruct continuous row masses from actual existing crop positions.
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
string before=main.Diagnostic;
var prior=UnityEngine.GameObject.Find("Crop row volume study");if(prior!=null)throw new System.Exception("Restart Play mode before repeating this study");
var root=new UnityEngine.GameObject("Crop row volume study");
var material=new UnityEngine.Material(UnityEngine.Resources.Load<UnityEngine.Shader>("TerraCrop")){name="Study crop row volume"};material.SetColor("_BaseColor",new UnityEngine.Color(.72f,.62f,.34f));
int plants=0,segments=0,groups=0,vertices=0;
foreach(var nature in UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(UnityEngine.FindObjectsSortMode.None))
{
 var batches=(System.Collections.IList)nature.GetType().GetField("batches",flags).GetValue(nature);
 var points=new System.Collections.Generic.List<UnityEngine.Vector3>();
 bool onlyCrop=batches.Count>0;
 foreach(var batch in batches)
 {
  var bt=batch.GetType();if(!(bool)bt.GetField("crop",flags).GetValue(batch)){onlyCrop=false;continue;}
  foreach(var m in (UnityEngine.Matrix4x4[])bt.GetField("transforms",flags).GetValue(batch))points.Add(m.GetColumn(3));
 }
 if(!onlyCrop||points.Count==0)continue;
 nature.enabled=false;plants+=points.Count;
 var v=new System.Collections.Generic.List<UnityEngine.Vector3>();var indices=new System.Collections.Generic.List<int>();
 var rows=points.GroupBy(p=>UnityEngine.Mathf.RoundToInt((p.x-12*UnityEngine.Mathf.Sin(p.z*.012f)-5*UnityEngine.Mathf.Sin(p.z*.029f))/1.35f));
 foreach(var row in rows)
 {
  var ordered=row.OrderBy(p=>p.z).ToArray();int prev=-1;float lastZ=-99999;
  foreach(var p in ordered)
  {
   int start=v.Count;float variation=UnityEngine.Mathf.PerlinNoise(p.x*.025f+62,p.z*.025f+281);float height=.62f+variation*.15f;
   float slope=.144f*UnityEngine.Mathf.Cos(p.z*.012f)+.145f*UnityEngine.Mathf.Cos(p.z*.029f);var side=new UnityEngine.Vector3(1,0,-slope).normalized;
   float[] widths={-.48f,-.35f,0,.35f,.48f};float[] levels={.12f,.76f,1,.76f,.12f};
   for(int k=0;k<5;k++)v.Add(p+side*widths[k]+UnityEngine.Vector3.up*(levels[k]*height));
   if(prev>=0&&p.z-lastZ<1.3f)
   {for(int k=0;k<4;k++){indices.AddRange(new[]{prev+k,start+k,prev+k+1,prev+k+1,start+k,start+k+1});}segments++;}
   prev=start;lastZ=p.z;
  }
 }
 var mesh=new UnityEngine.Mesh{name="Continuous planted row study",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
 var go=new UnityEngine.GameObject("Row group "+groups++);go.transform.SetParent(root.transform,false);go.AddComponent<UnityEngine.MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<UnityEngine.MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;vertices+=v.Count;
}
return new{plants,segments,groups,vertices,annualUnchanged=main.Diagnostic==before,frame=UnityEngine.Time.frameCount};
