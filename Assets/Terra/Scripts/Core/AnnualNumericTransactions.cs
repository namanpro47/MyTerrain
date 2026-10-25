using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terra.Contracts;

namespace Terra.Core
{
    // One store instance addresses one explicit location/run save key. Write must
    // atomically compare expectedHash (null means absent) and replace the whole
    // envelope. Complete its Task only after storage transaction acknowledgement.
    // An exception can mean an unknown outcome; the controller rereads before retry.
    public interface IAnnualNumericStore
    {
        Task<AnnualNumericSave> Read(CancellationToken cancellation);
        Task Write(string expectedHash, AnnualNumericSave candidate, CancellationToken cancellation);
    }

    // AnnualTown remains the sole numeric authority. Mutations operate on detached
    // replayed candidates; only an acknowledged saved envelope becomes observable.
    public sealed class AnnualNumericTransactions
    {
        readonly AnnualTownDefinition definition;
        readonly IAnnualNumericStore store;
        readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
        AnnualNumericSave saved;
        bool recoveryRequired;
        public event Action<AnnualNumericSave> Changed;
        public bool RecoveryRequired => recoveryRequired;
        public Exception LastNarrationError { get; private set; }
        public AnnualNumericSave SaveState => Copy(saved);
        public AnnualTownSnapshot State => TownCopies.Copy(saved.town);
        public AnnualTownDefinition Definition => TownCopies.Copy(definition);
        // UI queries may use this detached engine; mutating it cannot bypass saving.
        public AnnualTown ReadEngine() => AnnualTown.Restore(definition, saved.town);
        public AnnualNumericNarration ForYear(int year)
        {
            var index = Array.FindIndex(saved.town.history, report => report.year == year);
            return index < 0 ? null : TownCopies.Copy(saved.narration[index]);
        }

        AnnualNumericTransactions(AnnualTownDefinition rules, IAnnualNumericStore persistence)
        {
            AnnualTown.ValidateDefinition(rules);
            Require(rules.schemaVersion == 3 && persistence != null, "Numeric transactions require schema 3 and an acknowledged store.");
            definition = TownCopies.Copy(rules); store = persistence;
        }

        public static async Task<AnnualNumericTransactions> Open(AnnualTownDefinition rules, string runId,
            IAnnualNumericStore store, CancellationToken cancellation = default)
        {
            var session = new AnnualNumericTransactions(rules, store);
            var existing = await store.Read(cancellation);
            if (existing != null)
            {
                existing = session.Validate(existing);
                Require(existing.town.runId == runId, "This store contains another run; choose a separate save key.");
                session.saved = existing;
                return session;
            }
            var candidate = new AnnualNumericSave { sequence = 1, town = new AnnualTown(session.definition, runId).Snapshot() };
            candidate.saveHash = Hash(candidate);
            session.Validate(candidate);
            // No live session is returned if creation is unacknowledged. A later Open
            // can discover an already committed result after a lost acknowledgement.
            await store.Write(null, Copy(candidate), cancellation);
            session.saved = candidate;
            return session;
        }

        public Task QueueAction(string site, string action, int revision, CancellationToken cancellation = default)
            => Change(t => t.QueueAction(site, action, revision), cancellation);
        public Task RemoveQueuedAction(string site, int revision, CancellationToken cancellation = default)
            => Change(t => t.RemoveQueuedAction(site, revision), cancellation);
        public Task MovePlan(string source, string destination, int revision, CancellationToken cancellation = default)
            => Change(t => t.MoveQueuedAction(source, destination, revision), cancellation);
        public Task ClearPlan(int revision, CancellationToken cancellation = default) => Change(t => t.ClearPlan(revision), cancellation);
        public Task UndoPlan(int revision, CancellationToken cancellation = default) => Change(t => t.UndoPlan(revision), cancellation);
        public Task RedoPlan(int revision, CancellationToken cancellation = default) => Change(t => t.RedoPlan(revision), cancellation);
        public Task Continue(int revision, CancellationToken cancellation = default) => Change(t => t.ContinuePlanning(revision), cancellation);
        public Task CancelResolution(int revision, CancellationToken cancellation = default) => Change(t => t.CancelResolution(revision), cancellation);

        async Task Change(Action<AnnualTown> command, CancellationToken cancellation)
        {
            await gate.WaitAsync(cancellation);
            try
            {
                await Recover(cancellation);
                var candidate = Copy(saved); var town = AnnualTown.Restore(definition, candidate.town);
                command(town); candidate.town = town.Snapshot();
                await Persist(candidate, cancellation);
            }
            finally { gate.Release(); }
        }

