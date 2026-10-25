# Study34 — clustered foliage and grounded woodland candidate

Patterns TN-02/09/10/12. The [reference](../../../DESIGN_REFERENCE.md) and [visual goal](../../../PHASE_2A_GOAL.md) remain authoritative. Phase 2A is open; this study is not a parity claim.

## Different construction, same place

A [properly framed creator gallery](study34-creator-framed.png) now confirms the original branching silhouette and also its fine leaf noise in this project. Unlike the incorrectly framed study33 image, it shows both trees. Native material/light settings are retained; only the camera moves. Gallery frame count advanced 4584→4662 during the verification wait.

Instead of shading every card independently or smoothing one complete crown, this candidate bakes fourteen foliage clusters per leaf mesh, with cluster-radial normals plus a small crown/up contribution. Original artist geometry, UVs and leaf texture stay intact. Alpha-to-coverage retains leaf edges. [Gallery result](study34-cluster-gallery.png) uses the creator's intensity-4 sun and is overbright; it is a construction comparison, not Terra's selected lighting.

[Actual landscape trial](study34-cluster-world.png) → [palette/shadow control](study34-cluster-shadow.png) → [grounded forest floor](study34-woodland-floor-settled.png) → [close view](study34-close.png). Floor shading uses the existing painted earth/grass texture family and a mask derived from illustrative tree positions. It neither changes source cover nor claims observed litter or stem locations. The first floor capture is invalid because the new ground shader was still compiling; only the settled capture informs the review.

## Full-scene implementation

[Full-location opening](study34-full-opening.png), camera-only Editor output before the final bark import limits and incoming project-ground reconciliation. Tree masses and grounded roots improve on the thin Erce baseline. Repeated rounded crowns, broad empty ground, uniform crop pattern, coarse banks and weak understory remain conspicuous. This does not reach the six required scores.

- Replaces the existing illustrative woodland family with the free TriForge oak/sapling across the same source woodland. Native tree origin stays at soil level, leaving the creator's lower roots buried. Mature sizes remain roughly 12–22m; saplings roughly 2–4m. These are illustrations, not surveyed species/heights.
- The bake script splits parts and aligns their material slots across three LODs. Generic instancing submits only the selected level, rather than all prefab children. The first export reused medium bark at distance; the checkpoint correction below now uses the creator's far atlas and mesh.
- [LOD checks](study34-lod-check.json): oak 18,324 / 11,566 / 9,234 triangles; sapling 2,046 at each level. No slot/count mismatch. The duplicate-density sapling levels reflect the source and do not establish an optimization win.
- [Curated dependency manifest](study34-runtime-assets.json) and `Assets/TriForge Assets/TERRA_PROVENANCE.md` retain license/source/adaptation details. Original source pixels are unchanged. Import limits are 2048 leaf / 1024 bark, normal and mask. Full gallery and unused source files remain local.
- Main's project-ground adaptation through `763ef9b` is merged in `81f61e5`; both its material cleanup and the forest mask cleanup are retained. No annual state or ledger change.

Editor game initialization: Year1 / Planning / revision0 / cash1000000000 / queued0 / assets0; frame count 26→31 on the full implementation. No cluster shader errors reported. The temporary 380m/4096 shadow-pipeline experiment was removed; the candidate retains the project's original Mobile pipeline. The first Web export and Chrome review are recorded below; the corrected source still needs a fresh export.

Replay sources: [cluster diagnostic](study34-cluster-trial.cs.txt), [floor diagnostic](study34-floor-trial.cs.txt), and `scripts/art/bake-cluster-forest.cs.txt` (copy to a .cs file for Pipeline eval_file). [Editor capture hashes](study34-editor-captures.json). These diagnostics do not replace the final runtime source or browser evidence.

## Renewed reference comparison

