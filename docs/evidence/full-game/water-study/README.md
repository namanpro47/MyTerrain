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


## Combined Chrome decision — rejected

The local candidate `2356c72` was merged with latest main `18016cf` at **`6880869`**. The combined source passes 678 Atlas, 682 HUD and 13 water checks; actual Editor frames advance 114→1174. Its WebGL export succeeds in **398.424 seconds**, with **30 byte-matched served files / 46,622,175 bytes**. Control is the frozen final HUD export `dce9219`, 45,006,470 bytes. See [build](build.json), [served files](web-assets.json) and [combined Editor checks](integrated-editor-checks.json).

Actual Chrome 155, 1280×720, fresh Wisconsin Year1/Planning/revision0/$10M, no assets. Both builds follow the same reset → horizontal pan/return → zoom in/out → reset route. The other player is returned to the atlas with its game camera disabled while sampling. Each 30-second sample has 18 camera events, two preview events, no placements and zero unfocused frames. These are single comparative samples under shared-machine conditions, **not a controlled performance gate**:

| Export | FPS | p95 | p99 | Maximum | Frames >33.34ms |
|---|---:|---:|---:|---:|---:|
| Existing stream |41.53|42ms|48ms|127ms|231/1247|
| Native Clear candidate |33.48|58ms|63ms|73ms|314/1005|

Neither meets stable-30 p95. No frames exceed200ms, but no placement latency claim is possible. Candidate first interactive arrival is27.188s under the same shared-machine conditions, not a three-load gate. No error-level console messages are recorded for either player. [Control log](chrome-control-console.json), [candidate log](chrome-candidate-console.json).

The [matched control stream](chrome-control-stream.png) has more legible current; the [native candidate](chrome-candidate-stream.png) exposes the repeated bed and still reads as a patterned blue channel. The native appearance does not justify the measured cost. **Reject this adapter for production.** This is not a verdict on the package with its full intended workflow: planar reflections were deliberately omitted, and the supplied Clear preset has zero specular size. A proper supplied-reflection scene would be a different candidate, requiring its own whole-image and browser-cost proof.

Reduced motion is verified in Chrome. The stream-only rectangle `(570,270)–(600,320)` is identical in [reduced A](chrome-reduced-a.png) / [reduced B](chrome-reduced-b.png):0 of1500 pixels differ. Restoring full motion changes1165/1500 pixels between [full A](chrome-full-motion-a.png) / [full B](chrome-full-motion-b.png). Annual diagnostic remains unchanged. These image differences establish movement/freeze in that sampled region, not physical frame-time or aesthetic quality.

The runtime adapter, imported assets and material are removed from the integration branch; latest HUD/connected fields and atlas stay intact. Candidate source is recoverable at `codex/native-stream-study` / `6880869`; frozen candidate export is `Builds/Verified/6880869`. Evidence is retained here. Both test tabs were closed and their viewport overrides reset. No public deployment, accepted visual score or completed phase is claimed.
