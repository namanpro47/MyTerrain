# Full annual implementation — work in progress, October 24, 2026

Base: refreshed `origin/main` at `7fe7adc6f330d5b1a9b9e0b0e808b0218b7ebfac`, isolated `codex/full-implementation` worktree. The user authorized all remaining phases, direct main pushes between deliveries, and reconciliation with Peter's additions. Existing Phase 2A work stays with its active owner; no visual acceptance is inferred from these engineering results.

## Phase 4A/4B engineering slice

Peter's `AnnualTown` remains the only ledger/time/history authority. Schema 2 adds explicit nullable service contracts; the live `3.1.0` definition rejects missing capacities. Null extension fields retain legacy hashes. Original starter/demo definitions remain available for compatibility tests. The full game uses a separate `terra.meadowbrook.annual-full.v3.1` save key and does not reinterpret accepted starter history.

All 12 chart families and 17 variants have costs, terms, capacities, physical eligibility, metric art and operating outcomes. Full project quantities, site offers, conditions and rubric targets are in [definition.json](definition.json). Every effect quantity beyond the existing reference-storm model is an **authored educational scenario assumption**. All prices retain the supplied chart, including $3M wind/solar. Housing receipts depend on operation, occupancy and supplied resources. Private output never enters the treasury.

Economic weights: output/jobs/funded dues 40/30/30. Social: occupied households/services/undisrupted sites 40/40/20, with equal energy, drinking water, wastewater, recreation and local-service subweights. Environmental: cover/reference-storm discharge reduction/low-emissions supply/water pressure 40/25/20/15. Demand caps prevent stock-benefit stacking. Failed operation suspends benefits; due arrears count in the funding component. Year 3 and Year 7 use disclosed saved adverse conditions; Year 7 also requires a $150K public repair.

The wind option needed an additional **same-AOI hypothetical opportunity**, not a smaller turbine or another map. Its 150×150 m envelope is at local (375,-495)–(525,-345), clear of all existing sites and buffered mapped routes/water. Source-cover intersections remove exactly 22,500 m² from context; the drainage domain stays unchanged. It has a 16×16 m level foundation, distinct from full rotor clearance. [Geometry evidence](wind-opportunity.json); `python3 scripts/verify-annual-opportunity.py` reproduces the cover/overlap checks. This is not legal consent, engineering siting approval or a surveyed parcel. The source pack is unchanged; the extension is applied only to the annual runtime copy.

The three action categories use four visible cards per page. Reports show actual ledger entries and three causal summaries, with full meter details available separately. Pattern IDs: TN-03/04/05/06/08, CS-01/02/04, AX-01. UI/normal/close/late-year comparison against the eventual accepted Phase 2A benchmark is still required.

## Verification so far

- `dotnet run --project tests/full-game/Terra.FullGameChecks.csproj`: **608 assertions**. Each of 17 variants reaches an operating Year 10 result; every yearly ledger reconciles; conservation, meter bounds, save replay, missing inputs, changed rules, overspending, tax timing, saturation, maintenance, power and water-service separation checked.
- `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/annual/Terra.AnnualChecks.csproj`: **94 annual + 15 gameplay assertions**.
- `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/chapters/Terra.ChapterChecks.csproj`: **38 assertions**, including old fingerprints; no provider call.
- Unity 6000.6.6f1 compiled with zero errors/warnings and passed **34** full-catalog lifecycle/JSON replay checks across **127 eligible offers**. The follow-up call briefly disconnected during domain reload; waiting for readiness restored the connection. Existing consequences suite passed **1,372 assertions**. The attached headless Editor is running successfully. The user unlocked the Mac and actual Chrome verification resumed; see the browser checkpoint below.

**Not a phase-completion or release claim.** The direct visual gate, actual browser evidence, final balance/ending/accessibility/performance and release rehearsal are not yet accepted. Human novice observations remain unavailable.

## Phase 5 balance and transition checkpoint

The same standalone suite now passes **608 assertions**. It executes two distinct plans against the real expanded offers and cash reservation rules, then compares manual continuation (reloading after every year) with the exact fast-forward transition sequence. Snapshots are identical. Both plans encounter the saved Year 3/7 adverse conditions, fund every selected bundle and finish with no arrears and positive uncommitted cash.

| Actual plan | Year 10 Economic / Social / Environmental | Closing public cash |
|---|---|---:|
| [Community and services](community-and-services.json): apartments, rotation, park, village center, solar, wastewater, restoration | 85.4 / 99.0 / 54.8 | $5,240,000 |
| [Land and livelihoods](land-and-livelihoods.json): wind, farm storage, two rotations, restoration, apartments, wastewater, park | 94.2 / 91.0 / 66.6 | $6,163,000 |

