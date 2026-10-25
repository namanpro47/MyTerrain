# Study71 — preserve original crop surface normals

**Rendered and rejected as a finished field treatment. Not adopted.** Source1f2eba3, origin/main13a15a4 inspected. Continuing the isolated visual experiment branch; the release checkpoint remains unchanged. Prior goal turn produced actual study70 comparisons and identified a useful broad crop construction whose shading/coverage still fails. TN-02/09/12; [study70 findings](../study70-field/README.md), [operative goal](../../../../PHASE_2A_GOAL.md).

The next controlled question is whether preserving the source mesh's face normals makes the crop heads and stems read as plants rather than a flat bright canopy. The exact same CC0 Quaternius FieldWheat geometry, deterministic324-instance periodic tile,16depth slabs per axis, color array, coverage mipmaps, terrain-conforming layout and camera remain controls. A linear-data48-slice normal array preserves original transformed mesh normals, normalized at sampling. This is geometry rasterization, not generated artwork or a new asset family. No change to source geography or annual models.

Bake normals in Edit Mode; import their transparent edges/mips; enter Play; apply the same full field with NormalWeight0 for the control, then toggle1 for the candidate. Compare actual normal/close/wide renders and the same live annual diagnostic. A shader compiling or a normal map looking plausible does not establish improvement. Do not continue color/density tuning if normal detail still fails to resolve the visible form. The full-map proxy remains an intentionally unoptimized diagnostic mesh; no performance claim follows from a low triangle count alone.

Prepared sources: [bake shader](VolumeNormalsBake.shader.txt), [bake](bake-normals.cs.txt), [normal import](import-normals.cs.txt), [runtime shader](VolumeFieldLit.shader.txt), [application](apply.cs.txt), [toggle](normals-on.cs.txt), [checks](check.cs.txt). Existing color inputs and hashes are in [the preflight record](prepared-inputs.json). These experiments remain outside production Resources and scene assets.

## Actual results and decision

All nine captures are actual 1280×720 Unity 6000.6.6f1 Metal camera renders, without HUD, from the same Play session and scene. The camera, color/coverage array, source geometry, parcel mask, lighting and study69 shared meadow are held constant. Only the normal weight changes. These are not Chrome or performance acceptance captures. [Capture hashes and frame numbers](render-receipt.json).

| Lighting | Normal view | Close view | Wide view | Assessment |
|---|---|---|---|---|
| Upward normal control, weight 0 | [normal](flat-normal.png) | [close](flat-close.png) | [wide](flat-wide.png) | Smooth but flat and foam-like |
| Original mesh normals, weight 1 | [normal](lit-normal.png) | [close](lit-close.png) | [wide](lit-wide.png) | Harsh dark speckling; plant form still does not read convincingly |
| Upward/source blend, weight .35 | [normal](soft-normal.png) | [close](soft-close.png) | [wide](soft-wide.png) | Less speckling, but returns toward the flat slab appearance |

**Reject all three as the finished treatment.** Broad field massing remains a useful direction from study70, but preserving normals alone does not solve its construction. The close view still has cut vertical lane walls, repetitive fine texture, and too little recognizable plant silhouette. The wide view still flattens the fields. TN-02's readable masses, TN-09's convincing close surfaces and TN-12's coherent scale are not satisfied by adding normal detail. No visual score or Phase 2A acceptance is awarded.

The next construction test must change the actual field edge and near-view plant representation, with a coherent distant representation. Use the existing licensed geometry at representative metric scale and compare those forms before expanding another full-map implementation. Do not spend another pass adjusting only normal weight, density or tint. The current 2,467,584-vertex diagnostic volume is not a production optimization, and no motion/shadow/browser cost claim has been established.

## Data validation and cleanup

[Raw bake validation](normal-validation.json) finds zero alpha differences from all 48 original color slices across 1,299,243 covered pixels. Decoded normals have length .993907–1.006454, consistent with 8-bit precision. The [import](import-normals-result.json) produces 48 slices with nine mip levels using native transparent-edge dilation and coverage-preserving mipmaps. This validates the data path, not the look.

The [full-map application](apply-all.json) hides 48 original crop components transiently and renders 12,852 tiles / 1,233,792 triangles. Source-world data is unchanged. The live annual diagnostic remains year 1, Planning, revision 0, USD 1,000,000,000, zero queued actions and zero added assets. Both study shaders are supported and have zero messages in the [final check](check-result.json).

[Cleanup](cleanup.json): Play stopped, TerraMain clean, zero study objects remaining. Own Editor 71190 and launcher 71187 closed and confirmed absent; reservation 69 released. No production renderer, HUD, annual state, geography or scene assets were edited. Imported arrays and shaders remain isolated under FieldMassStudy and are not shipped. Main advanced to ea37af0 during the comparison; it was fetched and its incoming commits reviewed, but this controlled experiment was not rebased or represented as a test of that combined release.

To reproduce, first prepare study70's color array and study69's shared meadow sources. Import this study's two shaders under FieldMassStudy. Copy archived `.cs.txt` scripts to an ignored `.cs` path. Run bake-normals, import-normals and validate-normals in Edit Mode; enter the initialized Wisconsin game; run setup, apply and connected-meadow. The apply script starts at normal weight 0. Capture normal/close/wide; run normals-on for weight 1 or normals-soft for .35 and repeat the same cameras. Stop Play, run cleanup/check and close the owned Editor. Use normal coordination reservations throughout.
