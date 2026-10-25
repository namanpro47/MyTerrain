using System.Net;
using System.Text;
using Terra.Chapters;
using Terra.Contracts;
using Terra.Core;

if (args.Length == 2 && args[0] == "--unity-fixture") {
    var unityInput = ChapterJson.Read<AnnualChapterRequest>(await File.ReadAllTextAsync(args[1]));
    AnnualChapters.Preview(unityInput);
    Console.WriteLine("Unity-to-server serialized request replay passed.");
    return;
}

int checks = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
async Task Reject(Func<Task> action, string message) {
    bool rejected = false;
    try { await action(); } catch { rejected = true; }
    Check(rejected, message);
}
void RejectSync(Action action, string message) => Reject(() => { action(); return Task.CompletedTask; }, message).GetAwaiter().GetResult();
T Copy<T>(T value) => ChapterJson.Read<T>(ChapterJson.Write(value));
var envFixture = Path.Combine(Path.GetTempPath(), "terra-dotenv-" + Guid.NewGuid().ToString("N") + ".env");
var envValues = new Dictionary<string, string> { ["GEMINI_MODEL"] = "existing-process-model" };
try {
    await File.WriteAllTextAsync(envFixture, "# Synthetic test data only\nexport GEMINI_API_KEY=\"test-placeholder=a#b\" # comment\nGEMINI_MODEL=file-model\nTERRA_ALLOWED_ORIGINS='http://localhost:8080'\nASPNETCORE_URLS=http://0.0.0.0:80\n");
    LocalEnvironment.Load(envFixture, name => envValues.GetValueOrDefault(name), (name, value) => envValues[name] = value);
    Check(envValues["GEMINI_API_KEY"] == "test-placeholder=a#b", "Dotenv quotes, comment, export or equals parsing failed.");
    Check(envValues["GEMINI_MODEL"] == "existing-process-model", "Dotenv overwrote platform/shell environment.");
    Check(envValues["TERRA_ALLOWED_ORIGINS"] == "http://localhost:8080" && !envValues.ContainsKey("ASPNETCORE_URLS"), "Dotenv allowed unsupported server configuration.");
    envValues.Clear();
    await File.WriteAllTextAsync(envFixture, "GEMINI_API_KEY=synthetic-first-line\nGEMINI_MODEL=\"synthetic-unclosed\n");
    string diagnostic = "";
    try { LocalEnvironment.Load(envFixture, _ => null, (name, value) => envValues[name] = value); } catch (FormatException error) { diagnostic = error.Message; }
    Check(diagnostic.Contains("line 2") && !diagnostic.Contains("synthetic-unclosed") && envValues.Count == 0, "Malformed dotenv leaked content or partially applied secrets.");
    LocalEnvironment.Load(envFixture + "-missing", _ => null, (_, _) => throw new Exception("Missing file must be a no-op."));
    Check(envValues.Count == 0, "Missing dotenv changed environment.");
} finally { File.Delete(envFixture); }
var unityFixture = ChapterJson.Read<AnnualChapterRequest>(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "fixtures/unity-pending-year.json")));
Check(AnnualChapters.Preview(unityFixture).History.Length == 1, "Actual Unity JSON/hash could not replay on the server.");
var definition = AnnualTownChecks.Fixture();
var town = new AnnualTown(definition, "chapter-checks");
town.QueueAction("a", "single-species", town.Revision);
town.QueueAction("b", "multi-species", town.Revision);
town.BeginResolution("year-1", town.Revision);
var input = new AnnualChapterRequest { definition = definition, snapshot = town.Snapshot() };
var original = ChapterJson.Write(input);
var preview = AnnualChapters.Preview(input);
Check(preview.CashCents == 902500 && preview.History.Length == 1, "Preview must use the real public ledger.");
Check(ChapterJson.Write(input) == original && town.CashCents == 1000000 && town.Phase == TownPhase.Resolving, "Preview mutated live input.");
var offline = AnnualChapters.Offline(input);
AnnualChapters.ValidateReceipt(input, offline);
Check(offline.mode == "offline" && offline.chapter.cards.Length == 3, "Offline path must be labelled.");
var invalid = Copy(offline); invalid.consequences.effects = new[] { new TownSiteEffect { siteId = "a", sourceId = "fixture-year-1", productionFactor = 1.1, reason = "Invented bonus" } };
RejectSync(() => AnnualChapters.ValidateReceipt(input, invalid), "AI must not inject even an in-range effect.");
invalid = Copy(offline); invalid.consequences.inputHash = "different";
RejectSync(() => AnnualChapters.ValidateReceipt(input, invalid), "Must reject stale transition.");
var badText = Copy(offline.chapter); badText.cards[0].factIds = new[] { "site:invented" };
RejectSync(() => AnnualChapters.ValidateText(badText, AnnualChapters.Facts(preview)), "Unknown fact must fail.");
badText = Copy(offline.chapter); badText.title = "<b>fake</b>";
RejectSync(() => AnnualChapters.ValidateText(badText, AnnualChapters.Facts(preview)), "Markup must fail.");
var directory = Path.Combine(Path.GetTempPath(), "terra-chapters-check-" + Guid.NewGuid().ToString("N"));
var narrator = new FakeNarrator(offline.chapter);
var service = new ChapterService(narrator, directory);
var pair = await Task.WhenAll(service.Resolve(input, default), service.Resolve(input, default));
Check(narrator.Calls == 1 && ChapterJson.Write(pair[0]) == ChapterJson.Write(pair[1]), "Concurrent requests must share the first receipt.");
var restarted = new ChapterService(new ThrowNarrator(), directory);
var restored = await restarted.Resolve(input, default);
Check(ChapterJson.Write(restored) == ChapterJson.Write(pair[0]), "Service restart must reuse persisted response without calling provider.");
var reused = Copy(input); reused.snapshot.undo = Array.Empty<TownPlanSnapshot>();
await Reject(() => restarted.Resolve(reused, default), "ID reused with different frozen snapshot must fail.");
town.CompleteResolution(restored.consequences);
town.CompleteResolution(restored.consequences);
Check(town.CashCents == 902500 && town.History.Length == 1, "Repeated delivery double charged year.");
var repairing = new FakeNarrator(offline.chapter) { Failures = 2 };
await new ChapterService(repairing, directory + "-repair").Resolve(input, default);
Check(repairing.Calls == 3, "Two repairs should recover.");
var broken = new FakeNarrator(offline.chapter) { Failures = 10 };
await Reject(() => new ChapterService(broken, directory + "-broken").Resolve(input, default), "Invalid output must fail after bounded repair.");
Check(broken.Calls == 3 && !Directory.Exists(directory + "-broken"), "Failed output must not be cached or repaired indefinitely.");
var cancelled = new CancellationTokenSource(); cancelled.Cancel();
await Reject(() => new ChapterService(narrator, directory + "-cancel").Resolve(input, cancelled.Token), "Cancellation should not accept year.");

