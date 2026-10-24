using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Terra.Contracts;
using Terra.Core;

public static class Phase2Checks
{
    static readonly List<string> Passed=new List<string>();
    static T Read<T>(string path)=>JsonUtility.FromJson<T>(File.ReadAllText(path));
    static void Check(bool value,string message){if(!value)throw new Exception(message);Passed.Add(message);}
    static void Reject(Action action,string message)
    {bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}Check(rejected,message);}
    public static string Run()
    {
        Passed.Clear();const string root="worlds/meadowbrook-v1/";
        var m=Read<Manifest>(root+"manifest.json");var w=Read<WorldData>(root+"world.json");var s=Read<Scenario>(root+"scenario.json");
        PackValidator.Validate(m,w,s);Check(true,"Real geography accepted by unmodified v1 PackValidator and Unity serializer");
        var serialized=JsonUtility.ToJson(w);var roundtrip=JsonUtility.FromJson<WorldData>(serialized);
        Check(JsonUtility.ToJson(roundtrip)==serialized,"Real pack serialization round trip retains all geometry and site data");
        Check(w.sites.Length==12&&w.sites.Count(x=>!x.protectedSite)==10,"Ten editable sites and two protected scenario zones");
        Check(w.terrain.heights.Length==119*127&&w.features.Any(x=>x.kind=="water")&&w.features.Any(x=>x.kind=="path"),"Actual terrain, mapped water and mapped roads present");
        foreach(var site in w.sites)
        {
            var store=new WorldStateStore(w,s);
            if(site.protectedSite)
            {
                Check(!store.Commit(new EditCommand{operation="add",siteId=site.id,treatmentId="cultivate-standard",expectedRevision=0}).valid,"Reject editing "+site.id);
                continue;
            }
            foreach(var offer in site.offers)
            {
                var cmd=new EditCommand{operation="add",siteId=site.id,treatmentId=offer.treatmentId,expectedRevision=store.Revision};
                var preview=store.Preview(cmd);Check(preview.valid,"Preview "+site.id+" / "+offer.treatmentId);
                var result=store.Commit(cmd);Check(result.valid&&result.result.production==preview.result.production&&result.result.remainingCredits==preview.result.remainingCredits,"Commit equals preview "+site.id+" / "+offer.treatmentId);
                Check(Math.Abs(result.result.coverAreas.Sum(x=>x.areaM2)-w.sites.Sum(x=>x.analysisAreaM2))<.1f,"Area conserved "+site.id+" / "+offer.treatmentId);
                Check(store.Undo()&&store.Result.production==6&&store.Result.remainingCredits==60,"Undo exact baseline "+site.id+" / "+offer.treatmentId);
            }
        }
        Check(JsonUtility.ToJson(w)==serialized,"Real pack baseline remains immutable after every site/treatment");
        m.schemaVersion=2;Reject(()=>PackValidator.Validate(m,w,s),"Unsupported schema rejected");m.schemaVersion=1;
        var bounds=m.contextBounds;m.contextBounds=new[]{1f,1f,-1f,-1f};Reject(()=>PackValidator.Validate(m,w,s),"Malformed real bounds rejected");m.contextBounds=bounds;
        var terrain=w.terrain;w.terrain=null;Reject(()=>PackValidator.Validate(m,w,s),"Missing real terrain rejected");w.terrain=terrain;
        var fixtureM=Read<Manifest>("worlds/fixture-v1/manifest.json");var fixtureW=Read<WorldData>("worlds/fixture-v1/world.json");var fixtureS=Read<Scenario>("worlds/fixture-v1/scenario.json");
        PackValidator.Validate(fixtureM,fixtureW,fixtureS);Check(true,"Tiny golden fixture remains valid");
        var report=new Report{count=Passed.Count,checks=Passed.ToArray()};var json=JsonUtility.ToJson(report,true);
        Directory.CreateDirectory("docs/evidence/phase2");File.WriteAllText("docs/evidence/phase2/contract-checks.json",json+"\n");return json;
    }
    [Serializable] public sealed class Report { public int count;public string[] checks; }
}
