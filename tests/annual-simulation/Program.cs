using System.Text.Json;
using Terra.Contracts;
using Terra.Core;

var options = new JsonSerializerOptions { IncludeFields = true };
T Read<T>(string text) => JsonSerializer.Deserialize<T>(text, options)!;
int count = 0;
void Check(bool ok, string why) { count++; if (!ok) throw new Exception(why); }
void Reject(Action action, string why) { bool rejected = false; try { action(); } catch (ArgumentException) { rejected = true; } catch (InvalidOperationException) { rejected = true; } Check(rejected, why); }
var root = Path.GetFullPath("simulation-data/weather/coon-valley-v1");
var pack = File.ReadAllBytes(Path.Combine(root,"weather-pack.json"));
var daily = File.ReadAllBytes(Path.Combine(root,"daily-observations.json"));
var weather = new AnnualSimulationWeather(pack,daily,Read<TownWeatherPack>,Read<TownDailyWeather>);
using var vectors = JsonDocument.Parse(File.ReadAllText("docs/evidence/weather-reference/schedule-vectors.json"));
foreach (var vector in vectors.RootElement.GetProperty("vectors").EnumerateArray())
{
    string seed = vector.GetProperty("sharedScenarioSeed").GetString()!;
    var schedule = weather.Select(seed);
    Check(schedule.startYear == vector.GetProperty("startYear").GetInt32(), "Cross-runtime schedule differs: " + seed);
    Check(schedule.recordIds.SequenceEqual(vector.GetProperty("recordIds").EnumerateArray().Select(x=>x.GetString())), "Record IDs differ: " + seed);
    weather.ValidateSchedule(schedule);
    schedule.recordIds[0] = "changed";
    Reject(()=>weather.ValidateSchedule(schedule), "Accepted changed saved schedule");
    Check(weather.Select(seed).recordIds[0] != "changed", "Caller mutated weather reference");
}
Reject(()=>weather.Select(""), "Empty seed accepted");
Reject(()=>weather.Select(new string('é',129)), "Seed UTF-8 limit ignored");
var changed = pack.ToArray(); changed[0] ^= 1;
Reject(()=>new AnnualSimulationWeather(changed,daily,Read<TownWeatherPack>,Read<TownDailyWeather>), "Changed pack accepted");
changed = daily.ToArray(); changed[0] ^= 1;
Reject(()=>new AnnualSimulationWeather(pack,changed,Read<TownWeatherPack>,Read<TownDailyWeather>), "Changed daily companion accepted");
var crops = new[] {
    new TownCropDefinition { id="corn",sowMonth=5,sowDay=1,harvestMonth=10,harvestDay=15 },
    new TownCropDefinition { id="soy",sowMonth=5,sowDay=10,harvestMonth=10,harvestDay=1 }
};
var saved = weather.Select("coon-valley-demo-1");
var observations = Read<TownDailyWeather>(System.Text.Encoding.UTF8.GetString(daily));
for (int year=1;year<=10;year++)
{
    var driver = weather.Driver(saved,year,crops);
    Check(driver.recordId == saved.recordIds[year-1] && driver.historicalYear == 2014+year, "Year-to-record mismatch");
    var days = observations.years.Single(x=>x.year==driver.historicalYear).days;
    foreach (var summary in driver.crops)
    {
        var season = days.Where(d=>d.dayOfYear>=summary.firstDay && d.dayOfYear<=summary.harvestDay).ToArray();
        Check(summary.precipTenthsMm == season.Sum(d=>d.precipTenthsMm), "Crop rain differs from exact calendar");
        Check(summary.hotDays == season.Count(d=>d.tmaxTenthsC>=350), "Crop heat differs from exact calendar");
        Check(summary.longestDrySpellDays>0 && summary.longestDrySpellDays<=season.Length, "Invalid crop dry spell");
    }
    Check(driver.solarFactor==1 && driver.windFactor==1 && driver.generationRuleId.Contains("no-irradiance"), "Unsupported renewable driver inferred");
    string original = JsonSerializer.Serialize(driver,options);
    driver.events[0].precipTenthsMm = -1;
    Check(JsonSerializer.Serialize(weather.Driver(saved,year,crops),options)==original, "Mutable driver escaped reference");
}
Console.WriteLine("Annual simulation weather checks passed: " + count);

foreach (string folder in new[]{"hud-connected","full-game"})
{
    var legacy=Read<AnnualTownDefinition>(File.ReadAllText("docs/evidence/"+folder+"/definition.json"));
    var history=Read<AnnualTownSnapshot>(File.ReadAllText("docs/evidence/"+folder+"/community-and-services.json"));
    var restored=AnnualTown.Restore(legacy,history);
    Check(restored.DefinitionHash==history.definitionHash,"Published legacy definition hash changed");
    Check(restored.CashCents==history.cashCents&&restored.History.Length==10,"Published legacy history changed");
    legacy.simulation=new TownSimulationDefinition();
    history.simulation=new TownSimulationState();
    foreach(var report in history.history) {report.simulation=new TownSimulationReport();report.acceptedRequest.simulation=new TownSimulationRequest();}
    Check(AnnualTown.Restore(legacy,history).DefinitionHash==restored.DefinitionHash,"Expanded empty Unity DTO invalidated legacy save");
}
Console.WriteLine("Weather and legacy compatibility checks passed: "+count);

