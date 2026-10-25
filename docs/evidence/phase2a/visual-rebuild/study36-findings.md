# Study36 — broad grass stands across mapped grassland

Patterns TN-02/09/10/12. [Reference](../../../DESIGN_REFERENCE.md), [visual goal](../../../PHASE_2A_GOAL.md). Candidate implementation; Chrome diagnostic completed below, visual and full performance gates unaccepted.

## Visible change

The broad flat grass surface dominates the river/forest junction in study35. This study reuses the existing Quaternius artist-authored grass family and the connected meadow placement introduced in study31, while preserving mapped cover, terrain and annual accounting. No new assets, generated images or purchases.

[First representative surface](study36-connected-meadow.png) remains scattered. The short tuft's native bounds are 0.639m wide ×1.334m high, with 155 triangles; at the existing 2.2m grid, retained clumps leave wide gaps. [Broader stands](study36-broader-meadow-valid.png) use ×3 additional breadth and ×1.4 height, forming overlapping patches instead of speckles. These are illustrative vegetation clumps, not surveyed stems or species. [Source trial](study36-meadow.cs.txt), [breadth adaptation with recalculated bounds](study36-broader.cs.txt).

The grass shader previously used `_BaseColor` only inside the luminance calculation, so material hue and annual status tint barely affected the blade color. It now multiplies the authored blade gradient by that RGB tint while retaining alpha coverage. [Visible surface tint correction](study36-surface-tint.png). This also means annual project grass colors need a fresh browser comparison; earlier main captures do not validate this shader change.

## Runtime candidate

[Full-location opening, Editor camera only](study36-full-opening.png). A new BuildGrassland pass places the broader stands in 60m spatial patches, constrained to source grass cover and excluding all editable sites. Existing project-specific vegetation remains independently replaceable. It uses 49,335 plant instances in 420 spatial patches, representing 9,565,326 triangles before frustum/subpixel culling. This cost requires actual browser measurement. Existing source meshes/textures are reused; no extra asset download is implied.

PlantMeadow keeps its original five-argument interface and original project dimensions, delegating to PlantMeadowStands with unit breadth/height. The new baseline calls the helper with breadth3/height1.4. Ground contact is recalculated using the final height scale. The previously unreachable woodland groundcover condition is corrected: woodland openings no longer need a grouping value both <0.4 and >0.44. This consumes additional decorative random draws, so later illustrative tree positions can change; source geography and accounting do not.

The new meadow establishes larger forms but the whole-scene reference gap remains. Its close blade shape, broad patch boundaries, normal/overview behavior, consistency with project sites and browser cost remain to be reviewed. No score is raised to acceptance.

## Verification and invalid capture

An initial live trial lost its nonserialized runtime state after Unity recompiled pending script changes from the recent main merge. [study36-broader-meadow.png](study36-broader-meadow.png) is **invalid**: the adaptation returned zero instances, the vegetation batches were empty and annual controls threw initialization errors. It is retained only to explain the failure. The Editor log confirms an assembly reload after a capture triggered asset refresh. No built browser export was affected.

Own Play was stopped, AssetDatabase synchronously refreshed while stopped, and the trial restarted after compilation was idle. Frames447→458 advanced; replay instantiated and adapted all8,488 representative plants. Production source was then changed only after stopping Play and explicitly recompiled; recompile completed without errors. Fresh production Play frames2141→2150 advanced and initialized Year1 / Planning / revision0 / cash1000000000 / queued0 / assets0.

[209 existing lifecycle assertions](study36-annual-check.json) passed across8 vegetated agreements and9 remaining-cover building variants, preserving original source material colors, alpha, idempotence, the annual ledger and analytical terrain. The [replay](study36-annual-check.cs.txt) only changes the original fixture's material filter from HasProperty to HasColor because the native bark shader also has a texture named `_BaseColor`. These are state/material checks, not pixel-color acceptance. The updated grass shader reports no shader messages. Current Editor console had zero errors/warnings after the fresh full candidate and checks. Own Play is stopped and its background override restored.

The browser must verify this full source, at normal/close/overview and proposed/established states, before any claim of improvement or performance acceptance. Study34b remains the last verified browser export until that succeeds.

## Actual Chrome checkpoint

Source `a54d2fe` exported successfully in269,276ms. [Exact served bytes](study36-served-assets.json), [build inputs and capture hashes](study36-build.json), [browser logs](study36-browser-log.json). Nine local export files matched their served bytes. Chrome CSS/canvas1280×720, visible, zero unfocused frames.

[Opening](study36-chrome-opening.png), [close forest](study36-chrome-close.png), [overview](study36-chrome-overview.png) and [proposed restoration](study36-chrome-queued.png). Larger baseline patches reduce the flat empty surface in the opening, but the fine grass reads as noise, patch borders remain conspicuous and the overview still flattens to a schematic surface. Repeated crowns and exposed forest floor remain. No dimension reaches the reference bar.

One30.017s foreground route:46.34FPS average, p50=20ms, p95=32ms, p99=40ms, max52ms;1391frames,57over33ms, zero over200ms, zero unfocused. Five previews, one placement,392camera events; max previewCPU6.1ms and placementCPU19.6ms. This is materially slower than the prior34b route (59.94FPS), although the two interactive routes are diagnostics rather than a controlled GPU attribution benchmark. Own Unity Play/build were stopped; another project Editor could be active. No error-level browser logs. Full three-path performance acceptance is not claimed.

Queue/undo changed Year1Planning revision5/queued1 to revision6/queued0; cash stayed1000000000cents and accepted assets0. Proposed grass is visible in the capture; established/mature pixel comparisons remain pending. No year submitted. Browser viewport restored and verification tab closed.

Retain this as a recoverable working-branch checkpoint, not a main integration or art acceptance. Next address the broad grass layer's rendering cost and stronger coherent ground/forest forms; avoid further density/tint changes without a clear construction hypothesis.
