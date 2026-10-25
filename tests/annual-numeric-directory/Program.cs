using System.Text.Json;
using Terra.Contracts;
using Terra.Core;

int checks = 0;
void Check(bool ok, string why) { checks++; if (!ok) throw new Exception(why); }
async Task Reject<T>(Func<Task> action, string why) where T : Exception
{ bool caught = false; try { await action(); } catch (T) { caught = true; } Check(caught, why); }
var weather = new AnnualSimulationWeather(File.ReadAllBytes("simulation-data/weather/coon-valley-v1/weather-pack.json"),
    File.ReadAllBytes("simulation-data/weather/coon-valley-v1/daily-observations.json"), Json.Read<TownWeatherPack>, Json.Read<TownDailyWeather>);
var legacy = Json.Read<AnnualTownDefinition>(File.ReadAllText("docs/evidence/hud-connected/definition.json"));
var rules = AnnualSimulationFactory.Create(legacy, weather, "directory-seed-a");
var alternate = AnnualSimulationFactory.Create(legacy, weather, "directory-seed-b");
string world = rules.worldPackId;
var db = new Database(); var store = new DirectoryStore(db, world);
var directory = await AnnualNumericRunDirectoryTransactions.Open(world, store);
Check(directory.Selection == null && directory.DirectoryHash == null && store.Writes == 0, "Absent directory opens without creating anything");
Check(AnnualNumericRunDirectoryTransactions.KeyFor(world) != AnnualNumericRunDirectoryTransactions.KeyFor("point-reyes-v1"), "Places need separate directory keys");
int events = 0;
directory.Changed += selection => { events++; selection.save.town.cashCents = 0; throw new Exception("subscriber"); };
directory.Changed += selection => Check(selection.save.town.cashCents > 0, "Subscriber receives an independent selection");
store.BlockAck = true;
var first = directory.CreateAndActivate(rules, "run-a", "create-a", null);
await store.Blocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
var duplicatePending = directory.CreateAndActivate(rules, "run-a", "create-a", null);
Check(!duplicatePending.IsCompleted, "Repeated click waits for the same in-flight creation");
Check(directory.Selection == null && events == 0, "Creation does not publish before acknowledgement");
Check(db.Heads.Count == 1 && db.Runs.Count == 1 && db.Definitions.Count == 1, "Atomic creation has all three persisted records");
store.Release.SetResult(); await Task.WhenAll(first, duplicatePending);
Check(directory.Selection.directory.activeRunId == "run-a" && events == 1, "Acknowledgement publishes once despite subscriber failure");
var aHash = directory.DirectoryHash; int writes = store.Writes;
await directory.CreateAndActivate(rules, "run-a", "create-a", null);
Check(store.Writes == writes && events == 1, "Identical create retry does not insert or publish twice");
await Reject<ArgumentException>(() => directory.CreateAndActivate(alternate, "run-a", "create-a", null), "Changed definition cannot reuse accepted operation ID");
var detached = directory.Selection; detached.directory.runs[0].runId = "tampered"; detached.definition.startingCashCents = 0;
Check(directory.Selection.directory.runs[0].runId == "run-a" && directory.Selection.definition.startingCashCents > 0, "Returned selection cannot mutate authority");

