# Crop source grouping — frozen browser comparison

October 25, 2026. **Editing performance still fails.** This directory records the frozen `d4ae4f2` export and its study64 control in actual Chrome. The [Editor audit](../crop-spatial-01/README.md) verifies unchanged plant/soil content with tighter 32m source groups. It is not browser performance or Phase 2A acceptance.

## Export identity

Candidate source `d4ae4f2d145393b40773b4eb45da5b7388ede574` was clean before export. Unity 6000.6.6f1 / CLI 1.0.0-beta.14 completed job `005d3c6cfac144f096877924e232697d` successfully in 511,129 ms. The frozen local folder is `Builds/Verified/d4ae4f2`, served on port 61295. Its 31 runtime files total **48,092,402 bytes**; the hosting configuration is excluded. All local HTTP responses matched the export, Brotli payloads decoded and WASM magic passed. [Build result](build-result.json), [source inputs](build-source.json), [exact files](export-files.json), [HTTP verification](verified.json).

The export generated a TMP fallback-font change and removed two orphan DOTween debug metadata files; these three tracked changes were recorded and restored only after closing the owned Editor. [Generated diff](generated-input.diff), [font identity](generated-font.json), [post-build status](post-build-status.txt). All 28 pre-existing ignored pack/metadata inputs remained unchanged. This is a recorded source-plus-import export, not a claim of binary reproducibility from Git alone.

Control is the separately frozen owner study64 export, source reported `e3f4905` and integrated as `47ae061`, at `Builds/Verified/study64-combined`, port 61294. It has **31 files / 48,126,445 bytes**, with owner tracked/import caveats retained in the [prior receipt](../combined-crops-browser-01/README.md). Twenty-seven relative runtime files byte-match the candidate; index and hashed build output differ. The comparison is sequential and bounded, not strict proof that grouping alone caused any timing difference. [Control hashes](control-export-files.json), [comparison provenance](comparison-provenance.json).

## Actual Chrome observation

Chrome 155.0.8059.85, 1280×720, DPR1. Shared reservation 4 held desktop + unity-heavy; owned Editor was closed and other owners asked to stop Play Mode. Other idle Editor processes remained; [safe process inventory](environment.json) records their presence. The control tab was closed before candidate launch. Each route starts with R and repeats Right/Down/Left/Up held for 1.2 seconds, six cycles. Holds run in one uninterrupted tool batch, occupying about 29 seconds of each 30-second sample. The editing route first undoes and requeues Central restoration, then follows the same movement sequence. Both tabs were closed and viewport override reset before releasing the reservation.

| Route | p50 / p95 / p99 ms | Maximum ms | Frames >33.34 ms | Camera events | Unfocused frames |
|---|---:|---:|---:|---:|---:|
| Control opening | 17 / 18 / 18 | 27 | 0 / 1800 | 1734 | 0 |
| Candidate opening | 17 / 22 / 30 | 35 | 1 / 1779 | 1718 | 0 |
| Candidate editing | 35 / 39 / 40 | 45 | 586 / 1007 | 972 | 0 |
| Candidate Year10 | 17 / 26 / 32 | 36 | 13 / 1744 | 1684 | 0 |

[Parsed results](summary.json) and full raw logs preserve all samples. All four began/ended visible and focused, with no visibility changes or blur events. There are no >200 ms frames. Editing records five preview CPU events (maximum 6.3 ms), one placement (17.2 ms), three matched DOM preview feedback opportunities (maximum 37 ms) and one placement feedback opportunity (35.2 ms). This small input sample passes the bounded input thresholds; it is not physical display latency.

**Do not infer a browser speedup.** The control opening is faster than the candidate in this sequential observation. The 32m groups reduce selected geometry in the Editor, but have not demonstrated faster browser frames. Candidate editing p95 39 ms exceeds 33.3 ms, so the three-path performance gate fails. The previously recorded Web64 37/41/34 ms paths remain valid historical observations; this quieter opening at 18 ms shows substantial path/environment variability. Do not replace earlier failures or average them into a pass.

End-of-window actual render counters report total nature draws **497 control opening; 454 candidate opening; 466 candidate editing; 470 candidate Year10** (submitted source batches minus collector sources plus collector draws). These are one-frame counters at each route endpoint, not route-wide maxima or GPU timings. They show no large editing-only draw-count increase. Per-component annual tint copies remain a possible small cost, but this evidence does not justify rewriting material ownership. No production material change was made; a proposed Editor diagnostic was cancelled before launch. A subsequent matched editing route is recorded below.

