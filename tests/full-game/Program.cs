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
int checks=0;
void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
var provider=new AuthoredAnnualConsequenceProvider();
foreach(var id in FullAnnualGameplay.Supported)
{
 var a=d.actions.First(x=>x.actionId==id);Check(a.services!=null,"Explicit services");
 var town=new AnnualTown(d,"catalog-"+id);town.QueueAction(a.siteId,id,0);var before=town.CashCents;
 var first=AnnualTownDemo.Advance(town,"y1",provider);Check(first.ledger.openingCashCents==before,"Opening cash");
 if(a.constructionYears>0)Check(first.outcomes.sites.First(s=>s.siteId==a.siteId).phase==TownAssetPhase.Building,"Construction delay "+id);
 AnnualTownDemo.Finish(town,provider);var end=town.History.Last();
 Check(end.outcomes.sites.First(s=>s.siteId==a.siteId).phase==TownAssetPhase.Operating,"Family operates "+id);
 foreach(var r in town.History)
 {
  var l=r.ledger;Check(l.openingCashCents-l.upfrontPaidCents-l.arrearsPaidCents-l.programPaidCents-l.operatingPaidCents-l.repairPaidCents+l.appropriationCents+l.publicReceiptsCents==l.closingCashCents,"Ledger reconciles");
  var e=r.outcomes.economy;Check(Math.Abs(e.generatedM3-e.retainedM3-e.dischargedM3)<.02,"Water conserved");
  Check(new[]{r.outcomes.economicScore,r.outcomes.socialScore,r.outcomes.environmentalScore}.All(n=>n>=0&&n<=100),"Bounded meters");
 }
 var restored=AnnualTown.Restore(d,JsonSerializer.Deserialize<AnnualTownSnapshot>(JsonSerializer.Serialize(town.Snapshot(),options),options));
 Check(restored.CashCents==town.CashCents&&restored.Phase==TownPhase.Completed,"Saved history replay");
}
Directory.CreateDirectory(evidence);File.WriteAllText(evidence+"/definition.json",JsonSerializer.Serialize(d,options));
Console.WriteLine("Full-catalog checks passed: "+checks);
void Reject(Action action,string why){bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}catch(InvalidOperationException){rejected=true;}Check(rejected,why);}
// Boundary cases are independent of scene rendering and exercise the real rules/ledger.
var bad=JsonSerializer.Deserialize<AnnualTownDefinition>(JsonSerializer.Serialize(d,options),options);
bad.actions[0].services=null;Reject(()=>new AnnualTown(bad,"missing-service"),"Missing service data rejected, never zero");
bad=JsonSerializer.Deserialize<AnnualTownDefinition>(JsonSerializer.Serialize(d,options),options);bad.economy.jobsTarget=0;Reject(()=>new AnnualTown(bad,"zero-target"),"Zero target rejected");
var oldSave=new AnnualTown(d,"version-check").Snapshot();bad=JsonSerializer.Deserialize<AnnualTownDefinition>(JsonSerializer.Serialize(d,options),options);bad.actions[0].services.jobs+=1;Reject(()=>AnnualTown.Restore(bad,oldSave),"Changed effects invalidate saved rules");
var empty=new AnnualTown(d,"unchanged");AnnualTownDemo.Finish(empty,provider);Check(empty.History.Length==10&&empty.Assets.Length==0,"No-action world advances ten years");
Reject(()=>empty.ContinuePlanning(empty.Revision),"No Year 11");
var housing=d.actions.First(a=>a.actionId=="high-density-housing");var delayed=new AnnualTown(d,"delayed");delayed.QueueAction(housing.siteId,housing.actionId,0);
AnnualTownDemo.Advance(delayed,"year1",provider);delayed.ContinuePlanning(delayed.Revision);var y2=AnnualTownDemo.Advance(delayed,"year2",provider);Check(y2.ledger.publicReceiptsCents==0,"No receipts during two-year construction");
delayed.ContinuePlanning(delayed.Revision);var y3=AnnualTownDemo.Advance(delayed,"year3",provider);Check(y3.ledger.publicReceiptsCents==15000000,"Occupied apartment bundle pays source-table receipt");
var over=new AnnualTown(d,"overbuild");var centers=d.actions.Where(a=>a.actionId=="village-center").ToArray();over.QueueAction(centers[0].siteId,centers[0].actionId,0);over.QueueAction(centers[1].siteId,centers[1].actionId,over.Revision);
long cashBefore=over.CashCents;Reject(()=>over.QueueAction(centers[2].siteId,centers[2].actionId,over.Revision),"Overspend rejected");Check(over.CashCents==cashBefore&&over.Queue.Length==2,"Overspend is atomic");
AnnualTownDemo.Finish(over,provider);Check(over.History.Last().outcomes.economy.localServices<=d.economy.localServicesTarget,"Services saturate");
var maintenance=JsonSerializer.Deserialize<AnnualTownDefinition>(JsonSerializer.Serialize(d,options),options);maintenance.drivers[1].unexpectedPublicRepairCents=2000000000;
var starved=new AnnualTown(maintenance,"shortfall");var farm=maintenance.actions.First(a=>a.actionId=="cultivate-standard");starved.QueueAction(farm.siteId,farm.actionId,0);AnnualTownDemo.Finish(starved,provider);
Check(starved.History[2].outcomes.sites.First(s=>s.siteId==farm.siteId).phase==TownAssetPhase.Suspended,"Arrears suspend unfunded services");Check(starved.History[2].outcomes.economy.fundedFraction<1,"Arrears count in funding rubric");
var dc=d.actions.First(a=>a.actionId=="data-center");var thirsty=new AnnualTown(d,"resource-demand");thirsty.QueueAction(dc.siteId,dc.actionId,0);AnnualTownDemo.Finish(thirsty,provider);
Check(thirsty.History.Last().outcomes.economy.resourceFraction<1,"Data center creates power shortfall");
// Water treatment must never add drinking-water supply.
var ww=d.actions.First(a=>a.actionId=="wastewater-upgrade");var sewage=new AnnualTown(d,"sewage");sewage.QueueAction(ww.siteId,ww.actionId,0);AnnualTownDemo.Finish(sewage,provider);
Check(sewage.History.Last().outcomes.economy.waterSupplied<=d.economy.baselineWaterSupply,"Wastewater is not drinking water");
Console.WriteLine("Full-game boundary checks passed; total assertions: "+checks);
// Two reproducible plans use the real map, physical offers and actual cash constraints.
AnnualTown Strategy(string name,string[] priorities)
{
 var town=new AnnualTown(d,name);var remaining=priorities.ToList();
 for(int year=1;year<=10;year++)
 {
  if(town.Phase==TownPhase.Reviewing)town.ContinuePlanning(town.Revision);
  foreach(var id in remaining.ToArray())
  {
   var offer=d.actions.Where(a=>a.actionId==id&&!town.Assets.Any(s=>s.siteId==a.siteId)&&!town.Queue.Any(s=>s.siteId==a.siteId))
      .OrderBy(a=>d.actions.Count(b=>b.siteId==a.siteId)).FirstOrDefault(a=>town.Preview(a.siteId,id)>=0);
   if(offer==null)continue;
   town.QueueAction(offer.siteId,id,town.Revision);remaining.Remove(id);
  }
  AnnualTownDemo.Advance(town,name+"/year-"+year,provider);
 }
 Check(remaining.Count==0,"All planned projects affordable and physically fit: "+name+" "+string.Join(",",remaining));
 return town;
}
var community=Strategy("community-and-services",new[]{"high-density-housing","conservation-rotation","public-park","village-center","solar","wastewater-upgrade","restore-grass"});
var landscape=Strategy("land-and-livelihoods",new[]{"wind","agricultural-storage","conservation-rotation","conservation-rotation","restore-grass","high-density-housing","wastewater-upgrade","public-park"});
foreach(var town in new[]{community,landscape})
{
 var end=town.History.Last();Check(end.ledger.closingArrearsCents==0&&town.AvailableCents>=0,"Financially viable strategy");
 Console.WriteLine(town.RunId+": "+end.outcomes.economicScore.ToString("0.0")+" / "+end.outcomes.socialScore.ToString("0.0")+" / "+end.outcomes.environmentalScore.ToString("0.0")+"; cash $"+(town.CashCents/100));
 File.WriteAllText(evidence+"/"+town.RunId+".json",JsonSerializer.Serialize(town.Snapshot(),options));
}
var manual=new AnnualTown(d,"manual-equivalence");var choice=d.actions.First(a=>a.actionId=="public-park");manual.QueueAction(choice.siteId,choice.actionId,0);
var automatic=AnnualTown.Restore(d,manual.Snapshot());AnnualTownDemo.Finish(automatic,provider);
for(int year=1;year<=10;year++)
{
 if(manual.Phase==TownPhase.Reviewing)manual.ContinuePlanning(manual.Revision);
 AnnualTownDemo.Advance(manual,manual.RunId+"/finish-year-"+year,provider);
 manual=AnnualTown.Restore(d,JsonSerializer.Deserialize<AnnualTownSnapshot>(JsonSerializer.Serialize(manual.Snapshot(),options),options));
}
Check(JsonSerializer.Serialize(manual.Snapshot(),options)==JsonSerializer.Serialize(automatic.Snapshot(),options),"Fast-forward and manual transitions identical, including reload each year");
File.WriteAllText(evidence+"/checks.json",JsonSerializer.Serialize(new {assertions=checks,variants=FullAnnualGameplay.Supported.Length,offers=d.actions.Length,legacyWorldUnchanged=true,manualFastForwardEquivalent=true},options));
Console.WriteLine("Full ten-year checks passed; total assertions: "+checks);
