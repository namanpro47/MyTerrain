# Tested US adapter

The repository provides the maintained executable and tests. Run from a fresh Terra worktree; use a Python environment containing numpy, Pillow, pyproj, rasterio and requests (see `worldgen/requirements.txt`). Do not assume a venv exists in a new worktree. An existing read-only worldgen interpreter can execute the new checkout's script.

```sh
python worldgen/community.py \
  --id tomales-community-v2 --name Tomales --region 'California, United States' \
  --lat 38.24639 --lon -122.90556 --extent 1200 --pixels 2048 \
  --location-source 'https://www.marincounty.gov/sites/g/files/fdkgoe241/files/2024-03/tomales_community_plan_1997.pdf'
python worldgen/community.py --validate worlds/communities/tomales-community-v2
python -m unittest discover -s tests/community -v
python3 scripts/prepare-communities.py
```

`--output` permits an isolated trial directory. Default: `worlds/communities/<id>`. The supported crop is 400–2000 m in 10 m increments; image export 512–4000 square pixels. Keep imports bounded. The deterministic importer does not geocode: the skill resolves the name and supplies verified coordinates.

Sources:

- [USGS NAIP ImageServer](https://imagery.nationalmap.gov/arcgis/rest/services/USGSNAIPImagery/ImageServer): public-domain NAIP, queried source footprints, acquisition dates and native resolutions. Use this specific service; NAIPPlus and commercial imagery may have different terms.
- [NAIP product and rights](https://www.usgs.gov/centers/eros/science/usgs-eros-archive-aerial-photography-national-agriculture-imagery-program-naip).
- [USGS 3DEP elevation](https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer): unrendered float32 elevation, not a colored hillshade.

Both rasters are requested in the same local UTM CRS. DEM request bounds extend half a 10 m cell past the mesh extent so pixel centers coincide with terrain vertices. The image spans the actual mesh edges. Terrain values are flipped once and shifted by a recorded vertical origin, with no exaggeration. The photo is unlit in the viewer because its captured lighting is already baked in.

The 2026-10-25 fixtures are Coon Valley village center (43.7022, -91.0132) and Tomales (38.24639, -122.90556). Both use 2022 NAIP at 0.6 m native resolution, exported to 2048 pixels over 1200 m (0.586 m/export pixel). Coon Valley acquisition: August 30, 2022. Tomales uses May 19–20, 2022 source tiles. Source metadata, not these example dates, governs subsequent imports. USGS 3DEP is a mixed-date elevation mosaic; this adapter does not resolve its per-pixel acquisition date.

Failure handling: never create a partial published pack on service errors, incomplete elevation or changed bounds. Preserve cached input for diagnosis. A failed temporary image URL requires re-requesting the export metadata, not accepting an HTML/error payload as imagery. Do not repeatedly retry an unavailable provider; investigate coverage or choose a verified alternative. Check hashes before staging; deliberate geography changes require a new ID.

The `CommunityPack` schema is intentionally separate from WorldPack v1 and annual saves. The source/terrain viewer does not authorize projects on the photographed land.
