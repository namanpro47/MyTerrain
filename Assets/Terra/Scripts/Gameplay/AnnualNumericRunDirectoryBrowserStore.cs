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
    // Explicit owner-controlled lifetime; not attached by the default game flow.
    public sealed class AnnualNumericRunDirectoryBrowserStore : MonoBehaviour, IAnnualNumericRunDirectoryStore
    {
        const float TimeoutSeconds = 60;
        string key, worldPackId;
        bool destroyed;
        readonly Dictionary<string, Pending> pending = new Dictionary<string, Pending>();
#pragma warning disable CS0649
        [Serializable] sealed class Request
        {
            public string operationId, key, worldPackId, operation, runId, expectedHash, expectedSaveHash;
            public string directoryJson, definitionJson, saveJson;
        }
        [Serializable] sealed class Reply
        { public string operationId, error, directoryJson, definitionJson, saveJson; public bool ok; }
        sealed class Pending
        {
            public TaskCompletionSource<Reply> completion;
            public CancellationToken cancellation;
            public float started;
        }
#pragma warning restore CS0649
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void TerraNumericDirectory(string receiver, string request);
#endif
        public static AnnualNumericRunDirectoryBrowserStore Create(string worldPackId)
        {
            string scope = AnnualNumericRunDirectoryTransactions.KeyFor(worldPackId);
            var host = new GameObject("TerraNumericDirectory-" + Guid.NewGuid().ToString("N"));
            var result = host.AddComponent<AnnualNumericRunDirectoryBrowserStore>();
            result.key = scope; result.worldPackId = worldPackId;
            return result;
        }
        public async Task<AnnualNumericRunSelection> Read(CancellationToken cancellation)
        {
            var reply = await Send(new Request { operation = "read" }, cancellation);
            if (string.IsNullOrEmpty(reply.directoryJson))
            {
                if (!string.IsNullOrEmpty(reply.definitionJson) || !string.IsNullOrEmpty(reply.saveJson))
                    throw new IOException("Numeric directory returned incomplete selection data.");
                return null;
            }
            return new AnnualNumericRunSelection {
                directory = ReadDirectory(reply.directoryJson),
                definition = AnnualNumericJson.ReadDefinition(reply.definitionJson),
                save = AnnualNumericJson.ReadSave(reply.saveJson)
            };
        }
        public async Task<AnnualNumericRunContent> ReadRun(string runId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(runId)) throw new ArgumentException("A retained run ID is required.");
            var reply = await Send(new Request { operation = "read-run", runId = runId }, cancellation);
            return new AnnualNumericRunContent {
                definition = AnnualNumericJson.ReadDefinition(reply.definitionJson),
                save = AnnualNumericJson.ReadSave(reply.saveJson)
            };
        }
        public async Task CreateAndActivate(string expectedDirectoryHash, AnnualNumericRunDirectory candidate,
            AnnualTownDefinition definition, AnnualNumericSave initialSave, CancellationToken cancellation)
        {
            if (initialSave == null || initialSave.saveHash != AnnualNumericTransactions.Hash(initialSave))
                throw new ArgumentException("Cannot persist an incomplete initial numeric save.");
            await Send(new Request { operation = "create", expectedHash = expectedDirectoryHash,
                directoryJson = WriteDirectory(candidate), definitionJson = AnnualNumericJson.WriteDefinition(definition),
                saveJson = AnnualNumericJson.WriteSave(initialSave) }, cancellation);
        }
        public async Task Select(string expectedDirectoryHash, AnnualNumericRunDirectory candidate,
            string expectedRunSaveHash, CancellationToken cancellation)
        {
            await Send(new Request { operation = "select", expectedHash = expectedDirectoryHash,
                directoryJson = WriteDirectory(candidate), expectedSaveHash = expectedRunSaveHash }, cancellation);
        }
        string WriteDirectory(AnnualNumericRunDirectory value)
        {
            if (value == null || value.worldPackId != worldPackId
                || value.directoryHash != AnnualNumericRunDirectoryTransactions.Hash(value))
                throw new ArgumentException("Cannot persist an incomplete or out-of-scope directory.");
            // Directory fields contain only safe integers and strings. Exact double
            // rules/save payloads use AnnualNumericJson and remain separate strings.
            return JsonUtility.ToJson(value);
        }
        AnnualNumericRunDirectory ReadDirectory(string json)
        {
            var value = JsonUtility.FromJson<AnnualNumericRunDirectory>(json);
            if (value == null || value.worldPackId != worldPackId
                || value.directoryHash != AnnualNumericRunDirectoryTransactions.Hash(value))
                throw new IOException("Numeric directory identity is invalid.");
            return value;
        }
        Task<Reply> Send(Request request, CancellationToken cancellation)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(AnnualNumericRunDirectoryBrowserStore));
            if (string.IsNullOrEmpty(key)) throw new InvalidOperationException("Create a store bound to one location.");
            cancellation.ThrowIfCancellationRequested();
#if UNITY_WEBGL && !UNITY_EDITOR
            request.operationId = Guid.NewGuid().ToString("N"); request.key = key; request.worldPackId = worldPackId;
            var call = new Pending { completion = new TaskCompletionSource<Reply>(), cancellation = cancellation,
                started = Time.realtimeSinceStartup };
            pending.Add(request.operationId, call);
            try { TerraNumericDirectory(gameObject.name, JsonUtility.ToJson(request)); }
            catch (Exception error) { pending.Remove(request.operationId); call.completion.TrySetException(error); }
            return call.completion.Task;
#else
            return Task.FromException<Reply>(new PlatformNotSupportedException("Acknowledged run selection requires a WebGL player. Inject a test store in the Editor."));
#endif
        }
        [UnityEngine.Scripting.Preserve]
        public void OnNumericDirectoryReply(string json)
        {
            if (destroyed) return;
            Reply reply;
            try { reply = JsonUtility.FromJson<Reply>(json); } catch { return; }
            if (reply == null || string.IsNullOrEmpty(reply.operationId) || !pending.TryGetValue(reply.operationId, out var call)) return;
            pending.Remove(reply.operationId);
            if (reply.ok) call.completion.TrySetResult(reply);
            else call.completion.TrySetException(new IOException("Numeric directory storage: " + (reply.error ?? "unknown-outcome")));
        }
        void Update()
        {
            foreach (var pair in pending.ToArray())
            {
                var call = pair.Value;
                if (!call.cancellation.IsCancellationRequested && Time.realtimeSinceStartup - call.started < TimeoutSeconds) continue;
                pending.Remove(pair.Key);
                if (call.cancellation.IsCancellationRequested) call.completion.TrySetCanceled(call.cancellation);
                else call.completion.TrySetException(new TimeoutException("Run selection acknowledgement timed out; recover the saved pointer before retrying."));
            }
        }
        void OnDestroy()
        {
            destroyed = true;
            var calls = pending.Values.ToArray(); pending.Clear();
            foreach (var call in calls) call.completion.TrySetException(new ObjectDisposedException(nameof(AnnualNumericRunDirectoryBrowserStore)));
            // An in-flight transaction can still commit. Teardown is not rollback.
        }
    }
}
