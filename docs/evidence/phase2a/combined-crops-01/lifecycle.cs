if(!UnityEngine.Application.isPlaying)throw new System.Exception("Requires Play");
var f=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();string before=main.Diagnostic;var land=(Terra.Rendering.LandscapeRenderer)main.GetType().GetField("landscape",f).GetValue(main);var lt=land.GetType();
var world=(Terra.Contracts.WorldData)lt.GetField("world",f).GetValue(land);var site=world.sites.First(s=>s.id=="south-01");var bases=(System.Collections.Generic.Dictionary<string,UnityEngine.Transform>)lt.GetField("baselines",f).GetValue(land);var root=(UnityEngine.Transform)lt.GetField("root",f).GetValue(land);var owned=(System.Collections.Generic.List<UnityEngine.Mesh>)lt.GetField("meshes",f).GetValue(land);
var bf=typeof(Terra.Rendering.InstancedNature).GetField("batches",f);int checks=0,plants=0,corners=0,soilVertices=0,unchangedOtherBatches=0;var rows=new System.Collections.Generic.List<object>();
void Check(bool yes,string why){checks++;if(!yes)throw new System.Exception(why);}
void Inspect(UnityEngine.Transform parent,Terra.Contracts.Point[] polygon,bool source,bool context,string action,bool inspectSoil=true)
{
 float bottom=polygon==null?0:polygon.Min(p=>p.z),top=polygon==null?0:polygon.Max(p=>p.z);
 bool Allowed(float x,float z)
 {
  if(polygon!=null&&!Terra.Rendering.LandscapeRenderer.Contains(polygon,x,z))return false;
  if(source&&!(bool)lt.GetMethod("VisualCropAt",f).Invoke(land,new object[]{x,z}))return false;
  if(context&&world.sites.Any(s=>Terra.Rendering.LandscapeRenderer.Contains(s.polygon,x,z)))return false;
  if(action=="cultivate-margin"&&z<Mathf.Lerp(bottom,top,.25f)-.001f)return false;
  if(action=="conservation-rotation"){float h=(top-bottom)/3;int band=Mathf.Min(2,Mathf.FloorToInt((z-bottom)/h));if(z<bottom+(band+.3f)*h-.001f)return false;}
  return true;
 }
 foreach(var mf in inspectSoil?parent.GetComponentsInChildren<UnityEngine.MeshFilter>(true).Where(m=>m.name=="Cultivated soil beds"):Enumerable.Empty<UnityEngine.MeshFilter>())
 {
  foreach(var p in mf.sharedMesh.vertices){Check(float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z),"Soil finite");Check(Allowed(p.x,p.z),"Soil outside allowed cover/band");soilVertices++;}
 }
 foreach(var n in parent.GetComponents<Terra.Rendering.InstancedNature>())foreach(var batch in (System.Collections.IEnumerable)bf.GetValue(n))
 {
  var bt=batch.GetType();var mesh=(UnityEngine.Mesh)bt.GetField("mesh",f).GetValue(batch);if(!mesh.name.StartsWith("LeafCrop"))continue;var matrices=(UnityEngine.Matrix4x4[])bt.GetField("transforms",f).GetValue(batch);plants+=matrices.Length;
  Check((float)bt.GetField("highDetailPixels",f).GetValue(batch)==20&&(float)bt.GetField("mediumDetailPixels",f).GetValue(batch)==8,"Crop LOD profile");Check((float)bt.GetField("detailExtent",f).GetValue(batch)>2,"Broad crop detail extent");
  var b=mesh.bounds;
  foreach(var m in matrices)for(int i=0;i<8;i++){var p=m.MultiplyPoint3x4(b.center+UnityEngine.Vector3.Scale(b.extents,new UnityEngine.Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));Check(float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z),"Plant finite");Check(Allowed(p.x,p.z),"Plant outside allowed cover/band");corners++;}
 }
}
// Context owns only its direct soil meshes, not baseline descendants.
var contextRoot=new UnityEngine.GameObject("Temporary crop audit").transform;contextRoot.SetParent(root,false);
var contextMeshes=root.Cast<UnityEngine.Transform>().Where(t=>t.name=="Cultivated soil beds").ToArray();
foreach(var child in contextMeshes)child.SetParent(contextRoot,false);
try{Inspect(contextRoot,null,true,true,null);}finally{foreach(var child in contextMeshes)child.SetParent(root,false);UnityEngine.Object.DestroyImmediate(contextRoot.gameObject);}
// Root context plant batches are inspected without traversing child meshes.
Inspect(root,null,true,true,null,false);
foreach(var pair in bases){var s=world.sites.First(w=>w.id==pair.Key);Inspect(pair.Value,s.polygon,true,false,null);}
foreach(var n in root.GetComponentsInChildren<Terra.Rendering.InstancedNature>())foreach(var batch in (System.Collections.IEnumerable)bf.GetValue(n))
{
 var bt=batch.GetType();var mesh=(UnityEngine.Mesh)bt.GetField("mesh",f).GetValue(batch);if(mesh.name.StartsWith("LeafCrop"))continue;
 Check((float)bt.GetField("detailExtent",f).GetValue(batch)==(float)bt.GetField("maxHeight",f).GetValue(batch),"Other vegetation LOD extent changed");Check((float)bt.GetField("cullPixels",f).GetValue(batch)==2,"Other vegetation cutoff changed");unchangedOtherBatches++;
}
land.ClearGhost();land.Apply(site,null);land.ViewScale(.3f);int meshCount=owned.Count;
var pixels=(UnityEngine.Color32[])lt.GetField("surfacePixels",f).GetValue(land);var baselinePixels=pixels.ToArray();
foreach(var action in new[]{"cultivate-standard","cultivate-margin","conservation-rotation"})
{
 land.Ghost(site,true,action);land.ViewScale(.3f);Check(!bases[site.id].gameObject.activeSelf,"Ghost hides baseline");Inspect((UnityEngine.Transform)lt.GetField("ghost",f).GetValue(land),site.polygon,false,false,action);land.ClearGhost();land.ViewScale(.3f);Check(owned.Count==meshCount,"Preview mesh leak");Check(baselinePixels.SequenceEqual(pixels),"Preview surface not restored");
 var holder=land.Apply(site,action);land.ViewScale(.3f);Inspect(holder,site.polygon,false,false,action);land.Apply(site,null);land.ViewScale(.3f);Check(owned.Count==meshCount,"Apply mesh leak");Check(baselinePixels.SequenceEqual(pixels),"Applied surface not restored");Check(bases[site.id].gameObject.activeSelf,"Undo reveals baseline");rows.Add(action);
}
Check(main.Diagnostic==before,"Annual state changed");var errors=UnityEditor.ShaderUtil.GetShaderMessages(UnityEngine.Resources.Load<UnityEngine.Shader>("TerraGround")).Where(m=>m.severity.ToString()=="Error").Select(m=>m.message).ToArray();Check(errors.Length==0,"Ground shader error");
return new{checks,plants,corners,soilVertices,unchangedOtherBatches,actions=rows,annualState=before,shaderErrors=errors,scope="Editor geometry and renderer lifecycle. No financial transitions or Chrome performance claim. Includes context plant bounds and management-site exclusion."};
