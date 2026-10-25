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
int checks=0;
void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
void Reject(Action action,string why){bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}catch(InvalidOperationException){rejected=true;}Check(rejected,why);}
var provider=new AuthoredAnnualConsequenceProvider();
AnnualTownDefinition Rules(string seed="repeatable",string region="meadowbrook-v1")=>AnnualPlayerRules.Create(d,region,seed);
TownYearReport End(AnnualTown t)=>AnnualTownDemo.Advance(t,t.RunId+"/"+t.Year,provider);
AnnualTown Reload(AnnualTown t)=>AnnualTown.Restore(t.Definition,JsonSerializer.Deserialize<AnnualTownSnapshot>(JsonSerializer.Serialize(t.Snapshot(),options),options));
var playerRules=Rules();
var freshGuidanceTown=new AnnualTown(playerRules,"fresh-guidance");
var initialGuidance=AnnualPlanningGuidance.Create(freshGuidanceTown,AnnualGameplay.Baseline(playerRules));
Check(!string.IsNullOrWhiteSpace(initialGuidance.message)&&initialGuidance.actionId!=AnnualPlayerRules.CancelAction,"fresh-run HUD guidance ignores cancellation offers on empty sites");
Check(freshGuidanceTown.Queue.Length==0&&freshGuidanceTown.History.Length==0,"guidance does not mutate opening state");

Check(TownCopies.Hash(playerRules)==TownCopies.Hash(Rules()),"same seed, identical definition and event schedule");
Check(TownCopies.Hash(playerRules.drivers)!=TownCopies.Hash(Rules("another").drivers),"another seed changes saved schedule");
foreach(var region in new[]{"meadowbrook-v1","point-reyes-v1","suusamyr-v1"})
{
 var regional=Rules("seed",region);
 Check(regional.playerRules.hazards.Length==3,"three hazards per region");
 Check(regional.playerRules.hazards.All(h=>h.evidence.Contains("Inferred game estimate")),"probabilities honestly inferred");
 var t=new AnnualTown(regional,region);AnnualTownDemo.Finish(t,provider);
 Check(Reload(t).History.Length==10,"regional ten-year restore");
 foreach(var year in t.History)
 {
  var l=year.ledger;
  Check(l.openingCashCents+l.capitalRefundCents+l.appropriationCents+l.publicReceiptsCents-l.upfrontPaidCents-l.programPaidCents-l.operatingPaidCents-l.repairPaidCents-l.arrearsPaidCents==l.closingCashCents,"ledger exact cents");
 }
}
// A three-year construction project: 1/3 built, 2/3 of upfront capital remains.
var cancellationRules=Rules();
var a=cancellationRules.actions.First(x=>x.actionId=="high-density-housing");
a.constructionYears=3;a.upfrontPublicCostCents=300000;a.annualPublicOperatingCents=1000;a.annualPublicPaymentCents=0;
var cancel=new AnnualTown(cancellationRules,"cancel");cancel.QueueAction(a.siteId,a.actionId,0);End(cancel);cancel.ContinuePlanning(cancel.Revision);
Check(cancel.CancellationRefund(a.siteId)==200000,"33 percent complete refunds 67 percent");
long beforeCash=cancel.CashCents;cancel.QueueCancellation(a.siteId,cancel.Revision);
Check(cancel.CashCents==beforeCash,"queueing cancellation does not mint cash");
cancel.UndoPlan(cancel.Revision);Check(cancel.Queue.Length==0,"cancel proposal undo");cancel.RedoPlan(cancel.Revision);
var cancelling=Reload(cancel);var removed=End(cancelling);
Check(removed.ledger.capitalRefundCents==200000,"refund posted once");
Check(removed.ledger.operatingDueCents==0&&removed.ledger.futureCommitmentsCents==0,"future upkeep released");
Check(removed.outcomes.sites.Single(s=>s.siteId==a.siteId).vegetatedAreaM2<=cancellationRules.sites.Single(s=>s.id==a.siteId).areaM2*.25,"cancelled ground not restored");
Check(AnnualTownSummary.Latest(cancelling).receivedCents==removed.ledger.appropriationCents+removed.ledger.publicReceiptsCents+200000,"refund included in cash summary");
Check(cancelling.CompleteResolution(removed.acceptedConsequences).ledger.closingCashCents==removed.ledger.closingCashCents,"duplicate callback no second refund");
cancelling=Reload(cancelling);cancelling.ContinuePlanning(cancelling.Revision);
Reject(()=>cancelling.QueueCancellation(a.siteId,cancelling.Revision),"already cancelled cannot refund again");

