# Placement timing acceptance protocol

Measure the actual Chrome player with F9's existing30-second recorder, once for a building and once for a whole-field treatment. Keep the tested export hash, viewport, zoom, accessibility settings, source actions, background work, and raw console receipt with each run. Reserve desktop and unity-heavy together; other active rendering sessions must be noted.

Use actual canvas pointer/keyboard commands. Collect candidate changes across eligible and blocked sites, at least one successful queue and recovery/undo, and camera movement. Do not count a run containing no preview or placement sample as passing. Do not inject state or call the recorder from page evaluation.

Read both TERRA_PERFORMANCE and TERRA_INPUT_LATENCY. Report maximum preview CPU, placement CPU, the per-input presentation-opportunity samples, frame stalls, sample counts, unfocused frames, visibility changes, blur events and discardedStale. The rAF timestamp is a browser presentation opportunity, not measured physical display latency.

An acceptance run must start/end focused and visible, remain so throughout, and contain no stale-discarded sample. The current instrumentation discards events older than500ms; therefore a nonzero discardedStale count prevents a clean latency pass rather than proving the retained subset is fast. Record interrupted runs as inconclusive and repeat after restoring focus. Never average a missed/stale event away.

Compare preview responses with100ms and placement with200ms, while separately reporting overall frame stalls. Native assertion counts and source-level stopwatch calls are not evidence that these browser budgets were met. A quiet environment is required for comparison; functional interaction checks performed during unrelated builds are not performance results.