// Retain a genuinely accepted year and its narration when a new seed/run is selected.
var aStore = new RunStore(db, world, "run-a");
var annualA = await AnnualNumericTransactions.Open(rules, "run-a", aStore);
var park = rules.actions.First(a => a.actionId == "public-park");
await annualA.QueueAction(park.siteId, park.actionId, 0);
await annualA.ResolveYear("run-a/year-1", annualA.State.revision, "local-selected");
string acceptedA = Json.Text(await aStore.Read(default));
await directory.CreateAndActivate(rules, "run-same-rules", "create-same-rules", aHash);
Check(db.Definitions.Count == 1 && db.Runs.Count == 2, "Same-rule replay reuses immutable definition record");
await directory.CreateAndActivate(alternate, "run-b", "create-b", directory.DirectoryHash);
Check(db.Definitions.Count == 2 && directory.Selection.directory.runs.Length == 3, "Different seed is retained in same place/model directory");
Check(directory.Selection.directory.runs[0].definitionHash != directory.Selection.directory.runs[2].definitionHash, "Directory binds each run to its exact rules");
Check(Json.Text(await aStore.Read(default)) == acceptedA, "Replay creation preserves previous accepted history and narration");
var bHash = directory.DirectoryHash; store.ResetBlock(); store.BlockAck = true;
var selecting = directory.Select("run-a", "select-a", bHash);
await store.Blocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
Check(directory.Selection.directory.activeRunId == "run-b", "Selection waits for pointer acknowledgement");
store.Release.SetResult(); await selecting;
Check(directory.Selection.save.town.history.Length == 1 && directory.Selection.save.narration[0].status == "visible", "Selection restores exact accepted year and narration");
writes = store.Writes; await directory.Select("run-a", "select-a", bHash);
Check(store.Writes == writes, "Selection retry is idempotent");
await Reject<ArgumentException>(() => directory.Select("run-b", "select-a", bHash), "Reused operation with a different target is rejected");
await Reject<ArgumentException>(() => directory.CreateAndActivate(alternate, "run-a", "overwrite-a", directory.DirectoryHash), "Retained run ID cannot be overwritten");
await Reject<ArgumentException>(() => directory.Select("missing", "select-missing", directory.DirectoryHash), "Unknown target cannot become a new save");
await Reject<ArgumentException>(() => directory.CreateAndActivate(legacy, "legacy", "legacy", directory.DirectoryHash), "Legacy rules cannot enter numeric directory");

// Failed writes retain the current view; a lost acknowledgement is recovered by read.
string before = directory.DirectoryHash; store.FailBefore = true;
await Reject<IOException>(() => directory.CreateAndActivate(rules, "run-c", "create-c", before), "Creation failure surfaces");
Check(directory.DirectoryHash == before && directory.RecoveryRequired && !db.Runs.ContainsKey(store.RunKey("run-c")), "Failed creation leaves all prior records intact");
await directory.CreateAndActivate(rules, "run-c", "create-c", before);
Check(directory.Selection.directory.activeRunId == "run-c" && !directory.RecoveryRequired, "Retry after precommit failure recovers and creates once");
before = directory.DirectoryHash; store.FailAfter = true;
await Reject<IOException>(() => directory.CreateAndActivate(alternate, "run-d", "create-d", before), "Lost creation acknowledgement surfaces");
Check(directory.DirectoryHash == before && directory.RecoveryRequired && db.Heads[store.Scope].activeRunId == "run-d", "Unknown outcome is not prematurely published");
writes = store.Writes; store.FailReads = true;
await Reject<IOException>(() => directory.CreateAndActivate(alternate, "run-d", "create-d", before), "Recovery read failure cannot be mistaken for absence");
Check(store.Writes == writes && directory.DirectoryHash == before, "Failed recovery neither writes nor changes selected view");
store.FailReads = false; await directory.CreateAndActivate(alternate, "run-d", "create-d", before);
Check(store.Writes == writes && directory.Selection.directory.activeRunId == "run-d", "Lost-ack creation retry discovers existing result without duplicate write");

before = directory.DirectoryHash; store.FailBefore = true;
await Reject<IOException>(() => directory.Select("run-a", "select-failure", before), "Precommit selection failure surfaces");
Check(directory.DirectoryHash == before && db.Heads[store.Scope].activeRunId == "run-d", "Failed selection preserves prior pointer");
await directory.Select("run-a", "select-failure", before);
Check(directory.Selection.directory.activeRunId == "run-a", "Selection retry can succeed after recovery");
await directory.Select("run-b", "select-b-before-ack", directory.DirectoryHash);
before = directory.DirectoryHash; store.FailAfter = true;
await Reject<IOException>(() => directory.Select("run-a", "select-a-lost", before), "Lost selection acknowledgement surfaces");
await annualA.Continue(annualA.State.revision); // Selected target advances after the committed selection.
writes = store.Writes; await directory.Select("run-a", "select-a-lost", before);
Check(store.Writes == writes && directory.Selection.save.town.year == 2, "Retry keeps same selection intent when target advances after commit");

