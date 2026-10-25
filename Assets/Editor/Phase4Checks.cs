using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;

public static class Phase4Checks
{
    static readonly List<string> checks=new List<string>();
    static void Check(bool v,string label){if(!v)throw new Exception(label);checks.Add(label);}
    static T Read<T>(string path)=>JsonUtility.FromJson<T>(File.ReadAllText(path));
    static string Value(WorldResult r){r.revision=0;return JsonUtility.ToJson(r);}
    public static string Run()
    {
        checks.Clear();const string root="worlds/meadowbrook-v1/";
        var world=Read<WorldData>(root+"world.json");var scenario=Read<Scenario>(root+"scenario.json");var config=Read<RunoffConfiguration>(root+"hydrology.json");
        var model=new LandscapeRunoff(world,scenario,config);var state=new WorldStateStore(world,scenario,model);string baseline=Value(state.Result);
        Check(state.Result.runoffAvailable&&Math.Abs(state.Result.dischargedM3-9701.926f)<.01f,"Unity serialized baseline matches independent integration run");
        Check(model.ConnectedSites("south-07").SequenceEqual(new[]{"south-04","south-05","south-07","south-08"}),"Hypothetical southern connection membership is explicit and stable");
        foreach(var site in world.sites.Where(s=>!s.protectedSite))foreach(var offer in site.offers)
        {
            var command=new EditCommand{operation="add",siteId=site.id,treatmentId=offer.treatmentId,expectedRevision=state.Revision};
            var preview=state.Preview(command);var commit=state.Commit(command);
            Check(preview.valid&&commit.valid&&Value(preview.result)==Value(commit.result),"Unity complete preview/commit equality "+site.id+" / "+offer.treatmentId);
            Check(Math.Abs(commit.result.generatedM3-commit.result.retainedM3-commit.result.dischargedM3)<.005f,"Unity conserved volumes "+site.id+" / "+offer.treatmentId);
            Check(state.Undo()&&Value(state.Result)==baseline,"Unity undo restores model baseline "+site.id+" / "+offer.treatmentId);
        }
        var report=JsonUtility.ToJson(new Report{passed=true,count=checks.Count,checks=checks.ToArray()},true);
        Directory.CreateDirectory("docs/evidence/phase4");File.WriteAllText("docs/evidence/phase4/unity-checks.json",report+"\n");return "Phase 4: "+checks.Count+" Unity checks passed";
    }
    [Serializable] public sealed class Report{public bool passed;public int count;public string[] checks;}
}
