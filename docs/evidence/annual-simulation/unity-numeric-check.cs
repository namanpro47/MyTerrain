// Standalone runtime consistency check. No scene, live session, preferences or saves are modified.
int checks=0;
var failures=new System.Collections.Generic.List<string>();
var results=new System.Collections.Generic.List<object>();
void Check(bool okay,string reason){checks++;if(!okay)failures.Add(reason);}
var stopwatch=System.Diagnostics.Stopwatch.StartNew();
var root=System.IO.Directory.GetCurrentDirectory();
var pack=System.IO.File.ReadAllBytes(System.IO.Path.Combine(root,"simulation-data/weather/coon-valley-v1/weather-pack.json"));
var daily=System.IO.File.ReadAllBytes(System.IO.Path.Combine(root,"simulation-data/weather/coon-valley-v1/daily-observations.json"));
var weather=new Terra.Core.AnnualSimulationWeather(pack,daily,UnityEngine.JsonUtility.FromJson<Terra.Contracts.TownWeatherPack>,UnityEngine.JsonUtility.FromJson<Terra.Contracts.TownDailyWeather>);
var legacy=UnityEngine.JsonUtility.FromJson<Terra.Contracts.AnnualTownDefinition>(System.IO.File.ReadAllText(System.IO.Path.Combine(root,"docs/evidence/hud-connected/definition.json")));
var fresh=Terra.Core.AnnualSimulationFactory.Create(legacy,weather,"coon-valley-demo-1");
Check(fresh.simulation.weather.startYear==2015,"Unity weather sampler differs");
foreach(string name in new[]{"baseline","public-repair","repair-debt","six-centers"})
{
 var folder=System.IO.Path.Combine(root,"Builds/AnnualSimulation/UnityVectors",name);
 var d=UnityEngine.JsonUtility.FromJson<Terra.Contracts.AnnualTownDefinition>(System.IO.File.ReadAllText(System.IO.Path.Combine(folder,"definition.json")));
 var saved=UnityEngine.JsonUtility.FromJson<Terra.Contracts.AnnualTownSnapshot>(System.IO.File.ReadAllText(System.IO.Path.Combine(folder,"snapshot.json")));
 var run=Terra.Core.AnnualTown.Restore(d,saved);
 Check(run.DefinitionHash==saved.definitionHash,name+" cross-runtime definition hash");
 Check(run.Phase==Terra.Contracts.TownPhase.Completed&&run.History.Length==10,name+" complete decade");
 Check(run.CashCents==saved.cashCents&&run.ArrearsCents==saved.arrearsCents,name+" exact ledger");
 for(int y=0;y<10;y++)
 {
  var report=run.History[y];Terra.Core.AnnualSimulation.ValidateReport(d,report);Terra.Core.AnnualSimulation.ValidateReportHash(report);
  Check(report.simulation.transitionHash==saved.history[y].simulation.transitionHash,name+" cross-runtime full report hash year"+(y+1));
  var request=Terra.Core.AnnualReportProjection.Build(d,report,y==0?null:run.History[y-1]);
  var local=Terra.Core.ReportNarration.Local(request,"local-selected");
  Terra.Core.ReportNarration.ValidateReceipt(request,local);
  Check(local.acceptedTransitionHash==report.simulation.transitionHash,name+" local report identity year"+(y+1));
 }
 var roundtrip=Terra.Core.AnnualTown.Restore(d,UnityEngine.JsonUtility.FromJson<Terra.Contracts.AnnualTownSnapshot>(UnityEngine.JsonUtility.ToJson(run.Snapshot())));
 Check(UnityEngine.JsonUtility.ToJson(roundtrip.Snapshot())==UnityEngine.JsonUtility.ToJson(run.Snapshot()),name+" Unity JSON reload equality");
 results.Add(new{name,years=run.History.Length,cash=run.CashCents,definitionHash=run.DefinitionHash,lastHash=run.History[9].simulation.transitionHash});
}
return new{checks,failures,results,elapsedMilliseconds=stopwatch.ElapsedMilliseconds,unity=UnityEngine.Application.unityVersion,changedLiveState=false};
