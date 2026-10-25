# Study64 — framing comparisons and quieter cultivated surfaces

TN-01/02/09/12; [persistent reference](../../../DESIGN_REFERENCE.md), [unchanged visual gate](../../../PHASE_2A_GOAL.md). The source starts from7f258cc. **Editor candidate, not Chrome acceptance.** No image generation or new asset purchase.

## Camera alternatives, rejected as the solution

All captures are actual1280×720 camera renders, with the input controller disabled and frames confirmed advancing. [Control](study64-control.png) retains perspective/yaw−32/pitch34/zoom105/target(−90,30,−70). [Orthographic](study64-orthographic.png) keeps the target and yaw, pitch45/size85. It sharpens forms but leaves the large empty ground intact. [Woodland close](study64-woodland-close.png), perspective/pitch42/zoom72/target(−75,30,45), reveals forest structure but loses the field. [River framing](study64-river.png), orthographic/yaw−52/pitch48/size75/target(−120,25,−75), improves the diagonal river but still separates the landscape elements with weak ground. Exact settings are in the corresponding JSON receipts.

None solves the dominant whole-scene failure. No production camera or movement limits changed. Do not use these framings as a way to hide asset limitations.

## Retained bounded material change

Study63's close crop view exposes mottled yellow soil competing with the leaf silhouettes. [Quiet-soil normal](study64-quiet-soil-normal.png) and [close](study64-quiet-soil-close.png) remove texture entirely as a diagnostic; [green-leaf close](study64-green-leaf-close.png) adapts the same source geometry to a clearer green. These are intermediate controls, not separate accepted art directions.

The integrated candidate restores15% of the existing JulioVII earth paint within a quieter soil base. `TerraGround.CultivatedSoil` supplies the same world-space material function for the raised bed meshes and underlying cultivated land. The raised soil uses the existing ground shader with an explicit flag, and the cultivated base no longer adds unrelated earth normal-map bumps over the constructed ridge form. Other ground/river/path/woodland behavior remains unchanged. `CultivatedCropAssets.Build` writes the persistent soil material and leaf palette(.38,.62,.25), with legacy color aliases synchronized. The creator's leaf geometry, LODs, positions, row layout and source FBX are unchanged.

[Integrated normal](study64-integrated-normal.png), [integrated close](study64-integrated-close.png). Bed shapes and leaves read more clearly than study63, but repetition, open-ground composition, woodland transitions and whole-scene richness remain substantial gaps. This small refinement of the new crop construction does not establish Terra Nil parity or any4.5 score. Do not continue tint-only iterations as the next major art direction.

## Checks and next work

[Compilation](study64-compile.json):zero errors/warnings. [Material bake](study64-material-build.json):same1,392/390/84 triangles. Fresh Play-mode construction and advancing frames precede integrated captures. [Post-render check](study64-render-check.json):no ground shader errors,48 raised-soil surfaces, Year1/Planning/revision0/cash1,000,000,000 cents/queue0/assets0 unchanged. No new geometry or annual-state implementation was introduced, so the preceding geometry lifecycle evidence remains scoped to those unchanged routines; this is not a new full gameplay test.

The temporary diagnostic shader was archived then removed through AssetDatabase; Play exit discarded transient material clones. Production changes are limited to ground/crop materials, their builder and shader. The HUD is absent from Editor camera captures. The first Chrome crop opening remains study63; new combined build, Chrome visual/interaction checks and performance are pending. Next substantial visual work must connect the broad meadow, woodland edges and cultivated fields with coherent environmental forms, rather than repeat camera/tint experiments.
