# Save-first numeric annual transactions

October 25, 2026. **Pure transaction boundary tested; browser storage and Unity/session wiring remain unimplemented in this checkpoint.** Prepared on refreshed main `13a15a43c43bab9e0eb6a2a90bded4bb4d7aba0c`, with numeric dependency `0c28489` and projection `388267d`, merged at `2df6475809b240c5f95821dd2609def3920890af`. The entire dependency branch remains excluded from the current release. No default activation, scene, HUD or TerraMain change.

`AnnualNumericTransactions` stages every command through the existing `AnnualTown` authority. It does not implement another ledger or resolver. Commands replay a detached candidate, validate the complete save envelope, and publish it only after an injected `IAnnualNumericStore.Write` acknowledges an atomic compare-and-replace. A read-only consumer gets detached snapshots/engines; mutating them cannot bypass persistence. Listener failures cannot undo a saved command.

`ResolveYear` first saves the frozen request, then resolves the numeric candidate locally, builds its canonical report projection, and saves the accepted year together with its narration entry. A known local reason may commit its visible local receipt in the same transaction; otherwise the entry remains pending while the accepted year can continue. A failed accepted save leaves the same frozen input observable and retryable. A failed frozen save leaves planning unchanged. Optional narration never calls numeric acceptance.

`AcceptNarration` and `UseLocal` bind to the exact accepted transition and request hash. The first **acknowledged** visible receipt/mode wins. Later responses return false without changing it. An older accepted year's response may complete after continuation, but cannot change the current year or replace a newer report. Missing-key/network/timeout policy belongs to the forthcoming transport adapter; this checkpoint provides the saved local-fallback transaction, not a live HTTP client.

## Save and failure contract

- `AnnualNumericSave` schema 1 contains the town snapshot, ordered per-year requests/receipts, monotonically increasing save sequence and a canonical save hash. It is a distinct envelope from legacy chapter saves. Required numeric state is replayed and each projection is checked against its accepted report on load. No old save is silently converted.
- `IAnnualNumericStore` addresses one explicitly chosen location/run key. `Read` returns null only for absence. `Write(expectedHash, candidate)` atomically checks the old hash (null means absent), writes the entire envelope and completes **only after backend acknowledgement**. A stale window must fail rather than overwrite newer history or a visible mode.
- Any write exception marks the outcome uncertain. Before retry, the controller rereads and verifies the stored envelope. If the previous write committed but its acknowledgement was lost, the accepted year is recovered without spending or saving again. If rereading fails, the old state remains observable; do not claim success. A replaced run, rolled-back sequence or same-sequence conflict is rejected.
- Cancellation before/during a write does not publish an unacknowledged candidate. Cancellation observed after successful acknowledgement does not hide the already committed result. Store implementations must handle their own asynchronous transaction completion and errors correctly.
- A wholly empty Unity-expanded receipt is normalized to null only in a pending entry. A partial receipt or a visible entry with invalid content is rejected. This is source-level compatibility handling, not a Unity JsonUtility test.
- Hashes detect consistency changes, not malicious authorship. This is local game persistence, not an authenticated server ledger. Browser storage may still be cleared or unavailable; no physical power-loss guarantee is claimed.

## Verification

**62 assertions pass** using an in-memory atomic store with controlled acknowledgement gates, before-write failures and lost acknowledgements. Cases cover creation, planning, frozen/accepted saves, exact retry, detached reads, duplicate acceptance, invalid prose, failed local-mode persistence, local/AI races, old-run and older-year replies, two-window stale writes, cancellation, optional DTO expansion, rehashed invalid prose, a complete ten-year run and exact reopening. The final fixture envelope is **997,261 UTF-8 bytes** with the test serializer. [Machine receipt](checks.json). This size is specific to this strategy, draft model and serialization; it is not a measured browser quota or worst-case bound.

