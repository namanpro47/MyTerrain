# Annual gameplay integration evidence — October 24, 2026

Branch `codex/annual-gameplay-integration`, isolated from the active visual Editor. Imported `main` through `e247431` and editing/runoff `5e9621b`. Phase 2A remains unaccepted. Patterns: TN-03 compact HUD, TN-04 matching action art, TN-05 anticipatory preview, TN-06 progressive detail, TN-08 persistent results, CS-01 three stable categories, CS-02 map state, CS-03 queue/object separation, AX-01 browser measurements.

## Verified rules and source integration

- Unity 6000.6.6f1 recompiles with zero errors in the Editor.
- Peter's plain-source annual suite: 94 assertions. Additional pure queue/forecast/service suite: 11 assertions. Run with `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/annual/Terra.AnnualChecks.csproj` on this machine (installed runtime is .NET 10; project targets .NET 8).
- Real-map Unity annual integration: 84 assertions. All seven recipes have actual fitting parcels. Confirms exact source costs, unchanged cash while queued, homes Building in Year 1, $20K receipts and four occupied places in Year 2, rotation maturity in Year 3, independent housing services during wet-year crop sensitivity, retry idempotency, Unity JSON restore, no undo of accepted years, and completion at Year 10 using the same transitions. Also checks annual fixed-storm volume conservation, baseline equivalence (9701.926 m³ discharge), and storage only after swale commissioning.
- Current-source geography/editor regression suites: 27 Phase 1, 139 Phase 2, 1758 historical editing, 98 Unity runoff checks. Independent .NET runoff integration: 1372 checks.
- [Three-year accepted run](three-year-run.json) contains ledger, frozen input, accepted consequences and outcomes. [Frozen recipes](recipes.json) contains exact commitments, bundles, targets, drivers and assumptions.

## Browser / visual verification

Pending first export. The initial export encountered a WebGL-specific compilation failure from a new Editor on the Mac target; the explicit WebGL target switch is being verified. Attempt to add a presumed input package failed without changing the manifest. The correct WebGL module is supplied by the installed WebGL build target ([Unity API](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/WebGLInput.html)). No successful export or browser acceptance is claimed here yet.

The visual owner has designated `3684a40` as the stable checkpoint. Study20 remains their separate experiment. Integration must preserve this separation; do not equate the annual-loop smoke test with visual acceptance or a performance release gate.

## Deliberate limits

Seven starter recipes are enabled. The source profiles preserve all twelve chart rows, but additional buildings, resource/pollution effects, demand balancing and the final paired ending remain future phases. Upkeep, outputs, service capacities, weather and meter targets are explicit authored game assumptions. Fixed-storm discharge is a model comparison, not flood risk. Existing accepted assets cannot be moved, demolished or refunded in this slice. Save versions/rule hashes reject incompatible data while retaining a recovery copy. No public deployment or changes to the other worktrees' Editors.
