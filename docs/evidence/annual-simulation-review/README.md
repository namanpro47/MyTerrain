# Independent review of the annual numeric draft

October 25, 2026. Reviewed source **d908c01d28e8b4d11d98db8e728793917538202b**, branch `codex/annual-simulation`, against its base `67c01417906b74a8f024ae9975158cb1dda6979d`. Review workspace starts at refreshed main `13a15a43c43bab9e0eb6a2a90bded4bb4d7aba0c`. Scope agreed with the annual owner: evidence only; no Core, session, UI, runtime or test-suite changes.

**Verdict: preserve the draft, keep it out of the current release, and resolve the boundaries below before enabling schema 3.** The factory deliberately fails the old schema validator; that is a useful guard, not a defect to remove on its own. The weather reader, optional DTOs and factory are preparatory work. The numeric transition and client transaction do not yet exist. Findings describe integration risks in this draft, not newly observed failures in the shipped game.

Read against [simulation requirements](../../SIMULATION_APPROACH.md), [annual action contract](../../ACTIONS_AND_EFFECTS.md), the existing staged ledger and replay in `AnnualTown`, and the separately prepared report-selection v2 contract at `629d082cae4b74794eec68cc620dae65f2173f79`. The owner reports 124 weather/legacy checks and 2,399 connected full-game checks. This review did not rerun those suites, operate Unity/Chrome or claim a numeric runtime pass. No new scientific calibration is proposed.

## Decisions required before wiring the transition

### R1 — Define one noncircular accepted transition hash

`AnnualSimulationContracts.cs:61–70` puts `transitionHash` inside the report. Hashing the complete containing report would include that same hash. No implementation currently defines its preimage. Hashing only the request would instead fail to bind the accepted ledger, outcomes and closing condition that narration describes.

Use an explicitly versioned digest over the **complete accepted numeric transition**, excluding only the digest field itself. A detached copy with that one field set to an explicit sentinel is sufficient if its exact canonical encoding/version is pinned. Include definition identity, request identity/input, year, ledger, outcomes, the complete simulation report/closing state, and accepted deterministic consequences. Do not include narration or transient UI state. Normalize only that digest field; reject other missing/partial numeric fields. Store the digest, recompute it during restore, and compare the entire regenerated accepted numeric report as well as closing snapshot state. The existing legacy restore regenerates reports but does not compare every stored report field; schema 3 needs this additional boundary without changing legacy commitments.

The report-v2 adapter must obtain this accepted identity from the saved transition. It must not independently invent another numeric hash, hash the prose, or call `CompleteYear` while accepting prose. A new adapter may use a small public numeric verification/accessor API; it should not expose internal mutable state merely to obtain the digest.

### R2 — Preserve one condition authority and itemize repair repayment

`TownAsset.condition` already exists; `TownSimulationSiteState.condition/outstandingDamage` adds a second representation. Choose the site state as the new model's authority for baseline and developed sites, with any asset field explicitly synchronized for existing rendering/lifecycle consumers. Define whether `outstandingDamage` equals `1-condition` or is a distinct event-only stock. Nonpayment degradation, rain damage, private recovery and funded public repair must each enter once. A new project cannot reset pre-existing site damage by constructing a fresh `TownAsset(condition=1)`.

The old ledger pays all prior arrears in aggregate before operations. `TownRepairLiability` can retain individual unpaid amounts, but the draft does not yet define how `arrearsPaidCents` reduces those amounts and restores their linked condition. Publish a deterministic allocation policy. One simple policy is prior non-repair arrears first, then prior repair liabilities in `(createdYear, id)` order; other policies are possible, but they must be pinned rather than depend on array order. Derive non-repair arrears as the aggregate minus itemized repair debt and reject negative differences. A prior liability paid from `arrearsPaidCents` must not also be charged as current `repairDueCents`.

For each liability, preserve original loss/cost and cumulative paid/recovered amounts or an equivalent invariant. Proportional partial payment must not repeatedly restore the original loss. Cap combined restoration at the damage actually remaining. Publicly responsible assets must not also receive free private-farm recovery that silently erases unpaid public damage. Private crop loss is lost output, not a public liability. A paid year-end repair affects next year's condition only; it cannot improve an already realized harvest or rewrite this year's delivered services.

### R3 — Account for storage once per event and connection group

