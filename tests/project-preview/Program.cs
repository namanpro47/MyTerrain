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
if(args.Contains("--dense")) UniformParcels.Apply(world,hydro,JsonSerializer.Deserialize<UniformParcelPack>(File.ReadAllText("Assets/Terra/Resources/Parcels/meadowbrook-v1.json"),options),"meadowbrook-v1");
else AnnualSpatialOpportunities.AddWind(world,hydro);
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

int checks=0;
void Check(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
var rulesV5=AnnualPlayerRules.Create(d,"meadowbrook-v1","project-preview");
foreach(var driver in rulesV5.drivers){driver.regionalEvents=Array.Empty<TownRegionalEvent>();driver.productionFactor=1;}
var provider=new AuthoredAnnualConsequenceProvider();
TownYearReport End(AnnualTown t){return AnnualTownDemo.Advance(t,t.RunId+"/y"+t.Year,provider);}
TownYearOutcomes RealOutcome(AnnualTownSnapshot state,TownPlannedAction[] plan,int year)
{
 var t=AnnualTown.Restore(rulesV5,state);
 foreach(var q in t.Queue)t.RemoveQueuedAction(q.siteId,t.Revision);
 foreach(var q in plan)t.QueueAction(q.siteId,q.actionId,t.Revision);
 TownYearReport r=null;
 while(t.Year<=year){r=End(t);if(t.Year==year)break;t.ContinuePlanning(t.Revision);}
 return r.outcomes;
}
void Compare(AnnualTown t,TownActionDefinition a)
{
 var state=t.Snapshot();string saved=TownCopies.Hash(state),rulesHash=TownCopies.Hash(rulesV5);
 var p=AnnualProjectPreview.Create(rulesV5,state,a);Check(p.available,"No estimate: "+a.actionId);
 var same=state.queue.FirstOrDefault(q=>q.siteId==a.siteId)?.actionId==a.actionId;
 var before=state.queue.Where(q=>!same||q.siteId!=a.siteId).ToArray();
 var after=state.queue.Where(q=>q.siteId!=a.siteId).Concat(new[]{new TownPlannedAction{siteId=a.siteId,actionId=a.actionId}}).ToArray();
 var b=RealOutcome(state,before,p.year);var c=RealOutcome(state,after,p.year);
 Check(Math.Abs(p.economic-(c.economicScore-b.economicScore))<.000001,"Economic mismatch: "+a.actionId);
 Check(Math.Abs(p.social-(c.socialScore-b.socialScore))<.000001,"Social mismatch: "+a.actionId);
 Check(Math.Abs(p.environmental-(c.environmentalScore-b.environmentalScore))<.000001,"Environment mismatch: "+a.actionId);
 Check(saved==TownCopies.Hash(state)&&saved==TownCopies.Hash(t.Snapshot())&&rulesHash==TownCopies.Hash(rulesV5),"Preview mutated input");
}
foreach(var id in FullAnnualGameplay.Supported)
{
 var a=rulesV5.actions.First(x=>x.actionId==id);var t=new AnnualTown(rulesV5,"preview-"+id);
 Compare(t,a);t.QueueAction(a.siteId,id,t.Revision);Compare(t,a);
}
var park=rulesV5.actions.First(a=>a.actionId=="public-park");
var replacement=rulesV5.actions.First(a=>a.siteId==park.siteId&&a.actionId=="low-density-housing");
var replacing=new AnnualTown(rulesV5,"replace");replacing.QueueAction(park.siteId,park.actionId,0);Compare(replacing,replacement);
Check(AnnualProjectPreview.Create(rulesV5,replacing.Snapshot(),replacement).comparison=="vs queued project","Replacement baseline");
replacing.UndoPlan(replacing.Revision);Compare(replacing,replacement);
var data=rulesV5.actions.First(a=>a.actionId=="data-center");var late=new AnnualTown(rulesV5,"late");
while(late.Year<10){End(late);late.ContinuePlanning(late.Revision);}
var lp=AnnualProjectPreview.Create(rulesV5,late.Snapshot(),data);
Check(lp.year==10&&lp.openingYear==12&&lp.beyondRun,"Late lifecycle/horizon");
Check(lp.environmental<0,"Late industry conceals construction pressure");Compare(late,data);
var built=new AnnualTown(rulesV5,"progress");built.QueueAction(data.siteId,data.actionId,0);End(built);
Check(AnnualProjectPreview.CompletedConstruction(data,built.Assets[0],built.Snapshot())==1,"Year 1 construction progress");
built.ContinuePlanning(built.Revision);Check(AnnualProjectPreview.CompletedConstruction(data,built.Assets[0],built.Snapshot())==1,"Planning advanced construction");
End(built);Check(AnnualProjectPreview.CompletedConstruction(data,built.Assets[0],built.Snapshot())==2,"Year 2 construction progress");
Check(built.History.Last().outcomes.sites.First(s=>s.siteId==data.siteId).maturity==0,"Construction confused with establishment");
built.ContinuePlanning(built.Revision);
Check(built.CancellationRefund(data.siteId)==0,"Completed progress/refund disagree");
Check(!AnnualProjectPreview.Create(rulesV5,built.Snapshot(),data).available,"Accepted comparison should be unavailable");
var cancelled=new AnnualTown(rulesV5,"cancel");cancelled.QueueAction(park.siteId,park.actionId,0);End(cancelled);cancelled.ContinuePlanning(cancelled.Revision);cancelled.QueueCancellation(park.siteId,cancelled.Revision);
var other=rulesV5.actions.First(a=>a.actionId=="cover-crop"&&a.siteId!=park.siteId);Compare(cancelled,other);
End(cancelled);cancelled.ContinuePlanning(cancelled.Revision);Compare(cancelled,replacement);
var saturated=TownCopies.Copy(rulesV5);saturated.economy.baselineRecreation=saturated.economy.recreationTarget;
var sat=new AnnualTown(saturated,"saturated");var gain=AnnualProjectPreview.Create(saturated,sat.Snapshot(),park);
Check(gain.social<=.000001,"Saturated recreation promised social gain");
var growth=new AnnualTown(rulesV5,"growing");var grass=rulesV5.actions.First(a=>a.actionId=="restore-grass");growth.QueueAction(grass.siteId,grass.actionId,0);End(growth);growth.ContinuePlanning(growth.Revision);
Compare(growth,rulesV5.actions.First(a=>a.actionId=="public-park"&&a.siteId!=grass.siteId));
var legacy=new AnnualTown(d,"legacy");Check(AnnualProjectPreview.Create(d,legacy.Snapshot(),d.actions.First(a=>a.actionId=="data-center")).available,"Legacy rules unsupported");
var benchmarkState=new AnnualTown(rulesV5,"timing").Snapshot();
var sw=System.Diagnostics.Stopwatch.StartNew();for(int i=0;i<100;i++)AnnualProjectPreview.Create(rulesV5,benchmarkState,park);sw.Stop();
Console.WriteLine("Project preview checks passed: "+checks+"; uncached comparison mean "+(sw.Elapsed.TotalMilliseconds/100).ToString("0.0")+" ms");



Directory.CreateDirectory(evidence);File.WriteAllText(Path.Combine(evidence,"project-preview-checks.json"),JsonSerializer.Serialize(new{checks,fields=rulesV5.sites.Length,uncachedComparisonMeanMs=sw.Elapsed.TotalMilliseconds/100},options));
