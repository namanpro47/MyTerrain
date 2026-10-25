# Stream surface candidate — October 25, 2026

Base: `5960a65` (runtime atlas `45a8748`). Patterns TN-09/11/12. The comparison is the Terra Nil desktop reference linked in `docs/DESIGN_REFERENCE.md`: submerged detail, bank contact and distinct surface movement. This is a candidate under development, not Phase 2A acceptance.

Matched diagnostics use the actual playing Wisconsin scene, then freeze frame 254: 1600×900, perspective FOV42°, pitch42°, yaw−32°, center (-166.295, 8.8599, -134.294), distances55/145/450m. The nine mapped stream features and channel meshes are unchanged. UI/labels are hidden for diagnosis. These are offscreen Editor renders, not Chrome evidence or timings.

The baseline has mostly flat colored water with drawn current marks. A first bespoke texture-layer trial made the shallows read as a paved border; reducing that contrast restored the flat-strip problem. Reject that approach (`layered-border-55.jpg`). Muted colors on the native shader and extra submerged rock scatter also did not improve the whole image; neither enters the candidate.

The supplied Clear preset from the free MIT-licensed MatrixRex **Uber Stylized Water** works with the actual channel bed, camera depth and scene-color refraction. Compared with the baseline it exposes the submerged surface and gives the shore a depth-based fade. Its unchanged blue palette is stronger than the muted trial. The repeated bed texture and regular cut banks remain criticisms; no 4.5/5 claim is made.

- [Baseline close](baseline-55.jpg), [normal](baseline-145.jpg), [wide](baseline-450.jpg)
- [Native Clear close](native-clear-55.jpg), [normal](native-clear-145.jpg), [wide](native-clear-450.jpg)
- [Rejected layered-border trial](layered-border-55.jpg)

The production candidate copies current Clear shader properties into a fixed Terra material, disables planar reflection (no corresponding component installed), and adapts all shader time inputs to the existing `_MotionTime` clock. Camera depth/color are enabled only for maps with water; the shared quality assets are unchanged. Neither source geography, annual state nor dry swale presentation changes. See the asset's `TERRA_PROVENANCE.md` and included MIT notices. Native source/preset textures were imported without upstream C#, editor extensions, pipeline assets or demo scenes.

`WaterRenderingChecks.Run()` in the loaded Wisconsin Editor passed **13 checks**: shader support/compilation, one shared owned material, intended feature keywords, no planar reflection, required camera inputs, unchanged global quality settings, full/reduced motion clock, no extra camera passes for either other region, and unchanged annual diagnostic. Exact Chrome export, animation/artifact review, transfer and performance checks remain pending. The candidate must not be pushed to main as validated until those pass.

Replay scripts retain the diagnostic setup. `native-trial.cs` names the original temporary import path; for reproduction use the final asset path under `Assets/Terra/Art/ExternalKit/UberStylizedWater`. The native material's source and adapters are preserved there. The additional stone trial is diagnostic only and discarded by stopping Play.