var handler = new StubHttpHandler();
using var http = new HttpClient(handler);
var gemini = new GeminiNarrator(http, "test-only-not-a-real-key");
handler.Body = "{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":[{\"text\":" + System.Text.Json.JsonSerializer.Serialize(ChapterJson.Write(offline.chapter)) + "}]}}]}";
var parsed = await gemini.Generate(AnnualChapters.Facts(preview), "", default);
Check(parsed.cards.Length == 3 && handler.SawHeader && !handler.Url.Contains("key="), "Gemini wire format or header credential handling failed.");
Check(handler.SentBody.Contains("responseSchema") && handler.SentBody.Contains("site:a"), "Request lacks constrained schema/facts.");
handler.Status = HttpStatusCode.TooManyRequests;
await Reject(() => gemini.Generate(AnnualChapters.Facts(preview), "", default), "Quota response must fail.");
handler.Status = HttpStatusCode.OK; handler.Body = "{\"candidates\":[{\"finishReason\":\"MAX_TOKENS\"}]}";
await Reject(() => gemini.Generate(AnnualChapters.Facts(preview), "", default), "Truncation must fail.");
handler.Body = "{\"promptFeedback\":{\"blockReason\":\"SAFETY\"}}";
await Reject(() => gemini.Generate(AnnualChapters.Facts(preview), "", default), "Refusal must fail.");
handler.Body = "not-json";
await Reject(() => gemini.Generate(AnnualChapters.Facts(preview), "", default), "Malformed response must fail.");
var previousCalls = handler.Calls;
await Reject(() => new GeminiNarrator(http, null).Generate(AnnualChapters.Facts(preview), "", default), "Missing key must fail.");
Check(handler.Calls == previousCalls, "Missing key must not call network.");
RejectSync(() => ChapterJson.Read<AnnualChapterText>("{\"title\":\"T\",\"cash\":1000}"), "Unknown fields must fail.");

// Fast-forward uses the same model, shared saved drivers and one report per year.
while (town.Phase != TownPhase.Completed) {
    town.ContinuePlanning(town.Revision);
    town.BeginResolution("year-" + town.Year, town.Revision);
    var next = new AnnualChapterRequest { definition = definition, snapshot = town.Snapshot() };
    var chapter = AnnualChapters.Offline(next);
    AnnualChapters.ValidateReceipt(next, chapter);
    town.CompleteResolution(chapter.consequences);
}
Check(town.History.Length == 10 && town.Year == 10 && town.CashCents == 707500, "Ten-year history or exact three-installment budget differs.");
Check(AnnualTown.Restore(definition, Copy(town.Snapshot())).CashCents == town.CashCents, "Completed run failed restore.");
RejectSync(() => town.ContinuePlanning(town.Revision), "Must not create Year 11.");

if (args.Length == 2 && args[0] == "--fixture") {
    await File.WriteAllTextAsync(args[1], original);
}
Console.WriteLine("Terra chapter checks passed: " + checks + "; no real provider called. Cache fixtures: " + directory);

sealed class FakeNarrator(AnnualChapterText text) : IChapterNarrator {
    public int Calls, Failures;
    public string Model => "fake-test";
    public Task<AnnualChapterText> Generate(AnnualChapterFact[] facts, string diagnostics, CancellationToken cancellation) {
        Calls++;
        if (Failures-- > 0) throw new ArgumentException("Invalid test output.");
        return Task.FromResult(text);
    }
}
sealed class ThrowNarrator : IChapterNarrator {
    public string Model => "must-not-run";
    public Task<AnnualChapterText> Generate(AnnualChapterFact[] facts, string diagnostics, CancellationToken cancellation) => throw new Exception("Provider unexpectedly invoked.");
}
sealed class StubHttpHandler : HttpMessageHandler {
    public HttpStatusCode Status = HttpStatusCode.OK;
    public string Body, Url, SentBody;
    public bool SawHeader;
    public int Calls;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
        Calls++; Url = request.RequestUri.ToString(); SawHeader = request.Headers.Contains("x-goog-api-key");
        SentBody = await request.Content.ReadAsStringAsync(cancellationToken);
        return new HttpResponseMessage(Status) { Content = new StringContent(Body ?? "", Encoding.UTF8, "application/json") };
    }
}
