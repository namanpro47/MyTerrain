using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Terra.Contracts;
using Terra.Core;

public static class Phase3Checks
{
    static readonly List<string> passed=new List<string>();
    static void Check(bool value,string message){if(!value)throw new Exception(message);passed.Add(message);}
    static string Value(WorldResult r){r.revision=0;return JsonUtility.ToJson(r);}
    static EditCommand Command(WorldStateStore s,string op,string site=null,string treatment=null,string destination=null)=>new EditCommand{operation=op,siteId=site,treatmentId=treatment,destinationSiteId=destination,expectedRevision=s.Revision};
    static Evaluation Edit(WorldStateStore s,string op,string site=null,string treatment=null,string destination=null)
    {
        var command=Command(s,op,site,treatment,destination);var before=Value(s.Result);int revision=s.Revision;var preview=s.Preview(command);
        Check(preview.valid,op+" preview accepted: "+site+" / "+treatment+" → "+destination);
        Check(Value(s.Result)==before&&s.Revision==revision,"Preview is pure");
        var result=s.Commit(command);Check(result.valid&&Value(result.result)==Value(preview.result),"Commit matches complete preview accounting");
        Check(s.Revision==revision+1,"One transaction advances revision once");return result;
    }
    static void Reject(WorldStateStore s,EditCommand command,string label)
    {
        string before=Value(s.Result);int revision=s.Revision;bool undo=s.CanUndo,redo=s.CanRedo;
        Check(!s.Preview(command).valid&&!s.Commit(command).valid,label);
        Check(Value(s.Result)==before&&s.Revision==revision&&s.CanUndo==undo&&s.CanRedo==redo,label+" leaves state and both histories intact");
    }
    public static string Run()
    {
        passed.Clear();const string root="worlds/meadowbrook-v1/";
        var world=JsonUtility.FromJson<WorldData>(File.ReadAllText(root+"world.json"));
        var scenario=JsonUtility.FromJson<Scenario>(File.ReadAllText(root+"scenario.json"));
        var originalWorld=JsonUtility.ToJson(world);var originalScenario=JsonUtility.ToJson(scenario);
        var s=new WorldStateStore(world,scenario);string baseline=Value(s.Result);
        Check(s.Result.production==6&&s.Result.remainingCredits==60&&!s.CanUndo&&!s.CanRedo,"Baseline counts all existing productive use, 6 units / 60 credits");
        Check(!s.Undo()&&!s.Redo()&&s.Revision==0,"Empty history is a no-op");
        Reject(s,null,"Null command rejected");Reject(s,Command(s,"unknown"),"Unknown operation rejected");Reject(s,Command(s,"reset"),"Empty reset rejected");
        Reject(s,Command(s,"remove","south-01"),"Cannot remove baseline");Reject(s,Command(s,"move","south-01",null,"south-02"),"Cannot move baseline");
        Reject(s,Command(s,"replace","south-01","restore-grass"),"Cannot replace baseline");
        Reject(s,Command(s,"add","missing","cultivate-standard"),"Unknown site rejected");
        Reject(s,Command(s,"add","south-01","missing"),"Unknown treatment rejected");
        var add=Edit(s,"add","south-01","cultivate-standard");
        Check(s.Result.production==17&&s.Result.netProduction==11&&s.Result.spentCredits==24,"Open plot replaces 1 existing unit with 12; never adds a gross 12");
        // Output arrays are caller-owned, not handles into store/history.
        add.interventions[0].siteId="reserve-11";add.result.coverAreas[0].areaM2=999;
        Check(s.TreatmentAt("south-01")=="cultivate-standard"&&s.TreatmentAt("reserve-11")==null,"Commit outputs cannot mutate canonical state");
        var preview=s.Preview(Command(s,"replace","south-01","cultivate-margin"));preview.interventions[0].treatmentId="missing";
        Check(s.TreatmentAt("south-01")=="cultivate-standard","Preview outputs are detached");
        Reject(s,Command(s,"add","south-01","cultivate-standard"),"Occupied add rejected");
        Reject(s,Command(s,"replace","south-01","cultivate-standard"),"No-op replacement rejected");
        Reject(s,Command(s,"move","south-01",null,"south-01"),"Self move rejected");
        Reject(s,Command(s,"move","south-01",null,"missing"),"Missing destination rejected");
        Reject(s,Command(s,"move","south-01",null,"reserve-11"),"Protected destination rejected");
        Edit(s,"replace","south-01","cultivate-margin");
        Check(s.Result.production==14&&s.Result.spentCredits==30,"Replacement refunds old 24 then charges 30, with 9 final units at site");
        float marginGrass=s.Result.coverAreas.Single(c=>c.coverId=="grass").areaM2;
        Edit(s,"replace","south-01","cultivate-standard");
        Check(Math.Abs(marginGrass-s.Result.coverAreas.Single(c=>c.coverId=="grass").areaM2-1687.5f)<.01f,"Living margin reserves exactly 25% of the 6750 m² site");
        Edit(s,"replace","south-01","restore-grass");Check(s.Result.production==5&&s.Result.netProduction==-1&&s.Result.remainingCredits==42,"Restoration loses baseline production and refunds the previous recipe");
        Edit(s,"remove","south-01");Check(Value(s.Result)==baseline,"Remove restores exact original production, cover and all credits");
        Check(s.Undo()&&s.TreatmentAt("south-01")=="restore-grass","Undo removal restores proposal");
        var stale=Command(s,"replace","south-01","cultivate-margin");Check(s.Redo()&&Value(s.Result)==baseline,"Redo removal restores baseline");
        Reject(s,stale,"Stale preview rejected across history navigation");
        Check(s.Undo(),"Undo for branching history");Reject(s,Command(s,"add","reserve-11","cultivate-standard"),"Invalid edit preserves redo branch");
        Edit(s,"replace","south-01","cultivate-margin");Check(!s.CanRedo&&!s.Redo(),"Successful edit clears redo branch");
        Edit(s,"move","south-01",null,"south-07");
        Check(s.TreatmentAt("south-01")==null&&s.TreatmentAt("south-07")=="cultivate-margin"&&s.Result.production==15,"Move restores source baseline and uses destination's displaced production");
        Edit(s,"add","south-02","cultivate-margin");Check(s.Result.remainingCredits==0,"Exact budget allowed");
        Reject(s,Command(s,"move","south-07",null,"south-02"),"Occupied destination rejected atomically");
        Reject(s,Command(s,"add","south-03","restore-grass"),"Overspend rejected atomically");
        string beforeReset=Value(s.Result);Edit(s,"reset");Check(Value(s.Result)==baseline,"Reset restores complete baseline in one transaction");
        Check(s.Undo()&&Value(s.Result)==beforeReset,"Reset can be undone as one operation");Check(s.Redo()&&Value(s.Result)==baseline,"Reset can be redone");
        Edit(s,"add","south-07","water-swale");Reject(s,Command(s,"move","south-07",null,"south-01"),"Incompatible destination rejected without removing source");
        Edit(s,"reset");
        // A small in-memory price/area fixture proves move repricing; canonical data remains untouched.
        var w2=JsonUtility.FromJson<WorldData>(originalWorld);var offer=w2.sites.Single(x=>x.id=="south-02").offers.Single(x=>x.treatmentId=="cultivate-standard");offer.cost=40;offer.production=8;
        var priced=new WorldStateStore(w2,scenario);Edit(priced,"add","south-01","cultivate-standard");Edit(priced,"move","south-01",null,"south-02");
        Check(priced.Result.remainingCredits==20&&priced.Result.production==13,"Move uses destination price and production, never source quote");
        Edit(priced,"move","south-02",null,"south-01");Edit(priced,"add","south-03","cultivate-standard");
        Reject(priced,Command(priced,"move","south-01",null,"south-02"),"More expensive move cannot overspend even after source refund");
        foreach(var site in world.sites)
        {
            if(site.protectedSite){Reject(s,Command(s,"add",site.id,"restore-grass"),"Protected source "+site.id);continue;}
            foreach(var treatment in site.allowedTreatments)
            {
                Edit(s,"add",site.id,treatment);
                var destinations=world.sites.Where(d=>d.id!=site.id&&!d.protectedSite&&d.allowedTreatments.Contains(treatment));
                foreach(var destination in destinations)
                {
                    string before=Value(s.Result);Edit(s,"move",site.id,null,destination.id);string moved=Value(s.Result);
                    Check(Math.Abs(s.Result.coverAreas.Sum(c=>c.areaM2)-world.sites.Sum(x=>x.analysisAreaM2))<.1f,"Move conserves site area");
                    Check(s.Undo()&&Value(s.Result)==before&&s.Redo()&&Value(s.Result)==moved&&s.Undo()&&Value(s.Result)==before,"Move undo/redo restores full result both ways");
                }
                Edit(s,"remove",site.id);Check(Value(s.Result)==baseline,"Every offered treatment removes to baseline");
            }
        }
        for(int i=0;i<40;i++)
        {
            Edit(s,"add","south-01","cultivate-standard");Edit(s,"replace","south-01","cultivate-margin");Edit(s,"move","south-01",null,"south-07");Edit(s,"reset");
            Check(Value(s.Result)==baseline,"Repeated cycle "+i+" ends at exact baseline");
        }
        Check(originalWorld==JsonUtility.ToJson(world)&&originalScenario==JsonUtility.ToJson(scenario),"Baseline world and configurable recipes remain byte-equivalent");
        Check(!s.Result.runoffAvailable,"Environmental output remains explicitly unavailable until Phase 4 integration");
        var json=JsonUtility.ToJson(new Report{count=passed.Count,checks=passed.ToArray()},true);
        Directory.CreateDirectory("docs/evidence/phase3");File.WriteAllText("docs/evidence/phase3/state-checks.json",json+"\n");return "Phase 3: "+passed.Count+" checks passed";
    }
    [Serializable] public sealed class Report{public int count;public string[] checks;}
}
