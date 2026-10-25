var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();var landscape=(Terra.Rendering.LandscapeRenderer)main.GetType().GetField("landscape",flags).GetValue(main);
var holder=new UnityEngine.GameObject("Connected crop row trial");
var material=new UnityEngine.Material(UnityEngine.Shader.Find("Terra/Matte Grain"));material.SetColor("_BaseColor",new UnityEngine.Color(.72f,.58f,.25f));
float Offset(float z)=>12*UnityEngine.Mathf.Sin(z*.012f)+5*UnityEngine.Mathf.Sin(z*.029f);
int meshes=0,vertexTotal=0;
foreach(var nature in UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(UnityEngine.FindObjectsSortMode.None)){
 var batches=(System.Collections.IEnumerable)typeof(Terra.Rendering.InstancedNature).GetField("batches",flags).GetValue(nature);var positions=new System.Collections.Generic.List<UnityEngine.Vector3>();
 foreach(var batch in batches){var bt=batch.GetType();if(!(bool)bt.GetField("crop").GetValue(batch))continue;var matrices=(UnityEngine.Matrix4x4[])bt.GetField("transforms").GetValue(batch);positions.AddRange(matrices.Select(m=>(UnityEngine.Vector3)m.GetColumn(3)));}
 if(positions.Count==0)continue;
 var vertices=new System.Collections.Generic.List<UnityEngine.Vector3>();var indices=new System.Collections.Generic.List<int>();
 foreach(var row in positions.GroupBy(p=>UnityEngine.Mathf.RoundToInt((p.x-Offset(p.z))/1.35f))){
 var pts=row.OrderBy(p=>p.z).ToArray();
 for(int p=0;p<pts.Length-1;p++){
  if(pts[p+1].z-pts[p].z>1.15f)continue;
  int start=vertices.Count;
  for(int end=0;end<2;end++){
   var pt=pts[p+end];float n=UnityEngine.Mathf.PerlinNoise(pt.x*.08f+500,pt.z*.08f+500);float h=.88f+n*.18f;
   for(int cross=0;cross<5;cross++){
    float fraction=cross*.25f;float x=pt.x+(fraction-.5f)*1.20f;
    float crown=cross==0||cross==4?.30f:cross==2?h:h-.05f;
    vertices.Add(new UnityEngine.Vector3(x,landscape.Height(x,pt.z)+crown,pt.z));
   }
  }
  for(int q=0;q<4;q++){int a=start+q,b=a+1,c=a+5,d=a+6;indices.AddRange(new[]{a,c,b,b,c,d});}
 }
 }
 if(vertices.Count==0)continue;var mesh=new UnityEngine.Mesh{name="Standing crop rows"};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();var go=new UnityEngine.GameObject("Field stand");go.transform.SetParent(holder.transform,false);go.AddComponent<UnityEngine.MeshFilter>().sharedMesh=mesh;go.AddComponent<UnityEngine.MeshRenderer>().sharedMaterial=material;meshes++;vertexTotal+=vertices.Count;
}
return new{meshes,vertexTotal};
