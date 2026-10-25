# Road candidate integration and material inventory

October 25, 2026. Isolated source **`3eee670d9795cd3116e31c868e456be95513199c`** merges visual checkpoint `7d4cdcc` with main `4a232e5`, preserving selective display relief, overlay reuse, swale query optimization and lossless Brotli release tooling. Only this evidence is eligible for main; the scenery remains on `codex/relief-validation` and is not accepted or deployed. Patterns TN-02/05/09/12 and AX-01 remain the review basis.

## Integration checks

Unity 6000.6.6f1 / CLI 1.0.0-beta.14, isolated attached batch Editor. Recompile reports up-to-date with zero errors/warnings. The first Play session opened an empty scene: the inventory readiness guard failed, and a separate model wrapper encountered a missing controller. Neither changed production state or counts as a pass. After stopping and explicitly opening `TerraMain.unity`, the initialized player advanced from frame 1580 through subsequent checks and retained Year 1/Planning, revision 0, $10M, no queue/assets. Final state is recorded separately. The Editor was stopped and closed gracefully; no scene file was saved.

- `model-checks.json`: **678** Atlas/model/save assertions plus **34** annual lifecycle/JSON assertions, including 127 eligible offers and all 17 variants / 12 source families.
- `road-mask.cs` / `.json`: compares every **1,048,576** R-channel pixel to the old union-mask construction, with **zero mismatches**; 220 intersection pixels; correct Atapataco texture binding; production `_IllustratedPaths` already enabled; zero ground-shader errors; unchanged source-world JSON. The fixture checks the production setting instead of enabling it itself. This is mask/material integration, not a Chrome visual judgment.
- `annual-terrain-check.cs` / `annual-terrain.json`: 32 result records (31 invariants plus a timing observation), including 592 lowered swale vertices, 309 coincident chunk-edge vertices with matching positions/normals, exact cancel/undo restoration, replacement/removal previews, 1,281 narrow-strip groundcover instances, accepted Year 10 save replay, read-only comparison/return, unchanged live ledger/history and source sampling. This reruns the earlier integration fixture on the actual new road/material source, with a Play readiness guard. Its 84.47 ms Editor preview/flush observation is not browser latency.

## Material-sharing opportunity, not an implemented optimization

`render-inventory.cs` is read-only. At the opening Editor view it finds **666** InstancedNature components, **1,566** owned material objects and **14** distinct `(shader, content CRC)` groups:

| Shader | Material instances | Content CRC groups |
|---|---:|---:|
| URP Lit | 421 | 1 |
| Cluster Leaves | 421 | 3 |
| Soft Groundcover | 525 | 5 |
| Artist Grain Field | 48 | 1 |
| URP Simple Lit | 111 | 3 |
| Painted Canopy | 40 | 1 |

The frame reported 753 submitted and 1,388 culled batches; these are view-dependent Editor observations, not browser draw totals or new performance acceptance. Source calls and fingerprint behavior were checked against Unity 6000.6 [Material.ComputeCRC](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Material.ComputeCRC.html). The CRC is diagnostic grouping only: hashes can collide, and Unity warns their computation can change across versions. It must not become a persisted identity or sole correctness proof for sharing.

This suggests testing immutable material sharing with copy-on-write for `SetTint`, while preserving geometry, placement, LOD decisions and per-site lifecycle appearance. No materials were shared, no runtime renderer source changed, and no frame-time benefit is claimed. File ownership was requested from the active visual owner; their new grass construction may change these counts. Combined Chrome performance must be measured on the resulting candidate.

Remaining: accepted whole-scene imagery, final candidate Chrome moving/close/transformed views and three frame-time paths, final exact export/public run, two rehearsals and labelled recorded fallback. These checks do not certify the visual owner's in-progress grass study.
