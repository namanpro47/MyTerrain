using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terra.Contracts;

namespace Terra.Core
{
    // Bind one store to KeyFor(worldPackId). Read operations must return consistent
    // directory/definition/run records. Null means only an absent directory;
    // missing or corrupt indexed content throws. Never manufacture a fresh run.
    public interface IAnnualNumericRunDirectoryStore
    {
        Task<AnnualNumericRunSelection> Read(CancellationToken cancellation);
        Task<AnnualNumericRunContent> ReadRun(string runId, CancellationToken cancellation);
        // One atomic transaction: CAS directory, preserve immutable definition,
        // insert a previously absent run, then replace the active pointer.
        Task CreateAndActivate(string expectedDirectoryHash, AnnualNumericRunDirectory candidate,
            AnnualTownDefinition definition, AnnualNumericSave initialSave, CancellationToken cancellation);
        // One atomic transaction: CAS directory AND existing target save hash;
        // replace only the directory. Never rewrite a run or its definition.
        Task Select(string expectedDirectoryHash, AnnualNumericRunDirectory candidate,
            string expectedRunSaveHash, CancellationToken cancellation);
        // Writes acknowledge transaction completion, not an individual put.
        // Exceptions may mean a committed operation whose acknowledgement was lost.
    }

    public sealed class AnnualNumericRunDirectoryTransactions
    {
        public const string ModelFamily = "annual-numeric-v4";
        const long MaximumSequence = 9007199254740991L; // Exact in the browser bridge.
        readonly string worldPackId;
        readonly IAnnualNumericRunDirectoryStore store;
        readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
        AnnualNumericRunSelection saved;
        bool recoveryRequired;
        public bool RecoveryRequired => recoveryRequired;
        public string DirectoryHash => saved?.directory.directoryHash;
        // A detached selection/startup snapshot; the annual session remains the
        // authority for subsequent gameplay and may advance this run independently.
        public AnnualNumericRunSelection Selection => TownCopies.Copy(saved);
        public event Action<AnnualNumericRunSelection> Changed;