var legacyRules=Read<AnnualTownDefinition>(File.ReadAllText("docs/evidence/hud-connected/definition.json"));
var numericRules=AnnualSimulationFactory.Create(legacyRules,weather,"coon-valley-demo-1");
var provider=new AuthoredAnnualConsequenceProvider();
var numeric=new AnnualTown(numericRules,"numeric-smoke");
Check(numeric.Snapshot().simulation.sites.Length==numericRules.sites.Length,"Baseline site state missing");
for(int y=1;y<=10;y++)
{
    if(numeric.Phase==TownPhase.Reviewing)numeric.ContinuePlanning(numeric.Revision);
    var report=AnnualTownDemo.Advance(numeric,"numeric-smoke/"+y,provider);
    AnnualSimulation.ValidateReportHash(report);
    Check(report.simulation.driver.recordId==saved.recordIds[y-1],"Numeric driver changed");
    Check(report.simulation.crops.All(c=>c.outputUnits>=0&&c.harvestCondition<=1),"Invalid realized crop output");
    Check(report.ledger.repairDueCents==0,"Private crop losses created public bills");
    var restored=AnnualTown.Restore(numericRules,Read<AnnualTownSnapshot>(JsonSerializer.Serialize(numeric.Snapshot(),options)));
    Check(JsonSerializer.Serialize(restored.Snapshot(),options)==JsonSerializer.Serialize(numeric.Snapshot(),options),"Numeric saved replay differs");
}
Check(numeric.History.Length==10&&numeric.Phase==TownPhase.Completed,"Numeric ending missing");
var changedReport=numeric.History[0];changedReport.simulation.economicDelta+=1;
Reject(()=>AnnualSimulation.ValidateReportHash(changedReport),"Altered accepted delta hash accepted");
var changedSave=numeric.Snapshot();changedSave.history[0].simulation.economicDelta+=1;
Reject(()=>AnnualTown.Restore(numericRules,changedSave),"Altered accepted report replay accepted");
Console.WriteLine("Numeric transition smoke checks passed: "+count);
if(args.Contains("--emit-driver-hashes"))
{
    var hashes=new SortedDictionary<int,string>();
    for(int i=0;i<1000&&hashes.Count<24;i++)
    {
        var schedule=weather.Select("driver-digest-fixture-"+i);
        for(int y=1;y<=10;y++)
        {
            var driver=weather.Driver(schedule,y,numericRules.simulation.crops);driver.year=0;
            hashes[driver.historicalYear]=TownCopies.Hash(driver);
        }
    }
    Check(hashes.Count==24,"Incomplete trusted driver digest generation");
    Directory.CreateDirectory("Builds/AnnualSimulation");
    File.WriteAllText("Builds/AnnualSimulation/driver-hashes.json",JsonSerializer.Serialize(hashes,options));
}

AnnualTownDefinition CopyRules()=>Read<AnnualTownDefinition>(JsonSerializer.Serialize(numericRules,options));
var tampered=CopyRules();tampered.simulation.drivers[0].crops[0].precipTenthsMm++;
Reject(()=>new AnnualTown(tampered,"bad-driver"),"In-range weather tampering accepted");
tampered=CopyRules();tampered.simulation.drivers[0].events=tampered.simulation.drivers[0].events.Skip(1).ToArray();
Reject(()=>new AnnualTown(tampered,"missing-event"),"Removed observed event accepted");
tampered=CopyRules();tampered.simulation.practices[0].actionId="invented";
Reject(()=>new AnnualTown(tampered,"unknown-practice"),"Unknown practice accepted");
tampered=CopyRules();tampered.simulation.sites[0].connectionGroupId="invented";
Reject(()=>new AnnualTown(tampered,"unknown-group"),"Unknown connection accepted");
tampered=CopyRules();tampered.simulation.publicResponsibilities=Array.Empty<TownPublicResponsibility>();
Reject(()=>new AnnualTown(tampered,"missing-duty"),"Missing public responsibility accepted");
var parkRules=CopyRules();var parkOffer=parkRules.actions.First(a=>a.actionId=="public-park");
var publicRun=new AnnualTown(parkRules,"public-repair-smoke");publicRun.QueueAction(parkOffer.siteId,parkOffer.actionId,0);
AnnualTownDemo.Finish(publicRun,provider);
Check(publicRun.History.Sum(r=>r.ledger.repairDueCents)>0,"Built public park never incurred declared repair");
foreach(var report in publicRun.History)
{
    var sim=report.simulation;var ledger=report.ledger;AnnualSimulation.ValidateReportHash(report);
    Check(sim.payments.Sum(p=>p.paidCents)==ledger.upfrontPaidCents+ledger.arrearsPaidCents+ledger.programPaidCents+ledger.operatingPaidCents+ledger.repairPaidCents,"Itemized payments differ");
    Check(sim.closingState.repairs.Sum(r=>r.unpaidCents)<=ledger.closingArrearsCents,"Repair liability exceeds total arrears");
    Check(sim.storage.All(s=>Math.Abs(s.openingStoredM3-s.recoveredM3-s.capacityLossReleaseM3+s.capturedM3-s.closingStoredM3)<.00001&&s.capturedM3<=s.inflowM3+.00001&&s.closingStoredM3<=s.capacityM3+.00001),"Storage conservation/capacity failed");
    Check(sim.crops.All(c=>c.outputUnits==c.productiveAreaM2/10000*c.benchmarkPerHa*c.weatherFactor*c.managementFactor*c.harvestCondition),"Harvest factor counted twice");
}
var publicRestored=AnnualTown.Restore(parkRules,publicRun.Snapshot());
Check(JsonSerializer.Serialize(publicRestored.Snapshot(),options)==JsonSerializer.Serialize(publicRun.Snapshot(),options),"Public repair full replay differs");
Console.WriteLine("Numeric weather/repair boundary checks passed: "+count);

