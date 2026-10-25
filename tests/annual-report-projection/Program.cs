using System.Globalization;
using System.Text.Json;
using Terra.Contracts;
using Terra.Core;

var json = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
T Read<T>(string text) => JsonSerializer.Deserialize<T>(text, json)!;
T Copy<T>(T value) => Read<T>(JsonSerializer.Serialize(value, json));
int checks = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
void Reject(Action action, string message) { bool rejected = false; try { action(); } catch (ArgumentException) { rejected = true; } Check(rejected, message); }
var weatherRoot = "simulation-data/weather/coon-valley-v1/";
var weather = new AnnualSimulationWeather(File.ReadAllBytes(weatherRoot + "weather-pack.json"), File.ReadAllBytes(weatherRoot + "daily-observations.json"), Read<TownWeatherPack>, Read<TownDailyWeather>);
var legacy = Read<AnnualTownDefinition>(File.ReadAllText("docs/evidence/hud-connected/definition.json"));
var rules = AnnualSimulationFactory.Create(legacy, weather, "coon-valley-demo-1");
var provider = new AuthoredAnnualConsequenceProvider();
var idle = new AnnualTown(rules, "projection-baseline");
AnnualTownDemo.Finish(idle, provider);
var publicRun = new AnnualTown(rules, "projection-public");
var park = rules.actions.First(a => a.actionId == "public-park");
publicRun.QueueAction(park.siteId, park.actionId, publicRun.Revision);
AnnualTownDemo.Finish(publicRun, provider);
Check(publicRun.History.Any(r => r.ledger.repairDueCents > 0), "Public scenario exercises repair facts");
var cropRun = new AnnualTown(rules, "projection-crops");
foreach (string action in new[] { "conservation-rotation", "cover-crop", "cultivate-margin" })
{
    var offer = rules.actions.First(a => a.actionId == action && !cropRun.Queue.Any(q => q.siteId == a.siteId));
    cropRun.QueueAction(offer.siteId, offer.actionId, cropRun.Revision);
}
AnnualTownDemo.Finish(cropRun, provider);
Check(cropRun.History.SelectMany(r => r.simulation.crops).Where(c => c.practiceId == "conservation-rotation").Select(c => c.cropId).Distinct().Count() == 2, "Practice scenario exercises both rotation crops");
var outputs = new List<ReportNarrationRequest>();
foreach (var town in new[] { idle, publicRun, cropRun })
{
    string snapshot = JsonSerializer.Serialize(town.Snapshot(), json);
    string definition = JsonSerializer.Serialize(rules, json);
    var restored = AnnualTown.Restore(rules, Copy(town.Snapshot()));
    var history = town.History;
    for (int i = 0; i < history.Length; i++)
    {
        var report = history[i]; var previous = i == 0 ? null : history[i - 1];
        var request = AnnualReportProjection.Build(rules, report, previous);
        outputs.Add(request);
        ReportNarration.ValidateRequest(request);
        Check(request.year == i + 1 && request.acceptedTransitionHash == report.simulation.transitionHash, "Projection binds exact accepted year/hash");
        Check(request.introduction.Contains("Economic") && request.introduction.Contains("Social") && request.introduction.Contains("Environmental"), "All three headline scores remain canonical");
        Check(request.facts.First().id == "budget" && request.facts.Any(f => f.id == "services"), "Deterministic budget and need facts");
        Check(request.facts.Single(f => f.id == "budget").text.Contains("$" + (report.ledger.closingCashCents / 100m).ToString("N2", CultureInfo.InvariantCulture)), "Closing cash is exact to the cent");
        Check(request.facts.SelectMany(f => f.siteIds).All(id => rules.sites.Any(s => s.id == id)), "No invented site links");
        Check(request.facts.Length <= 64 && request.facts.All(f => f.causalRuleIds.Length <= 8 && f.siteIds.Length <= 8), "Bounded complete projection");
        Check(request.facts.Any(f => f.text == AnnualSimulationWeather.Disclosure), "Wisconsin shared-reference disclosure retained");
        if (report.year == 10) Check(!request.facts.Any(f => f.text.Contains("carries into the next year") || f.text.Contains("cash carries forward")), "Ending facts do not imply Year 11");
        Check(request.facts.Count(f => f.id.StartsWith("harvest:")) == report.simulation.crops.Length, "Every recorded crop has a fact");
        Check(!request.facts.Where(f => f.id.StartsWith("harvest:")).Any(f => f.text.Contains("conservation-rotation") || f.text.Contains("cover-crop") || f.text.Contains("cultivate-margin")), "Player prose uses practice names, not internal identifiers");
        Check(request.facts.Count(f => f.id.StartsWith("disruption:")) == report.simulation.impacts.Select(x => x.siteId).Distinct().Count(), "Every affected site has an aggregate fact");
        var repeated = AnnualReportProjection.Build(rules, restored.History[i], i == 0 ? null : restored.History[i - 1]);
        Check(JsonSerializer.Serialize(request, json) == JsonSerializer.Serialize(repeated, json), "Restored projection is byte-identical");
        var local = ReportNarration.Local(request, "service-unavailable");
        ReportNarration.ValidateReceipt(request, local);
        Check(local.cards.Select(c => c.factId).SequenceEqual(request.facts.Take(3).Select(f => f.id)), "Local summary preserves canonical priorities");
        Check(local.introduction == request.introduction, "Local fallback retains all three scores and exact saved cash");
    }
    Check(snapshot == JsonSerializer.Serialize(town.Snapshot(), json) && definition == JsonSerializer.Serialize(rules, json), "Projection never mutates ledger, history, weather or definition");
}
var first = publicRun.History[0]; var second = publicRun.History[1];
var firstRequest = AnnualReportProjection.Build(rules, first, null);
var fingerprint = ReportNarration.RequestHash(firstRequest);
var originalCulture = CultureInfo.CurrentCulture;
try
{
    foreach (string culture in new[] { "tr-TR", "fr-FR", "ar-SA", "ja-JP" })
    {
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        Check(ReportNarration.RequestHash(AnnualReportProjection.Build(rules, first, null)) == fingerprint, "Projection is culture invariant: " + culture);
    }
}
finally { CultureInfo.CurrentCulture = originalCulture; }
Reject(() => AnnualReportProjection.Build(legacy, first, null), "Legacy report path is not reinterpreted");
Reject(() => AnnualReportProjection.Build(rules, first, first), "Year 1 rejects a prior report");
Reject(() => AnnualReportProjection.Build(rules, second, null), "Later year requires its preceding report");
Reject(() => AnnualReportProjection.Build(rules, second, second), "Wrong preceding year rejected");
Reject(() => AnnualReportProjection.Build(rules, second, idle.History[0]), "Other run's prior report rejected");
var wrongRules = Copy(rules); wrongRules.startingCashCents++;
Reject(() => AnnualReportProjection.Build(wrongRules, first, null), "Different definition identity rejected");
foreach (var mutate in new Action<TownYearReport>[] {
    r => r.ledger.closingCashCents++, r => r.simulation.economicDelta += 1,
    r => r.simulation.driver.crops[0].precipTenthsMm++, r => r.simulation.crops[0].outputUnits += 1,
    r => r.simulation.closingState.sites[0].condition = .123
})
{
    var altered = Copy(first); mutate(altered);
    Reject(() => AnnualReportProjection.Build(rules, altered, null), "Changed report hash rejected");
}
// A recomputed digest is not proof of validity. These local joins must still reject.
foreach (var mutate in new Action<TownYearReport>[] {
    r => r.simulation.economicDelta += 1,
    r => r.simulation.socialDelta = double.NaN,
    r => r.simulation.energyDeficit += 1,
    r => r.simulation.transitionId = "other-request",
    r => r.acceptedRequest.inputHash = "bad",
    r => r.simulation.crops[0].siteId = "invented-site",
    r => r.simulation.crops[0].ruleId = "invented-cause",
    r => r.simulation.crops[0].practiceId = "invented-practice",
    r => r.simulation.crops[0].harvestDay++,
    r => r.simulation.crops[0].outputUnits += 1,
    r => r.simulation.impacts[0].eventId = "invented-event",
    r => r.simulation.impacts[0].connectionGroupId = "invented-group",
    r => r.simulation.impacts[0].beforeHarvest = !r.simulation.impacts[0].beforeHarvest,
    r => r.simulation.closingState.sites[0].siteId = "invented-site",
    r => r.outcomes.sites[0].siteId = "invented-site",
    r => r.simulation.payments[0].paidCents++,
    r => r.simulation.payments = r.simulation.payments.Concat(new[] { r.simulation.payments[0] }).ToArray(),
    r => r.simulation.driver.crops[0].precipTenthsMm++,
    r => r.ledger.closingCashCents++
})
{
    var altered = Copy(first); mutate(altered); altered.simulation.transitionHash = AnnualSimulation.ReportHash(altered);
    Reject(() => AnnualReportProjection.Build(rules, altered, null), "Rehashed inconsistent projection source rejected");
}
var badPrevious = Copy(first); badPrevious.ledger.closingCashCents++;
badPrevious.simulation.transitionHash = AnnualSimulation.ReportHash(badPrevious);
Reject(() => AnnualReportProjection.Build(rules, second, badPrevious), "Rehashed previous ledger mismatch rejected");
var badMode = ReportNarration.Local(firstRequest, "network"); badMode.cards[0].text += " Guaranteed profits next year.";
Reject(() => ReportNarration.ValidateReceipt(firstRequest, badMode), "Canonical fact prose cannot be extended by a provider");
var detached = AnnualReportProjection.Build(rules, first, null); detached.facts[0].text = "caller edit";
Check(ReportNarration.RequestHash(AnnualReportProjection.Build(rules, first, null)) == fingerprint, "Returned projection is detached");
if (args.Contains("--write-evidence"))
{
    var folder = "docs/evidence/annual-report-projection"; Directory.CreateDirectory(folder);
    File.WriteAllText(folder + "/year1-public-request.json", JsonSerializer.Serialize(firstRequest, json) + "\n");
    var last = AnnualReportProjection.Build(rules, publicRun.History[9], publicRun.History[8]);
    File.WriteAllText(folder + "/year10-public-local.json", JsonSerializer.Serialize(ReportNarration.Local(last, "service-unavailable"), json) + "\n");
    File.WriteAllText(folder + "/checks.json", JsonSerializer.Serialize(new { assertions = checks, numericYears = outputs.Count, liveProviderCalls = 0,
        unity = false, chrome = false, firstRequestHash = fingerprint, maximumFacts = outputs.Max(r => r.facts.Length),
        maximumFactTextLength = outputs.SelectMany(r => r.facts).Max(f => f.text.Length) }, json) + "\n");
}
Console.WriteLine($"Accepted-report projection: {checks} assertions passed across {outputs.Count} accepted years; no live provider calls.");
Console.WriteLine("Year 1 request hash: " + fingerprint);