        AnnualNumericRunDirectoryTransactions(string world, IAnnualNumericRunDirectoryStore persistence)
        {
            Require(Text(world) && persistence != null, "A location and bound directory store are required.");
            worldPackId = world; store = persistence;
        }
        public static string KeyFor(string worldPackId)
        {
            Require(Text(worldPackId), "A location is required.");
            return "myterrain.numeric-directory.v1/" + Uri.EscapeDataString(worldPackId) + "/schema-3/" + ModelFamily;
        }
        public static async Task<AnnualNumericRunDirectoryTransactions> Open(string worldPackId,
            IAnnualNumericRunDirectoryStore store, CancellationToken cancellation = default)
        {
            var directory = new AnnualNumericRunDirectoryTransactions(worldPackId, store);
            await directory.Load(cancellation);
            return directory; // Opening an absent directory performs no write.
        }
        public async Task Reload(CancellationToken cancellation = default)
        {
            await gate.WaitAsync(cancellation);
            try { await Load(cancellation); }
            finally { gate.Release(); }
        }
        public async Task CreateAndActivate(AnnualTownDefinition definition, string runId, string operationId,
            string expectedDirectoryHash, CancellationToken cancellation = default)
        {
            var rules = TownCopies.Copy(definition); // Freeze caller input even while another operation holds the gate.
            await gate.WaitAsync(cancellation);
            try
            {
                Require(Text(runId) && Text(operationId), "Stable run and operation IDs are required.");
                var engine = Engine(rules, runId);
                string intent = Intent("create", runId, engine.DefinitionHash, expectedDirectoryHash);
                await Recover(cancellation);
                if (Applied(operationId, intent)) return;
                Expected(expectedDirectoryHash);
                Require(saved == null || saved.directory.runs.All(r => r.runId != runId), "Run ID already exists; retained runs cannot be overwritten.");
                var initial = new AnnualNumericSave { sequence = 1, town = engine.Snapshot() };
                initial.saveHash = AnnualNumericTransactions.Hash(initial);
                var next = Next(runId, operationId, intent);
                next.runs = next.runs.Concat(new[] { new AnnualNumericRunEntry { runId = runId, definitionHash = engine.DefinitionHash } }).ToArray();
                next.directoryHash = Hash(next);
                ValidateDirectory(next);
                try { await store.CreateAndActivate(expectedDirectoryHash, TownCopies.Copy(next), TownCopies.Copy(rules), TownCopies.Copy(initial), cancellation); }
                catch { recoveryRequired = true; throw; }
                Publish(new AnnualNumericRunSelection { directory = next, definition = rules, save = initial });
            }
            finally { gate.Release(); }
        }
        public async Task Select(string runId, string operationId, string expectedDirectoryHash,
            CancellationToken cancellation = default)
        {
            await gate.WaitAsync(cancellation);
            try
            {
                Require(Text(runId) && Text(operationId), "Stable run and operation IDs are required.");
                await Recover(cancellation);
                var entry = saved?.directory.runs.SingleOrDefault(r => r.runId == runId);
                Require(entry != null, "Select a retained run in this location and model family.");
                // The mutable save hash is a store precondition, not user intent.
                // A selected run can legitimately advance after a successful selection.
                string intent = Intent("select", runId, entry.definitionHash, expectedDirectoryHash);
                if (Applied(operationId, intent)) return;
                Expected(expectedDirectoryHash);
                AnnualNumericRunContent target;
                try { target = await ValidateContent(entry, await store.ReadRun(runId, cancellation), cancellation); }
                catch { recoveryRequired = true; throw; }
                var next = Next(runId, operationId, intent); next.directoryHash = Hash(next);
                ValidateDirectory(next);
                try { await store.Select(expectedDirectoryHash, TownCopies.Copy(next), target.save.saveHash, cancellation); }
                catch { recoveryRequired = true; throw; }
                Publish(new AnnualNumericRunSelection { directory = next, definition = target.definition, save = target.save });
            }
            finally { gate.Release(); }
        }

