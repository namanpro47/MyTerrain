# Meadowbrook Valley geography

Phase 2 supplies one real landscape for the frozen Wisconsin AOI. **Meadowbrook Valley** is the fictional game name; the real source location remains Coon Valley / Brye Lane / Coon Creek. No second location or live generator is implemented. Package A is owned and integrated locally by the primary engineer.

The ready pack is `worlds/meadowbrook-v1`. Its three runtime files use the unchanged schema v1: `manifest.json`, `world.json`, `scenario.json`. The previous synthetic `fixture-v1` remains a small contract test fixture, not another playable level. Browser rendering and performance acceptance are recorded separately in [the Phase 2 audit](PHASE_2_AUDIT.md).

## Rebuild

Use Python **3.12** and the pinned environment. Raw sources are cached under ignored `worldgen/cache/`; normalized deliverables are committed under `worlds/meadowbrook-v1/`.

```sh
python3.12 -m venv .venv-worldgen
.venv-worldgen/bin/python -m pip install -r worldgen/requirements.txt
.venv-worldgen/bin/python worldgen/fetch.py
.venv-worldgen/bin/python worldgen/build.py
.venv-worldgen/bin/python worldgen/verify.py
```

`fetch.py` reuses and hashes cached inputs. `source-lock.json` pins each source file. A changed upstream service response fails clearly rather than silently changing the geography; intentionally updating sources requires reviewing and versioning the lock. Rebuilding from the same cached inputs produces identical normalized JSON. Dates are frozen source/retrieval dates, not current execution timestamps. Fresh internet access is needed only to populate an absent cache. The two raw DEM tiles total approximately 474 MB; they are never shipped to the browser.

Regeneration writes an initial `scenario.seed.json` and preserves any existing primary-owned `scenario.json`. The seed retains the Phase 1 illustrative treatment accounting; mission balance and environmental evaluation remain later phases. The generator does not edit Unity scenes, contracts or simulation code.

## Coordinates and fidelity

- Frozen editable WGS84 bounds: `[-91.043,43.697,-91.031,43.706]`. Every curated site is inside the exact projected polygon, not merely its axis-aligned bounding rectangle.
- WGS84 reference origin `[-91.037,43.7015]` projects to **E 658170.12464317, N 4840592.3595111435 m**, EPSG:26915 (NAD83 / UTM 15N).
- Unity X = easting minus origin, Z = northing minus origin, Y = source NAVD88 height minus **210 m**. No vertical exaggeration or datum conversion.
- Context is a 119 × 127 vertex grid at 10 m spacing, local bounds `[-590,-630,590,630]`. It encloses the frozen WGS84 context with a maximum extra corner margin of 40.886 m; it does not expand the editable location. Vector queries select features intersecting the frozen WGS84 context; wholly external features in those small extra corners are not inventoried.
- Elevation is bilinearly resampled from 1 m source cells, then exported south to north. Rendered surfaces use the same terrain triangles for ground, source cover, line ribbons and proposal overlays.
- Native 30 m NLCD cells are projected at their corners and clipped. The runtime receives 1,739 convex, CCW polygons, preserving class boundaries. `cover-classes.json` retains each original NLCD code. The display groups are deliberately broader than source classes: developed-open-space, shrub and hay/pasture become grass; developed classes/barren become sealed; woody wetlands become woodland. These are visual labels, not imperviousness or ecological condition measurements.
- Nine NHD line parts and thirteen road line parts are projected, clipped and simplified within 1 m. Rendered line widths are illustrative, not surveyed bank or carriageway boundaries. Multipart lines are split. Cover cells are convex and hole-free; auxiliary analysis GeoJSON preserves multipart polygons and interior rings.

[Alignment map](../worlds/meadowbrook-v1/alignment.png) shows terrain contours, source cover, mapped roads and streams, the exact editable boundary, all sites and three reference anchors. [Control points](../worlds/meadowbrook-v1/control-points.json) give the WGS84 origin, western stream junction and Highway 162/Brye junction. Millimetre round-trip tolerances test coordinate arithmetic only; they do not establish millimetre source accuracy.

