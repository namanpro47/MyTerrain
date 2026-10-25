var main=UnityEngine.Object.FindAnyObjectByType<Terra.Gameplay.TerraMain>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
object MainField(string name)=>typeof(Terra.Gameplay.TerraMain).GetField(name,flags).GetValue(main);
var landscape=(Terra.Rendering.LandscapeRenderer)MainField("landscape");
var world=(Terra.Contracts.WorldData)MainField("world");
var session=(Terra.Gameplay.AnnualTownSession)MainField("annual");
var rules=(Terra.Core.BuildActionRules)MainField("buildActionRules");
var proposals=(System.Collections.Generic.Dictionary<string,UnityEngine.Transform>)landscape.GetType().GetField("proposals",flags).GetValue(landscape);
var saved=UnityEngine.JsonUtility.ToJson(session.State);
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
    landscape.Apply(site,offer.actionId);
    var instance=proposals[site.id].Find(entry.title);
    Assert(instance!=null&&instance.localScale==UnityEngine.Vector3.one,offer.actionId+": original metric scale");
    Assert(instance.GetComponentsInChildren<UnityEngine.MeshFilter>(true).Select(f=>f.sharedMesh).SequenceEqual(sourceMeshIds),offer.actionId+": source meshes reused unchanged");
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
    landscape.Apply(site,null);landscape.Ghost(site,true,offer.actionId);landscape.ClearGhost();
    Assert(sourceFilters.Select(f=>f.sharedMesh).SequenceEqual(sourceMeshIds),offer.actionId+": cancel preserves prefab meshes");
    Assert(sourceRenderers.Select(r=>r.sharedMaterial).SequenceEqual(sourceMaterials)&&sourceRenderers.Select(r=>r.sharedMaterial.GetColor("_BaseColor")).SequenceEqual(sourceColors),offer.actionId+": prefab material references and colors unchanged");
}
Assert(seenMaterials.Count==2,"exactly two cached project materials across all building variants");
Assert(UnityEngine.JsonUtility.ToJson(session.State)==saved,"visual lifecycle trials leave live ledger and accepted history unchanged");
var shader=UnityEngine.Resources.Load<UnityEngine.Shader>("TerraProjectGround");
var messages=UnityEditor.ShaderUtil.GetShaderMessages(shader);
Assert(!messages.Any(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),"project ground shader has no errors");
return new {variants=offers.Length,checks=checks.Count,results=checks,shaderMessages=messages,frame=UnityEngine.Time.frameCount};
