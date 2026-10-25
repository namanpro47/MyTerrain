# Study46 — resource and submission checkpoint

Unfinished work on `codex/visual-study20`, based on `5041113`. Latest fetched `origin/main`: `6e5e55e2a2b068beb097863328b8f2cb427d0c0b`; its two newer documentation commits have not been merged. This checkpoint does not certify Phase 2A or authorize release.

## Study45 actual Chrome result

The successful [Web export](study45-build-job.json) from source `5041113` contains 32 files totaling 55,141,799 bytes, all [verified against the local HTTP server](study45-web-assets.json). Compressed data plus Wasm alone exceed 50 MB. One fresh startup took 15,558 ms at the native 1728×940 viewport and DPR2. The subsequent foreground route was measured at 1280×720 CSS, DPR1: 42.53 FPS average, 47 ms p95, 65 ms p99, 95 ms maximum; 292 of 1,277 frames exceeded 33.34 ms. This fails the frame, size and single-load targets. It is one shared-machine observation, not a completed performance suite.

[Opening](study45-chrome-opening.png), [wide](study45-chrome-wide.png), [proposed Restore](study45-chrome-proposed.png), [undo](study45-chrome-undo.png), and [browser logs](study45-chrome-log.json) preserve the actual full-HUD browser output. Queueing reserved $95,000; clicking Undo restored the empty queue and $10,000,000 available/cash. The Z shortcut after catalog focus did not undo; do not report keyboard undo as passing. No error-level logs were returned. Mature transformation, other regions and the complete acceptance routes remain unverified.

The canopy is stronger, but broad meadow surfaces, restoration, repeated crop patterns and ground contact remain substantially below Terra Nil. Patterns TN-01/02/09/10/12 from [the design reference](../../../DESIGN_REFERENCE.md) still govern the comparison. No visual dimension is certified at 4.5/5.

## Changes saved in this checkpoint

Sixteen superseded Nature resource roots move to `Art/ArchivedNature45` with their GUIDs intact. Unity checked the dependency closure of remaining Resources roots and enabled scenes before moving them: [receipt](study46-archive.json), [script](study46-archive.cs.txt), [independent audit](study46-resource-audit.json). Historical wheat generator paths follow their moved assets. This cleanup is valid with this branch's renderer; do not apply it alone to main's older ErceTree renderer.

InstancedNature now skips off-screen submissions for tall plants as well as groundcover, with the existing conservative bounds expansion for shadows. Geometry, materials and LOD selection are unchanged. An Editor-only control restores off-screen submissions for comparison. The F9 report adds a final submission-count snapshot after frame measurements finish.

At the normal diagnostic, 864 batches were submitted and 1,302 culled, including 397 groundcover batches ([snapshot](study46-culling.json)). These are Editor counters, not a measured browser speedup. Normal and close comparisons retain the scene: [normal enabled](study46-culled-normal.png) / [control](study46-unculled-normal.png), [close enabled](study46-culled-close.png) / [control](study46-unculled-close.png). [Pixel differences](study46-pixel-comparison.json) affect 418 and 366 of 921,600 pixels respectively, with maximum channel difference 6/255; they are nearly identical, not byte-identical. Close capture was inspected. This does not cover all camera angles or prove every off-screen shadow case.

Compilation passed before these captures; the current Play session advanced to frame 19,831. No texture-resolution or PlayerSettings change was applied. A new Web export, measured package savings, wide/moving shadow checks and fresh Chrome performance remain pending. Keep the visual goal open; performance work cannot compensate for weak scenery.
