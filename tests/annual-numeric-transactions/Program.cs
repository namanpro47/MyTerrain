using System.Text.Json;
using Terra.Contracts;
using Terra.Core;

var json = new JsonSerializerOptions { IncludeFields = true };
T Read<T>(string text) => JsonSerializer.Deserialize<T>(text, json)!;
T Copy<T>(T value) => Read<T>(JsonSerializer.Serialize(value, json));
string Text<T>(T value) => JsonSerializer.Serialize(value, json);
int checks = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
async Task Reject<T>(Func<Task> action, string message) where T : Exception
{ bool rejected = false; try { await action(); } catch (T) { rejected = true; } Check(rejected, message); }
var weather = new AnnualSimulationWeather(File.ReadAllBytes("simulation-data/weather/coon-valley-v1/weather-pack.json"),
    File.ReadAllBytes("simulation-data/weather/coon-valley-v1/daily-observations.json"), Read<TownWeatherPack>, Read<TownDailyWeather>);
var rules = AnnualSimulationFactory.Create(Read<AnnualTownDefinition>(File.ReadAllText("docs/evidence/hud-connected/definition.json")), weather, "coon-valley-demo-1");
var park = rules.actions.First(a => a.actionId == "public-park");
var creationStore = new FakeStore { FailAfterAt = 1 };
await Reject<IOException>(() => AnnualNumericTransactions.Open(rules, "creation-ack", creationStore), "Unacknowledged creation returns no session");
var recoveredCreation = await AnnualNumericTransactions.Open(rules, "creation-ack", creationStore);
Check(creationStore.Writes == 1 && recoveredCreation.State.history.Length == 0, "Reopen discovers committed creation without replacing it");
var store = new FakeStore();
var session = await AnnualNumericTransactions.Open(rules, "transaction-main", store);
Check(store.Writes == 1 && session.SaveState.sequence == 1, "New run is saved before a session is returned");
var snapshots = new List<AnnualNumericSave>();
session.Changed += value => snapshots.Add(value);
session.Changed += _ => throw new Exception("UI subscriber failure");
await session.QueueAction(park.siteId, park.actionId, session.State.revision);
Check(session.State.queue.Length == 1 && snapshots.Count == 1, "Planning command is saved then published despite failing subscriber");
var mutable = session.SaveState; mutable.town.cashCents = 0;
var detachedEngine = session.ReadEngine(); detachedEngine.ClearPlan(detachedEngine.Revision);
Check(session.State.cashCents == rules.startingCashCents && session.State.queue.Length == 1, "Returned state/engine cannot bypass persistence");

int revision = session.State.revision;
store.BlockAt = store.Writes + 2; // Frozen request succeeds; accepted candidate waits.
var resolved = session.ResolveYear("transaction-main/year-1", revision, "missing-config");
await store.Blocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
Check(session.State.phase == TownPhase.Resolving && session.State.history.Length == 0, "No accepted publication while save waits");
Check(store.Value!.town.phase == TownPhase.Resolving && store.Value.town.history.Length == 0, "Frozen retryable request persisted first");
Check(session.State.cashCents == rules.startingCashCents && session.ForYear(1) == null, "No spend or visible prose before acknowledgement");
Check(snapshots.All(s => s.town.history.Length == 0), "Subscribers have not received uncommitted results");
store.Release.SetResult(); await resolved;
Check(session.State.phase == TownPhase.Reviewing && session.State.history.Length == 1, "Acknowledgement publishes accepted year");
Check(session.ForYear(1).receipt.mode == ReportNarration.LocalMode && session.ForYear(1).status == "visible", "Known local fallback committed with numeric report");
Check(Text(session.SaveState) == Text(store.Value), "Published envelope matches stored envelope");
int writes = store.Writes;
await session.ResolveYear("transaction-main/year-1", revision, "network");
Check(store.Writes == writes && session.State.history.Length == 1, "Duplicate numeric acceptance does not save or spend twice");
await Reject<ArgumentException>(() => session.ResolveYear("transaction-main/year-1", revision + 1), "Committed identity cannot use a different revision");
await AnnualNumericTransactions.Open(rules, "transaction-main", store);
await Reject<ArgumentException>(() => AnnualNumericTransactions.Open(rules, "other-run", store), "Existing store never overwritten by another run");

