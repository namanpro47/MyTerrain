using Terra.Gameplay;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.Json;
using System.Text.RegularExpressions;
using Terra.Contracts;
using Terra.Core;

var options = new JsonSerializerOptions {IncludeFields=true,WriteIndented=true};
var world=JsonSerializer.Deserialize<WorldData>(File.ReadAllText("worlds/meadowbrook-v1/world.json"),options);
var scenario=JsonSerializer.Deserialize<Scenario>(File.ReadAllText("worlds/meadowbrook-v1/scenario.json"),options);
var hydro=JsonSerializer.Deserialize<RunoffConfiguration>(File.ReadAllText("worlds/meadowbrook-v1/hydrology.json"),options);
bool connected=args.Contains("--connected");
string evidence=connected?"docs/evidence/hud-connected":"docs/evidence/full-game";
int evidenceArg=Array.IndexOf(args,"--evidence");
if(evidenceArg>=0)
{
 if(evidenceArg+1>=args.Length||args[evidenceArg+1].StartsWith("--"))throw new ArgumentException("--evidence requires an output directory");
 evidence=args[evidenceArg+1];
}
if(connected)
{
 var before=world.sites.Sum(s=>(double)s.analysisAreaM2)+hydro.context.Sum(c=>(double)c.areaM2);
 AnnualSpatialOpportunities.ConnectFields(world,hydro,JsonSerializer.Deserialize<WorldData>(File.ReadAllText("Assets/Terra/Resources/ConnectedFields.json"),options).sites);
 var after=world.sites.Sum(s=>(double)s.analysisAreaM2)+hydro.context.Sum(c=>(double)c.areaM2);
 if(Math.Abs(after-before)>.1)throw new Exception("Connected partition area changed");
 bool twice=false;try{AnnualSpatialOpportunities.ConnectFields(world,hydro,world.sites.Take(8).ToArray());}catch(ArgumentException){twice=true;}
 if(!twice)throw new Exception("Duplicate application must reject");
}
AnnualSpatialOpportunities.AddWind(world,hydro);
var yaml=File.ReadAllText("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
var rules=new BuildActionRules(world,Regex.Split(yaml,"  - actionId: ").Skip(1).Select(part=>{
 string art=part.Split('\n')[0].Trim();string id=art switch {"cultivation"=>"cultivate-standard","living-margin"=>"cultivate-margin","ground-cover-restoration"=>"restore-grass","vegetated-swale"=>"water-swale",_=>art};
 var dim=Regex.Match(part,@"siteSizeMetres: \{x: ([\d.]+), y: ([\d.]+)\}");
 string category=new[]{"cultivate-standard","cultivate-margin","cover-crop","conservation-rotation"}.Contains(id)?"Farming":id.Contains("housing")?"Residential":id=="public-park"||id=="village-center"?"Community":id=="restore-grass"||id=="stewardship-agreement"?"Nature":"Utilities";
 return new BuildActionRule {id=id,artId=art,title=Regex.Match(part,"title: (.+)").Groups[1].Value.Trim(),category=category,width=float.Parse(dim.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture),depth=float.Parse(dim.Groups[2].Value,System.Globalization.CultureInfo.InvariantCulture),landscape=part.Contains("landscapeSample: 1"),cost=30};
}).ToArray());
rules.ExpandOffers(world,scenario);
var runoff=new LandscapeRunoff(world,scenario,hydro);
Console.WriteLine(string.Join("\n",rules.actions.Select(a=>a.id+": "+world.sites.Count(s=>rules.Rejection(s,a.id)==null)+" fitting fields")));
var d=FullAnnualGameplay.Create(world,scenario,"meadowbrook-v1",rules,runoff);

int checks=0;void Check(bool value,string label){checks++;if(!value)throw new Exception(label);}
var definition=AnnualPlayerRules.Create(d,"meadowbrook-v1","chapter-session");
AnnualTownSession Town(string key,bool restore=false){var t=new AnnualTownSession{saveKey=key};t.Initialize(definition,"saved-run",restore);return t;}
AnnualChapterSession Chapter(AnnualTownSession t,string key)=>new AnnualChapterSession{town=t,saveKey=key,endpoint="http://127.0.0.1:8787/chapter",useJavaScriptBackend=false};
var town=Town("rules-town");var chapters=Chapter(town,"rules-chapters");
UnityWebRequest.OnSend=()=>{
 var saved=JsonUtility.FromJson<AnnualTownSnapshot>(AnnualTownSession.ReadStored(town.saveKey));
 Check(saved.history.Length==1&&saved.phase==TownPhase.Reviewing,"numerical year saved before transport starts");
 Check(PlayerPrefs.HasKey(chapters.saveKey),"local chapter saved before transport starts");
};
chapters.ResolveOnline();Check(chapters.Busy&&town.State.history.Length==1,"AI prose pending while year accepted");
string accepted=TownCopies.Hash(town.State);long acceptedCash=town.State.cashCents;
var transport=UnityWebRequest.Last;transport.result=UnityWebRequest.Result.ProtocolError;transport.responseCode=429;transport.downloadHandler.text="{}";transport.operation.isDone=true;
chapters.Poll();Check(!chapters.Busy&&chapters.Latest.mode=="offline","quota failure retains local chapter");
Check(chapters.Latest.chapter.introduction.StartsWith("AI unavailable."),"fallback visibly labelled");
Check(TownCopies.Hash(town.State)==accepted&&town.State.cashCents==acceptedCash,"quota failure cannot change accepted numbers");
var restored=Town("rules-town",true);var restoredChapters=Chapter(restored,"rules-chapters");restoredChapters.RestoreSaved();
Check(TownCopies.Hash(restored.State)==accepted&&restoredChapters.ForYear(1)!=null,"reload accepted result and local chapter");
string oldChapterSave=restoredChapters.ExportSave();
restoredChapters.Continue();UnityWebRequest.OnSend=null;restoredChapters.ResolveOnline();
var receipt=restoredChapters.ForYear(2);receipt.mode="gemini";receipt.model="test-narrator";receipt.chapter.title="Saved year explained";
string accepted2=TownCopies.Hash(restored.State);
transport=UnityWebRequest.Last;transport.result=UnityWebRequest.Result.Success;transport.responseCode=200;transport.downloadHandler.text=JsonUtility.ToJson(receipt);transport.operation.isDone=true;restoredChapters.Poll();
Check(restoredChapters.ForYear(2).mode=="gemini","AI replaces prose only");Check(TownCopies.Hash(restored.State)==accepted2,"AI cannot reapply money/events");
// Simulate a crash after saving the new annual key but before saving the chapter key.
PlayerPrefs.SetString("rules-chapters",oldChapterSave);
var newest=Town("rules-town",true);var stale=Chapter(newest,"rules-chapters");stale.RestoreSaved();
Check(newest.State.history.Length==2,"older chapter save cannot rewind a newer accepted year");
Check(stale.ForYear(2)?.mode=="offline","missing prose rebuilt locally from accepted results");
stale.Continue();stale.ResolveOnline();string third=TownCopies.Hash(newest.State);stale.StopRequest();
Check(TownCopies.Hash(newest.State)==third&&stale.ForYear(3)!=null,"stop optional AI retains saved year");
stale.Continue();stale.ResolveOnline();stale.Continue();Check(newest.State.phase==TownPhase.Planning&&newest.State.year==5&&!stale.Busy,"player can continue without waiting for prose");
Check(!AnnualChapterSession.UsesHostedProtocol("http://127.0.0.1:8787/chapter")&&!AnnualChapterSession.UsesHostedProtocol("http://localhost:8787/chapter"),"loopback uses frozen-request C# protocol");
Check(AnnualChapterSession.UsesHostedProtocol("https://terra-chapters.vercel.app/chapter"),"hosted uses facts protocol");
// Finish locally, then request only the ending prose without replaying live cash/events.
stale.StopRequest();stale.FinishOffline();Check(newest.State.phase==TownPhase.Completed,"offline fast-forward reaches accepted ending");
var endingHash=TownCopies.Hash(newest.State);stale.ResolveOnline();
Check(stale.Busy&&TownCopies.Hash(newest.State)==endingHash,"accepted Year 10 can request AI without changing state");
var endingRequest=JsonUtility.FromJson<AnnualChapterRequest>(System.Text.Encoding.UTF8.GetString(UnityWebRequest.Last.uploadHandler.data));
Check(endingRequest.snapshot.pendingRequest.year==10&&endingRequest.snapshot.history.Length==9,"ending request retains nine earlier years and original frozen Year 10");
var endingReceipt=stale.ForYear(10);endingReceipt.mode="gemini";endingReceipt.model="ending-test";
transport=UnityWebRequest.Last;transport.result=UnityWebRequest.Result.Success;transport.responseCode=200;transport.downloadHandler.text=JsonUtility.ToJson(endingReceipt);transport.operation.isDone=true;stale.Poll();
Check(stale.ForYear(10).mode=="gemini"&&TownCopies.Hash(newest.State)==endingHash,"AI ending replaces prose only");
// Storage remains exact, while compressed backups fit alongside current saves.
newest.Save();stale.Save();
string fullSave=newest.ExportSave(), fullChapter=stale.ExportSave();
AnnualTownSession.WriteStored(newest.saveKey+".previous",fullSave);
AnnualTownSession.WriteStored(stale.saveKey+".previous",fullChapter);
AnnualTownSession.WriteStored(newest.saveKey+".previous.definition",JsonUtility.ToJson(definition));
Check(AnnualTownSession.ReadStored(newest.saveKey+".previous")==fullSave,"compressed complete history is lossless");
Check(AnnualTownSession.ReadStored(stale.saveKey+".previous")==fullChapter,"compressed chapters are lossless");
Check(PlayerPrefs.Values.Sum(kv=>System.Text.Encoding.UTF8.GetByteCount(kv.Key+kv.Value))<1024*1024,"current and archived ten-year runs fit WebGL quota");
PlayerPrefs.SetString("legacy-json",fullSave);
var legacy=Town("legacy-json",true);Check(TownCopies.Hash(legacy.State)==endingHash,"legacy plain JSON still restores exactly");
var compressed=Town(newest.saveKey,true);Check(TownCopies.Hash(compressed.State)==endingHash,"compressed current save restores exactly");
Console.WriteLine("Save-first chapter session checks passed: "+checks+" (transport/Unity stubs, no live provider)");
