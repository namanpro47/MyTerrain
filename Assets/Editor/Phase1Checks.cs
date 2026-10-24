using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;
public static class Phase1Checks
{
    static readonly List<string> passed=new List<string>();
    static T Read<T>(string path)=>JsonUtility.FromJson<T>(File.ReadAllText(path));
    static void Check(bool result,string name){if(!result)throw new Exception("FAILED: "+name);passed.Add(name);}
    public static string Run()
    {
        passed.Clear();var m=Read<Manifest>("worlds/fixture-v1/manifest.json");var w=Read<WorldData>("worlds/fixture-v1/world.json");var s=Read<Scenario>("worlds/fixture-v1/scenario.json");
        PackValidator.Validate(m,w,s);Check(true,"valid tiny pack accepted");
        string original=JsonUtility.ToJson(w);var roundtrip=JsonUtility.FromJson<WorldData>(original);Check(JsonUtility.ToJson(roundtrip)==original,"JsonUtility round-trip preserves arrays, IDs, fractions and offers");
        foreach(var path in Directory.GetFiles("tests/fixtures/invalid","*.json"))
        {
            bool rejected=false;try{if(path.EndsWith("unknown-version.json"))PackValidator.Validate(Read<Manifest>(path),w,s);else PackValidator.Validate(m,Read<WorldData>(path),s);}catch(ArgumentException){rejected=true;}Check(rejected,"reject "+Path.GetFileName(path));
        }
        var store=new WorldStateStore(w,s);Check(store.Result.production==6&&store.Result.remainingCredits==60,"baseline includes existing production");
        var cmd=new EditCommand{operation="add",siteId="site-2",treatmentId="cultivate-margin",expectedRevision=0};
        var preview=store.Preview(cmd);Check(preview.valid&&preview.result.production==9.5f&&preview.result.remainingCredits==30,"margin preview deducts displaced baseline production");
        preview.interventions[0].treatmentId="restore-grass";
        Check(store.Result.production==6,"mutating returned preview does not mutate stored state");
        preview=store.Preview(cmd);
        Check(store.Revision==0&&store.Result.production==6&&!store.CanUndo,"preview leaves state/history unchanged");
        var applied=store.Commit(cmd);Check(applied.valid&&applied.result.production==preview.result.production&&applied.result.remainingCredits==preview.result.remainingCredits,"preview equals committed accounting");
        Check(!store.Commit(cmd).valid&&store.Revision==1,"stale revision rejected without mutation");
        Check(!store.Commit(new EditCommand{operation="add",siteId="site-4",treatmentId="cultivate-standard",expectedRevision=1}).valid,"protected site rejected");
        Check(store.Commit(new EditCommand{operation="add",siteId="site-1",treatmentId="cultivate-standard",expectedRevision=1}).valid,"second site committed");
        Check(!store.Commit(new EditCommand{operation="add",siteId="site-3",treatmentId="cultivate-standard",expectedRevision=2}).valid&&store.Result.remainingCredits==6,"overspend rejected atomically");
        Check(store.Commit(new EditCommand{operation="replace",siteId="site-2",treatmentId="restore-grass",expectedRevision=2}).valid&&store.Result.production==14&&store.Result.remainingCredits==18,"replace recomputes final costs and lost production");
        Check(Math.Abs(store.Result.coverAreas.Sum(x=>x.areaM2)-w.sites.Sum(x=>x.analysisAreaM2))<.01,"cover area conserved");
        Check(!store.Result.runoffAvailable,"unintegrated runoff explicitly unavailable");
        Check(store.Undo()&&store.Result.production==21.5f&&store.Result.remainingCredits==6,"undo returns previous proposal list");
        store.Undo();store.Undo();Check(store.Result.production==6&&store.Result.remainingCredits==60&&!store.CanUndo,"undo to exact baseline accounting");
        Check(JsonUtility.ToJson(w)==original,"baseline remains immutable through edit/undo sequence");
        var input=Read<SimulationInput>("tests/fixtures/simulation/input.json");var expected=Read<SimulationOutput>("tests/fixtures/simulation/expected.json");
        Check(input.sites.Length==3&&expected.sites.Length==3&&input.revision==expected.revision,"simulation I/O fixture deserializes independently of scene");
        var report=new Report{date=DateTime.UtcNow.ToString("o"),checks=passed.ToArray(),count=passed.Count};string json=JsonUtility.ToJson(report,true);Directory.CreateDirectory("docs/evidence/phase1");File.WriteAllText("docs/evidence/phase1/contract-checks.json",json+"\n");return json;
    }
    [Serializable] public sealed class Report{public string date;public int count;public string[] checks;}
}
