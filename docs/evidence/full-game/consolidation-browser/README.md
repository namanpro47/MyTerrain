# Consolidated vegetation — actual Chrome checkpoint

October 25, 2026. Clean source **`2f8543dd2734172942f92c1e3395b695174395e9`** combines study55 restoration, shared materials and nearby small-batch consolidation. Its three measured routes meet the documented stable-30 fallback; three fresh Explore-to-game entries are below15s. **This is a bounded engineering checkpoint, not Phase 2A scenery acceptance or a public release.** TN-02/05/09/10 and AX-01 still apply. Crop repetition and broader ground/forest cohesion remain under visual revision.

## Exact export

[Build](build.json), [clean source provenance](provenance.json), [identity](identity.json). Unity6000.6.6f1/CLI1.0.0-beta.14 completed with no reported Editor errors. Frozen output is `Builds/Verified/2f8543d`. All **31 runtime files /46,425,461 bytes** anonymously [match served bytes](web-assets.json); Brotli payloads decode correctly. This total includes all three location packs and atlas assets, so it is an upper bound on one arrival's body transfer, not a memory measurement.

[Ignored-input audit](ignored-assets.json):32 ignored Assets files; no ignored code/shader/DLL/prefab/asset/scene; all12 generated world JSON files match committed sources. The runtime candidate remains isolated from main's older vegetation renderer. No art is promoted merely because this performance check passes.

## Foreground Chrome measurements

Chrome155.0.8059.85 on the same macOS/M1 Max development machine as preceding checkpoints, **1280×720**, normal text/full motion. Own build/Editor and the visual owner's Editor were closed. Existing unrelated project Editors remained open; the machine was not otherwise isolated. Each route is one actual30-second sample, not a repeated statistical benchmark.

| Route | Seconds / frames | Average FPS | p50 / p95 / p99 ms | Max ms | >33.34 / >200 ms |
|---|---:|---:|---:|---:|---:|
| Opening camera |30.028 /1461|48.65|18 /33 /47|71|65 /0|
| Restoration editing |30.035 /1648|54.87|16 /31 /45|58|51 /0|
| Year10 camera |30.001 /1639|54.63|16 /29 /44|66|47 /0|

All three meet **p95≤33.3ms**; the strict≤16.7ms target is not met. DOM telemetry confirms visible/focused start/end, zero blur/visibility changes and zero Unity unfocused frames. [Summary](summary.json), [opening](camera.json), [editing](editing.json), [Year10](year10-camera.json). No error-level logs were returned.

Camera paths use R, F9 and Right/Down/Left/Up1.2-second holds in two three-cycle groups, with a state observation between. They produced1117/1274 camera events. Editing measured7 previews,1 placement, undo/redo and reselection; attempted camera movement occurred after the30-second window (**0 measured camera events**). Thus its lower frame time is not a matched reproduction of the previous moving/two-placement editing sample. End-of-sample source batches were828/789/819, with **148/143/146 fewer actual draw calls** from consolidation. This confirms the mechanism in WebGL, not a single-variable causal FPS comparison: study55 and path differences also separate this export from `16691f2`.

Preview CPU maximum0.9ms; placement19.6ms. DOM→first-rAF-after-feedback maximum preview22.6ms and placement39.8ms, within100/200ms bounds. These measure a presentation opportunity, not physical display latency.

## Annual state and render lifecycle

Actual inputs queued West-field restoration, undid and redid it. Cash remained$10M with$95K reserved. A later attempted second-site selection did not add another proposal; this receipt claims **one** project. Confirmed fast-forward reached Year10 in747ms including yields/final world refresh, with **$19.755M**, meters **41.3 /58.0 /45.2**, one operating restoration and no remaining obligations.

Opening comparison removed the illustrative project while retaining accepted finances/meters. T then Escape restored the accepted landscape. Reload restored Year10/Completed/revision22, the same cash and one asset. Confirmed New run returned to Year1/Planning/$10M and onboarding. [Full logs](full-run.json), [ending](year10.jpg), [comparison](comparison-opening.jpg), [restored land](comparison-restored.jpg), [reload](reload.jpg), [replay](replay.jpg). Runtime counters at the future report:129,963,796 allocated /187,891,712 reserved bytes,460 meshes,125 materials. These are Unity counters, not full browser/GPU memory.

## Three fresh entries

Each run used a different local origin, a new tab set to1280×720 before navigation, first Unity load and no prior save. Prior game tabs were closed before the next arrival. [Raw receipts and screenshot dimensions](arrivals.json).

| Origin port | Explore handler → visible focused game |
|---|---:|
|61256|12.306s|
|61257|12.342s|
|61258|12.013s|

The new `TERRA_ENTRY_TIMING` instrumentation is verified in the actual export. It includes loader/runtime/scenery work and the cinematic transition, excludes time choosing on the globe, and retains the historical navigation-based timer. These are fresh-origin local observations on a warm machine, not cold-device or public-network guarantees. [Arrival1](arrival-1.jpg), [arrival2](arrival-2.jpg), [arrival3](arrival-3.jpg). The separately recorded reload is warm and is not counted among these three.

All test tabs and local servers were closed; viewport override reset. No public deployment, final rehearsal, fallback video, novice, physical listening or low-end test is claimed. Subsequent visual changes require their own final integrated performance/arrival verification.
