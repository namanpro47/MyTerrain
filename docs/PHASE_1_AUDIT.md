# Phase 1 acceptance audit

**Gate passed — October 24, 2026.** One selected rural AOI, one explicitly synthetic development fixture, local implementation and frozen v1 handoff. Stop before Phase 2. No packages delegated. Runtime checkpoint `a78f49cf401a6ff602676b35f23c84fb5866ae93`.

| Requirement | Observed evidence | State |
|---|---|---|
| Rural selection | [Shortlist, source previews, coverage/rights, frozen bounds/CRS, gaps](location-selection.md) | Pass |
| Main bootstrap | TerraMain URL-loads manifest/world/scenario; public scene-ready log, exact public pack hashes | Pass |
| Fixture | Synthetic slope, cover, path, water connection, three editable sites and protected woodland; both-size captures | Pass |
| Input | Actual pointer/keyboard preview/commit/cancel/undo, protected rejection, camera/drag isolation, UI isolation; browser-checks.json | Pass |
| UI | uGUI/TMP cards, tooltip, loading/error/retry, consistent palette/type, actual placement-frame capture | Pass |
| Browser review | Ten named captures at 1280×720 and 1440×900, actual interaction GIF; five internal scores 4/5 after weakest-item repair | Pass |
| Scenario | Open plot/margin area-capacity-cover-cost recipes, restore/swale, bounded model assumptions; no seasons or simulated runoff claim | Pass |
| Contracts | worldpack-format.md, runtime-contracts.md and art-direction.md cover manifest/axes/terrain/sites/scenario/commands/results/assets/content; required/optional/version rules explicit | Pass |
| Fixtures | Tiny versioned pack + three SHA256 hashes, nine invalid cases, real JsonUtility round trip, simulation I/O reference | Pass |
| Independent packages | Scene-independent 27-check runner, standalone Python simulation example, separate 24-specimen asset gallery | Pass |
| Shared baseline | Committed runtime SHA, v1 versions, complete bundle/clean checkout checks, narrow ownership in PARALLEL_WORK.md | Pass |
| Evidence and boundary | PHASE_STATUS.md records actual source/export/deployment/tests/captures/limits; no real terrain ingestion or extra level | Pass |

See [the complete evidence ledger](PHASE_STATUS.md#phase-1--acceptance-evidence-october-24-2026). Hydrology remains explicitly pending. The selected place is real; the playable fixture is invented. No novice playtest, FPS budget, physical trackpad check, or complete game is claimed by this foundation gate.

Subsequent naming-only update: **Meadowbrook Valley** is the fictional game name for this same AOI. Historical evidence preserves the original capture text; [naming policy](location-selection.md#name-and-provenance). Phase 2 remains unstarted.
