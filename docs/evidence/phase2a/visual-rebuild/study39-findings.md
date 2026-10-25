# Study39 — source-grass mesh LOD candidate

Patterns TN-02/09/12; [reference](../../../DESIGN_REFERENCE.md), [visual goal](../../../PHASE_2A_GOAL.md). Supporting rendering work, not visual acceptance. Study38 bank/native-grass trials failed and were discarded. Study37's42.22FPS/37ms p95 remains the latest actual Chrome timing until this export is tested.

## Evidence-directed cost reduction

A diagnostic inventories candidate triangles in frustum-intersecting instance batches, using the renderer's LOD selection. It is not a GPU profiler, occlusion result, shadow-pass count or frame-time measurement. The initial343.5px Editor viewport exposed several million grass triangles; the matched comparison then fixed the actual Camera target to1280×720, full rect/16:9, with input frozen and the same opening camera.

[Original](study39-before.png) → [aggressive trial](study39-lod.png) → [selected final](study39-final-normal.png). The aggressive54-triangle short grass shrank width and height, so it was rejected. Flower reduction also removed too much of the bounding silhouette and is not included. [Final close](study39-final-close.png) and [same close with all original grass forced](study39-full-close.png) retain the same overall planted structure. Motion changes the water marks between captures. These are camera-only Editor images, not Chrome acceptance.

[Source meshes and tool identity](study39-assets.json); [bake inventory](study39-selected-bake.json). Quaternius CC0 source meshes/materials/UV palette remain the family. [UnityMeshSimplifier](https://github.com/Whinarn/UnityMeshSimplifier), MIT,3.1.1 at53fdb312 is installed through Package Manager, pinned by commit. [License](study39-tool-license.txt). Reproduction: `scripts/art/bake-grass-lods.cs.txt`. No new texture/image generation or purchase.

| Mesh | Original | Medium | Distant |
|---|---:|---:|---:|
| Common short |155|129|100|
| Wispy short |494|321|317|
| Common tall |326|212|113|

Full original geometry remains above20 projected pixels; medium below20, distant below8. The nearest part of each batch controls selection conservatively. Positions/density, material instances, original bounds/height reference, shadow mode, fade distance and annual tint rules remain unchanged. New baked grass is explicitly exempt from the existing forest-only2px cutoff so it does not disappear earlier; the prior soft-groundcover1.5px rule remains.

[Matched opening estimate before](study39-before-cost.json):12,345,760 candidate triangles; [selected after](study39-final-cost.json):9,977,858, about19% fewer. This does not prove faster Chrome rendering. The diagnostic script retains its original generic2px check and may slightly undercount selected grass at1.5–2px; actual renderer preserves those batches. The estimate must not be promoted to a measured GPU saving.

## Verification and remaining work

[54 baked-mesh integrity checks](study39-mesh-check.json) pass: finite channels, valid indices, material slot count, lower triangle count and bounded silhouette changes. [209 lifecycle assertions](study39-annual-check.json) pass with annual ledger/history and analytical terrain unchanged, source materials preserved, stage alpha retained and maturity reset. Own frames21→30 advanced before testing. Current Editor console ground truth:0errors/0warnings, compilation successful; historical retained logs contain old errors. Temporary camera/LOD overrides disappear on Play exit; own Play is stopped.

No visual scores increased. The dominant art gaps remain broad weak ground, repetitive crowns, mechanical crop rows and underdeveloped banks. Optimization cannot substitute for rebuilding those forms. No main integration or public deployment of this candidate yet.

## Chrome checkpoint — performance unresolved

[Export identity](study39-build.json): source `cc04fb2`, successful 480704 ms build, all nine [served files](study39-local-http.json) byte-identical to the local export, 47,838,671 bytes. The recorded pre-existing generated inputs remain part of the build identity. [Opening](study39-chrome-opening.png), [near](study39-chrome-close.png), [overview](study39-chrome-overview.png), and [queued restoration](study39-chrome-proposed.png) show the actual 1280×720 Chrome product. Grass remained visible across the observed zoom/pan route; this does not certify every LOD boundary or mature annual state. Queue/undo returned the reserve to zero and preserved Year 2 cash at $10,923,000. One prior restoration remained establishing at 33%.

The first foreground route failed: **9.64 FPS, p95 144 ms, p99 157 ms, max 168 ms**, six preview events, one placement, 103 camera events, zero unfocused frames. [Returned log](study39-chrome-log.json) contained no error-level entries. Other wasm compilation, Editor and Chrome renderer processes were active; ownership was not established, so they were left alone.

To distinguish a candidate regression from a broader slowdown, closed the candidate and served the unchanged previous export on a separate local port. Every file matched study37's saved hashes. The [control log](study39-control37-log.json) measured **18.04 FPS, p95 75 ms, p99 86 ms, max 93 ms**, versus study37's earlier 42.22 FPS / 37 ms p95. The control [capture](study39-control37.png) also appears softer than the historical image, including HUD text. Reloading the candidate at the fixed viewport did not remove that softness. There is no basis yet to attribute this whole-image difference to grass meshes.

These sequential tests occurred under changing local activity and different saved years (candidate Year 2 with one restoration, control Year 1). They establish neither a clean LOD speedup nor a clean regression. **Performance acceptance remains failed/unresolved; do not integrate as a validated candidate.** Repeat with controlled foreground conditions before deciding whether to retain the optimization. Own game tabs were closed afterward. The next art direction should address coherent terrain/plant/water construction; an asset-family sample is inventoried in [study40 preflight](study40-resource-preflight.md).
