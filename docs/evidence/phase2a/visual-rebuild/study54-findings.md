# Study54 — connected grassland drifts

October 25, 2026. **Bounded Wisconsin candidate; Phase 2A is not accepted.** Source is `7d4cdcc` plus the saved change to `LandscapeRenderer.Grassland.cs`. Latest fetched main is `4a232e5`; its Brotli release tooling is not yet included in these captures.

Reference patterns [TN-02/09/10/12](../../../DESIGN_REFERENCE.md) and the [visual goal](../../../PHASE_2A_GOAL.md) govern this work. The existing capture remains substantially below the whole-scene target. This is an authored presentation of grassland, not a vegetation survey.

## Construction and decision

The previous mapped grass layer scattered 48,636 source plants across many small growth patches. Its thin, low blades broke the meadow into fine marks at normal play scale. The first experiment assembled the same free Quaternius grass into 215 larger lobed communities, totalling 25,004 plants. [Normal](study54-communities-normal.png) and [close](study54-communities-close.png) views show stronger individual masses, but they read as isolated islands. That layout is rejected.

The second experiment groups taller, overlapping source plants into connected growth swaths. It uses a broad two-scale field, graded height/width and irregular placement; it does not add a raised continuous shell or a new texture. The temporary trial had 22,968 plants in 182 groups. The saved implementation also clips its final tiles to the actual world bounds: **20,770 plants in 168 groups**. These counts cover only the mapped grassland root, not all scenery. Fewer plants do not establish faster rendering, because larger silhouettes affect detail-level selection.

[Saved normal view](study54-production-normal.png), [close](study54-production-close.png) and [wide](study54-production-wide.png) show the retained candidate. [Normal control](study54-control-normal.png) and [close control](study54-control-close.png) preserve the original layout at matching cameras. At normal scale the swaths have clearer boundaries and less fine scatter; close up they retain overlapping blade forms. The plant heights are deliberately illustrative (some exceed ordinary pasture grass); this is not a claim about observed species or management.

The change is confined to Wisconsin's mapped grassland construction, gated by the same `clusteredWoodland` choice as its current visual rebuild. Point Reyes, Suusamyr, annual treatment vegetation, instancing code, shaders, source textures, renderer material lifecycle and gameplay state are unchanged. The older treatment vegetation now needs a deliberate consistency comparison with this candidate before whole-game acceptance.

## Evidence and limits

A fresh compiled Play session—not only the transient experiment—produced the saved captures. [Compilation](study54-compile.json) reports zero errors/warnings; [frame check](study54-ticking.json) confirms advancement. [Placement verification](study54-verify.json) inspects all 20,770 instance roots: none outside grass cover, none within the six-metre route exclusion, none in editable site polygons; maximum difference from the intended ground +0.03m root height is 0.00000123m. This checks roots, not every leaf vertex. Source shaders report no compile errors. State is Year 1 / Planning / revision0 / $10M / queued0 / assets 0.

The preceding study images are actual 1280×720 Unity camera output with overlay HUD omitted and controller input disabled for controlled framing. They do not verify Chrome, full UI, motion, transformation, performance or public deployment. The wide view exposes persistent regular fields, bare steep faces, canopy repetition and a weak overall ground treatment. No dimension is rated 4.5; the visual gate remains open.

The historical console buffer includes earlier experimental assertions. It also records a recurring teardown exception when stopping Play: `AnnualChapterSession.OnDisable → StopRequest → ChapterChanged` refreshes a destroyed HUD RectTransform. [Before-stop buffer](study54-console.json) and [after-stop buffer](study54-console-after-stop.json) preserve its timestamps and stack. Do not describe the session as entirely error-free. This pre-existing callback/lifetime issue is not attributed to meadow construction and was unresolved at that capture checkpoint; the post-export integration below records its later fix.

## Recovery and next work

The source models, materials and their existing licenses remain intact. No asset download, purchase or image generation was needed. Replay scripts and receipts preserve both experiments; temporary scene objects are discarded on exiting Play. The source-backed candidate remains on the visual branch. The subsequent browser checkpoint below includes current release compression and actual movement checks. The field/ground and transformation gaps still need substantial work; do not continue another scatter-density loop as a substitute.