var failStore = new FakeStore();
var failure = await AnnualNumericTransactions.Open(rules, "save-failure", failStore);
failStore.FailBeforeAt = failStore.Writes + 2;
await Reject<IOException>(() => failure.ResolveYear("save-failure/year-1", 0), "Accepted numeric storage failure surfaces");
Check(failure.State.phase == TownPhase.Resolving && failure.State.history.Length == 0 && failure.RecoveryRequired, "Failed save keeps same pending year and requires reread");
string pendingHash = failure.State.pendingRequest.inputHash;
await failure.ResolveYear("save-failure/year-1", 0);
Check(failure.State.history.Length == 1 && failure.State.history[0].acceptedRequest.inputHash == pendingHash, "Retry rereads and accepts same frozen input");
Check(failure.ForYear(1).status == "pending" && failure.ForYear(1).receipt == null, "Numeric success need not wait for prose");
var frozenStore = new FakeStore();
var unfrozen = await AnnualNumericTransactions.Open(rules, "frozen-save-failure", frozenStore);
frozenStore.FailBeforeAt = frozenStore.Writes + 1;
await Reject<IOException>(() => unfrozen.ResolveYear("frozen-save-failure/year-1", 0), "Failure to persist frozen request stops resolution");
Check(unfrozen.State.phase == TownPhase.Planning && unfrozen.State.history.Length == 0 && frozenStore.Value!.town.phase == TownPhase.Planning,
    "No numeric result or resolving publication after failed frozen save");

var lostStore = new FakeStore();
var lost = await AnnualNumericTransactions.Open(rules, "lost-ack", lostStore);
lostStore.FailAfterAt = lostStore.Writes + 2;
await Reject<IOException>(() => lost.ResolveYear("lost-ack/year-1", 0), "Lost acknowledgement surfaces without publication");
Check(lost.State.history.Length == 0 && lostStore.Value!.town.history.Length == 1, "Ambiguous save outcome is not falsely announced");
int ambiguousWrites = lostStore.Writes;
await lost.ResolveYear("lost-ack/year-1", 0);
Check(lost.State.history.Length == 1 && lostStore.Writes == ambiguousWrites && !lost.RecoveryRequired, "Reread discovers already committed year without a second write");

var planStore = new FakeStore();
var planning = await AnnualNumericTransactions.Open(rules, "planning-failure", planStore);
planStore.FailBeforeAt = planStore.Writes + 1;
await Reject<IOException>(() => planning.QueueAction(park.siteId, park.actionId, 0), "Planning save failure surfaces");
Check(planning.State.queue.Length == 0 && planning.State.revision == 0, "Failed planning command does not mutate visible state");
await planning.QueueAction(park.siteId, park.actionId, 0);
await planning.UndoPlan(planning.State.revision); await planning.RedoPlan(planning.State.revision);
Check(planning.State.queue.Length == 1, "Saved undo/redo retains current-year queue");

var request1 = failure.ForYear(1).request;
var ai1 = ReportNarration.BuildReceipt(request1, new ReportNarrationSelection { factIds = new[] { "services" } });
var invalid = Copy(ai1); invalid.cards[0].text += " Guaranteed profit.";
await Reject<ArgumentException>(() => failure.AcceptNarration(request1, invalid), "Invalid prose cannot become visible");
Check(failure.ForYear(1).status == "pending", "Invalid prose leaves accepted numeric state intact");
failStore.FailBeforeAt = failStore.Writes + 1;
await Reject<IOException>(() => failure.UseLocal(request1, "network"), "Prose persistence failure surfaces");
Check(failure.ForYear(1).status == "pending" && failure.State.history.Length == 1, "Unsaved local mode is not published and cannot undo numeric year");
await failure.UseLocal(request1, "network");
Check(!await failure.AcceptNarration(request1, ai1) && failure.ForYear(1).receipt.mode == "local", "Late AI cannot overwrite first visible local mode");
var reopened = await AnnualNumericTransactions.Open(rules, "save-failure", failStore);
Check(reopened.ForYear(1).receipt.reasonCode == "network", "First visible local mode survives reload");

