var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var landscape=(Terra.Rendering.LandscapeRenderer)main.GetType().GetField("landscape",flags).GetValue(main);
var world=(Terra.Contracts.WorldData)main.GetType().GetField("world",flags).GetValue(main);
var site=world.sites.First(s=>s.id=="south-01");

bool Inside(UnityEngine.Vector3 p)=>Terra.Rendering.LandscapeRenderer.Contains(site.polygon,p.x,p.z);
float Offset(float z)=>12*UnityEngine.Mathf.Sin(z*.012f)+5*UnityEngine.Mathf.Sin(z*.029f);
(long,int,int) Key(UnityEngine.Vector3 p){int x=UnityEngine.Mathf.RoundToInt((p.x-Offset(p.z))/1.35f),z=UnityEngine.Mathf.RoundToInt(p.z/1.1f);return (((long)x<<32)|(uint)z,x,z);}
long Id(int x,int z)=>((long)x<<32)|(uint)z;
var positions=new System.Collections.Generic.Dictionary<long,UnityEngine.Vector3>();
var cropBatches=new System.Collections.Generic.List<object>();
foreach(var nature in UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(UnityEngine.FindObjectsSortMode.None)){
 foreach(var batch in (System.Collections.IEnumerable)typeof(Terra.Rendering.InstancedNature).GetField("batches",flags).GetValue(nature)){
  var bt=batch.GetType();if(!(bool)bt.GetField("crop").GetValue(batch))continue;
  var matrices=(UnityEngine.Matrix4x4[])bt.GetField("transforms").GetValue(batch);
  foreach(var matrix in matrices){var p=(UnityEngine.Vector3)matrix.GetColumn(3);if(Inside(p))positions[Key(p).Item1]=p;}
  cropBatches.Add(batch);
 }
}
var vertices=new System.Collections.Generic.List<UnityEngine.Vector3>();var indices=new System.Collections.Generic.List<int>();var ids=new System.Collections.Generic.Dictionary<long,int>();
foreach(var pair in positions){
 var p=pair.Value;var key=Key(p);
 bool edge=!positions.ContainsKey(Id(key.Item2-1,key.Item3))||!positions.ContainsKey(Id(key.Item2+1,key.Item3))||!positions.ContainsKey(Id(key.Item2,key.Item3-1))||!positions.ContainsKey(Id(key.Item2,key.Item3+1));
 float large=UnityEngine.Mathf.PerlinNoise(p.x*.055f+155,p.z*.055f+324);
 float small=UnityEngine.Mathf.PerlinNoise(p.x*.43f+23,p.z*.43f+86);
 float height=edge?.25f:.75f+large*.28f+small*.09f;
 ids[pair.Key]=vertices.Count;vertices.Add(new UnityEngine.Vector3(p.x,landscape.Height(p.x,p.z)+height,p.z));
}
foreach(var pair in positions){var k=Key(pair.Value);int a=ids[k.Item1],b,c,d;if(ids.TryGetValue(Id(k.Item2+1,k.Item3),out b)&&ids.TryGetValue(Id(k.Item2,k.Item3+1),out c)&&ids.TryGetValue(Id(k.Item2+1,k.Item3+1),out d))indices.AddRange(new[]{a,c,b,b,c,d});}
int original=0,retained=0;
foreach(var batch in cropBatches){
 var bt=batch.GetType();var matrices=(UnityEngine.Matrix4x4[])bt.GetField("transforms").GetValue(batch);var keep=new System.Collections.Generic.List<UnityEngine.Matrix4x4>();
 foreach(var m in matrices){var p=(UnityEngine.Vector3)m.GetColumn(3);if(!Inside(p)){keep.Add(m);continue;}original++;var k=Key(p);
 bool edge=!positions.ContainsKey(Id(k.Item2-1,k.Item3))||!positions.ContainsKey(Id(k.Item2+1,k.Item3))||!positions.ContainsKey(Id(k.Item2,k.Item3-1))||!positions.ContainsKey(Id(k.Item2,k.Item3+1));
 uint h=unchecked((uint)(k.Item2*73856093^k.Item3*19349663));
 if(edge||h%13==0){keep.Add(m);retained++;}
 }
 bt.GetField("transforms").SetValue(batch,keep.ToArray());bt.GetField("animated").SetValue(batch,null);
}
var mesh=new UnityEngine.Mesh{name="Continuous crop canopy diagnostic"};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
var holder=new UnityEngine.GameObject("Continuous crop canopy diagnostic");holder.AddComponent<UnityEngine.MeshFilter>().sharedMesh=mesh;var mat=new UnityEngine.Material(UnityEngine.Shader.Find("Terra/Matte Grain"));mat.SetColor("_BaseColor",new UnityEngine.Color(.68f,.57f,.30f));holder.AddComponent<UnityEngine.MeshRenderer>().sharedMaterial=mat;
return new{frame=UnityEngine.Time.frameCount,vertices=vertices.Count,triangles=indices.Count/3,originalStalks=original,retainedStalks=retained};
