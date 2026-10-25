# Studies 47–48 checkpoint — October 25, 2026

**Work in progress; Phase 2A is not accepted.** This checkpoint preserves research and the first source integration of illustrative terrain relief. It is not a validated release or deployment. Current remote main was fetched at `d70bad097b72b4ad5465b6581e7005767e53d8f4` and merged into the visual branch at `d0f9d13` before this checkpoint.

## Reference and decisions

Keep [DESIGN_REFERENCE](../../../DESIGN_REFERENCE.md), especially TN-01, TN-02 and TN-09, and the [visual rebuild goal](../../../PHASE_2A_GOAL.md) visible during the next comparison. Terra Nil's composed terrain masses and material separation remain the bar. Study 48 has not established parity.

Study 47 tested source-painted ground, a bank ribbon, and the free [Yughues/Nobiax grass family](https://opengameart.org/content/multi-stylized-grass). The imported subset retains original model/texture bytes, authored normals, CC0 license and SHA256 provenance in `Assets/Terra/Art/YughuesGrassStudy`. These are research assets with no production bindings. Native and adapted grass trials produced thin blades, a dark dense carpet, or repeated pale texture; the source-painted ground was blotchy. None was adopted. The `adapted-meadow` captures have an incorrect height-fade control; use the `corrected-meadow` captures for the corrected trial. Scripts, shader snapshots and receipts are retained alongside captures.

Study 48's temporary post-warp prototype made wooded hills and lower fields more distinct, but the rock texture resembled masonry and deforming completed bridges caused artifacts. The `relief` and `face` captures show that temporary prototype, **not the integrated source in this checkpoint**. Its source-world JSON and annual diagnostic were unchanged; this does not validate the new integrated implementation.

## Integrated candidate

- Wisconsin display terrain uses illustrative 6 m elevation bands on the existing approximately 1.5 m render grid. Analytical source sampling remains separately available as `SourceHeight`; core rules continue to use their own analytical sampler.
- Overlays share the display triangulation. Presentation layouts are derived separately from cached analytical building layouts, including display-height foundation sampling; camera framing uses the presentation layout.
- Steep faces use the existing artist earth map. This differs from the stone-face prototype captured here. The face shader also affects steep surfaces in other regions and still needs regional review.
- Wisconsin's Data & Assumptions panel explicitly discloses the illustrative bands and unchanged analytical terrain.

[Unity recompile](study48-compile.json): completed, zero errors and zero warnings. The integrated candidate has **not yet been freshly played, captured, exported or tested in Chrome**. No performance, geometry-contact, annual-state or regional acceptance is claimed for it.

## Next verification

Start a fresh Play session and check shared terrain/overlay heights, water, bridge construction, rigid project foundations and camera focus. Verify source geography, cached rule layouts and annual accounting remain unchanged. Measure added overlay geometry and construction cost. Then compare normal/close/wide/transformed views and full HUD in Chrome before deciding whether to retain this direction. Meadow and crop repetition, whole-scene cohesion, performance and public export gates remain open.

Unused asset imports and incidental Unity-generated local settings are deliberately excluded from this checkpoint. No purchases, image generation, main-branch release or deployment occurred.
