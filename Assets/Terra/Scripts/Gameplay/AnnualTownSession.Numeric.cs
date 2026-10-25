using System;
using System.Threading;
using System.Threading.Tasks;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;

namespace Terra.Gameplay
{
    public sealed partial class AnnualTownSession
    {
        AnnualNumericTransactions numeric;
        bool numericOpening, destroyed;
        public bool NumericActive => numeric != null;
        public Exception NumericNarrationError => numeric?.LastNarrationError;
        public bool NumericRecoveryRequired => numeric?.RecoveryRequired ?? false;
        public AnnualNumericSave NumericSaveState => RequireNumeric().SaveState;
        public event Action<AnnualNumericSave> NumericStateChanged;

        // Explicit opt-in. The owner supplies schema-3 rules and a store bound to
        // this location/definition/run. No existing PlayerPrefs key is migrated.
        public async Task InitializeNumericAsync(AnnualTownDefinition definition, string runId,
            IAnnualNumericStore store, CancellationToken cancellation = default)
        {
            RequireLegacyMutation();
            numericOpening = true;
            try
            {
                var opened = await AnnualNumericTransactions.Open(definition, runId, store, cancellation);
                if (destroyed) throw new ObjectDisposedException(nameof(AnnualTownSession));
                // Build the read model before replacing the current session.
                var view = opened.ReadEngine();
                numeric = opened; town = view;
                numeric.Changed += NumericCommitted;
                NumericCommitted(numeric.SaveState);
            }
            finally { numericOpening = false; }
        }
        public Task QueueActionAsync(string site, string action, int revision, CancellationToken cancellation = default)
            => RequireNumeric().QueueAction(site, action, revision, cancellation);
        public Task RemoveQueuedActionAsync(string site, int revision, CancellationToken cancellation = default)
            => RequireNumeric().RemoveQueuedAction(site, revision, cancellation);
        public Task MovePlanAsync(string source, string destination, int revision, CancellationToken cancellation = default)
            => RequireNumeric().MovePlan(source, destination, revision, cancellation);
        public Task UndoPlanAsync(int revision, CancellationToken cancellation = default) => RequireNumeric().UndoPlan(revision, cancellation);
        public Task RedoPlanAsync(int revision, CancellationToken cancellation = default) => RequireNumeric().RedoPlan(revision, cancellation);
        public Task ClearPlanAsync(int revision, CancellationToken cancellation = default) => RequireNumeric().ClearPlan(revision, cancellation);
        public Task ContinueYearAsync(int revision, CancellationToken cancellation = default) => RequireNumeric().Continue(revision, cancellation);
        public Task CancelResolutionAsync(int revision, CancellationToken cancellation = default) => RequireNumeric().CancelResolution(revision, cancellation);
        public Task<TownYearReport> ResolveNumericYearAsync(string requestId, int revision, string localReason = null,
            string modelVersion = ReportNarration.DefaultModelVersion, CancellationToken cancellation = default)
            => RequireNumeric().ResolveYear(requestId, revision, localReason, modelVersion, cancellation);
        public Task PrepareNumericNarrationAsync(int year, string localReason = null,
            string modelVersion = ReportNarration.DefaultModelVersion, CancellationToken cancellation = default)
            => RequireNumeric().PrepareNarration(year, localReason, modelVersion, cancellation);
        public Task<bool> AcceptNumericNarrationAsync(ReportNarrationRequest request, ReportNarrationReceipt receipt, CancellationToken cancellation = default)
            => RequireNumeric().AcceptNarration(request, receipt, cancellation);
        public Task<bool> UseLocalNumericNarrationAsync(ReportNarrationRequest request, string reason, CancellationToken cancellation = default)
            => RequireNumeric().UseLocal(request, reason, cancellation);
        public Task ReloadNumericAsync(CancellationToken cancellation = default) => RequireNumeric().Reload(cancellation);
        public AnnualNumericNarration NumericNarrationForYear(int year) => RequireNumeric().ForYear(year);
        public string ExportNumericSave() => AnnualNumericJson.WriteSave(RequireNumeric().SaveState);

        AnnualNumericTransactions RequireNumeric()
        {
            if (destroyed) throw new ObjectDisposedException(nameof(AnnualTownSession));
            return numeric ?? throw new InvalidOperationException("Initialize numeric storage before using asynchronous controls.");
        }
        void RequireLegacyMutation()
        {
            if (destroyed) throw new ObjectDisposedException(nameof(AnnualTownSession));
            if (numeric != null || numericOpening)
                throw new InvalidOperationException("Numeric runs require acknowledged asynchronous transactions.");
        }
        void NumericCommitted(AnnualNumericSave value)
        {
            if (destroyed) return;
            town = numeric.ReadEngine();
            Publish();
            if (NumericStateChanged == null) return;
            foreach (Action<AnnualNumericSave> listener in NumericStateChanged.GetInvocationList())
                try { listener(numeric.SaveState); } catch (Exception error) { Debug.LogException(error, this); }
        }
        void OnDestroy()
        {
            destroyed = true;
            if (numeric != null) numeric.Changed -= NumericCommitted;
            // An in-flight store may still commit. Reopen its same key to discover
            // the outcome; teardown is never represented as a rollback.
        }
    }
}
