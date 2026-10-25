# Annual simulation v4 — implementation in progress

Fresh isolated worktree based on `67c01417906b74a8f024ae9975158cb1dda6979d`. This candidate is not integrated, activated or browser verified. Existing 3.x runs remain unchanged.

The first pure C# component consumes the exact published Wisconsin weather pack and daily companion. It checks their original byte hashes before decoding, derives the agreed UTF-8 SHA-256 contiguous schedule, validates saved schedule identity, returns detached data and calculates separate crop-calendar precipitation/heat/dry-spell summaries. No run/branch/action enters the weather selection. It preserves precipitation as liquid equivalent and does not infer intensity, flood depth or a renewable-generation driver.

Initial validation: 118 checks pass, including all six published ASCII/Unicode schedule vectors, changed-byte/schedule rejection, UTF-8 seed size, ten annual crop-calendar summaries and detached return objects. This does not validate the upcoming crop response, persistent site damage, public repair or report-only acceptance implementation.

```sh
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/annual-simulation/Terra.AnnualSimulationChecks.csproj
```

Next work extends `AnnualTown` as the same authoritative queue/ledger/history owner, with optional versioned numeric DTOs and preserved legacy hashes. Every site needs persistent condition/crop state; accepted reports need itemized liabilities, raw deficits and score deltas. Report-only narration is a separately owned additive contract. No scene/renderer/HUD is owned by this pure-model slice.

## Combined release checkpoint — not ready for runtime integration

At the release owner's user-authorized checkpoint request, work stops at a preserved draft. Weather consumer commits `58e58e3` and `821a022` are additive, tested, and inactive; they do not change gameplay. Subsequent draft changes add numeric DTOs, optional legacy-compatible hash/copy handling, a rule factory, and validation scaffolding. **Do not merge the whole annual-simulation branch into the release.** The v4 factory is intentionally unusable until `AnnualTown` accepts/implements schema 3; this prevents a new definition from silently running the old transition.

Current checks: **2,399** existing connected full-game/stress assertions and **124** weather/legacy compatibility assertions, including two previously published ten-year save fixtures and Unity-style empty DTO expansion. No new numeric transition, public repair, crop/event resolution, session save-first commit, v2 client, browser acceptance or balance is implemented. The draft constants are authored proposals awaiting sensitivity and edge-case tests, not accepted calibration.

### Continuation boundary

Keep the current `AnnualTown` as the sole queue, cash, arrears, history and transition owner. The intended implementation extends its existing staged payment pipeline, not a parallel ledger: initialize per-site state for all sites; freeze it with the pinned driver; calculate crop/calendar and connected event impacts; derive explicit public repair liabilities; pay under the existing upfront → prior arrears → stable program/operation → new repair order; derive outcome primitives; apply paid repair only to next year's condition; publish the complete staged result. Baseline farms must retain damage too. Use the same state and schedule for no-action comparisons. Do not reinterpret old 3.x histories.

Crop species/practice are distinct; margin area reduces harvest, cover crop does not. Practice maturity changes conditional mitigation/transition cost; it does not give an unconditional yield premium. Preserve harvest condition at the authored crop harvest date so a later event cannot erase it. Solar/wind remain separate explicit constant assumptions until supported drivers exist. Event storage must drain across elapsed days and be consumed once per event/group; remote/unbuilt/unfunded capacity cannot protect another group. The upstream component cannot be eliminated by local storage. Public repair responsibility is currently proposed only for swale/park/wastewater; private crop loss is output, never treasury expenditure.

Still needed: wire full schema/copy/hash/replay and validate nonempty extensions; implement bounded numeric resolution and itemized repair debt/recovery; compare every accepted regenerated report/hash on restore; expose scores/deltas/raw deficits; replace exact future harvest previews with conditional ranges; save-first session transaction with recoverable failures; consume the separately owned `ReportNarration` v2 helper/service after numeric acceptance, without `CompleteYear` in prose handling; pin first visible mode and ignore late replies; keep legacy v1 paths isolated; actual Chrome three-year adaptation + ten-year/reload/ending; final combined release rehearsals/visual acceptance.

