# Terra visual contract v1

Status: Phase 1 implementation and review in progress. One Coon Valley landscape is the intended full game. The current scene is explicitly an invented technical fixture, not real geography.

## Direction

A warm miniature landscape: restrained moss/sage grass, darker clustered woodland, ochre earth edges, straw cultivation patterns and muted teal water. Contrast comes from readable cover masses and height, not saturated debug tiles. Keep terrain dominant, with a framed edge and a fixed opening view. Mapped scenery feels settled; a proposal has a pale ghost footprint, raised outline and a short settling reveal. Protection is communicated in text as well as a warm blocked outline. Decorative vegetation remains labeled as decoration in provenance.

HUD system is frozen to **uGUI + TextMeshPro**, with a working tool card/preview and information modal in `TerraHud`. One type family (Liberation Sans supplied with TMP Essentials), main title 34, measure values 26–28, tool headings 19, body 16–18, secondary labels 12–14. Base reference 1280×720 with CanvasScaler matched width/height; verify also 1440×900. Use 8/16/24-ish spacing, cream panels, rounded corners, forest-green selection, warm yellow margin choice. No second HUD framework. HTML owns only browser loading/failure and canvas/accessibility hints.

The top measures show illustrative productive capacity, pending same-storm runoff, and abstract credits. Until simulation integration, no zero runoff, percentages, or science-colored success badge. The selected real place name is paired with an explicit invented-terrain label. Tooltips show a hypothetical site label, exact current-to-preview production/cost, availability reason, and a single action hint. They use a fixed safe viewport region to avoid following the pointer off-screen.

## Assets and transform conventions

Reusable assets live in `Assets/Terra/Art/ExternalKit/` (package C) or the installed `Assets/Terra/Toolkit/Prefabs/` baseline. Root scale 1, local Y up, local +Z forward. Root pivot at ground contact, centered in X/Z; meters at runtime. Any imported source with a different pivot/scale gets a wrapper prefab; do not change main renderer scale constants to compensate. Publish the actual footprint `[xmin,zmin,xmax,zmax]`, maximum height, material slots and triangle count with each delivered prefab.

- Trees/grass/shrubs: decorative vegetation, not mapped points. Ground pivot; geometry should fit declared footprint. Keep materials shared (URP Lit/Simple Lit supported by browser). Current synthetic test applies deliberate scale ranges; these are not observed tree sizes.
- Crops: a small modular ground-cover cluster, no growth/harvest stages. +Z aligns to row direction, ground pivot. Renderer places repeated clusters within the site, never outside the polygon. Living-margin visual reserves 25% of the site for grass.
- Swale: finite footprint and a declared contour/orientation anchor (local origin, +X along contour, +Z downslope). No implied flood simulation. Fixture connection follows its authored scenario group, not invented geographic evidence.
- Preview compatibility: all final geometry can be hidden/replaced atomically. Ghost footprint and outline are separate render objects; no effect baked into shared baseline materials. Animation never owns accounting, and undo destroys the prior proposal overlay without modifying baseline.
- Materials: opaque baseline; separate preview material/object, no mutation of shared assets. No compute shaders, screen-space raytracing, expensive volumetrics, large texture sets or platform-only shader dependencies.
- Audio: reusable short interface clips after player input only; mute available. Current fixture uses one soft placement cue. Ambient bed is optional later, not an acceptance claim now.

## Independent preview and ownership

The existing **`Assets/Terra/Toolkit/ToolkitPreview.unity`** is the isolated asset preview scene: 24 Kenney nature specimens, shared URP materials, replayable DOTween reveal, interface sound button. It remains separate from TerraMain and from enabled build scenes. Its baseline verification is in SETUP.md; the Phase 1 handoff records the exact included assets/checkpoint. Open the scene directly to test geometry/materials/audio without editing the main scene. Package C supplies additions in its own isolated preview scene under its owned ExternalKit folder and includes footprint/scale/material metadata; no main-scene or global-render-settings changes.

Kenney Nature Kit and Interface Sounds retain their source license files (CC0). DOTween Free retains its included license. TMP Essentials retains its Liberation Sans license. Do not remove attribution/licenses or imply that kit meshes are actual surveyed plants.

## Interaction and review

Pointer and keyboard share the same command path. Drag threshold prevents camera motion from placing, UI raycasts isolate clicks, Escape cancels/closes the top modal, Tab cycles sites, Enter commits, 1/2/3 choose tools, M selects the margin, Z undoes, R resets camera. Pan/orbit/zoom are bounded. Motion is a 550 ms settling reveal; reduced motion publishes state instantly without the reveal. Panels have visible close/back controls.

The review requires browser captures at both target sizes for default, hover, blocked, placement and modal states, plus a short actual interaction capture. Score scene appeal, legibility, feedback, ease and coherence 1–5; repair the weakest category before calling Phase 1 complete. Evidence and final scores belong in PHASE_STATUS.md, not invented here. Internal scores are design review, not novice-user research.
