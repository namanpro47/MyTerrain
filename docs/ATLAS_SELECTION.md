# Living Atlas — October 25, 2026

The user explicitly authorized a draggable globe, cinematic entry and three supported rural locations. This supersedes earlier one-location/no-selector limits for this bounded feature. The separate Phase 2A landscape-quality gate remains open.

## Native Unity correction

The user rejected the HTML landing page and explicitly required the draggable globe and descent **inside Unity**. The product is now **MyTerrain** wherever players see its name. Earlier HTML-atlas captures remain historical, not the accepted implementation.

`WorldAtlas` owns a real textured 3D sphere, separate URP camera, atmospheric shell, geographic pins, uGUI buttons and TMP labels. Drag rotates with damped inertia; scroll and +/− zoom; 1–3 or Tab selects a region; Enter descends. Selection follows the shortest longitude arc. Field notes identify the imagery and distinguish observed geography from educational assumptions.

Travel turns toward the selected coordinate, approaches the surface and fades through an atmospheric veil. It then loads the actual selected landscape and reveals its camera descending to the playable view. The load is never treated as complete before terrain and annual state are ready. Back/Escape cancels; failed loads expose retry. Reduced motion bypasses the sustained flight and is remembered.

The native World atlas button or Shift+Escape returns to the globe. Same-place entry preserves the live scene; different-place entry reloads the gameplay scene behind the persistent atlas and restores that place's isolated save. Existing committed-year rules and save keys remain intact. Internal Terra names are retained for compatibility; they are not player branding.

Patterns: **TN-01/02/03/06**, **AX-01**. A cinematic orbital scene, stylized sourced Earth, gold travel arcs and numbered destinations provide a game level-selection view. Native isometric biome miniatures support the three location choices; cream typography and gold travel action emphasize the selected landscape. See the [comparison study and new render evidence](evidence/myterrain-combined-release-oct25/README.md). This bounded atlas correction does not certify the separate Phase 2A landscape gate.

## Fixed catalog and evidence

| Place | Stable pack / centre | Observed versus authored |
|---|---|---|
| Meadowbrook Valley, Wisconsin | `meadowbrook-v1`; 43.7015, −91.037 | Existing USGS 3DEP / NLCD / mapped lines and existing scenario. Original `coon-valley-brye-v1` provenance and save keys retained. |
| Point Reyes, California | `point-reyes-v1`; 38.066, −122.810 | Real elevation near Point Reyes Station. Cover mosaic and ten opportunities are authored, not a national-park development plan or parcel survey. |
| Suusamyr Valley, Kyrgyzstan | `suusamyr-v1`; 42.18, 73.96 | Real high-valley elevation. Cover and ten opportunities are authored. Shared USD budgets are educational units, not Kyrgyz public finances. |

Each run stays in its selected place for ten years. The two new regional packs use the existing annual engine, project catalog and deterministic scenario rules. Every action variant has an eligible site. They do not introduce a second ledger, local economic calibration, observed drainage, a city simulator, geocoder or live world generation.

**October 25 numerical-reference decision:** Peter chose one shared Coon Valley / Vernon County, Wisconsin weather/agricultural/economic reference for the next simulation slice, regardless of fictional display name or selected terrain. Keep the three geography packs and isolated saves. Do not present Wisconsin weather/cost/yield benchmarks as Point Reyes or Kyrgyz measurements. The source/details UI must distinguish sourced terrain from the common numerical reference and authored parameters: **"Fictional scenario · weather and economic references: Coon Valley, Wisconsin · other parameters are authored game assumptions."** This is a new disclosure/requirements decision; the current atlas and runtime are not claimed updated by this document change.

New terrain comes from **Mapzen / AWS Terrain Tiles**, USGS 3DEP and global SRTM/GMTED2010. Terrarium zoom-14 elevation is sampled bilinearly to a 10 m display grid over 1.44 km²; effective source resolution may be coarser. No vertical exaggeration is applied. Cover, opportunities, soil B and runoff groups are explicit assumptions. Each site's hypothetical runoff group is isolated; no invented roads, streams or external inflow are added. Tile URLs and hashes, vertical limits and source gaps are retained per pack in `provenance.json`.

- [Elevation source and service](https://registry.opendata.aws/terrain-tiles/)
- [Required terrain attribution](https://github.com/tilezen/joerd/blob/master/docs/attribution.md)
- [Suusamyr pasture context, UNDP](https://files.acquia.undp.org/public/migration/kg/Brochure-pastures-ENG.pdf)
- [NASA globe source](https://science.nasa.gov/earth/earth-observatory/blue-marble-next-generation/base-topography-bathymetry/)

Earth is a July 2004 composite with a native shader colour treatment, not live imagery. Credits are retained in `Assets/Terra/Resources/Atlas/CREDITS.md` and the native Field notes panel. The old HTML atlas, Three.js, illustration cards and duplicate browser selection state have been removed.

## Integration and reproduction

- Native runtime: `Assets/Terra/Scripts/Gameplay/WorldAtlas{,.Visuals}.cs` and `TerraMain.Atlas.cs`.
- Native assets: `Assets/Terra/Resources/Atlas/`.
- Browser: `Assets/WebGLTemplates/Terra/index.html` and `player.js` only boot the canvas and transport input. No browser location selection or camera animation.
- Annual/chapter keys remain isolated by pack. Wisconsin retains current connected-field v3.2 state; other places keep v3.1. Cross-location restore is rejected.
- Editor Play and the WebGL build use the same globe and entry lifecycle. No EditorPrefs location bypass is required.

```sh
python3 scripts/prepare-world.py --pack all
python3 scripts/verify-atlas.py
```

Use the shared resource coordinator before Editor/build/Chrome work (see `docs/WORK_COORDINATION.md`). Invoke `TerraBuild.BuildWeb()` in the owned project to package all three places and export the complete Unity player. Serve only that worktree's `Builds/Web` on an unused port.

Verification and actual rendered captures: [native atlas evidence](evidence/native-atlas/README.md). Historical web-atlas evidence: [previous implementation](evidence/atlas/README.md).
