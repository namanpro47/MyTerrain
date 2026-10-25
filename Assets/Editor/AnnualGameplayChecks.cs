using System;
using System.IO;
using System.Linq;
using Terra.Art;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using UnityEditor;
using UnityEngine;

public static class AnnualGameplayChecks
{
    static int checks;
    static void Check(bool valid,string reason){checks++;if(!valid)throw new Exception(reason);}
    [MenuItem("Terra/Check annual playable integration")]
    public static void Run()
    {
        checks=0;
        var world=JsonUtility.FromJson<WorldData>(File.ReadAllText("worlds/meadowbrook-v1/world.json"));
        var scenario=JsonUtility.FromJson<Scenario>(File.ReadAllText("worlds/meadowbrook-v1/scenario.json"));
        var catalog=AssetDatabase.LoadAssetAtPath<BuildActionCatalog>("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
        var rules=BuildActionRuntime.Expand(catalog,ref world,ref scenario);
        var definition=AnnualGameplay.Create(world,scenario,"meadowbrook-v1",rules.Rejection);
        foreach(var id in AnnualGameplay.Supported)Check(definition.actions.Any(a=>a.actionId==id),"Playable recipe has at least one fitting field: "+id);
        foreach(var action in definition.actions)Check(rules.Rejection(world.sites.First(s=>s.id==action.siteId),action.actionId)==null,"Every offered model actually fits");
        Check(definition.actions.First(a=>a.actionId=="low-density-housing").upfrontPublicCostCents==100000000,"Housing source price");
        Check(definition.actions.First(a=>a.actionId=="public-park").upfrontPublicCostCents==5000000,"Park source price");
        Check(definition.actions.First(a=>a.actionId=="cultivate-standard").upfrontPublicCostCents==10000000,"Traditional source price");
        Check(definition.actions.First(a=>a.actionId=="conservation-rotation").upfrontPublicCostCents==16000000,"Regenerative source price");
        var town=new AnnualTown(definition,"three-year-integration");
        // Homes have few eligible parcels; reserve one before choosing rural practices.
        var home=definition.actions.First(a=>a.actionId=="low-density-housing");town.QueueAction(home.siteId,home.actionId,town.Revision);
        foreach(var id in new[]{"public-park","conservation-rotation","water-swale"})
        {var a=definition.actions.First(x=>x.actionId==id&&!town.Queue.Any(q=>q.siteId==x.siteId));town.QueueAction(a.siteId,a.actionId,town.Revision);}
        Check(town.CashCents==1000000000,"Queue never spends cash");
        var report1=AnnualTownDemo.Advance(town,"y1",new AuthoredAnnualConsequenceProvider());
        Check(report1.ledger.publicReceiptsCents==0,"Construction year has no home receipts");
        Check(report1.outcomes.sites.First(s=>s.siteId==home.siteId).phase==TownAssetPhase.Building,"Year one home is building");
        town.ContinuePlanning(town.Revision);
        var report2=AnnualTownDemo.Advance(town,"y2",new AuthoredAnnualConsequenceProvider());
        Check(report2.ledger.publicReceiptsCents==2000000,"Occupied home receives tentative annual tax once");
        Check(report2.outcomes.sites.First(s=>s.siteId==home.siteId).serviceUnits==4,"Four occupied places after completion");
        town.ContinuePlanning(town.Revision);
        var report3=AnnualTownDemo.Advance(town,"y3",new AuthoredAnnualConsequenceProvider());
        Check(report3.outcomes.sites.First(s=>s.siteId==home.siteId).serviceUnits==4,"Wet year preserves home services");
        Check(report3.outcomes.sites.First(s=>s.actionId=="conservation-rotation").maturity==1,"Rotation establishes across three years");
        var hydrology=JsonUtility.FromJson<RunoffConfiguration>(File.ReadAllText("worlds/meadowbrook-v1/hydrology.json"));
        var runoff=new LandscapeRunoff(world,scenario,hydrology);
        var water=runoff.EvaluateAnnual(report3.outcomes,town.Revision);
        Check(Math.Abs(water.generatedM3-water.retainedM3-water.dischargedM3)<.01,"Annual water-volume conservation");
        Check(Math.Abs(runoff.EvaluateAnnual(AnnualGameplay.Baseline(definition),0).dischargedM3-9701.926)<.01,"Annual baseline matches fixed-storm handoff");
        Check(report1.outcomes.retentionCapacityM3==0&&report2.outcomes.retentionCapacityM3>0,"Swale storage waits for commissioning");
        var repeated=AnnualTownDemo.Advance(town,"y3",new AuthoredAnnualConsequenceProvider());
        Check(town.History.Length==3&&repeated.ledger.closingCashCents==report3.ledger.closingCashCents,"Retry doesn't charge or append");
        string json=JsonUtility.ToJson(town.Snapshot());var restored=AnnualTown.Restore(definition,JsonUtility.FromJson<AnnualTownSnapshot>(json));
        Check(restored.History.Length==3&&restored.CashCents==town.CashCents,"Unity JSON restores accepted three-year history");
        Check(!restored.CanUndo&&!restored.CanRedo,"No undo across accepted boundary");
        // Continue through the same transitions, including Wait and the final-year guard.
        AnnualTownDemo.Finish(restored,new AuthoredAnnualConsequenceProvider());Check(restored.Phase==TownPhase.Completed&&restored.History.Length==10,"Ten-year boundary uses same loop");
        Directory.CreateDirectory("docs/evidence/annual-gameplay");
        File.WriteAllText("docs/evidence/annual-gameplay/three-year-run.json",json);
        File.WriteAllText("docs/evidence/annual-gameplay/recipes.json",JsonUtility.ToJson(definition,true));
        Debug.Log("TERRA_ANNUAL_GAMEPLAY_CHECKS_PASSED: "+checks);
    }
}
