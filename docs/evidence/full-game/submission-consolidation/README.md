# Nearby vegetation submission consolidation — October 25, 2026

Candidate based on `b21dc5c` (study55 meadow + shared materials + main `7f5b292`). **Editor correctness and comparison checks pass. [Subsequent actual Chrome verification](../consolidation-browser/README.md) meets the documented stable-30 fallback and three fresh-entry load checks.** No Phase 2A acceptance or public export is claimed. The previous real Chrome paths still fail the p95 gate in [material-browser](../material-browser/README.md).

## Measured problem and bounded change

The read-only [submission replay](submission-profile.cs.txt) mirrors the existing last-frame culling and LOD decisions, and asserts its count equals `LastSubmittedBatches`. At the 1280×720 normal camera, [inventory](submission-1280.json) finds 793 submitted batches, 94,543 submesh instances and 15,324,033 submitted triangles. Woodland accounts for 492 batches and 9,941,260 triangles. 444 batches contain fewer than 32 instances. These are CPU submissions, including conservative shadow coverage, **not actual rasterized triangles or measured GPU time**. The earlier [640×480 diagnostic](submission-opening.json) is retained separately.

`NatureSubmission` combines only batches smaller than 128 instances, with identical mesh, material identity, submesh and shadow mode, in the same 96 m grid cell. Original culling and LOD selection happen first. It retains every matrix and unions the original bounds; larger and transparent batches use their existing direct path. A late ordered component flushes at most 500 instances per draw. Unity documents group culling and its default 511-instance capacity in [RenderMeshInstanced](https://docs.unity.com/en-us/engine/7000.0/script-reference/unityengine/graphics/rendermeshinstanced). No geometry, placement, shader, artistic threshold or annual rule changes.

Normal view: 594 small source batches become 451 draws, saving 143; with the 199 direct batches, **793 → 650 actual draw submissions (18.0%)**. [Counters](consolidation-normal-counts.json) also show one helper and 183 allocated pooled matrix buffers (5,856,000 payload bytes). Lazy allocation keeps single-source arrays unchanged and allocates a pooled buffer only when another batch joins. Frame cleanup releases mesh/material/source-array references; buffers are reused. The new `TERRA_RENDER_CONSOLIDATION` log reports the reduction separately from the existing source-batch counter.

Combining bounds can change native culling efficiency. Fewer API calls are not proof of faster frames. A matched Chrome export is required before promotion.

## Verification

- [15 collector/lifecycle checks](consolidation-checks.json): exact matrix ordering/scale, union bounds, negative/positive spatial separation, material/mesh/submesh/shadow isolation, capacity rollover, pooled reuse, stale frame clearing and disable cleanup. [Replay](consolidation-checks.cs.txt).
- [678 Atlas assertions](consolidation-model-checks.json), including the three existing packs, catalog, ten-year outcomes and isolated saves.
- [42 meadow lifecycle checks](combined-study55-verify.json): growth, fixed roots, reload, undo/redo, preview cancellation and unchanged live session/source; uses the [study55 fixture](../../phase2a/visual-rebuild/study55-verify.cs.txt).
- [28 material lifecycle checks](consolidation-material-lifecycle.json): shared leases, independent status colors, LOD remapping and cleanup; existing [fixture](../material-sharing/lifecycle.cs).
- Compilation succeeded after replacing an obsolete Unity API in the initial prototype. The final Play session initialized and closed gracefully; [log scan](final-session-log-check.json) has no matching runtime errors. A console-status query was not captured because its redirected path was wrong; this log scan is explicitly narrower evidence.

## Matched rendered comparisons

TN-02/09/10: preserve readable vegetation, close surface detail and canopy silhouette while reducing submissions. These are actual 1280×720 Unity camera renders with the HUD overlay absent, fixed camera and stream motion, not Chrome acceptance images. No visual quality score is raised.

| View | Combined | Separate control | Changed pixels / 921,600 |
|---|---|---|---:|
| Normal, initial collector | [image](consolidation-normal.png) | [image](separate-normal.png) | 11; maximum channel difference 3/255 |
| Close field, initial collector | [image](consolidation-close.png) | [image](separate-close.png) | 0 |
| Mature meadow, initial collector | [image](consolidation-mature.png) | [image](separate-mature.png) | 0 |
| Normal, final lazy-buffer collector | [image](consolidation-lazy-normal.png) | same separate normal control, previous Play session | 30; maximum channel difference 5/255 |

[Pixel comparison](consolidation-pixel-diff.json), [final normal comparison](final-normal-diff.json). The tiny normal-view differences are retained, not called bit equality. Close/mature pixel comparisons preceded the buffer-allocation refinement; the final version reran the collector, material, meadow and model checks and normal render. [Mature fixture](mature-fixture.json) displays one isolated ten-year restoration state through the real `DisplayAnnual` path while preserving the live Year 1 session; it is not a played browser run. At its close camera the collector saved 73 calls (340 → 267 small-batch draws).

[Replay scripts, image hashes and receipts](checkpoint.json). Public delivery, motion/placement Chrome timings and whole-scene visual acceptance remain open.
