# Oak renderer audit — October 24, 2026

Peter requested tree model vertex counts, LOD verification and investigation of the 5 FPS browser sample. This audit uses main `ba88fdf` in a fresh worktree. The existing local game and its saved plan are untouched.

## Imported models and actual forest

All four `SolidOak` variants share the same imported FBX meshes:

- LOD0: **2,833 vertices / 4,908 triangles**.
- LOD1: **1,524 vertices / 2,454 triangles**.
- LOD2: **842 vertices / 1,226 triangles**.

Each mesh has one submesh. The prefabs have no Unity `LODGroup`; the custom `InstancedNature` renderer explicitly binds all three meshes by name and switches between them. It does not draw all three LOD meshes simultaneously. The source FBX's control-point counts (2,458 / 1,231 / 617) are lower than Unity's imported vertex buffers because import can split vertices at normal/UV boundaries.

The baseline forest has **2,255 oak-mesh instances**, including smaller oaks used as shrubs, in **264 batches**. The renderer also creates **264 material instances**; these are clones of the variant materials rather than 264 distinct tree textures. Instancing shares meshes in memory but does not remove the GPU's work to draw each instance.

## Culling and LOD census

At the authored opening camera, with the measured 1707 × 876 CSS viewport:

- 58 forest batch bounds intersect the camera frustum.
- Those batches contain **504 instances / 2,103,032 selected-LOD triangles**.
- **296 of those instances are individually outside the camera frustum**. The batch average diagonal is about 153 m. Unity culls these instanced submissions as groups, so an intersecting batch can retain substantial offscreen geometry.
- Applying the same LOD thresholds individually, without changing instance count or assets, estimates **1,979,034 triangles**, a **5.9%** reduction for this view.

At the wide camera, batch LOD selection estimates **4,420,072 triangles** versus **3,506,440** using per-tree selection, a **20.7%** reduction. At maximum zoom out, most trees already use LOD2, and the difference is only 1.1%. These results do not support a claim that LOD is universally absent or stuck at LOD0.

These are reproducible CPU-side candidate submissions, not a GPU frame capture. They exclude additional real-time shadow passes and do not measure occlusion or exact visible pixels. Offscreen trees can still cast useful shadows; an optimization must preserve shadow casters while reducing unnecessary color-pass submissions.