        public async Task<TownYearReport> ResolveYear(string requestId, int revision,
            string localReason = null, string modelVersion = ReportNarration.DefaultModelVersion, CancellationToken cancellation = default)
        {
            await gate.WaitAsync(cancellation);
            try
            {
                await Recover(cancellation);
                var town = AnnualTown.Restore(definition, saved.town);
                var accepted = town.History.FirstOrDefault(r => r.acceptedRequest.requestId == requestId);
                if (accepted != null)
                {
                    Require(accepted.acceptedRequest.revision == revision, "Committed request ID reused with another revision.");
                    await TryPrepare(accepted.year, localReason, modelVersion, cancellation);
                    return accepted;
                }
                var pending = town.BeginResolution(requestId, revision);
                if (saved.town.phase != TownPhase.Resolving)
                {
                    var frozen = Copy(saved); frozen.town = town.Snapshot();
                    await Persist(frozen, cancellation); // Retryable input is saved before numeric work.
                }
                cancellation.ThrowIfCancellationRequested();
                town = AnnualTown.Restore(definition, saved.town);
                var report = town.CompleteResolution(new AuthoredAnnualConsequenceProvider().Resolve(pending));
                var complete = Copy(saved); complete.town = town.Snapshot();
                // Numeric acceptance is durable before optional projection or prose.
                // Position binds this pending entry to the replay-validated history.
                complete.narration = complete.narration.Concat(new[] { new AnnualNumericNarration { status = "unprojected" } }).ToArray();
                await Persist(complete, cancellation);
                await TryPrepare(report.year, localReason, modelVersion, cancellation);
                return TownCopies.Copy(report);
            }
            finally { gate.Release(); }
        }

        // Retry projection independently after numeric acceptance. Unlike ResolveYear,
        // this explicit presentation operation reports its own failure to its caller.
        public async Task PrepareNarration(int year, string localReason = null,
            string modelVersion = ReportNarration.DefaultModelVersion, CancellationToken cancellation = default)
        {
            await gate.WaitAsync(cancellation);
            try { await Recover(cancellation); await Prepare(year, localReason, modelVersion, cancellation); LastNarrationError = null; }
            catch (Exception error) { LastNarrationError = error; throw; }
            finally { gate.Release(); }
        }
        async Task TryPrepare(int year, string localReason, string modelVersion, CancellationToken cancellation)
        {
            try { await Prepare(year, localReason, modelVersion, cancellation); LastNarrationError = null; }
            catch (Exception error) { LastNarrationError = error; } // Numeric result is already acknowledged.
        }
        async Task Prepare(int year, string localReason, string modelVersion, CancellationToken cancellation)
        {
            var index = Array.FindIndex(saved.town.history, report => report.year == year);
            Require(index >= 0, "Narration requires an accepted numeric year.");
            if (saved.narration[index].status != "unprojected") return;
            cancellation.ThrowIfCancellationRequested();
            var report = saved.town.history[index];
            var request = AnnualReportProjection.Build(definition, report, index == 0 ? null : saved.town.history[index - 1], modelVersion);
            var entry = new AnnualNumericNarration { status = "pending", request = request };
            if (localReason != null) { entry.receipt = ReportNarration.Local(request, localReason); entry.status = "visible"; }
            var projected = Copy(saved); projected.narration[index] = entry;
            await Persist(projected, cancellation);
        }

        public Task<bool> AcceptNarration(ReportNarrationRequest request, ReportNarrationReceipt receipt, CancellationToken cancellation = default)
            => Narrate(request, receipt, null, cancellation);
        public Task<bool> UseLocal(ReportNarrationRequest request, string reason, CancellationToken cancellation = default)
            => Narrate(request, null, reason, cancellation);

        async Task<bool> Narrate(ReportNarrationRequest input, ReportNarrationReceipt incoming, string localReason, CancellationToken cancellation)
        {
            // Freeze caller-owned data before waiting behind another save.
            var request = ReportNarration.CopyRequest(input); var receipt = TownCopies.Copy(incoming);
            await gate.WaitAsync(cancellation);
            try
            {
                await Recover(cancellation);
                var existing = saved.narration.SingleOrDefault(n => n.request != null && n.request.acceptedTransitionId == request.acceptedTransitionId
                    && n.request.acceptedTransitionHash == request.acceptedTransitionHash);
                if (existing == null || existing.status == "visible") return false; // Old run or a committed first mode.
                Require(ReportNarration.RequestHash(existing.request) == ReportNarration.RequestHash(request), "Narration projection changed.");
                receipt = localReason == null ? receipt : ReportNarration.Local(existing.request, localReason);
                ReportNarration.ValidateReceipt(existing.request, receipt);
                var candidate = Copy(saved);
                var target = candidate.narration.Single(n => n.request != null && n.request.acceptedTransitionId == request.acceptedTransitionId);
                target.receipt = receipt; target.status = "visible";
                await Persist(candidate, cancellation);
                return true;
            }
            finally { gate.Release(); }
        }

