# Larger nature submission cells — provisional candidate

October 25, 2026. Source baseline `53139b0`, including the visual owner's conifer checkpoint `6e1b3cb`. This bounded experiment changes only NatureSubmission spatial aggregation from 96m to 192m, after existing per-source LOD selection and culling. Meshes, source transforms, source bounds, material identities, maximum 500-instance draw capacity, and the existing large/transparent-batch bypass remain unchanged. An Editor-only comparison override supports the paired audit. This is **not Phase 2A acceptance or a measured Web performance improvement**. TN-01/02 whole-scene and camera readability remain requirements; improved batching cannot establish scenery quality.

## Editor evidence

Unity 6000.6.6f1, 1280×720 render target, display pixel ratio 1, frozen camera and landscape animation. An initial startup frame-advance wait timed out; a later wait proved frame advancement before these tests. The first replay script failed to compile; the corrected committed script and successful receipt supersede that attempt.

| View | Small source batches | 96m draws | 192m draws | Total submitted sources | Culled sources |
|---|---:|---:|---:|---:|---:|
| Normal | 894 | 676 | 350 | 1094 | 1481 |
| Close | 495 | 367 | 215 | 616 | 1966 |

Counts exclude bypass draws from the consolidated-draw columns. Source submission/culling counts are identical within each pair. Captures were directly inspected: no visible loss or changed placement. They are camera output without HUD, not Chrome captures. The normal pair has 4,188 changed pixels (211 above a two-level channel difference; max 25/255, mean<0.0033/255); close has 20 changed pixels, all ≤1/255. They are not claimed pixel-identical. Small depth/order differences remain possible after regrouping.

The all-active-scene, full-detail replay processes 1,816 eligible source batches: 1,372→659 draws. All 30,558 instances and exact mesh/material/submesh/shadow/matrix multiplicities are retained, each source bounds fits its compatible output union, and no draw exceeds 500. This audit deliberately does not exercise visibility selection, runtime LOD choice or GPU time. The fifteen collector checks pass at both 96m and 192m: compatibility separation, transform order/nonuniform scale, union bounds/shadow settings, cell separation, buffer rollover/reuse, frame reset and disable cleanup.

[Normal 96m](normal-96.png), [normal 192m](normal-192.png), [close 96m](close-96.png), [close 192m](close-192.png). [Exact replay](replay.json), [pixel comparison](pixel-diff.json), [96m checks](checks-96.json), [192m checks](checks-192.json). `hashes.json` identifies the archived inputs and outputs.

## Browser comparison — bounded stable-30 result

A local copy of the owner's existing conifer 96m export was preserved at `Builds/Verified/study62-conifer-96`; every one of the 31 served files matched the owner's Study 62 receipt before copying. [Control served hashes](control-served.json). Its receipt identifies actual bytes; this document does not retroactively assert a clean-source build provenance absent from that receipt. The 192m runtime change now has the bounded Chrome result below. No main promotion or deployment is implied.


The source `d620ff2` Web export succeeds and is frozen at `Builds/Verified/d620ff2`: **31 files /48,086,076 bytes**, all anonymously served hashes verified, with valid Brotli decoding. [Build result](browser/build-result.json), [identity](browser/identity.json), [source/input provenance](browser/source-provenance.json), [candidate verification](browser/candidate-verified.json), [control verification](browser/control-verified.json). The Editor began from a clean committed checkout; during export it deleted two stale DOTween debug metadata files. They were restored after shutdown. The32 ignored Assets inputs contain no code/shader/DLL/scene/prefab/asset; only two test-resource metadata hashes differ from the prior audited inputs. The owner's control retains its explicitly documented imported/dirty-input scope. Across the two exports27 of31 runtime paths/hash pairs match; data/wasm/loader/index differ. Runtime tracked source between the conifer checkpoint and pre-change baseline is identical. This remains a sequential diagnostic comparison, not proof that every build input was controlled.

Chrome **155.0.8059.85**, 1280×720, full motion and normal text. The own Editor/build was closed; the visual chat closed its game tab. Other unrelated applications/Editors remained open. Control then candidate, with only one test game open at a time. Each route used R, F9, then six Right/Down/Left/Up cycles with1.2-second holds, split into two groups of three with a state observation between. Command gaps and exact camera positions can differ. [Raw control logs](browser/control-chrome.json), [candidate logs](browser/candidate-full.json), [summary](browser/summary.json).

| Export | Seconds / frames | Average FPS | p50 /p95 /p99 ms | Max ms | Frames >33ms / >200ms |
|---|---:|---:|---:|---:|---:|
| Conifer96m control |30.017 /1252|41.71|20 /39 /57|80|237 /0|
| Candidate192m |30.013 /1659|55.28|17 /30 /43|65|48 /0|

The candidate meets p95≤33.3ms for this one path; strict60 does not pass. Both show visible/focused start/end, no blur/visibility changes and zero unfocused Unity frames. Camera events903/1097 confirm movement. End-of-sample consolidation is922→704 versus904→360; total submitted1136/1115. These endpoint counters are not matched-camera profiling. No preview or placement occurred inside the timed windows, so this check makes no input-latency claim. There are no error-level logs; both retain the same unsupported optional FSR post-processing shader warning.

[Control normal](browser/control-normal.jpg) and [candidate normal](browser/candidate-normal.jpg) were directly inspected, with no visible missing vegetation. Actual candidate inputs queue West-field restoration then undo: [queued](browser/candidate-queued.jpg), [undo](browser/candidate-undo.jpg). F10 reports Year1/Planning/revision1/one proposal then revision2/zero proposals, stored cash$10M and zero accepted assets throughout. The UI shows$95K reserved before undo. This does not certify later-year rendering. Candidate initially loaded at the default larger viewport, then was resized before timing; its entry timer is not a matched-resolution comparison or three-entry acceptance.

Both test tabs and local servers are closed and the viewport override reset. The crop integration `4488af9` was not included; the final combined scene still needs three moving/editing/mature timing paths, fresh arrivals, visual acceptance and full public/rehearsal/recording checks. The tested192m change has been handed to the visual owner for integration.