These are reproducible engineering balance examples, not novice playtest findings or a claim that every family is equally strong. The tested no-action, saturated services, insufficient funding, adverse conditions and resource-intensive data-center cases remain part of the suite. The source chart prices have not been changed. No elapsed-time or 8–12-minute human-comprehension claim is made. Full-browser adaptation, confirmation and timing remain to be verified.


## Future, replay and whole-game browser checkpoint

The 3.1.0 scenario explicitly stores final targets of Economic 75, Social 75 and Environmental 60. Its separate save key preserves earlier version-3 test history without reinterpreting accepted scores. Confirmed fast-forward saves the frozen request and every accepted year, yields between years, and uses the same transition path as manual play. The final comparison computes a no-new-action run through the same engine and saved drivers. Opening/reference land is read-only and identified; budget and meters remain the player's accepted result. Timeline, optional transfer question, takeaway and confirmed baseline replay are connected.

Chrome 155.0.8059.85 on the local Web export has exercised the actual annual controls: two proposals, undo one, Year 1 acceptance, reload of the accepted report, a different Year 2 park decision, Year 3 wet conditions, and confirmed continuation to Year 10. The first browser route ended at $19,664,000 with meters 55.5 / 61.0 / 42.0 and $4,000 future obligations; no duplicate payment occurred after reload. Both timeline pages, the optional question, replay cancellation and confirmed replay were exercised. These are engineering observations, not novice findings.

The first browser pass exposed reference-map tooltips showing accepted projects, larger-text help overflow, a cramped 125% layout and a cropped tall wind project. Corrections suppress reference tooltips, retain an explicit map caption, shorten help/year headings, adapt the compact dock, and frame selected projects using their real catalog bounds. Full-site land treatments now use terrain-following plants instead of a tiny model sample on a flat parcel-sized pad. Wind keeps its real rotor envelope and a separate level 16 m foundation. Existing free audio is gesture-triggered, fades in, obeys mute/focus and includes action cues.

The compact browser audit at 125% zoom (1024×576 CSS pixels in a 1280×720 physical capture) found no text overflow. Wind is rejected on an undersized field and accepts its dedicated same-AOI opportunity, reserving $3.4M. Standard captures use explicit viewport clips to avoid the Chrome full-window screenshot scaling artifact. Historical first-pass captures and `large-text-initial-issues.json` document findings rather than final acceptance. Final correction checks, operating-family review and foreground performance results will be added before phase acceptance.

Pattern IDs: TN-03/04/05/06/08, CS-01/02/04, AX-01. No Phase 2A acceptance, final visual benchmark, novice comprehension, controlled performance, public release or rehearsal completion is claimed here.


The subsequent Chrome run reproduced the complete **land-and-livelihoods** strategy through actual catalog/site controls: seven Year 1 projects, waiting through Year 5, apartments on Lower west field in Year 6, the Year 7 setback, then confirmed fast-forward from Year 8. The ending exactly matches the standalone model: **94.2 / 91.0 / 66.6**, **$6,163,000** cash, **$612,000** beyond-horizon obligations and no arrears. Year 7 review correctly includes **$270,400** pay-now ($120,400 operation plus $150,000 repair). The no-action reference visibly removes projects, labels the map and suppresses accepted-project tooltips; returning to the future restores the accepted map/result. Evidence: `chrome-land-strategy-future.png`, `chrome-year7-repair-review.png`, `chrome-no-action-reference.png`.

Larger-text help passed its corrected text-height audit at 1440×900 and at 125% zoom / 1024×576 CSS. The larger-text ending also fits the compact modal (`chrome-large-ending-125.png`). The 1920×1080 larger-text pass identified a three-pixel accepted-tooltip excess; its redundant closing sentence was shortened. These checks do not establish screen-reader equivalence for the Unity canvas.


Final correction export `1e172d1aad5c4a4abc90fb3f7c745cda` succeeded in 108.391 s. It restores the completed strategy unchanged, correctly frames the operating 165 m turbine (`chrome-operating-wind.png`), and emits no browser errors in the inspected log. Two mute/unmute cycles changed Chrome's audio-playing indicator as expected; reduced-motion controls changed state. This verifies control/output activity, not a human listening-quality assessment. Standalone full-game checks remain 608/608; the Unity lifecycle entry point completed successfully. The latest local asset receipt pins the tested bytes. Diagnostics now separately report annual-transition duration and native/managed memory observations for the remaining performance work.
