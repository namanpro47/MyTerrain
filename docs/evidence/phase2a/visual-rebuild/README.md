# Phase 2A visual rebuild evidence

Status: active art studies; **not accepted**. The prior direction was rejected by the user on October 24, 2026.

Read the [operative goal](../../../PHASE_2A_GOAL.md) and [visual research](../../../VISUAL_REBUILD.md). The prior [wide](../captures/1280-default.png) and [close](../captures/1280-close.png) captures are the rejected baseline.

## Study 01 — changes being tested

- Replace dominant Kenney faceted canopy with nine selected Quaternius model assets (three broadleaf shapes, shrub, two grasses, flower, two rocks). CC0 license copied with the source files; [archive and selected-file hashes](assets.json). Catalogued variants do not imply observed local species.
- Use a continuous source-height surface with a separately blended visual land-cover texture. The analytical polygons and accounting remain unchanged; visual blending and decorative detail are illustrative.
- Frame the main field–woodland–creek area at orthographic size 290, pitch 50°, yaw −18°. Compare an overview at size 580, pitch 58°, yaw −32°. These are provisional study cameras, not a frozen contract.
- Rework canopy/understory distribution and stream bank colors/widths. Stream centerlines retain their source route; bank appearance is illustrative.
- Share foliage meshes with spatial GPU instance batches. [Unity API reference](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Graphics.RenderMeshInstanced.html). This is a memory/rendering hypothesis, not a performance result.

Actual Chrome captures and critique will be added after the study export runs. Texture/model import alone does not establish the visual bar. Water, transformations, terrain edges, and complete composition still require close review.

## Study 01 review — rejected

Actual local Chrome captures: [closer framing](study01-a.png), [overview](study01-b.png). Export job `324c16f5040a404f8e223d9f23236641` completed successfully; [HTTP/export checks](study01-export-check.json) passed.

The result is substantially below the reference: ground is over-bright, tree silhouettes remain too isolated, channel strips have visible segment joints, decision rectangles dominate the fields, and the overview returns to a distant slab. The assets load and render, but this is not an accepted art direction. No parity score or performance acceptance is claimed.

Next study addresses the whole image: correct shader color-space handling, compose a closer/lower camera, build overlapping canopy masses, join channel strip geometry, and remove persistent decision rectangles from the quiet world. The geographic/accounting model remains traceable.

## Study 02 — intermediate render, still below the gate

Build job `0e1f487b8a634d7a8a68867893aa4e01` succeeded. Chrome shows improved palette separation, overlapping canopy and closer framing. Persistent decision rectangles were removed from idle scenery; hover/selection still uses the site outline. The channel still looks like a flat diagrammatic strip, fields remain under-authored, and no final acceptance is claimed.

## Study 03 — in progress

- Separate visual terrain reconstruction at 2.5 m mesh spacing, sampled from the same 10 m source surface. A decorative stream cross-section cuts up to 1.7 m below that surface. This is **visual interpolation and illustrative bank geometry, not newly measured terrain or a runoff simulation**. Analytical Height(), cover geometry and accounting remain source-based.
- Mapped stream routes retain their centerlines; the illustrated water width is the pack width plus 6 m. The surface follows the source-height slope at a −0.6 m visual offset. No real depth, discharge or channel-width claim is made.
- Original generated meadow texture used as a terrain material detail layer; see [prompt/provenance](generated-meadow-prompt.md). Repeated at a deliberately illustrative 32 m visual scale, not a species-size measurement.
- Test 4× MSAA and a 4096 shadow atlas with 1600 m shadow distance against the closer camera. These are visual study settings awaiting browser cost measurements.

### Study 03 browser result

[Actual 1280×720 Chrome capture](study03-a.png). The inset water removes the segmented parallel-stripe appearance, and the generated ground material now renders in the game. Still below the gate: crowns lack enough internal volume, field areas remain too plain, roads need visual integration, and restoration still produces an inadequate flat rectangular treatment. These are unresolved art problems, not accepted compromises.

