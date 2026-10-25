# PRD build-action art kit

Peter authorized the PRD action list for matching icons and metric 3D assets on October 24, 2026. Base: freshly fetched `origin/main`, `080f6926e0a8a0216c3c51a5c85cc722da134329`. This additive art package leaves the teammate's main scene, renderer, HUD and annual simulation untouched. It does not enable deferred gameplay recipes.

## Deliverables and use

`Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset` connects 17 stable art IDs to Unity prefabs, transparent 512-pixel Sprite icons, measured mesh bounds, site dimensions, triangle counts and assumptions. Drag the catalog into the future HUD/placement adapter's serialized field. Use `catalog.Find(actionId).icon` for uGUI `Image.sprite` and instantiate its `prefab` for the map. Do not infer public costs or outputs from visual size.

`Find` also resolves the existing scenario IDs: `cultivate-standard` → `cultivation`, `cultivate-margin` → `living-margin`, `restore-grass` → `ground-cover-restoration`, and `water-swale` → `vegetated-swale`. Unknown IDs return null; the caller should hide unavailable actions or show an explicit unavailable state. New PRD gameplay IDs should use an explicit adapter if they differ from this catalog.

Every icon is a camera render of its corresponding saved prefab. All icons share orientation, light and palette; they are individually framed for readability and **are not to scale relative to one another**. The isolated `Preview/BuildActionGallery.unity` keeps every prefab at unit scale for physical comparison. It is not added to build settings.

Portable glTF 2.0 binary models are under `artifacts/build-actions/models/`. Each contains the same geometry/material colors in metres, converted to glTF's right-handed coordinate system. After Unity generation, run `python scripts/export-build-action-art.py` (Pillow required) to rebuild the GLBs, contact sheet and `artifacts/build-actions/index.html`. The self-contained review page works locally and compares draggable/rotatable site footprints with the real map's cover/site polygons; it is a scale illustration, not the playable game's placement validator.

Rebuild with **Terra → Build Action Art → Generate and verify**, or run Unity 6000.6.6f1 with `-batchmode -projectPath <checkout> -executeMethod BuildActionAssetGenerator.Generate -quit -logFile <log>`. Use graphics (do not pass `-nographics`). The generator is scoped to this kit and its evidence directory; reruns preserve asset GUIDs. An interactive run asks Unity to save open scenes, then restores the previous scene setup.

## Metric contract

- One Unity unit is one metre, matching `docs/worldpack-format.md`. Local X east/right, Y up, Z north/front. All roots have identity rotation/scale and ground-contact pivots centered in X/Z. Front doors face +Z.
- `meshBoundsMetres` is measured geometry, including pads/landscape detail. `siteSizeMetres` is the authored occupied site, including access and landscaping. Use the latter for placement previews, with yaw applied to all four corners. Never fit a building by stretching it to a site polygon.
- Check the **whole rotated site polygon**, not just its center or corners: edges can cross a concave boundary, protected land, water or existing structures. Reject or reposition if it does not fit; do not silently shrink it. This asset catalog does not implement or certify placement eligibility.
- Sample terrain across the full foundation. Keep buildings upright and level; grading/foundation adaptation or rejection on slopes is an integration responsibility. Putting the pivot on the terrain alone does not guarantee correct ground contact across a large building.
- For wind, the rotor's swept radius is separate from the concrete pad and the illustrative site. The 65 m radius is geometry clearance, **not a safe/legal setback, turbine spacing rule or siting approval**. Its 165 m tip height should visibly dwarf a house.
- Farmland, farming variants, restoration, swales and stewardship are landscape samples, marked `landscapeSample`. Clip/repeat appropriate detail to actual site geometry; do not turn the sample tile into a building or assume it defines the full treatment area. Swales require the matching inset terrain cross-section; the standalone module displays the section above its ground pivot for inspection.

## Representative dimensions

All dimensions below are **authored visual assumptions** for this game, not surveyed structures or engineering designs. They are independent of the PRD's unverified costs/capacities. Pads/roof overhangs can extend beyond a building's nominal wall dimensions; the exported measured bounds are authoritative for rendered geometry.

