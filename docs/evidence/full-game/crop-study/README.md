# Continuous crop-row study — rejected, October 25, 2026

Source `6de45d7`, own full-implementation worktree and Unity6000.6.6f1. This temporary Editor experiment tested a materially different field construction while the visual owner evaluated meadow/terrain alternatives. No production runtime, source assets, scenes or annual rules changed. Main's older canopy remains in these views; they are not combined scenery acceptance.

## Hypothesis and result

Individual cutout clumps repeat conspicuously at normal play scale. Reconstruct continuous rounded row masses from their actual planted positions to replace the many disconnected silhouettes with more coherent forms. Preserve row curvature, gaps, field boundaries, existing source geography and annual state. TN-01/02/09/12 guide whole-field forms, material scale and consistency.

[Baseline](baseline-normal.png) → [continuous rows](row-normal.png) → [hybrid](hybrid-normal.png), all actual 1280×720 camera captures at the same view. Overlay HUD is excluded for this diagnostic.

**Reject both variants.** Continuous rows replace the clumps with mechanical corrugation and uniform rounded ridges. They are too abstract to replace the artist wheat at close scale; their normal view already fails. Lowering the row bodies by0.25m and restoring the original wheat brings back essentially the same high-frequency repetition. That extra geometry does not resolve the dominant failure. No further density/tint iteration or Web export is justified for this construction. Next field work should be judged within the visual owner's broader terrain-form composition, not repeat enlarged Quaternius wheat (earlier study13), dense clumps, or this smooth-row fill.

## Reproduction and boundaries

Open TerraMain in the source worktree, enter Play and verify ready/advancing frames. Run [camera.cs](camera.cs), then [row-volumes.cs](row-volumes.cs), then optionally [hybrid.cs](hybrid.cs) through Unity CLI script eval_file. Capture after each. The camera script disables the main gameplay Update for a fixed diagnostic view; instanced nature and the Editor player loop continue. Stop Play afterwards to discard all transient mesh/material objects. Do not save the modified live scene.

[Row receipt](row-volumes.json):265,147 source positions,260,541 connecting segments,48 mesh groups,1,325,735 vertices. These are geometry counts, **not performance measurements**; the combined hybrid adds these vertices to the original wheat. The first successful capture setup was frame11041, row trial19432, and [cleanup](cleanup.json)27873. Annual diagnostic remained Year1 Planning, revision0, $10M, zero queued proposals/assets. The temporary root was removed, original instancers re-enabled, Play stopped and own Editor closed gracefully. Generated TMP/cache metadata and session settings were restored.

No close/moving/Chrome test, visual score, frame-time improvement, new asset-license claim or phase completion is asserted. Failure at normal view is sufficient to discard this trial.