## Fresh reference inspection during export

The official [restored-bank frame](https://freelives.net/uploads/2024/04/Terra-Nil-Screen-3.webp) and [tropical frame](https://freelives.net/uploads/2024/04/Terra-Nil-Screen-2.webp) were inspected again in Chrome. The bank frame puts fine grass/flowers mainly at material boundaries while leaving broad interior surfaces legible. The tropical frame layers low vegetation, taller forms and crown masses; apparent richness is not a uniform density of small marks. These are observations of appearance, not claims about the reference renderer. Different geography, game scale and restoration state prevent a literal like-for-like scene comparison.

The new grass swaths move toward that scale hierarchy. They do not fix the conspicuously regular crop marks or the steep bare banks/faces. Next crop work should test a materially different planted mass with readable normal-scale forms and a compatible substrate, not another tiny row jitter. Retain source provenance and separate illustrative crop appearance from annual outcomes.

## Actual Chrome checkpoint — export `48154c4`

The grass candidate was combined with main `4a232e5`'s lossless Brotli tooling before export. The [successful build report](study54-build-report.json) records 371.513 seconds, zero build errors and 24 warnings (preserved in [the warning list](study54-build-warnings.json)). [All 31 served files](study54-web-assets.json) byte-match the local export; compressed Unity payloads decode successfully. Their total is **46,446,910 bytes**, including all three regions' runtime files. Do not confuse Unity's narrower `totalSize` report with transferred bytes.

This is a diagnostic build from the working tree, not a clean-checkout release. The [pre-build tracked inputs](study54-build-inputs.json), [packed-asset inventory](study54-packed-assets.json) and [identity/limits](study54-build.json) preserve provenance. An untracked imported TriForge wind-controller script and ignored Unity performance-test resource JSON appear in the build inventory. No claim is made that Git `48154c4` alone reproduces every byte. The unaccepted visual branch has not been merged to main or deployed.

Actual Chrome at **1280×720, DPR 1**, with matching 1280×720 canvas backing, shows the [full-HUD normal view](study54-chrome-normal-final.png) and [close view](study54-chrome-close.png). Larger grass forms survive browser rendering and normal pan/zoom. The [focused restoration proposal](study54-chrome-restoration.png) exposes the next important inconsistency: annual restoration still uses the old fine scatter and looks substantially weaker than the new surrounding swaths. This is a queued proposal, not an established or mature annual state. Queue → undo passes; cash remains 1000000000 cents, revisions 0→1→2, queue 0→1→0, assets 0. [Final logs and viewport](study54-chrome-final.json) contain no browser error-level entries.

One 30-second foreground keyboard-camera diagnostic records **54.28 FPS average, 34 ms p95, 44 ms p99, 64 ms max**, 88 frames above 33.34 ms and none above 200 ms. There are 1,140 camera events and zero unfocused frames; browser telemetry remains visible/focused with no blur events. **This misses the 33.3 ms fallback limit.** Six intended five-second arrow holds span two tool calls; dispatch gaps and a separate build possibly starting on the same machine make it unsuitable for attributing a speedup against earlier runs. It is one diagnostic, not the required three-path gate. [Raw camera logs](study54-chrome-logs-camera.json).

The first navigation logs interactive time 13,961 ms, but begins at the original desktop viewport before the 1280×720 test and includes atlas interaction timing. It is not a three-fresh-load acceptance test. No 45–90 second recording, 1440/1920 acceptance, full transformed-scene review or public verification is claimed. Chrome test tabs are closed and viewport override reset.

Main `b4bc7d8`'s separately validated silent chapter-teardown fix is integrated after this export; its evidence is [here](../../full-game/chapter-teardown/README.md). It is not present in the captured `48154c4` browser player. The next visual work should bring restoration's baseline/proposed/established vegetation into a coherent larger-scale construction, then address the crop field mass. Preserve the frozen pre-material-sharing export for comparison with the independent optimization candidate.