A controlled Restore → first site → commit → undo browser check recorded revision 1 / production 5 / credits 42, then revision 2 / production 6 / credits 60. No runtime errors were returned in that tab. This verifies that one retained transaction path works; it is not full visual or performance acceptance.

Build job `0d4979d1686b49ea837148dea3ad845a` lost its retained status after a script domain reload. The Editor log explicitly recorded a successful Web export; the [export identity/HTTP checks](study03-export-check.json) and actual loaded browser image independently confirm the exported build.

## Study 04 — canopy volume

A custom instanced leaf shader uses the imported leaf silhouette with rounded canopy normals, cool-to-warm shading and modest pigment variation. Source leaf-card normals alone made the crowns look flat. It retains alpha-tested leaf shadows. The [normal](study04-a.png) and [close canopy](study04-canopy.png) captures confirm that it renders in Chrome, but crown texture/volume still falls short of the painterly reference. This is a provisional shader study, not accepted foliage. Build `ca32b02f2e094ab38673fcc1a36dc807` succeeded; [export verification](study04-export-check.json) passed. The existing stripped FSR-upscaler warning remains; no new error was returned in the inspected tab. 139 [retained state/data checks](retained-state-checks.json) passed. None of these checks closes the visual gate.

## Study 05 — painted leaves and complete planted treatments, not accepted

The [normal view](study05-a.png), [canopy detail](study05-canopy.png), [restoration preview](study05-restore-preview.png), [committed restoration](study05-restore-committed.png), and [swale preview](study05-swale-preview.png) are actual Chrome output at 1280×720. Export job `0302909912684a39977286a02a8be732` succeeded in 72.041 seconds; [export checks](study05-export-check.json) passed. The expanded eleven-model subset and [original painted leaf texture](generated-leaf-prompt.md) render in the browser. Preview and committed treatments share their plant/material construction; baseline vegetation is hidden and restored with proposal state.

**Strongest criticism:** fine leaf detail increased, but the canopy remains a flat, busy mass. Large repeating fields, diagrammatic roads/water and sharp rectangular treatment boundaries still dominate. The restoration is visibly planted, but it is too small in the focused frame to deliver a strong transformation. This is below the visual gate. Studies 04–05 are iterations of the same direction, not two distinct art directions.

The controlled restoration commit produced revision 1 / production 5 / credits 42; undo restored revision 2 / production 6 / credits 60. An eligible Meadow edge swale preview rendered and cancellation returned to the baseline. No errors were returned in the inspected current-export Console; the existing FSR shader warning remains. These are limited regression checks, not full correctness, motion, performance or art acceptance.

**Required next pivot:** test a substantially different spatial composition and scale hierarchy. Compare a closer site-centered view and an oblique/perspective landscape study, with deliberate canopy volumes and quieter ground masses. Current focus size 160/minimum 150 leaves the treatment small; those numbers are disposable. Test a different foliage construction or lighting treatment if crown depth remains inadequate. Do not answer this critique with another density increase or global tint alone. Reference patterns: TN-01, TN-02, TN-09–12.

## Study 06 — perspective, scale hierarchy and planted fields

[Perspective](study06-perspective.png) and [orthographic](study06-orthographic.png) are matching Chrome comparisons at 1280×720. [Focused restoration](study06-restoration.png) shows the material difference made by reducing the close camera size from 160 to 72. The new perspective lens is 36°, default pitch 37°, yaw −32°, size-equivalent 150. F7 toggles projection for comparison; F5 is a closer canopy view, F6 the overview. These are experimental controls, not an accepted camera contract.

The trees no longer have the 1.6× sideways stretch; side lighting reveals their height and the terrain relief. Cultivated land now uses dense planted tufts from the same selected nature family instead of isolated tiny crop props. Mapped road surfaces have softer textured edges. Visible water corners are rounded within a six-metre corner neighbourhood while source line coordinates remain untouched; bank vegetation is illustrative. These changes apply across the same location.

