# Crop forms 01 — import diagnosis and leafy-bed candidate

October 25, 2026. Base `431b8f8` combines the existing validation candidate with study60 shoreline source/evidence. This is an isolated **Editor render study**, not a production integration, Chrome result, saved gameplay action or Phase 2A acceptance. TN-02/09/10/12 guide the comparison: larger cultivation forms, readable leaf structure, grounded contact, and normal/close views of the same Wisconsin field.

## Finding and candidate

Study59's Beet_4 import is upright. Its FBX axis correction and child scale are preserved correctly. Transformed green-leaf vertices span 0.751 × 0.664 × 0.345 m; the source is intrinsically narrow in one horizontal direction. The shared `PlantMatrix` helper raises the full model's lowest bound to terrain, exposing the root. [Import measurements](import.json), [archived placement](baseline-close.png), [native-pivot depth and varied yaw](planted-close.png). The correction improves planting but does not yield fuller crop mass; reject this beet variant, without further density/tint tuning.

The creator's CC0 [Ultimate Crops](https://quaternius.com/packs/ultimatecrops.html) supplies a more suitable low, broad leaf model, Lettuce_4. [Exact source/license/hash](source.json), [license](LICENSE.txt), normalized to LF without changing wording. Native green is retained and adapted to URP Simple Lit. The model spans 1.044 × 0.474 × 1.044 m. This is illustrative crop art, not evidence of observed crop species, practices or yields.

The initial two-head rows overlap too much: [normal](lettuce-normal.png), [close](lettuce-close.png). The retained diagnostic uses one head every 2.5 m along each existing study59 bed, with 2.1 m horizontal scaling and 1.25–1.45 vertical scaling. It has **1,097 plants** versus the original 4,311. The 29 curved soil beds retain 37,584 vertices / 18,792 triangles. Soil remains the study59 experimental shader and existing artist earth map; neither is a newly accepted surface.

**Latest candidate:** [normal view](lettuce-lods-normal.png), [close view](lettuce-lods-close.png). Compare full-detail controls: [normal](lettuce-spaced-normal.png), [close](lettuce-spaced-close.png). Leaf heads and broad bed rhythm read more clearly than the exposed-root seedling variants. Full-scene surface/cohesion quality still needs the visual owner's review; these camera renders exclude overlay HUD and are not a gate pass.

## Bounded verification

- Confirmed Play-mode frames advanced, with server-side `Time.frameCount` change waits before capture.
- All 8,776 transformed full-source bounds corners are finite and inside the West field (`south-01`). This checks conservative bounding corners, not every terrain contact point or plant vertex.
- Year 1 / Planning / revision 0 / cash 1,000,000,000 stored cents / zero queued projects / zero assets throughout. The study changes rendering directly, never the annual authority or save.
- Baked meshes retain 1,392 / 390 / 84 triangles. The normal camera selects the 390-triangle level for all 1,097 plants: 427,830 color-pass candidate triangles, versus 1,527,024 at full detail. The close camera keeps full detail. These are geometry counts, not GPU timings, FPS or shadow-pass totals. [Normal audit](audit-normal.json), [close audit](audit-close.json), [bake](lods.json).
- The study overrides existing batch fields at runtime: 2.2 m projected extent, 20/8 pixel LOD thresholds. Production adoption needs explicit low-crop configuration; the existing tree-based thresholds/culling are unsuitable. No shared `InstancedNature` code was changed.
- [Final console](console-final.json): zero error entries, no compile failure, zero current Editor warnings. Historical package warning counts remain in the receipt; no claim of a warning-free entire session.
- Render target 1280 × 720, same archived study59 normal/close cameras; native materials and same terrain/soil layout. No Chrome, interaction, lifecycle, performance or public release claim.

## Replay and integration boundary

The `.cs.txt` scripts are executable Pipeline `script eval_file` inputs, archived outside compiled Assets. Copy them into ignored `Builds/CropDiagnostic/` and restore `Lettuce_4.fbx` there. In Edit mode run `import.cs` (reuses archived study59 Beet source), `shader.cs`, `import-lettuce.cs`, then `lods.cs`. These create disposable assets only under `Assets/Editor/CropDiagnostic`.

Enter Play in TerraMain and verify advancing frames and a ready `TerraMain.Diagnostic`. Run `normal.cs`, then the chosen setup (`baseline.cs`, `planted.cs`, `lettuce.cs`, `lettuce-spaced.cs`, or `lettuce-lods.cs`), then `capture.cs`. `close.cs` changes to the matched focused view. `audit.cs` applies to the LOD candidate. Captures go to `Builds/CropDiagnostic/latest.png`. Stop Play discards runtime objects. The archived baseline setup should be run in a fresh Play session; follow-up variants remove the previous diagnostic root.

Handoff is to the visual owner before any shared cultivation hook changes. Production adoption must preserve mapped cover and site boundaries, existing action surfaces, proposed/building/operating appearances, preview/undo and lifecycle teardown; use persistent resources and remove reflection. It also needs actual Chrome/HUD/movement and source-matched performance review. No production art is promoted by this checkpoint.