var debtRules=CopyRules();debtRules.startingCashCents=7000000;debtRules.appropriationsCents=new long[10];debtRules.appropriationsCents[1]=10000000;
debtRules.simulation.publicResponsibilities.Single(r=>r.actionId=="public-park").repairCentsPerConditionUnit=1000000000;
var debtRun=new AnnualTown(debtRules,"repair-arrears");debtRun.QueueAction(parkOffer.siteId,parkOffer.actionId,0);AnnualTownDemo.Finish(debtRun,provider);
Check(debtRun.History.Any(r=>r.ledger.closingArrearsCents>0),"Repair shortage fixture incurred no debt");
Check(debtRun.History.SelectMany(r=>r.simulation.payments).Any(p=>p.kind=="prior-repair-arrears"&&p.paidCents>0&&p.paidCents<p.dueCents),"Fixture did not exercise partial prior repair");
foreach(var report in debtRun.History)
{
    var l=report.ledger;var s=report.simulation;
    Check(s.payments.Sum(p=>p.dueCents)==l.upfrontPaidCents+l.openingArrearsCents+l.programDueCents+l.operatingDueCents+l.repairDueCents,"Old repair billed twice");
    Check(l.closingCashCents==l.openingCashCents-l.upfrontPaidCents-l.arrearsPaidCents-l.programPaidCents-l.operatingPaidCents-l.repairPaidCents+l.appropriationCents+l.publicReceiptsCents,"Cash discontinuity");
    foreach(var site in s.closingState.sites)
    {
        double debtDamage=s.closingState.repairs.Where(r=>r.siteId==site.siteId).Sum(r=>r.unrecoveredCondition);
        Check(site.condition>=0&&site.condition<=1&&Math.Abs(site.outstandingDamage-(1-site.condition))<1e-10,"Condition authority disagrees");
        Check(debtDamage<=site.outstandingDamage+1e-10,"Unpaid public damage freely recovered");
    }
    foreach(var crop in s.crops)
    {
        double expected=crop.openingCondition-s.impacts.Where(i=>i.siteId==crop.siteId&&i.beforeHarvest).Sum(i=>i.conditionLoss);
        Check(Math.Abs(expected-crop.harvestCondition)<1e-10,"Post-harvest damage changed the harvest");
    }
}
Check(debtRun.History.Where(r=>r.year>2).Any(r=>r.outcomes.sites.Single(s=>s.siteId==parkOffer.siteId).phase==TownAssetPhase.Suspended),"Unpaid operations were retroactively funded by closing receipts");
var reloadDebt=AnnualTown.Restore(debtRules,debtRun.Snapshot());
Check(JsonSerializer.Serialize(reloadDebt.Snapshot(),options)==JsonSerializer.Serialize(debtRun.Snapshot(),options),"Debt/recovery replay differs");
var pendingRun=new AnnualTown(numericRules,"pending-numeric");pendingRun.BeginResolution("pending-numeric/y1",0);string pendingJson=JsonSerializer.Serialize(pendingRun.Snapshot(),options);
var badConsequence=provider.Resolve(pendingRun.PendingRequest);badConsequence.effects=new[]{new TownSiteEffect{siteId=numericRules.sites[0].id,reason="invented",sourceId=numericRules.id}};
Reject(()=>pendingRun.CompleteResolution(badConsequence),"External numeric effect accepted");Check(JsonSerializer.Serialize(pendingRun.Snapshot(),options)==pendingJson,"Rejected effect partially published year");
var pendingReload=AnnualTown.Restore(numericRules,Read<AnnualTownSnapshot>(pendingJson));
var committed=AnnualTownDemo.Advance(pendingReload,"pending-numeric/y1",provider);
string accepted=JsonSerializer.Serialize(pendingReload.Snapshot(),options);pendingReload.CompleteResolution(provider.Resolve(committed.acceptedRequest));
Check(JsonSerializer.Serialize(pendingReload.Snapshot(),options)==accepted,"Duplicate numeric acceptance spent twice");
Reject(()=>AnnualChapters.Preview(new AnnualChapterRequest{definition=numericRules,snapshot=Read<AnnualTownSnapshot>(pendingJson)}),"Legacy resolving narration accepted schema3");
Reject(()=>AnnualGameplay.Forecast(new AnnualTown(numericRules,"forecast")),"Exact unplayed numeric harvest exposed");
var invalidState=debtRun.Snapshot();invalidState.simulation.sites[0].condition=double.NaN;
Reject(()=>AnnualTown.Restore(debtRules,invalidState),"Nonfinite saved state accepted");
var otherWeather=AnnualSimulationFactory.Create(legacyRules,weather,"same-weather-comparison");
var otherRun=new AnnualTown(otherWeather,"other-weather");AnnualTownDemo.Finish(otherRun,provider);
Check(otherRun.History.Sum(r=>r.outcomes.productionUnits)!=numeric.History.Sum(r=>r.outcomes.productionUnits),"Different weather schedules cannot change output");
Console.WriteLine("Numeric replay/debt/calendar checks passed: "+count);