`TownSimulationSiteState.storedWaterM3` is per site, while `TownEventImpact.connectedStorageUsedM3` is repeated for each affected site. This is representable, but its meaning needs to be fixed before loops are written. Otherwise each affected site can consume the full group capacity or reports can sum the same retained water several times.

Track occupied storage on its actual asset(s). For each chronological event: drain once for elapsed calendar days; calculate eligible funded, operating, connected capacity; allocate incoming local water once; then calculate site effects from the shared group result. Store either one group/event allocation record or per-site **allocated shares whose sum equals the group use**. A displayed repeated group total must be labelled non-additive. Never let local storage subtract upstream hazard. Bound storage by effective capacity and record what happens if condition/funding reduces capacity below existing stored volume.

Choose and version the drainage formula. `dailyStorageRecoveryFraction=.1` alone does not tell a future reader whether recovery is exponential or a constant fraction of capacity. The fixture proposal uses occupied-volume exponential drainage. Carry elapsed days across December/January and leap years. Skipped non-rain events must not freeze the clock; the next eligible event still uses its full elapsed interval. A zero recovery fraction, a full recovery fraction and a capacity loss all need explicit behavior.

### R4 — Authenticate driver contents and validate identifier joins

The raw weather reader verifies exact pack/daily bytes, which is strong. `AnnualSimulation.Validate` lines 45–68 subsequently checks saved driver metadata, ranges and calendars, but does not establish that its precipitation, temperature, event list and crop summaries equal those pinned bytes. An in-range changed seasonal total, removed event, or changed nonempty event record ID can satisfy those checks. This is not evidence that an existing fixed-definition save bypasses its definition hash; it is an unsafe boundary if a serialized definition/driver is accepted as an authoritative pack consumer.

At definition construction/restore, compare the complete driver with a fresh derivation from the verified pack and crop calendars, or compare with a trusted precomputed canonical driver digest bound to the same pack, derivation and crop definitions. Range checks complement that comparison. An arbitrary `packSha256` label does not authenticate an altered derived value.

The draft also checks four distinct practice IDs without matching the required action IDs (lines 31–36), accepts any nonempty exposure connection group (lines 38–43), and permits an empty public-responsibility array (lines 70–73). Require the declared supported practice set, its action mappings, and exact source connection-group joins. For the fixed v4 factory, require the intended public responsibility set or give an intentional alternative its own rule/version. Validate null array elements before dereferencing them, event identity against the source record, and derived analysis-window flags. Reject unknown IDs before they can silently become an unprotected site or an unrepairable public asset.

### R5 — Make timing, state validation and save publication explicit

The draft's harvest condition and next-year condition fields are useful, but `beforeHarvest` alone does not define events on the harvest day, before sowing, or a non-crop site. Pin the order: for example, eligible day events occur before end-of-day harvest. Distinguish post-harvest damage (carryover only) from realized crop loss. Preserve crop rotation phase when a year is suspended according to a declared rule; do not infer rotation from global year if a project began later. Baseline crop state must persist without an asset.

Add schema-3 validators for every frozen/opening/closing state, liability, payment, crop result, impact and score delta. Require exact site coverage and unique IDs, finite bounded values, valid phase/crop/action joins, valid money bounds, conservation equations and report-year identity. A normalized empty optional extension remains compatible with old definitions, but **must be rejected wherever schema 3 requires a numeric state/report**. Nonempty extensions attached to legacy rules must be rejected, not ignored.

The current session mutates the town then publishes; saving is a separate operation. The new save-first path must stage a complete candidate, persist it successfully, then publish the accepted result and request optional narration. On a failed numeric save, retain/recover the same pending request and do not announce that the year is saved. Persist the first visible narration mode separately against the accepted hash; a late AI response must not replace a local summary already shown. Continuing to a later year must not attach an old response to the new report.

## Disproving fixtures to implement with the numeric hooks

These are **proposed tests, not passing results**. Use small synthetic internal drivers for timing/conservation and real pinned weather for full-run reproducibility. Synthetic event injection must not become a production bypass for the verified NOAA consumer.

