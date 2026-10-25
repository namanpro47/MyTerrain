# Inactive numeric run directory: prepared contract

The numeric client owner requested this independent prerequisite on October 25 while the production export is frozen. This is a design handoff, **not implemented or activated runtime behavior**. Source implementation will use a fresh checkout after the release's integration reservation completes. No Main, HUD, current browser store, default gameplay or legacy save key is changed here.

## Data and store boundary

- `AnnualNumericRunDirectory`: schema version 1, town schema 3, world pack, model family, sequence, directory hash, active run ID, entries of run ID plus exact definition hash, and the most recent operation ID/input hash.
- `AnnualNumericRunSelection`: one directory plus its exact active definition and active save. Only the active definition/save accompany a read; the directory does not duplicate every run's rules or history.
- Bound `IAnnualNumericRunDirectoryStore.Read(cancellation)` returns one consistent selection, or null only if the directory is absent. Missing or corrupt referenced rules/save is an error, never a new run.
- `CreateAndActivate(expectedDirectoryHash, candidateDirectory, exactDefinition, initialSave, cancellation)` atomically compares the current pointer, preserves any existing identical definition, inserts the new run and publishes its directory pointer. No existing run may be overwritten.
- `Select(expectedDirectoryHash, candidateDirectory, expectedRunSaveHash, cancellation)` atomically checks both directory and target save identity and changes only the pointer. Definitions and run saves remain untouched.

The scope is world pack + schema 3 + model family. It intentionally excludes full definition hash: changing the pinned weather seed creates different exact rules within the same location's replay history. Exact definitions are immutable records addressed by hash. The browser owner will add the directory/definition stores and dispatch in its later IndexedDB version; this pure module does not modify the existing run-store path.

## Controller behavior

Open/reload validates the directory, exact definition and existing numeric envelope. Reuse the existing annual engine and numeric transaction validator; do not add a ledger or reimplement consequence checks. Returned state and event payloads are detached copies. Subscriber failure cannot roll back an acknowledged operation.

Create/select serializes local operations and accepts a caller-retained operation ID plus expected directory hash. Their input fingerprint includes operation kind, scope, target run and definition, expected old directory, and target save identity. A retry matching the latest committed operation returns the saved result without another write. Reusing the ID with different inputs is rejected; an older retry after another selection cannot reactivate its target.

No new selection becomes visible before storage acknowledgement. Any write error can represent a lost acknowledgement and requires rereading before the next mutation. Read failures preserve the previously displayed state and block dependent edits. A known saved directory disappearing or moving backward is an error. Cancellation before dispatch does not write; cancellation after an uncertain commit must recover the actual persisted result.

## Verification matrix

- First creation, same-definition replay and different-seed replay; exact initial save and prior accepted history retained.
- Create/select failure before commit, failure after commit, blocked acknowledgement and lost-ack retry without duplicate creation or selection.
- Two controllers creating from an absent directory or selecting from the same prior hash; one winner and no overwrite by the stale loser.
- Retry of an old operation after a later selection; duplicate operation ID with changed input.
- Per-place isolation and multiple definitions/seeds in one directory; definition records remain immutable.
- Missing/corrupt definition, missing/corrupt run, mismatched scope/hash, duplicate run ID, unknown active pointer and stale/corrupt directory sequence.
- Caller/subscriber mutation, throwing subscriber, cancellation and recovery-read failure.
- Existing schema-2 and numeric transaction tests unchanged; standalone .NET Standard 2.1/C# 9 compile before handoff.

These pure checks do not replace actual IndexedDB atomicity, Unity/IL2CPP codec checks, integrated new-run UI, public run, rehearsals or the separate visual gate.
