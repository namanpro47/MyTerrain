# Study 74 — continuous crop canopy and native edges

Actual rendered comparison: rejected. No production adoption or acceptance. Continuing isolated visual branch 8d3edbb after fetching origin/main ea37af047f4d8d167f88b802e76827c82a6c8183. No production changes enter the frozen combined release.

Study73 improved individual plant form but still read as bright isolated marks at normal zoom. This study changes representation: a terrain-following continuous canopy carries the field mass; original licensed Dglopez wheat geometry supplies exposed edges. An offline top render of the same native geometry/colors supplies the surface palette, with ordinary mipmaps. No generated image, purchase or reference-game artwork.

Reference targets: TN-02 landscape readability, TN-09 close surface art, TN-12 light and scale hierarchy. Read [design reference](../../../../DESIGN_REFERENCE.md) and [art contract](../../../../art-direction.md). Reject flat carpet/slab appearance, obvious periodic paint, abrupt edges, or excessive geometry even if technically correct. Study52/58/70 failures remain relevant controls.

Scope: Play-only south-01 parcel, source-eligible cover and route exclusions, unchanged annual state. Same normal/close/detail cameras as study73. Actual Editor images are diagnostic only: no Chrome, performance, full HUD or Phase2A acceptance claim. The detail camera retains the existing 35 m terrain clearance floor.

Free asset source/license and archive hashes: [study72 acquisition](../study72-geometry/free-pack-acquisition.json). Original/derived standalone asset data remain local and untracked because the source license prohibits standalone redistribution. Scripts here contain no source mesh payload. Bake is 512 square, 8 m periodic tile, 961 native instances; runtime candidate uses a 0.75 m canopy grid and native edge plants. Actual counts will be recorded after application.

## Executed comparison

Unity6000.6.6f1 Metal, actual1280×720 Editor camera images without HUD. Frames advance from6 to760. The baked source tile is inspected locally; it contains native green leaves and golden grain clusters. Standalone baked texture remains untracked under the source license.

| View | Control | Continuous canopy |
|---|---|---|
| Normal | [control](control-normal.png) | [candidate](canopy-normal.png) |
| Close | [control](control-close.png) | [candidate](canopy-close.png) |
| Detail | [control](control-detail.png) | [candidate](canopy-detail.png) |

Each image has an adjacent receipt recording frame, dimensions and SHA-256. Normal camera matches study73 at(51.97,203.55,-297.20); close/detail use the archived scripts and actual game clearance constraint. The cyan selected-site outline stays present. Only south-01 changes; neighboring vegetable rows remain controls, not an intended mixed treatment.

**Reject this version.** Normal zoom gains a readable continuous field mass, but close/detail expose a flat green/gold patterned floor with taller plants outlining the lanes. The softened edge avoids a vertical slice wall, yet the interior has no convincing plant structure. The source texture's contrast also makes it read as repeated flower-like marks. This reproduces the surface limitation of studies52/58 despite improved boundary construction. Do not try to fix it with another tint, mip or density adjustment.

The canopy has21,460 vertices /39,258 triangles. Native edge plants alone number3,119 in40 cells and add5,214,968 triangles. This unoptimized diagnostic has no measured browser cost and is unsuitable for expansion. Seven native material regions per plant also remain unconsolidated. Successful compilation does not justify its cost or appearance.

## Next decision

A continuous surface can only be a distant representation; edge plants alone cannot make its close interior convincing. Any further version must change representation with viewing distance: recognizable plant groups throughout the visible near field, economical distant mass, and a verified transition during motion. Consolidate native materials and reduce geometry before a parcel-scale test; set a bounded geometry budget before adding instances. Preserve the artist's leaf/grain form and avoid another single-model swap. If the near/far transition cannot retain coherent forms, replace the field approach rather than extending this flat canopy. Whole-scene meadow/woodland/shore transitions remain open too.

[Application and source guard](application.json): exact source-world JSON unchanged; annual diagnostic remains Year1 /Planning /revision0 /cash1,000,000,000 cents /queue0 /assets0. [Shader inspection](inspect-result.json): supported, zero messages. [Scoped log scan](log-check.json): no compiler/shader/exception/pack-error markers. [Cleanup](cleanup.json): Play stopped, scene clean, zero study objects. Owned Editor90349 and launcher90345 closed and confirmed absent; reservation97 released for queued production review105. No runtime asset, main branch, scene or annual changes enter the combined release. No Chrome, performance or Phase2A acceptance.