**Review:** perspective and closer framing provide materially stronger depth and a readable transformation. The forest still looks like repeated stock forms, the new road mottling is initially too strong, and water lacks convincing exposed banks. The closer treatment is a promising improvement, but the whole scene remains below the art gate. This motivates a genuinely different foliage construction, not another recolour of the same mesh canopy.

Export `a8b818ade5a1483fb2663e41d9fd2523` succeeded in 69.575 seconds; [export checks](study06-export-check.json) passed. No errors were returned in the inspected current-export Console; the existing FSR warning remains. Earlier Editor camera captures omit overlay UI and are labelled `study06-editor-*`; they are diagnostic evidence, not browser acceptance. Reference patterns: TN-01, TN-02, TN-09–12.

## Study 07 — original painted forest, browser export rejected

An [original transparent tree atlas](generated-trees-prompt.md) tests a different rendering approach: painted silhouettes rooted in the 3D terrain, with the existing tree meshes retained only as shadow geometry while this mode is active. Both approaches use the same placement sample so composition can be compared fairly. F4 toggles this study in the running game; the standard mesh view remains available.

The first Editor [normal](study07-editor-painted-first.png) and [close](study07-editor-painted-close-first.png) captures demonstrate richer internal canopy shapes, but exposed floating root pivots and stretched card shadows. Those defects prompted explicit atlas-root offsets and a switch to 3D shadow geometry. The illustration by itself is not evidence of game quality. The revised shader, close camera motion, ground contact, whole-scene coherence and browser cost still require actual Chrome inspection.

The same study also tests an illustrative channel mesh at 1.5 m spacing, with a maximum 2.6 m decorative cut and water at a −0.75 m offset from source elevation. This is visual interpolation and an art profile, **not measured terrain, water depth or a simulation**. Channel corners use the same rounded visual route for the mesh, surface and distance mask. The analytical data and `Height()` remain unchanged.

### Study 07 browser result

