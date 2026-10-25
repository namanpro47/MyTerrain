#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using UnityEngine;

namespace Terra.EditorChecks
{
    // Explicit disposable check. It does not open/save scenes or touch PlayerPrefs.
    public static class AnnualNumericClientChecks
    {
        [Serializable] sealed class Evidence
        { public bool passed; public int assertions; public string unity, definitionHash, finalReportHash, saveHash; }
        sealed class JsonStore : IAnnualNumericStore
        {
            public string json;
            public int writes;
            public Task<AnnualNumericSave> Read(CancellationToken cancellation)
                => Task.FromResult(string.IsNullOrEmpty(json) ? null : AnnualNumericJson.ReadSave(json));
            public Task Write(string expectedHash, AnnualNumericSave candidate, CancellationToken cancellation)
            {
                cancellation.ThrowIfCancellationRequested();
                var current = string.IsNullOrEmpty(json) ? null : AnnualNumericJson.ReadSave(json);
                if (current?.saveHash != expectedHash) throw new IOException("Stale check store");
                json = AnnualNumericJson.WriteSave(candidate); writes++;
                return Task.CompletedTask;
            }
        }
        public static void Run()
        {
            var evidence = new Evidence { unity = Application.unityVersion };
            var host = new GameObject("NumericClientCheck") { hideFlags = HideFlags.HideAndDontSave };
            var second = new GameObject("NumericClientCheckReload") { hideFlags = HideFlags.HideAndDontSave };
            void Check(bool okay, string reason) { evidence.assertions++; if (!okay) throw new Exception(reason); }
            try
            {
                string root = Environment.GetEnvironmentVariable("TERRA_NUMERIC_SOURCE_ROOT") ?? Directory.GetCurrentDirectory();
                string vectors = Path.Combine(root, "Builds/AnnualNumericClientVectors");
                var definition = AnnualNumericJson.ReadDefinition(File.ReadAllText(Path.Combine(vectors, "definition.json")));
                var store = new JsonStore { json = File.ReadAllText(Path.Combine(vectors, "save.json")) };
                var original = AnnualNumericJson.ReadSave(store.json);
                var session = host.AddComponent<AnnualTownSession>();
                session.InitializeNumericAsync(definition, original.town.runId, store).GetAwaiter().GetResult();
                Check(session.State.phase == TownPhase.Completed && session.State.history.Length == 10, "Ten-year .NET save replays through Unity");
                Check(session.NumericSaveState.saveHash == original.saveHash, "Cross-runtime envelope hash");
                Check(session.State.history[9].simulation.transitionHash == original.town.history[9].simulation.transitionHash, "Cross-runtime accepted report hash");
                Check(session.NumericSaveState.narration.All(n => n.status == "visible"), "Every saved visible mode reopens");
                Check(store.writes == 0, "Opening an existing run does not rewrite it");
                bool guard = false; try { session.Save(); } catch (InvalidOperationException) { guard = true; }
                Check(guard, "Legacy save cannot bypass acknowledged storage");
                evidence.definitionHash = session.State.definitionHash;
                evidence.finalReportHash = session.State.history[9].simulation.transitionHash;
                evidence.saveHash = session.NumericSaveState.saveHash;
                // Exercise new exact Unity writes, preserving absent optional DTOs.
                var freshStore = new JsonStore();
                var fresh = second.AddComponent<AnnualTownSession>();
                fresh.InitializeNumericAsync(definition, "unity-client-live", freshStore).GetAwaiter().GetResult();
                var park = definition.actions.First(a => a.actionId == "public-park");
                fresh.QueueActionAsync(park.siteId, park.actionId, fresh.State.revision).GetAwaiter().GetResult();
                Check(freshStore.writes == 2 && fresh.State.queue.Length == 1, "Unity initial and planning saves acknowledged");
                var result = fresh.ResolveNumericYearAsync("unity-client-live/year-1", fresh.State.revision,
                    modelVersion: "invalid model with spaces").GetAwaiter().GetResult();
                Check(fresh.State.history.Length == 1 && fresh.NumericNarrationForYear(1).status == "unprojected", "Projection failure preserves saved numeric year");
                var reopened = AnnualNumericTransactions.Open(definition, "unity-client-live", freshStore).GetAwaiter().GetResult();
                Check(reopened.ForYear(1).request == null, "Exact JSON preserves absent unprojected request");
                reopened.PrepareNarration(1, "local-selected").GetAwaiter().GetResult();
                fresh.ReloadNumericAsync().GetAwaiter().GetResult();
                Check(fresh.NumericNarrationForYear(1).receipt.mode == "local", "Saved local prose reloads through exact Unity adapter");
                Check(fresh.State.history[0].simulation.transitionHash == result.simulation.transitionHash, "Presentation never rewrites numeric result");
                var denseDefinition = AnnualNumericJson.ReadDefinition(File.ReadAllText(Path.Combine(vectors, "dense-definition.json")));
                var denseStore = new JsonStore { json = File.ReadAllText(Path.Combine(vectors, "dense-save.json")) };
                var denseOriginal = AnnualNumericJson.ReadSave(denseStore.json);
                var dense = AnnualNumericTransactions.Open(denseDefinition, denseOriginal.town.runId, denseStore).GetAwaiter().GetResult();
                Check(dense.State.history.Length == 10 && dense.State.phase == TownPhase.Completed, "Dense ten-year .NET envelope replays through Unity");
                Check(dense.SaveState.saveHash == denseOriginal.saveHash, "Dense cross-runtime save hash");
                for (int year = 0; year < 10; year++) Check(dense.State.history[year].simulation.transitionHash == denseOriginal.town.history[year].simulation.transitionHash, "Dense exact report year " + (year + 1));
                evidence.passed = true;
                string destination = Path.Combine(root, "docs/evidence/annual-numeric-client/unity-checks.json");
                File.WriteAllText(destination, JsonUtility.ToJson(evidence, true) + "\n");
                Debug.Log("Annual numeric client Unity checks passed: " + evidence.assertions);
            }
            finally { UnityEngine.Object.DestroyImmediate(host); UnityEngine.Object.DestroyImmediate(second); }
        }
    }
}
#endif