The full plain Core/Contracts/Simulation sources compile for **.NET Standard 2.1 / C# 9, zero warnings/errors**. Tests target .NET 8 and run on installed .NET 10 with major roll-forward. No actual IndexedDB, PlayerPrefs, Unity, Chrome or provider call was exercised.

```sh
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/annual-numeric-transactions/AnnualNumericTransactionChecks.csproj
```

## Exact adapter requirements

1. Keep current legacy session/chapter behavior isolated. Do not activate numeric defaults or reinterpret old saves. Add explicit numeric initialization and asynchronous commands; guard synchronous legacy mutation methods when a numeric session is active. The existing annual session remains the sole owner presented to consumers. Do not let its public engine accessor expose mutable live numeric state.
2. Provide one acknowledged store for a separate numeric location/run key. The intended Web adapter uses one IndexedDB object-store record per save and a single `readwrite` transaction for checking `expectedHash` and replacing the envelope. A request's success callback alone is not the commit acknowledgement; resolve on transaction completion, reject on abort/error. Keep game UI on Unity's thread and do not synchronously block awaiting browser work. [IndexedDB transaction lifecycle](https://www.w3.org/TR/IndexedDB-3/#transaction-lifetime) is the relevant API contract; actual Chrome behavior still needs verification.
3. Do not adapt the existing unacknowledged `PlayerPrefs.Save()` call by merely returning a completed Task. Unity documents Web PlayerPrefs storage in IndexedDB with a 1 MB limit; this roughly 1 MB fixture already leaves little room for additional runs/preferences. [Unity PlayerPrefs](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/playerprefs). A new explicit IndexedDB record also avoids rewriting existing preference/save keys.
4. Numeric-save failure must surface as pending/recoverable, with no “year saved” text. A prose-save failure must retain the already saved year but must not present an unpersisted first mode as committed. Retry rereads before mutation. For an acknowledged accepted year, the adapter may independently request `/report-chapter` or automatically persist a local receipt on failure.
5. Continue/fast-forward may progress while prior narration is pending. Map responses to their original request/hash, preserve current view, and ignore late results after a visible receipt is committed. Reopening uses stored requests/receipts, not a previous year's fallback or a new weather draw.
6. Test the concrete browser bridge with quota/abort/stale-key/lost-ack failure hooks, reload and separate runs. Then test actual numeric three-year adaptation, ten-year completion/reload/ending and the combined client. Pure store tests are not browser persistence acceptance.

The next slice is the guarded annual session adapter and explicit browser acknowledgement, coordinated with the numeric owner. Shared HUD/TerraMain and default activation remain outside this owner's file claims. Full visual, final performance and release/rehearsal gates remain open.

## Numeric acceptance precedes optional canonical projection

Follow-up on model `632c159`: accepted numeric reports are now saved with a positional `unprojected` entry before canonical text is built. The history already binds the accepted request/transition hash. A projection/configuration failure or a failed projection save leaves the numeric year accepted and allows continuation; it does not pretend prose is visible. `LastNarrationError` records the optional failure in memory; `PrepareNarration(year, ...)` retries it explicitly. Reopening an unprojected entry requires no successful projection. Empty Unity-expanded inline request/receipt DTOs normalize only when wholly empty. Partial data is rejected.

`ResolveYear` attempts optional preparation after acknowledgement but returns the accepted report even if that presentation step fails. A lost projection acknowledgement requires recovery and retains the first stored receipt. Duplicate accepted submissions never rerun numeric resolution; they may finish an unprojected presentation. Both accepted and prepared publications are separate acknowledged saves. The previous single-envelope local-prose wording describes the superseded checkpoint.

The expanded suite passes **73 assertions**. New cases deliberately break the model identifier, fail projection persistence before/after acknowledgement, reopen/continue an unprojected year, retry an older year's presentation without moving current year, and normalize/reject empty/partial DTO expansions. This is still a fake-store pure-source check, not Unity or browser acceptance. Dense report aggregation remains separate.