var reopenedRules=TownCopies.Copy(cancellationRules);reopenedRules.actions=reopenedRules.actions.Where(x=>x.siteId==a.siteId&&(x.actionId==a.actionId||x.actionId==AnnualPlayerRules.CancelAction)).ToArray();
var reopened=new AnnualTown(reopenedRules,"reopened-guidance");reopened.QueueAction(a.siteId,a.actionId,0);End(reopened);reopened.ContinuePlanning(reopened.Revision);reopened.QueueCancellation(a.siteId,reopened.Revision);End(reopened);reopened.ContinuePlanning(reopened.Revision);
var reopenedGuidance=AnnualPlanningGuidance.Create(reopened,reopened.History.Last().outcomes);
Check(reopenedGuidance.siteId==a.siteId&&reopenedGuidance.actionId==a.actionId,"cancelled site and removed capacity are available to guidance again");
cancelling.QueueAction(a.siteId,a.actionId,cancelling.Revision);End(cancelling);
Check(cancelling.Assets.Single(s=>s.siteId==a.siteId).cancelledYear==0,"rebuild replaces cleared marker next year");
var finished=new AnnualTown(cancellationRules,"completed");finished.QueueAction(a.siteId,a.actionId,0);for(int y=1;y<=4;y++){End(finished);finished.ContinuePlanning(finished.Revision);}
Check(finished.CancellationRefund(a.siteId)==0,"completed project returns zero capital");
// Exogenous expense exhausts reserved funds; subsequent upkeep remains charged as signed cash.
var debtRules=Rules();debtRules.appropriationsCents=Enumerable.Repeat(0L,10).ToArray();
var paid=debtRules.actions.First(x=>x.actionId=="low-density-housing");paid.annualPublicReceiptsCents=0;paid.constructionYears=0;paid.upfrontPublicCostCents=300000;paid.annualPublicOperatingCents=10000;paid.annualPublicPaymentCents=0;
debtRules.startingCashCents=500000;debtRules.drivers[0].unexpectedPublicRepairCents=1000000;debtRules.appropriationsCents[2]=3000000;
var debt=new AnnualTown(debtRules,"debt");debt.QueueAction(paid.siteId,paid.actionId,0);End(debt);debt.ContinuePlanning(debt.Revision);
Check(debt.CashCents<0&&debt.ArrearsCents==0&&debt.DebtCents==-debt.CashCents,"negative cash records debt once");
var debtGuidance=AnnualPlanningGuidance.Create(debt,debt.History.Last().outcomes);
Check(debtGuidance.actionId==null&&debtGuidance.message.Contains("negative"),"debt produces guidance rather than pricing blocked construction");

