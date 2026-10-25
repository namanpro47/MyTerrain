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
2. Dense projection follow-up3619069 passes432 assertions across30 accepted dense years and preserves material whole-map totals with disclosed selected details. Actual game/UI integration still needs validation.
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

## Exact numeric JSON follow-up (runtime verification in progress)

`AnnualNumericJson` uses Json.NET 13.0.2 / Unity package `com.unity.nuget.newtonsoft-json@3.2.2` only for schema-3 definition and numeric-save transport. Type names are disabled, unknown members/extra content are rejected, culture is invariant, optional nulls remain null, and input is bounded to64Mi characters/depth64. Stored JSON is passed verbatim through IndexedDB; parsing the JavaScript wrapper does not rewrite its numerical text. Legacy session saves continue using their existing JsonUtility path. Control callback envelopes contain strings/boolean only and still use JsonUtility safely. Numeric DTOs have an explicit contract-assembly linker preservation file.

Forty source/adapter assertions now pass, including the real one-ULP regression value, complete envelope/definition hashes, culture invariance and invalid metadata/content. The portable C#9/.NET Standard2.1 library builds with0 warnings/errors. The exact codec, frozen ten-year saved envelope, and installed Unity package DLL also pass **25 assertions under Unity's bundled ARM64 Mono**, including all10 accepted reports/narration hashes and its own full-envelope/definition round trips. [Mono result](mono-codec.txt), [binary/vector identities](mono-codec-identity.json). The ten-year save hash remains `d3e053d11f2158744046fd6b77fa0fff166f4b24df4074909d9674802e8a9c37`; no values or accepted hashes were rounded.

The root worktree reconciled combined candidate770d4c0 as merge7b7a004, including compatibility fixes88f9a4a. Its current source remains separate from the frozen public export. The disposable project installs its direct package pin through Unity's PackageManager API using `web-player/PackageInstaller.cs`; actual Editor/IL2CPP/WebGL results are still pending. The main project's package is currently transitive through Pipeline; explicitly pin it through its owner's UPM workflow before numeric activation. No default Main/HUD/scene activation is included.

References: [Unity's packaged Json.NET version](https://docs.unity.cn/Packages/com.unity.nuget.newtonsoft-json%403.2/manual/index.html) and [serializer settings](https://www.newtonsoft.com/json/help/html/t_newtonsoft_json_jsonserializersettings.htm). The installed3.2.2 package.json independently identifies Json.NET13.0.2.

The same exact-codec Mono probe now includes the real340-site Suusamyr ten-year fixture: **50 assertions across20 accepted years** pass, including every transition/narration binding and complete sparse/dense envelope round trips. Dense save hash: `3d9a2bf2c037b2ef857f7336722ee2d80e93928686ec4c3f255759e6428b7a5d`. Its fixture generator reruns144 dense projection checks and exports the12,685,309-byte save into ignored Builds storage. An initial probe assumed all dense narration entries were visible; the fixture intentionally retains pending earlier entries, so the corrected test validates both statuses and their exact request/optional receipt identities.

The prepared adapter, check scene, builder and installer also compile against the installed Unity6000.6.6f1 API assemblies in C#9/.NET Standard2.1, both Editor and WebGL conditional branches, with0 warnings/errors. This remains API compilation, not an executed Editor or IL2CPP player. See `tests/annual-numeric-client/web-player/UnityApiCompile.csproj`; supply `UnityManagedPath` and `UnityNewtonsoftPath` for the installed libraries.

The linked-player harness records definition parsing, opening, reload and total elapsed milliseconds with its functional result. These are isolated fixture diagnostics, not full-game frame-time/load acceptance. Both Unity API compilation branches still pass after this instrumentation.

## Actual Unity Editor result

The disposable Unity6000.6.6f1 project passed **23 assertions** against source `ec39e38`: full sparse and340-site dense ten-year saves replay with exact envelope/report hashes; existing saves are not rewritten; fresh initial/planning writes acknowledge; optional projection failure retains the accepted numeric year; later local narration survives reload; legacy saving remains guarded. The package installer exited0 and UPM resolved the direct Newtonsoft3.2.2 pin. [Editor result](unity-checks.json) and [project/package/source identity](unity-project-identity.json). This check used injected JSON storage and did not touch PlayerPrefs or production scenes. The linked WebGL build/actual browser checks are still pending.

The isolated IL2CPP/WebGL export then completed with exit0: five files,24,997,736 bytes uncompressed, including both fixtures. [Exact export identity](webgl-build-identity.json). This is a minimal test scene, not a production size/load/performance result. Chrome functional verification is queued separately.

## Actual Chrome linked player: sparse pass, dense failure

The sourceec39e38 IL2CPP player passed8 fresh sparse-run assertions and4 after a page reload, retaining the same complete save/report hashes and local narration. The sparse first-run check completed in1.93s; the page-reload check completed in0.45s. These are one-shot fixture timings under shared machine activity, not game performance acceptance. [Fresh result](webgl-first-run.json), [reload](webgl-reload.json), [capture](chrome-webgl-first-run.png), [reload capture](chrome-webgl-reload.png).

The real340-site ten-year fixture then aborted with WebAssembly `OOM` and its tab stalled. **Numeric activation remains blocked.** No dense success callback/reload was obtained. The generated test project had32MiB initial,2048MiB maximum and geometric growth; the game's initial allocation is256MiB. This is not evidence that increasing initial memory solves the failure. [Recorded observation](webgl-dense-failure.json). Only the owned test tab was closed; the local server and desktop122 were released.

Diagnostic follow-up adds visible stage, managed-memory and Wasm-heap-capacity reports, posted to the isolated local evidence server. It deliberately introduces no yields or heap-setting changes, so the next run can localize the existing failure. Unity6000.6 [webgl-memory.html](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-memory.html) explains end-of-frame garbage collection and temporary allocation pressure. Native allocation probes remain separate from actual WebGL peak measurements.
