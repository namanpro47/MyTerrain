# Terra visual contract — v1 is historical, replacement in progress

> **Annual game presentation target — October 24, 2026:** Keep the historical visual facts below, but later gameplay uses Year, USD cash/commitments, Economic/Social/Environmental meters, categorized project actions, a current-year queue, End Year, annual reports and a Year 10 comparison/replay. [ACTIONS_AND_EFFECTS.md](ACTIONS_AND_EFFECTS.md) is the action/footprint/lifecycle contract. Proposed, building, establishing, operating and suspended appearances must be distinct. Peter's remote metric prefab/icon kit is an integration candidate, not accepted in-scene art. TN-01/03/04/05/08, CS-01/02/04 and AX-01 guide these surfaces. No new visuals or browser acceptance are claimed; Phase 2A's direct-quality gate and free-assets/no-generation constraints remain.

> **User-directed visual reset, October 24, 2026:** The current visuals were rejected. This v1 contract and the incremental Phase 2A experiments are not an accepted target. The [rewritten visual goal](PHASE_2A_GOAL.md) and [new direction/resource register](VISUAL_REBUILD.md) govern the rebuild. Existing meshes, materials, renderer, camera, layout and feature presentation may be replaced. Keep useful provenance/license facts below; do not preserve obsolete art prescriptions.

**Persistent reference:** [Terra Nil + Cities: Skylines design library](DESIGN_REFERENCE.md). Read its relevant pattern IDs before player-facing work. **Upcoming revision:** [Phase 2A](PHASE_2A_GOAL.md) establishes a stricter reference-quality bar after Phase 2 and before Phase 3. The contract below remains the historical implemented v1 foundation; do not present the proposed v2 direction as already shipped. Phase 2A will replace current tokens/layout guidance with the verified v2 system and link accepted browser captures here. Shared schema and data-contract versions do not change merely because art direction becomes v2.

Status: Phase 1 visual contract frozen; browser review passed. One Meadowbrook Valley landscape (fictional game name for the selected Wisconsin AOI) is the intended full game. At that checkpoint the scene was explicitly an invented technical fixture, not real geography. Current real-landscape integration status is recorded in [PHASE_STATUS.md](PHASE_STATUS.md); the historical v1 details below are not a statement that Phase 2 still uses synthetic terrain.

## Approved play-first interaction direction — October 25, 2026

Peter approved the icon-led planning, project inspector and map-linked year-end mockups: Cities: Skylines clarity with Minecraft's approachable learning-by-doing feel. The implementation keeps the landscape central, uses original vector icons beside short labels and existing project miniatures, and moves detailed explanations into optional panels. Budget commitments, delays, blocked actions and scientific assumptions remain inspectable. See [the HUD contract and checks](PLAY_FIRST_HUD.md). This updates interaction and information hierarchy; it does not certify or replace the separate scenery acceptance gate.

## Current HUD and connected-field revision — October 25, 2026

**Initial-community candidate:** [Study65](evidence/phase2a/visual-rebuild/study65-findings.md) depicts the four homes already in Wisconsin's fictional annual baseline. Metric catalog buildings sit on small foundations with terrain-following access; the continuous landscape supplies their yards. These are authored illustrations, not surveyed structures or newly funded projects. Editor checks pass; Chrome and whole-scene acceptance remain pending. TN-01/02/09/12 apply.

The user requested a much closer Terra Nil / Cities: Skylines interface and continuous development. The annual HUD now uses compact slate/navy panels, a prominent USD available balance, three equally weighted colored indicators, pictorial project cards with upfront price badges, and one bottom command bar. Cream remains the field-note/report surface. The older green-panel layout below is historical. Patterns TN-03/04/05, CS-01/02 and AX-01; [implementation, reference images and Chrome evidence](evidence/hud-connected/README.md).

The eight hypothetical southern management parcels share exact edges. Land treatments reach these boundaries and cultivation rows use a common world origin. Mapped water/woodland determine the stepped outside boundary; building prefab and lot dimensions stay metric. This bounded revision does not accept the broader Phase 2A scenery gate.

## Direction

A warm miniature landscape: restrained moss/sage grass, darker clustered woodland, ochre earth edges, straw cultivation patterns and muted teal water. Contrast comes from readable cover masses and height, not saturated debug tiles. Keep terrain dominant, with a framed edge and a fixed opening view. Mapped scenery feels settled; a proposal has a pale ghost footprint, raised outline and a short settling reveal. Protection is communicated in text as well as a warm blocked outline. Decorative vegetation remains labeled as decoration in provenance.

HUD system is frozen to **uGUI + TextMeshPro**, with a working tool card/preview and information modal in `TerraHud`. One type family (Liberation Sans supplied with TMP Essentials), main title 34, measure values 26–28, tool headings 19, body 16–18, secondary labels 12–14. Base reference 1280×720 with CanvasScaler matched width/height; verify also 1440×900. Use 8/16/24-ish spacing, cream panels, rounded corners, forest-green selection, warm yellow margin choice. No second in-game HUD framework. **October 25 correction:** The user rejected the HTML atlas. MyTerrain’s globe, location pins, selection controls, descent and return control all live inside Unity (URP + uGUI/TMP). The browser only loads the game canvas. Dark slate, mint markers and restrained cream typography connect the orbital scene to the annual HUD. See [ATLAS_SELECTION.md](ATLAS_SELECTION.md).

