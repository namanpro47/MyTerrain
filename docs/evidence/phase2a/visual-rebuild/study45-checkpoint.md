# Study45 — unfinished woodland integration checkpoint

Saved at the user’s request to commit and push work so far. Phase 2A remains open; this is not an accepted visual or performance result.

The study44 clustered canopy now has three distance levels per tree and is connected to the Wisconsin renderer. Whole artist leaf cards are spatially selected for reduced meshes; bark is simplified independently with the existing UnityMeshSimplifier dependency. Crown expansion is bounded after an initial check caught excessive far-level height. Ground paint repeats at 12 metres in Wisconsin; the other two locations retain their prior forest and 40-metre scale. Bark imports are capped at 1024, with original source pixels preserved.

| Tree | Full triangles | Mid | Far |
| --- | ---: | ---: | ---: |
| 1 | 8520 | 3726 | 1952 |
| 3 | 7096 | 2916 | 1666 |
| 5 | 2182 | 928 | 496 |

The preceding working session completed compilation with zero errors/warnings and [96 asset checks](study45-assets.json): prefab structure, material/submesh correspondence, unit normals, valid indices/UVs, nonempty foliage, grounded roots, height envelopes and reduced triangle counts. The [bake receipt](study45-lod-bake.json), [texture receipt](study45-textures.json) and [verification replay](study45-assets.cs.txt) are retained here. These receipts predate this checkpoint; no new tests were run just to save it.

Full Wisconsin Play-mode review, normal/close/wide LOD comparisons, annual lifecycle checks, Web export and actual Chrome visual/performance verification remain pending. No new screenshot, browser result, deployment or 4.5/5 claim is made. Compare against [study44](study44-findings.md) and the persistent [design reference](../../../DESIGN_REFERENCE.md), with Terra Nil’s whole-scene quality as the bar.

Base: 6bac68f, including origin/main through eb43f9d. At checkpoint preparation origin/main was refreshed to 8093653f7d51eb1079870484673be94ba169e39c; its newer stream/deployment changes are not merged into this unfinished checkpoint. Keep this work on codex/visual-study20 until combined validation. Incidental Editor/font/settings changes and unused imported packs remain local and excluded.
