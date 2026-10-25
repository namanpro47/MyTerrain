# Study 74 — continuous crop canopy and native edges

Prepared comparison; no rendered result or acceptance yet. Continuing isolated visual branch 8d3edbb after fetching origin/main ea37af047f4d8d167f88b802e76827c82a6c8183. No production changes enter the frozen combined release.

Study73 improved individual plant form but still read as bright isolated marks at normal zoom. This study changes representation: a terrain-following continuous canopy carries the field mass; original licensed Dglopez wheat geometry supplies exposed edges. An offline top render of the same native geometry/colors supplies the surface palette, with ordinary mipmaps. No generated image, purchase or reference-game artwork.

Reference targets: TN-02 landscape readability, TN-09 grouped vegetation, TN-12 coherent materials. Read [design reference](../../../../DESIGN_REFERENCE.md) and [art contract](../../../../art-direction.md). Reject flat carpet/slab appearance, obvious periodic paint, abrupt edges, or excessive geometry even if technically correct. Study52/58/70 failures remain relevant controls.

Scope: Play-only south-01 parcel, source-eligible cover and route exclusions, unchanged annual state. Same normal/close/detail cameras as study73. Actual Editor images are diagnostic only: no Chrome, performance, full HUD or Phase2A acceptance claim. The detail camera retains the existing 35 m terrain clearance floor.

Free asset source/license and archive hashes: [study72 acquisition](../study72-geometry/free-pack-acquisition.json). Original/derived standalone asset data remain local and untracked because the source license prohibits standalone redistribution. Scripts here contain no source mesh payload. Bake is 512 square, 8 m periodic tile, 961 native instances; runtime candidate uses a 0.75 m canopy grid and native edge plants. Actual counts will be recorded after application.

## Verification still required

Run bake outside Play; inspect output. Enter Play, verify advancing frames, capture matched control, apply candidate and capture normal/close/detail. Check unchanged source/annual state and shader errors. Stop Play, remove transient objects, confirm clean scene, close owned Editor and release resources. Preserve rejected outcomes without promoting them.
