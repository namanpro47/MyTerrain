# Crop spatial source groups — preserved content, lower selected geometry

October 25, 2026. Baseline `2d68e8c` includes the combined study64 crop/material source and 192m submission collector. The [actual Chrome failure](../combined-crops-browser-01/README.md) motivates this bounded candidate. Only `LandscapeRenderer.CropBeds.cs` changes runtime: group the existing plant matrices by 32m X/Z cells before passing each group into `InstancedNature.Add`. No plant, soil vertex, material, camera, source LOD mesh, LOD pixel threshold, annual model or site eligibility changes.

Previously u-major generation and fixed 128-instance chunks could span a whole crop row. Conservative nearest-edge LOD then retained higher detail across the long batch. Full 128-instance arrays also bypass the shared collector. Local source groups let existing culling and LOD act on tighter bounds; the existing 192m collector still combines compatible selected small groups afterward. Grouping happens during crop construction, not each frame.

Unity 6000.6.6f1, fresh advancing Play sessions, 1280×720 fixed camera output, pixel ratio 1 and frozen landscape animation. Compilation completed with zero errors/warnings. [Advancing frames](frames-spatial.json). These camera captures omit the overlay HUD and are not browser performance evidence.

| View | Selected leaf triangles before → after | Total instance draw calls before → after | Near / mid / far instances after |
|---|---:|---:|---:|
| Normal | 2,761,524 → 1,603,806 | 499 → 452 | 0 / 3,559 / 2,569 |
| Close | 5,344,236 → 2,282,274 | 295 → 261 | 592 / 3,739 / 0 |

Triangle totals replay the active camera's existing source culling/LOD rule over actual batches; they are submitted leaf geometry, not GPU rasterized triangles or measured GPU duration. Total draw counts include collector plus bypass submissions for all nature. Maximum crop batch X/Z span falls 150.60→34.85m. [Raw profiles and parsed summary](summary.json).

## Appearance and content invariants

TN-01/02/09/10 remain the reference patterns. Directly inspected [normal control](normal-control.png), [normal candidate](normal-spatial.png), [close control](close-control.png) and [close candidate](close-spatial.png): row layout, density, soil form and whole field remain intact. Expected leaf silhouette/shadow differences follow the existing LOD meshes; these are **not pixel-identical** views. Normal has 3,167 changed pixels, 1,782 above two channel levels; close has 86,554 changed pixels, 37,899 above two. Mean RGB absolute differences are 0.042/0.042/0.020 and 0.659/0.669/0.335 on a 0–255 scale. Some edge pixels differ substantially; inspect the images, not just the averages. Whole-scene acceptance and moving LOD transitions remain open.

The canonical fingerprint sorts individual plant-matrix hashes and complete soil mesh hashes, making it independent of source batch order. It matches exactly before grouping, after fresh construction, and after traditional/margin/rotation preview/apply/remove: **38,435 matrices, 1,282,588 soil vertices, 38,483 records**, SHA256 `a1df3b13074c1840862e21717a5db658b5096e51fa17b477aa7809bd0e37fcf9`. Mesh hashes include vertices, normals, UVs and triangle indices. This verifies unchanged content, not identical culling/LOD choices.

The [lifecycle audit](lifecycle-result.json) checks 348,424 plant-bound corners and 1,464,388 soil vertex visits across baseline and repeated three-action paths. These are repeated inspection totals, not unique world objects. Owned mesh counts and surface masks restore; 2,025 other vegetation batches retain their original LOD extent/cutoff. Annual Year1/Planning/revision0/$10M/zero proposals/zero accepted assets remains unchanged. Shader errors are empty. One ground-shader warning appears in cached shader messages after a separate discarded early-return experiment and remains after restoring the original shader; no new compiler-warning cause is established. Console ground truth shows no errors/warnings. No production shader change is included.

A separate raised-soil early-return trial produced pixel-identical views but no measured performance benefit and a shader warning investigation; it was reverted. This candidate addresses measured batch structure instead. The existing collector implementation is unchanged. Final combined Chrome timing, motion, public export and visual gates remain required before promotion.