The top measures show illustrative productive capacity, pending same-storm runoff, and abstract credits. Until simulation integration, no zero runoff, percentages, or science-colored success badge. The Meadowbrook Valley game name is paired with an explicit invented-terrain label. Tooltips show a hypothetical site label, exact current-to-preview production/cost, availability reason, and a single action hint. They use a fixed safe viewport region to avoid following the pointer off-screen.

## Assets and transform conventions

Reusable assets live in `Assets/Terra/Art/ExternalKit/` (package C) or the installed `Assets/Terra/Toolkit/Prefabs/` baseline. Root scale 1, local Y up, local +Z forward. Root pivot at ground contact, centered in X/Z; meters at runtime. Any imported source with a different pivot/scale gets a wrapper prefab; do not change main renderer scale constants to compensate. Publish the actual footprint `[xmin,zmin,xmax,zmax]`, maximum height, material slots and triangle count with each delivered prefab.

- Trees/grass/shrubs: decorative vegetation, not mapped points. Ground pivot; geometry should fit declared footprint. Keep materials shared (URP Lit/Simple Lit supported by browser). Current synthetic test applies deliberate scale ranges; these are not observed tree sizes.
- Crops: a small modular ground-cover cluster, no growth/harvest stages. +Z aligns to row direction, ground pivot. Renderer places repeated clusters within the site, never outside the polygon. Living-margin visual reserves 25% of the site for grass.
- Swale: finite footprint and a declared contour/orientation anchor (local origin, +X along contour, +Z downslope). No implied flood simulation. Fixture connection follows its authored scenario group, not invented geographic evidence.
- Preview compatibility: all final geometry can be hidden/replaced atomically. Ghost footprint and outline are separate render objects; no effect baked into shared baseline materials. Animation never owns accounting, and undo destroys the prior proposal overlay without modifying baseline.
- Materials: opaque baseline; separate preview material/object, no mutation of shared assets. No compute shaders, screen-space raytracing, expensive volumetrics, large texture sets or platform-only shader dependencies.
- Audio: reusable short interface clips after player input only; mute available. Current fixture uses one soft placement cue. Ambient bed is optional later, not an acceptance claim now.

## Independent preview and ownership

The existing **`Assets/Terra/Toolkit/ToolkitPreview.unity`** is the isolated asset preview scene: 24 Kenney nature specimens, shared URP materials, replayable DOTween reveal, interface sound button. It remains separate from TerraMain and from enabled build scenes. Its fresh Phase 1 verification recorded 24 specimens, 24 completed animations and 10 clips in [art-preview-check.json](evidence/phase1/art-preview-check.json), with [an actual Play-mode capture](evidence/phase1/art-preview.png); the handoff records the included checkpoint. Open the scene directly to test geometry/materials/audio without editing the main scene. Package C supplies additions in its own isolated preview scene under its owned ExternalKit folder and includes footprint/scale/material metadata; no main-scene or global-render-settings changes.

Kenney Nature Kit and Interface Sounds retain their source license files (CC0). DOTween Free retains its included license. TMP Essentials retains its Liberation Sans license. Do not remove attribution/licenses or imply that kit meshes are actual surveyed plants.

## Interaction and review

Pointer and keyboard share the same command path. Drag threshold prevents camera motion from placing, UI raycasts isolate clicks, Escape cancels/closes the top modal, Tab cycles sites, Enter commits, 1/2/3 choose tools, M selects the margin, Z undoes, R resets camera. Pan/orbit/zoom are bounded. Motion is a 550 ms settling reveal; reduced motion publishes state instantly without the reveal. Panels have visible close/back controls.

The review requires browser captures at both target sizes for default, hover, blocked, placement and modal states, plus a short actual interaction capture. Score scene appeal, legibility, feedback, ease and coherence 1–5; repair the weakest category before calling Phase 1 complete. Evidence and final scores belong in PHASE_STATUS.md, not invented here. Internal scores are design review, not novice-user research.

## Phase 2 implemented landscape baseline

The canonical scene now loads `meadowbrook-v1`: real 10 m terrain, native 30 m source-cover polygons, mapped stream/road centerlines, a two-layer diorama edge and decorative woodland/field detail constrained by those layers. Individual trees and rows are not survey data. Cover and proposal meshes clip to the same terrain triangles; plant roots use sampled ground contact. Ten hypothetical editable sites and two scenario no-edit zones share the existing placement/undo semantics.

The Web profile now uses native render scale and a 3 km shadow distance appropriate to the real AOI. The authored wide view and F close-up replace fixture camera bounds. The location label has a cream backing for contrast over moving scenery. A two-page source panel distinguishes real place/bounds/sources from fiction. Current label: **REAL GEOGRAPHY · hypothetical choices**.

The old 2 m floating settling translation was removed to keep proposals seated; commits update immediately with existing sound/toast/outline feedback. Reduced motion also freezes subtle water glints. This is a deliberate Phase 2 correction to the historical v1 reveal above; Phase 2A may author a grounded reveal. No reference-quality parity, illustrated tools, redesigned HUD, ambient soundtrack or novice usability result is claimed. TN-01/02 (world and readable cover), TN-05 (preview), AX-01 (backed text), and CS-02 (spatial constraints) guide this baseline. Final captures and remaining design gaps are in [the Phase 2 audit](PHASE_2_AUDIT.md).
