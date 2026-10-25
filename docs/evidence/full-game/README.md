# Full annual implementation — work in progress, October 24, 2026

Base: refreshed `origin/main` at `7fe7adc6f330d5b1a9b9e0b0e808b0218b7ebfac`, isolated `codex/full-implementation` worktree. The user authorized all remaining phases, direct main pushes between deliveries, and reconciliation with Peter's additions. Existing Phase 2A work stays with its active owner; no visual acceptance is inferred from these engineering results.

## Phase 4A/4B engineering slice

Peter's `AnnualTown` remains the only ledger/time/history authority. Schema 2 adds explicit nullable service contracts; the live `3.0.0` definition rejects missing capacities. Null extension fields retain legacy hashes. Original starter/demo definitions remain available for compatibility tests. The full game uses a separate `terra.meadowbrook.annual-full.v3` save key and does not reinterpret accepted starter history.

All 12 chart families and 17 variants have costs, terms, capacities, physical eligibility, metric art and operating outcomes. Full project quantities, site offers, conditions and rubric targets are in [definition.json](definition.json). Every effect quantity beyond the existing reference-storm model is an **authored educational scenario assumption**. All prices retain the supplied chart, including $3M wind/solar. Housing receipts depend on operation, occupancy and supplied resources. Private output never enters the treasury.

Economic weights: output/jobs/funded dues 40/30/30. Social: occupied households/services/undisrupted sites 40/40/20, with equal energy, drinking water, wastewater, recreation and local-service subweights. Environmental: cover/reference-storm discharge reduction/low-emissions supply/water pressure 40/25/20/15. Demand caps prevent stock-benefit stacking. Failed operation suspends benefits; due arrears count in the funding component. Year 3 and Year 7 use disclosed saved adverse conditions; Year 7 also requires a $150K public repair.

The wind option needed an additional **same-AOI hypothetical opportunity**, not a smaller turbine or another map. Its 150×150 m envelope is at local (375,-495)–(525,-345), clear of all existing sites and buffered mapped routes/water. Source-cover intersections remove exactly 22,500 m² from context; the drainage domain stays unchanged. It has a 16×16 m level foundation, distinct from full rotor clearance. [Geometry evidence](wind-opportunity.json); `python3 scripts/verify-annual-opportunity.py` reproduces the cover/overlap checks. This is not legal consent, engineering siting approval or a surveyed parcel. The source pack is unchanged; the extension is applied only to the annual runtime copy.

The three action categories use four visible cards per page. Reports show actual ledger entries and three causal summaries, with full meter details available separately. Pattern IDs: TN-03/04/05/06/08, CS-01/02/04, AX-01. UI/normal/close/late-year comparison against the eventual accepted Phase 2A benchmark is still required.

## Verification so far

- `dotnet run --project tests/full-game/Terra.FullGameChecks.csproj`: **603 assertions**. Each of 17 variants reaches an operating Year 10 result; every yearly ledger reconciles; conservation, meter bounds, save replay, missing inputs, changed rules, overspending, tax timing, saturation, maintenance, power and water-service separation checked.
- `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/annual/Terra.AnnualChecks.csproj`: **94 annual + 15 gameplay assertions**.
- `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/chapters/Terra.ChapterChecks.csproj`: **38 assertions**, including old fingerprints; no provider call.
- Unity 6000.6.6f1 compiled with zero errors/warnings and passed **34** full-catalog lifecycle/JSON replay checks across **127 eligible offers**. The follow-up call briefly disconnected during domain reload; waiting for readiness restored the connection. Existing consequences suite passed **1,372 assertions**. Browser evidence is pending. The first two detached Editor launches exited during startup; an attached headless launch is importing. The Mac is locked and computer-use automatic unlock failed; the user has been asked to unlock it for required Chrome verification.

**Not a phase-completion or release claim.** The direct visual gate, actual browser evidence, final balance/ending/accessibility/performance and release rehearsal are not yet accepted. Human novice observations remain unavailable.
