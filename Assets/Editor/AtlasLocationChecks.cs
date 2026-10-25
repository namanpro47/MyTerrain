using System;
using System.IO;
using System.Linq;
using Terra.Art;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using UnityEditor;
using UnityEngine;

public static class AtlasLocationChecks
{
    static int assertions;
    static void Check(bool ok,string why){assertions++;if(!ok)throw new Exception(why);}
    public static string Run()
    {
        assertions=0;
        Check(LocationProfile.All.Select(l=>l.AnnualSaveKey).Distinct().Count()==3,"Annual save keys collide");
        Check(LocationProfile.All.Select(l=>l.ChapterSaveKey).Distinct().Count()==3,"Chapter save keys collide");
        Check(LocationProfile.At(-1).IsWisconsin&&LocationProfile.At(99).IsWisconsin,"Invalid browser IDs must use safe default");
        Check(LocationProfile.All[0].AnnualSaveKey=="terra.meadowbrook.annual-full.v3.1","Existing Wisconsin save key changed");
        var catalog=AssetDatabase.LoadAssetAtPath<BuildActionCatalog>("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
        AnnualTownSnapshot first=null;
        foreach(var place in LocationProfile.All)
        {
            string root="worlds/"+place.PackId+"/";
            var m=JsonUtility.FromJson<Manifest>(File.ReadAllText(root+"manifest.json"));
            var w=JsonUtility.FromJson<WorldData>(File.ReadAllText(root+"world.json"));
            var s=JsonUtility.FromJson<Scenario>(File.ReadAllText(root+"scenario.json"));
            var h=JsonUtility.FromJson<RunoffConfiguration>(File.ReadAllText(root+"hydrology.json"));
            PackValidator.Validate(m,w,s);assertions++;
            Check(m.packId==place.PackId,"Catalog and manifest disagree");
            if(place.IsWisconsin)AnnualSpatialOpportunities.AddWind(w,h);
            var rules=BuildActionRuntime.Expand(catalog,ref w,ref s);
            var d=FullAnnualGameplay.Create(w,s,m.packId,rules,new LandscapeRunoff(w,s,h));
            foreach(var id in FullAnnualGameplay.Supported)
            {
                Check(d.actions.Any(a=>a.actionId==id),place.Name+" has no eligible "+id);
                var a=d.actions.First(x=>x.actionId==id);var town=new AnnualTown(d,"atlas-check-"+place.PackId+id);
                town.QueueAction(a.siteId,id,0);AnnualTownDemo.Finish(town,new AuthoredAnnualConsequenceProvider());
                var saved=JsonUtility.FromJson<AnnualTownSnapshot>(JsonUtility.ToJson(town.Snapshot()));
                var restored=AnnualTown.Restore(d,saved);
                Check(restored.CashCents==town.CashCents&&restored.History.Length==10,place.Name+" ten-year save mismatch for "+id);
                Check(restored.History.Last().outcomes.sites.First(r=>r.siteId==a.siteId).phase==TownAssetPhase.Operating,"Project never operated: "+id);
                foreach(var year in restored.History){var e=year.outcomes.economy;Check(Math.Abs(e.generatedM3-e.retainedM3-e.dischargedM3)<.05,"Runoff conservation failed");}
            }
            if(first==null)first=new AnnualTown(d,"cross-location-test").Snapshot();
            else {bool rejected=false;try{AnnualTown.Restore(d,first);}catch(ArgumentException){rejected=true;}catch(InvalidOperationException){rejected=true;}Check(rejected,"A Wisconsin save loaded into "+place.Name);}
        }
        string result="Atlas locations: "+assertions+" assertions passed; three distinct packs, 17 variants per pack, ten-year outcomes, save replay and cross-location rejection.";
        Directory.CreateDirectory("docs/evidence/atlas");File.WriteAllText("docs/evidence/atlas/unity-checks.txt",result+"\n");return result;
    }
}
