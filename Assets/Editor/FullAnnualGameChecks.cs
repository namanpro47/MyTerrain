using System;
using System.IO;
using System.Linq;
using Terra.Art;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using UnityEditor;
using UnityEngine;

public static class FullAnnualGameChecks
{
    public static string Run()
    {
        var world=JsonUtility.FromJson<WorldData>(File.ReadAllText("worlds/meadowbrook-v1/world.json"));
        var scenario=JsonUtility.FromJson<Scenario>(File.ReadAllText("worlds/meadowbrook-v1/scenario.json"));
        var hydro=JsonUtility.FromJson<RunoffConfiguration>(File.ReadAllText("worlds/meadowbrook-v1/hydrology.json"));
        AnnualSpatialOpportunities.AddWind(world,hydro);
        var catalog=AssetDatabase.LoadAssetAtPath<BuildActionCatalog>("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
        var rules=BuildActionRuntime.Expand(catalog,ref world,ref scenario);
        var d=FullAnnualGameplay.Create(world,scenario,"meadowbrook-v1",rules,new LandscapeRunoff(world,scenario,hydro));
        int checks=0;
        foreach(var id in FullAnnualGameplay.Supported)
        {
            var a=d.actions.First(x=>x.actionId==id);var town=new AnnualTown(d,"unity-full-"+id);
            town.QueueAction(a.siteId,a.actionId,0);AnnualTownDemo.Finish(town,new AuthoredAnnualConsequenceProvider());
            var restored=AnnualTown.Restore(d,JsonUtility.FromJson<AnnualTownSnapshot>(JsonUtility.ToJson(town.Snapshot())));
            if(restored.CashCents!=town.CashCents||restored.History.Length!=10)throw new Exception("Unity save replay failed: "+id);
            if(restored.History.Last().outcomes.sites.First(s=>s.siteId==a.siteId).phase!=TownAssetPhase.Operating)throw new Exception("No operating outcome: "+id);
            checks+=2;
        }
        return "Full annual game: "+checks+" Unity lifecycle/JSON checks; "+d.actions.Length+" eligible offers, 17 variants / 12 source families.";
    }
}
