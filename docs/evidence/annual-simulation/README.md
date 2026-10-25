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

Current checks: **124** weather/legacy compatibility assertions, including two previously published ten-year save fixtures and Unity-style empty DTO expansion. No new numeric transition, public repair, crop/event resolution, session save-first commit, v2 client, browser acceptance or balance is implemented. The draft constants are authored proposals awaiting sensitivity and edge-case tests, not accepted calibration.

### Continuation boundary

Keep the current `AnnualTown` as the sole queue, cash, arrears, history and transition owner. The intended implementation extends its existing staged payment pipeline, not a parallel ledger: initialize per-site state for all sites; freeze it with the pinned driver; calculate crop/calendar and connected event impacts; derive explicit public repair liabilities; pay under the existing upfront → prior arrears → stable program/operation → new repair order; derive outcome primitives; apply paid repair only to next year's condition; publish the complete staged result. Baseline farms must retain damage too. Use the same state and schedule for no-action comparisons. Do not reinterpret old 3.x histories.

Crop species/practice are distinct; margin area reduces harvest, cover crop does not. Practice maturity changes conditional mitigation/transition cost; it does not give an unconditional yield premium. Preserve harvest condition at the authored crop harvest date so a later event cannot erase it. Solar/wind remain separate explicit constant assumptions until supported drivers exist. Event storage must drain across elapsed days and be consumed once per event/group; remote/unbuilt/unfunded capacity cannot protect another group. The upstream component cannot be eliminated by local storage. Public repair responsibility is currently proposed only for swale/park/wastewater; private crop loss is output, never treasury expenditure.

Still needed: wire full schema/copy/hash/replay and validate nonempty extensions; implement bounded numeric resolution and itemized repair debt/recovery; compare every accepted regenerated report/hash on restore; expose scores/deltas/raw deficits; replace exact future harvest previews with conditional ranges; save-first session transaction with recoverable failures; consume the separately owned `ReportNarration` v2 helper/service after numeric acceptance, without `CompleteYear` in prose handling; pin first visible mode and ignore late replies; keep legacy v1 paths isolated; actual Chrome three-year adaptation + ten-year/reload/ending; final combined release rehearsals/visual acceptance.
