using System.Text.Json;
using System.Text.RegularExpressions;
using Terra.Core;
using Terra.Contracts;

var options=new JsonSerializerOptions{IncludeFields=true,WriteIndented=false};
int checks=0;
void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
T Read<T>(string path)=>JsonSerializer.Deserialize<T>(File.ReadAllText(path),options);
var yaml=File.ReadAllText("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
BuildActionRule[] Actions()=>Regex.Split(yaml,"  - actionId: ").Skip(1).Select(part=>{
 string art=part.Split('\n')[0].Trim();string id=art switch {"cultivation"=>"cultivate-standard","living-margin"=>"cultivate-margin","ground-cover-restoration"=>"restore-grass","vegetated-swale"=>"water-swale",_=>art};
 var dim=Regex.Match(part,@"siteSizeMetres: \{x: ([\d.]+), y: ([\d.]+)\}");
 string category=new[]{"cultivate-standard","cultivate-margin","cover-crop","conservation-rotation"}.Contains(id)?"Farming":id.Contains("housing")?"Residential":id=="public-park"||id=="village-center"?"Community":id=="restore-grass"||id=="stewardship-agreement"?"Nature":"Utilities";
 return new BuildActionRule{id=id,artId=art,title=Regex.Match(part,"title: (.+)").Groups[1].Value.Trim(),category=category,width=float.Parse(dim.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture),depth=float.Parse(dim.Groups[2].Value,System.Globalization.CultureInfo.InvariantCulture),landscape=part.Contains("landscapeSample: 1"),cost=30};
}).ToArray();

var weather = new AnnualSimulationWeather(File.ReadAllBytes("simulation-data/weather/coon-valley-v1/weather-pack.json"),
 File.ReadAllBytes("simulation-data/weather/coon-valley-v1/daily-observations.json"),
 text=>JsonSerializer.Deserialize<TownWeatherPack>(text,options),text=>JsonSerializer.Deserialize<TownDailyWeather>(text,options));
var summaries=new List<object>();
foreach(var location in LocationProfile.All)
{
 string pack=location.PackId,root="worlds/"+pack+"/";
 var world=Read<WorldData>(root+"world.json");var scenario=Read<Scenario>(root+"scenario.json");var hydro=Read<RunoffConfiguration>(root+"hydrology.json");
 UniformParcels.Apply(world,hydro,Read<UniformParcelPack>("Assets/Terra/Resources/Parcels/"+pack+".json"),pack);
 var actions=new BuildActionRules(world,Actions());actions.ExpandOffers(world,scenario);
 var definition=AnnualSimulationFactory.Create(FullAnnualGameplay.Create(world,scenario,pack,actions,new LandscapeRunoff(world,scenario,hydro)),weather,"coon-valley-demo-1",baselineWorld:world);
 var town=new AnnualTown(definition,"dense-projection/"+pack);
 var farm=definition.actions.First(a=>a.actionId=="cover-crop");town.QueueAction(farm.siteId,farm.actionId,town.Revision);
 var home=definition.actions.First(a=>a.actionId=="low-density-housing"&&a.siteId!=farm.siteId);town.QueueAction(home.siteId,home.actionId,town.Revision);
 AnnualTownDemo.Finish(town,new AuthoredAnnualConsequenceProvider());
 var history=town.History;
 var requests=new List<ReportNarrationRequest>();
 for(int i=0;i<10;i++)
 {
  var report=history[i];var sim=report.simulation;
  var request=AnnualReportProjection.Build(definition,report,i==0?null:history[i-1]);requests.Add(request);
  Check(request.reportVersion==AnnualReportProjection.DenseVersion,"Dense aggregation version");
  Check(request.facts.Length<=24&&request.facts.All(f=>f.text.Length<=600&&f.siteIds.Length<=8&&f.causalRuleIds.Length<=8),"Bounded dense request");
  Check(request.acceptedTransitionHash==sim.transitionHash,"Exact accepted transition binding");
  Check(request.facts[0].id=="budget"&&request.facts[2].id=="services","Fallback preserves budget/consequence/services");
  Check(request.facts.Any(f=>f.id=="detail-coverage"),"Partial detail coverage disclosed");
  foreach(var crop in sim.crops.GroupBy(c=>c.cropId))
  {
   string number(double n)=>n.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);
   var text=request.facts.Single(f=>f.id=="harvest-total:"+crop.Key).text;
   Check(text.Contains(number(crop.Sum(c=>c.outputUnits))+" "+crop.First().units)
    &&text.Contains(number(crop.Sum(c=>c.productiveAreaM2)/10000)+" ha"),"Every crop output and productive area included in total");
  }
  var impacts=request.facts.Single(f=>f.id=="disruption-total");
  Check(impacts.text.StartsWith(sim.impacts.Length+" site-event records"),"Every impact included in total");
  Check(impacts.siteIds.Length==0,"Aggregate does not imply a partial linked site list");
  var areas=definition.sites.ToDictionary(s=>s.id,s=>(double)s.areaM2);double totalArea=areas.Values.Sum();
  string rounded(double n)=>n.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);
  Check(impacts.text.Contains(rounded(100*sim.impacts.Sum(x=>x.conditionLoss*areas[x.siteId])/totalArea)+" percentage points"),"Impact mean uses defined whole-map area denominator");
  var recovery=request.facts.Single(f=>f.id=="recovery-total");
  Check(recovery.text.Contains("closing condition is "+rounded(100*sim.closingState.sites.Sum(x=>x.condition*areas[x.siteId])/totalArea)+"%"),"Closing condition mean includes every site area");
  string money(long cents)=>"$"+(cents/100m).ToString("N2",System.Globalization.CultureInfo.InvariantCulture);
  Check(recovery.text.Contains("Public repair paid "+money(sim.payments.Where(p=>p.kind=="new-repair"||p.kind=="prior-repair-arrears").Sum(p=>p.paidCents)))
   &&recovery.text.Contains("remaining repair debt "+money(sim.closingState.repairs.Sum(r=>r.unpaidCents))),"Every public repair payment and debt included");
  Check(request.facts.Single(f=>f.id=="budget").text.Contains(money(report.ledger.closingCashCents)),"Exact closing public money");
  Check(request.facts.Any(f=>f.text==AnnualSimulationWeather.Disclosure),"Shared-reference disclosure survives aggregation");
  var culture=System.Globalization.CultureInfo.CurrentCulture;
  try {System.Globalization.CultureInfo.CurrentCulture=new System.Globalization.CultureInfo("ar-SA");
   Check(ReportNarration.RequestHash(request)==ReportNarration.RequestHash(AnnualReportProjection.Build(definition,report,i==0?null:history[i-1])),"Dense projection culture invariance");}
  finally {System.Globalization.CultureInfo.CurrentCulture=culture;}
  if(i==9)Check(!request.facts.Any(f=>f.text.Contains("carries into the next year")),"No Year 11 implication");
 }
 var envelope=new AnnualNumericSave {sequence=31,town=town.Snapshot(),narration=requests.Select(request=>new AnnualNumericNarration {status="pending",request=request}).ToArray()};
 envelope.saveHash=AnnualNumericTransactions.Hash(envelope);
 var store=new JsonStore {Json=JsonSerializer.Serialize(envelope,options)};
 var reopened=await AnnualNumericTransactions.Open(definition,town.RunId,store);
 Check(reopened.State.history.Length==10&&reopened.State.phase==TownPhase.Completed,"Dense complete envelope replay and projection validation");
 await reopened.UseLocal(requests[9],"local-selected");
 var again=await AnnualNumericTransactions.Open(definition,town.RunId,store);
 Check(again.ForYear(10).receipt.mode=="local"&&again.State.history[9].simulation.transitionHash==history[9].simulation.transitionHash,"Large acknowledged prose save preserves numeric identity");
 Check(store.Writes==1,"One large save acknowledgement");
 int bytes=System.Text.Encoding.UTF8.GetByteCount(store.Json);
 summaries.Add(new{pack,sites=definition.sites.Length,years=10,maximumFacts=requests.Max(r=>r.facts.Length),maximumFactTextLength=requests.SelectMany(r=>r.facts).Max(f=>f.text.Length),fullEnvelopeBytes=bytes,definitionBytes=JsonSerializer.SerializeToUtf8Bytes(definition,options).Length});
 Console.WriteLine($"{pack}: 10years; {requests.Max(r=>r.facts.Length)}facts; {bytes}byte envelope; replay and acknowledged presentation save passed");
}
if(args.Contains("--write-evidence"))File.WriteAllText("docs/evidence/annual-report-projection/dense-checks.json",JsonSerializer.Serialize(new{assertions=checks,numericYears=30,unity=false,chrome=false,liveProviderCalls=0,summaries},new JsonSerializerOptions(options){WriteIndented=true})+"\n");
Console.WriteLine($"PASS {checks} dense projection assertions; no Unity/Chrome claim.");
sealed class JsonStore : IAnnualNumericStore {
 public string Json;public int Writes;static readonly JsonSerializerOptions Options=new(){IncludeFields=true};
 public Task<AnnualNumericSave> Read(CancellationToken c)=>Task.FromResult(JsonSerializer.Deserialize<AnnualNumericSave>(Json,Options));
 public Task Write(string expected,AnnualNumericSave candidate,CancellationToken c){
  if(JsonSerializer.Deserialize<AnnualNumericSave>(Json,Options).saveHash!=expected)throw new IOException("conflict");
  Json=JsonSerializer.Serialize(candidate,Options);Writes++;return Task.CompletedTask;
 }
}
