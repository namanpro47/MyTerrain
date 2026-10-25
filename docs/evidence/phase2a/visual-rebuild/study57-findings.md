# Study 57 — solid-leaf crop comparison, rejected

October 25, 2026. TN-02/09/12. Previous goal turn made progress by archiving the rejected study56 constructions and acquiring licensed source files. This turn imports and tests those sources, integrates the separately verified vegetation collector, and continues to a distinct surface-baking experiment in [study58](study58-findings.md). Phase 2A remains open.

## Source and actual import

The two Quaternius corn files and their CC0 license are preserved in the [study56 source record](study56-crop-sources.json). The [Unity import](study57-import.json) finds 684 vertices / 348 triangles / one material for Corn_3, and 1,903 / 958 / three materials for Corn_4. Source scale is 0.01 with a 100× model-root scale and approximately −90° X conversion; wrapper prefabs preserve those transforms. The source's green/yellow colors are copied to matte URP Simple Lit materials. [Prefab receipt](study57-prefabs.json), [replay](study57-prefabs.cs.txt). This is a native model/material adaptation, not image generation. To replay, copy the two archived study56 FBX files to `Assets/Terra/Art/QuaterniusCrops/Corn_3.fbx` and `Corn_4.fbx`, run the import and prefab scripts outside Assets, then the Play-mode layout/camera scripts.

## Matched scene comparison

At source `5b8ad52`, the actual Wisconsin West field is tested beside the unchanged wheat fields. All views are 1280×720 Unity camera renders with the overlay HUD absent. [Normal control](study57-control-normal.png). The normal camera matches study55; the close camera uses orthographic half-height 35 m and pitch 47°, centred on the same parcel. These are not Chrome frames or gameplay performance results.

The first construction selects 1,366 existing eligible roots from 7,702 original wheat positions, in larger 3.2×2.8 m curved planting cells. It broadens the mature model horizontally and varies height over a broad field. [Normal](study57-corn-normal.png), [close](study57-corn-close.png). The source colors and squat proportions produce sparse bright leaf marks, with too much exposed floor.

One follow-up preserves the source aspect ratio, darkens the copied leaf materials and forms continuous rows using 3,998 selected original roots. [Normal](study57-rows-normal.png), [close](study57-rows-close.png). It communicates planting direction more clearly, but still reads as repeated thin plants and bare floor. It also introduces far more triangles than the prior card clusters in this single parcel. No browser benefit, full-map viability, lifecycle adaptation or scientific crop-species claim is established. **Reject the crop swap; do not continue scaling/density/palette adjustments of these corn rows as the main solution.** Corn_3 was imported but not scene-tested; do not claim both models failed a rendered review.

The source plants, copied materials and evaluation prefabs were removed from Assets through AssetDatabase after the trial. Original FBX files remain archived outside Assets. The live annual diagnostic remained Year1/Planning/revision0/cash1,000,000,000 cents/queued0/assets0. Root selection does not move original horizontal positions, but enlarged leaf extents were not proved contained within every parcel/lane. No production crop construction changed.

## Retained technical integration

The actual runtime change retained in this turn is `5b8ad52`, a cherry-pick of `2f8543d`'s small vegetation submission consolidation. `a2339e1` then merges main `23e666d`'s completed browser receipts, preserving both the visual status and the newer technical evidence. All files under `Assets/Terra/Scripts` match `2f8543d` at this checkpoint. The [source-specific Chrome evidence](../../full-game/consolidation-browser/README.md) records p95 33/31/29 ms, three fresh entries 12.306/12.342/12.013 seconds, and 46.43 MB. These are one-machine bounded measurements on that exact export, not measurements of the transient corn or baked-surface trials, nor a final clean public release.

Own-worktree [compilation](study57-compile.json) reports zero errors/warnings. After all diagnostic art is removed, [15 collector checks](study57-consolidation-checks.json) pass. The restored normal scene has one collector, zero study57/58 objects, unchanged live annual state and 594 small submissions consolidated into 451 draws ([receipt](study57-restored.json)). Existing source batches ≥128 instances continue on their direct path, so these are not total scene draw counts. Main's integration evidence has the full 793→650 comparison.

The first CLI launch exited before connection without a recorded cause. A retained lifecycle launch succeeded; Editor PID11803 is the verified own-worktree instance. The failed startup is not described as a successful compile or run. No logs were sent externally. Study57 ends with Play stopped and the Editor open, ready for further visual work.