// A changed target between read and select fails CAS; refreshed retry selects latest save.
await directory.Select("run-b", "select-b-race", directory.DirectoryHash);
before = directory.DirectoryHash;
store.AfterReadRun = async () => await annualA.QueueAction(rules.actions.First(a => a.actionId == "cover-crop" && a.siteId != park.siteId).siteId, "cover-crop", annualA.State.revision);
await Reject<IOException>(() => directory.Select("run-a", "select-target-race", before), "Target save mutation is checked in the selection transaction");
Check(directory.DirectoryHash == before && db.Heads[store.Scope].activeRunId == "run-b", "Stale target read does not change pointer");
await directory.Select("run-a", "select-target-race", before);
Check(directory.Selection.save.town.queue.Length == 1, "Selection retry reads and validates the latest target");

// Stale operation cannot override another tab's newer explicit selection.
await directory.Select("run-b", "select-b-before-stale", directory.DirectoryHash);
before = directory.DirectoryHash; store.FailAfter = true;
await Reject<IOException>(() => directory.Select("run-a", "old-select", before), "Unknown selection outcome prepared");
var other = await AnnualNumericRunDirectoryTransactions.Open(world, new DirectoryStore(db, world));
await other.Select("run-b", "newer-select", other.DirectoryHash);
writes = store.Writes;
await Reject<ArgumentException>(() => directory.Select("run-a", "old-select", before), "Old retry cannot reactivate a run after newer selection");
Check(directory.Selection.directory.activeRunId == "run-b" && store.Writes == writes, "Stale tab recovers current pointer without overwriting it");

// Initial creators race, with the first acknowledgement deliberately pending.
var raceDb = new Database(); var raceStoreA = new DirectoryStore(raceDb, world) { BlockAck = true }; var raceStoreB = new DirectoryStore(raceDb, world);
var raceA = await AnnualNumericRunDirectoryTransactions.Open(world, raceStoreA); var raceB = await AnnualNumericRunDirectoryTransactions.Open(world, raceStoreB);
var racing = raceA.CreateAndActivate(rules, "winner", "race-a", null);
await raceStoreA.Blocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
await Reject<IOException>(() => raceB.CreateAndActivate(alternate, "loser", "race-b", null), "Atomic directory CAS allows one initial creator");
Check(raceDb.Runs.Count == 1 && raceDb.Definitions.Count == 1, "Losing creation leaves no orphan run or definition");
raceStoreA.Release.SetResult(); await racing;
await Reject<ArgumentException>(() => raceB.CreateAndActivate(alternate, "loser", "race-b", null), "Stale creator must acknowledge winning selection before a new choice");
Check(raceB.Selection.directory.activeRunId == "winner", "Losing creator recovers winning selection");

// Freeze the supplied definition at invocation, before waiting behind an in-flight operation.
var gateDb = new Database(); var gateStore = new DirectoryStore(gateDb, world) { BlockAck = true };
var gated = await AnnualNumericRunDirectoryTransactions.Open(world, gateStore);
var gateFirst = gated.CreateAndActivate(rules, "gate-a", "gate-create-a", null);
await gateStore.Blocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
var callerRules = Json.Copy(rules);
var gateSecond = gated.CreateAndActivate(callerRules, "gate-b", "gate-create-b", gateDb.Heads[gateStore.Scope].directoryHash);
callerRules.startingCashCents = 1;
Check(!gateSecond.IsCompleted, "Independent local operation is serialized behind pending acknowledgement");
gateStore.Release.SetResult(); await Task.WhenAll(gateFirst, gateSecond);
Check(gated.Selection.definition.startingCashCents == rules.startingCashCents && gateDb.Runs.Count == 2, "Caller mutation cannot change queued creation rules");