| ID | Controlled setup | Required observation |
|---|---|---|
| CAL-01 | Same field and event on harvest day −1, day, +1 | Day policy is explicit; +1 cannot change harvested output, but can lower next-year condition. Corn and soy use their own calendars. |
| CAL-02 | Identical seasonal summaries; event before sowing, in season, after harvest | Carryover and harvest effects follow stage; seasonal drought/heat excludes dates outside the crop window. No double loss through weather and event condition. |
| CAL-03 | Feb 28/29 and Dec 31/Jan 1 elapsed intervals | Day ordering/recovery is correct in leap and normal years; no reset of occupied storage on Continue/reload. |
| BASE-01 | Untouched baseline farm damaged in Year 1; Wait in Year 2 | Damage persists without a `TownAsset`; authorized private recovery occurs once. Manual, reload and fast-forward match. |
| BASE-02 | Add a project on an already damaged unused site | A fresh asset cannot reset site condition. Remaining farm area and developed capacity are counted once. |
| STORE-01 | One 100 m³ swale, two same-group sites, one 150 m³ local input | Group use ≤100 m³; per-site additive allocations sum to group use. Reordering sites does not change total use or results. |
| STORE-02 | Reset to full 100 m³ occupied storage; 10% daily occupied-volume drainage; next event after 1 or 30 days | Free capacity is 10 or about 95.760884 m³ respectively. The exact formula is a proposed rule decision, not source-derived hydrology. |
| STORE-03 | Same hazard; swale remote, building, unfunded, then funded/operating | Only the eligible connected asset supplies capacity. Upstream hazard survives even when all local input is retained. |
| STORE-04 | Two funded swales, changed input-array order; then one loses capacity | Stable allocation; no negative capacity or free extra storage. Carryover volume follows the declared reduced-capacity policy. |
| REPAIR-01 | Private baseline crop and public park receive equal condition loss | Crop loses output only; only explicit park responsibility creates public repair debt. |
| REPAIR-02 | Opening arrears $200, including $120 repair; cash $100; non-repair-first policy; current operation $50 and new repair $30 | Prior non-repair $80 and repair $20 paid. Closing arrears $180; itemized repair debt $130; no second charge of old $120. Year-end receipts/appropriation cannot fund the already missed current operation. |
| REPAIR-03 | Original public loss .24 costs $120; cumulative partial repair $20 then $40 | Recovery increments .04 then .08, not .04 then .12; total .12. Harvest already accepted remains unchanged; unpaid public damage is not freely recovered. |
| REPAIR-04 | New damage arrives while a prior liability is partly paid; two liabilities share one site | New debt is distinct, payments are stable and capped, recovery cannot exceed remaining condition loss. Repeated receipt/transition acceptance has no extra effect. |
| HASH-01 | Change one accepted ledger amount, score delta, driver value, crop result, payment or closing-state value | Accepted digest changes; restore rejects the altered commitment even if final aggregate cash is unchanged. Digest recomputation excludes only its own field. |
| HASH-02 | Tamper driver total within range; remove an event; rename its record; change group/practice to unknown ID | Trusted derivation/join validation rejects each before publication. |
| LEGACY-01 | Both published ten-year legacy fixtures, null and wholly empty optional DTO expansion | Existing definition/request/report fingerprints and outcomes remain exact. Partly populated/nonzero legacy extensions reject. |
| LEGACY-02 | Schema 3 save with missing/empty state, duplicate site, NaN, unknown liability, negative debt | Reject without partial state; do not normalize required v4 state into a playable legacy path. |
| RUN-01 | Same seed/choices through manual, retry, reload, fast-forward and no-network narration | Numeric reports/hashes identical. No-action comparison shares the schedule and rule identity, with distinct accepted run identity. |
| SCORE-01 | Funded six-data-center fixture using legal sites, compared with no action | Trace raw demand/deficits/displaced land and all three score deltas. No invented adjacency or health penalty; bounded scores do not hide growing deficits. |
| SAVE-01 | Inject persistence failure before accepted publication; retry same request | No accepted UI/narration before a durable success; one year, one spend, same driver on retry. |
| PROSE-01 | Accept year; local fallback; late AI response; continue; reload | First visible mode/receipt remains bound to the same accepted year/hash. Previous-year facts never substitute for a failed numeric year. |

## Proposed accepted-report projection for narration v2

Implement only after the owner freezes the numeric verification API. The projection is a pure derivation from a **verified, saved** `TownYearReport`, its matching definition and previous accepted report/opening baseline. No resolver, weather sampler, mutable town or provider belongs in the projection helper. Patterns TN-05/08 and CS-01/04 apply to concise outcome explanation; they do not accept the scenery gate.

