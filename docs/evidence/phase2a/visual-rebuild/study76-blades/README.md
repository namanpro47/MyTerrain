# Study76 — source-derived blade field on current main

Prepared implementation; import/build and actual visual comparison pending. Fresh isolated `codex/visual-field76`, base `19e7eae835ed4c06a21d42887a9a53ed28cc4fc2`, fetched October25. No Library copied/shared. This keeps the combined native selector, inherited settlements, dense parcels, current HUD and annual loop intact.

The preceding [study75 evidence](https://github.com/namanpro47/Terra/tree/22e3741/docs/evidence/phase2a/visual-rebuild/study75-distance) rejects detailed wheat plus a distant surface: fine noise, representation mismatch and11.52M selected plant triangles in one normal view. Its color fade also did not match the shadow pass. Do not repeat that implementation or infer that a cleaner compile means a finished field.

## Substantially different construction

Use the acquired free GDQuest grass demonstration's seven-triangle blade and root-to-tip gradient. Preserve the credited source mesh/shader/material text and license in `Assets/Terra/Art/FieldBladeStudy/GDQuest`. [Source revision and hashes](source.json). Code is MIT; art is CC BY4.0 with GDQuest/contributor attribution. No generated image or paid resource.

The isolated builder normalizes the source to1.2m, preserves its silhouette through7/3/1 triangle meshes, consolidates the material and derives height UVs. The URP shader adapts world-driven wind and the original gradient; it omits character interaction. Its seamless numerical Perlin wind map replaces Godot's OpenSimplex resource, so this is not an exact native-engine reproduction. The native Godot demo is source-inspected, not executed. There is no shadow-casting pass; optional received terrain shadow is explicit. No hidden full shadow survives a color fade because this version does not use the earlier complementary surface fade.

The prototype is outside Resources, with no production scene or rendering activation. First compare original-gradient control and a documented terrain-coherent material adaptation, then judge whole-field normal/close/wide and camera movement. Author vegetation as illustrative cultivated cover; do not claim observed crop species. Preserve source land-cover eligibility, terrain heights, route exclusions and annual state. Inspect actual current-world parcel IDs instead of assuming the older south-01 test parcel exists.

## Gate and next verification

[TN-02/09/10/12 design patterns](../../../../DESIGN_REFERENCE.md), [art contract](../../../../art-direction.md), [Phase2A goal](../../../../PHASE_2A_GOAL.md) remain binding. Cheap geometry is not art acceptance. Record exact submitted geometry, actual source/annual invariants and matched captures; reject aliasing, floating roots, isolated marks, artificial bright carpets or a strong near/far transition. Whole-scene and Chrome performance/interaction/release gates remain open.

Run `Study76Builder.Build` in the owned Editor or isolated batch process after reserving editor + unity-heavy for cold import. Then stop/release before acquiring the short editor + desktop capture slot. No Unity job has started at this checkpoint. Shared phase status is owned by another task; evidence remains here until a coordinated status update.

## Prepared candidate and reproducible comparison

Candidate `124393c` is pushed to `codex/visual-field76`. All four retained source-file hashes match `source.json`; the OBJ has nine positions and seven triangles. `git diff --check` passes. **C# compilation, shader compilation, rendering, timing and visual quality remain unverified.** Latest fetched main `ce27355f6bd7b1c55e1c53c2db00b1b748ccf926` adds release evidence/documentation after the code base `19e7eae`; no new runtime change was omitted in that comparison.

`Study76Scene` is an Editor-only reversible study harness. Its default target is the current `grid-07-05` Wisconsin parcel (180 m square), not the removed historical `south-01`. Placement uses the active renderer's crop eligibility, route exclusion and actual terrain height. Only the parcel's baseline presentation is hidden while the study exists. Exact serialized source-world and complete annual snapshot comparisons guard every variant, capture and removal.

After import and `Study76Builder.Build` succeed:

1. Start the owned scene in Play mode, use `WorldAtlas.Instance.TravelTo(0)`, wait for actual arrival and prove frames advance. `Study76Scene.Inspect()` records actual parcel and annual context.
2. Capture the baseline at `Study76Scene.View(105)`, `View(60)`, `View(32)` and `View(180)`, waiting for frames between view setup and `Capture("control-normal")` / the corresponding named captures. The view center is (-30, -210), within the current parcel. These camera renders exclude screen-space HUD and are explicitly diagnostic.
3. Run `Study76Scene.Apply()` once. Compare all four views with the original source gradient; run `Palette(false)` for the documented olive/yellow-green adaptation and repeat. Material changes reach the actual leased renderer material, not just its template. Source geometry stays fixed.
4. `Wind(true)` enables actual time-driven movement for a bounded live inspection. `Wind(false)` returns to the fixed-time control. Wind padding is included in native instancing bounds.
5. Reject visible thin speckling, a luminous flat carpet, harsh field edges or representation popping. A field improvement must hold with the forest, banks and water visible; it cannot close the whole-scene gate by itself.
6. Run `Remove()` and `RestoreView()`, stop Play, check logs and clean scene, close the owned Editor, then release resources. No production scene, resource activation or annual save is written by this harness.

Capture files and frame/hash/material receipts go to ignored `Builds/ArtResearch/Study76`. Copy only inspected results into this evidence folder, with a findings document distinguishing actual results from the prepared procedure. Source-mesh LOD counts are 7/3/1; geometry upper bounds are not GPU timings.

Coordination checkpoint: waiting import reservation 128 was released without launching Unity, accommodating the queued bounded cached HUD check. Replacement reservation **132** requests only this worktree's editor + unity-heavy. Desktop capture is a separate subsequent reservation. No shared resource is held by this study at this checkpoint.
