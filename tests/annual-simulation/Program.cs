using System.Text.Json;
using Terra.Contracts;
using Terra.Core;

var options = new JsonSerializerOptions { IncludeFields = true };
T Read<T>(string text) => JsonSerializer.Deserialize<T>(text, options)!;
int count = 0;
void Check(bool ok, string why) { count++; if (!ok) throw new Exception(why); }
void Reject(Action action, string why) { bool rejected = false; try { action(); } catch (ArgumentException) { rejected = true; } Check(rejected, why); }
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