        public async Task Reload(CancellationToken cancellation = default)
        {
            await gate.WaitAsync(cancellation);
            try { recoveryRequired = true; await Recover(cancellation); }
            finally { gate.Release(); }
        }
        async Task Recover(CancellationToken cancellation)
        {
            if (!recoveryRequired) return;
            var value = Validate(await store.Read(cancellation));
            Require(value.town.runId == saved.town.runId && value.sequence >= saved.sequence,
                "Stored run was replaced or rolled back; do not overwrite it.");
            Require(value.sequence != saved.sequence || value.saveHash == saved.saveHash, "Stored sequence has conflicting contents.");
            bool changed = value.saveHash != saved.saveHash;
            saved = value; recoveryRequired = false;
            if (changed) Publish();
        }
        async Task Persist(AnnualNumericSave candidate, CancellationToken cancellation)
        {
            candidate.sequence = checked(saved.sequence + 1);
            candidate.saveHash = Hash(candidate);
            candidate = Validate(candidate);
            cancellation.ThrowIfCancellationRequested();
            try { await store.Write(saved.saveHash, Copy(candidate), cancellation); }
            catch { recoveryRequired = true; throw; }
            // Do not throw for cancellation after a successful acknowledgement: the
            // committed candidate must become visible exactly once.
            saved = candidate; Publish();
        }
        void Publish()
        {
            if (Changed == null) return;
            foreach (Action<AnnualNumericSave> listener in Changed.GetInvocationList())
                try { listener(Copy(saved)); } catch { /* A UI listener cannot undo an acknowledged save. */ }
        }

        AnnualNumericSave Validate(AnnualNumericSave input)
        {
            var value = Copy(input);
            Require(value != null && value.schemaVersion == 1 && value.sequence >= 1 && value.town != null
                && value.narration != null && value.saveHash == Hash(value), "Incomplete or changed numeric save envelope.");
            var town = AnnualTown.Restore(definition, value.town); var history = town.History;
            Require(value.narration.Length == history.Length, "Each accepted year needs its own narration state.");
            for (int i = 0; i < history.Length; i++)
            {
                var item = value.narration[i];
                Require(item != null && (item.status == "unprojected" || item.status == "pending" || item.status == "visible"), "Unknown narration state.");
                if (item.status == "unprojected")
                {
                    Require(item.request == null && item.receipt == null, "Unprojected narration must not contain presentation data.");
                    continue;
                }
                Require(item.request != null, "Projected narration needs a canonical request.");
                var projected = AnnualReportProjection.Build(definition, history[i], i == 0 ? null : history[i - 1], item.request.modelVersion);
                Require(ReportNarration.RequestHash(item.request) == ReportNarration.RequestHash(projected), "Saved narration does not match accepted numeric facts.");
                if (item.status == "visible") ReportNarration.ValidateReceipt(item.request, item.receipt);
                else Require(item.receipt == null, "Pending narration cannot contain an uncommitted receipt.");
            }
            return value;
        }
        public static string Hash(AnnualNumericSave value)
        {
            Require(value != null, "Missing numeric save.");
            var copy = Copy(value); copy.saveHash = null;
            return TownCopies.Hash(copy);
        }
        static AnnualNumericSave Copy(AnnualNumericSave value)
        {
            var result = TownCopies.Copy(value);
            // Unity may expand a null inline receipt. Only a wholly empty pending
            // receipt is equivalent to absence; populated data must be validated.
            if (result?.narration != null)
                foreach (var item in result.narration)
                    {
                        if ((item?.status == "pending" || item?.status == "unprojected") && EmptyReceipt(item.receipt)) item.receipt = null;
                        if (item?.status == "unprojected" && EmptyRequest(item.request)) item.request = null;
                    }
            return result;
        }
        static bool EmptyRequest(ReportNarrationRequest r)
        {
            if (r == null) return true;
            if ((r.schemaVersion != 0 && r.schemaVersion != 2) || r.year != 0) return false;
            return typeof(ReportNarrationRequest).GetFields().Where(f => f.Name != "schemaVersion" && f.Name != "year").All(f => {
                var value = f.GetValue(r); return value == null || value is string text && text.Length == 0
                    || value is Array array && array.Length == 0;
            });
        }
        static bool EmptyReceipt(ReportNarrationReceipt r)
        {
            if (r == null) return true;
            if (r.schemaVersion != 0 && r.schemaVersion != 2) return false;
            return typeof(ReportNarrationReceipt).GetFields().Where(f => f.Name != "schemaVersion").All(f => {
                var value = f.GetValue(r); return value == null || value is string text && text.Length == 0
                    || value is Array array && array.Length == 0;
            });
        }
        static void Require(bool condition, string message) { if (!condition) throw new ArgumentException(message); }
    }
}
