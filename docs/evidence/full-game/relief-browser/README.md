# Integrated terrain Chrome diagnostic — October 25, 2026

Source `987d4407769046db59f8966b0c2990b1f82bb5ea`, isolated `codex/relief-validation`: selective cached display relief from `a71bcc4`, shared overlay vertices, swale query optimization and main through `fff6eae`. This is an **unaccepted visual candidate**, not a release or Phase 2A pass. No public deployment occurred.

## Export identity and delivery

Unity 6000.6.6f1 / CLI 1.0.0-beta.14 clean-source WebGL export succeeded; `build.json` and `provenance.json` retain the receipt. Frozen local folder `Builds/Verified/987d440`; `identity.json` hashes all 31 runtime files, totaling **53,101,242 bytes**. `web-assets.json` verifies every served file against those bytes.

The actual Wisconsin arrival requested 20 unique response bodies, **52,557,275 bytes**, exceeding the 50,000,000-byte initial budget. `initial-wisconsin-bytes.json` lists them. This sum excludes other places' streaming packs, unopened credit/license files and transport overhead; gzip Unity bodies and uncompressed local-server atlas/JSON bodies are counted as served. It is not an estimate from uncompressed Unity asset sizes. One fresh-origin arrival reported **12,421 ms** to interactive; the required three-arrival gate remains open.

## Actual Chrome interaction

1280×720, DPR 1, normal text, full motion. Candidate and control ran sequentially in separate local origins; candidate closed before control. The visual owner's Editor was paused. No error-level console entries were captured. Browser visibility telemetry for every sample began/ended focused and visible, with no visibility changes or blur events; Unity recorded zero unfocused frames.

| Sample | Seconds | Frames | Average FPS | p50 / p95 / p99 ms | Max ms | >33.34 / >200 ms |
|---|---:|---:|---:|---:|---:|---:|
| Candidate camera | 30.017 | 1280 | 42.64 | 21 / 39 / 53 | 71 | 141 / 0 |
| Candidate editing | 30.007 | 1078 | 35.92 | 25 / 41 / 59 | 69 | 132 / 0 |
| Control `67f867e` camera | 30.004 | 1797 | 59.89 | 14 / 30 / 38 | 56 | 43 / 0 |

Camera inputs: R reset, F9 capture, repeated Right/Down/Left/Up held 1.2 seconds each, in two three-cycle batches with an observation between. Tool/compaction gaps differ, so these are diagnostic samples, not a precisely timed benchmark. Candidate/control recorded 907/898 camera events. The control initially loaded at native viewport before resizing to 1280×720; its startup time is not a matched arrival sample. **Candidate camera and editing fail stable-30 p95 ≤33.3 ms.** This does not establish a single responsible renderer change, and neither sample replaces the required three final routes.

Editing: select swale, cycle from West field (correctly rejected without a runoff connection) to Meadow edge (`south-07`), focus, queue, open catalog, undo, reopen catalog and redo. Unity recorded 10 preview events (max CPU 9 ms) and one placement (22.3 ms CPU). DOM-input-to-next-rAF feedback: eight recorded previews, maximum **43.8 ms**, one placement **43.4 ms**. These bounded responsiveness checks pass ≤100 ms preview / no >200 ms placement stall. They measure presentation opportunities, not physical display latency.

Actual catalog keyboard and pointer regression checks passed (TN-04/05, AX-01):

- Queue revision 1; B→Z restored queue zero at revision 2; B→Y restored queue one at revision 3.
- Z returned queue zero at revision 4. Selecting swale, B, then a pointer proposal on Meadow edge created revision 5; Z/Y produced revisions 6/7 and queue 0/1. The world click clears catalog keyboard focus.
- Cash remained $10,000,000 with no accepted assets throughout; the queued swale reserved $130,000. These are edits to the current plan, not committed-year refunds.

`chrome-catalog-undo.png`, `chrome-catalog-redo.png` and `chrome-pointer-redo.png` show the results. Console JSON preserves state and input telemetry.

## Visual and acceptance limits

`chrome-opening.png` and `chrome-control-opening.png` preserve the same viewport comparison. Against TN-01/02/06, selective relief remains an experiment: groundcover/crop cohesion and transformed swale appearance are not accepted. No new visual rating is certified by this performance test. Final 1440/1920/125%-zoom, moving/close/transformed reference comparisons, three-load/three-route gates, public full ten-year run, two rehearsals and a recorded fallback remain open.
