# Study45 — full Wisconsin canopy and crop distance correction

Patterns TN-01/02/09/10/12. This follows the [asset checkpoint](study45-checkpoint.md), with incoming main through 8093653 merged at ec56597. The lightweight stream improvement is included. Phase 2A remains open.

## Actual Editor observations

[Normal](study45-final-normal.png), [close](study45-close.png), [wide](study45-final-wide.png). These are 1280×720 camera captures, excluding overlay HUD. The camera replay uses the normal 34° pitch/reset framing, 40°/zoom40 for the woodland close diagnostic, and reset framing/zoom275 for wide. The camera controller is temporarily disabled after positioning; the player loop continues. First observed advancing frames13→20; fresh corrected session450→458. No camera change is persisted.

Continuous woodland now runs across Wisconsin, with three metric tree forms, lower source-woodland edges, broad glades, cluster normals and full/mid/far geometry. The wide [full-detail control](study45-full-wide.png) versus [automatic LOD](study45-wide.png) retains the broad crown envelope; reduced meshes have coarser leaf shapes and remain subject to browser motion review. At the wide diagnostic,842 woodland submesh batches choose10 full,14 mid and818 far levels across the map; these are not all visible draws. The inherited native instancer handles culling. This is stronger canopy massing, not proof of reference parity.

The initial wide view exposes broad crop dropouts. Turning off CPU crop culling alone yields identical pixels; disabling the shader fade restores the rows. The candidate delays fade from2.4–4.2pixels to0.6–1.4pixels and updates the conservative CPU cutoff to0.6. [No-fade control](study45-crop-no-fade.png), [subpixel-fade trial](study45-crop-subpixel.png), and [fresh final runtime](study45-final-wide.png) show restored continuity. This affects all regions and requires browser cost/motion review. Close imagery is unchanged in intent; source grain art and placement remain intact.

## Verification and limitations

Compilation completed with zero errors/warnings. [Atlas and annual checks](study45-merged-checks.json):678 atlas/save assertions and34 annual lifecycle/JSON checks pass. [Rendering lifecycle](study45-lifecycle.json):209 assertions pass across17 variants, including owned material tint/alpha, unchanged source materials, byte-identical annual ledger/history and analytical terrain. [Current console ground truth](study45-console.json) distinguishes current counts from retained historical logs. The first console query returned an older09:21 teardown exception, not an exception from this fresh run.

Point Reyes and Suusamyr keep this branch’s earlier vegetation path when the Wisconsin clustered flag is false. **That is not identical to current main’s ErceTree path:** earlier branch-wide grass, instancing and ground changes also affect those regions. Regional browser smoke and performance remain pending. This study makes no claim that only Wisconsin differs from main.

Dominant remaining failures: grass speckle over an evenly green base, repetitive field rhythm and weak surface/contact richness. Wide silhouette repetition remains visible. These are substantial visual gaps; no dimension is rated4.5. The temporary crop shader and runtime controls were discarded; only the delayed crop fade/CPU threshold are retained with the canopy integration. No purchase or generated image. Chrome export and review follow this source checkpoint.
