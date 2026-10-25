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
