# WorldPack v1 — Phase 1 contract

Status: implemented fixture contract, **schemaVersion 1**, fixture **1.0.0**. The selected real location is [Coon Valley / Brye Lane](location-selection.md); `worlds/fixture-v1` is synthetic test data only. Neither this format nor the fixture is a completed real location.

## Files and serialization

Canonical authored sources live in `worlds/<pack-id>/`: `manifest.json`, `world.json`, `scenario.json`. `Phase1Setup.PackageFixture()` copies the three files into ignored `Assets/StreamingAssets/WorldPacks/fixture-v1` before build. Never maintain both copies by hand. Runtime loads all three with `UnityWebRequest`, including in the browser. Fields map directly to the serializable DTOs in `Assets/Terra/Scripts/Contracts/WorldContracts.cs`; plain arrays, nested objects, camelCase public fields, finite JSON numbers, no dictionaries or polymorphic values. No comments, NaN or Infinity. UTF-8 strings; IDs are stable case-sensitive strings. Unknown schema versions fail before rendering; missing required structures fail with a readable error. `Phase1Checks.Run()` exercises actual Unity `JsonUtility` round trips and invalid fixtures.

All listed fields are required unless explicitly marked optional. Empty arrays express an available but empty layer; `availability: absent` expresses missing data. Never fabricate features to fill a gap. A source with unknown acquisition date must say `unknown` rather than reuse the download date.

## Manifest

`schemaVersion`, `packId`, `fixtureVersion`, `geographyKind`, `selectedLocationId`; `sourceCrs`, `elevationDatum`, `originWgs84` `[lon,lat]`, `originElevationM`; `boundsWgs84` `[west,south,east,north]`; `editableBounds` and `contextBounds` `[xmin,zmin,xmax,zmax]` in local metres. `worldDataPath=world.json`, `scenarioPath=scenario.json`. `layers[]` has `id`, `path`, `availability` (`mapped`, `synthetic`, `absent`). `sources[]` has `id,title,url,date,license,role`; source IDs on features/sites refer here. URL may be empty only for original authored data. Synthetic fixtures have empty geographic bounds/origin arrays and an explicit local engineering CRS/datum. Real packs require actual coordinates/datum; see the selection note for the frozen export CRS/reference point.

`rasterRows=south-to-north`, `winding=counterclockwise-xz`. Local **X east, Y up, Z north**, all metres. GIS projects and transforms externally. No latitude/longitude is passed to placement. Vertex Y is height above the recorded elevation origin, without exaggeration. The GIS origin maps to X/Z 0/0; record an independently checked reference point in provenance. Grid index `row*columns+column` increases X within rows then Z between rows. Build upward-facing Unity triangles in reverse of the CCW X/Z ring order (`0,2,1` for a 3-point ring). Preserve geometric alignment across layers.

## World data

- `terrain`: `columns,rows` (2–257), `cellSizeM>0`, `xMin,zMin`, row-major `heights[]` exactly columns×rows. One regular grid. Package A must fill or reject missing heights explicitly, never silently turn nodata into sea level.
- `covers[]`: `id,coverId,sourceId,polygon[]`; each point `{x,z}`. v1 cover IDs: `cultivated`, `grass`, `woodland`, `water`, `sealed`. Record any input-class mapping in package provenance; do not infer crop species from a coarse land-cover class.
- `features[]`: `id,kind,sourceId,decorative,widthM,points[]`. Kinds `water`, `path`, `connection`, optional `building` and `vegetation`. Water/paths are polylines with ≥2 points. Building footprints use polygon rings; decorative vegetation positions use one point. `connection` is authored scenario geometry, not inferred measured flow. Set `decorative=true` for nonmapped vegetation; no implied surveyed tree positions.
- `sites[]`: `id,name,sourceId,polygon,analysisAreaM2,baselineProduction,baselineCover[],allowedTreatments[],offers[],protectedSite,protectionReason,contributingAreaId,modelParameterId`. `baselineCover` is `{coverId,fraction}`, sums to 1. Canonical area is polygon area in square metres, independent of terrain triangle surface area. `baselineProduction` is illustrative existing production, not measured yield. Protected sites have a reason and zero offers; unprotected reason may be empty. `offers[]`: `treatmentId,cost` integer abstract credits, `production` final illustrative units, `retentionCapacityM3` nonnegative assumed capacity.

v1 site/cover polygons are convex, simple, nonzero CCW rings, 3–64 points, **no repeated closing point, holes or multipolygons**. Package A must split complex cover polygons into nonoverlapping convex patches; author convex scenario sites. Do not flatten holes into filled areas. Site areas cannot overlap; points lie inside editable bounds, and all geographic context stays in context bounds. Terrain spans context bounds. Raster-to-cover vectorization must preserve area within a reported simplification tolerance. Geometry ingestion/validation expansion belongs to package A; the tiny runtime fixture is not a GIS validator.

## Scenario

`schemaVersion=1`, `id,packId,budget,introId,learningId,assumptionsId`; `treatments[]`: `id,family,title,description,visualRecipeId,modelParameterId,productiveFraction,cover[]`. All costs/capacities are site-specific offers, not hidden UI constants. `storm`: `id,soilGroup,antecedentCondition,sourceId,rainfallMm,initialAbstractionRatio`. `objectives`: `productionMin,dischargeMaxM3,runoffEnabled`. When runoffEnabled is false, discharge threshold is inactive, never shown as a zero target. Phase 1 has no win/mission flow.

Fixture recipes: open plot = 100% cultivated; living margin = 75% cultivated/25% grass and 75% of the open-plot illustrative units; restore meadow = 100% grass/no productive units; connected swale = 100% grass, no productive units and finite 8 m³ assumed capacity at its one eligible site. Example costs are 24/30/18/28 credits, initial budget 60. Existing site production totals 6; the illustrative target is 20. These values are test balance, not prescriptions for Coon Valley. No growing, harvest, seasons or crop-yield model.

The margin occupies real area in the model. Its eventual runoff effect comes from cover/CN inputs, not an arbitrary global bonus. Use one 50 mm storm, assumed soil B and initial abstraction 0.2, and cap retention to generated water in the connected group. The test HUD says pending until the simulation package is integrated.

## Validation and identity

`PackValidator.Validate(manifest,world,scenario)` is pure C#; `Phase1Checks.Run()` loads the valid fixture and rejects unknown-version, missing-terrain, negative-cost, duplicate-site, bad-cover and area-mismatch files under `tests/fixtures/invalid/`. `checksums.sha256` in the fixture records the exact three source files; check with `shasum -a 256 -c checksums.sha256` from that directory. Contract owner approves schema changes. Increment schema for incompatible structure/semantics, fixtureVersion for content changes, and regenerate checksums. Package A delivers the selected real pack under its own stable pack ID, replacing the fixture in the main scene only during Phase 2 integration.
