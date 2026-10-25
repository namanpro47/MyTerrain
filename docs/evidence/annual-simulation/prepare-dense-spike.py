from pathlib import Path
import subprocess
root=Path('Builds/DenseModelSpike');root.mkdir(exist_ok=True)
for folder in ['Core','Contracts','Simulation']:
 dst=root/'Code'/folder;dst.mkdir(parents=True,exist_ok=True)
 for p in Path('Assets/Terra/Scripts',folder).glob('*.cs'):(dst/p.name).write_bytes(p.read_bytes())
for name in ['UniformParcels','StartingSettlement','BuildActionRules','FullAnnualGameplay','LocationProfile']:
 path='Assets/Terra/Scripts/Core/'+name+'.cs';(root/'Code/Core'/Path(path).name).write_bytes(subprocess.check_output(['git','show','ea37af0:'+path]))
(root/'Parcels').mkdir(exist_ok=True)
for name in ['meadowbrook-v1','point-reyes-v1','suusamyr-v1']:
 path='Assets/Terra/Resources/Parcels/'+name+'.json';(root/'Parcels'/Path(path).name).write_bytes(subprocess.check_output(['git','show','ea37af0:'+path]))
(root/'DenseModelSpike.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings></PropertyGroup></Project>')
source=subprocess.check_output(['git','show','ea37af0:tests/parcels/Program.cs']).decode();prefix=source[:source.index('var summaries=')]
program=r'''
var packBytes=File.ReadAllBytes("simulation-data/weather/coon-valley-v1/weather-pack.json");
var dailyBytes=File.ReadAllBytes("simulation-data/weather/coon-valley-v1/daily-observations.json");
var weather=new AnnualSimulationWeather(packBytes,dailyBytes,s=>JsonSerializer.Deserialize<TownWeatherPack>(s,options),s=>JsonSerializer.Deserialize<TownDailyWeather>(s,options));
var summaries=new List<object>();
foreach(var location in LocationProfile.All)
{
 var clock=System.Diagnostics.Stopwatch.StartNew();
 string pack=location.PackId, root="worlds/"+pack+"/";
 var world=Read<WorldData>(root+"world.json");var scenario=Read<Scenario>(root+"scenario.json");var hydro=Read<RunoffConfiguration>(root+"hydrology.json");
 var overlay=Read<UniformParcelPack>("Builds/DenseModelSpike/Parcels/"+pack+".json");
 UniformParcels.Apply(world,hydro,overlay,pack);
 var rules=new BuildActionRules(world,Actions());rules.ExpandOffers(world,scenario);
 var runoff=new LandscapeRunoff(world,scenario,hydro);
 var definition=AnnualSimulationFactory.Create(FullAnnualGameplay.Create(world,scenario,pack,rules,runoff),weather,"coon-valley-demo-1",baselineWorld:world);
 Check(Math.Abs(definition.simulation.sites.Sum(s=>s.baselineCroppedFraction*definition.sites.Single(x=>x.id==s.siteId).areaM2)-world.sites.Sum(s=>s.analysisAreaM2*s.baselineCover.Where(c=>c.coverId=="cultivated").Sum(c=>(double)c.fraction)))<.01,"Numeric baseline overcounts cultivated land");
 var town=new AnnualTown(definition,"dense-spike/"+pack);
 var farm=definition.actions.First(a=>a.actionId=="cover-crop");town.QueueAction(farm.siteId,farm.actionId,town.Revision);
 var home=definition.actions.First(a=>a.actionId=="low-density-housing"&&a.siteId!=farm.siteId);town.QueueAction(home.siteId,home.actionId,town.Revision);
 var provider=new AuthoredAnnualConsequenceProvider();var times=new List<long>();var counts=new List<object>();
 for(int year=1;year<=10;year++)
 {
  if(town.Phase==TownPhase.Reviewing)town.ContinuePlanning(town.Revision);
  long start=clock.ElapsedMilliseconds;
  var report=AnnualTownDemo.Advance(town,town.RunId+"/y"+year,provider);
  times.Add(clock.ElapsedMilliseconds-start);
  Check(report.ledger.closingArrearsCents==0,"Unexpected debt in bounded dense fixture");
  counts.Add(new{year,impacts=report.simulation.impacts.Length,storage=report.simulation.storage.Length});
  Console.WriteLine(pack+" year"+year+" accepted in"+times.Last()+"ms");
 }
 long replayStart=clock.ElapsedMilliseconds;
 var snapshotJson=JsonSerializer.Serialize(town.Snapshot(),new JsonSerializerOptions(options){WriteIndented=false});
 var restored=AnnualTown.Restore(definition,JsonSerializer.Deserialize<AnnualTownSnapshot>(snapshotJson,options));
 Check(restored.CashCents==town.CashCents&&restored.History.Last().simulation.transitionHash==town.History.Last().simulation.transitionHash,"Dense replay differs");
 long replayMs=clock.ElapsedMilliseconds-replayStart;
 string projectionError=null;int facts=0;
 try{facts=AnnualReportProjection.Build(definition,town.History.Last(),town.History[^2]).facts.Length;}
 catch(ArgumentException e){projectionError=e.Message;}
 summaries.Add(new{pack,source="model continuation after9c0d156 + read-only dense sourceea37af0",sites=definition.sites.Length,offers=definition.actions.Length,definitionBytes=JsonSerializer.SerializeToUtf8Bytes(definition,options).Length,snapshotBytes=System.Text.Encoding.UTF8.GetByteCount(snapshotJson),times,replayMs,projectionError,facts,counts,totalMs=clock.ElapsedMilliseconds});
 File.WriteAllText("Builds/DenseModelSpike/results.json",JsonSerializer.Serialize(new{checks,summaries},options));
}
'''
(root/'Program.cs').write_text(prefix+program)
