# One location: Meadowbrook Valley

Selected October 24, 2026 for the full initial game. Game name **Meadowbrook Valley**; stable location ID **coon-valley-brye-v1**. Phase 1 selects the place; it does not ingest its geography or claim the development fixture depicts it.

## Name and provenance

On October 24, 2026, the user requested replacing the original name. **Meadowbrook Valley** is the fictional display name for this game location. Use it in the HUD, tooltips, plans, demos, learning content and future package deliveries. The actual AOI remains near Coon Valley / Brye Lane / Coon Creek, Wisconsin; retain those names only for source attribution, geographic lookup and historical records. This changes neither the chosen place nor its bounds, data, lesson or phase scope. Keep `coon-valley-brye-v1` as a stable internal identifier so the frozen fixture, checksums and prior evidence remain valid. The game name is not a claim that an officially named Meadowbrook Valley exists at these coordinates.

## Frozen footprint

- WGS84 editable envelope `[west,south,east,north]`: **[-91.043, 43.697, -91.031, 43.706]**, approximately **0.97 km²** (approximate latitude-scaled area).
- Context envelope: **[-91.044, 43.696, -91.030, 43.707]**. No additional level.
- Offline export CRS: **EPSG:26915, NAD83 / UTM zone 15N**, metres. Reference origin: WGS84 **[-91.037, 43.7015]**; package A must transform this point and record its projected easting/northing numerically.
- Elevation: use a consistent 2020 acquisition project, **WI_8County_2020_A20**, tiles x65y484 and x65y485, published 2024-01-24. Confirm each TIFF's vertical metadata before export; expected 3DEP orthometric metres/NAVD88, not yet verified from raster metadata. Choose a recorded elevation origin after ingestion; no invented datum conversion.
- Local X east, Y up, Z north; no exaggeration in data. The reference origin maps to local X/Z 0/0. Full editable/context clipping and pixel alignment are Phase 2/package A work.

## Shortlist and observed composition

All three candidates have CONUS USGS/MRLC data paths and public-domain federal data products. The same USGS Topo service supplied each saved preview. These are inspection images, not authored game maps.

| Candidate / inspected bounds | Source preview observations | Decision |
|---|---|---|
| Coon Valley, WI: [-91.045,43.697,-91.025,43.711] | Clear wooded slopes, open valley, branching mapped water, sparse structures and roads. Selected crop further balances woodland and open land. | Selected: strongest compact land/vegetation/water contrast; a direct-runoff lesson works without irrigation mechanics. |
| Cambridge/South Skunk River vicinity, IA: [-93.575,41.965,-93.555,41.979] | Broad flat open land, river corridor, prominent interstate. | Rejected: less relief and a major road dominates this small composition. The initial request ID `walnut-creek` was a search label, not a verified place name; preview shows this is not a Walnut Creek location. |
| Sonoma County/Goose Lake vicinity, CA: [-122.485,38.203,-122.465,38.217] | Strong hills, woodland, open patches and small ponds. | Rejected: attractive but steeper and less continuous editable land in this crop; drought/irrigation framing would require extra care outside our direct-runoff scope. |

Saved preview requests and bounds: [candidates.json](evidence/phase1/location/candidates.json). Selected [topography](evidence/phase1/location/selected-topo.png), [imagery](evidence/phase1/location/selected-imagery.png), and [NLCD cover](evidence/phase1/location/selected-cover.png) were inspected directly. These show composition and coarse cover; they do not establish parcel ownership, habitat condition, crop type, or permission to change land.

## Sources and reproducible data path

| Stable source ID | Observed availability / use / rights |
|---|---|
| usgs-3dep-wi-2020 | [TNM product query](evidence/phase1/location/selected-requests.json) returned four overlapping 1 m DEM products, including the two chosen 2020-project tiles; full records are in [elevation-products.json](evidence/phase1/location/elevation-products.json). The x65y485 TIFF returned HTTP 200, image/tiff, 127,490,723 bytes on 2026-10-24. Download and clip those two GeoTIFFs offline in package A; do not ship source tiles to the browser. [3DEP](https://www.usgs.gov/3d-elevation-program) is a USGS public-domain data programme. |
| usgs-nlcd-2021 | Freeze **NLCD 2021 land cover**, 30 m, for a reproducible initial cover layer, rather than chasing annual releases. MRLC WMS capabilities list `NLCD_2021_Land_Cover_L48`; an actual AOI GetMap succeeded. Use MRLC raster download for categorical values, not PNG colors. [USGS release](https://www.usgs.gov/centers/eros/news/nlcd-2021-now-available), [MRLC services](https://www.mrlc.gov/data-services-page), [viewer/download](https://www.mrlc.gov/viewer/). Public domain; credit USGS/MRLC. |
| usgs-topo-review-2026 | [USGS Topo service](https://basemap.nationalmap.gov/arcgis/rest/services/USGSTopo/MapServer), service refresh September 4, 2026. Preview-only composite includes 3DEP, NHD, transportation and other credited sources. For the pack, retrieve NHD flowlines and national transportation vectors through [The National Map](https://apps.nationalmap.gov/downloader/), retain per-layer metadata/date. Do not infer cadastral boundaries from the image. |
| usgs-naip-review | [USGSImageryOnly](https://basemap.nationalmap.gov/arcgis/rest/services/USGSImageryOnly/MapServer): saved [service metadata](evidence/phase1/location/imagery-service.json). Public-domain NAIP composite, advertised CONUS range 2017–2021; exact AOI acquisition year not determined. Used for visual inspection only, not shipped as a game texture or treated as contemporary field condition. |
| scenario-assumptions-v1 | Authored hypothetical demand, intervention sites, costs, productivity, soil B, 50 mm storm, capacity and protection rules. Project-original educational assumptions, never local forecasts or prescriptions. |

Exact preview URLs are in [selected-requests.json](evidence/phase1/location/selected-requests.json). The USGS imagery visibly supports a woodland/open-field mosaic. NLCD resolution will not capture narrow margins: preserve categorical area honestly, and label authored treatment margins and decorative vegetation separately.

## Scenario fit and gaps

Use the working productive-land / vegetation / direct-runoff trade-off. The rural place is real; all sites, demand and treatments are hypothetical. Cultivate uses an open plot or a 25% living margin. Restore initially means continuous grass cover, not automatic tree planting. Manage Water is a finite vegetated swale connected only to its defined contributing group. The fixture tests these categories without pretending to recreate the selected terrain.

Package A must still verify raster CRS/vertical datum, exact imagery dates if used, NHD/road geometry coverage, cover alignment, missing-data policy, and sensible simplify tolerances. Soil B is deliberately assumed; no measured soil survey claim. A small catchment inside this crop is not an entire hydrological watershed; do not model incoming floods or promise downstream flood protection. No surveyed field boundaries, local production/yield data, ownership, legal protection status or ecological condition have been established.

These gaps are explicit ingestion work for the selected location, not a reason to create another candidate level. Phase 1 stops at credible sources and the frozen selection.
