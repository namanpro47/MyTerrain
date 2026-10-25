using System.Net;
using System.Text;
using System.Text.Json;
using Terra.Chapters;
using Terra.Contracts;
using Terra.Core;

ReportNarrationRequest Fixture() => new() {
    year = 1, acceptedTransitionId = "fixture-run:year-1:revision-2", acceptedTransitionHash = new string('a', 64),
    rulesVersion = "annual-fixture-v4", reportVersion = "accepted-report-v2", promptVersion = ReportNarration.PromptVersion,
    modelVersion = ReportNarration.DefaultModelVersion, title = "Year 1: accepted results",
    introduction = "Economic 51 (+2), Social 58 (+0), Environmental 47.5 (+1). Cash $9,000,000 is saved. These are illustrative results.",
    facts = new[] {
        new ReportNarrationFact { id = "budget", title = "Public budget", text = "$100,000 of public upkeep was paid. Private harvest is not public income.", siteIds = Array.Empty<string>(), causalRuleIds = new[] { "ledger-v4" } },
        new ReportNarrationFact { id = "harvest", title = "Harvest and weather", text = "West field output decreased under the saved dry-season response. This is an authored scenario, not a forecast.", siteIds = new[] { "west-field" }, causalRuleIds = new[] { "crop-dry-v4" } },
        new ReportNarrationFact { id = "recovery", title = "Next year's starting condition", text = "Funded public repair improves next year's starting condition; it does not reverse this year's harvest loss.", siteIds = new[] { "public-swale" }, causalRuleIds = new[] { "repair-after-harvest-v4" } },
        new ReportNarrationFact { id = "reference", title = "Shared reference", text = "Fictional scenario · weather and economic references: Coon Valley, Wisconsin · other parameters are authored game assumptions.", siteIds = Array.Empty<string>(), causalRuleIds = Array.Empty<string>() }
    }
};
if (args.Length == 2 && args[0] == "--fixture") {
    await File.WriteAllTextAsync(args[1], ChapterJson.Write(Fixture()));
    return;
}
int checks = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
void Reject(Action action, string message) { bool rejected = false; try { action(); } catch (ArgumentException) { rejected = true; } Check(rejected, message); }
async Task RejectAsync<T>(Func<Task> action, string message) where T : Exception {
    bool rejected = false; try { await action(); } catch (T) { rejected = true; } Check(rejected, message);
}
T Copy<T>(T value) => ChapterJson.Read<T>(ChapterJson.Write(value));
var root = Path.Combine(Path.GetTempPath(), "terra-report-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
string Cache(string test) => Path.Combine(root, test);
ReportNarrationSelection Select(params string[] ids) => new() { factIds = ids };
try {
    var input = Fixture(); ReportNarration.ValidateRequest(input);
    var fingerprint = ReportNarration.RequestHash(input);
    Check(fingerprint == "06070d5012ba1d2e1a2850c49a0598abb9db225be0f03ae499448493e9ea0f86", "Pinned cross-runtime hash contract");
    Check(fingerprint.Length == 64 && fingerprint == ReportNarration.RequestHash(Copy(input)), "Canonical request identity survives JSON roundtrip");
    var oldCulture = System.Globalization.CultureInfo.CurrentCulture;
    try { System.Globalization.CultureInfo.CurrentCulture = new("tr-TR"); Check(fingerprint == ReportNarration.RequestHash(input), "Hash is culture independent"); }
    finally { System.Globalization.CultureInfo.CurrentCulture = oldCulture; }
    var chosen = ReportNarration.BuildReceipt(input, Select("harvest", "budget"));
    Check(chosen.cards[0].text == input.facts[1].text && chosen.cards[1].title == input.facts[0].title, "AI only orders exact canonical blocks");
    Check(chosen.title == input.title && chosen.introduction == input.introduction, "Summary is canonical too");
    Check(chosen.mode == ReportNarration.AiMode && ReportNarration.ModeLabel(chosen.mode) == "AI-organized explanation", "Accurate AI provenance");
    Check(!ChapterJson.Write(chosen).Contains("consequences") && !ChapterJson.Write(input).Contains("snapshot"), "No numeric effect/snapshot contract");
    foreach (var mutate in new Action<ReportNarrationRequest>[] {
        r => r.schemaVersion = 1, r => r.year = 0, r => r.year = 11, r => r.acceptedTransitionHash = "bad",
        r => r.promptVersion = "new-unreviewed-prompt", r => r.title = "<b>unsafe</b>", r => r.introduction = new string('x', 501),
        r => r.facts = Array.Empty<ReportNarrationFact>(), r => r.facts[1].id = r.facts[0].id,
        r => r.facts[0].siteIds = null, r => r.facts[0].siteIds = new[] { "bad site" },
        r => r.facts[0].causalRuleIds = new[] { "same", "same" }, r => r.facts[0].text = "bad\0control",
        r => r.facts[0].text = "unpaired\ud800", r => r.facts[0].text = new string('x', 601)
    }) { var bad = Copy(input); mutate(bad); Reject(() => ReportNarration.ValidateRequest(bad), "Bad projection rejected"); }
    foreach (var ids in new[] { Array.Empty<string>(), new[] { "unknown" }, new[] { "budget", "budget" }, new[] { "budget", "harvest", "recovery", "reference" } })
        Reject(() => ReportNarration.BuildReceipt(input, Select(ids)), "Selection count, uniqueness and identity enforced");
    foreach (var mutate in new Action<ReportNarrationReceipt>[] {
        r => r.cards[0].text = r.cards[0].text.Replace("decreased", "increased"),
        r => r.cards[1].text = r.cards[1].text.Replace("$100,000", "$900,000"),
        r => r.cards[0].text += " This prevents flooding everywhere.", r => r.title = "Unsupported story",
        r => r.introduction += " Profit is guaranteed.", r => r.acceptedTransitionHash = new string('b', 64),
        r => r.requestHash = new string('0', 64), r => r.modelVersion = "other-model", r => r.contractVersion = "other",
        r => r.mode = "offline-other", r => r.reasonCode = "quota", r => r.cards[0].factId = "unknown"
    }) { var bad = Copy(chosen); mutate(bad); Reject(() => ReportNarration.ValidateReceipt(input, bad), "Receipt cannot alter amounts, direction, causes or identity"); }
    var local = ReportNarration.Local(input, "quota");
    Check(local.cards.Select(c => c.factId).SequenceEqual(new[] { "budget", "harvest", "recovery" }), "Local order is deterministic");
    Check(!ReportNarration.ReasonMessage("quota").Contains("credit") && !ReportNarration.ReasonMessage("quota").Contains("billing"), "Quota does not guess billing status");
    var reOrderedLocal = Copy(local); Array.Reverse(reOrderedLocal.cards);
    Reject(() => ReportNarration.ValidateReceipt(input, reOrderedLocal), "Local order cannot silently change");
    Reject(() => ReportNarration.Local(input, "secret provider payload"), "Only typed safe failure reasons");
    var changed = Copy(input); changed.facts[0].siteIds = new[] { "east-field" };
    Check(ReportNarration.RequestHash(changed) != fingerprint && ReportNarration.CacheKey(changed) == ReportNarration.CacheKey(input), "Linked sites affect projection hash, not immutable transition cache identity");
    changed = Copy(input); changed.facts[0].causalRuleIds = new[] { "other-cause" };
    Check(ReportNarration.RequestHash(changed) != fingerprint, "Causal rules are pinned");

    var narrator = new FakeNarrator { Behavior = (_, _, _) => Task.FromResult(Select("harvest", "budget")) };
    var service = new ReportNarrationService(narrator, Cache("success"));
    var duplicates = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => service.Resolve(input, CancellationToken.None)));
    Check(narrator.Calls == 1, "Concurrent duplicates consume one provider call");
    Check(duplicates.All(r => ChapterJson.Write(r) == ChapterJson.Write(duplicates[0])), "Concurrent responses are identical");
    var forbiddenProvider = new FakeNarrator { Behavior = (_, _, _) => throw new Exception("Must use cache") };
    var restored = await new ReportNarrationService(forbiddenProvider, Cache("success")).Resolve(input, CancellationToken.None);
    Check(forbiddenProvider.Calls == 0 && ChapterJson.Write(restored) == ChapterJson.Write(duplicates[0]), "Restart returns first saved receipt");
    changed = Copy(input); changed.facts[0].text += " Changed projection.";
    await RejectAsync<ReportNarrationConflictException>(() => service.Resolve(changed, CancellationToken.None), "Same transition cannot overwrite different facts");
    Check(narrator.Calls == 1, "Conflict rejected before quota");
    var alias = Copy(input); alias.acceptedTransitionId = "different-transition-id";
    await RejectAsync<ReportNarrationConflictException>(() => service.Resolve(alias, CancellationToken.None), "A second identity cannot reuse the same pinned transition hash");
    var contenderA = new FakeNarrator(); var contenderB = new FakeNarrator();
    var competing = await Task.WhenAll(new ReportNarrationService(contenderA, Cache("contenders")).Resolve(input, CancellationToken.None),
        new ReportNarrationService(contenderB, Cache("contenders")).Resolve(input, CancellationToken.None));
    Check(contenderA.Calls + contenderB.Calls == 1 && ChapterJson.Write(competing[0]) == ChapterJson.Write(competing[1]), "Separate service instances share first-receipt ownership");

    foreach (var reason in new[] { "missing-config", "network", "quota", "refusal", "provider-unavailable" }) {
        var failure = new FakeNarrator { Behavior = (_, _, _) => throw new ReportProviderFailure(reason) };
        var fallback = await new ReportNarrationService(failure, Cache(reason)).Resolve(input, CancellationToken.None);
        ReportNarration.ValidateReceipt(input, fallback);
        Check(fallback.mode == "local" && fallback.reasonCode == reason && failure.Calls == 1, "Typed failure automatically uses same saved facts");
        var retry = new FakeNarrator { Behavior = (_, _, _) => Task.FromResult(Select("reference")) };
        var pinned = await new ReportNarrationService(retry, Cache(reason)).Resolve(input, CancellationToken.None);
        Check(retry.Calls == 0 && ChapterJson.Write(pinned) == ChapterJson.Write(fallback), "First local mode survives later provider recovery");
    }
    var repairs = new FakeNarrator { Behavior = (_, _, _) => Task.FromResult(Select("invented")) };
    var rejectedText = await new ReportNarrationService(repairs, Cache("invalid")).Resolve(input, CancellationToken.None);
    Check(repairs.Calls == 3 && rejectedText.reasonCode == "invalid-selection", "Invalid selection has only two repairs then local fallback");
    var repaired = new FakeNarrator(); repaired.Behavior = (_, feedback, _) => {
        if (repaired.Calls == 1) throw new JsonException("bad output");
        Check(feedback.Contains("No title"), "Repair has bounded structural feedback"); return Task.FromResult(Select("recovery"));
    };
    var repairedReceipt = await new ReportNarrationService(repaired, Cache("repaired")).Resolve(input, CancellationToken.None);
    Check(repaired.Calls == 2 && repairedReceipt.mode == ReportNarration.AiMode, "A valid bounded repair is accepted");
    var slow = new FakeNarrator { Behavior = async (_, _, token) => { await Task.Delay(60000, token); return Select("budget"); } };
    var timed = await new ReportNarrationService(slow, Cache("timeout"), TimeSpan.FromMilliseconds(30)).Resolve(input, CancellationToken.None);
    Check(timed.mode == "local" && timed.reasonCode == "timeout", "Deadline returns local facts without numeric resolution");
    var network = new FakeNarrator { Behavior = (_, _, _) => throw new HttpRequestException("secret transport detail") };
    var disconnected = await new ReportNarrationService(network, Cache("disconnected")).Resolve(input, CancellationToken.None);
    Check(disconnected.reasonCode == "network" && !ChapterJson.Write(disconnected).Contains("secret"), "Network details do not leak");
    var mismatch = new FakeNarrator { ModelVersion = "other-model" };
    var mismatched = await new ReportNarrationService(mismatch, Cache("mismatch")).Resolve(input, CancellationToken.None);
    Check(mismatched.reasonCode == "model-mismatch" && mismatch.Calls == 0, "Model contract mismatch consumes no quota");

    using (var canceled = new CancellationTokenSource()) {
        canceled.Cancel(); var noCall = new FakeNarrator();
        await RejectAsync<OperationCanceledException>(() => new ReportNarrationService(noCall, Cache("cancel-before")).Resolve(input, canceled.Token), "Caller cancellation propagates");
        Check(noCall.Calls == 0 && !Directory.Exists(Cache("cancel-before")), "Canceled call creates no receipt");
    }
    var started = new TaskCompletionSource();
    using (var cancellation = new CancellationTokenSource()) {
        var pending = new FakeNarrator { Behavior = async (_, _, token) => { started.SetResult(); await Task.Delay(60000, token); return Select("budget"); } };
        var pendingTask = new ReportNarrationService(pending, Cache("cancel-during")).Resolve(input, cancellation.Token);
        await started.Task; cancellation.Cancel();
        await RejectAsync<OperationCanceledException>(() => pendingTask, "Canceled in-flight caller does not publish a local receipt");
        Check(!Directory.Exists(Cache("cancel-during")), "In-flight cancellation has no cached mode");
    }
    var release = new TaskCompletionSource<ReportNarrationSelection>();
    var mutable = Copy(input); var before = Copy(mutable);
    var held = new FakeNarrator { Behavior = (projection, _, _) => { projection.facts[0].text = "Mutated provider copy"; return release.Task; } };
    var detachedTask = new ReportNarrationService(held, Cache("detached")).Resolve(mutable, CancellationToken.None);
    mutable.facts[0].text = "Mutated caller"; mutable.facts[0].siteIds = new[] { "different" };
    release.SetResult(Select("budget"));
    var detached = await detachedTask; ReportNarration.ValidateReceipt(before, detached);
    Check(detached.cards[0].text == before.facts[0].text, "Caller/provider mutation cannot rewrite frozen facts");
    var cacheFile = Directory.GetFiles(Path.Combine(Cache("success"), "report-v2"), "*.json").Single();
    var corrupted = ChapterJson.Read<ReportNarrationService.SavedReceipt>(await File.ReadAllTextAsync(cacheFile));
    corrupted.receipt.cards[0].text = "Unsupported new amount $777";
    await File.WriteAllTextAsync(cacheFile, ChapterJson.Write(corrupted));
    await RejectAsync<ReportNarrationCacheException>(() => service.Resolve(input, CancellationToken.None), "Corrupt cache is rejected, never regenerated");
    Check(narrator.Calls == 1, "Corrupt cache does not spend more quota");
    await File.WriteAllTextAsync(Cache("disk-file"), "occupied");
    await RejectAsync<ReportNarrationCacheException>(() => new ReportNarrationService(new FakeNarrator(), Cache("disk-file")).Resolve(input, CancellationToken.None), "Persistence failure cannot return an unpersisted success");

    string Envelope(string selection) => JsonSerializer.Serialize(new { candidates = new[] { new { finishReason = "STOP", content = new { parts = new[] { new { text = selection } } } } } });
    foreach (var output in new[] { "{}", "[]", "{\"factIds\":[\"budget\"],\"text\":\"invented cause\"}", "{\"factIds\":[\"budget\"],\"factIds\":[\"harvest\"]}", "{\"factIds\":[\"unknown\"]}" }) {
        var handler = new FakeHttp { Response = _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Envelope(output)) }) };
        using var http = new HttpClient(handler);
        var result = await new ReportNarrationService(new ReportGeminiNarrator(http, "test-key"), Cache("http-" + Guid.NewGuid().ToString("N"))).Resolve(input, CancellationToken.None);
        Check(result.mode == "local" && result.reasonCode == "invalid-selection" && handler.Calls == 3, "Malformed/extra free prose/unknown IDs rejected after bounded repairs");
    }
    var goodHttp = new FakeHttp { Response = async request => {
        var body = await request.Content.ReadAsStringAsync();
        Check(request.RequestUri.Host == "generativelanguage.googleapis.com" && request.Headers.Contains("x-goog-api-key"), "Provider target and credential header match existing API");
        Check(!body.Contains("test-key") && !body.Contains("acceptedTransitionHash"), "Provider receives facts, not credential or internal transition identity");
        Check(body.Contains("Do not generate or paraphrase"), "Prompt requests selection only");
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Envelope("{\"factIds\":[\"harvest\"]}")) };
    } };
    using (var http = new HttpClient(goodHttp)) {
        var result = await new ReportNarrationService(new ReportGeminiNarrator(http, "test-key"), Cache("http-good")).Resolve(input, CancellationToken.None);
        Check(result.cards.Single().text == input.facts[1].text && goodHttp.Calls == 1, "Actual provider adapter returns exact selected canonical text");
    }
    foreach (var status in new[] { HttpStatusCode.TooManyRequests, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError }) {
        var handler = new FakeHttp { Response = _ => Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent("secret provider credentials and billing claims") }) };
        using var http = new HttpClient(handler);
        var result = await new ReportNarrationService(new ReportGeminiNarrator(http, "test-key"), Cache("status-" + (int)status)).Resolve(input, CancellationToken.None);
        var expected = status == HttpStatusCode.TooManyRequests ? "quota" : status == HttpStatusCode.InternalServerError ? "provider-unavailable" : "missing-config";
        Check(result.reasonCode == expected && handler.Calls == 1 && !ChapterJson.Write(result).Contains("secret"), "HTTP failure is typed and sanitized without retry loop");
    }
    var missingHttp = new FakeHttp();
    using (var http = new HttpClient(missingHttp)) {
        var result = await new ReportNarrationService(new ReportGeminiNarrator(http, ""), Cache("http-missing")).Resolve(input, CancellationToken.None);
        Check(result.reasonCode == "missing-config" && missingHttp.Calls == 0, "Missing key performs no network request");
    }
    foreach (var body in new[] { "{\"candidates\":[]}", "{\"candidates\":[{\"finishReason\":\"SAFETY\"}]}" }) {
        var handler = new FakeHttp { Response = _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) }) };
        using var http = new HttpClient(handler);
        var result = await new ReportNarrationService(new ReportGeminiNarrator(http, "test-key"), Cache("refusal-" + Guid.NewGuid().ToString("N"))).Resolve(input, CancellationToken.None);
        Check(result.reasonCode == "refusal" && handler.Calls == 1, "Refusal does not trigger repair loop");
    }
    foreach (var body in new[] { "[]", "not-json", new string('x', 128 * 1024 + 1) }) {
        var handler = new FakeHttp { Response = _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) }) };
        using var http = new HttpClient(handler);
        var result = await new ReportNarrationService(new ReportGeminiNarrator(http, "test-key"), Cache("envelope-" + Guid.NewGuid().ToString("N"))).Resolve(input, CancellationToken.None);
        Check(result.reasonCode == "invalid-selection" && handler.Calls == 3, "Malformed or oversized envelope has bounded repair and fallback");
    }
    Check(!Directory.GetFiles(root, "*.tmp", SearchOption.AllDirectories).Any(), "No incomplete cache files remain");
    Console.WriteLine($"Report-only narration v2: {checks} assertions passed; no live provider calls.");
    Console.WriteLine("Fixture request SHA-256: " + fingerprint);
}
finally { Directory.Delete(root, true); }

sealed class FakeNarrator : IReportNarrator
{
    public string ModelVersion { get; set; } = ReportNarration.DefaultModelVersion;
    public int Calls;
    public Func<ReportNarrationRequest, string, CancellationToken, Task<ReportNarrationSelection>> Behavior =
        (input, _, _) => Task.FromResult(new ReportNarrationSelection { factIds = new[] { input.facts[0].id } });
    public Task<ReportNarrationSelection> Select(ReportNarrationRequest input, string diagnostics, CancellationToken cancellation)
    { Interlocked.Increment(ref Calls); return Behavior(input, diagnostics, cancellation); }
}
sealed class FakeHttp : HttpMessageHandler
{
    public int Calls;
    public Func<HttpRequestMessage, Task<HttpResponseMessage>> Response = _ => throw new Exception("Unexpected HTTP request");
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation)
    { Interlocked.Increment(ref Calls); return Response(request); }
}