before = directory.DirectoryHash; store.CancelAfter = true;
await Reject<OperationCanceledException>(() => directory.CreateAndActivate(rules, "cancel-after", "cancel-after", before), "Cancellation can have an already committed outcome");
Check(directory.DirectoryHash == before && directory.RecoveryRequired, "Cancellation after commit does not prematurely publish");
writes = store.Writes; await directory.CreateAndActivate(rules, "cancel-after", "cancel-after", before);
Check(store.Writes == writes && directory.Selection.directory.activeRunId == "cancel-after", "Cancelled acknowledgement is recovered without duplicate creation");
await directory.Select("run-b", "return-b", directory.DirectoryHash);

var aDefinitionHash = new AnnualTown(rules, "identity").DefinitionHash;
var aDefinition = Json.Copy(db.Definitions[aDefinitionHash]);
before = directory.DirectoryHash; db.Definitions.Remove(aDefinitionHash); writes = store.Writes;
await Reject<IOException>(() => directory.Select("run-a", "missing-retained-definition", before), "Retained inactive target needs its exact definition");
Check(store.Writes == writes && directory.DirectoryHash == before, "Missing retained target cannot change current selection");
db.Definitions[aDefinitionHash] = Json.Copy(aDefinition);
await directory.Reload();
db.Definitions[aDefinitionHash].startingCashCents++;
int runCount = db.Runs.Count;
await Reject<IOException>(() => directory.CreateAndActivate(rules, "immutable-conflict", "immutable-conflict", directory.DirectoryHash), "Immutable definition conflict aborts complete creation");
Check(db.Runs.Count == runCount && directory.DirectoryHash == before, "Definition conflict leaves no partial run or pointer");
db.Definitions[aDefinitionHash] = Json.Copy(aDefinition); await directory.Reload();

// Retaining/reselecting a full decade does not rewrite any accepted year or prose.
while (annualA.State.phase != TownPhase.Completed)
{
    if (annualA.State.phase == TownPhase.Reviewing) await annualA.Continue(annualA.State.revision);
    await annualA.ResolveYear("run-a/year-" + annualA.State.year, annualA.State.revision, "local-selected");
}
var decade = annualA.SaveState;
await directory.Select("run-a", "select-decade", directory.DirectoryHash);
Check(directory.Selection.save.town.history.Length == 10 && directory.Selection.save.town.phase == TownPhase.Completed, "Selected exact run reaches its completed ten-year state");
for (int y = 0; y < 10; y++)
    Check(directory.Selection.save.town.history[y].simulation.transitionHash == decade.town.history[y].simulation.transitionHash
        && Json.Text(directory.Selection.save.narration[y].receipt) == Json.Text(decade.narration[y].receipt), "Retained decade preserves report and narration identity year " + (y+1));
await directory.CreateAndActivate(alternate, "after-decade", "replay-decade", directory.DirectoryHash);
Check(Json.Text(await aStore.Read(default)) == Json.Text(decade), "Replay preserves the complete prior decade byte-for-byte in the fake store");

// Distinct world namespaces can use identical run IDs without sharing state.
var otherRules = Json.Copy(rules); otherRules.worldPackId = "point-reyes-v1"; // Technical identity fixture, not another authored geographic pack.
var elsewhere = await AnnualNumericRunDirectoryTransactions.Open(otherRules.worldPackId, new DirectoryStore(db, otherRules.worldPackId));
await elsewhere.CreateAndActivate(otherRules, "run-a", "other-place-a", null);
Check(elsewhere.Selection.definition.worldPackId == otherRules.worldPackId && directory.Selection.definition.worldPackId == world, "Place isolation preserves exact definitions");
Check(Json.Text(await aStore.Read(default)) == Json.Text(annualA.SaveState), "Other place never rewrites existing run with the same ID");
await Reject<ArgumentException>(() => directory.CreateAndActivate(otherRules, "wrong-place", "wrong-place", directory.DirectoryHash), "Bound directory rejects rules from another place");