var raceStore = new FakeStore();
var race = await AnnualNumericTransactions.Open(rules, "prose-race", raceStore);
await race.ResolveYear("prose-race/year-1", 0);
var raceRequest = race.ForYear(1).request;
var raceAI = ReportNarration.BuildReceipt(raceRequest, new ReportNarrationSelection { factIds = new[] { "services" } });
raceStore.BlockAt = raceStore.Writes + 1;
var localRace = race.UseLocal(raceRequest, "timeout");
await raceStore.Blocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
var aiRace = race.AcceptNarration(raceRequest, raceAI);
Check(race.ForYear(1).status == "pending", "First-mode race remains unpublished during storage wait");
raceStore.Release.SetResult();
Check(await localRace && !await aiRace && race.ForYear(1).receipt.mode == "local", "First acknowledged visible receipt wins concurrent race");

await lost.Continue(lost.State.revision);
await lost.ResolveYear("lost-ack/year-2", lost.State.revision);
var oldRequest = lost.ForYear(1).request;
var oldAI = ReportNarration.BuildReceipt(oldRequest, new ReportNarrationSelection { factIds = new[] { "services" } });
int currentYear = lost.State.year;
Check(await lost.AcceptNarration(oldRequest, oldAI), "An older accepted report may finish after continuation");
Check(lost.State.year == currentYear && lost.State.history.Length == 2 && lost.ForYear(2).status == "pending", "Old response does not move view/year or replace newer report");
Check(!await lost.AcceptNarration(request1, ai1), "A different run's response is ignored");

var shared = new FakeStore();
var firstWindow = await AnnualNumericTransactions.Open(rules, "two-windows", shared);
var secondWindow = await AnnualNumericTransactions.Open(rules, "two-windows", shared);
await firstWindow.QueueAction(park.siteId, park.actionId, 0);
await Reject<IOException>(() => secondWindow.QueueAction(park.siteId, park.actionId, 0), "Compare-and-replace rejects stale window");
Check(secondWindow.State.queue.Length == 0 && shared.Value!.town.queue.Length == 1, "Stale window does not overwrite the newer save");
await secondWindow.Reload();
Check(secondWindow.State.queue.Length == 1 && secondWindow.State.revision == firstWindow.State.revision, "Stale window can reread the accepted save");

using (var cancellation = new CancellationTokenSource())
{
    var canceledStore = new FakeStore();
    var canceled = await AnnualNumericTransactions.Open(rules, "cancel-after-ack", canceledStore);
    canceledStore.AfterWrite = () => cancellation.Cancel();
    await canceled.QueueAction(park.siteId, park.actionId, 0, cancellation.Token);
    Check(canceled.State.queue.Length == 1, "Cancellation after successful acknowledgement cannot hide the committed candidate");
}
using (var cancellation = new CancellationTokenSource())
{
    cancellation.Cancel(); int count = planning.SaveState.town.queue.Length;
    await Reject<OperationCanceledException>(() => planning.ClearPlan(planning.State.revision, cancellation.Token), "Canceled mutation never acquires the save boundary");
    Check(planning.State.queue.Length == count, "Cancellation leaves visible queue unchanged");
}
using (var cancellation = new CancellationTokenSource())
{
    var blockedStore = new FakeStore();
    var blockedSession = await AnnualNumericTransactions.Open(rules, "cancel-during-save", blockedStore);
    blockedStore.BlockAt = blockedStore.Writes + 2;
    var blockedResult = blockedSession.ResolveYear("cancel-during-save/year-1", 0, cancellation: cancellation.Token);
    await blockedStore.Blocked.Task.WaitAsync(TimeSpan.FromSeconds(10)); cancellation.Cancel();
    await Reject<OperationCanceledException>(() => blockedResult, "Canceled storage wait remains unaccepted");
    Check(blockedSession.State.phase == TownPhase.Resolving && blockedSession.RecoveryRequired, "Cancellation during an uncertain write requires reread");
    await blockedSession.ResolveYear("cancel-during-save/year-1", 0, "local-selected");
    Check(blockedSession.State.history.Length == 1, "Retry after canceled write accepts exactly once");
}

