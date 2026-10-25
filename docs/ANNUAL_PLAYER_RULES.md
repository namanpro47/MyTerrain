# Annual player rules v5

Implemented candidate: October 25, 2026. Rule ID `annual-player-rules-v5.0.0`, schema-2 definition version `5.0.0`. Peter approved the behavioural changes; numerical hazard coefficients and pressure weights below are authored game balance, not sourced measurements.

## Public budget and upkeep

All due upkeep/program/repair costs are charged in integer USD cents, including when cash becomes negative. The negative cash balance is the debt; it is not recorded again as arrears. No interest, bank, borrowing UI or additional utility rationing is introduced. Public appropriations and eligible receipts offset the balance automatically. New construction is blocked while cash is negative. Cancellation, undoing proposals, reports and End Year remain available. Construction reserves upfront costs, any conversion work, and this year’s program/upkeep costs against current cash. Future upkeep remains disclosed but is not held in cash at placement; future appropriations and receipts cannot fund current placement.

Economic is a separate 0–100 indicator of output, jobs and paid obligations. It is not the public cash balance. Utility quantities remain disclosed indicators and score inputs; they no longer separately ration jobs, local services or occupied homes in this mode.

## Cancellation

Browse an accepted site, select **Cancel project…**, review construction progress/refund and queue it. It commits at End Year and can be removed or undone beforehand. The site opens for a new project in the next planning year, including while planning on a negative balance. Refund:

`upfront public capital × max(0, construction years − elapsed annual construction steps) / construction years`

Round down to integer cents. A three-year project cancelled after one annual step returns two thirds. Completed and zero-construction-duration projects return $0. Spent upkeep/program costs are not refunded. No future obligations are charged for a cancelled project. The original accepted reports stay immutable; retry/reload cannot issue the refund again.

Cleared land remains vacant and disturbed: no production, service or connected retention, vegetation at most 25% of the preceding cover fraction, reference-storm curve number at least 86. These are explicit authored simplifications, not a demolition or ecological recovery model. A replacement has normal costs and establishment timing. No automatic recovery/repair of cancelled land is added.

## Signed Environment

Economic and Social remain 0–100. Environment uses the existing cover/runoff/clean-energy/water components, then subtracts occupied industrial-site pressure: data centre 18 points, fossil power 30, agricultural storage 6, floored at -100. Pressure applies during construction and later occupation, including retirement until cancellation; adding capacity beyond demand cannot erase it. It is recomputed each year, not repeatedly accumulated for the same project. Six financed, eligible data centres can therefore produce a negative score. This is a game trade-off, not a measured pollution, health or proximity claim. Environment does not introduce an additional construction lock.

## Inferred regional event profiles

Each percentage below is an **annual chance of the game's broad regional disruption**, not the chance that a given parcel is damaged, an official forecast or a measured disaster return period. Identity sources and research caveats are preserved in [the regional inventory](REGIONAL_HAZARDS.md). Numerical values live in `AnnualPlayerRules.Profiles` and are copied into each versioned definition.

- Coon Valley / Vernon County: heavy-rain/flood 20%; dry growing season 15%; severe storm/hail 25%.
- Point Reyes / Marin: wildfire disruption 8%; dry growing season 25%; earthquake damage 2%.
- Suusamyr / Chuy: earthquake damage 1.5%; slope/snow disruption 15%; snowmelt/heavy-rain disruption 12%.

Exposure is separately sampled per unprotected site: flood 30–35%, drought 70%, storm/slope 25%, wildfire 20%, earthquake 35%. Exposed-site output multipliers range 0.60–0.90; operational-asset condition loss 0.04–0.18 per event, combined annual condition loss capped at 0.25. Flood condition loss is reduced by authored vegetation/retention protection, at most 50%; earthquake occurrence cannot be changed by a swale. Repair cost is the damaged fraction of the project's upfront public capital, charged once. Paying it does not instantly restore condition. The same event can affect baseline output without inventing an asset repair bill.

Draws use SHA-256 of version, saved seed, region, year, hazard and channel/site ID. They are independent simplified game draws, not correlated climate/weather samples or hazard maps. Multiple hazards may occur in a year; no live weather service is required. Legacy Year-3 weather multipliers and fixed repair shocks are replaced in this mode, avoiding a duplicate weather/block flood charge. New Run creates another saved seed; retries, reloads and no-action comparisons reuse it. Planning previews use neutral regional conditions rather than revealing the saved future event roll. Hazard magnitudes, affected parcels and economic benchmarks are not locally calibrated.

## Persistence and narration

The current native annual UI opts new runs into v5. Each location/layout keeps a separate `.player-rules-v5` annual key, chapter key and seed key. Existing v3/v4 keys are preserved; there is no silent save migration. Missing/corrupt data retains the existing recovery-copy behaviour.

End Year computes and saves the accepted numerical year plus a local summary before optional AI transport. AI replaces only the prose of that same accepted result. Quota/provider/validation failure produces **AI unavailable / Local summary**, using the accepted values. Players can stop waiting or continue without AI. An older chapter key cannot rewind a newer annual save. Full annual and chapter saves use lossless gzip encoding in PlayerPrefs; old plain JSON saves remain readable. Replay backs up the full run before changing the active state, and leaves the current run intact if storage rejects the backup. PlayerPrefs supplies browser-backed Unity storage; unavailable/full/cleared storage cannot guarantee recovery and is reported through the existing save-error UI.

The narrator receives current ESE/cash/refund facts, the three inferred regional hazard chances and recent accepted history; Year 10 includes all ten annual summaries. Past choice groups are explicitly abbreviated to three per year in prose context; complete history stays saved. The model does not update balances, scores or events. Facts and offline text are validated against the accepted transition. No real provider call or deployment is part of this delivery.

## Integration boundary and verification

This extends the existing playable schema-2 AnnualTown authority; it does not create another ledger. The schema-3 numeric-v4 calibrated-weather/crop foundation and acknowledged numeric transaction APIs remain separate and unchanged. **This is not a claim that numeric-v4 is now wired into TerraMain.** V5 productive outputs are illustrative scenario units, not crop-specific tonnes or next-season yield forecasts.

Native inspector/confirmation patterns follow TN-05, TN-06 and CS-01 in the design reference. The renderer removes cancelled project art and leaves a tilled surface; rendered layout/scene acceptance still requires Chrome QA under AX-01. See [validation evidence](evidence/annual-player-rules/README.md).
