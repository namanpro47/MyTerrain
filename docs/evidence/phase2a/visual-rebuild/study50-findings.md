# Studies 49–50 — selective relief and painted ground research

October 25, 2026. **Phase 2A remains unaccepted.** Source base is checkpoint `1bd75c2` plus the reviewed documentation merge of `origin/main` `667f482afd5733c24f248a1de42612fa43aa23c3`. These are actual 1280×720 Unity camera captures, with the main input controller disabled for a fixed diagnostic camera. They omit overlay HUD and are not browser, motion or public-build evidence.

Keep [DESIGN_REFERENCE](../../../DESIGN_REFERENCE.md) TN-01/02/09/10/12 and the [operative goal](../../../PHASE_2A_GOAL.md) beside these studies. Stronger terrain form cannot substitute for weak meadow/field construction.

## Terrain decision

Fresh construction of checkpoint 48 removed the visible post-warp bridge artifact: [normal](study49-normal.png), [close](study49-close.png). But [wide](study49-wide.png) revealed repetitive terraces on every hill and unnecessary ledges across the valley. Reject the uniform 6 m display bands as the ongoing direction.

Study 50 retains the analytical height on gentle slopes and introduces broader illustrative ledges on steep hillsides. It blends 18 m bands according to source slope (0.18–0.45 gradient), using a cached approximately 1.5 m triangular display grid. The [normal](study50-final-normal.png), [close](study50-final-close.png) and [wide](study50-wide.png) are more natural than the uniformly stepped study49. Hill relief is a candidate, not an accepted landscape. The broad meadow still reads as weak, fine repeated grass and crops remain conspicuous, and earth faces do not yet have convincing material/shape quality.

`LandscapeRenderer.Height` is the display sampler; `SourceHeight` matches the core analytical sampler. Rules and annual state must continue to use analytical data. `VisualLayout` returns a separate presentation layout without mutating the cached rule layout. Source sampling, picking, overlays and rigid project elevation must not be conflated. The new height cache has 662,708 floats / 2,650,832 bytes, avoiding repeated relief reconstruction during each plant/overlay query. No browser performance improvement is claimed.

The earth-face shader is now explicitly enabled by `_IllustrativeRelief` only for the Wisconsin candidate. Other regions retain the prior face behavior; the new material flag does not certify those regions' broader branch changes. Wisconsin's assumption text discloses illustrative steep-hill ledges rather than surveyed cliffs.

## Two rejected face constructions

The free Katsukagi/3DTextures.me Cliff Rock002 material from the [source audit](../../full-game/relief-material/README.md) was actually sampled on the candidate faces, including its normal map. [Initial close](study49-cliff-close.png) and [broader-scale close](study49-cliff-b-close.png) produced a dark repetitive wall rather than convincing irregular rock. Reject this flat-face application; the creator's displaced example is not reproduced by projecting its color and normals onto our slope. The source maps remain unchanged outside production Assets.

Native TriForge rock geometry/material was also placed along selected contours: [close](study49-outcrops-close.png), [normal](study49-outcrops-normal.png). The 180 instances / 62,640 source vertices look like oversized boulders laid against the hillside, not integrated outcrops. Reject this placement recipe. The temporary root was removed, Play stopped, and both study shaders were deleted through AssetDatabase after their text snapshots were preserved.

## New hand-painted resource, still a material study

Downloaded [Atapataco's free hand-painted grass/ground pack](https://atapataco.itch.io/handpainted-grass-ground-textures) through its free download option. The creator permits commercial use with credit in linked replies; [provenance and per-file hashes](study50-atapataco-source.json) retain that exact scope. No purchase or image generation. Actual archive contents are 24 PNGs, with no shader, mask or normal maps despite the page's broader terrain-tool description. Original archive/pixels are retained locally in `Builds/ArtResearch/AtapatacoGround`; no source-texture redistribution or production import is claimed.

A temporary shader samples the original Grass and Grass_desatured brushwork at a 32 m decorative scale, removes the unrelated grass normal maps and uses a 2× linear albedo multiplier. [Normal](study50-painted-normal.png), [close](study50-painted-close.png), [replay](study50-paint.cs.txt), [shader](study50-ground.shader.txt). This broad brushwork is a more useful candidate than the old fine grain; it still becomes mottled at normal scale and exposes the clump/field repetition. It is not adopted yet. The normal capture has stale wide-view outline widths from the locked-camera diagnostic; do not use those cyan outlines as UI acceptance or a fair outline comparison. The fresh-source final captures have normal outline widths. Temporary textures/materials disappeared on Play exit.

## Verification and limits

[Final compile](study50-final-compile.json): zero errors/warnings. [Fresh ticking](study50-final-ticking.json) proves the player loop advanced. [Bounded invariant replay](study50-invariants.json) checks 20,000 random source-height samples against core rules exactly; all 80 valid building layouts keep their cached analytical values; 1,324,044 independent foundation samples remain below the display foundation (minimum clearance0.03999996m, maximum fill2.45358658m). World JSON and the live annual diagnostic remain unchanged. Largest sampled display/source displacement6.59550476m is illustrative, not geographic accuracy. This is not a complete annual lifecycle test or a proof of all terrain contacts.

No new Web export, Chrome playtest, transformed-scene review, final performance sample, public deployment or acceptance score. The goal remains active. Next work must make ground/grass/field masses coherent together, then bring that direction into actual Chrome normal/close/wide and transformed views. Avoid repeating flat cliff projection, detached rock scatter, uniform height terraces, density-only grass tuning or the already rejected continuous rounded crop rows.

## Combined overlay follow-up

Reviewed and cherry-picked the narrow `a03d0d8` Surface vertex-sharing change as `55092a2`, then retained its separately scoped earlier evidence as `4c9ecb3`. The new cached/selective-height implementation was recompiled and the [old/new overlay geometry comparison](study50-overlay-equivalence.json) rerun on this combined source:334,594 checks over167,282 triangles pass, max position difference0.000061776m and normal-vector difference0.000016365. The comparison uses the documented symmetric sliver tolerance in [the replay](../../full-game/relief-validation/equivalence.cs); this is not byte-identical geometry. Fine-grid wind construction in this single Editor script was45.73→5.55ms with the same20,400 triangles and61,200→11,900 vertices. It is a local construction diagnostic, not input latency, Ghost presentation timing or a browser performance gate. [Compile](study50-overlay-compile.json) has zero errors/warnings; [frames advance](study50-overlay-ticking.json).
