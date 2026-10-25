using System;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;

namespace Terra.Gameplay
{
    /// <summary>
    /// Optional binding for the engine owner. No scene/HUD is changed automatically.
    /// Call Initialize after loading the map, then bind buttons to these transactions.
    /// </summary>
    public sealed class AnnualTownSession : MonoBehaviour
    {
        public string saveKey = "terra.annual-town.v1";
        AnnualTown town;
        public event Action<AnnualTownSnapshot> Changed;
        public bool Initialized => town != null;
        public AnnualTownSnapshot State => RequireTown().Snapshot();
        public long AvailableCents => RequireTown().AvailableCents;
        public long CommittedCents => RequireTown().CommittedCents;
        public long QueuedCommitmentCents => RequireTown().QueuedCommitmentCents;
        public AnnualTown Engine => RequireTown();
        public bool CanRedo => RequireTown().CanRedo;
        public bool CanUndo => RequireTown().CanUndo;
        public AnnualTownSummary LatestSummary => AnnualTownSummary.Latest(RequireTown());
        public long Preview(string siteId, string actionId) => RequireTown().Preview(siteId, actionId);

        public void Initialize(AnnualTownDefinition definition, string runId, bool restoreSaved = false)
        {
            var next = new AnnualTown(definition, runId);
            if (restoreSaved && PlayerPrefs.HasKey(saveKey))
                next = AnnualTown.Restore(definition, JsonUtility.FromJson<AnnualTownSnapshot>(PlayerPrefs.GetString(saveKey)));
            town = next; Publish();
        }
        public void InitializeFromWorldPack(WorldData world, Scenario scenario, string packId, string runId, bool restoreSaved = false)
            => Initialize(AnnualTownDemo.FromWorldPack(world, scenario, packId), runId, restoreSaved);

        public void QueueAction(string siteId, string actionId, int expectedRevision)
        { RequireTown().QueueAction(siteId, actionId, expectedRevision); Publish(); }
        public void RemoveQueuedAction(string siteId, int expectedRevision)
        { RequireTown().RemoveQueuedAction(siteId, expectedRevision); Publish(); }
        public void UndoPlan(int expectedRevision)
        { RequireTown().UndoPlan(expectedRevision); Publish(); }
        public void RedoPlan(int revision) { RequireTown().RedoPlan(revision); Publish(); }
        public void ClearPlan(int revision) { RequireTown().ClearPlan(revision); Publish(); }
        public void MovePlan(string source, string destination, int revision) { RequireTown().MoveQueuedAction(source, destination, revision); Publish(); }
        public TownYearRequest BeginYear(string requestId, int expectedRevision)
        { var request = RequireTown().BeginResolution(requestId, expectedRevision); Publish(); return request; }
        public TownYearReport CompleteYear(TownYearConsequences consequences)
        { var result = RequireTown().CompleteResolution(consequences); Publish(); return result; }
        public void CancelResolution(int expectedRevision)
        { RequireTown().CancelResolution(expectedRevision); Publish(); }
        public void ContinueYear(int expectedRevision)
        { RequireTown().ContinuePlanning(expectedRevision); Publish(); }

        // TODO(simulation service): call BeginYear once, pass its frozen input to an async provider,
        // validate its bounded structured output through CompleteYear, then display LatestSummary.
        // Keep retries on the same request ID. Money, lifecycle and scores remain engine-owned.
        // Explicit offline placeholder: no LLM/network call or scientific prediction is made here.
        public TownYearReport EndYearOffline(string requestId)
        {
            var current = RequireTown();
            var committed = current.History.FirstOrDefault(r => r.acceptedRequest.requestId == requestId);
            if (committed != null) return committed;
            var request = BeginYear(requestId, current.Revision);
            return CompleteYear(new AuthoredAnnualConsequenceProvider().Resolve(request));
        }
        public string ExportSave() => JsonUtility.ToJson(RequireTown().Snapshot(), true);
        public void FinishOffline()
        {
            var current = RequireTown();
            while (current.Phase != TownPhase.Completed)
            {
                if (current.Phase == TownPhase.Reviewing) ContinueYear(current.Revision);
                string id = current.PendingRequest?.requestId ?? current.RunId + "/finish-year-" + current.Year;
                EndYearOffline(id);
            }
        }
        public void ImportSave(string json)
        {
            var next = AnnualTown.Restore(RequireTown().Definition, JsonUtility.FromJson<AnnualTownSnapshot>(json));
            town = next; Publish();
        }
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(saveKey)) throw new InvalidOperationException("Set a run-specific save key before saving.");
            PlayerPrefs.SetString(saveKey, ExportSave()); PlayerPrefs.Save();
        }
        AnnualTown RequireTown() => town ?? throw new InvalidOperationException("Initialize the annual town before binding controls.");
        void Publish()
        {
            // Consumers receive detached snapshots. A listener failure cannot undo a committed year.
            if (Changed == null) return;
            foreach (Action<AnnualTownSnapshot> listener in Changed.GetInvocationList())
            {
                try { listener(town.Snapshot()); }
                catch (Exception error) { Debug.LogException(error, this); }
            }
        }
    }
}