var free=debtRules.actions.First(x=>x.siteId!=paid.siteId&&x.actionId!=AnnualPlayerRules.CancelAction);
Reject(()=>debt.QueueAction(free.siteId,free.actionId,debt.Revision),"debt blocks new construction");
var debt2=End(debt);Check(debt2.ledger.operatingPaidCents==10000,"upkeep charged in debt");Check(debt2.ledger.closingCashCents<debt2.ledger.openingCashCents,"upkeep deepens debt");
debt=Reload(debt);debt.ContinuePlanning(debt.Revision);debt.QueueCancellation(paid.siteId,debt.Revision);End(debt);
Check(debt.CashCents>0&&!debt.BuildingBlockedByDebt,"new public funding repays debt");
debt.ContinuePlanning(debt.Revision);debt.QueueAction(paid.siteId,paid.actionId,debt.Revision);Check(debt.Queue.Length==1,"building resumes after repayment");
// Six occupied sites must not escape impact through bounded service needs.
var industryRules=Rules();industryRules.startingCashCents=100000000000;
var industry=new AnnualTown(industryRules,"industry");var dc=industryRules.actions.Where(x=>x.actionId=="data-center").Take(6).ToArray();Check(dc.Length==6,"six fitting data-center sites");
foreach(var project in dc)industry.QueueAction(project.siteId,project.actionId,industry.Revision);
var industrial=End(industry);Check(industrial.outcomes.environmentalScore<0,"six data centers make environment negative");
Check(industrial.outcomes.economy.resourceFraction==1,"no additional utility rationing rule");
Check(Reload(industry).History[0].outcomes.environmentalScore==industrial.outcomes.environmentalScore,"signed environment saved exactly");
var priorCulture=System.Globalization.CultureInfo.CurrentCulture;
try{System.Globalization.CultureInfo.CurrentCulture=System.Globalization.CultureInfo.GetCultureInfo("fr-FR");Check(TownCopies.Hash(Reload(industry).Snapshot())==TownCopies.Hash(industry.Snapshot()),"rule replay survives browser locale change");}finally{System.Globalization.CultureInfo.CurrentCulture=priorCulture;}
var narration=AnnualChapters.OfflineAccepted(industry);Check(narration.mode=="offline"&&narration.chapter.cards.Length==3,"local chapter uses accepted result");
var previous=TownCopies.Hash(industry.Snapshot());AnnualChapters.Facts(industry);Check(TownCopies.Hash(industry.Snapshot())==previous,"explanations cannot change state");
// Engine owns hazard effects, not the narrator or client provider.
var frozen=new AnnualTown(playerRules,"effects");var request=frozen.BeginResolution("one",0);var effects=provider.Resolve(request);effects.effects=new[]{new TownSiteEffect{siteId=playerRules.sites[0].id,sourceId=request.driver.id,reason="invented",productionFactor=2}};
Reject(()=>frozen.CompleteResolution(effects),"invented effects rejected");
var eventual=AnnualTownDemo.Advance(frozen,"one",provider);Check(Reload(frozen).CashCents==frozen.CashCents,"frozen event replay saved");
// Event damage persists, repair costs charge once, and previews do not reveal future rolls.
var damageRules=Rules();foreach(var driver in damageRules.drivers)driver.regionalEvents=Array.Empty<TownRegionalEvent>();
var farm=damageRules.actions.First(x=>x.actionId=="cultivate-standard");var flood=damageRules.playerRules.hazards.First(h=>h.id=="flood");
damageRules.drivers[1].regionalEvents=new[]{new TownRegionalEvent{id="meadowbrook-v1/2/flood",title=flood.title,evidence=flood.evidence,outputFactor=flood.outputFactor,conditionLoss=flood.conditionLoss,affectedSiteIds=new[]{farm.siteId}}};
var damaged=new AnnualTown(damageRules,"damage");damaged.QueueAction(farm.siteId,farm.actionId,0);End(damaged);damaged.ContinuePlanning(damaged.Revision);
string damageBefore=TownCopies.Hash(damaged.Snapshot());var estimate=AnnualGameplay.Forecast(damaged);
Check(estimate.acceptedRequest.driver.regionalEvents.Length==0,"planning estimate hides future event");Check(TownCopies.Hash(damaged.Snapshot())==damageBefore,"planning estimate cannot mutate saved events");
var damagedYear=End(damaged);var damagedRow=damagedYear.outcomes.sites.Single(r=>r.siteId==farm.siteId);
Check(damagedYear.ledger.repairDueCents>0&&damagedYear.ledger.repairDueCents<=farm.upfrontPublicCostCents/10,"bounded authored damage creates a real repair bill");
Check(damagedRow.condition<1&&damagedRow.disrupted,"event changes affected asset state");
damaged=Reload(damaged);damaged.ContinuePlanning(damaged.Revision);var recovery=End(damaged);
Check(recovery.ledger.repairDueCents==0,"same damage is not charged twice");Check(recovery.outcomes.sites.Single(r=>r.siteId==farm.siteId).condition==damagedRow.condition,"condition persists without magical repair");
var malformed=TownCopies.Copy(damageRules);malformed.drivers[1].regionalEvents[0].affectedSiteIds=new[]{"invented"};Reject(()=>new AnnualTown(malformed,"bad"),"unknown exposure rejected");
var foreign=TownCopies.Copy(damageRules);foreign.playerRules.regionId="other";Reject(()=>new AnnualTown(foreign,"bad-region"),"unsupported region rejected");
var historyTown=new AnnualTown(Rules(),"history");AnnualTownDemo.Finish(historyTown,provider);Check(AnnualChapters.Facts(historyTown).First(f=>f.id=="year").text.Contains("Y1 choices")&&AnnualChapters.Facts(historyTown).First(f=>f.id=="year").text.Contains("Y10 choices"),"Year 10 receives the entire annual history");
AnnualChapters.ValidateText(AnnualChapters.OfflineAccepted(historyTown).chapter,AnnualChapters.Facts(historyTown));Check(true,"ending local chapter validates");
Directory.CreateDirectory("docs/evidence/annual-player-rules");
File.WriteAllText("docs/evidence/annual-player-rules/checks.json",JsonSerializer.Serialize(new{checks,industry=industrial.outcomes.environmentalScore,refund=removed.ledger.capitalRefundCents,debt=debt2.ledger.closingCashCents,profiles=playerRules.playerRules.hazards,scope="Pure engine checks; no rendered UI acceptance"},options));
Console.WriteLine("Player rules checks passed: "+checks);

if(args.Contains("--export-llm-fixtures"))
{
 var path="Builds/LocalLlmReadiness";Directory.CreateDirectory(path);
 var demo=new AnnualTown(Rules("local-readiness"),"local-readiness");var first=d.actions.First(a=>a.actionId=="low-density-housing");demo.QueueAction(first.siteId,first.actionId,0);End(demo);
 File.WriteAllText(path+"/year1-request.json",JsonSerializer.Serialize(AnnualChapters.AcceptedRequest(demo),options));
 AnnualTownDemo.Finish(demo,provider);File.WriteAllText(path+"/year10-request.json",JsonSerializer.Serialize(AnnualChapters.AcceptedRequest(demo),options));
}
