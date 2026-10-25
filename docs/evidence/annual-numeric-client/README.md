# Inactive numeric session and acknowledged browser store

October 25, 2026. Additive integration checkpoint. **Not activated in TerraMain, HUD, scenes or hosted deployment. Not a completed annual simulation or phase gate.**

Base: remote main `ea37af047f4d8d167f88b802e76827c82a6c8183`, reconciled in `7c09e5e`; numeric owner `9c0d156` and `632c159` are included as `c2788d8` and `a4af242`. Dependencies include root report projection `388267d`, transaction source `f74d72a` and required metadata correction `fc31901`. Main's dense-parcel schema-2/version-4 gameplay, 16 MiB chapter body limit, and optional chapter port remain intact.

## Behavior

`AnnualTownSession.InitializeNumericAsync` opts into the acknowledged controller with an injected `IAnnualNumericStore`. A failed or waiting initial save leaves the previous session observable. A successful save switches the read model and publishes detached snapshots. Every synchronous legacy mutation/save/import/export entry point is rejected while numeric initialization or numeric mode is active. Schema 3 cannot enter the old synchronous initializer. Numeric `Engine` returns a detached engine, so its mutations cannot bypass storage. Separate async methods expose plan changes, resolution, continuation, local/remote narration and recovery. No automatic conversion or migration of existing saves is implemented.

`AnnualNumericBrowserStore.Create` creates one dedicated callback object per explicit location/definition/run. Its key contains the numeric namespace, world pack, schema 3, definition hash and run ID. It uses separate IndexedDB database `MyTerrainNumericSavesV1`, store `runs`. A single readwrite transaction gets the old hash, compares it, and replaces the complete JSON record. Success is acknowledged only at transaction completion, with strict durability requested. Request success alone cannot publish a year. Failure, blocked opening, quota, timeout, cancellation and teardown are explicit failures/unknown outcomes; a late write may still commit, so the transaction controller rereads before retrying. No physical power-loss guarantee is claimed. The owner must destroy the dedicated store GameObject when leaving the run.

The editor/non-WebGL implementation explicitly reports unsupported storage. Tests inject a fake store; PlayerPrefs is never represented as acknowledged browser persistence.

## Verification at this checkpoint

- 32 pure adapter assertions: legacy preservation/guards, blocked opening/plan publication, detached reads, accepted year/local prose, reopen, separate keys, destruction during save. Unity stubs are used, so this is **not** Unity/JsonUtility validation.
- 33 JavaScript bridge fault assertions using controlled IndexedDB mocks: no acknowledgement before commit, abort after put, stale hash conflict, missing/corrupt read, synchronous get/put quota failures, unavailable storage, blocked opening with late success, duplicate callback and invalid envelope. These are **not** actual browser transactions or a WebGL-linked test.
- Commands: `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/annual-numeric-client/AnnualNumericClientChecks.csproj`; `node tests/annual-numeric-client/storage-checks.mjs`. .NET 10.0.13 runs the net8-targeted harness by explicit roll-forward. Tests also compile the WebGL conditional branch with `-p:DefineConstants=UNITY_WEBGL`.

## Required before activation

1. The persist-first follow-up now saves numeric acceptance with an unprojected narration entry before optional projection. The 73-assertion transaction suite covers projection/configuration/save failures and independent retry. Verify this new lifecycle through the actual Unity/browser adapter; pure-source checks alone do not establish runtime acceptance.
2. Bound dense projections without losing material aggregate outcomes. Current sparse projection checks are not dense acceptance.
3. Run actual Unity compilation/JsonUtility round-trip and actual Chrome IndexedDB transactions, two-window contention, reload and >20 MiB envelopes. The owner measured roughly 12.6 MB dense ten-year snapshot plus 6.44 MB definition before projection; no arbitrary 1 MB limit is introduced here, but real storage capacity and processing time remain unverified.
4. Wire explicit async controls, numeric read/planning ranges, rendering, report transport and fallback through their owners. The main game's synchronous code must not be pointed at this mode.
5. Dense factory callers must pass the actual loaded `WorldData` to preserve partial cultivated fractions. Keep all three places/run keys separate and disclose shared numerical reference.

The active visual gate, final performance, public ten-year playthrough and rehearsal remain open independently.

## References

Unity documents string callbacks to GameObjects through [SendMessage](https://docs.unity.cn/6000.0/Documentation/Manual/web-interacting-browser-js-to-unity.html). The [IndexedDB transaction lifecycle](https://www.w3.org/TR/IndexedDB-3/#transaction-lifetime) distinguishes individual request completion from transaction completion. Neither reference establishes that this implementation has passed the pending WebGL/Chrome checks.

## Actual Chrome storage checks and a separate numeric codec blocker

On October 25 at 18:53 UTC, Chrome 155 ran the committed JavaScript bridge on isolated localhost port 8879. Eight actual IndexedDB checks passed: absent key, initial transaction acknowledgement, two concurrent independent connections with exactly one CAS winner, complete winner readback, abort after successful put with no false acknowledgement, preservation of the prior value, and exact write/read of a **25 MiB synthetic payload**. A subsequent page reload preserved the exact payload and identity. [Result](real-indexeddb.json), [reload](after-reload.json), [capture](chrome-indexeddb.png), [reload capture](chrome-indexeddb-reload.png). The two connections were in one tab, not a claimed two-window UI test. The explicit page controls run this harness; no hidden browser-state inspection was used. Test tab/server were closed and desktop88 released.

This validates the JavaScript storage layer only. The release owner's actual Unity model checks found that `JsonUtility` parses some .NET double decimals with a one-ULP difference, breaking exact report/definition hashes. Numeric runtime remains **inactive**. A numeric-only exact JSON codec must replace JsonUtility at definition/save transport boundaries; the legacy game's serialization stays separate. The numeric owner has demonstrated exact four-decade round trips with Unity's existing Newtonsoft package on bundled Mono, but IL2CPP/WebGL still needs verification.

`Assets/Editor/AnnualNumericClientChecks.cs` and `tests/annual-numeric-client/web-player/` prepare disposable actual-Unity/linked-WebGL tests. **They have not run at this checkpoint and still use the known-problem JsonUtility path.** The .NET harness generates exact ten-year vectors with `--unity-vectors` into ignored `Builds/AnnualNumericClientVectors`; the current vector save hash is `d3e053d11f2158744046fd6b77fa0fff166f4b24df4074909d9674802e8a9c37`. Adopt the final compatibility fixes and exact codec before claiming those runtime checks.
