using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace Terra.Gameplay
{
    /// <summary>Optional annual chapter controller; no main scene/HUD changes. All commits run on Unity's thread.</summary>
    public sealed class AnnualChapterSession : MonoBehaviour
    {
        public string endpoint = "http://127.0.0.1:8787/chapter";
        public string saveKey = "terra.annual-chapters.v1";
        public bool autoSave = true;
        public AnnualTownSession town;
        public bool Busy => operation != null;
        public string Error { get; private set; }
        public AnnualChapterReceipt Latest => receipts.Count == 0 ? null : Clone(receipts[receipts.Count - 1]);
        public bool Finishing { get; private set; }
        readonly List<AnnualChapterReceipt> receipts = new List<AnnualChapterReceipt>();
        UnityWebRequest request;
        UnityWebRequestAsyncOperation operation;
        AnnualChapterRequest frozen;

        public void ResolveOnline()
        {
            if (Busy) return; // Duplicate button clicks must not issue another request.
            Error = null;
            try
            {
                if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
                    || (uri.Scheme != "https" && !(uri.Scheme == "http" && uri.IsLoopback)))
                    throw new InvalidOperationException("Use HTTPS, or HTTP on localhost for the developer service.");
                frozen = Freeze();
                if (frozen == null) return; // Already reviewing/completed: never reroll.
                Persist(); // Save frozen request before the first network request.
                request = new UnityWebRequest(endpoint, "POST") {
                    uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(frozen))),
                    downloadHandler = new DownloadHandlerBuffer(), timeout = 35
                };
                request.SetRequestHeader("Content-Type", "application/json");
                operation = request.SendWebRequest();
            }
            catch (Exception error) { Fail(error.Message); }
        }

        AnnualChapterRequest Freeze()
        {
            if (town == null || !town.Initialized) throw new InvalidOperationException("Initialize the annual town first.");
            var state = town.State;
            if (state.phase == TownPhase.Reviewing || state.phase == TownPhase.Completed) return null;
            if (state.phase == TownPhase.Planning)
                town.BeginYear(state.runId + "/chapter-" + state.year + "-r" + state.revision, state.revision);
            return new AnnualChapterRequest { definition = town.Definition, snapshot = town.State };
        }

        void Update() { Poll(); }
        // Editor playtest calls this from EditorApplication.update; no Editor coroutine package needed.
        public void Poll()
        {
            if (!Busy || !operation.isDone) return;
            try
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    string message = "Chapter request failed. Retry this year or choose offline.";
                    try {
                        var body = JsonUtility.FromJson<ChapterError>(request.downloadHandler.text);
                        if (body != null && !string.IsNullOrWhiteSpace(body.error) && body.error.Length <= 500) message = body.error;
                    } catch { }
                    throw new InvalidOperationException(message);
                }
                var receipt = JsonUtility.FromJson<AnnualChapterReceipt>(request.downloadHandler.text);
                ReleaseRequest();
                Accept(receipt);
                if (Finishing && town.State.phase != TownPhase.Completed) { Continue(); ResolveOnline(); }
                else Finishing = false;
            }
            catch (Exception error) { Fail(error.Message); }
        }

        void Accept(AnnualChapterReceipt receipt)
        {
            // Validate content AND all outcomes against a detached replay before any live mutation.
            AnnualChapters.ValidateReceipt(frozen, receipt);
            var current = town.State;
            if (current.phase != TownPhase.Resolving || current.pendingRequest.inputHash != frozen.snapshot.pendingRequest.inputHash
                || current.pendingRequest.runId != frozen.snapshot.runId)
                throw new InvalidOperationException("This chapter belongs to an old run or plan. It was not applied.");
            town.CompleteYear(receipt.consequences);
            receipts.Add(Clone(receipt));
            frozen = null;
            Persist();
        }

        public void ResolveOffline()
        {
            StopRequest(); // Explicit selection; never an automatic silent fallback.
            Error = null;
            frozen = Freeze();
            if (frozen != null) { Persist(); Accept(AnnualChapters.Offline(frozen)); }
        }
        public void Continue()
        {
            if (Busy) throw new InvalidOperationException("Wait for the chapter to finish.");
            town.ContinueYear(town.State.revision); Persist();
        }
        public void CancelResolution()
        {
            StopRequest();
            town.CancelResolution(town.State.revision); Error = null; Persist();
        }
        // Bind only after the player confirms no further new interventions.
        public void FinishOnline()
        {
            if (Busy) return;
            Finishing = true;
            if (town.State.phase == TownPhase.Completed) { Finishing = false; return; }
            if (town.State.phase == TownPhase.Reviewing) Continue();
            ResolveOnline();
        }
        public void FinishOffline()
        {
            StopRequest();
            while (town.State.phase != TownPhase.Completed)
            {
                if (town.State.phase == TownPhase.Reviewing) Continue();
                ResolveOffline();
            }
        }
        public void StopRequest() { Finishing = false; ReleaseRequest(); frozen = null; }
        void ReleaseRequest() { operation = null; if (request != null) { request.Abort(); request.Dispose(); request = null; } }
        void Fail(string message) { StopRequest(); Error = message; }
        void OnDisable() { StopRequest(); }

        public string ExportSave() => JsonUtility.ToJson(new AnnualChapterSave { town = town.State, chapters = receipts.ToArray() }, true);
        public void ImportSave(string json)
        {
            if (Busy) throw new InvalidOperationException("Stop the current request before importing a save.");
            var save = JsonUtility.FromJson<AnnualChapterSave>(json);
            if (save == null || save.schemaVersion != 1 || save.town == null || save.chapters == null)
                throw new ArgumentException("Expected an annual chapter save.");
            var definition = town.Definition;
            AnnualTown.Restore(definition, save.town);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var receipt in save.chapters)
            {
                if (receipt?.consequences == null || !seen.Add(receipt.consequences.requestId)) throw new ArgumentException("Duplicate or missing chapter receipt.");
                var report = save.town.history.FirstOrDefault(h => h.acceptedRequest.requestId == receipt.consequences.requestId);
                if (report == null) throw new ArgumentException("Chapter is not in accepted town history.");
                // Validate chapter text against the accepted report; outcomes were already replayed above.
                var factIds = new[] { "year", "weather", "economic", "social", "environmental", "budget", "limits" }
                    .Concat(report.outcomes.sites.Select(s => "site:" + s.siteId)).Select(id => new AnnualChapterFact { id = id }).ToArray();
                AnnualChapters.ValidateText(receipt.chapter, factIds);
                if (receipt.schemaVersion != 1 || receipt.rulesVersion != AnnualChapters.RulesVersion
                    || (receipt.mode != "offline" && receipt.mode != "gemini") || string.IsNullOrWhiteSpace(receipt.model)
                    || JsonUtility.ToJson(receipt.consequences) != JsonUtility.ToJson(report.acceptedConsequences)
                    || receipt.consequences.effects == null || receipt.consequences.effects.Length != 0)
                    throw new ArgumentException("Chapter receipt does not match the saved year.");
            }
            town.ImportSave(JsonUtility.ToJson(save.town));
            receipts.Clear(); receipts.AddRange(save.chapters.OrderBy(c => c.consequences.year).Select(Clone));
            frozen = null; Error = null; Persist();
        }
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(saveKey)) throw new InvalidOperationException("Set a chapter save key.");
            PlayerPrefs.SetString(saveKey, ExportSave()); PlayerPrefs.Save();
        }
        public void RestoreSaved() { if (PlayerPrefs.HasKey(saveKey)) ImportSave(PlayerPrefs.GetString(saveKey)); }
        void Persist() { if (autoSave) Save(); }
        static T Clone<T>(T value) => JsonUtility.FromJson<T>(JsonUtility.ToJson(value));
        [Serializable] sealed class ChapterError { public string error; }
    }
}