Inspected the official [restored-bank frame](https://freelives.net/uploads/2024/04/Terra-Nil-Screen-3.webp) and [tropical canopy frame](https://freelives.net/uploads/2024/04/Terra-Nil-Screen-2.webp) directly in Chrome during this study. The latter is not a Wisconsin species target. The transferable differences are larger, clearer foliage masses with cool shaded interiors; dense low/medium/tall transitions; and strong water/earth/plant boundaries. Our candidate is still too uniform, with fine noisy crowns and broad weakly articulated surfaces. The next substantial work should address the stream-bank/field/woodland transition as a complete composition, not continue tuning tree tint alone. No numerical score has been promoted to acceptance.

## First Web export — regression found, not accepted

Source `81f61e5` plus generated local TMP fallback state; build job `11fb89550a8b4e7996c00b924e940a44` succeeded in 398.876s. [Served-byte manifest](study34-served-assets.json): 51,195,367 bytes, exceeding the decimal 50 MB target. Chrome used 1280×720 CSS and canvas resolution at local port 59134. [Opening](study34-chrome-opening.png), [close](study34-chrome-close.png), [queued project](study34-chrome-queued.png), [overview](study34-chrome-overview.png), [browser log](study34-browser-log.json).

The distant canopy loses coverage and exposes conspicuous brown forest-floor marks. This is a regression to resolve before main integration. Queue/undo returned Year 1 to zero queued projects with unchanged cash; no year was submitted. One 30.013s foreground route: 46.846 fps average, p50 17ms, p95 35ms, p99 43ms, max 70ms; 121 of 1,406 frames above 33ms, none above 200ms, zero unfocused frames. A concurrent build may have affected timing, so this is diagnostic evidence only. No error-level browser logs were returned.

## Checkpoint correction — Editor verified, browser pending

[Editor readback](study34b-editor-check.json) confirms oak LOD triangles 18,324 / 11,566 / 4,207. Far bark now uses its matching original atlas; the duplicate far bark part is empty and skipped. Instancing chooses the corresponding material per LOD, with earlier level transitions. Leaf mip generation preserves alpha coverage at the shader's 0.4 threshold. Bark textures retain 1024 resolution and use Crunch quality 80; the leaf atlas remains 2048 without Crunch. [Updated dependencies and hashes](study34b-runtime-assets.json).

Compilation is not failed and the Editor is idle with zero current console errors. This does not establish visual, download-size or performance acceptance for the corrected source. No broad quality settings changed. The original first-export manifests remain historical. The next export must verify opening, close and overview canopy, exact transferred bytes, and foreground frame timing before promoting this candidate.

Integration note: AnnualStage currently tints child Renderers, while InstancedNature submits graphics batches directly. Annual project foliage therefore does not inherit that lifecycle tint. Labels/footprints still distinguish stages; exposing lifecycle tint to instanced vegetation remains a separate integration gap, not a claim this forest checkpoint resolves.

## Follow-up export and next construction test

Remote main `f4f148d` was fetched and merged cleanly in `f88b6f6`, retaining the graded annual project edges. The corrected export runs from this merged source. No input assets or runtime source are changed during export.

The current groundcover branch contains an unreachable woodland path: woodland requires grouping <0.4 to enter, then >0.44 to place plants. A representative understory trial should repair that condition and compare the native TriForge forest plant material with the existing scenery, using source cover and road/stream exclusions. Native plant size and footprint must be inspected before placement. This is intended to establish the missing lower vegetation layer, not merely scatter extra objects. No result is claimed until rendered.

## Corrected Chrome export: study34b

The [exact export record](study34b-build.json) and [served bytes](study34b-served-assets.json) identify merged source `f88b6f6`; export succeeded in 247.495s. Nine anonymous files match at **47,811,775 bytes**, under 50 MB. Original first-export data remain intact.

Chrome at 1280×720 CSS/canvas: [opening](study34b-chrome-opening.png), [close](study34b-chrome-close.png), [overview](study34b-chrome-overview.png), [queued restoration](study34b-chrome-queued.png). Distant leaf coverage is visibly recovered, but the overview still reads as small repeated crowns over conspicuous floor footprints. It remains below the visual gate. Close crowns repeat and lower vegetation is weak.

One 30.012s foreground route: **59.94 fps average, p50 17ms, p95/p99 19ms, max 36ms**, 1 of 1,799 frames above 33ms, zero above 200ms and zero unfocused frames. Five previews (max CPU 5.5ms), one placement (20.6ms max CPU), 484 camera events. This route passes the stable-30 fallback; it is not the full three-path acceptance suite and CPU timings are not presentation latency. No active build was running during this route. No error-level browser logs returned. Queue/undo: Year 1 Planning, revision 3→4, queued 1→0, cash 1,000,000,000 cents unchanged, no assets, no year submitted. [Browser log](study34b-browser-log.json).

The browser tab was confirmed closed after capture; closing produced a session-detach notification after the evidence file had been saved. Subsequent scene/material experiments are Editor-only [study35](study35-findings.md), not this export. Later main lifecycle changes are not part of these Chrome results.