// Validate semantic structure even when a caller recomputes a digest over bad data.
var semanticRun=new AnnualTown(numericRules,"semantic-storage");var semanticSwale=numericRules.actions.First(a=>a.actionId=="water-swale");
semanticRun.QueueAction(semanticSwale.siteId,semanticSwale.actionId,0);AnnualTownDemo.Finish(semanticRun,provider);
foreach(var mutation in new Action<TownYearReport>[] {
 r=>r.simulation.crops[0].outputUnits+=1,
 r=>r.simulation.crops[0].harvestDay++,
 r=>r.simulation.storage[0].capturedM3+=1,
 r=>r.simulation.impacts[0].beforeHarvest=!r.simulation.impacts[0].beforeHarvest,
 r=>r.simulation.payments[0].paidCents++,
 r=>r.simulation.energyDeficit=double.NaN,
 r=>r.simulation.driver.events[0].precipTenthsMm++,
 r=>r.simulation.closingState.sites[0].outstandingDamage=.123
})
{
 var invalid=Read<TownYearReport>(JsonSerializer.Serialize(semanticRun.History[1],options));mutation(invalid);
 Reject(()=>AnnualSimulation.ValidateReport(parkRules,invalid),"Malformed numeric report accepted");
}
foreach(var mutation in new Action<TownSimulationState>[] {
 s=>s.sites=null!,s=>s.repairs=null!,s=>s.sites[1].siteId=s.sites[0].siteId,
 s=>s.sites[0].rotationPhase=99,s=>s.sites[0].cropId="invented",s=>s.sites[0].condition=double.PositiveInfinity,
 s=>s.repairs[0].unpaidCents=-1,s=>s.repairs[0].eventId="invented",s=>s.repairs[0].unrecoveredCondition=0,
 s=>s.repairs[0].createdYear=0
})
{
 var invalid=Read<AnnualTownSnapshot>(JsonSerializer.Serialize(debtRun.Snapshot(),options));mutation(invalid.simulation);
 Reject(()=>AnnualTown.Restore(debtRules,invalid),"Malformed numeric state accepted");
}
var previewRun=new AnnualTown(numericRules,"preview-envelope");
string previewBefore=JsonSerializer.Serialize(previewRun.Snapshot(),options);
var planning=AnnualSimulation.PlanningRange(previewRun);
Check(planning.sites.Length==numericRules.sites.Length&&planning.sites.All(s=>s.minimumProductionUnits==0&&s.maximumProductionUnits>=0),"Invalid conditional envelope");
Check(previewBefore==JsonSerializer.Serialize(previewRun.Snapshot(),options),"Preview mutated annual state");
var alternatePreview=AnnualSimulation.PlanningRange(new AnnualTown(otherWeather,"other-seed-preview"));
Check(JsonSerializer.Serialize(planning,options)==JsonSerializer.Serialize(alternatePreview,options),"Preview leaks future realized weather");
for(int y=1;y<=10;y++)
{
 if(previewRun.Phase==TownPhase.Reviewing)previewRun.ContinuePlanning(previewRun.Revision);
 var envelope=AnnualSimulation.PlanningRange(previewRun);
 var result=AnnualTownDemo.Advance(previewRun,"preview/y"+y,provider);
 Check(result.outcomes.sites.All(s=>s.productionUnits>=0&&s.productionUnits<=envelope.sites.Single(x=>x.siteId==s.siteId).maximumProductionUnits+1e-9),"Realized output escapes conditional envelope");
}
Console.WriteLine("Numeric semantic validation and conditional preview checks passed: "+count);

