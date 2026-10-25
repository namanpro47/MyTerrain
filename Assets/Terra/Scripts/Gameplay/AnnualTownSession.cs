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
    public sealed partial class AnnualTownSession : MonoBehaviour
    {
        public string saveKey = "terra.annual-town.v1";
        AnnualTown town;
        public event Action<AnnualTownSnapshot> Changed;
        public bool Initialized => town != null;
        public AnnualTownSnapshot State => RequireTown().Snapshot();
        public AnnualTownDefinition Definition => RequireTown().Definition;
        public long AvailableCents => RequireTown().AvailableCents;
        public long CommittedCents => RequireTown().CommittedCents;
        public long QueuedCommitmentCents => RequireTown().QueuedCommitmentCents;
        public AnnualTown Engine => numeric != null ? numeric.ReadEngine() : RequireTown();
        public bool CanRedo => RequireTown().CanRedo;
        public bool CanUndo => RequireTown().CanUndo;
        public AnnualTownSummary LatestSummary => AnnualTownSummary.Latest(RequireTown());
        public long Preview(string siteId, string actionId) => RequireTown().Preview(siteId, actionId);

        public void Initialize(AnnualTownDefinition definition, string runId, bool restoreSaved = false)
        {
            RequireLegacyMutation();
            if (definition?.schemaVersion == 3) throw new InvalidOperationException("Numeric runs require InitializeNumericAsync and acknowledged storage.");
            var next = new AnnualTown(definition, runId);
            if (restoreSaved && PlayerPrefs.HasKey(saveKey))
                next = AnnualTown.Restore(definition, AnnualNumericJson.ReadTownSave(ReadStored(saveKey)));
            town = next; Publish();
        }
        public void InitializeFromWorldPack(WorldData world, Scenario scenario, string packId, string runId, bool restoreSaved = false)
            => Initialize(AnnualTownDemo.FromWorldPack(world, scenario, packId), runId, restoreSaved);

        public void ConfigurePlanningYears(int years, int expectedRevision)
        { RequireLegacyMutation(); RequireTown().ConfigurePlanningYears(years, expectedRevision); Publish(); }

        public void QueueAction(string siteId, string actionId, int expectedRevision)
        { RequireLegacyMutation(); RequireTown().QueueAction(siteId, actionId, expectedRevision); Publish(); }
        public void RemoveQueuedAction(string siteId, int expectedRevision)
        { RequireLegacyMutation(); RequireTown().RemoveQueuedAction(siteId, expectedRevision); Publish(); }
        public void UndoPlan(int expectedRevision)
        { RequireLegacyMutation(); RequireTown().UndoPlan(expectedRevision); Publish(); }
        public void RedoPlan(int revision) { RequireLegacyMutation(); RequireTown().RedoPlan(revision); Publish(); }
        public void ClearPlan(int revision) { RequireLegacyMutation(); RequireTown().ClearPlan(revision); Publish(); }
        public void MovePlan(string source, string destination, int revision) { RequireLegacyMutation(); RequireTown().MoveQueuedAction(source, destination, revision); Publish(); }
        public TownYearRequest BeginYear(string requestId, int expectedRevision)
        { RequireLegacyMutation(); var request = RequireTown().BeginResolution(requestId, expectedRevision); Publish(); return request; }
        public TownYearReport CompleteYear(TownYearConsequences consequences)
        { RequireLegacyMutation(); var result = RequireTown().CompleteResolution(consequences); Publish(); return result; }
        public void CancelResolution(int expectedRevision)
        { RequireLegacyMutation(); RequireTown().CancelResolution(expectedRevision); Publish(); }
        public void ContinueYear(int expectedRevision)
        { RequireLegacyMutation(); RequireTown().ContinuePlanning(expectedRevision); Publish(); }

        // AnnualChapterSession supplies optional asynchronous chapters around this boundary.
        // A future approved causal pack may add bounded effects; chapter v1 only narrates results.
        // Keep retries on the same request ID. Money, lifecycle and scores remain engine-owned.
        // Explicit offline placeholder: no LLM/network call or scientific prediction is made here.
        public TownYearReport EndYearOffline(string requestId)
        {
            RequireLegacyMutation();
            var current = RequireTown();
            var committed = current.History.FirstOrDefault(r => r.acceptedRequest.requestId == requestId);
            if (committed != null) return committed;
            var request = BeginYear(requestId, current.Revision);
            return CompleteYear(new AuthoredAnnualConsequenceProvider().Resolve(request));
        }
        public string ExportSave()
        { RequireLegacyMutation(); return JsonUtility.ToJson(RequireTown().Snapshot(), true); }
        public void FinishOffline()
        {
            RequireLegacyMutation();
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
            RequireLegacyMutation();
            var next = AnnualTown.Restore(RequireTown().Definition, AnnualNumericJson.ReadTownSave(json));
            town = next; Publish();
        }
        public void Save()
        {
            RequireLegacyMutation();
            if (string.IsNullOrWhiteSpace(saveKey)) throw new InvalidOperationException("Set a run-specific save key before saving.");
            WriteStored(saveKey, ExportSave()); PlayerPrefs.Save();
        }
        // PlayerPrefs WebGL has a small per-origin quota. Keep full accepted history,
        // losslessly compressed; legacy plain JSON saves remain readable.
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern string TerraSaveEncode(string json);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern string TerraSaveDecode(string encoded);
#endif
        internal static void WriteStored(string key, string json)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string encoded = TerraSaveEncode(json);
            if (encoded == null) throw new InvalidOperationException("Unable to compress this browser save.");
            PlayerPrefs.SetString(key, encoded);
#else
            using (var bytes = new System.IO.MemoryStream())
            {
                using (var zip = new System.IO.Compression.GZipStream(bytes, System.IO.Compression.CompressionMode.Compress, true))
                {
                    var data = System.Text.Encoding.UTF8.GetBytes(json);
                    zip.Write(data, 0, data.Length);
                }
                PlayerPrefs.SetString(key, "terra-gzip-v1:" + Convert.ToBase64String(bytes.ToArray()));
            }
#endif
        }
        internal static string ReadStored(string key)
        {
            string value = PlayerPrefs.GetString(key);
            const string prefix = "terra-gzip-v1:";
            if (!value.StartsWith(prefix, StringComparison.Ordinal)) return value;
#if UNITY_WEBGL && !UNITY_EDITOR
            return TerraSaveDecode(value) ?? throw new ArgumentException("The browser save is corrupt or exceeds its supported size.");
#else
            using (var bytes = new System.IO.MemoryStream(Convert.FromBase64String(value.Substring(prefix.Length))))
            using (var zip = new System.IO.Compression.GZipStream(bytes, System.IO.Compression.CompressionMode.Decompress))
            using (var reader = new System.IO.StreamReader(zip, System.Text.Encoding.UTF8)) return reader.ReadToEnd();
#endif
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
