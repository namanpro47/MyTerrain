# Terrain candidate integration and overlay construction

2026-10-25. Candidate `1bd75c2` combined with main `667f482` in isolated merge `6584e95`, branch `codex/relief-validation`. This is engineering evidence for an **unaccepted** visual candidate, not Phase 2A acceptance or a Chrome performance receipt. Unity 6000.6.6f1 / CLI 1.0.0-beta.14 on this Mac; own Editor in Play mode, advancing frames, with a concurrent visual-owner Editor.

## Display and analytical separation

- `model-checks.json`: 678 Atlas/source/save assertions and 34 annual lifecycle/JSON checks pass before the mesh-only optimization.
- `check.cs`, `display-before.json`, `display-after.json`: 349 checks pass on both versions, with 112,627 independent foundation samples. Renderer source heights match the core sampler exactly at 1,000 points. Full WorldData and annual state remain byte-identical.
- All 67 eligible project layouts retain source x/z/yaw and leave cached analytical layouts untouched. The maximum display/source foundation shift is 2.2713 m. The largest independently sampled displayed fill is 5.8163 m; this is illustrative terrain, not a change to the analytical 3 m suitability rule. The owner must retain the terrain disclosure and visually inspect tall pads.
- Actual rejected-placement overlay triangle centroids follow the display surface within 0.000056 m. Independent foundation sampling is a bounded check, not proof of a continuous global maximum.
- `project-edge-check.json` and `project-edge-after.json`: all 394 checks pass across ten building variants, including scale, mesh/material ownership, foundation/contact and valid bounds. Post-change Apply times are 28–73 ms in this Editor run.

## Surface optimization

The finer display grid made the existing polygon clipper repeatedly allocate and calculate height/normal for every interior triangle vertex. Surface now caches polygon containment at grid vertices, directly emits the same two triangles for cells fully inside the polygon, and shares their vertices. Cells touching the polygon edge retain the original clipper. Grid density, height sampler, normal calculation and boundary clipping are unchanged. Clockwise polygons safely retain the clipping path.

Actual Ghost construction across the eleven editable sites changed from 28–159 ms to 7–26 ms in these runs. The wind overlay changed from 158.61 to 25.89 ms and from 61,200 to 11,900 vertices, retaining 20,400 triangles. These are single Editor observations, with concurrent work, not browser percentiles or a final frame-time gate.

`equivalence.cs` embeds the old implementation from `6584e95` as the reference and compares generated triangles and normals for all thirteen sites plus rotated and reverse-winding polygons, on both fine and source grids. 334,594 checks cover 167,282 non-sliver triangles. Maximum vertex error is 0.000063668 m and normal-vector error 0.000030091. Matching uses cyclic order only, so reversed winding cannot pass.

The initial exact-count assertion (`equivalence-initial.json`) caught two floating-point clipping slivers in old south-02, totaling 0.0000114441 m². The comparison explicitly removes triangles smaller than 0.001 m² from **both** outputs and caps total removed area at 0.01 m² per shape; actual discarded areas are recorded in `equivalence.json`. The rotated examples remove the same roughly 0.00373 m² in both versions. This is tolerance equivalence, not byte-identical geometry or an exact triangle-count claim for every shape.

## Remaining acceptance

The active visual owner is evaluating cliff/ground materials. No scene is accepted here. Reference patterns and the six-dimension gate in `docs/DESIGN_REFERENCE.md` and `docs/PHASE_2A_GOAL.md` still govern the actual rendered result. Combined Chrome movement, transformed pads, picking, catalog Undo/Redo, three foreground frame paths, three fresh loads, frozen publication, public full run and rehearsals remain required. No public deployment was changed by this work.