## Separate continuation after checkpoint

The immutable release-excluded branch remains `codex/annual-simulation` at `d908c01`. New work continues on **`codex/annual-simulation-resolution`**, reconciled with main `13a15a4` (deployment evidence only). It remains outside the combined release and default game.

The first numeric transition now extends the existing staged `AnnualTown` payment pipeline. Explicit schema 3 definitions can run in the standalone harness; all sites retain condition, crop/rotation and stored water, new public repair debt is derived from declared park/swale/wastewater responsibility, prior repair arrears map to stable liability rows, and end-of-year recovery changes only subsequent condition. Accepted reports include a noncircular full-report digest, per-event/group storage accounting, itemized payments, crops, impacts, all score deltas and raw service deficits. Legacy v1 narration rejects this new schema. Calling the old exact `Forecast` API on an unplayed v4 year explicitly rejects until conditional-range UI is supplied.

Early checks: **215** weather, saved legacy, numeric ten-year replay, report tampering, driver-content authentication, unknown-join rejection, public/private liability, storage and payment reconciliation checks. These are bounded smoke/boundary checks, not all 21 cases in the independent review. Public APIs for the additive narration projection are `AnnualSimulation.ReportHash(TownYearReport)`, `ValidateReportHash(TownYearReport)`, and `OpeningOutcomes(AnnualTownDefinition)`. Hashing requires the accepted report version `annual-numeric-report-v1`, clears only its own transitionHash field on a detached copy, and hashes the complete report. Restore compares the regenerated complete report and site state.

Trusted driver digests are derived from the exact verified weather bytes and v4 corn/soy calendars, with only game-year zeroed; historical year and every other driver field remain bound. Regenerate for review with `--emit-driver-hashes`, which writes an ignored local artifact. They prevent an in-range altered seasonal summary/event from passing on the strength of a copied pack-hash label.

Unfinished: full adversarial/state validation, crop-stage and storage edge cases, partial repair/debt and six-data-center fixtures, sensitivity/balance, conditional preview, save-first session persistence, v2 report projection/client, Unity and Chrome validation. No new-model release, scientific calibration or visual acceptance is claimed.

## Semantic validation and scenario checkpoint

The continuation passes **724 numeric assertions** and **2,399 existing connected legacy/full-game/stress assertions**. [Source-bound receipt](validation-checkpoint.json). This includes partial old repair repayment, linked unpaid damage, cash and itemized-due conservation, harvest timing, pending reload/duplicate acceptance, malformed state/report rejection, and read-only planning envelopes independent of future weather. `ValidateState` checks exact site coverage, finite bounds, repair cause/payment/recovery joins and debt; `ValidateReport` checks crops, event stage, storage mass/capacity and itemized payments before publishing. Restore still requires full deterministic replay equality.

Storage recovery is explicitly `linear-capacity-drain-per-day-v1`: `min(occupied, effectiveCapacity × dailyFraction × elapsedDays)`. This deliberately differs from the review's proposed exponential example. Events occur before end-of-day harvest; eligible pre-sowing damage lowers persistent site condition, harvest-day damage affects that harvest, and later damage only affects carryover. Rotation advances by elapsed operational calendar season, including suspended seasons; it does not wait for a funded harvest. Calendar days include leap days and year-end draining. Functional vegetation applies condition once, fixing the earlier double condition multiplication.

`PlanningRange(town)` exposes exact queued upfront/reserved/available money and conservative zero-to-capacity production envelopes using choices, lifecycle and existing condition. It does not read or resolve unplayed drivers. These are bounds under authored rules, not probabilities or promised harvests; UI integration is pending.

The [financed six-data-center fixture](six-data-centers.json) uses the six legal connected-map sites with a deliberately enlarged opening treasury. All operations are funded, all years share the baseline weather, and replay matches. By Year10, energy demand is141.59 scenario units versus20 baseline, supplied40; water demand141.59, supplied100; cropped area72,512.5m² versus98,112.5m². Raw deficits remain visible beside bounded scores. No unsupported proximity/health effect or automatic private tax revenue is introduced. This is an authored stress fixture, not a realistic financing recommendation.