Fresh-player Explore-to-visible-focused-game observations were 18.653 s control and 15.675 s candidate. Both exceed 15 s, and these are one observation each, not the required three fresh arrivals. Initial transfer remains below 50 MB. Final arrival acceptance is open.

## Functional and appearance checks

Actual keyboard input queues a West-field living margin, undoes/redoes it, and queues Central-field restoration. Available cash is $9,755,000 while actual cash remains $10,000,000 and the plan reserves $245,000. Editing measurement undoes/requeues restoration. The confirmed ten-year future accepts the same two projects, reaches Completed/revision25 with zero queued proposals, and closes at **$19,605,000**. Economic/Social/Environmental are **51.0 / 58.0 / 47.5**, against no-new-action **42.3 / 58.0 / 38.9**. Public spending is $395,000, appropriations $10M, receipts and beyond-horizon obligations/arrears zero. Each offline year took 30.8–50.3 ms; the complete fast-forward including yielding/final world refresh took 759.5 ms. [Ending capture](candidate-ending.jpg), [raw state and timing](candidate-final.json).

[Control opening](control-opening.jpg), [candidate opening](candidate-opening.jpg), [queued view](candidate-queued.jpg), and [closer Year10 view](candidate-close.jpg) were directly inspected. TN-01/02/09/10: field row rhythm, leaf plants and raised soil remain present across normal and close views. The existing repetitive rows, sparse open terrain and overall scenery gap remain; this is not visual acceptance. Camera motion was exercised, but endpoint captures do not establish an absence of brief LOD popping. No motion recording or complete visual transition review is claimed.

Zero error-level browser logs; the existing optional FSR shader warning appears once per load. One future memory snapshot is preserved (185,603,860 allocated / 217,120,768 reserved / 140,763,136 managed bytes, 520 meshes and 136 materials); this is not repeated-run memory validation. Reload/replay were covered on the preceding Web64 source but not repeated on this bounded grouping check. Final combined visual, performance, arrival, public-run, rehearsal and recording gates remain open. Candidate runtime/art is not promoted to main by this evidence.

## Matched editing follow-up

Reservation18 held desktop + unity-heavy on October25, 16:36–16:39UTC. Fresh local origins61296 (same frozen control) and61297 (same frozen candidate) prevented old completed saves from entering the comparison. Own Editor was closed; native-atlas owner also closed its Editor, visual owner kept Play stopped. Two other primary Editor processes remained at the start (PIDs29573 and74462, observed8.3%/1.8% CPU); the reservation does not isolate all OS activity. No additional build or source changes preceded these samples. Each actual fresh run queued West living margin and Central restoration, exercised undo/redo, then measured the identical editing/movement sequence described above. Control closed before candidate opened. Tabs closed, viewport reset and both servers stopped before release.

| Matched editing | p50 / p95 / p99 ms | Maximum ms | Frames >33.34 ms | Camera events | Unfocused frames | Endpoint nature draws |
|---|---:|---:|---:|---:|---:|---:|
| Control | 16 / 34 / 39 | 48 | 80 / 1573 | 1531 | 0 | 512 |
| Candidate | 15 / 36 / 38 | 55 | 121 / 1670 | 1626 | 0 | 473 |

[Parsed measurements](editing-comparison.json), [control raw logs](editing-control-logs.json), [candidate raw logs](editing-candidate-logs.json), [control endpoint](editing-control.jpg), [candidate endpoint](editing-candidate.jpg). Both preserve Year1 Planning/revision6, $10M cash and two queued projects; the visible plan reserves $245,000. Both contain five preview events and one placement, with maximum CPU times6.1/17.4ms control and6.2/17.0ms candidate. Neither has a >200ms frame or error-level log. Unity reports zero unfocused frames; this follow-up did not separately save DOM focus-transition counters. The existing optional shader warnings and unloaded-audio metadata notices remain in raw logs.

**Both editing routes fail the33.3ms target.** The candidate reduces endpoint nature draws512→473 but gives no measured p95 benefit34→36ms. This sequential pair neither establishes a regression caused solely by grouping nor supports promoting it as a browser speedup. Retain all earlier failures, including39ms candidate editing; do not average or select runs into acceptance. Fresh-entry values in the raw records are additional observations, not a completed three-arrival gate. The next rendering change must address the actual accepted scene and be measured again; speculative annual tint material changes remain unjustified.
