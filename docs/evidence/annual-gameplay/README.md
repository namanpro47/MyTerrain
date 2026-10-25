# Annual gameplay integration evidence — October 24, 2026

Branch `codex/annual-gameplay-integration`, isolated from the active visual Editor. Imported `main` through `e247431` and editing/runoff `5e9621b`. Phase 2A remains unaccepted. Patterns: TN-03 compact HUD, TN-04 matching action art, TN-05 anticipatory preview, TN-06 progressive detail, TN-08 persistent results, CS-01 three stable categories, CS-02 map state, CS-03 queue/object separation, AX-01 browser measurements.

## Verified rules and source integration

- Unity 6000.6.6f1 recompiles with zero errors in the Editor.
- Peter's plain-source annual suite: 94 assertions. Additional pure queue/forecast/service suite: 15 assertions. Run with `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/annual/Terra.AnnualChecks.csproj` on this machine (installed runtime is .NET 10; project targets .NET 8).
- Real-map Unity annual integration: 84 assertions. All seven recipes have actual fitting parcels. Confirms exact source costs, unchanged cash while queued, homes Building in Year 1, $20K receipts and four occupied places in Year 2, rotation maturity in Year 3, independent housing services during wet-year crop sensitivity, retry idempotency, Unity JSON restore, no undo of accepted years, and completion at Year 10 using the same transitions. Also checks annual fixed-storm volume conservation, baseline equivalence (9701.926 m³ discharge), and storage only after swale commissioning.
- Current-source geography/editor regression suites: 27 Phase 1, 139 Phase 2, 1758 historical editing, 98 Unity runoff checks. Independent .NET runoff integration: 1372 checks.
- [Three-year accepted run](three-year-run.json) contains ledger, frozen input, accepted consequences and outcomes. [Frozen recipes](recipes.json) contains exact commitments, bundles, targets, drivers and assumptions.

## Browser / visual verification

Two successful WebGL exports from the isolated Unity 6000.6.6f1 Editor; final browser checks used the second export. The initial Mac-target compilation issue was resolved by opening the Editor on the installed WebGL target. No extra input package was installed. `TerraBuild.BuildWeb` now rejects the wrong active target with an actionable message.

Chrome exercised the final export at 1280×720 and 1440×900, with an additional default-window pass. The stable visual checkpoint `3684a40` is merged; study20 remains the visual owner's separate experiment. TN-03/05/06/08 and CS-01/02/03 are visible in the compact meters, forecasts, project inspector, proposal editing, and persistent lifecycle labels. This verifies annual integration against the stable scene, not Phase 2A visual acceptance or performance.

Verified through actual controls:

- Queue home, undo/redo, move to Central field and undo the move; then queue park and crop rotation. Cash remains $10M, total reserve $1,362,000. Reload retains all three proposals and reservations.
- Year 1 settles three projects: cash $9,786,800, two building and one establishing. [Submission](year1-confirm-1280.jpg) · [accepted report](year1-report-1280.jpg).
- Year 2: prior accepted assets survive Undo. Invalid swale placement is rejected with its authored location reason. A swale queues at Meadow edge; home and park become operational, housing receives $20,000, and cash closes at $10,691,600. [Report](year2-report-1280.jpg).
- Year 3 uses Wait with no new actions. Wet-year crop/food factor is 0.8; home and recreation services remain unaffected. All four projects operate; cash closes at $11,693,400 with $145,400 future obligations. Meters: Economic 47.8, Social 78.0, Environmental 37.1; operational storage 8 m³. [1280 report](year3-report-1280.jpg) · [1440 report](year3-report-1440.jpg) · [operating home](year3-operating-1440.jpg).
- Reload restores Year 3 Reviewing, cash, commitments, meters, and accepted projects. A prior build with a different rules hash was rejected with a visible recovery-copy notice.
- Three F10 layout audits have no measured text-height overflow: [confirmation 1280](ui-confirm-1280.json), [report 1280](ui-report-1280.json), [operating inspector 1440](ui-operating-1440.json). Larger-text accessibility and performance were not re-certified in this slice.
- [Browser logs](browser-logs.json): no captured error-level events. Known warnings remain for the stripped FSR/post-processing shader and Unity's deprecated manual persistent-data synchronization. Neither blocked saving or the three-year route.

[Export file identity](web-build.json) records hashes of the tested output. The browser route deliberately added the swale in Year 2; the separate deterministic Unity fixture queues all four in Year 1, so its ledger differs by design. The newest main chapter/narration delivery (`a82c74f`, main `2387393`) was discovered during final verification and is already integrated by the primary merge owner; it is not included in this tested export. Hand off this branch for combination with that additive work, retaining the annual engine as the sole authority.

## Deliberate limits

Seven starter recipes are enabled. The source profiles preserve all twelve chart rows, but additional buildings, resource/pollution effects, demand balancing and the final paired ending remain future phases. Upkeep, outputs, service capacities, weather and meter targets are explicit authored game assumptions. Fixed-storm discharge is a model comparison, not flood risk. Existing accepted assets cannot be moved, demolished or refunded in this slice. Save versions/rule hashes reject incompatible data while retaining a recovery copy. No public deployment or changes to the other worktrees' Editors.