var compatibilityStore = new FakeStore { Value = Copy(lostStore.Value) };
// Unity inline expansion of an absent pending receipt is semantically absent.
compatibilityStore.Value!.narration[1].receipt = new ReportNarrationReceipt();
var expanded = await AnnualNumericTransactions.Open(rules, "lost-ack", compatibilityStore);
Check(expanded.ForYear(2).receipt == null, "Wholly empty pending receipt expansion preserves envelope hash");
compatibilityStore.Value.narration[1].receipt.title = "unexpected partial receipt";
await Reject<ArgumentException>(() => AnnualNumericTransactions.Open(rules, "lost-ack", compatibilityStore), "Partial pending receipt is not erased as empty");
var corrupt = new FakeStore { Value = Copy(failStore.Value) };
corrupt.Value!.narration[0].receipt.cards[0].text = "Invented cause";
corrupt.Value.saveHash = AnnualNumericTransactions.Hash(corrupt.Value);
await Reject<ArgumentException>(() => AnnualNumericTransactions.Open(rules, "save-failure", corrupt), "Rehashed save still rejects unsupported visible prose");

// Complete all remaining numeric years with persisted local receipts, then reopen.
while (session.State.phase != TownPhase.Completed)
{
    if (session.State.phase == TownPhase.Reviewing) await session.Continue(session.State.revision);
    await session.ResolveYear("transaction-main/year-" + session.State.year, session.State.revision, "service-unavailable");
    Check(session.State.history.Length == session.SaveState.narration.Length && Text(session.SaveState) == Text(store.Value), "Every accepted year has its own acknowledged report envelope");
}
var final = await AnnualNumericTransactions.Open(rules, "transaction-main", store);
Check(final.State.history.Length == 10 && final.State.phase == TownPhase.Completed && final.SaveState.narration.All(n => n.status == "visible"), "Complete ten-year state and all visible modes reopen exactly");
Check(Text(final.SaveState) == Text(session.SaveState), "Final envelope is unchanged by reload");
await Reject<InvalidOperationException>(() => final.Continue(final.State.revision), "No Year 11 transaction");
if (args.Contains("--write-evidence"))
{
    Directory.CreateDirectory("docs/evidence/annual-numeric-transactions");
    File.WriteAllText("docs/evidence/annual-numeric-transactions/checks.json", JsonSerializer.Serialize(new { assertions = checks,
        numericYears = 10, finalEnvelopeBytes = System.Text.Encoding.UTF8.GetByteCount(Text(final.SaveState)),
        browserStorage = false, unity = false, liveProviderCalls = 0, store = "in-memory atomic compare-and-replace with injected failures and acknowledgement gates" }, json) + "\n");
}
Console.WriteLine($"Numeric save transactions: {checks} assertions passed; fake acknowledged store, no Unity/browser/provider calls.");

sealed class FakeStore : IAnnualNumericStore
{
    static readonly JsonSerializerOptions Json = new() { IncludeFields = true };
    static AnnualNumericSave Clone(AnnualNumericSave value) => value == null ? null : JsonSerializer.Deserialize<AnnualNumericSave>(JsonSerializer.Serialize(value, Json), Json)!;
    public AnnualNumericSave Value;
    public int Writes, FailBeforeAt = -1, FailAfterAt = -1, BlockAt = -1;
    public readonly TaskCompletionSource Blocked = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Action AfterWrite;
    public Task<AnnualNumericSave> Read(CancellationToken cancellation) { cancellation.ThrowIfCancellationRequested(); return Task.FromResult(Clone(Value)); }
    public async Task Write(string expectedHash, AnnualNumericSave candidate, CancellationToken cancellation)
    {
        int call = ++Writes; candidate = Clone(candidate);
        cancellation.ThrowIfCancellationRequested();
        if (call == BlockAt) { Blocked.TrySetResult(); await Release.Task.WaitAsync(cancellation); }
        if (call == FailBeforeAt) throw new IOException("Simulated quota/storage failure");
        if (Value?.saveHash != expectedHash) throw new IOException("Simulated atomic compare-and-replace conflict");
        Value = candidate; AfterWrite?.Invoke();
        if (call == FailAfterAt) throw new IOException("Simulated lost acknowledgement after commit");
    }
}
