# Study75 — near plants and distant field mass

Actual rendered comparison completed; implementation rejected, no adoption. Continuing visual branch30ede25 after fetch/review of origin/main f4bd323506a8c9a904c784703de5ab664591ff17. Current-main settlements, dense parcels and newer HUD remain with the combined release; this is an isolated Play-only rendering comparison.

Study74 proved that a raised patterned surface with real edge plants does not provide convincing close interiors. Change representation with viewing distance: reduced native plants fill the parcel when visible at useful size; a continuous field surface carries the distant mass. Compare actual normal/close/detail and several intermediate camera distances. No further texture/tint-only iteration or full-detail map expansion.

The same licensed dglopez wheat provides geometry and its seven native colors. Pinned MIT UnityMeshSimplifier53fdb312 targets25% and9% geometry. One vertex-color material replaces seven draws; colors convert to linear explicitly. A third empty geometry level removes distant plants. Initial thresholds:12px near mesh,4px medium/empty boundary,4–8px complementary screen dither between plant and surface. These are a test hypothesis, not accepted motion or performance.

Scope: original source-eligible south-01, terrain/route exclusions and unchanged annual state. Plant spacing0.48m is held for this construction comparison. The cost guard is to avoid full-detail geometry at normal/wide views; record actual per-view submissions and reject excessive geometry or poor transition. Simplification must preserve recognizable leaf/grain form in the actual close camera. No generated images, purchases or raw licensed source uploads. Original and derived standalone asset files remain local; source and license are recorded in study72 acquisition and study73 source-input evidence.

Reference: [TN-02/09/10/12](../../../../DESIGN_REFERENCE.md), [art contract](../../../../art-direction.md), [mandatory goal](../../../../PHASE_2A_GOAL.md). Whole-scene quality, current-source Chrome, motion, transformation and performance gates remain open. A single successful material/mesh check cannot accept this field or Phase2A.

## Relevant primary implementation research

[Spacejack's WebGL grass implementation](https://github.com/spacejack/terra#implementation) describes different near-ground and distant grass treatments, matching grass/terrain lighting, and fading geometry with distance. This supports testing the representations together; it does not prove our crop result or Terra Nil parity. [Cone Grass's author documentation](https://github.com/coen22/com.cone.grass#projected-blade-stability) discusses projected-size stability and calls for actual camera-motion validation. No source code or package from either project is imported here, and no WebGL compatibility is inferred for Cone's compute-based implementation.

## Actual geometry and rendered comparison

The first reduction removed the stalk bases: its bounds began about0.22m above the native root. [Rejected reduction receipt](geometry-root-loss.json). Retaining the original culm submesh restores contact before capture. Final single-material levels are508 /242 /0 triangles, down from1,672 and seven native material slots. [Final geometry](geometry.json). This is a reduction, not an accepted rendering budget.

[Application](application.json):49,333 plants,56 spatial cells and39,258 canopy triangles in source-eligible south-01. Exact source JSON and annual diagnostic remain unchanged. Actual1280×720 Editor camera renders, no HUD:

| View | Control | Distance-aware candidate |
|---|---|---|
| Normal | [control](control-normal.png) | [candidate](distance-normal.png) |
| Close | [control](control-close.png) | [candidate](distance-close.png) |
| Detail | [control](control-detail.png) | [candidate](distance-detail.png) |

Intermediate views: [zoom80](transition-80.png), [60](transition-60.png), [45](transition-45.png), [30](transition-30.png). Each has a frame/hash receipt and geometry-accounting JSON. The detail camera retains the existing35m terrain floor; these diagnostic zoom settings do not change playable camera limits.

The close interior now has actual plant structure instead of only a patterned floor. Nevertheless it is dominated by thin high-contrast stems and grain marks. Intermediate views become dense speckling. A source-lit surface comparison ([normal](matched-light-normal.png), [bake receipt](lit-bake.json)) avoids applying directional light twice but does not remove the change in coverage and visual texture across distance.

**Reject this implementation.** The field remains visibly below the reference in shape grouping and surface coherence. CPU accounting estimates11.52M selected plant triangles at normal and23.62M at diagnostic zoom30, before shader discard and hardware culling. This is not frame-time measurement, but it fails the intended geometry reduction: conservative spatial batches still submit most plants, and fragment fading does not remove their vertex work. One shared material alone is insufficient.

A further implementation caveat: the borrowed Simple Lit shadow pass does not share the custom plant fade, so partially faded plants can retain full shadows. The capture is evidence against this implementation, not proof that every near/far method fails. A future implementation must make color/shadow visibility consistent, or deliberately use a shadow-free small-plant treatment with supported ground depth. Do not rescore these captures upward or hide the error.

## Diagnostic zoom sequence

[Four-second encoded sequence](zoom-diagnostic.mp4), [60 actual-frame receipts](sweep-receipt.json), [encoding/hash](video-encoding.json). The callback waits two gameplay frames after each camera change and records actual camera render output. Playback is15 samples/second; PNG capture pauses are not real-time gameplay performance. This does not satisfy the final45–90s Chrome motion gate. Samples0/10/20/30 were visually inspected: dense near speckling changes into a broad flat distant field. The sequence preserves that transition for review rather than presenting isolated favorable frames.

## Next construction, not another wheat tuning pass

Stop expanding this detailed wheat family or adjusting its density/tint/normal weight. A free established alternative is acquired for the next bounded comparison: [GDQuest's Godot3 grass demo](https://github.com/gdquest-demos/godot-shaders/tree/1.0.0), pinnedf8fbea9. Its source mesh is seven triangles with nine positions; its two-sided shader uses a root-to-tip color ramp and world-driven wind. [Acquisition, hashes and source inspection](next-source.json), [license](next-source-license.txt). It is not yet ported, rendered in Unity, or accepted. The current Godot4 main branch is explicitly an incomplete migration, hence the pinned working-release source.

The useful next distinction is economical blade surfaces and authored gradient/mass shading, rather than thousands of detailed golden seed heads or another flat crop sheet. Inspect the supplied demo/materials, adapt the coherent construction to the same scene, preserve realistic scale/source masks and compare the whole field against meadow/woodland/water. Do not claim a surveyed crop species. Keep the native gradient as an explicit control, document adaptations, and measure actual submissions. The other whole-scene failures remain part of Phase2A.

## Cleanup and handoff

Shaders supported with zero reported messages; [scoped log scan](log-check.json) has no compiler/shader/exception/pack-error markers. Play stopped; [cleanup](cleanup.json) confirms clean scene and zero study objects. Owned Editor97230 and launcher97223 closed and confirmed absent. Reservation112 released; no further desktop queued. No production scene/renderer/HUD/annual change, main integration or deployment. Free resources only, no image generation. Phase2A remains active and unaccepted.
