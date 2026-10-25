# Numeric run directory — October 25, 2026

Additive, inactive controller and contracts based on refreshed `origin/main` `19e7eae835ed4c06a21d42887a9a53ed28cc4fc2`. The current schema-2 player, scenes, HUD and annual session are unchanged. This package supplies the pure boundary requested by the numeric client owner; it is not browser storage or runtime activation evidence.

## API and persistence contract

`AnnualNumericRunDirectoryTransactions.Open(worldPackId, store)` reads a consistent directory, exact definition and active numeric save. An absent directory returns an empty selection without writing. Missing indexed records, unavailable storage and invalid records throw; they never manufacture a new run.

The store is bound to `KeyFor(worldPackId)`: `myterrain.numeric-directory.v1/<escaped-world>/schema-3/annual-numeric-v4`. A directory is scoped by location/schema/model family, not definition hash, so replay seeds with different exact definitions remain selectable together. Each entry contains its run ID and exact definition hash; definitions are immutable records shared by matching runs. AnnualTown and AnnualNumericTransactions remain the only annual ledger and replay validators.

Public mutations:

- `CreateAndActivate(definition, runId, operationId, expectedDirectoryHash, cancellation)` freezes the caller's definition before waiting for another operation. One store transaction checks the prior directory hash, retains or inserts the immutable definition, inserts a previously absent initial run, and updates the directory. Existing runs cannot be overwritten.
- `Select(runId, operationId, expectedDirectoryHash, cancellation)` reads and validates the retained target, then atomically checks both the prior directory hash and the target's current save hash before changing only the directory. Selection never rewrites accepted years or narration.
- `Reload(cancellation)` recovers the current selection. Previously observed directory rollback, equal-sequence directory mutation, and rollback/conflict of the same known active run are rejected. The controller is not a permanent cross-session anti-rollback registry.

`IAnnualNumericRunDirectoryStore` exposes `Read`, `ReadRun`, `CreateAndActivate`, and `Select`. ReadRun returns one retained definition/save pair consistently; a missing target throws. Stores acknowledge transaction completion, not an individual record write. Cancellation or an exception after dispatch may have a committed outcome: the controller retains its prior view, marks `RecoveryRequired`, and rereads before another mutation. Atomicity belongs to the real store implementation; the pure controller cannot make separate backend writes atomic.

The operation fingerprint includes kind, location, model, run, definition and expected prior directory hash. Mutable target save hash is deliberately a transaction precondition rather than part of selection intent: a run can advance after selection but before a lost acknowledgement is recovered. Only the latest committed operation is recognized as an idempotent retry. This is not an unbounded registry of all previously used operation IDs. A stale retry with its original expected pointer is rejected after a newer directory operation.

`Selection` and Changed event arguments are detached startup snapshots. The annual session remains authoritative after gameplay advances. Subscriber exceptions cannot roll back acknowledged persistence. IDs are bounded to 256 characters; directory sequence is bounded to JavaScript's maximum exact integer. No deletion, migration, fallback to legacy keys, generated rules, or shared HUD/Main glue is included.

## Verification

Run from this worktree:

```sh
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/annual-numeric-directory/AnnualNumericDirectoryChecks.csproj
```

The test uses the actual annual engine and a fake atomic store. It covers acknowledgement ordering, concurrent initial creation, duplicate clicks, stale tabs, target-save races, lost acknowledgements, cancellation, unavailable reads, immutable definition conflicts, cross-location identity, corrupt/missing references, caller mutation, and preservation of a completed ten-year history and narration through replay. Assertions do not establish actual IndexedDB or linked Unity correctness.

The unchanged transaction regression passes 73 assertions. The new source also builds against .NET Standard 2.1/C# 9 with zero warnings/errors. The exact final directory assertion result is recorded in `checks.txt`. No new Unity Editor/export was launched for this pure slice.

## Integration handoff

The numeric client owner supplies the IndexedDB bridge and Unity adapter separately. All bridge, adapter, controller and exact JSON codec changes must be integrated and tested together before activation; the version-2 database cannot be opened by the old version-1 bridge. Native and linked-browser tests must retain exact floating-point definition/save hashes, real atomic transaction acknowledgement, previous runs, and narration receipts.

New planning-horizon and paid-conversion work is concurrently changing shared annual contracts/core in a separate task. This receipt certifies only the pinned base above; reconcile those changes and repeat definition/replay checks before shipping numeric mode. Phase 2A scenery and whole-game acceptance remain separate open gates.