Export `8dd5a067deb04b17880b25a9099ff1dd` succeeded in 369.944 seconds and [HTTP checks](study07-export-check.json) passed. The [mesh comparison](study07-mesh.png) renders; the [painted close view](study07-painted-missing.png) exposes a player-only failure: trees disappear, while their mesh shadow proxies remain. The Editor renders the atlas, so Editor output alone would have missed this defect. The build log retained four forward variants rather than eight. A serialized instancing-enabled material is being added to preserve the runtime renderer's variants. See [Unity's shader variant guidance](https://unity.com/blog/engine-platform/shader-variants-optimization-troubleshooting-tips).

**Capture correction:** the earlier viewport override reported a 1280×720 canvas, but the screenshot API produced a 1280×696 image with the emulated canvas reduced inside it. These are useful art-study observations, not full-resolution 1280×720 acceptance captures. Do not promote their viewport labels to final verification. New studies record actual screenshot dimensions and canvas dimensions separately.

## Study 08 — browser material, terrain-following camera and grounded paths

The painted-tree material is now an explicit instancing-enabled Resources asset, so build stripping can see its required variant. Camera targeting follows source terrain while panning, with a 35 m camera clearance for wooded hills and a guard during interpolation. A [source geometry review](camera-clearance-review.json) found 111 below-ground positions in 2,888 legal close views with the previous fixed target height. Browser motion still needs verification.

Mapped path strips now sample along and across their width at roughly one-metre spacing. Previously, two vertices across the full road could intersect convex terrain between its edges, leaving broken dark streaks. This geometry repair is separate from surface art; it does not establish a finished road or crossing design.

### Study 08 review — stronger canopy, camera combination rejected

Build `2a9e1d9321414ec2bd1ebe9fdb35ef8c` succeeded in 60.897 seconds; [export checks](study08-export-check.json) passed. The [mesh](study08-mesh.jpg), [painted perspective](study08-painted.jpg), [close canopy](study08-painted-close.jpg), [orbit](study08-painted-orbit.jpg), and [orthographic comparison](study08-painted-orthographic.jpg) are actual Chrome screenshots at native 1728×940, with the viewport override reset. The explicit material retained eight forward variants, and the painted forest now renders in the player.

**Strongest criticism:** the [steep-angle test](study08-painted-steep-failure.jpg) visibly flattens the vertical tree cards. Free tilt plus this asset construction fails the consistency gate. It cannot be excused by the good default frame. The next study deliberately replaces free orbit with a fixed illustrated angle and pan/zoom; all terrain and sites must remain reachable. It also replaces the mismatched round stock shrubs with original painted understory. Reference patterns: TN-01/02/10/12.

The [restoration preview](study08-restore-preview.jpg) and [settled treatment](study08-restore-committed.jpg) remain visually richer than the old flat patch, but cultivated repetition, coarse verges, schematic stream contact and hard treatment edges still fall below the gate. A real world click committed West field (revision 1 / production 5 / credits 42); Z restored revision 2 / production 6 / credits 60. No errors were returned for this export. One load logged 10,038 ms to interactive; this is an observation, not the three-load acceptance check. Current FSR warning remains.

## Study 09 — deliberate illustrated camera and coherent understory

This study takes the user's permission to remove existing aspects literally: free camera orbit/tilt is replaced by a fixed 37° pitch / −32° yaw, with primary drag or arrow keys panning, scroll/+/- zooming, F focusing a site, and R returning to the valley. The painted forest becomes the candidate default; F4 still permits a mesh comparison. This is an active experiment, not a frozen accepted art contract. Source geography and terrain-following camera clearance remain intact.

The [original understory atlas](generated-understory-prompt.md) adds four low shrub silhouettes from the same painted family. 3D foliage geometry remains only as shadow proxies in painted mode. More deliberate clearings break the uniform canopy coverage. Coarse sealed cover receives a subdued verge finish around the separately rendered mapped route; this is illustrative material treatment, not revised land-cover data. The [Editor diagnostic](study09-editor-understory.png) excludes HUD and is not browser evidence.

Build `b755f8f990a04ad7a86cf90229f17d2f` completed in 58.605 seconds; [export checks](study09-export-check.json) passed. Actual native 1728×940 Chrome captures show the [default](study09-default.jpg), [pan](study09-pan.jpg), [hillside](study09-hillside.jpg), and [overview](study09-overview.jpg). Limited pan/zoom checks rendered without observed terrain clipping. The overview still reads as a GIS slab; the complete scene remains below the visual gate. These observations do not establish full interaction or performance acceptance.

## User correction — existing assets first

The user rejected the reliance on generated images and asked for a much higher game-quality bar. Further generation is stopped for this pass. Studies 07–09 and their camera compromises remain unaccepted experiments. The next study evaluates an existing coherent environment family with its intended materials/shaders and demo setup, then checks it at actual Terra gameplay scales. [Resource register](../../../VISUAL_REBUILD.md#resource-register); [download inventory and provenance](assets.json). No new production asset selection or visual acceptance is claimed.

## Highest-impact remaining work

1. Evaluate coherent existing environment assets using their intended materials/shaders. First inspect the artist's setup, then test a representative section of the same AOI at elevated normal/close play scales. Do not continue generating or compensating for painted-card limitations. The current mesh and painted paths are both unaccepted comparisons.
2. Recompose woodland edges and field rhythms into larger intentional groups. Rich individual assets do not excuse a repetitive carpet or sparse, diagrammatic ground.
3. Integrate banks, stream crossings and mapped paths into the same material language. Exposed road geometry and flat blue water remain conspicuous even with a better canopy.
4. Bring existing interventions to the selected world standard, including boundaries and transformation. Evaluate at the closer playable camera, not only from the valley overview.
5. Only after the visual direction passes, finish the full supporting interface, performance and delivery gates. No current study certifies Phase 2A.


## Study 10 — supplied environment kit, actual Unity evaluation

The free Elijah Cobden Nature Pack was imported with its own materials, shader graphs and demo scene. The project's disabled Terrain module initially removed the demo ground; adding `com.unity.modules.terrainphysics` through Package Manager also enabled its Terrain dependency. The untouched demo scene was reopened. Package status confirmed completion after a reload dropped the original command connection. No new level was added to Terra; this is an asset evaluation fixture.

[Elevated creator demo](study10-creator-demo.png) and [same setup without fog](study10-creator-no-fog.png) are live Editor camera captures at 1280×720, with advancing frames verified. They are **not Terra gameplay or Chrome acceptance**. The source first-person controller has missing scripts; it was not used. Supplied leaf/grass/rock shaders report supported in Unity 6.6. These birch crowns still read as repeated topiary-like forms, with excessive leaf-frequency detail at the elevated angle. The family is not selected as Terra's canopy replacement. Its wheat is a separate bounded candidate for replacing repurposed grass in cultivated fields. Reference patterns: TN-01/02/09/10.

## Study 11 — artist wheat and stream/ground refinement

The downloaded wheat retains its supplied shader and textures, through an identity wrapper that preserves the artist's 100× root scale and removes its stored world-position offset. The source material is explicitly instancing-enabled for the player build. Both contextual cultivated land and editable sites use the same candidate; their source land classification and accounting remain unchanged. Stream incision is visually exaggerated to 3.8 m (illustrative only, not surveyed elevation), with dark exposed earth, a narrower grassy rim and reduced bright edge/ripple markings. Ground texture contrast is reduced to stop the pervasive stippling. Existing generated canopy remains an unaccepted comparison while the next free 3D tree family is evaluated; no further images were generated.

Build `ed92a82060554338a0de9ef5262d4f2f` succeeded in 75.649 seconds; [export checks](study11-export-check.json) passed. Native 1728×940 Chrome [default](study11-default.jpg) and [close](study11-wheat-failure.jpg) captures reject the wheat: fine alpha-cutout stalks disappear at elevated play scale. Coverage-preserving mipmaps restored pixels in an Editor diagnostic but produced a noisy stipple, so this is not accepted. [Restore](study11-restore.jpg) changed production 6→5 and credits 60→42; undo visibly returned 6/60. One startup NullReferenceException with an uninformative native stack was returned by the browser, plus the existing FSR warning. Do not certify a clean console or performance. The early Editor capture predates the corrected wheat origin and is not final evidence.


## Study 12 — free three-dimensional canopy and geometric crops

The free Elijah Cobden Trees Pack was imported and evaluated in its [supplied demo](study12-creator-tree-demo.png), with advancing Play-mode frames verified. Its oak forms were then tested in the same AOI. Separate identity wrappers normalize each tree to an 8 m base height and root contact; a parent transform broadens X/Z by 1.6 before the source FBX rotation, avoiding an accidental height stretch. Copied materials preserve the source shader/textures and use a coordinated palette. The [initial palette test](study12-palette-test.png) includes that subsequently corrected local-axis stretch and is diagnostic only. [Corrected oaks](study12-editor-oaks.png) remain an unaccepted Editor study. No new images were generated.

The textured wheat failed at the elevated camera distance. Three free Quaternius crop meshes were individually downloaded from the creator-linked public folder; only mature Wheat_4 is used. It is combined into a three-stalk group with illustrative proportions and a shared URP material. [Editor crop test](study12-editor-geometric-wheat.png) shows actual geometry, but repetition and sparse grain still need improvement. No crop growth/harvest system or additional level was added. Source files, hashes, modifications and licenses are in [assets.json](assets.json); required attribution is accessible through Field guide → Art credits.

Reference patterns: TN-01/02/09/10/12. Strongest criticism before browser export: the whole scene remains too repetitive and the roads/stream lack convincing integration. These substitutions are not reference parity. Study12 build `ced5650662734338ac13d1ae8d2e7d0f` completed in 65.236 seconds; [export checks](study12-export-check.json) passed. Native 1728×940 Chrome captures show [default](study12-default.jpg), [close canopy](study12-canopy-close.jpg), [cultivation preview](study12-cultivation-preview.jpg), [committed crops](study12-cultivation-committed.jpg), and [art credits](study12-art-credits.jpg). Cultivate changed West field to revision 1 / production 17 / credits 36; undo returned revision 2 / production 6 / credits 60. The credits page was reached through all four guide pages and closed normally. [139 real-pack contract checks](study12-contract-checks.json) passed on this source; [prefab bounds/material metadata](study12-prefab-metadata.json) verifies ground pivots and normalized dimensions.

**Visual gate still failed.** Real volume is present, but the close canopy is too uniform and lacks the reference's internal form/painted detail. Wheat remains thin, sparse and visibly repetitive. The road crossings and stream profile are still schematic. Do not promote this free-asset substitution to an accepted art contract. Internal diagnostic scores: composition 3, vegetation 3, ground/fields/water 2.5, lighting 3, transformation 2.5, whole-scene consistency 3; all below 4.5. These are observations, not a completed matched-reference review.

One Chrome load reported 25,150 ms to interactive, above the 15-second target. A single 30-second stationary canopy/guide diagnostic reported 45.47 average FPS and 24 ms p95; it included **zero camera, preview or placement events**, so it is not an acceptance performance route. No current-export errors were returned; the existing FSR warning remains. The study11 startup exception did not recur in this one load and is not proven fixed. Full browser sizes, motion video, three-load/three-route validation and public delivery remain outstanding. [Raw browser observations](study12-browser-log.json).

Next: address the dominant visual construction and composition gaps, not another isolated palette pass. Both supplied families have now received bounded evaluation. Favor a demonstrably richer free environment treatment or a substantial reworking of canopy/field masses, with representative forest–field–water output before another broad export. Keep the same location and existing gameplay scope. No new generation or purchases.


## Study 13 — construct the forest–field–water junction

Reference patterns: TN-02, TN-09, TN-11, TN-12. The trial addresses physical construction: a 2.1 m water-surface offset exposes the existing illustrative 3.8 m incision; a steeper bank profile, continuously spaced edge plants and partially submerged artist rocks establish shore contact. Three mapped road/stream intersections receive illustrative deck thickness, abutments and timber rails. These are decorative crossing designs, not observed bridges; the source guide now says so. Source terrain queries, cover classification and accounting are unchanged.

Cultivation uses denser, broader Quaternius wheat groups (1.6 m spacing, three mature stalks per group). A first Editor render was excessively bright and noisy. Muting the ordinary lit material made the field muddy; a dedicated matte grain shader now wraps direct lighting, retains external shadows and darkens lower stalks without per-stalk shadow casting. This is rendering adaptation of the existing free mesh, not generated artwork. [Editor diagnostic](study13-editor-matte.png). Crop instance batches increase to 500 while woodland retains 128, below Unity’s [default 511-instance limit](https://docs.unity3d.com/ja/current/ScriptReference/Graphics.RenderMeshInstanced.html). Elijah Cobden's supplied grass and earth textures replace the provisional generated ground texture, with unchanged source pixels and luminance mapped to the scene palette. Their attribution and hashes are recorded in the resource register. No purchases or new generated images.

Final build `8608fea76b4f4dcabbf0ac193870bf38` succeeded in 68.161 seconds (the earlier 102.080-second build preceded matte lighting and two technical corrections). [Exact export checks](study13-export-check.json) passed. Actual Chrome captures at 1728×940: [normal](study13-default.jpg), [crossing close](study13-crossing-close.jpg), [settled cultivation](study13-cultivation.jpg), [restoration](study13-restored.jpg), [overview](study13-overview.jpg). Cultivation committed at production 17 / credits 36; undo returned 6 / 60. Restoration committed at 5 / 42; undo returned 6 / 60. A second settled cultivation check and undo ended at revision 6 / 6 / 60. No current-build errors were returned; the existing FSR warning remains.

**Visual gate still fails.** The creek has real depth, exposed banks, planted/stone edges and readable crossings. Golden crop masses are clearer at normal/wide scales, but the close heads look oversimplified and repetitive; altered sites expose dark furrows and differ too sharply from surrounding fields. Woodland still resembles repeated fluffy crowns. Road intersections show seams; broad ground and banks remain simpler than the reference. The raster-shaped crop boundaries remain conspicuous in the overview. These are substantial gaps, not optional polish. No accepted art contract or parity claim.

A 30.010-second foreground route included 14 camera events (normal/close/overview/drag/zoom), 9 preview events and 2 placements. Average 58.21 FPS, p50 11 ms, **p95 42 ms**, p99 89 ms, max 184 ms; 184 frames exceeded 33 ms, none exceeded 200 ms, no unfocused frames. This fails both the 60-FPS target and the stable-30 fallback. Max preview CPU 2.9 ms / placement CPU 4.4 ms; 9 presentation-opportunity samples ranged 10.7–40.1 ms, not physical display latency. One warm/local load was interactive in 4,309 ms; it is not three fresh-cache loads. [Raw current-build logs](study13-browser-log.json).

Next work must resolve the geometry cost and visual construction together: profile normal versus overview, cache instance bounds and evaluate crop detail by projected size without degrading the view. Stop expanding/scaling the same low-detail wheat heads after this failed close review. Test a materially different field treatment with coherent close and distant appearance; then improve woodland silhouette/grouping and ground transitions. Preserve the successful channel depth and free authored terrain sources as candidates, not a passed whole scene. No Phase 3 work, purchases or generation.


## Study 14 — softened cover edges, creek composition, artist crop cards (failed gate)

The same real AOI now uses a blurred visual cover mask, a closer creek-crossing opening, streamside willow models and grouped oak sizes. Source classification/accounting remain unchanged. Generated canopy, understory and ground studies were moved out of Resources and removed from live rendering. Free Elijah Cobden source wheat was combined into nine-plant, 108-vertex clusters; cached instance bounds were checked against every transformed corner (zero failures).

Chrome captures: [opening](study14-default.jpg), [canopy](study14-canopy-close.jpg), [cultivation during reveal](study14-cultivation.jpg), [restoration](study14-restored.jpg), [overview](study14-overview.jpg). The Editor captures in this directory include intermediate 2.2m crop spacing and are not proof of the final 1.6m export. Build job `38514f19e996465f89505f751bae7e21` completed in 109,828ms; [exact export check](study14-export-check.json) passed. Served export 20,713,933 bytes, down from 23,156,571; data file 10,038,007 bytes.

After the user confirmed the browser was idle, a clean reload restored 6 production / 60 credits. Cultivate reached 17 / 36, undo 6 / 60; Restore reached 5 / 42, undo 6 / 60 (revision 4). The [browser log](study14-browser-log.json) records a 30.004s foreground diagnostic: average 32.329fps, p50 24ms, **p95 83ms**, p99 109ms, max 131ms, 134 frames >33ms, zero >200ms; 10 preview, 2 placement and 4 camera events, zero unfocused frames. Preview CPU max 2.5ms, placement CPU max 4.2ms. One local warm reload took 4,451ms; this is not the three fresh-cache acceptance test.

**Rejected as a final direction.** TN-02/09/10/11/12: rounded boundaries improve the map silhouette, but the field is an undifferentiated gold carpet, the willow line is too continuous, oak interiors remain noisy/dark, and water/roads remain simple. Crop overlap regressed frame times despite fewer vertices; vertex count alone was an inadequate proxy. No visual score meets the 4.5 gate. Next experiment must reduce overlap and establish field structure; do not add more detail to this dense carpet or use export size as a visual-quality claim.