// Deliberately financed six-data-center stress on the six legal connected-map lots.
var industrialRules=CopyRules();industrialRules.startingCashCents=100000000000;
var industrial=new AnnualTown(industrialRules,"numeric-six-data-centers");
var centers=industrialRules.actions.Where(a=>a.actionId=="data-center").OrderBy(a=>a.siteId,StringComparer.Ordinal).Take(6).ToArray();
Check(centers.Length==6,"Six legal data-center sites unavailable in connected fixture");
foreach(var offer in centers)industrial.QueueAction(offer.siteId,offer.actionId,industrial.Revision);
var financedBaseline=new AnnualTown(industrialRules,"numeric-financed-no-action");
var industrialEvidence=new List<object>();
for(int y=1;y<=10;y++)
{
 if(industrial.Phase==TownPhase.Reviewing){industrial.ContinuePlanning(industrial.Revision);financedBaseline.ContinuePlanning(financedBaseline.Revision);}
 var envelope=AnnualSimulation.PlanningRange(industrial);
 var built=AnnualTownDemo.Advance(industrial,"industrial/y"+y,provider);var empty=AnnualTownDemo.Advance(financedBaseline,"baseline/y"+y,provider);
 Check(JsonSerializer.Serialize(built.simulation.driver,options)==JsonSerializer.Serialize(empty.simulation.driver,options),"Data centers changed external weather");
 Check(built.ledger.closingArrearsCents==0&&built.ledger.operatingPaidCents==built.ledger.operatingDueCents,"Financed fixture did not fund operation");
 Check(built.outcomes.sites.All(s=>s.productionUnits<=envelope.sites.Single(x=>x.siteId==s.siteId).maximumProductionUnits+1e-9),"Industrial preview underestimates feasible production");
 var e=built.outcomes.economy;var b=empty.outcomes.economy;
 if(y>=4)
 {
  Check(e.energyDemand>b.energyDemand&&e.waterDemand>b.waterDemand,"Six operating centers did not accumulate resource demand");
  Check(built.simulation.energyDeficit>=empty.simulation.energyDeficit&&built.simulation.waterDeficit>=empty.simulation.waterDeficit,"Raw deficits hidden by scores");
  Check(built.simulation.crops.Sum(c=>c.productiveAreaM2)<empty.simulation.crops.Sum(c=>c.productiveAreaM2),"Six buildings displaced no agricultural area");
 }
 Check(new[]{built.outcomes.economicScore,built.outcomes.socialScore,built.outcomes.environmentalScore}.All(s=>s>=0&&s<=100),"Stress score outside rubric");
 industrialEvidence.Add(new {year=y,centers=centers.Select(c=>c.siteId),economy=e,baseline=b,
  deltas=new[]{built.simulation.economicDelta,built.simulation.socialDelta,built.simulation.environmentalDelta},
  deficits=new[]{built.simulation.energyDeficit,built.simulation.waterDeficit,built.simulation.wastewaterDeficit,built.simulation.housingDeficit,built.simulation.jobsDeficit},
  built.ledger,agriculturalAreaM2=built.simulation.crops.Sum(c=>c.productiveAreaM2),baselineAreaM2=empty.simulation.crops.Sum(c=>c.productiveAreaM2)});
}
Check(JsonSerializer.Serialize(AnnualTown.Restore(industrialRules,industrial.Snapshot()).Snapshot(),options)==JsonSerializer.Serialize(industrial.Snapshot(),options),"Six-center ending replay differs");
// Pin sensitivity per run; do not randomly draw a favorable treatment effect each year.
var sensitivityEvidence=new List<object>();
foreach(string sensitivity in new[]{"low","central","high"})
foreach(string strategy in new[]{"baseline","cultivate-standard","cover-crop","conservation-rotation","cultivate-margin","water-swale"})
{
 var rules=AnnualSimulationFactory.Create(legacyRules,weather,"coon-valley-demo-1",sensitivity);
 var run=new AnnualTown(rules,"sensitivity/"+sensitivity+"/"+strategy);
 if(strategy!="baseline") {var offer=rules.actions.First(a=>a.actionId==strategy);run.QueueAction(offer.siteId,offer.actionId,run.Revision);}
 AnnualTownDemo.Finish(run,provider);
 Check(run.History.All(r=>r.simulation.driver.recordId==saved.recordIds[r.year-1]),"Sensitivity changed historical schedule");
 Check(run.History.All(r=>r.ledger.closingCashCents>=0&&r.simulation.crops.All(c=>c.outputUnits>=0)),"Sensitivity produced negative harvest/cash");
 Check(JsonSerializer.Serialize(AnnualTown.Restore(rules,run.Snapshot()).Snapshot(),options)==JsonSerializer.Serialize(run.Snapshot(),options),"Sensitivity replay differs");
 sensitivityEvidence.Add(new {sensitivity,strategy,production=run.History.Sum(r=>r.outcomes.productionUnits),
  loss=run.History.Sum(r=>r.simulation.impacts.Sum(i=>i.conditionLoss)),publicRepairCents=run.History.Sum(r=>r.ledger.repairDueCents),
  finalScores=new[]{run.History.Last().outcomes.economicScore,run.History.Last().outcomes.socialScore,run.History.Last().outcomes.environmentalScore}});
}
Directory.CreateDirectory("Builds/AnnualSimulation");
File.WriteAllText("Builds/AnnualSimulation/sensitivity.json",JsonSerializer.Serialize(sensitivityEvidence,new JsonSerializerOptions(options){WriteIndented=true}));
File.WriteAllText("Builds/AnnualSimulation/six-data-centers.json",JsonSerializer.Serialize(industrialEvidence,new JsonSerializerOptions(options){WriteIndented=true}));
Console.WriteLine("Numeric financed industrial and sensitivity checks passed: "+count);

