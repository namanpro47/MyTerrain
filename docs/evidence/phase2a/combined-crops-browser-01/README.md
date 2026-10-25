# Combined crops — actual Chrome regression and annual checks

October 25, 2026. Frozen owner `Builds/Web64`, reported source `e3f4905`, combined on the verification branch as `47ae061`. Latest fetched main was `913c79f`. The owner records six unrelated tracked Unity changes and unused imported inputs during export: this is **not a clean Git-only build**. All 31 runtime files were copied only after matching the owner receipt, then anonymously served and byte-verified again: **48,126,445 bytes**, including all three location packs. This is a complete inventory, not initial network transfer. [Owner receipt](owner-verified.json), [local receipt](verified.json).

Actual Chrome 155.0.8059.85, 1280×720 CSS/canvas, pixel ratio 1, Wisconsin. The owner closed its Editor/game tab for this window; the verification Editor was also closed. Browser actions use normal keyboard/click input. No hidden state injection or model fixture substitutes for play. TN-01/02/05/08/09/10 remain the design references. Scenery acceptance remains open; the earlier conifer-only 30ms result does not transfer to these crops.

| 30-second sample | p50 / p95 / p99 ms | Camera events | Frames >33.34 / >200ms | Result |
|---|---|---:|---:|---|
| Opening camera | 15 / 37 / 44 | 1,031 | 152 / 0 | Fails stable-30 p95 limit |
| Static editing | 15 / 33 / 40 | 0 | 78 / 0 | Bounded static result only |
| Moving editing | 23 / 41 / 59 | 760 | 354 / 0 | Fails stable-30 p95 limit |
| Year 10 camera | 17 / 34 / 48 | 724 | 85 / 0 | Fails stable-30 p95 limit |

All samples report zero unfocused frames; DOM receipts show visible/focused start and end, no visibility changes or blur. The requested camera path was repeated Right/Down/Left/Up holds of 1.2 seconds, split into tool batches. Tool/observation gaps differ; the Year 10 second batch occurred after the timed window during context recovery. Thus Year 10 is a bounded partial camera route, not a continuous matched traversal. The first editing window ended before movement; it is retained, not silently replaced. These sequential samples establish a failure to meet the gate, not a controlled causal attribution to one shader or mesh.

Static editing: maximum preview CPU 10.1ms, placement CPU 26ms; DOM input→first animation-frame after feedback maxima 26ms preview /40.9ms placement. Moving editing: CPU 6.3/18.1ms, DOM 24.9/31.2ms. DOM timing measures a presentation opportunity, not physical display latency. Both bounded input paths meet their limits. [Parsed metrics](summary.json) and timestamped cumulative raw logs preserve all observations.

## Annual route, comparison and replay

A living margin was queued on West field (`south-01`), undone and redone. Restoration was queued on Central field (`south-02`), undone and requeued for the moving sample. Two projects reserve $245,000 while stored cash remains $10M. The actual Future confirmation then submits the queue and fast-forwards the remaining years; this is **not a manually played ten-year strategy**. Offline annual processing reports 32.8–55.3ms per year and 821.2ms for the complete fast-forward including yielding and final world refresh.

[Year 10 ending](ending.jpg): Economic 51.0, Social 58.0, Environmental 47.5; no-new-action 42.3/58.0/38.9. Closing cash **$19,605,000**, public spending $395,000, appropriations $10M, receipts/remaining obligations/arrears zero. Zero of three targets met. Completed revision 25, two accepted assets and zero proposals.

The actual [opening comparison](comparison.jpg) leaves that completed snapshot intact. Reload returns to the atlas; clicking Explore [resumes the same ending](reloaded.jpg), cash and revision. Confirming New run [restores Year 1](replay.jpg), Planning/revision 0/$10M/zero proposals/zero accepted assets. The new-run introduction is visible.

No error-level console entries. The optional unsupported FSR shader warning occurs once per load, as in prior exports. The future/new-run memory checkpoints are retained in raw logs, but this single reload/replay is not a repeated-run leak test; the managed-memory readings are not used to claim a heap-size bound. No new three-arrival benchmark, public deployment, rehearsal or final fallback recording is claimed. Tab closed, viewport reset and local server stopped; Chrome returned to the visual owner. The next work is to identify the combined rendering cost while preserving accepted gameplay and intended crop appearance.
