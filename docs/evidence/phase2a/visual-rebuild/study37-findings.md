# Study37 — layered woodland groups and grass submission cost

Patterns TN-02/09/10/12. [Reference](../../../DESIGN_REFERENCE.md) and [visual goal](../../../PHASE_2A_GOAL.md). Candidate, not visual acceptance. Free existing TriForge oak/sapling and Quaternius grass; no new asset, purchase or generated image.

Study36's Chrome route dropped from59.94 to46.34FPS while the woodland still read as uniformly scattered crowns on isolated floor footprints. This study changes the woodland's spatial hierarchy and separately reduces grass batch submissions.

## Woodland composition

[Matched baseline](study37-matched-baseline.png) → [runtime cohort trial](study37-matched-cohorts.png). These are camera-only Editor captures, with ResetView applied and the own-session input controller temporarily disabled to hold the comparison. An earlier unfixed capture moved camera and is excluded from the comparison. [Camera lock/diagnostics](study37-camera-lock.cs.txt), [cohort replay](study37-cohorts.cs.txt).

Taller trees form group centers; younger trees occupy their edges; broader openings separate the groups. Tree roots remain inside source woodland and away from mapped routes, at source terrain height. Native geometry, leaf pixels, root origins and existing three-level LOD materials remain unchanged. These are illustrative groups and age/height structure, not surveyed forest stands or species.

The trial had306groups,2197mature trees and4827young trees. [Production opening](study37-production-opening.png) implements that construction with126m spatial render tiles, independently of group boundaries, rather than separate components for every group. [Runtime inventory](study37-render-check.json):69tiles,2272mature trees and4981young trees. The integration includes explicit inside-AOI checks and a different random draw order; it is the same recipe, not identical stems to the trial. Existing decorative grass/stone scatter changes because it no longer shares random draws with the old individual-tree loop. Source geography and the annual ledger do not change.

The floor mask now follows a group's broad footprint, reducing separate brown dots under each crown. The result has clearer height/group structure, but repeated source shapes, broad weak ground, noisy grass and mechanical crop rows remain. No acceptance score is raised. The native moss texture was inspected and not selected: its fine mottling does not supply the missing larger forms.

## Rendering hypothesis

`InstancedNature` now uses up to500 instances per soft-groundcover batch (previously128), within the documented511 default limit. It caches six frustum planes once per camera frame and skips groundcover batches whose expanded bounds are outside the view. The border includes plant height and local terrain relief for shadows under the existing fixed52° sun. Tall canopy and ShadowsOnly retain native culling. No density, mesh, material, shadow-casting mode or visible-distance threshold is reduced. Revisit the conservative shadow bounds if dynamic sun angles or very steep presentation terrain are introduced.

Unity6000.6 references read: [Graphics.RenderMeshInstanced](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Graphics.RenderMeshInstanced.html), [GeometryUtility.CalculateFrustumPlanes](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/GeometryUtility.CalculateFrustumPlanes.html), [GeometryUtility.TestPlanesAABB](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/GeometryUtility.TestPlanesAABB.html). Unity groups instances for culling; the explicit early rejection also avoids submitting irrelevant grass/shadow work.

Own-session opening diagnostic:1138submitted batches across all nature,402groundcover batches rejected before submission. This is a behavioral receipt, not GPU timing or proof of browser improvement. Study36 remains the browser performance baseline until the new export is measured.

## Verification so far

Source compiled while Play was stopped. Fresh production frames1→5 advanced. [209 existing lifecycle assertions](study37-annual-check.json), replaying study36's fixture, passed: source material colors/alpha, lifecycle idempotence, remaining cover, ledger/history and analytical terrain preserved. Current Editor console ground truth reported0errors/0warnings; its retained historical buffer still contains study36's old domain-reload errors. Temporary trials disappear on Play exit; own Play is stopped. Browser verification and public delivery remain pending.

## Renewed reference comparison during export

Official Free Lives [bank screenshot](https://freelives.net/uploads/2024/04/Terra-Nil-Screen-3.webp), [tropical canopy screenshot](https://freelives.net/uploads/2024/04/Terra-Nil-Screen-2.webp), and [WSGF GOG1.0.1 temperate play capture](https://www.wsgf.org/f/u/contrib/dr/30168/ingame_16x9.png) were re-inspected directly in Chrome at1280×720. Reference views show a smaller physical area and different geography; they are visual comparisons, not matching scientific scales. Strong terrain/water boundaries, contrasting plant silhouettes, controlled fine detail and clear contact remain the important transferable gaps. Tropical palms/coral and invented stepped cliffs are not prescriptions for Wisconsin. The cohort change addresses hierarchy only; it does not establish comparable whole-scene quality.

Before export, incoming main3ce7df9 was reviewed and merged: `HasColor` guards protect native texture properties in both ordinary and instanced annual tinting. The build source is53bfacb; prior209 fixture results precede that guard change, whose dedicated15-case and209-case evidence is in the incoming main checkpoint. Browser verification must identify this combined source.

## Chrome checkpoint

Export job `eb9883f5189c4a7e9e67d0fe21eec130` completed successfully in376892ms. [Nine locally served files](study37-served-assets.json) match the export. [Build identity](study37-build.json) records the pre-existing generated inputs; this is not a clean-worktree build claim. [Opening](study37-chrome-opening.png), [close](study37-chrome-close.png), [overview](study37-chrome-overview.png), [proposal](study37-chrome-proposed.png), [establishing33%](study37-chrome-establishing.png). The last two show different lifecycle appearances, but mature rendering is not checked here. Offline chapter fallback accepted Year1 and opened Year2; queue/undo preserved cash at both tested years.

[Browser log](study37-chrome-log.json): one30-second foreground1280×720 pan/placement route averaged42.22FPS, p95=37ms, p99=44ms, max72ms;131frames>33ms, zero>200ms,377camera events, one placement, zero unfocused frames. No error-level logs returned. The other observed wasm linker had exited before sampling. This is slower than study36 (46.34FPS,32ms p95); it also runs in Year2 with one established restoration rather than Year1, so it does not isolate the batch/cohort cost. The optimization hypothesis is not established, and this candidate is not ready for main. No density or shader-quality rollback is justified solely by this diagnostic.

The visual gate remains open: broad weak ground, repetitive fine crowns and mechanical crop rows remain. Next work must materially improve surface/bank construction and evaluate woodland rendering cost, not simply repeat tint/density edits. Checkpoint is recoverable on the visual branch, without public deployment.