// Zero-drain and full-drain bounds use verified historical events, not a production
// injection seam. Unfunded capacity must release carried water without protection.
var waterEvidence=new List<object>();
foreach(double drain in new[]{0d,1d})
{
 var rules=CopyRules();rules.simulation.dailyStorageRecoveryFraction=drain;
 rules.startingCashCents=13000000;rules.appropriationsCents=new long[10];
 rules.simulation.publicResponsibilities.Single(r=>r.actionId=="water-swale").repairCentsPerConditionUnit=1000000000;
 var swale=rules.actions.First(a=>a.actionId=="water-swale");
 var run=new AnnualTown(rules,"storage-boundary/"+drain);run.QueueAction(swale.siteId,swale.actionId,0);
 AnnualTownDemo.Finish(run,provider);
 string group=rules.simulation.sites.Single(s=>s.siteId==swale.siteId).connectionGroupId;
 foreach(var report in run.History)
 {
  AnnualSimulation.ValidateReport(rules,report);
  var rows=report.simulation.storage.Where(x=>x.connectionGroupId==group).ToArray();
  if(drain==0)Check(rows.All(x=>x.recoveredM3==0),"Zero drain magically recovered storage");
  int priorDay=0;
  foreach(var row in rows)
  {
   int day=report.simulation.driver.events.Single(e=>e.recordId==row.eventId).dayOfYear;
   double expectedDrain=Math.Min(row.openingStoredM3,row.capacityM3*drain*(day-priorDay));
   Check(Math.Abs(row.recoveredM3-expectedDrain)<1e-9,"Storage drainage differs from capacity-per-day rule");
   Check(Math.Abs(row.capacityLossReleaseM3-Math.Max(0,row.openingStoredM3-expectedDrain-row.capacityM3))<1e-9,"Capacity-loss release differs from declared policy");
   priorDay=day;
  }
  foreach(var other in report.simulation.storage.Where(x=>x.connectionGroupId!=group))Check(other.capturedM3==0,"Remote group reused the swale");
  var site=report.outcomes.sites.Single(s=>s.siteId==swale.siteId);
  if(site.phase==TownAssetPhase.Building||site.phase==TownAssetPhase.Suspended)
    Check(rows.All(x=>x.capacityM3==0&&x.capturedM3==0),"Building/unfunded swale protected a site");
  Check(report.simulation.impacts.Where(i=>i.connectionGroupId==group).All(i=>i.upstreamHazard==Math.Max(0,report.simulation.driver.events.Single(e=>e.recordId==i.eventId).precipTenthsMm/10d-rules.simulation.upstreamRainThresholdMm)*rules.simulation.upstreamDamagePerMm),"Storage suppressed upstream hazard");
 }
 Check(run.History.Any(r=>r.outcomes.sites.Single(s=>s.siteId==swale.siteId).phase==TownAssetPhase.Suspended),"Fixture never removed swale upkeep");
 if(drain==0)Check(run.History.SelectMany(r=>r.simulation.storage).Any(x=>x.capacityLossReleaseM3>0),"Disabled/capacity-lost stored water disappeared without release");
 Check(JsonSerializer.Serialize(AnnualTown.Restore(rules,run.Snapshot()).Snapshot(),options)==JsonSerializer.Serialize(run.Snapshot(),options),"Storage carryover changed on reload");
 waterEvidence.Add(new{drain,history=run.History.Select(r=>new{r.year,r.ledger,storage=r.simulation.storage.Where(x=>x.connectionGroupId==group),closing=r.simulation.closingState.sites.Single(s=>s.siteId==swale.siteId)})});
}
// A newly built asset must inherit the already damaged baseline site's condition.
var inherited=new AnnualTown(numericRules,"condition-inheritance");
TownActionDefinition farmOffer=null;double previousCondition=1;
while(inherited.Year<10&&farmOffer==null)
{
 AnnualTownDemo.Advance(inherited,"inherit/y"+inherited.Year,provider);inherited.ContinuePlanning(inherited.Revision);
 var state=inherited.Snapshot().simulation;
 farmOffer=numericRules.actions.FirstOrDefault(a=>a.actionId=="cultivate-standard"&&state.sites.Single(s=>s.siteId==a.siteId).condition<.99);
 if(farmOffer!=null)previousCondition=state.sites.Single(s=>s.siteId==farmOffer.siteId).condition;
}
Check(farmOffer!=null&&previousCondition<1,"Inheritance fixture has no baseline damage");
inherited.QueueAction(farmOffer!.siteId,farmOffer.actionId,inherited.Revision);
var inheritedReport=AnnualTownDemo.Advance(inherited,"inherit/y"+inherited.Year,provider);
Check(inheritedReport.simulation.crops.Single(c=>c.siteId==farmOffer.siteId).openingCondition==previousCondition,"New asset reset old site damage");
// Repeat fast-forward with a failing resolver at each year, then reload and retry.
var fullRun=new AnnualTown(numericRules,"retry-equality");AnnualTownDemo.Finish(fullRun,provider);
for(int stop=1;stop<=10;stop++)
{
 var run=new AnnualTown(numericRules,"retry-equality");
 string requestId=fullRun.History[stop-1].acceptedRequest.requestId;
 for(int year=1;year<stop;year++)
 {
  AnnualTownDemo.Advance(run,fullRun.History[year-1].acceptedRequest.requestId,provider);
  run.ContinuePlanning(run.Revision);
 }
 run.BeginResolution(requestId,run.Revision);
 var frozen=JsonSerializer.Serialize(run.PendingRequest,options);
 run=AnnualTown.Restore(numericRules,Read<AnnualTownSnapshot>(JsonSerializer.Serialize(run.Snapshot(),options)));
 Check(frozen==JsonSerializer.Serialize(run.PendingRequest,options),"Pending driver/request changed on reload");
 AnnualTownDemo.Finish(run,provider);
 Check(JsonSerializer.Serialize(run.Snapshot(),options)==JsonSerializer.Serialize(fullRun.Snapshot(),options),"Manual/pending/reload differs from fast-forward at year"+stop);
}
File.WriteAllText("Builds/AnnualSimulation/storage-boundaries.json",JsonSerializer.Serialize(waterEvidence,new JsonSerializerOptions(options){WriteIndented=true}));
Console.WriteLine("Numeric storage, baseline inheritance and pending retry checks passed: "+count);

