var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindAnyObjectByType<Terra.Gameplay.TerraMain>();
object Field(string name)=>typeof(Terra.Gameplay.TerraMain).GetField(name,flags).GetValue(main);
var landscape=(Terra.Rendering.LandscapeRenderer)Field("landscape");
var world=(Terra.Contracts.WorldData)Field("world");
var session=(Terra.Gameplay.AnnualTownSession)Field("annual");
var rules=(Terra.Core.BuildActionRules)Field("buildActionRules");
var saved=UnityEngine.JsonUtility.ToJson(session.State);var terrain=UnityEngine.JsonUtility.ToJson(world.terrain);
var checks=new System.Collections.Generic.List<string>();
void Assert(bool value,string message){if(!value)throw new System.Exception(message);checks.Add(message);}
var materialsField=typeof(Terra.Rendering.InstancedNature).GetField("ownedMaterials",flags);
UnityEngine.Material[] Materials(UnityEngine.Transform parent)=>parent.GetComponentsInChildren<Terra.Rendering.InstancedNature>(true).SelectMany(n=>(System.Collections.Generic.List<UnityEngine.Material>)materialsField.GetValue(n)).Where(m=>m.HasProperty("_BaseColor")).ToArray();
bool Close(UnityEngine.Color a,UnityEngine.Color b)=>UnityEngine.Vector4.Distance(a,b)<.00001f;
var sourceMaterials=UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Material>().Where(m=>UnityEditor.AssetDatabase.Contains(m)&&m.HasProperty("_BaseColor")).ToDictionary(m=>m,m=>m.GetColor("_BaseColor"));
var offers=session.Definition.actions.GroupBy(a=>a.actionId).Select(g=>g.First()).ToArray();
int treated=0,protectedCover=0;
foreach(var offer in offers)
{
    var site=world.sites.First(s=>s.id==offer.siteId);var rule=rules.Find(offer.actionId);
    var holder=landscape.Apply(site,offer.actionId);
    var materials=Materials(holder);var original=materials.Select(m=>m.GetColor("_BaseColor")).ToArray();
    bool isAgreement=rule.landscape||offer.actionId=="wind";
    if(materials.Length==0)continue;
    if(isAgreement)treated++;else protectedCover++;
    foreach(var stage in new[]{"PROPOSED","SUSPENDED","ESTABLISHING"})
    {
        landscape.AnnualStage(site,stage,.25);
        var changed=materials.Select(m=>m.GetColor("_BaseColor")).ToArray();
        Assert(isAgreement?changed.Where((c,i)=>!Close(c,original[i])).Any():changed.Where((c,i)=>!Close(c,original[i])).Count()==0,offer.actionId+": "+stage+(isAgreement?" colors project batches":" preserves remaining source cover"));
        Assert(changed.Where((c,i)=>c.a!=original[i].a).Count()==0,offer.actionId+": "+stage+" preserves alpha coverage");
        landscape.AnnualStage(site,stage,.25);
        Assert(materials.Select(m=>m.GetColor("_BaseColor")).Where((c,i)=>!Close(c,changed[i])).Count()==0,offer.actionId+": "+stage+" repeats without color drift");
    }
    landscape.AnnualStage(site,"ESTABLISHING",1);
    Assert(materials.Select(m=>m.GetColor("_BaseColor")).Where((c,i)=>!Close(c,original[i])).Count()==0,offer.actionId+": full maturity restores original tint");
    foreach(var n in holder.GetComponentsInChildren<Terra.Rendering.InstancedNature>(true))
    {
        n.SetTint(new UnityEngine.Color(.1f,.2f,.3f,0),1);n.SetTint(UnityEngine.Color.white,0);
    }
    Assert(materials.Select(m=>m.GetColor("_BaseColor")).Where((c,i)=>!Close(c,original[i])).Count()==0,offer.actionId+": zero tint resets original color and alpha");
    Assert(materials.All(m=>!UnityEditor.AssetDatabase.Contains(m)),offer.actionId+": only instance-owned materials edited");
    landscape.Apply(site,null);
}
Assert(sourceMaterials.All(p=>p.Key!=null&&Close(p.Key.GetColor("_BaseColor"),p.Value)),"all loaded source material colors unchanged");
Assert(UnityEngine.JsonUtility.ToJson(session.State)==saved,"live annual ledger and accepted history byte-identical");
Assert(UnityEngine.JsonUtility.ToJson(world.terrain)==terrain,"analytical terrain byte-identical");
Assert(treated>=8,"all seven land actions plus wind agreement exercised");
Assert(protectedCover>=9,"remaining source cover preserved for all nine building/utility variants");
return new {checks=checks.Count,treated,protectedCover,results=checks,frame=UnityEngine.Time.frameCount};
