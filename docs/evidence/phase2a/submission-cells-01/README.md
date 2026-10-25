# Larger nature submission cells — provisional candidate

October 25, 2026. Source baseline `53139b0`, including the visual owner's conifer checkpoint `6e1b3cb`. This bounded experiment changes only NatureSubmission spatial aggregation from 96m to 192m, after existing per-source LOD selection and culling. Meshes, source transforms, source bounds, material identities, maximum 500-instance draw capacity, and the existing large/transparent-batch bypass remain unchanged. An Editor-only comparison override supports the paired audit. This is **not Phase 2A acceptance or a measured Web performance improvement**. TN-01/03 whole-scene and camera readability remain requirements; improved batching cannot establish scenery quality.

## Editor evidence

Unity 6000.6.6f1, 1280×720 render target, display pixel ratio 1, frozen camera and landscape animation. An initial startup frame-advance wait timed out; a later wait proved frame advancement before these tests. The first replay script failed to compile; the corrected committed script and successful receipt supersede that attempt.

| View | Small source batches | 96m draws | 192m draws | Total submitted sources | Culled sources |
|---|---:|---:|---:|---:|---:|
| Normal | 894 | 676 | 350 | 1094 | 1481 |
| Close | 495 | 367 | 215 | 616 | 1966 |

Counts exclude bypass draws from the consolidated-draw columns. Source submission/culling counts are identical within each pair. Captures were directly inspected: no visible loss or changed placement. They are camera output without HUD, not Chrome captures. The normal pair has 4,188 changed pixels (211 above a two-level channel difference; max 25/255, mean<0.0033/255); close has 20 changed pixels, all ≤1/255. They are not claimed pixel-identical. Small depth/order differences remain possible after regrouping.

The all-active-scene, full-detail replay processes 1,816 eligible source batches: 1,372→659 draws. All 30,558 instances and exact mesh/material/submesh/shadow/matrix multiplicities are retained, each source bounds fits its compatible output union, and no draw exceeds 500. This audit deliberately does not exercise visibility selection, runtime LOD choice or GPU time. The fifteen collector checks pass at both 96m and 192m: compatibility separation, transform order/nonuniform scale, union bounds/shadow settings, cell separation, buffer rollover/reuse, frame reset and disable cleanup.

[Normal 96m](normal-96.png), [normal 192m](normal-192.png), [close 96m](close-96.png), [close 192m](close-192.png). [Exact replay](replay.json), [pixel comparison](pixel-diff.json), [96m checks](checks-96.json), [192m checks](checks-192.json). `hashes.json` identifies the archived inputs and outputs.

## Browser comparison pending

A local copy of the owner's existing conifer 96m export was preserved at `Builds/Verified/study62-conifer-96`; every one of the 31 served files matched the owner's Study 62 receipt before copying. [Control served hashes](control-served.json). Its receipt identifies actual bytes; this document does not retroactively assert a clean-source build provenance absent from that receipt. The 192m runtime change is candidate-only until actual paired Chrome timing and interaction checks finish. No main promotion or deployment is implied.
