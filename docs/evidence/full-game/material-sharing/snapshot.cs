if(!UnityEngine.Application.isPlaying)throw new System.Exception("Play required");
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
if(main==null||string.IsNullOrEmpty(main.Diagnostic))throw new System.Exception("Initialized game required");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
string PathOf(UnityEngine.Transform t){return t.parent==null?t.name:PathOf(t.parent)+"/"+t.name;}
var groups=UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(UnityEngine.FindObjectsSortMode.None).OrderBy(n=>PathOf(n.transform)).ToArray();
var unique=new System.Collections.Generic.HashSet<UnityEngine.Material>();
var records=new System.Collections.Generic.List<object>();
using(var stream=new System.IO.MemoryStream())using(var w=new System.IO.BinaryWriter(stream)){
foreach(var n in groups){var batches=(System.Collections.IList)n.GetType().GetField("batches",flags).GetValue(n);w.Write(PathOf(n.transform));w.Write(batches.Count);
foreach(var batch in batches){var t=batch.GetType();foreach(var f in t.GetFields(flags).OrderBy(f=>f.Name)){var v=f.GetValue(batch);if(f.Name=="animated")continue;w.Write(f.Name);
if(v is UnityEngine.Material material){unique.Add(material);w.Write(material.shader.name);w.Write(material.ComputeCRC());}
else if(v is UnityEngine.Mesh mesh){w.Write(mesh.name);w.Write(mesh.vertexCount);w.Write(mesh.subMeshCount);for(int sub=0;sub<mesh.subMeshCount;sub++)w.Write(mesh.GetIndexCount(sub));}
else if(v is UnityEngine.Matrix4x4[] matrices){w.Write(matrices.Length);foreach(var m in matrices)for(int i=0;i<16;i++)w.Write(m[i]);}
else if(v is UnityEngine.Bounds b){for(int i=0;i<3;i++){w.Write(b.center[i]);w.Write(b.size[i]);}}
else w.Write(v==null?"null":System.Convert.ToString(v,System.Globalization.CultureInfo.InvariantCulture));
}}}
w.Flush();var digest=System.BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(stream.ToArray())).Replace("-", "");
return new{frame=UnityEngine.Time.frameCount,state=main.Diagnostic,groups=groups.Length,uniqueMaterials=unique.Count,contentGroups=unique.Select(m=>new{shader=m.shader.name,crc=m.ComputeCRC()}).Distinct().Count(),batchAppearanceAndPlacementSha256=digest,submitted=groups.Sum(g=>g.LastSubmittedBatches),culled=groups.Sum(g=>g.LastCulledBatches)};
}