[Full census](census.json) · [source hashes](source-hashes.json). Relevant Unity behavior: [RenderMeshInstanced group bounds/culling](https://docs.unity3d.com/ScriptReference/Graphics.RenderMeshInstanced.html).

## Reproduction and isolated browser experiment

Run Unity batch mode with `-executeMethod TreeRenderingAudit.RunBatch` to produce `Logs/TreeAudit/tree-rendering-audit.json`. The audit loads the real scene/assets, builds a temporary baseline landscape, reads the custom batches and evaluates four camera views. It exits without saving the temporary scene or modifying player progress.

`-executeMethod TreeRenderingAudit.BuildExperiment` also produces `Builds/TreeAudit`. It discards the census scene before packaging the real scene. Only this build receives the `TERRA_TREE_AUDIT` scripting define and an HTML test panel. Normal builds exclude `TreeAuditControl` and contain no tree-test panel.

Serve only `Builds/TreeAudit` on a separate localhost origin. Close the initial arrival panel, then compare **Trees on**, **Trees off** and **Shadows only** with the camera stationary. Each button starts a 2-second warmup and 12-second sample, logged as `TERRA_TREE_ABLATION`. The samples record camera position/rotation, buffer dimensions, annual revision and focus state so mismatched runs can be rejected. The controls change only the forest renderer; the terrain, fields, grass, lighting, render quality and game state remain the same.

## Browser results and next target

After Peter woke the laptop, the comparison resumed on the **same diagnostic export**, in foreground Chrome, at the fixed opening camera and empty year-one plan. All seven samples match **1287 × 1304 drawing buffer**, DPR 1.5, camera transform/FOV and annual revision; all record zero unfocused frames. Each used 2 seconds warmup and about 12 seconds sampling. The sequence brackets the comparison with normal forest: normal → off → shadows → off → shadows → normal → normal.

- **Trees on, three trials: 4.59–5.70 FPS**; median frame times 163 / 239 / 172 ms; p95 263 / 253 / 252 ms.
- **Trees off, two trials: 12.05–12.06 FPS**; median 82 / 80 ms; p95 95 / 101 ms.
- **Tree shadows only, two trials: 6.99–8.12 FPS**; median 143 / 124 ms; p95 174 / 139 ms.

[Exact resumed samples and conditions](browser-ablation-resumed.json) · [export hashes](diagnostic-build-hashes.json). The complete measured ranges are retained: normal and shadow-only timing varied, while forest-off was stable. The second normal trial overlaps GPU counter collection; the final normal trial was repeated after counters stopped. Native Chrome output was visibly inspected in all three modes: trees-off removed oak canopies and shadows, while shadows-only retained ground silhouettes without canopies. No resize, camera movement or annual action was performed during the seven samples.

The repeated forest-off trials run at roughly **2.1–2.6 times** the frame throughput of the normal trials. Shadow-only remains materially slower than forest-off, and full forest remains slower than shadow-only. Both the visible forest and its shadows matter; this is not evidence that all cost is in the shadow pass. The renderer submits its selected meshes as real-time shadow casters; the Web URP asset uses a 4096 main-light shadow map, 1600 m shadow distance and two cascades. A bounded next rendering experiment should vary tree shadow distance/LOD and shadow-map resolution while preserving the foreground canopy and useful shadows, then compare repeated matched trials and inspect the appearance. Tighter batch bounds and shared materials remain secondary candidates.

**Current GPU routing:** read-only Windows GPU-engine counters were matched to Chrome's current GPU process (PID 13440) and DXGI adapter LUIDs. Intel UHD 3D activity averaged **58.93%** over the 35-second capture, with **58.18%** over 11 counter samples overlapping the second normal trial. No NVIDIA engine rows were exposed for this Chrome process in the capture. This supports active Intel rendering/compositing in the browser; it does not isolate WebGL commands, prove Intel saturation, or measure zero total RTX utilization. [Counter samples and adapter mapping](gpu-resumed.json). Establish and verify an RTX-backed browser baseline before treating these numbers as a benchmark of the laptop's discrete GPU. No GPU preference change or browser restart was made.

The earlier pre-sleep single trials (8.23 / 14.73 / 8.77 FPS) remain in [historical results](browser-ablation.json); they are not pooled with the resumed trials. Earlier exact 1 FPS hidden-tab and startup-contaminated trials remain excluded. The current buffer is smaller than the original 5.2 FPS report, so this is not an improvement over that run. Removing canopy changes occlusion/ground shading; differences measure net scene cost, not GPU pass self-time. Even forest-off remains far short of smooth play. These local debugging-extension measurements complete the bounded tree comparison, not release performance certification or a complete CPU/GPU profile.

## Verification and delivery boundary

- Actual Unity imported-mesh census passed, including checks that all three custom LOD bindings exist.
- Separate diagnostic Web player built successfully in Unity 6000.6.6f1; `TREE_AUDIT_WEB_BUILD_PASSED` in the local build log.
- Experimental controls were exercised through the visible browser UI. They exist only behind the build-specific define; no production renderer, scene, asset or quality setting is changed by this audit.
- Teammate main `589b655` was inspected after the census base: it changes chapter/report and build settings, with no changes to the audited oak model, forest renderer or URP assets. Browser timings remain measurements of the stated `ba88fdf` diagnostic build, not a benchmark of the later release build.
- The resumed evidence was recorded in a fresh worktree from refreshed main `1fb5ec3`. Incoming changes after the first audit modify HUD/loading/documentation, not the audited forest renderer, meshes or URP assets. The existing export was deliberately reused for a consistent comparison; its hashes identify the measured build.

## Visual boundary

TN-01 / TN-02: preserve the readable woodland composition and silhouettes. Hiding the forest is a diagnostic condition, not a proposed art change or visual acceptance. Priorities after measurement are tighter culling/LOD organization, material sharing and shadow cost; choosing a lower-poly replacement is justified only if it preserves the intended appearance and improves measured frame times.
