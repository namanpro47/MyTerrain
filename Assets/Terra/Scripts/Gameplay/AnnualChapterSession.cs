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
        public bool useJavaScriptBackend = true;
        public bool fallbackWhenUnavailable = true;
        // Optional trusted-development credential; never serialize a shared token into a Web build.
        [NonSerialized] public string bearerToken;
        public AnnualTownSession town;
        public event Action StateChanged;
        public bool Busy => operation != null;
        public string Error { get; private set; }
        public AnnualChapterReceipt Latest => receipts.Count == 0 ? null : Clone(receipts[receipts.Count - 1]);
        public AnnualChapterReceipt ForYear(int year) => Clone(receipts.FirstOrDefault(r => r.consequences.year == year));
        public bool Finishing { get; private set; }
        readonly List<AnnualChapterReceipt> receipts = new List<AnnualChapterReceipt>();
        UnityWebRequest request;
        UnityWebRequestAsyncOperation operation;
        AnnualChapterRequest frozen;
        System.Diagnostics.Stopwatch resolutionTiming;
        string guestToken;
        double guestExpiresAt;
        bool requestingSession, refreshedSession;
        bool readyForFallback;
        public static bool UsesHostedProtocol(string address)
            => Uri.TryCreate(address,UriKind.Absolute,out var uri)&&!uri.IsLoopback;

        public void ResolveOnline()
        {
            if (Busy) return; // Duplicate button clicks must not issue another request.
            Error = null;
            readyForFallback = false;
            try
            {
                if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
                    || (uri.Scheme != "https" && !(uri.Scheme == "http" && uri.IsLoopback)))
                    throw new InvalidOperationException("Use HTTPS, or HTTP on localhost for the developer service.");
                resolutionTiming = System.Diagnostics.Stopwatch.StartNew();
                frozen = Freeze();
                if (frozen == null) return; // Already reviewing/completed: never reroll.
                if(town.Engine.PlayerRulesActive)
                {
                    // Commit and save the numerical year before optional remote prose.
                    var submitted=frozen;
                    Accept(AnnualChapters.Offline(submitted));
                    frozen=submitted;
                }
                Persist(); // Save frozen request before the first network request.
                readyForFallback = true;
                refreshedSession = false;
                SendChapterOrSession();
            }
            catch (Exception error) { Fail(error.Message); }
        }

        void SendChapterOrSession()
        {
            bool needsGuest = useJavaScriptBackend && new Uri(endpoint).Scheme == "https" && string.IsNullOrWhiteSpace(bearerToken);
            requestingSession = needsGuest && (string.IsNullOrEmpty(guestToken) || Time.realtimeSinceStartupAsDouble >= guestExpiresAt);
            string address = requestingSession ? new Uri(new Uri(endpoint), "/session").AbsoluteUri : endpoint;
            string payload = requestingSession ? "{}" : useJavaScriptBackend
                ? JsonUtility.ToJson(AnnualChapters.NarrationRequest(frozen)) : JsonUtility.ToJson(frozen);
            request = new UnityWebRequest(address, "POST") {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload)),
                downloadHandler = new DownloadHandlerBuffer(), timeout = 35
            };
            request.SetRequestHeader("Content-Type", "application/json");
            if (!requestingSession)
            {
                string credential = needsGuest ? guestToken : bearerToken;
                if (!string.IsNullOrWhiteSpace(credential)) request.SetRequestHeader("Authorization", "Bearer " + credential);
            }
            operation = request.SendWebRequest();
            Publish();
        }

        AnnualChapterRequest Freeze()
        {
            if (town == null || !town.Initialized) throw new InvalidOperationException("Initialize the annual town first.");
            var state = town.State;
            if (state.phase == TownPhase.Reviewing || state.phase == TownPhase.Completed)
                return town.Engine.PlayerRulesActive?AnnualChapters.AcceptedRequest(town.Engine):null;
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
                if (!requestingSession && request.responseCode == 401 && !refreshedSession && !string.IsNullOrEmpty(guestToken))
                {
                    refreshedSession = true; guestToken = null;
                    ReleaseRequest(); SendChapterOrSession(); return;
                }
                if (request.result != UnityWebRequest.Result.Success)
                {
                    string message = "Chapter request failed. Retry this year or choose offline.";
                    try {
                        var body = JsonUtility.FromJson<ChapterError>(request.downloadHandler.text);
                        if (body != null && !string.IsNullOrWhiteSpace(body.error) && body.error.Length <= 500) message = body.error;
                    } catch { }
                    throw new InvalidOperationException(message);
                }
                if (requestingSession)
                {
                    var session = JsonUtility.FromJson<GuestSession>(request.downloadHandler.text);
                    if (session == null || session.token == null || session.token.Length != 43
                        || session.token.Any(c => !(char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                        || session.expiresIn < 60 || session.expiresIn > 7200)
                        throw new InvalidOperationException("The chapter session is unavailable.");
                    guestToken = session.token; guestExpiresAt = Time.realtimeSinceStartupAsDouble + session.expiresIn - 30;
                    ReleaseRequest(); SendChapterOrSession(); return;
                }
                var receipt = JsonUtility.FromJson<AnnualChapterReceipt>(request.downloadHandler.text);
                ReleaseRequest();
                Accept(receipt);
                ContinueFinishing();
            }
            catch (Exception error) { Fail(error.Message); }
        }

        void ContinueFinishing()
        {
            if (Finishing && town.State.phase != TownPhase.Completed) { Continue(); ResolveOnline(); }
            else Finishing = false;
        }

        void Accept(AnnualChapterReceipt receipt)
        {
            // Validate content AND all outcomes against a detached replay before any live mutation.
            AnnualChapters.ValidateReceipt(frozen, receipt);
            var current = town.State;
            var accepted=current.history.FirstOrDefault(r=>r.acceptedRequest.requestId==frozen.snapshot.pendingRequest.requestId);
            bool proseOnly=town.Engine.PlayerRulesActive&&accepted!=null
                && JsonUtility.ToJson(accepted.acceptedConsequences)==JsonUtility.ToJson(receipt.consequences)
                && current.runId==frozen.snapshot.runId;
            if (!proseOnly&&(current.phase != TownPhase.Resolving || current.pendingRequest.inputHash != frozen.snapshot.pendingRequest.inputHash
                || current.pendingRequest.runId != frozen.snapshot.runId))
                throw new InvalidOperationException("This chapter belongs to an old run or plan. It was not applied.");
            if(!proseOnly)town.CompleteYear(receipt.consequences);
            if(town.Engine.PlayerRulesActive)town.Save();
            receipts.RemoveAll(r=>r.consequences.requestId==receipt.consequences.requestId);
            receipts.Add(Clone(receipt));
            frozen = null;
            Persist();
            Publish();
            if (resolutionTiming != null)
                Debug.Log("TERRA_ANNUAL_TIMING: " + receipt.mode + " year " + receipt.consequences.year + " accepted in "
                    + resolutionTiming.Elapsed.TotalMilliseconds.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                    + " ms including persistence and report refresh; online includes request wait");
            resolutionTiming = null;
        }

        public void ResolveOffline()
        {
            StopRequest(); // Explicit selection; never an automatic silent fallback.
            Error = null;
            resolutionTiming = System.Diagnostics.Stopwatch.StartNew();
            frozen = Freeze();
            if (frozen != null) { Persist(); Accept(AnnualChapters.Offline(frozen)); }
        }
        public void Continue()
        {
            if (Busy&&town.Engine.PlayerRulesActive)StopRequest();
            if (Busy) throw new InvalidOperationException("Wait for the chapter to finish.");
            town.ContinueYear(town.State.revision); Persist(); Publish();
        }
        public void CancelResolution()
        {
            Finishing = false; ReleaseRequest(); frozen = null;
            town.CancelResolution(town.State.revision); Error = null; Persist(); Publish();
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
        public void StopRequest() { ClearRequest(); Publish(); }
        void ClearRequest() { Finishing = false; ReleaseRequest(); frozen = null; readyForFallback = false; }
        void ReleaseRequest() { operation = null; if (request != null) { request.Abort(); request.Dispose(); request = null; } }
        void Fail(string message)
        {
            ReleaseRequest();
            if (fallbackWhenUnavailable && readyForFallback && frozen != null)
            {
                try
                {
                    var receipt = AnnualChapters.Offline(frozen);
                    receipt.chapter.introduction = "AI unavailable. Local summary from your saved game results.\n\n" + receipt.chapter.introduction;
                    // The same frozen request and normal receipt validator own the transition.
                    Accept(receipt); Error = null; ContinueFinishing(); return;
                }
                catch (Exception fallbackError) { message = "The year is still pending. " + fallbackError.Message; }
            }
            Error = message; StopRequest();
        }
        // Scene teardown may have already destroyed subscribers' HUD objects.
        // Cancel transport silently; the town's frozen request remains retryable.
        void OnDisable() { ClearRequest(); }

        public string ExportSave() => JsonUtility.ToJson(new AnnualChapterSave { town = town.State, chapters = receipts.ToArray() }, true);
        public void ImportSave(string json)
        {
            if (Busy) throw new InvalidOperationException("Stop the current request before importing a save.");
            var save = JsonUtility.FromJson<AnnualChapterSave>(json);
            if (save == null || save.schemaVersion != 1 || save.town == null || save.chapters == null)
                throw new ArgumentException("Expected an annual chapter save.");
            var definition = town.Definition;
            var current=town.State;
            if(town.Engine.PlayerRulesActive&&save.town.runId==current.runId&&save.town.history.Length<current.history.Length)save.town=current;
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
                    || receipt.consequences.effects == null || (!town.Engine.PlayerRulesActive&&receipt.consequences.effects.Length != 0))
                    throw new ArgumentException("Chapter receipt does not match the saved year.");
            }
            town.ImportSave(JsonUtility.ToJson(save.town));
            receipts.Clear(); receipts.AddRange(save.chapters.OrderBy(c => c.consequences.year).Select(Clone));
            if(town.Engine.PlayerRulesActive&&town.State.history.Length>0&&ForYear(town.State.history.Last().year)==null)receipts.Add(AnnualChapters.OfflineAccepted(town.Engine));
            frozen = null; Error = null; Persist(); Publish();
        }
        public void ResetForRun()
        {
            Finishing = false; ReleaseRequest(); frozen = null; Error = null;
            receipts.Clear(); Persist(); Publish();
        }
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(saveKey)) throw new InvalidOperationException("Set a chapter save key.");
            AnnualTownSession.WriteStored(saveKey, ExportSave()); PlayerPrefs.Save();
        }
        public void RestoreSaved() { if (PlayerPrefs.HasKey(saveKey)) ImportSave(AnnualTownSession.ReadStored(saveKey)); }
        void Persist() { if (autoSave) Save(); }
        static T Clone<T>(T value) => value == null ? default : JsonUtility.FromJson<T>(JsonUtility.ToJson(value));
        void Publish()
        {
            if (StateChanged == null) return;
            foreach (Action listener in StateChanged.GetInvocationList())
                try { listener(); } catch (Exception error) { Debug.LogException(error, this); }
        }
        [Serializable] sealed class ChapterError { public string error; }
        [Serializable] sealed class GuestSession { public string token; public int expiresIn; }
    }
}
