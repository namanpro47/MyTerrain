# Community imagery imports

User direction, October 25, 2026: make a reusable location-to-imagery-to-game skill, test it on additional communities, and stub the eventual self-service experience on the main selector. This bounded expansion adds real **terrain studies**, separate from the three annual scenarios. It does not certify Phase 2A, implement an AI API or reconstruct a community's buildings from one photograph.

## Available workflow

The personal skill is `terra-community-import`; its versioned source lives in `skills/terra-community-import`. Given a place name, the skill resolves its location, researches usable sources, inspects the crop, imports, stages and tests it. The first deterministic adapter is `worldgen/community.py`: continental US NAIP orthophotography plus USGS 3DEP elevation in one local metric UTM projection. Other regions require provider research and an adapter with verified reuse terms. It must not silently use a US substitute.

The native Unity atlas has **Your community +** → a three-step flow: Find your place, Review the real place, Shape its future. Automatic location search is visibly unavailable. Prepared community buttons load the actual source-aligned mesh; **Source photo**, **Overhead**, **Terrain**, rotation and zoom compare geography. The final annual-scenario step is visibly unfinished. No location is submitted to a service and no project is placed on photographed property. Closing releases the preview's geometry, textures and render target.

`WorldAtlas.Community.cs` is additive to the atlas. `CommunityTerrain` owns the separate render target and terrain study; it never calls annual state/session/save APIs. The original three journeys are unchanged. Sources load from local `StreamingAssets/Communities`, including in WebGL. A build preprocessor runs `scripts/prepare-communities.py` automatically. Gameplay runs remain independent of source exploration.

## Imported test communities

| Study | Window center | Imagery | Source acquisition | Elevation range |
|---|---|---|---|---|
| Coon Valley village, Wisconsin (`coon-valley-community-v1`) | 43.7022, −91.0132 | USDA NAIP, 0.6 m native | August 30, 2022 | 217.6–306.0 m |
| Tomales, California (`tomales-community-v1`) | 38.24639, −122.90556 | USDA NAIP, 0.6 m native | May 19–20, 2022 | 4.1–114.4 m |

Each crop is **1,200 × 1,200 projected meters**, exported to a 2048² image (0.586 m/export pixel). That slight resampling does not improve the 0.6 m source detail. Elevation is a 121² grid, 10 m between vertices, without vertical exaggeration. USGS 3DEP is a mixed-date mosaic; its acquisition dates and effective local resolution are not resolved by this adapter. Image acquisition is distinct from retrieval.

Representative crop review: Coon Valley includes the east–west main road, village streets, baseball diamonds, a curved creek and surrounding fields. Tomales includes the main road junction, residential grid, cemetery, school/track and pastures. These are bounded windows, not claims to cover the entire administrative community. The sources are **aerial photographs**, not satellite captures.

The exporter locks source raster IDs; it stores the exact requests, source catalog (including acquisition dates/resolution), provider description, SHA-256 hashes, projected/WGS84 bounds and transformation. `worlds/communities/<id>` contains small distributable outputs; raw elevation downloads stay in ignored `worldgen/cache/community`. Regeneration cannot overwrite an existing ID. `checksums.json` validates bytes before staging.

## Geographic contract and limits

The image fills the mesh's projected bounding box. U increases west→east and V south→north; Unity decodes the photo so its northern rows map to V=1. The DEM export extends 5 m past each mesh edge so its pixel centers lie on vertices. Its rows are flipped exactly once. Mesh Y is source elevation minus a recorded origin, not generated relief. UTM grid north is shown; it is not asserted to be identical to true north.

Plan-view roads, streams, field boundaries and roofs remain at their photographed positions because the original image is retained. Geometry represents terrain only: roofs and tree canopies do not become vertical objects, and the captured shadows remain in the photo. The unlit shader avoids adding a second set of ground lighting/shadows. This is a fidelity baseline, not the final Terra Nil art style or a complete semantic/3D reconstruction.

No property rights, zoning, cover classification, water network, hazard estimate, management opportunities or calibrated local annual scenario is inferred. Promotion to playable WorldPack v1 needs separately sourced/authored layers, parcel eligibility, scientific disclosure and annual tests. Do not fabricate those by copying the existing Wisconsin map or its policy assumptions. Existing annual saves are never migrated by this importer.

## Reproduce

Use a Python environment containing the worldgen dependencies. From the task checkout:

```sh
python worldgen/community.py --id my-community-v1 --name 'My community' \
  --region 'State, United States' --lat 43.7022 --lon -91.0132 \
  --location-source 'https://www.villageofcoonvalley.com/'
python worldgen/community.py --validate worlds/communities/my-community-v1
python -m unittest discover -s tests/community -v
python3 scripts/prepare-communities.py
```

Run `CommunityChecks.Run()` in the task's reserved Unity editor, then a normal Web build. Use Chrome for the selector and source/terrain comparison. Actual evidence belongs in [the verification record](evidence/community-import/README.md), not in this description of intended checks.

Design references: **TN-01** preserves the landscape as the main subject, **TN-06 / CS-04** expose sources and limitations, **AX-01** guides readable controls and keyboard traversal. There is still no accepted Phase 2A whole-scene benchmark; do not use these photos to declare that visual gate passed.

Sources: [USGS NAIP service](https://imagery.nationalmap.gov/arcgis/rest/services/USGSNAIPImagery/ImageServer), [NAIP product/rights](https://www.usgs.gov/centers/eros/science/usgs-eros-archive-aerial-photography-national-agriculture-imagery-program-naip), [USGS 3DEP service](https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer), [Village of Coon Valley](https://www.villageofcoonvalley.com/), [Marin County Tomales community plan](https://www.marincounty.gov/sites/g/files/fdkgoe241/files/2024-03/tomales_community_plan_1997.pdf). NAIP is public domain; USDA/USGS attribution is retained in the UI and manifests.