// Cross-runtime fixtures are generated by .NET, then independently restored with
// Unity JsonUtility/Mono; no test driver injection or runtime activation.
foreach(var fixture in new[]{(name:"baseline",rules:numericRules,run:numeric),(name:"public-repair",rules:parkRules,run:publicRun),(name:"repair-debt",rules:debtRules,run:debtRun),(name:"six-centers",rules:industrialRules,run:industrial)})
{
 string directory=Path.Combine("Builds/AnnualSimulation/UnityVectors",fixture.name);Directory.CreateDirectory(directory);
 File.WriteAllText(Path.Combine(directory,"definition.json"),JsonSerializer.Serialize(fixture.rules,options));
 File.WriteAllText(Path.Combine(directory,"snapshot.json"),JsonSerializer.Serialize(fixture.run.Snapshot(),options));
}
var strategies=new List<object>();
foreach(var strategy in new[]{(name:"community",actions:new[]{"high-density-housing","conservation-rotation","public-park","village-center","solar","wastewater-upgrade","restore-grass"}),
 (name:"land",actions:new[]{"wind","agricultural-storage","conservation-rotation","conservation-rotation","restore-grass","high-density-housing","wastewater-upgrade","public-park"})})
{
 var town=new AnnualTown(numericRules,"numeric-strategy/"+strategy.name);var remaining=strategy.actions.ToList();
 for(int year=1;year<=10;year++)
 {
  if(town.Phase==TownPhase.Reviewing)town.ContinuePlanning(town.Revision);
  foreach(string id in remaining.ToArray())
  {
   var offer=numericRules.actions.Where(a=>a.actionId==id&&!town.Assets.Any(s=>s.siteId==a.siteId)&&!town.Queue.Any(s=>s.siteId==a.siteId))
    .OrderBy(a=>numericRules.actions.Count(b=>b.siteId==a.siteId)).FirstOrDefault(a=>town.Preview(a.siteId,id)>=0);
   if(offer==null)continue;
   town.QueueAction(offer.siteId,id,town.Revision);remaining.Remove(id);
  }
  var report=AnnualTownDemo.Advance(town,town.RunId+"/year-"+year,provider);
  Check(report.ledger.closingArrearsCents==0&&town.AvailableCents>=0,"Numeric strategy violated reserved affordability");
 }
 Check(remaining.Count==0,"Numeric strategy could not finish planned catalog choices: "+strategy.name);
 var ending=town.History.Last().outcomes;
 File.WriteAllText("Builds/AnnualSimulation/strategy-"+strategy.name+".json",JsonSerializer.Serialize(town.Snapshot(),options));
 strategies.Add(new {strategy.name,scores=new[]{ending.economicScore,ending.socialScore,ending.environmentalScore},town.CashCents,town.CommittedCents,history=town.History.Select(r=>new{r.year,r.acceptedRequest.actions,r.ledger})});
}
File.WriteAllText("Builds/AnnualSimulation/strategies.json",JsonSerializer.Serialize(strategies,new JsonSerializerOptions(options){WriteIndented=true}));
Console.WriteLine("Numeric complete strategies and runtime vectors passed: "+count);

