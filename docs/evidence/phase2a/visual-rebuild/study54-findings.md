# Study54 — connected grassland drifts

October 25, 2026. **Bounded Wisconsin candidate; Phase 2A is not accepted.** Source is `7d4cdcc` plus the saved change to `LandscapeRenderer.Grassland.cs`. Latest fetched main is `4a232e5`; its Brotli release tooling is not yet included in these captures.

Reference patterns [TN-02/09/10/12](../../../DESIGN_REFERENCE.md) and the [visual goal](../../../PHASE_2A_GOAL.md) govern this work. The existing capture remains substantially below the whole-scene target. This is an authored presentation of grassland, not a vegetation survey.

## Construction and decision

The previous mapped grass layer scattered 48,636 source plants across many small growth patches. Its thin, low blades broke the meadow into fine marks at normal play scale. The first experiment assembled the same free Quaternius grass into 215 larger lobed communities, totalling 25,004 plants. [Normal](study54-communities-normal.png) and [close](study54-communities-close.png) views show stronger individual masses, but they read as isolated islands. That layout is rejected.

The second experiment groups taller, overlapping source plants into connected growth swaths. It uses a broad two-scale field, graded height/width and irregular placement; it does not add a raised continuous shell or a new texture. The temporary trial had 22,968 plants in 182 groups. The saved implementation also clips its final tiles to the actual world bounds: **20,770 plants in 168 groups**. These counts cover only the mapped grassland root, not all scenery. Fewer plants do not establish faster rendering, because larger silhouettes affect detail-level selection.

[Saved normal view](study54-production-normal.png), [close](study54-production-close.png) and [wide](study54-production-wide.png) show the retained candidate. [Normal control](study54-control-normal.png) and [close control](study54-control-close.png) preserve the original layout at matching cameras. At normal scale the swaths have clearer boundaries and less fine scatter; close up they retain overlapping blade forms. The plant heights are deliberately illustrative (some exceed ordinary pasture grass); this is not a claim about observed species or management.

The change is confined to Wisconsin's mapped grassland construction, gated by the same `clusteredWoodland` choice as its current visual rebuild. Point Reyes, Suusamyr, annual treatment vegetation, instancing code, shaders, source textures, renderer material lifecycle and gameplay state are unchanged. The older treatment vegetation now needs a deliberate consistency comparison with this candidate before whole-game acceptance.

## Evidence and limits

A fresh compiled Play session—not only the transient experiment—produced the saved captures. [Compilation](study54-compile.json) reports zero errors/warnings; [frame check](study54-ticking.json) confirms advancement. [Placement verification](study54-verify.json) inspects all20,770 instance roots: none outside grass cover, none within the six-metre route exclusion, none in editable site polygons; maximum difference from the intended ground +0.03m root height is0.00000123m. This checks roots, not every leaf vertex. Source shaders report no compile errors. State is Year1 / Planning / revision0 / $10M / queued0 / assets0.

All images are actual1280×720 Unity camera output with overlay HUD omitted and controller input disabled for controlled framing. They do not verify Chrome, full UI, motion, transformation, performance or public deployment. The wide view exposes persistent regular fields, bare steep faces, canopy repetition and a weak overall ground treatment. No dimension is rated4.5; the visual gate remains open.

The historical console buffer includes earlier experimental assertions. It also records a recurring teardown exception when stopping Play: `AnnualChapterSession.OnDisable → StopRequest → ChapterChanged` refreshes a destroyed HUD RectTransform. [Before-stop buffer](study54-console.json) and [after-stop buffer](study54-console-after-stop.json) preserve its timestamps and stack. Do not describe the session as entirely error-free. This pre-existing callback/lifetime issue is not attributed to meadow construction and remains unresolved here.

## Recovery and next work

The source models, materials and their existing licenses remain intact. No asset download, purchase or image generation was needed. Replay scripts and receipts preserve both experiments; temporary scene objects are discarded on exiting Play. The source-backed candidate remains on the visual branch pending browser comparison. Integrate current release compression before exporting, then inspect moving normal/close views and measure actual frame costs. The field/ground and transformation gaps still need substantial work; do not continue another scatter-density loop as a substitute.