[18 sensitivity/strategy runs](sensitivity.json) pin low/central/high coefficients independently of weather. Baseline ten-year illustrative production ranges from274.45 at low response to178.25 at high response; central218.75. Cover-crop, rotation and margin routes show different output/cover trade-offs under the same schedule; margin output is lower than baseline at low/central response and slightly higher at high response. The variation establishes sensitivity, not calibration or balance acceptance.

Still outstanding: more storage/boundary/disruption cases, client save acknowledgment and v2 narration integration, all-three-terrain runtime isolation, actual Unity/Chrome and final combined release/rehearsal/visual gates. This numeric branch remains excluded from the current combined release.

## Storage, retry and two-path follow-through

**2,351 numeric assertions** passed before the continuity/culture checks below. [Storage boundary evidence](storage-boundaries.json) covers zero and full capacity-based daily drainage, explicit capacity-loss release, no protection from a building/suspended swale, no use in remote groups, unaffected upstream hazard and exact replay. A fixture that waits for actual baseline damage verifies that a later project inherits it. Reloading each of ten frozen year requests and then finishing yields the exact uninterrupted snapshot. This tests pending persistence/retry in the engine; it does not replace the forthcoming durable-client save-failure tests.

The independent canonical report projection passes **435 assertions across30 accepted years** against the newer model. Its original static fixtures still identify their earlier model checkpoint; no earlier hashes are silently relabelled.

[Two affordable full-catalog paths](strategies.json) finish every intended project without arrears under central response: community initially ended69.6/62.6/47.5 with$4,607,740.83 cash and$797,000 reserved; land-and-livelihoods initially ended68.6/57.9/57.4 with$5,472,102.65 cash and$612,000 reserved. Both are **mixed outcomes** against75/75/60 targets. This establishes reachable and financially viable choices, not successful new-model balance acceptance. Weather and persistent condition make the old scripted-demo strategy results inapplicable to v4. Further adaptation/balance checks remain open.

The [disposable Unity check](unity-numeric-check.cs) is prepared to restore .NET-authored baseline, public-repair, repair-debt and six-center decades through actual JsonUtility/Mono, compare all report hashes and derive validated local v2 reports. It has **not run yet**; heavy-resource priority was yielded to the user-requested current production release. It never touches a live scene/session or preferences.

## Continuous severity and locale correction

The new model initially inherited a binary “any disrupted site loses its whole-year Social continuity share” equation. Its much more detailed rain impacts made even tiny nonzero loss remove the same20-point component as severe year-long disruption. Numeric v4 now uses the already computed mean time-weighted site condition for this authored20%continuity proxy. All actual impacts remain recorded and private/public condition rules are unchanged. Schema2/legacy keeps its exact published binary equation. A zero-versus-very-small-impact case disproves the former discontinuity. This is a declared game rubric, not measured welfare.

Numeric report reason strings also use invariant formatting; French, Arabic and Turkish system locales must reproduce the complete same-seed ten-year snapshot byte for byte. Legacy reason formatting remains unchanged. Current total: **2,356 numeric assertions**, **435 projection assertions**, **2,399 legacy assertions**. Source hashes in validation-checkpoint.json pin this continuation.

Current two-path scores are community69.6/76.7/47.5 and land68.6/72.6/57.4. A [bounded five-plan exploration](balance-exploration.json) preserves the same costs/weather and reveals why placement and timing matter: one plan reaches71.6/78.3/66.1; another greedily fills all housing-eligible plots before it can afford its delayed housing, leaving that intended project unbuilt. These are diagnostic mixed outcomes, not a claimed winning strategy or reason to hide unmet demand.

Incoming main ea37af0 introduces32/305/333 open parcels plus inherited settlements. This continuation's exact model receipts above still use the13-site connected fixture. Dense-map simulation/performance/report/save-size acceptance is pending; do not promote old sparse-fixture results as coverage of those maps. New numeric saves must use a separate schema-qualified namespace because dense legacy definitions also use version4.0.0 with schema2.