// Corrupt/missing references cannot be interpreted as a fresh run or silently reset.
async Task BadRead(Action corrupt, Action repair, string why)
{
 string selected = Json.Text(directory.Selection); int priorWrites = store.Writes; corrupt();
 await Reject<Exception>(() => directory.Reload(), why);
 Check(Json.Text(directory.Selection) == selected && directory.RecoveryRequired && store.Writes == priorWrites, why + ": prior view preserved, no writes");
 repair(); await directory.Reload(); Check(!directory.RecoveryRequired, why + ": explicit recovery succeeds");
}
var active = directory.Selection; var activeDefHash = active.save.town.definitionHash; var definitionBackup = Json.Copy(db.Definitions[activeDefHash]);
await BadRead(() => db.Definitions.Remove(activeDefHash), () => db.Definitions[activeDefHash] = Json.Copy(definitionBackup), "Missing exact definition");
await BadRead(() => db.Definitions[activeDefHash].startingCashCents++, () => db.Definitions[activeDefHash] = Json.Copy(definitionBackup), "Corrupt exact definition");
string activeKey = store.RunKey(active.directory.activeRunId); var runBackup = Json.Copy(db.Runs[activeKey]);
await BadRead(() => db.Runs.Remove(activeKey), () => db.Runs[activeKey] = Json.Copy(runBackup), "Missing indexed run");
await BadRead(() => db.Runs[activeKey].town.cashCents++, () => db.Runs[activeKey] = Json.Copy(runBackup), "Changed numeric envelope");
var headBackup = Json.Copy(db.Heads[store.Scope]);
var currentRunStore = new RunStore(db, world, active.directory.activeRunId);
var currentAnnual = await AnnualNumericTransactions.Open(active.definition, active.directory.activeRunId, currentRunStore);
await currentAnnual.QueueAction(park.siteId, park.actionId, currentAnnual.State.revision);
await directory.Reload();
var newerRun = currentAnnual.SaveState;
await BadRead(() => db.Runs[activeKey] = Json.Copy(runBackup), () => db.Runs[activeKey] = Json.Copy(newerRun), "Known active run sequence rollback with unchanged directory");
var conflictingRun = Json.Copy(runBackup); conflictingRun.sequence = newerRun.sequence;
conflictingRun.saveHash = AnnualNumericTransactions.Hash(conflictingRun);
await BadRead(() => db.Runs[activeKey] = Json.Copy(conflictingRun), () => db.Runs[activeKey] = Json.Copy(newerRun), "Known active run changed at the same save sequence");
await BadRead(() => db.Heads.Remove(store.Scope), () => db.Heads[store.Scope] = Json.Copy(headBackup), "Previously known directory disappears");
await BadRead(() => db.Heads[store.Scope].directoryHash = new string('0',64), () => db.Heads[store.Scope] = Json.Copy(headBackup), "Invalid directory hash");
await BadRead(() => { var h = db.Heads[store.Scope]; h.runs[1].runId = h.runs[0].runId; h.directoryHash = AnnualNumericRunDirectoryTransactions.Hash(h); }, () => db.Heads[store.Scope] = Json.Copy(headBackup), "Duplicate run identity");
await BadRead(() => { var h = db.Heads[store.Scope]; h.activeRunId = "unknown"; h.directoryHash = AnnualNumericRunDirectoryTransactions.Hash(h); }, () => db.Heads[store.Scope] = Json.Copy(headBackup), "Unknown active run");
await BadRead(() => { var h = db.Heads[store.Scope]; h.worldPackId = "elsewhere"; h.directoryHash = AnnualNumericRunDirectoryTransactions.Hash(h); }, () => db.Heads[store.Scope] = Json.Copy(headBackup), "Wrong directory scope");
await BadRead(() => { var h = db.Heads[store.Scope]; h.sequence--; h.directoryHash = AnnualNumericRunDirectoryTransactions.Hash(h); }, () => db.Heads[store.Scope] = Json.Copy(headBackup), "Directory sequence rollback");
await BadRead(() => { var h = db.Heads[store.Scope]; h.operationId = "changed-without-sequence"; h.directoryHash = AnnualNumericRunDirectoryTransactions.Hash(h); }, () => db.Heads[store.Scope] = Json.Copy(headBackup), "Changed directory at same sequence");

