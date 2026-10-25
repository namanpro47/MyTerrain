# Planning performance optimization handoff

The optimization is integrated with refreshed main `a0218a9`. Naman owns the final build, foreground FPS verification and deployment; this handoff does not claim a verified 30 FPS minimum.

## Changes

- Building previews reuse the original field's vegetation and soil outside the project lot. Clipping metadata is prepared while loading; preview mesh copies are lazy. Cancel, replacement, removal preview and undo restore the original geometry.
- Unchanged previews remain alive. Land treatments show their exact field boundary during selection and generate detailed planting when queued.
- Field availability uses one detached town snapshot per update instead of repeatedly copying history for each field.
- Forecasts fork an already validated engine instead of copying and validating the whole definition and replaying accepted history after every edit. Mutable next-year state and weather drivers remain detached; public reports remain copies. The configured decision horizon is preserved.
- Town and chapter saves retain the existing compressed codec and keys, with one persistence flush per edit.

These changes preserve the responsive placement and continuous landscape patterns TN-05, TN-07 and CS-02. No graphics quality setting, tree density, project price or gameplay rule was changed by this optimization.

## Evidence and limits

The earlier native CPU stage profile measured HUD refresh at 77–98 ms. The optimized path measured 3–14 ms; scene updates fell from 8–33 ms to 4–16 ms. Saving was below 1 ms. The paired JSON files retain each event. These were headless CPU attribution runs against the same frozen assets, not FPS measurements, and precede the final current-main handoff.

The isolated Unity lifecycle suite passed 3,062 assertions. The current-main forecast parity suite passes 2,080 assertions across 416 comparisons: original replay versus optimized forecast, live-state and weather non-mutation, detached reports, cancellation, undo/redo, pending and accepted years, legacy/v5 rules, all three regions, and three-/ten-year decision horizons.

An earlier optimized frame capture lost foreground focus. Its approximately 60 FPS result is not accepted as foreground performance proof. The original focused baseline was about 54 FPS idle and 42 FPS during tool selection, with a 478 ms worst frame. A new focused native/WebGL capture is required on the final export.

The optional `--terra-selection-perf ABSOLUTE_JSON_PATH` native diagnostic exercises selection, hover, camera, housing placement/undo and Year 7 selection using disposable town/chapter keys. `--terra-placement-stages` selects the short CPU attribution path. The Editor build helper is `SelectionPerformanceChecks.BuildNative`; the lifecycle check is `SelectionPerformanceChecks.Run`.
