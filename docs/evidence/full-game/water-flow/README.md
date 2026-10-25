# Lightweight stream-surface candidate

Patterns TN-09/11/12. This follows the rejected full native adapter in `../water-study/`. The dominant failure being tested is flat surface shading without a readable moving normal. Generated channel meshes lack tangents, so the single supplied normal texture is sampled explicitly in world space instead of using a tangent-space Shader Graph path.

The existing one-pass shader gains two moving texture samples, restrained direct-light highlights and an illustrative sky-color response. It retains drawn currents and opaque water so the repeated bed texture does not dominate. Full and reduced motion use the existing `_MotionTime` clock. The dry-swale branch retains the previous output. No camera depth/color copy, refraction, planar reflection, new geometry, annual change or global quality change. The MIT source notice ships with the atlas and is linked from About; source pixels are unchanged.

Matched Editor diagnostics: actual connected-field Wisconsin scene, frame716,1600×900,FOV42°,pitch42°,yaw−32°,center(-166.295,8.8599,-134.294),distances55/145/450m,clock12.5s. Baseline and candidate are captured while paused at the same frame. Close water reads more clearly as a reflective surface; regular banks and repetitive nearby ground/vegetation remain unresolved. No Phase2A score is assigned.

- Control: [close](control-55.jpg), [normal](control-145.jpg), [wide](control-450.jpg).
- Candidate: [close](candidate-55.jpg), [normal](candidate-145.jpg), [wide](candidate-450.jpg).

The initial trial replay names temporary Editor-only asset paths. The final shader is `Resources/TerraWater.shader`, with an explicit runtime texture binding to `Resources/Water/SurfaceNormal.png`. [Running-Editor check](editor-check.json) confirms the shared runtime material has the real1024×1024 texture, shader compilation succeeds, no extra camera passes are requested, motion advances/freezes, and the annual diagnostic is unchanged. Actual frames advance667→716 in the study and1948 after final binding.

The corrected export passed the bounded Chrome checks below. This improves stream shading; it does not complete Phase 2A.


## Export binding correction

The first export, `b49e638`, compiles and serves31 matching files (45,006,970bytes), but [actual Chrome](chrome-importer-default-missing.png) lacks the Editor surface detail. The importer default did not establish the intended runtime texture binding; the export grew by only500bytes including its new license/HTML. Reject this export; no timing acceptance is claimed. Startup16.435s under shared load, zero error-level logs; [log](chrome-importer-default-console.json), [build](importer-default-build.json), [served files](importer-default-web-assets.json).

The correction moves the unmodified texture into Resources and explicitly binds it when constructing the runtime stream material. A single `TERRA_STREAM_SURFACE` startup diagnostic records the loaded name and width in the actual player. The shader-importer default is removed. The corrected export is verified below.


## Corrected Chrome decision — retain bounded improvement

Source `67f867e`, [successful build](build.json), 31 [byte-matched served files](web-assets.json), **45,867,155 bytes** (860,685 above the `dce9219` control). Frozen local copy: `Builds/Verified/67f867e`. Chrome1280×720 confirms the actual `SurfaceNormal`1024×1024 runtime binding and visible moving highlights. [Opening](chrome-opening.png), [normal](chrome-stream-normal.png), [close](chrome-stream-close.png), [diagnostic overview](chrome-wide.png), [matched prior surface](chrome-control-normal.png). The overview uses F6; it is a diagnostic, not the default camera. The earlier55m Editor study is also diagnostic, closer than the playable limit.

The new surface removes the conspicuous tiled-bed read and adds restrained reflection/current cues. Banks remain too regular and surrounding grass/forest remain below the whole-scene target. Retain this narrow improvement under TN-09/11/12; **no six-dimension Phase2A acceptance** is assigned.

[Browser checks](browser-checks.json): first fresh-origin arrival11.264s (one observation, not three-load gate). Actual Year1/Planning/revision0/$10M/zero proposals or assets is unchanged. Both inspected console logs contain zero error-level entries. Reduced motion freezes a water-only30×50px region exactly (0/1500 pixels differ), versus1419/1500 under full motion; [method/results](motion-comparison.json), [reducedA](chrome-reduced-a.png), [reducedB](chrome-reduced-b.png).

With our Editor closed, matched30-second camera routes yield candidate64.86FPS/p95 28ms/p99 35ms/max49ms versus control68.76FPS/p95 25ms/p99 34ms/max48ms. Both have19camera events,2preview events,0placements,0unfocused frames and0frames over200ms. Both browser receipts also confirm visible/focused DOM at start/end, no visibility changes and no blur events. Candidate preview CPU max0.2ms. These are one comparative sample per version on a shared machine; no guaranteed quiet workload, physical latency, GPU identity or final three-path performance gate is claimed. Chrome's internal GPU page was unavailable under the browser tool's HTTP/HTTPS URL policy; no workaround was attempted.

Only stream shader/material/texture and required credits change. Swale output, annual state, terrain/geometry and global quality remain unchanged. The corrected binding/clock/camera-pass [Editor check](editor-check.json) passed at actual frame456 before this export. Earlier Atlas678/HUD682 checks cover the reconciled base, not new reruns after this material-only change. Public deployment and final full-game acceptance remain pending.