var quietRules=CopyRules();quietRules.simulation.localRainDamagePerMm=0;quietRules.simulation.upstreamDamagePerMm=0;
var slightRules=Read<AnnualTownDefinition>(JsonSerializer.Serialize(quietRules,options));slightRules.simulation.localRainDamagePerMm=1e-10;
var quiet=new AnnualTown(quietRules,"quiet");var slight=new AnnualTown(slightRules,"slight");
var quietYear=AnnualTownDemo.Advance(quiet,"quiet/y1",provider);var slightYear=AnnualTownDemo.Advance(slight,"slight/y1",provider);
Check(slightYear.simulation.impacts.Length>0&&quietYear.simulation.impacts.Length==0,"Continuity fixture does not distinguish minor impact");
Check(Math.Abs(quietYear.outcomes.socialScore-slightYear.outcomes.socialScore)<.001,"Tiny impact applies whole-year binary social penalty");
Console.WriteLine("Numeric continuity severity checks passed: "+count);
if(args.Contains("--explore-balance"))
{
 var candidates=new[]{
 new[]{"village-center","wind","conservation-rotation","conservation-rotation","restore-grass","restore-grass","high-density-housing","wastewater-upgrade","public-park"},
 new[]{"high-density-housing","village-center","wind","cover-crop","cover-crop","cover-crop","restore-grass","restore-grass","wastewater-upgrade","public-park"},
 new[]{"wind","village-center","high-density-housing","wastewater-upgrade","public-park","restore-grass","restore-grass","restore-grass","cultivate-margin","cultivate-margin","cover-crop","cover-crop"},
 new[]{"high-density-housing","wind","agricultural-storage","village-center","wastewater-upgrade","public-park","cover-crop","cover-crop","restore-grass","restore-grass"},
 new[]{"solar","village-center","high-density-housing","wastewater-upgrade","public-park","restore-grass","restore-grass","restore-grass","conservation-rotation","conservation-rotation"}
 };
 var exploration=new List<object>();
 for(int c=0;c<candidates.Length;c++)
 {
  var town=new AnnualTown(numericRules,"adaptation-candidate-"+c);var remaining=candidates[c].ToList();
  for(int year=1;year<=10;year++)
  {
   if(town.Phase==TownPhase.Reviewing)town.ContinuePlanning(town.Revision);
   foreach(string id in remaining.ToArray())
   {
    var offer=numericRules.actions.Where(a=>a.actionId==id&&!town.Assets.Any(s=>s.siteId==a.siteId)&&!town.Queue.Any(s=>s.siteId==a.siteId))
     .OrderBy(a=>numericRules.actions.Count(b=>b.siteId==a.siteId)).FirstOrDefault(a=>town.Preview(a.siteId,id)>=0);
    if(offer==null)continue;
    town.QueueAction(offer.siteId,id,town.Revision);remaining.Remove(id);
   }
   AnnualTownDemo.Advance(town,town.RunId+"/year-"+year,provider);
  }
  var result=town.History.Last().outcomes;
  exploration.Add(new{candidate=c,remaining,scores=new[]{result.economicScore,result.socialScore,result.environmentalScore},town.CashCents,town.CommittedCents,town.AvailableCents,assets=town.Assets,economy=result.economy});
 }
 File.WriteAllText("Builds/AnnualSimulation/balance-exploration.json",JsonSerializer.Serialize(exploration,new JsonSerializerOptions(options){WriteIndented=true}));
}

var originalCulture=System.Globalization.CultureInfo.CurrentCulture;
try
{
 string expected=JsonSerializer.Serialize(fullRun.Snapshot(),options);
 foreach(string culture in new[]{"fr-FR","ar-SA","tr-TR"})
 {
  System.Globalization.CultureInfo.CurrentCulture=System.Globalization.CultureInfo.GetCultureInfo(culture);
  var run=new AnnualTown(numericRules,"retry-equality");AnnualTownDemo.Finish(run,provider);
  Check(JsonSerializer.Serialize(run.Snapshot(),options)==expected,"Numeric report/history depends on system locale: "+culture);
 }
}
finally{System.Globalization.CultureInfo.CurrentCulture=originalCulture;}
Console.WriteLine("Numeric cross-locale transition checks passed: "+count);

var sourceWorld=new WorldData{sites=legacyRules.sites.Select(s=>new Site{id=s.id,analysisAreaM2=(float)s.areaM2,baselineCover=new[]{new CoverFraction{coverId="cultivated",fraction=s.baselineProductionUnits>0?.25f:0},new CoverFraction{coverId="grass",fraction=s.baselineProductionUnits>0?.75f:1}}}).ToArray()};
var sourceRules=AnnualSimulationFactory.Create(legacyRules,weather,"source-cover",baselineWorld:sourceWorld);
Check(sourceRules.simulation.sites.All(s=>s.baselineCroppedFraction==(legacyRules.sites.Single(x=>x.id==s.siteId).baselineProductionUnits>0?.25:0)),"Source partial crop partition lost");
Check(sourceRules.sites.All(s=>s.baselineProductionUnits==s.areaM2/10000*4*sourceRules.simulation.sites.Single(x=>x.siteId==s.id).baselineCroppedFraction),"Baseline crop area benchmark differs");
var denseLegacy=Read<AnnualTownDefinition>(JsonSerializer.Serialize(legacyRules,options));denseLegacy.version="4.0.0";
Reject(()=>AnnualSimulationFactory.Create(denseLegacy,weather,"dense-missing-cover"),"Dense parcels accepted full-field inference");
sourceWorld.sites[0].baselineCover[1].coverId="cultivated";
Reject(()=>AnnualSimulationFactory.Create(legacyRules,weather,"duplicate-cover",baselineWorld:sourceWorld),"Duplicate source cover accepted");
Check(numeric.History.All(r=>r.simulation.storage.Length==0),"No-device groups bloated numeric reports");
Console.WriteLine("Numeric source-cover and sparse-storage checks passed: "+count);