| Canonical block | Exact source | Permitted explanation |
|---|---|---|
| Title and introduction | Accepted year; `outcomes` scores; simulation signed deltas; ledger closing cash | All three scores and signed changes, saved closing public cash, illustrative status. Do not call an unresolved preview accepted. |
| `budget` | Opening cash, upfront, prior arrears paid, program/operating/new repair paid, receipts, appropriation, closing cash and arrears | Reconcile public finances; distinguish private output and next-year opening cash. Unpaid repair detail uses liability IDs, not a generic cash penalty. |
| `harvest:<siteId>` | Crop result area/species/practice/calendar, factors, harvest condition and output | Explain this site's realized illustrative output using recorded rules. Do not label all crops as regenerative, sum incompatible physical units, or infer farm income. |
| `disruption:<siteId>` | Accepted event impacts and associated source event/connection rules | Supported rain disruption, site and stage; state post-harvest carryover explicitly. Do not infer depth, probability or blame a project for external weather. |
| `recovery:<siteId>` | Paid liability links, unrecovered loss, closing state and next-year condition | What was funded and what remains damaged/debted; no restoration of an earlier harvest. |
| `services` | Accepted raw resource/service deficits and rubric components | Which demands were unmet; compare like units and prior accepted values. Include only causal links the numeric rules actually record. |
| `reference` | Pinned schedule/pack, selected landscape provenance and disclosure | Shared Wisconsin reference, authored response curves and unavailable calibration. Terrain selection does not turn Wisconsin weather into local measurement. |

Use stable identifiers and invariant formatting for canonical text; localization must be a separately versioned projection, not a silent cache rewrite. Derive `siteIds` and `causalRuleIds` from validated joins. Each block allows at most eight of each link, text ≤600 UTF-16 code units; the request allows 1–64 facts and introduction ≤500. Aggregate events by site/year for a compact bounded set, or split facts deterministically; never silently truncate a material loss to fit. Prefer a small set of material outcome blocks, with full detail inspectable elsewhere. Fact ordering supplies the local summary's first three priorities and must be deterministic. All three headline scores remain in the canonical introduction even when no score card is selected.

The v2 provider selects/orders existing blocks only. Use **“AI-organized explanation”**, as agreed with the owner, rather than implying that the provider wrote the canonical factual text. A receipt must match accepted transition ID/hash, request hash and contract versions. Any mismatch is a failed prose receipt, never permission to recompute numeric history.

## Handoff

Numeric owner: settle R1–R5 and implement the new authority inside the existing annual pipeline. Then provide a minimal public accepted-report verification API and exact DTO/hash version for the additive projection helper. Root can implement that helper and its focused contract tests in a separately agreed file slice. Current combined release remains independent; this review is not authorization to activate the unfinished model.

### Owner response during review

The annual owner agreed to these specific implementation boundaries in chat, while keeping `d908c01` immutable and continuing on `codex/annual-simulation-resolution`:

- `AnnualSimulation.ReportHash(report)` copies the complete `TownYearReport`, clears only `simulation.transitionHash`, then uses the existing canonical `TownCopies.Hash`. The complete staged report gets its digest before persistence; restore compares the regenerated full report. The reviewer additionally requested a pinned report version, required-field validation and separate compute/verify semantics.
- Prior non-repair arrears are paid first, followed by old repair liabilities in stable creation/site/event order. Current repair rows represent only new repair dues. The reviewer requested liability ID as a final tie-breaker, rejection when itemized old debt exceeds total arrears, and incremental proportional recovery.
- A new `report.storage[]` will contain one record per group/event: capacity, opening stored volume, recovered volume, capacity-loss release, inflow, captured volume, closing volume and device site IDs. Impacts will hold a reduction fraction instead of repeating group cubic metres. The reviewer requested per-device/group conservation, unique device IDs, capacity bounds and an explicit policy for disabled or reduced-capacity assets.

These are **agreed implementation decisions, not verified runtime behavior**. Driver authenticity/identifier joins, timing/state validation and persistence still require implementation and disproving tests. [Review manifest and independently calculated fixture arithmetic](review-manifest.json) pin what was inspected; arithmetic alone is not a simulation test.
