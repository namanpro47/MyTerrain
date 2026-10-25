using System.Reflection;
using System.Text.Json;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using UnityEngine;
var options = new JsonSerializerOptions { IncludeFields = true };
T Read<T>(string s) => JsonSerializer.Deserialize<T>(s,options)!;
int checks=0;
void Check(bool ok,string message) { if(!ok)throw new Exception(message); checks++; }
void Reject(Action action) { try { action(); } catch(InvalidOperationException) { checks++; return; } throw new Exception("Legacy bypass accepted"); }
var weather = new AnnualSimulationWeather(File.ReadAllBytes("simulation-data/weather/coon-valley-v1/weather-pack.json"),
 File.ReadAllBytes("simulation-data/weather/coon-valley-v1/daily-observations.json"),Read<TownWeatherPack>,Read<TownDailyWeather>);
var legacy = Read<AnnualTownDefinition>(File.ReadAllText("docs/evidence/hud-connected/definition.json"));
var rules = AnnualSimulationFactory.Create(legacy,weather,"coon-valley-demo-1");
var session = new AnnualTownSession();
Reject(()=>session.Initialize(rules,"wrong"));
session.Initialize(legacy,"legacy"); session.Save();
Check(PlayerPrefs.Values.Count==1,"Legacy saving preserved");
var storage = new Store { Block = true };
var opening = session.InitializeNumericAsync(rules,"client-run",storage);
Check(!session.NumericActive && session.State.runId=="legacy","Opening cannot publish before ack");
Reject(()=>session.Initialize(legacy,"replace")); Reject(()=>session.Save());
Reject(()=>session.QueueAction("any","any",0));
storage.Release(); await opening;
Check(session.NumericActive && session.State.runId=="client-run","Ack opens numeric session");
Action[] forbidden = {
 ()=>session.Initialize(legacy,"replace"),()=>session.ImportSave("{}"),()=>session.ExportSave(),()=>session.Save(),
 ()=>session.QueueAction("a","b",0),()=>session.RemoveQueuedAction("a",0),()=>session.UndoPlan(0),()=>session.RedoPlan(0),
 ()=>session.ClearPlan(0),()=>session.MovePlan("a","b",0),()=>session.BeginYear("x",0),()=>session.CompleteYear(null),
 ()=>session.CancelResolution(0),()=>session.ContinueYear(0),()=>session.EndYearOffline("x"),()=>session.FinishOffline()
}; foreach(var action in forbidden) Reject(action);
var park=rules.actions.First(a=>a.actionId=="public-park");
int changed=0; session.Changed += _=>changed++; session.NumericStateChanged += _=>throw new Exception("listener");
storage.Block=true;
var queue=session.QueueActionAsync(park.siteId,park.actionId,session.State.revision);
Check(session.State.queue.Length==0 && changed==0,"Waiting plan not visible");
storage.Release(); await queue;
Check(session.State.queue.Length==1 && changed==1,"Committed plan published once");
var detached=session.Engine; detached.ClearPlan(detached.Revision);
Check(session.State.queue.Length==1,"Engine read cannot bypass store");
await session.ResolveNumericYearAsync("client-run/year1",session.State.revision,"missing-config");
Check(session.State.history.Length==1 && session.NumericNarrationForYear(1).status=="visible","Saved result and local prose published");
Check(PlayerPrefs.Values.Count==1,"Numeric save never writes PlayerPrefs");
var reopened=new AnnualTownSession(); await reopened.InitializeNumericAsync(rules,"client-run",storage);
Check(reopened.ExportNumericSave()==session.ExportNumericSave(),"Reopen preserves accepted state and prose");
string key=AnnualNumericBrowserStore.KeyFor(rules,"client-run");
Check(key.Contains("/schema-3/") && key!=AnnualNumericBrowserStore.KeyFor(rules,"another-run"),"Numeric key separates schema and run");
var elsewhere=Read<AnnualTownDefinition>(JsonSerializer.Serialize(rules,options)); elsewhere.worldPackId="other-place";
Check(key!=AnnualNumericBrowserStore.KeyFor(elsewhere,"client-run"),"Numeric key separates place and definition");
var dying=new AnnualTownSession(); var slow=new Store {Block=true}; var pending=dying.InitializeNumericAsync(rules,"teardown",slow);
typeof(AnnualTownSession).GetMethod("OnDestroy",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(dying,null);
slow.Release(); bool disposed=false; try { await pending; } catch(ObjectDisposedException) { disposed=true; }
Check(disposed && !dying.Initialized && slow.Value!=null,"Teardown does not publish or claim storage rollback");
// Exact numeric transport regression from the real Unity JsonUtility failure.
const double changedByJsonUtility=0.024625792811839322;
var decimalFixture=AnnualNumericJson.ReadDefinition("{\"schemaVersion\":3,\"actions\":[{\"vegetatedFraction\":0.024625792811839322}]}");
Check(BitConverter.DoubleToInt64Bits(decimalFixture.actions[0].vegetatedFraction)==BitConverter.DoubleToInt64Bits(changedByJsonUtility),"Exact decimal-to-double transport preserves regression bits");
string numericDefinition=AnnualNumericJson.WriteDefinition(rules);
var numericDefinitionAgain=AnnualNumericJson.ReadDefinition(numericDefinition);
Check(new AnnualTown(rules,"codec").DefinitionHash==new AnnualTown(numericDefinitionAgain,"codec").DefinitionHash,"Exact definition codec hash");
var savedCopy=AnnualNumericJson.ReadSave(AnnualNumericJson.WriteSave(session.NumericSaveState));
Check(AnnualNumericTransactions.Hash(savedCopy)==session.NumericSaveState.saveHash,"Exact numeric envelope codec hash");
var culture=System.Globalization.CultureInfo.CurrentCulture;
try {System.Globalization.CultureInfo.CurrentCulture=new System.Globalization.CultureInfo("fr-FR");
 Check(AnnualNumericJson.WriteDefinition(rules)==numericDefinition,"Numeric codec ignores current culture");}
finally {System.Globalization.CultureInfo.CurrentCulture=culture;}
foreach(var invalid in new[]{"null", "{} {}", "{\"$type\":\"System.IO.FileInfo,mscorlib\",\"schemaVersion\":3}", "{\"schemaVersion\":3,\"unknown\":true}"})
{
 bool rejected=false;try{AnnualNumericJson.ReadDefinition(invalid);}catch(Exception e)when(e is ArgumentException||e is Newtonsoft.Json.JsonException){rejected=true;}
 Check(rejected,"Reject null, extra content, dynamic type metadata and unknown fields");
}
if(args.Contains("--unity-vectors"))
{
 while(session.State.phase!=TownPhase.Completed)
 {
  if(session.State.phase==TownPhase.Reviewing)await session.ContinueYearAsync(session.State.revision);
  await session.ResolveNumericYearAsync("client-run/year"+session.State.year,session.State.revision,"missing-config");
 }
 Directory.CreateDirectory("Builds/AnnualNumericClientVectors");
 File.WriteAllText("Builds/AnnualNumericClientVectors/definition.json",AnnualNumericJson.WriteDefinition(rules));
 File.WriteAllText("Builds/AnnualNumericClientVectors/save.json",session.ExportNumericSave());
 Console.WriteLine("Unity cross-runtime vectors: "+session.NumericSaveState.saveHash);
}
Console.WriteLine($"PASS {checks} adapter assertions; Unity/browser verification still required.");
sealed class Store : IAnnualNumericStore {
 public AnnualNumericSave Value; public bool Block; TaskCompletionSource wait;
 public Task<AnnualNumericSave> Read(CancellationToken cancellation) => Task.FromResult(Value);
 public async Task Write(string expected,AnnualNumericSave candidate,CancellationToken cancellation) {
  if(Block) { wait=new TaskCompletionSource(); await wait.Task; }
  if(Value?.saveHash!=expected) throw new IOException("conflict"); Value=candidate;
 }
 public void Release() {Block=false;wait!.SetResult();}
}