[Geography checks](evidence/phase2/geography-checks.json) compare 1,638 independent elevation probes: mean absolute difference **0.217 m**, 95th percentile **0.808 m**, maximum **2.329 m** between the source's nearest 1 m sample and the 10 m rendered triangle surface. Acceptance limits are 1 m mean / 2 m p95 / 10 m max. Vector Hausdorff differences are **0.59–0.98 m**, below the 1.01 m processing tolerance. Rounded cover and unrounded analysis geometries conserve the complete context area within the documented tolerances. These measure processing fidelity, not freshness, survey accuracy or hydraulic suitability.

## Sources and assumptions

The [provenance manifest](../worlds/meadowbrook-v1/provenance.json) records complete request URLs, input sizes/hashes, dates, source licenses, processing and gaps. [Source feature properties](../worlds/meadowbrook-v1/source-features.json) retain individual vector dates and identifiers.

| Layer | Frozen source and date | Use / limits |
|---|---|---|
| Terrain | USGS 3DEP `WI_8County_2020_A20`, tiles x65y484 and x65y485; April 11–May 7, 2020, published January 24, 2024 | Bare-earth NAVD88 metres, confirmed by [USGS product metadata](https://www.sciencebase.gov/catalog/item/65bb34cdd34e18c6baf2e0f6). Public domain. No buildings or tree canopy heights. |
| Cover | USGS/MRLC NLCD 2021, L48 release `20230630`, native 30 m categorical WCS | Public domain; credit USGS/MRLC. Narrow margins unresolved. No inference of current farm condition or specific crop. |
| Water | [USGS NHD high-resolution flowlines](https://hydro.nationalmap.gov/arcgis/rest/services/nhd/MapServer/6), retrieved October 24, 2026 | Public domain; individual feature dates vary. Nominal source mapping scale is 1:24,000/1:12,000, not a field survey. Stream/ditch/artificial channel centerlines only. |
| Roads | [USGS National Transportation](https://carto.nationalmap.gov/arcgis/rest/services/transportation/MapServer), layers 30–32, retrieved October 24, 2026 | Public domain; credit USGS / US Census Bureau. Retrieval date is not acquisition date. |
| Vegetation / field detail | Decorative placements constrained by mapped cover | No surveyed trees, crop rows or field-parcel inventory. Mapped woodland masses guide the scene; individual plants, glints and widths are illustrative. |
| Sites and scenario | Project-original, October 24, 2026 | Ten hypothetical editable sites; two educational no-edit zones. No legal protection, ownership, conversion rights or agricultural suitability inferred. |
| Missing data | No local soil, cadastral, yield, structure or ecological-condition survey | Soil B, 50 mm storm, costs, demand, production and retention capacities remain explicit fictional assumptions. Runoff is disabled. |

## Management sites and later model handoff

`worldgen/sites.json` is the authored site source. Editable polygons preserve mapped woodland and wetland cells and keep at least **20 m from water centerlines** and **15 m from road centerlines**. These are conservative scenario buffers, not legal setbacks. [Measured clearances](../worlds/meadowbrook-v1/site-clearances.json) document each site. Protected woodland/stream zones are scenario choices, not statutory designations. They do not imply surrounding land is legally available for conversion.

`analysis-units.geojson` partitions the entire context into **1,711 non-overlapping units**, including all unchanged cover around sites. Each has metric area, baseline cover, parameter and contributing-group references. Site groups are deliberately limited to each site's own area; unchanged context is isolated. This is not a delineated drainage network or a full watershed. Future model integration must refine these assumptions explicitly, with no external inflow or downstream flood-protection claim. No Phase 4 model is implemented by this handoff.

[Unity contract checks](evidence/phase2/contract-checks.json) exercise the real pack with the existing serializer, all site/treatment previews and commits, undo, area conservation, protection, malformed bounds, missing terrain and unsupported versions. The tiny golden fixture remains accepted. Browser appearance, interaction and timing still require their own evidence; passing these source checks does not certify the rendered game.
