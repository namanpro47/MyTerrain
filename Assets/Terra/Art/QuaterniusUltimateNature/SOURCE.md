# Quaternius Ultimate Stylized Nature — selected tree study

Creator: Quaternius. Official pack: https://quaternius.com/packs/ultimatestylizednature.html (May 2022). Free CC0 download from the creator-linked public Drive. Exact archive hashes and acquisition details are in `docs/evidence/phase2a/visual-rebuild/study42-acquisition.json` and `study42-preflight.md`.

The included License.txt is unchanged. Its header says Ultimate Platformer Pack; its body grants CC0, and the official nature pack page independently labels this download CC0. This discrepancy is recorded rather than silently rewriting the source license.

Only NormalTree_1/3/5 and their three original textures are committed here. Source geometry/pixels are unchanged. Unity imports retain mipmaps, cap leaf texture resolution at 2048 and bark/color-normal imports at 1024, and enable leaf transparency/alpha-coverage preservation at cutoff 0.5. The source download contains FBX/PNG, not a supplied Unity shader/demo. Native previews use URP Lit, a bark normal map, double-sided surfaces and low smoothness.

`ClusterStudy/` is an experimental derivative, with full-detail source meshes outside Resources. Leaf normals are grouped into 10/8/5 spatial clusters, then geometry/normals/tangents are baked from FBX axes and scale to metric Y-up roots at ground level. Topology and UVs are preserved. A project shader shades those normals and adapts color; original texture files remain unchanged. Prefabs are 6.86662 / 5.28104 / 2.98005 metres tall at scale 1, with 8520 / 7096 / 2182 triangles. Study45 adds three-level prefabs in `Assets/Terra/Resources/ClusterForest`, built by `Assets/Editor/ClusteredForestAssets.cs`, and connects them to the Wisconsin renderer on the unfinished visual branch. Reduced levels preserve selected whole leaf cards and simplify bark independently. Full-scene and Chrome validation are pending.

The exact replay, checks, comparisons and limitations are in `docs/evidence/phase2a/visual-rebuild/study44-findings.md`. Tree species, sizes and positions in the trial are illustrations, not survey observations. No purchase, generated image, production quality acceptance or performance claim.