        async Task Recover(CancellationToken cancellation) { if (recoveryRequired) await Load(cancellation); }
        async Task Load(CancellationToken cancellation)
        {
            try
            {
                var read = TownCopies.Copy(await store.Read(cancellation));
                if (read == null)
                {
                    Require(saved == null, "A previously saved directory is missing; preserve the current selection and recover storage.");
                    recoveryRequired = false; return;
                }
                ValidateDirectory(read.directory);
                if (saved != null)
                    Require(read.directory.sequence > saved.directory.sequence ||
                        read.directory.sequence == saved.directory.sequence && read.directory.directoryHash == saved.directory.directoryHash,
                        "Saved directory moved backward or changed without a new sequence.");
                var entry = read.directory.runs.Single(r => r.runId == read.directory.activeRunId);
                var content = await ValidateContent(entry, new AnnualNumericRunContent { definition = read.definition, save = read.save }, cancellation);
                read.definition = content.definition; read.save = content.save;
                Publish(read);
            }
            catch { recoveryRequired = true; throw; }
        }
        async Task<AnnualNumericRunContent> ValidateContent(AnnualNumericRunEntry entry, AnnualNumericRunContent input, CancellationToken cancellation)
        {
            var content = TownCopies.Copy(input);
            Require(content?.definition != null && content.save?.town != null, "Indexed run or exact definition is missing.");
            var engine = Engine(content.definition, entry.runId);
            Require(engine.DefinitionHash == entry.definitionHash && content.save.town.runId == entry.runId,
                "Indexed run and exact definition do not match.");
            // Reuse all envelope, ledger, replay and narration checks. This read-only
            // adapter cannot create a missing save or modify the selected run.
            var verified = await AnnualNumericTransactions.Open(content.definition, entry.runId, new ExistingSnapshot(content.save), cancellation);
            content.save = verified.SaveState;
            if (saved?.save.town.runId == entry.runId && saved.save.town.definitionHash == entry.definitionHash)
                Require(content.save.sequence > saved.save.sequence ||
                    content.save.sequence == saved.save.sequence && content.save.saveHash == saved.save.saveHash,
                    "Selected run moved backward or changed without a new save sequence.");
            return content;
        }
        AnnualTown Engine(AnnualTownDefinition rules, string runId)
        {
            Require(rules != null && rules.schemaVersion == 3 && rules.worldPackId == worldPackId,
                "Numeric directory rules belong to another schema or location.");
            return new AnnualTown(rules, runId); // Validates the supported numeric model.
        }
        void ValidateDirectory(AnnualNumericRunDirectory d)
        {
            Require(d != null && d.schemaVersion == 1 && d.townSchemaVersion == 3 && d.worldPackId == worldPackId
                && d.modelFamily == ModelFamily && d.sequence >= 1 && d.sequence <= MaximumSequence
                && Text(d.operationId) && Digest(d.operationHash) && Digest(d.directoryHash), "Unsupported or incomplete run directory.");
            Require(d.runs != null && d.runs.Length > 0 && d.runs.Length <= d.sequence
                && d.runs.All(r => r != null && Text(r.runId) && Digest(r.definitionHash))
                && d.runs.Select(r => r.runId).Distinct(StringComparer.Ordinal).Count() == d.runs.Length
                && d.runs.Any(r => r.runId == d.activeRunId), "Run directory has duplicate, missing or invalid entries.");
            Require(d.directoryHash == Hash(d), "Run directory identity changed.");
        }
        AnnualNumericRunDirectory Next(string runId, string operationId, string intent)
        {
            var next = saved == null ? new AnnualNumericRunDirectory { worldPackId = worldPackId, modelFamily = ModelFamily } : TownCopies.Copy(saved.directory);
            Require(next.sequence < MaximumSequence, "Run directory sequence limit reached.");
            next.sequence++; next.activeRunId = runId; next.operationId = operationId; next.operationHash = intent;
            return next;
        }
        void Expected(string expected) => Require(expected == DirectoryHash, "Directory changed; reload before choosing another run.");
        bool Applied(string operationId, string intent)
        {
            if (saved?.directory.operationId != operationId) return false;
            Require(saved.directory.operationHash == intent, "Committed operation ID reused with different inputs.");
            return true;
        }
        string Intent(string kind, string runId, string definitionHash, string expected)
        {
            Require(expected == null || Digest(expected), "Expected directory hash must be exact, or null for absence.");
            return TownCopies.Hash(new[] { kind, worldPackId, ModelFamily, runId, definitionHash, expected });
        }
        void Publish(AnnualNumericRunSelection next)
        {
            saved = TownCopies.Copy(next); recoveryRequired = false;
            if (Changed == null) return;
            foreach (Action<AnnualNumericRunSelection> listener in Changed.GetInvocationList())
                try { listener(Selection); } catch { /* A listener cannot roll back acknowledged storage. */ }
        }
        public static string Hash(AnnualNumericRunDirectory directory)
        {
            Require(directory != null, "Missing run directory.");
            var copy = TownCopies.Copy(directory); copy.directoryHash = null;
            return TownCopies.Hash(copy);
        }
        static bool Text(string value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 256;
        static bool Digest(string value) => value != null && value.Length == 64 && value.All(c => c >= '0' && c <= '9' || c >= 'a' && c <= 'f');
        static void Require(bool condition, string message) { if (!condition) throw new ArgumentException(message); }
        sealed class ExistingSnapshot : IAnnualNumericStore
        {
            readonly AnnualNumericSave value;
            public ExistingSnapshot(AnnualNumericSave value) { this.value = value; }
            public Task<AnnualNumericSave> Read(CancellationToken cancellation) { cancellation.ThrowIfCancellationRequested(); return Task.FromResult(TownCopies.Copy(value)); }
            public Task Write(string expectedHash, AnnualNumericSave candidate, CancellationToken cancellation)
                => throw new InvalidOperationException("Indexed runs must already exist; validation cannot create a save.");
        }
    }
}
