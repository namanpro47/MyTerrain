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

Web export, actual Chrome near/far/transition inspection, queue/undo and controlled performance are next. No visual scores increased. The dominant art gaps remain broad weak ground, repetitive crowns, mechanical crop rows and underdeveloped banks. Optimization cannot substitute for rebuilding those forms. No main integration or public deployment of this candidate yet.
