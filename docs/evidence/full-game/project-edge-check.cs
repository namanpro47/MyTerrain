var main=UnityEngine.Object.FindAnyObjectByType<Terra.Gameplay.TerraMain>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
object MainField(string name)=>typeof(Terra.Gameplay.TerraMain).GetField(name,flags).GetValue(main);
var landscape=(Terra.Rendering.LandscapeRenderer)MainField("landscape");
var world=(Terra.Contracts.WorldData)MainField("world");
var session=(Terra.Gameplay.AnnualTownSession)MainField("annual");
var rules=(Terra.Core.BuildActionRules)MainField("buildActionRules");
var proposals=(System.Collections.Generic.Dictionary<string,UnityEngine.Transform>)landscape.GetType().GetField("proposals",flags).GetValue(landscape);
var saved=UnityEngine.JsonUtility.ToJson(session.State);
var terrainBefore=UnityEngine.JsonUtility.ToJson(world.terrain);
var timings=new System.Collections.Generic.List<object>();
var checks=new System.Collections.Generic.List<string>();
void Assert(bool condition,string description){if(!condition)throw new System.Exception(description);checks.Add(description);}
var seenMaterials=new System.Collections.Generic.HashSet<UnityEngine.Material>();
var offers=session.Definition.actions.GroupBy(a=>a.actionId).Select(g=>g.First()).Where(a=>!rules.Find(a.actionId).landscape).ToArray();
foreach(var offer in offers)
{
    var site=world.sites.First(s=>s.id==offer.siteId);
    var rule=rules.Find(offer.actionId);var entry=main.buildActionCatalog.Find(rule.artId);
    var sourceFilters=entry.prefab.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
    var sourceRenderers=entry.prefab.GetComponentsInChildren<UnityEngine.MeshRenderer>(true);
    var sourceMeshIds=sourceFilters.Select(f=>f.sharedMesh).ToArray();
    var sourceMaterials=sourceRenderers.Select(r=>r.sharedMaterial).ToArray();
    var sourceColors=sourceRenderers.Select(r=>r.sharedMaterial.GetColor("_BaseColor")).ToArray();
    var timer=System.Diagnostics.Stopwatch.StartNew();landscape.Apply(site,offer.actionId);timer.Stop();timings.Add(new {action=offer.actionId,applyMs=timer.Elapsed.TotalMilliseconds});
    var instance=proposals[site.id].Find(entry.title);
    Assert(instance!=null&&instance.localScale==UnityEngine.Vector3.one,offer.actionId+": original metric scale");
    var instanceFilters=instance.GetComponentsInChildren<UnityEngine.MeshFilter>(true);
    int groundVertices=0;
    var layout=rules.Layout(site,rule);
    for(int part=0;part<instanceFilters.Length;part++)
    {
        var filter=instanceFilters[part];bool graded=offer.actionId!="wind"&&(filter.name=="grass"||filter.name=="path"||filter.name=="asphalt");
        Assert(graded?filter.sharedMesh!=sourceMeshIds[part]:filter.sharedMesh==sourceMeshIds[part],offer.actionId+": "+filter.name+" mesh ownership");
        if(!graded)continue;groundVertices+=filter.sharedMesh.vertexCount;
        var vertices=filter.sharedMesh.vertices;
        Assert(vertices.All(v=>!float.IsNaN(v.x)&&!float.IsNaN(v.y)&&!float.IsNaN(v.z)),offer.actionId+": finite graded vertices");
        Assert(filter.sharedMesh.normals.All(n=>n.sqrMagnitude>.9f&&n.sqrMagnitude<1.1f),offer.actionId+": finite unit normals");
        Assert(vertices.All(v=>{var q=filter.transform.TransformPoint(v);return q.x>=layout.foundation[0].x-.001f&&q.x<=layout.foundation[2].x+.001f&&q.z>=layout.foundation[0].z-.001f&&q.z<=layout.foundation[2].z+.001f;}),offer.actionId+": grading stays inside the original footprint");
        Assert(vertices.All(v=>v.y<=sourceMeshIds[part].bounds.max.y+.001f),offer.actionId+": no raised ground above source top");
        Assert(vertices.All(v=>{var q=filter.transform.TransformPoint(v);bool inner=q.x>=layout.foundation[0].x+3&&q.x<=layout.foundation[2].x-3&&q.z>=layout.foundation[0].z+3&&q.z<=layout.foundation[2].z-3;return !inner||v.y>=sourceMeshIds[part].bounds.min.y-.001f;}),offer.actionId+": central ground stays at source elevation");
        var oldBounds=sourceMeshIds[part].bounds;var newBounds=filter.sharedMesh.bounds;
        Assert(UnityEngine.Mathf.Abs(oldBounds.min.x-newBounds.min.x)<.001f&&UnityEngine.Mathf.Abs(oldBounds.max.x-newBounds.max.x)<.001f&&UnityEngine.Mathf.Abs(oldBounds.min.z-newBounds.min.z)<.001f&&UnityEngine.Mathf.Abs(oldBounds.max.z-newBounds.max.z)<.001f,offer.actionId+": exact horizontal ground bounds");
    }
    Assert(groundVertices<40000,offer.actionId+": bounded ground geometry ("+groundVertices+")");
    Assert(proposals[site.id].Find("Level foundation").gameObject.activeSelf==(offer.actionId=="wind"),offer.actionId+": completed foundation wall visibility");
    var ground=instance.GetComponentsInChildren<UnityEngine.MeshRenderer>(true).Where(r=>r.name=="grass"||r.name=="path").ToArray();
    foreach(var renderer in ground)
    {
        Assert(renderer.sharedMaterial.shader.name=="Terra/Project Ground",offer.actionId+": matched "+renderer.name);
        Assert(renderer.sharedMaterial.GetFloat("_GroundKind")== (renderer.name=="path"?1:0),offer.actionId+": correct surface kind "+renderer.name);
        seenMaterials.Add(renderer.sharedMaterial);
    }
    if(offer.actionId=="wind")Assert(proposals[site.id].Find("Wind pad").GetComponent<UnityEngine.MeshRenderer>().sharedMaterial.shader.name=="Terra/Project Ground","wind: bounded pad uses matched surface");
    foreach(var stage in new[]{"PROPOSED","SUSPENDED"})
    {
        landscape.Apply(site,offer.actionId);landscape.AnnualStage(site,stage,1);
        foreach(var renderer in proposals[site.id].GetComponentsInChildren<UnityEngine.MeshRenderer>().Where(r=>r.sharedMaterial.shader.name=="Terra/Project Ground"))
        {
            var block=new UnityEngine.MaterialPropertyBlock();renderer.GetPropertyBlock(block);
            Assert(block.GetColor("_BaseColor")!=UnityEngine.Color.white,offer.actionId+": "+stage+" has instance tint");
            Assert(renderer.sharedMaterial.GetColor("_BaseColor")==UnityEngine.Color.white,offer.actionId+": "+stage+" preserves shared material");
        }
    }
    landscape.Apply(site,offer.actionId);landscape.AnnualStage(site,"BUILDING",0);
    Assert(proposals[site.id].Find(entry.title).gameObject.activeSelf==false,offer.actionId+": construction still hides finished model");
    Assert(proposals[site.id].Find("Level foundation").gameObject.activeSelf,offer.actionId+": construction exposes original foundation");
    landscape.Apply(site,null);landscape.Ghost(site,true,offer.actionId);landscape.ClearGhost();
    Assert(sourceFilters.Select(f=>f.sharedMesh).SequenceEqual(sourceMeshIds),offer.actionId+": cancel preserves prefab meshes");
    Assert(sourceRenderers.Select(r=>r.sharedMaterial).SequenceEqual(sourceMaterials)&&sourceRenderers.Select(r=>r.sharedMaterial.GetColor("_BaseColor")).SequenceEqual(sourceColors),offer.actionId+": prefab material references and colors unchanged");
}
Assert(UnityEngine.JsonUtility.ToJson(world.terrain)==terrainBefore,"analytical terrain remains byte-identical");
var retained=(System.Collections.Generic.List<UnityEngine.Mesh>)landscape.GetType().GetField("meshes",flags).GetValue(landscape);
Assert(!retained.Any(m=>m.name.StartsWith("Graded project ")),"cancel releases all graded meshes from renderer ownership");
Assert(seenMaterials.Count==2,"exactly two cached project materials across all building variants");
Assert(UnityEngine.JsonUtility.ToJson(session.State)==saved,"visual lifecycle trials leave live ledger and accepted history unchanged");
var shader=UnityEngine.Resources.Load<UnityEngine.Shader>("TerraProjectGround");
var messages=UnityEditor.ShaderUtil.GetShaderMessages(shader);
Assert(!messages.Any(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),"project ground shader has no errors");
return new {variants=offers.Length,checks=checks.Count,results=checks,shaderMessages=messages,timings,frame=UnityEngine.Time.frameCount};
