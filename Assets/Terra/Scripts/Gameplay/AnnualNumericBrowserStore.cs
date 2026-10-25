using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;

namespace Terra.Gameplay
{
    // Explicitly created per run; never attached by the default game flow.
    public sealed class AnnualNumericBrowserStore : MonoBehaviour, IAnnualNumericStore
    {
        const float TimeoutSeconds = 60;
        string key;
        bool destroyed;
        readonly Dictionary<string, Pending> pending = new Dictionary<string, Pending>();
        #pragma warning disable CS0649 // JsonUtility fields and WebGL-only initialization.
        [Serializable] sealed class Request
        { public string operationId, key, operation, expectedHash, value; }
        [Serializable] sealed class Reply
        { public string operationId, error, value; public bool ok; }
        sealed class Pending
        {
            public TaskCompletionSource<string> completion;
            public CancellationToken cancellation;
            public float started;
        }
#pragma warning restore CS0649
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void TerraNumericStorage(string receiver, string request);
#endif
        public static string KeyFor(AnnualTownDefinition definition, string runId)
        {
            if (definition == null || definition.schemaVersion != 3)
                throw new ArgumentException("Numeric storage requires schema-3 rules.");
            var engine = new AnnualTown(definition, runId);
            return "myterrain.numeric.v1/" + Uri.EscapeDataString(definition.worldPackId)
                + "/schema-3/" + engine.DefinitionHash + "/" + Uri.EscapeDataString(runId);
        }
        public static AnnualNumericBrowserStore Create(AnnualTownDefinition definition, string runId)
        {
            var saveKey = KeyFor(definition, runId);
            var host = new GameObject("TerraNumericStore-" + Guid.NewGuid().ToString("N"));
            var store = host.AddComponent<AnnualNumericBrowserStore>();
            store.key = saveKey;
            return store; // The owner destroys this dedicated GameObject on leaving the run.
        }
        public async Task<AnnualNumericSave> Read(CancellationToken cancellation)
        {
            var json = await Send("read", null, null, cancellation);
            return string.IsNullOrEmpty(json) ? null : AnnualNumericJson.ReadSave(json);
        }
        public async Task Write(string expectedHash, AnnualNumericSave candidate, CancellationToken cancellation)
        {
            if (candidate == null || candidate.saveHash != AnnualNumericTransactions.Hash(candidate))
                throw new ArgumentException("Cannot persist an incomplete numeric envelope.");
            await Send("write", expectedHash, AnnualNumericJson.WriteSave(candidate), cancellation);
        }
        Task<string> Send(string operation, string expectedHash, string value, CancellationToken cancellation)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(AnnualNumericBrowserStore));
            if (string.IsNullOrEmpty(key)) throw new InvalidOperationException("Create a store bound to one explicit run.");
            cancellation.ThrowIfCancellationRequested();
#if UNITY_WEBGL && !UNITY_EDITOR
            var id = Guid.NewGuid().ToString("N");
            var call = new Pending { completion = new TaskCompletionSource<string>(), cancellation = cancellation,
                started = Time.realtimeSinceStartup };
            pending.Add(id, call);
            try { TerraNumericStorage(gameObject.name, JsonUtility.ToJson(new Request {
                operationId = id, key = key, operation = operation, expectedHash = expectedHash, value = value })); }
            catch (Exception error) { pending.Remove(id); call.completion.TrySetException(error); }
            return call.completion.Task;
#else
            return Task.FromException<string>(new PlatformNotSupportedException("Acknowledged browser storage requires a WebGL player. Inject a test store in the Editor."));
#endif
        }
        // Called by the WebGL plug-in with one string, on Unity's main thread.
        [UnityEngine.Scripting.Preserve]
        public void OnNumericStorageReply(string json)
        {
            if (destroyed) return;
            Reply reply;
            try { reply = JsonUtility.FromJson<Reply>(json); } catch { return; }
            if (reply == null || string.IsNullOrEmpty(reply.operationId) || !pending.TryGetValue(reply.operationId, out var call)) return;
            pending.Remove(reply.operationId);
            if (reply.ok) call.completion.TrySetResult(reply.value);
            else call.completion.TrySetException(new IOException("Numeric storage: " + (reply.error ?? "unknown-outcome")));
        }
        void Update()
        {
            foreach (var pair in pending.ToArray())
            {
                var call = pair.Value;
                if (!call.cancellation.IsCancellationRequested && Time.realtimeSinceStartup - call.started < TimeoutSeconds) continue;
                pending.Remove(pair.Key);
                // Stopping the wait cannot undo an IndexedDB commit. Controller
                // recovery rereads the record before retrying a failed operation.
                if (call.cancellation.IsCancellationRequested) call.completion.TrySetCanceled(call.cancellation);
                else call.completion.TrySetException(new TimeoutException("Numeric storage acknowledgement timed out; outcome is unknown."));
            }
        }
        void OnDestroy()
        {
            destroyed = true;
            var calls = pending.Values.ToArray(); pending.Clear();
            foreach (var call in calls) call.completion.TrySetException(new ObjectDisposedException(nameof(AnnualNumericBrowserStore)));
        }
    }
}