- Traditional farmland, cover crop, conservation rotation, living margin: 40 × 40 m demonstration plots. Living margin reserves 10 m of the width (25%); the other practices do not inherit that area loss.
- Ground-cover restoration: 40 × 40 m meadow/shrub sample. It does not automatically turn every site into a forest.
- Vegetated swale: 30 m illustrative treatment length in a 32 × 10 m module; 2 m flat bottom, 0.6 m depth, 4:1 sides. No permanent open water or simulated flood effect.
- Low-density housing: one 12 × 9 m home, 3 m eaves / 5.8 m nominal ridge, on a 26 × 32 m lot.
- High-density housing: 32 × 18 m block; four 3 m storeys, approximately 13.2 m parapet; 44 × 38 m site.
- Public park: 48 × 40 m, including an 8 × 6 m shelter, 2 m paths, trees and benches.
- Agricultural storage: 30 × 18 m shed with 6 m eaves / 8.5 m nominal ridge; two 5 m diameter bins; 48 × 42 m site. This explicitly selects the PRD's agricultural-storage variant rather than a distribution warehouse.
- Data center: 54 × 32 m hall with 7 m roof plus cooling/utility equipment; 80 × 64 m site. No electrical or computing capacity assigned.
- Village center: three 16 × 12 m two-storey shops with a square and access; 64 × 52 m site.
- Non-renewable power: representative gas-engine facility, 32 × 20 × 9 m hall and 22 m stacks; 72 × 60 m site. No unspecified coal/nuclear imagery or output/emissions claim.
- Solar: sixty 4.4 × 4.2 m fixed-tilt tables, 25° tilt, 7 m row pitch; 64 × 48 m site. No capacity assigned without module specifications and electrical layout.
- Wind: 100 m hub, 130 m rotor diameter, 165 m maximum tip, 16 × 16 m concrete pad; 150 × 150 m illustrative site envelope. Modern utility-scale silhouette, not a tiny turbine resized to match housing.
- Conservation / Indigenous stewardship agreement: 48 × 40 m restoration sample with a modest interpretation sign. This illustrates specified landscape work, not an agreement's legal boundary. No invented Indigenous motifs, buildings, governance outcomes, ecological guarantee or assumed public access.
- Wastewater upgrade: two 12 m diameter tanks, a 20 × 10 m process building, two 8 × 16 m basins; 64 × 52 m site. Conceptual treatment components, not a process-engineered plant or a drinking-water facility.

## Evidence for scale choices

The [US Department of Energy's 2024 land-based wind report](https://www.energy.gov/cmei/systems/land-based-wind-market-report-2024-edition) reports 103.4 m average hub height and 133.8 m rotor diameter for 2023 installations. The kit rounds these to an illustrative 100 / 130 m turbine; it does not claim to reproduce a particular model.

The [Minnesota Pollution Control Agency swale guide](https://stormwater.pca.state.mn.us/ms4_fact_sheet_vegetated_swales_buffer_strips) recommends 4:1 side slopes and describes sizing in relation to flow/storage. The kit uses the slope as a geometric example, not a hydrologic sizing result. Other building dimensions are explicitly authored representative examples; they have no survey or compliance status.

## Art, provenance and acceptance

Original geometry and palette authored in this repository; no downloaded or AI-generated raster artwork, paid assets, cultural reference art or additional runtime packages. Standard Unity primitives are combined into persistent meshes by material. URP Lit opaque materials are shared; icons use temporary Standard-material copies with matching colors in an isolated studio. Thus icon appearance is a studio rendering, not proof of the final URP/Web lighting.

Reference patterns: **TN-04** (tool image matches placed object), **TN-05 / CS-02** (truthful occupied footprint), **TN-01** (terrain scale remains meaningful). The warm plaster, slate/terracotta roofs, sage planting and restrained industrial metal form one small asset family. No claim of Terra Nil parity or Phase 2A acceptance.

The generator checks catalog completeness, finite meshes, triangle budget, unit roots, ground/site bounds, serialized references and nonempty/unclipped transparent icon renders. Receipts and contact sheet live under `docs/evidence/build-action-art/`. The asset kit is separate from the playable map; HUD binding, terrain grading, site eligibility, browser performance and final in-game appearance require the owner's integration pass.

Observed verification: 230 Unity generation/render checks plus 41 saved-catalog checks; all 17 GLBs passed Khronos glTF Validator 2.0.0-dev.3.10 with zero errors and warnings. Chrome rendered the full catalog; selecting wind changed the size/details/download target, setting 45° rotated the footprint, and dragging moved it on the map. `chrome-scale-preview.png` records that actual local interaction. This checks the art-review page only, not the game's browser build. `Verify saved catalog` separately checks persisted references, existing scenario aliases and the 165 m wind tip height.


Runtime ground adaptation now uses the existing licensed Julio paint family on instance grass/path materials. The verified edge adaptation grades only the outer three metres of grass/path/asphalt inside the original lot and preserves all building dimensions, source prefab assets, analytical terrain and footprint rules. These generated meshes belong to the renderer and must be released with their proposal; construction retains the level foundation. Wind keeps its separate bounded pad. This is an integration treatment, not an update to the metric source art or an accepted whole-scene art direction. See [verification status](evidence/full-game/README.md#project-edge-grading--implementation-candidate).
