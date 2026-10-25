# Crops with larger submission groups — integration verification

October 25, 2026. Source **3984343** merges crop/material checkpoint **7f258cc** with the192m grouping verified on the preceding conifer-only export. This is an Editor integration check, not Phase2A acceptance or a combined Chrome performance result. [Earlier actual Chrome comparison](../submission-cells-01/README.md). The visual owner is separately revising soil and woodland transitions; these captures do not include those later changes. TN-01/02/09/10 remain the visual reference patterns. No runtime/art source changes were needed for this check.

Unity6000.6.6f1,1280×720 camera output, pixel ratio1, fixed camera and landscape animation. The initial frame wait timed out during construction; the later wait proves1289→1299 frames before captures. No HUD or browser rendering is claimed.

| View | Small source batches | 96m draws | 192m draws | Total submitted sources | Culled sources |
|---|---:|---:|---:|---:|---:|
| Normal |900|680|352|1047|1296|
| Close |498|370|217|576|1774|

The source submission/culling counts match within each pair. Bypass draws are excluded from the consolidated draw columns. [Normal96m](normal-96.png), [normal192m](normal-192.png), [close96m](close-96.png), [close192m](close-192.png) were directly inspected: no visible loss or altered plant placement. Pixel differences remain small but nonzero: normal4,956 changed pixels,315 above two channel levels, mean<0.0042/255; close20 pixels change by≤1/255. The scene retains its known visual gaps, including noisy/bright soil and weak ground-to-woodland integration; this test does not accept those surfaces.

The [full-detail collector replay](replay.json) preserves all32,451 eligible instances and exact mesh/material/submesh/shadow/matrix multiplicities, containing source bounds and the500-instance draw cap. Across1,847 source batches,96m produces1,397 draws and192m669. This replay covers all active eligible small batches, not the large/transparent bypass, actual visibility/LOD choices or GPU time. The comparison override restores its original value after the audit.

The visual owner's [crop lifecycle script](lifecycle.cs) runs against this merged source and passes. It inspects348,424 plant-bound corners and1,464,388 soil vertices across opening and repeated traditional/margin/rotation previews and applications. Every inspected point is finite and within its source/site/action band;2,025 other vegetation batches retain their original LOD extent/cutoff. Preview cleanup and apply/remove retain owned-mesh counts and restore the surface mask. These are repeated inspection totals, not opening-scene plant counts or millions of independent test cases.

A stronger exact roundtrip check hashes all crop vertices, normals, UVs, triangles and plant transforms before and after the three lifecycles. [Both snapshots match](roundtrip.json):373 group hashes,1,282,588 soil vertices and38,435 leaf plants. This confirms that the renderer lifecycle restores the same baseline data. Year1/Planning/revision0/stored cash1,000,000,000 cents/zero proposals/zero accepted assets stays unchanged; no annual year was submitted.

[Console](console.json) reports no current errors/warnings and no compilation failure;14 historical warning entries remain counted in the session. Ground shader errors are empty after actual rendering. The own Editor was stopped and closed gracefully, and its four generated metadata/font/config changes were restored. No Web export, Chrome timing, public publication or release acceptance was performed in this integration check. The final combined scene still needs its browser and whole-game gates.