var cancelled = new CancellationToken(true); writes = store.Writes;
await Reject<OperationCanceledException>(() => directory.CreateAndActivate(rules, "cancelled", "cancelled", directory.DirectoryHash, cancelled), "Cancelled request never dispatches");
Check(store.Writes == writes && !db.Runs.ContainsKey(store.RunKey("cancelled")), "Cancellation before dispatch creates no records");
Check(db.Runs.ContainsKey(store.RunKey("run-a")) && db.Runs.ContainsKey(store.RunKey("run-b")) && db.Runs.ContainsKey(store.RunKey("run-c")) && db.Runs.ContainsKey(store.RunKey("run-d")), "All prior runs retained after selection and recovery failures");
Console.WriteLine("Annual numeric directory: " + checks + " assertions passed; fake atomic store, no runtime activation.");

static class Json
{
    static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };
    public static T Read<T>(string text) => JsonSerializer.Deserialize<T>(text, Options)!;
    public static string Text<T>(T value) => JsonSerializer.Serialize(value, Options);
    public static T Copy<T>(T value) => Read<T>(Text(value));
}
sealed class Database
{
    public readonly object Gate = new object();
    public readonly Dictionary<string, AnnualNumericRunDirectory> Heads = new();
    public readonly Dictionary<string, AnnualTownDefinition> Definitions = new();
    public readonly Dictionary<string, AnnualNumericSave> Runs = new();
}
sealed class DirectoryStore : IAnnualNumericRunDirectoryStore
{
    readonly Database db; readonly string world;
    public string Scope { get; }
    public int Writes;
    public bool FailBefore, FailAfter, FailReads, BlockAck, CancelAfter;
    public TaskCompletionSource Blocked = new(TaskCreationOptions.RunContinuationsAsynchronously), Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Func<Task> AfterReadRun;
    public DirectoryStore(Database database, string world) { db = database; this.world = world; Scope = AnnualNumericRunDirectoryTransactions.KeyFor(world); }
    public string RunKey(string run) => Scope + "/" + run;
    public void ResetBlock() { Blocked = new(TaskCreationOptions.RunContinuationsAsynchronously); Release = new(TaskCreationOptions.RunContinuationsAsynchronously); }
    public Task<AnnualNumericRunSelection> Read(CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested(); if (FailReads) throw new IOException("Read unavailable");
        lock(db.Gate)
        {
            if (!db.Heads.TryGetValue(Scope, out var head)) return Task.FromResult<AnnualNumericRunSelection>(null);
            var content = Content(head.activeRunId);
            return Task.FromResult(new AnnualNumericRunSelection { directory = Json.Copy(head), definition = content.definition, save = content.save });
        }
    }
    public async Task<AnnualNumericRunContent> ReadRun(string runId, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested(); if (FailReads) throw new IOException("Read unavailable");
        AnnualNumericRunContent result; lock(db.Gate) result = Content(runId);
        var callback = AfterReadRun; AfterReadRun = null; if (callback != null) await callback();
        return result;
    }
    AnnualNumericRunContent Content(string runId)
    {
        if (!db.Heads.TryGetValue(Scope, out var head)) throw new IOException("Missing directory");
        var entry = head.runs.SingleOrDefault(r => r.runId == runId);
        if (entry == null || !db.Definitions.TryGetValue(entry.definitionHash, out var definition) || !db.Runs.TryGetValue(RunKey(runId), out var run)) throw new IOException("Missing indexed content");
        return new AnnualNumericRunContent { definition = Json.Copy(definition), save = Json.Copy(run) };
    }
    public async Task CreateAndActivate(string expected, AnnualNumericRunDirectory candidate, AnnualTownDefinition definition, AnnualNumericSave initial, CancellationToken cancellation)
    {
        Begin(cancellation);
        lock(db.Gate)
        {
            var old = Head(expected, candidate);
            var engine = new AnnualTown(definition, initial.town.runId);
            if (definition.worldPackId != world || initial.sequence != 1 || initial.narration.Length != 0 || initial.saveHash != AnnualNumericTransactions.Hash(initial)
                || Json.Text(engine.Snapshot()) != Json.Text(initial.town) || candidate.activeRunId != initial.town.runId
                || candidate.runs.Length != (old?.runs.Length ?? 0) + 1 || Json.Text(candidate.runs.Take(candidate.runs.Length - 1).ToArray()) != Json.Text(old?.runs ?? Array.Empty<AnnualNumericRunEntry>())
                || candidate.runs.Last().runId != initial.town.runId || candidate.runs.Last().definitionHash != engine.DefinitionHash || db.Runs.ContainsKey(RunKey(initial.town.runId))) throw new IOException("Invalid atomic creation");
            if (db.Definitions.TryGetValue(engine.DefinitionHash, out var existing) && new AnnualTown(existing,"verify-definition").DefinitionHash != engine.DefinitionHash) throw new IOException("Immutable definition conflict");
            db.Definitions.TryAdd(engine.DefinitionHash, Json.Copy(definition)); db.Runs.Add(RunKey(initial.town.runId), Json.Copy(initial)); db.Heads[Scope] = Json.Copy(candidate);
        }
        await Acknowledge();
    }
    public async Task Select(string expected, AnnualNumericRunDirectory candidate, string targetHash, CancellationToken cancellation)
    {
        Begin(cancellation);
        lock(db.Gate)
        {
            var old = Head(expected, candidate); var target = Content(candidate.activeRunId);
            if (old == null || Json.Text(old.runs) != Json.Text(candidate.runs) || target.save.saveHash != targetHash) throw new IOException("Target save changed");
            db.Heads[Scope] = Json.Copy(candidate);
        }
        await Acknowledge();
    }
    AnnualNumericRunDirectory Head(string expected, AnnualNumericRunDirectory candidate)
    {
        db.Heads.TryGetValue(Scope, out var old);
        if (old?.directoryHash != expected || candidate.sequence != (old?.sequence ?? 0) + 1 || candidate.directoryHash != AnnualNumericRunDirectoryTransactions.Hash(candidate)
            || candidate.worldPackId != world || candidate.modelFamily != AnnualNumericRunDirectoryTransactions.ModelFamily) throw new IOException("Directory conflict");
        return old;
    }
    void Begin(CancellationToken cancellation) { cancellation.ThrowIfCancellationRequested(); Writes++; if (FailBefore) { FailBefore = false; throw new IOException("Before commit"); } }
    async Task Acknowledge()
    {
        if (FailAfter) { FailAfter = false; throw new IOException("Lost acknowledgement"); }
        if (CancelAfter) { CancelAfter = false; throw new OperationCanceledException("Transport cancelled after commit"); }
        if (BlockAck) { BlockAck = false; Blocked.SetResult(); await Release.Task; }
    }
}
sealed class RunStore : IAnnualNumericStore
{
    readonly Database db; readonly string key;
    public RunStore(Database db, string world, string run) { this.db = db; key = AnnualNumericRunDirectoryTransactions.KeyFor(world) + "/" + run; }
    public Task<AnnualNumericSave> Read(CancellationToken cancellation) { cancellation.ThrowIfCancellationRequested(); lock(db.Gate) return Task.FromResult(db.Runs.TryGetValue(key, out var value) ? Json.Copy(value) : null); }
    public Task Write(string expectedHash, AnnualNumericSave candidate, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested(); lock(db.Gate) { db.Runs.TryGetValue(key, out var old); if(old?.saveHash != expectedHash) throw new IOException("Run conflict"); db.Runs[key] = Json.Copy(candidate); } return Task.CompletedTask;
    }
}
