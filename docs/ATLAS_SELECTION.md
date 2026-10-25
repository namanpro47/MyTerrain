# Living Atlas — October 25, 2026

The user explicitly authorized a draggable globe, cinematic entry and three supported rural locations. This supersedes earlier one-location/no-selector limits for this bounded feature. The separate Phase 2A landscape-quality gate remains open.

## Experience

The browser starts in a warm, quiet atlas: NASA Earth imagery, a softly lit globe, fine geographic graticule, readable pins, original landscape illustrations and three fixed destination cards. Drag Earth with mouse or touch, use the arrow keys while the globe is focused, or select a card/pin. Selection rotates along the shortest longitude arc. Scroll or +/− zoom; the centre control returns to the selected place. The cards also support left/right arrows and normal Tab/Enter navigation.

Explore moves the globe into the centre of the screen, turns toward the selected coordinate and descends through the surface, then reveals the loaded Unity landscape. Loading waits for the actual landscape-ready signal, not just the WASM download. Back/Escape during loading returns to the atlas; errors expose a retry. Motion follows the system preference and has a persistent explicit toggle. With reduced motion the camera flight is omitted. The game’s World atlas control returns without resetting the run; returning to the same landscape reuses the player, while another location starts a fresh runtime with its own save.

Patterns: **TN-01** landscape as the emotional focus, **TN-02** clear masses and restrained palette, **TN-03** sparse controls, **TN-06** short entry guidance, **AX-01** keyboard, contrast, viewport and motion checks. The atlas is a new browser entry surface, not a claim that the underlying Phase 2A scenery has been accepted. The existing rejected landscape captures remain historical comparison material.

## Fixed catalog and evidence

| Place | Stable pack / centre | Observed versus authored |
|---|---|---|
| Meadowbrook Valley, Wisconsin | `meadowbrook-v1`; 43.7015, −91.037 | Existing USGS 3DEP / NLCD / mapped lines and existing scenario. Original `coon-valley-brye-v1` provenance and save keys retained. |
| Point Reyes, California | `point-reyes-v1`; 38.066, −122.810 | Real elevation near Point Reyes Station. Cover mosaic and ten opportunities are authored, not a national-park development plan or parcel survey. |
| Suusamyr Valley, Kyrgyzstan | `suusamyr-v1`; 42.18, 73.96 | Real high-valley elevation. Cover and ten opportunities are authored. Shared USD budgets are educational units, not Kyrgyz public finances. |

Each run stays in its selected place for ten years. The two new regional packs use the existing annual engine, project catalog and deterministic scenario rules. Every action variant has an eligible site. They do not introduce a second ledger, local economic calibration, observed drainage, a city simulator, geocoder or live world generation.

New terrain comes from **Mapzen / AWS Terrain Tiles**, USGS 3DEP and global SRTM/GMTED2010. Terrarium zoom-14 elevation is sampled bilinearly to a 10 m display grid over 1.44 km²; effective source resolution may be coarser. No vertical exaggeration is applied. Cover, opportunities, soil B and runoff groups are explicit assumptions. Each site's hypothetical runoff group is isolated; no invented roads, streams or external inflow are added. Tile URLs and hashes, vertical limits and source gaps are retained per pack in `provenance.json`.

- [Elevation source and service](https://registry.opendata.aws/terrain-tiles/)
- [Required terrain attribution](https://github.com/tilezen/joerd/blob/master/docs/attribution.md)
- [Suusamyr pasture context, UNDP](https://files.acquia.undp.org/public/migration/kg/Brochure-pastures-ENG.pdf)
- [NASA globe source](https://science.nasa.gov/earth/earth-observatory/blue-marble-next-generation/base-topography-bathymetry/)

The three landscape cards are original SVG illustrations, not photos or surveyed features. Earth is a July 2004 composite with an atlas colour treatment, not live imagery. See `Assets/WebGLTemplates/Terra/atlas/CREDITS.md`. All resources are bundled; no runtime CDN, paid asset, key or image generation is used.

## Integration and reproduction

- Browser entry: `Assets/WebGLTemplates/Terra/index.html`, `atlas/{atlas,globe,locations,player}.js` and `atlas.css`.
- Location contract: `LocationProfile` in Core; bounded ID/index adapter in `TerraMain.Atlas.cs` and `TerraBrowser.jslib`.
- `TerraMain` reads the location before loading. Only Wisconsin applies its historical wind-site adapter. Regional packs contain their own complete opportunities.
- Annual and chapter keys are separate for each region; Wisconsin’s exact existing keys and definition ID remain unchanged. Cross-location save restore is rejected.
- Browser input capture is released while the atlas is visible. Camera/audio resume when returning. Outstanding accepted-year work may finish and save; atlas navigation never rewrites a committed year.
- In-game identity, loading, ending and field-guide notes follow the location. The main gameplay HUD remains uGUI/TMP.

```sh
python3 worldgen/build-regions.py         # downloads only missing verified-HTTPS elevation tiles
python3 scripts/prepare-world.py --pack all
python3 scripts/verify-atlas.py
unity command script eval --caller plugin --skill unity-cli --project-path "$PWD" \
  'return AtlasLocationChecks.Run();' --format json
unity command script eval --caller plugin --skill unity-cli --project-path "$PWD" --detach \
  'TerraBuild.BuildWeb();' --format json
python3 -m http.server 49174 --bind 127.0.0.1 --directory Builds/Web
```

`build-regions.py` needs Python, NumPy and Pillow. Downloaded tiles stay in ignored `worldgen/cache/regions`; the normalized packs are committed and play without a network GIS service. Unity's build entry packages all three packs automatically. Editor Play defaults to Wisconsin; a developer may set `terra.editor.location` to one of the other stable pack IDs to inspect it.

For browser-only iteration, `scripts/preview-atlas.py --player /absolute/path/to/an/existing/Builds/Web` stages the real template around an explicitly older player. This is **not** an integrated build and cannot establish new runtime behavior. Final verification must use the new `TerraBuild.BuildWeb` output.

Verification receipts and captures: [atlas evidence](evidence/atlas/README.md).
