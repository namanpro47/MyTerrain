using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;

public static class Phase3Checks
{
    static readonly List<string> Passed=new List<string>();
    static void Check(bool ok,string label){if(!ok)throw new Exception(label);Passed.Add(label);}
    static T Read<T>(string path)=>JsonUtility.FromJson<T>(File.ReadAllText(path));
    static string State(WorldStateStore store){var result=store.Result;result.revision=0;return JsonUtility.ToJson(result);}
    static EditCommand Command(WorldStateStore store,string operation,string site=null,string treatment=null,string destination=null)
        =>new EditCommand{operation=operation,siteId=site,treatmentId=treatment,destinationSiteId=destination,expectedRevision=store.Revision};
    static void Reject(WorldStateStore store,EditCommand command,string label)
    {
        string before=State(store);int revision=store.Revision;bool undo=store.CanUndo,redo=store.CanRedo;
        Check(!store.Preview(command).valid&&!store.Commit(command).valid&&State(store)==before&&store.Revision==revision&&store.CanUndo==undo&&store.CanRedo==redo,label);
    }
    static void Edit(WorldStateStore store,EditCommand command,string label)
    {
        string before=State(store);int revision=store.Revision;var preview=store.Preview(command);
        Check(preview.valid&&State(store)==before&&store.Revision==revision,label+": pure preview");
        var committed=store.Commit(command);preview.result.revision=committed.result.revision;
        Check(committed.valid&&store.Revision==revision+1&&JsonUtility.ToJson(preview.result)==JsonUtility.ToJson(committed.result),label+": commit matches entire preview");
    }
    public static string Run()
    {
        Passed.Clear();const string root="worlds/meadowbrook-v1/";
        var world=Read<WorldData>(root+"world.json");var scenario=Read<Scenario>(root+"scenario.json");
        string originalWorld=JsonUtility.ToJson(world),originalScenario=JsonUtility.ToJson(scenario);
        // The catalog validator must survive composition with the reversible editor.
        var restricted=new WorldStateStore(world,scenario,(site,treatment)=>site.id=="south-02"||treatment=="restore-grass"?"Fixture placement restriction":null);
        Reject(restricted,Command(restricted,"add","south-02","cultivate-standard"),"Placement callback rejects add without mutation");
        Edit(restricted,Command(restricted,"add","south-01","cultivate-standard"),"Allowed callback placement");
        Reject(restricted,Command(restricted,"replace","south-01","restore-grass"),"Placement callback rejects replacement without refund");
        Reject(restricted,Command(restricted,"move","south-01",null,"south-02"),"Placement callback validates move destination");
        var store=new WorldStateStore(world,scenario);string baseline=State(store);
        Check(store.Result.production==6&&store.Result.remainingCredits==60&&!store.Result.runoffAvailable,"Baseline: 6 illustrative units, 60 credits, runoff unavailable");
        Check(!store.Undo()&&!store.Redo()&&store.Revision==0,"Empty history is a no-op");
        Reject(store,null,"Null command rejected without mutation");
        Reject(store,Command(store,"unknown"),"Unknown operation rejected");
        Reject(store,Command(store,"reset"),"Empty reset does not create history");
        foreach(string op in new[]{"add","replace","remove","move"})
        {
            Reject(store,Command(store,op,"reserve-11","cultivate-standard","south-01"),"Protected source rejects "+op);
            Reject(store,Command(store,op,"missing","cultivate-standard","south-01"),"Unknown source rejects "+op);
        }
        foreach(string op in new[]{"replace","remove","move"})Reject(store,Command(store,op,"south-01","restore-grass","south-02"),"Empty source rejects "+op);
        Reject(store,Command(store,"add","south-01",null),"Missing treatment rejected");
        Reject(store,Command(store,"add","south-01","water-swale"),"Ineligible treatment rejected");
        Edit(store,Command(store,"add","south-01","cultivate-standard"),"Cultivate west");
        Check(store.Result.production==17&&store.Result.remainingCredits==36,"Cultivation displaces the original one unit, adds eleven");
        string west=State(store);
        Reject(store,Command(store,"add","south-01","restore-grass"),"Add cannot overwrite occupied source");
        Reject(store,Command(store,"replace","south-01","cultivate-standard"),"Identical replacement is not a history entry");
        foreach(string destination in new[]{null,"missing","south-01","reserve-11"})Reject(store,Command(store,"move","south-01",null,destination),"Invalid move target: "+(destination??"null"));
        var stale=Command(store,"remove","south-01");
        Edit(store,Command(store,"replace","south-01","cultivate-margin"),"Replace with margin");
        Check(store.Result.production==14&&store.Result.remainingCredits==30,"Replacement charges final 30, not 24 plus 30; production becomes 14");
        Reject(store,stale,"Stale preview cannot commit");
        Edit(store,Command(store,"replace","south-01","restore-grass"),"Restore productive source");
        Check(store.Result.production==5&&store.Result.remainingCredits==42,"Restoration loses baseline productive unit and refunds replacement difference");
        Edit(store,Command(store,"remove","south-01"),"Remove restoration");
        Check(State(store)==baseline,"Remove returns all costs, cover and productive capacity exactly to baseline");
        Check(store.Undo()&&store.Result.production==5&&store.Result.remainingCredits==42,"Undo removal restores restoration");
        Check(store.Redo()&&State(store)==baseline,"Redo removal restores baseline");
        Check(store.Undo(),"Prepare redo branch");
        Reject(store,Command(store,"add","reserve-11","restore-grass"),"Invalid action preserves redo branch");
        Edit(store,Command(store,"replace","south-01","cultivate-standard"),"Branch history with a valid edit");
        Check(!store.CanRedo&&!store.Redo(),"Valid branch clears abandoned redo history");
        Edit(store,Command(store,"move","south-01",null,"south-07"),"Move to nonproductive meadow");
        Check(store.TreatmentAt("south-01")==null&&store.TreatmentAt("south-07")=="cultivate-standard"&&store.Result.production==18&&store.Result.remainingCredits==36,"Move restores source baseline and uses destination accounting once");
        Check(store.Undo()&&State(store)==west,"Single undo reverses both halves of move");
        Check(store.Redo()&&store.Result.production==18,"Single redo reapplies both halves of move");
        Edit(store,Command(store,"add","south-02","cultivate-standard"),"Add second plot");
        Reject(store,Command(store,"move","south-07",null,"south-02"),"Move rejects occupied destination without replacing it");
        Reject(store,Command(store,"add","south-03","restore-grass"),"Overspend leaves proposal and history intact");
        string beforeReset=State(store);var staleAfterReset=Command(store,"remove","south-07");
        Edit(store,Command(store,"reset"),"Clear all proposals");
        Check(State(store)==baseline&&!store.HasProposals,"Clear restores complete baseline");
        Reject(store,staleAfterReset,"Clear invalidates old previews");
        Check(store.Undo()&&State(store)==beforeReset,"Clear can be undone as one action");
        Check(store.Redo()&&State(store)==baseline,"Clear can be redone as one action");
        // A tiny in-memory technical fixture tests repricing; it is not another authored location.
        var repriced=Read<WorldData>(root+"world.json");var dest=repriced.sites.Single(s=>s.id=="south-07").offers.Single(o=>o.treatmentId=="cultivate-standard");dest.cost=41;dest.production=20;
        var pricing=new WorldStateStore(repriced,scenario);
        Edit(pricing,Command(pricing,"add","south-01","cultivate-standard"),"Pricing fixture source");
        Edit(pricing,Command(pricing,"move","south-01","ignored-for-move","south-07"),"Destination repricing");
        Check(pricing.Result.remainingCredits==19&&pricing.Result.production==26,"Move inherits recipe but destination offer sets cost and capacity");
        pricing.Undo();Edit(pricing,Command(pricing,"add","south-02","cultivate-standard"),"Pricing fixture second proposal");
        Reject(pricing,Command(pricing,"move","south-01",null,"south-07"),"Unaffordable move is atomic and refunds nothing");
        var swale=new WorldStateStore(world,scenario);
        Edit(swale,Command(swale,"add","south-07","water-swale"),"Swale source fixture");
        Reject(swale,Command(swale,"move","south-07",null,"south-01"),"Ineligible destination rejects move and preserves source");
        var beforeUndo=Command(swale,"remove","south-07");swale.Undo();
        Reject(swale,beforeUndo,"Undo invalidates a previously valid preview");
        var beforeRedo=Command(swale,"add","south-01","restore-grass");swale.Redo();
        Reject(swale,beforeRedo,"Redo invalidates a previously valid preview");
        // Mutation of returned previews/results must not corrupt current proposals or future history.
        var safe=new WorldStateStore(world,scenario);var mutable=safe.Commit(Command(safe,"add","south-01","cultivate-standard"));
        mutable.interventions[0].siteId="reserve-11";mutable.interventions[0].treatmentId="bogus";mutable.result.coverAreas[0].areaM2=-1;
        Check(State(safe)==west&&safe.TreatmentAt("south-01")=="cultivate-standard","Caller mutations cannot change published store");
        safe.Undo();safe.Redo();Check(State(safe)==west,"Caller mutations cannot change redo snapshot");
        var sample=safe.Preview(Command(safe,"remove","south-01"));sample.result.sites[0].production=999;
        Check(State(safe)==west,"Caller mutations cannot change preview source");
        // Exercise every offered recipe in both directions across all eligible sites, including swales.
        int moves=0;
        foreach(var source in world.sites.Where(s=>!s.protectedSite))foreach(var offer in source.offers)
        foreach(var destination in world.sites.Where(s=>s.id!=source.id&&!s.protectedSite&&s.allowedTreatments.Contains(offer.treatmentId)))
        {
            var sweep=new WorldStateStore(world,scenario);
            Check(sweep.Commit(Command(sweep,"add",source.id,offer.treatmentId)).valid,"Sweep source "+source.id+" / "+offer.treatmentId+" to "+destination.id);
            string prior=State(sweep);var destinationOffer=destination.offers.Single(o=>o.treatmentId==offer.treatmentId);
            Edit(sweep,Command(sweep,"move",source.id,null,destination.id),"Move "+source.id+" to "+destination.id+" / "+offer.treatmentId);
            Check(sweep.Result.remainingCredits==60-destinationOffer.cost&&Math.Abs(sweep.Result.production-(6-destination.baselineProduction+destinationOffer.production))<.001f,"Destination-only accounting "+(++moves));
            Check(Math.Abs(sweep.Result.coverAreas.Sum(a=>a.areaM2)-world.sites.Sum(s=>s.analysisAreaM2))<.1f,"Managed area conserved "+moves);
            Check(sweep.Undo()&&State(sweep)==prior&&sweep.Redo(),"History equivalence "+moves);
        }
        for(int cycle=0;cycle<25;cycle++)
        {
            var repeat=new WorldStateStore(world,scenario);
            repeat.Commit(Command(repeat,"add","south-01","cultivate-standard"));repeat.Commit(Command(repeat,"move","south-01",null,"south-07"));
            repeat.Commit(Command(repeat,"replace","south-07","cultivate-margin"));repeat.Commit(Command(repeat,"add","south-02","restore-grass"));
            repeat.Commit(Command(repeat,"remove","south-07"));repeat.Commit(Command(repeat,"reset"));
            string final=State(repeat);int undos=0,redos=0;while(repeat.Undo())undos++;
            Check(State(repeat)==baseline&&undos==6,"Repeated sequence exact undo "+cycle);
            while(repeat.Redo())redos++;
            Check(State(repeat)==final&&State(repeat)==baseline&&redos==6,"Repeated sequence exact redo "+cycle);
        }
        Check(JsonUtility.ToJson(world)==originalWorld&&JsonUtility.ToJson(scenario)==originalScenario,"Baseline and scenario unchanged after all operations");
        string report=JsonUtility.ToJson(new Report{count=Passed.Count,moves=moves,checks=Passed.ToArray()},true);
        Directory.CreateDirectory("docs/evidence/phase3");File.WriteAllText("docs/evidence/phase3/transaction-checks.json",report+"\n");return report;
    }
    [Serializable] public sealed class Report {public int count,moves;public string[] checks;}
}
