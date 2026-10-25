if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Inventory requires initialized Play mode");
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
if(main==null||string.IsNullOrEmpty(main.Diagnostic))throw new System.Exception("Game not initialized");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var groups=UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(UnityEngine.FindObjectsSortMode.None);
var field=typeof(Terra.Rendering.InstancedNature).GetField("ownedMaterials",flags);
var materials=groups.SelectMany(g=>(System.Collections.Generic.List<UnityEngine.Material>)field.GetValue(g)).ToArray();
return new {frame=UnityEngine.Time.frameCount,state=main.Diagnostic,groups=groups.Length,materials=materials.Length,contentGroups=materials.Select(m=>new {shader=m.shader.name,crc=m.ComputeCRC()}).Distinct().Count(),shaders=materials.GroupBy(m=>m.shader.name).Select(g=>new {shader=g.Key,count=g.Count(),contentGroups=g.Select(m=>m.ComputeCRC()).Distinct().Count()}).ToArray(),submitted=groups.Sum(g=>g.LastSubmittedBatches),culled=groups.Sum(g=>g.LastCulledBatches)};
