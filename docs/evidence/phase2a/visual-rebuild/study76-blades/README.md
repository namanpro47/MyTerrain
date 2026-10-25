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
