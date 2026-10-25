# Annual simulation: choices, history and variable conditions

Decision brief, October 25, 2026. Proposed behavior, not implemented simulation.
Initial source review and 608-check run were at `64384a0`. This requirements refinement starts from refreshed `origin/main` `b11a2fc`, preserving intervening work. Canonical spec/PRD updated; runtime remains unchanged.

## Decision and diagnosis

**Recommendation: extend the existing offline annual engine with saved Coon Valley, Wisconsin weather and explicit response rules; give the LLM the resulting facts and relevant history.**

**Peter's refinement:** all fictional names/selected landscapes share one Coon Valley / Vernon County, Wisconsin weather/agricultural/economic reference. Terrain packs and saves stay separate. Inspect the hackpack first, with NOAA as the practical weather fallback. Every accepted year includes all three scores and signed changes. On AI backend/credit failure, automatically show labelled local prose from the same saved year's results. This updates the previously proposed per-region weather preparation; it does not claim implementation.

Diagnosis: the live annual definition uses the same authored Year 3/7 production factors and Year 7 public repair for every run; its consequence provider returns no contextual site effects. Additional prompt history cannot create a validated crop/weather/disruption simulation.

Acceptance: a player funds a farming practice, ends the year, sees its realized output, Economic/Social/Environmental scores and signed changes, itemized public balance and any affected sites, then adapts next year. Earlier investments, maturity, condition and unrepaired damage affect later results. Different weather schedules can produce different outcomes; retries and reloads cannot. Player and no-new-action futures share external conditions. All three existing terrain choices retain separate saves and disclose their common numerical reference.

Cheapest disproving check, performed before design: trace `FullAnnualGameplay.Create` -> `AnnualTown.BeginResolution` -> `AuthoredAnnualConsequenceProvider` -> `CompleteResolution` -> `AnnualEconomy.Evaluate`. The provider returns an empty effects array; weather is definition data with hardcoded Year 3/7 factors. There is no dormant weather sampler or crop-yield simulator to enable. The existing full-game executable passed 608 assertions on the legacy Wisconsin layout. This verifies the existing foundation, not the proposed simulation, connected layout, browser behavior or scientific accuracy.

## What the PRD actually asks for

- `PRD_ANNUAL_GAMEPLAY.md` sections 7-8 describe compact state/history, bounded consequences, coherent historical weather blocks, persisted drivers and same-weather comparison.
- Its adoption notice and canonical spec section 12 supersede a mandatory LLM consequence engine. Offline rules remain authoritative; numeric LLM effects require a separately reviewed causal contract. This proposal uses engine-computed effects.
- Public finances are government finances. Private harvest or sales do not automatically enter public cash. Farm assistance is a public cost; a crop harvest is community output.
- Heavy-rain disruption may be an explicitly fictional scenario. Neither the current map nor fixed-storm runoff establishes real flood boundaries, depth or annual probability.
- Three terrain locations remain authorized. Their common numerical reference is now explicitly Coon Valley/Wisconsin; display names and terrain provenance do not turn those reference statistics into measured local weather/economics elsewhere.

## Existing pieces to reuse

- `AnnualTown` owns the queue, frozen requests, history, asset start year/condition, integer-cent ledger, commitments and replay. Keep that single owner.
- Current outcomes already depend on lifecycle, funded upkeep, service demand and spatially connected reference-storm retention. Annual effects can already carry a production factor, condition delta and disrupted flag, but lack the proposed causal detail and weather/crop state.
- `AnnualChapters.Offline` fills local report templates from the accepted engine summary. Online narration explains those same facts. This is how offline chapters work; there is no local language model.
- `AnnualEconomy.Water` evaluates a fixed reference storm. Preserve that comparable indicator separately from realized annual storm/disruption scenarios.
- Current farm recipes use generic output units, including an authored 1.15 multiplier for conservation rotation. These are game tuning, not measured yields or supported practice benefits. New crop rules must replace this assumption explicitly in a new definition, never reinterpret old saves.

## One annual transition

1. Start planning Year Y with last year's accepted closing cash, liabilities, land uses, assets and carryover condition. Preview certain costs and conditional production ranges.
2. Freeze this year's choices and pin the immutable rule/data versions. Draw/save external drivers from the run's already pinned weather schedule, independently of those choices. Retries use the saved driver IDs and values.
3. Apply existing construction/establishment and public funding rules. Advance crop/land condition under the declared yearly timing policy.
4. Resolve agricultural output and connected heavy-rain impacts using the same external conditions, site state, practice and upkeep. Persist damage/recovery state; apply a damage factor once, avoiding double loss through both condition and a production factor.
5. Derive itemized public repair liabilities from the affected public assets and bounded repair-cost rules, then pay them under the existing cash/arrears policy. External drivers contain weather/hazard, not a universal repair dollar penalty. Calculate eligible receipts and closing appropriation, and reconcile the closing ledger. Closing receipts cannot retroactively fund missed operations. Preserve the stable payment order. For the first slice, paid year-end repair restores the declared portion of next year's starting condition; it cannot reverse that year's realized harvest loss. Unpaid liabilities retain damage/arrears, with asset-specific responsibility and recovery links in the report.
6. Commit the complete accepted transition atomically and save it. Closing cash for Year Y is opening cash for Year Y+1; never add another appropriation on Continue. Year 10 ends the run with liabilities visible.
7. Derive short causal facts and the report, including all three accepted scores, signed year changes and raw resource/service deficits. Narration can then run asynchronously against that accepted report. A missing key, timeout, refusal, invalid text or exhausted provider quota/credits automatically shows the labelled local template from that same saved report, without rerolling or erasing the simulated year. This requires refactoring the current chapter path, which waits for a receipt before committing the year; current fallback is still an explicit player choice.

**Timing matters:** End Year resolves the year just played. It does not reveal an exact harvest for an unplayed future year. Next year's preview is a conditional range until its year is resolved. Late-year flooding must not erase an already completed harvest; event season/stage must be part of its effect rule.

## Weather and uncertainty

Use one versioned offline weather pack referenced to Coon Valley, Wisconsin, for every fictional scenario and selected terrain. Separate terrain provenance and save identities remain. Required disclosure: **"Fictional scenario · weather and economic references: Coon Valley, Wisconsin · other parameters are authored game assumptions."** This common reference is deliberate, not a hidden substitute for local calibration. Benchmarks available only at Vernon County/Wisconsin scale retain those true source geographies. Budgets and unsupported prices stay authored.

Data selection: inspect the supplied hackpack for suitable weather, agricultural/economic and environmental inputs; record actual file, coverage, units, quality flags and license before adopting it. [The bounded audit](HACKPACK_DATA_AUDIT.md) found no accessible bundle/manifest, so NOAA GHCN-Daily remains the practical weather path rather than a claim that the unseen bundle lacks useful data. Check Coon Valley-area station/element coverage, distance/elevation, quality flags, missing-day rules and period. Select coherent seasonal/year blocks, preferably contiguous multi-year segments where coverage permits. This keeps wet spells, temperature and seasonality together. Independent daily dice rolls would erase those structures. If year blocks are spliced, specify cross-year moisture carryover and test the discontinuities. Do not claim that resampling represents future climate change.

Pin the actual ten-year schedule, weather pack checksum, sampler version, shared scenario seed and selected record IDs at run creation. Derive additional environmental draws from stable keys such as scenario-seed/year/site/driver/effect-channel, excluding branch/run identity. A different choice or comparison-branch run ID cannot shift unrelated randomness. Use the run ID only for state/receipt identity. A new scenario may vary; replaying another strategy may deliberately reuse its scenario seed. Both ending branches use the identical exogenous schedule and latent draws, then apply their different vulnerability states.

Separate weather variability from uncertain response coefficients. A poorly known treatment effect uses a labelled low/central/high sensitivity setting pinned for the run, not an independently sampled yearly "empirical probability." Fixed public contracts remain fixed. Do not infer a yield distribution from the PRD's two county-yield examples or flood frequency from one historical event.

Persist compact drivers such as growing-season rain, heat/dry-spell measures and heavy-rain episodes with antecedent-wetness/season indicators. Derivation definitions and missing data must be explicit. Daily observations do not establish subdaily storm intensity. Solar and wind need distinct supported drivers or separate labelled assumptions; stop reusing the crop production factor for renewable generation in the new rules.

## Agriculture: type and history both matter

Crop species and farming practice are separate fields: e.g. corn plus a cover-crop practice, not "regenerative" as a crop. A rotation needs a pinned sequence/current phase; a living margin reduces cropped area, while a seasonal cover crop does not automatically do so. Account for existing baseline farms as well as newly funded ones. A new project cannot claim output before its configured operational season.

Minimum response model: productive area x crop-specific benchmark output per area x establishment/condition response x seasonal weather response x management response, with explicit units and bounds. All response curves need provenance and evidence labels. Add persistent per-site state keyed by stable site ID for every site, including baseline/no-action farms: current crop, rotation phase, condition, outstanding damage and recovery. Initialize it from baseline and include it in frozen requests, snapshots, detached copies, hashes, validation and replay. The current asset-only condition mechanism cannot persist damage to untouched farms. Add a soil/soil-water stock only if its update and role in output are implemented; prose memory is not a substitute for state.

Use predefined crop/practice recipes grounded to the common Wisconsin reference first; a full crop-selection interface is outside this slice. Keep physical harvest quantities separate by crop/unit and convert them to the declared comparable output index before meter aggregation; never sum tonnes, bushels and industrial output directly. Existing committed sites cannot currently be redeveloped. This slice allows new investments elsewhere, configured rotation and automatic funded recovery; changing an established farm's practice needs a later explicitly costed conversion command, not an implied free switch.

For the first playable slice, retain **illustrative production units** with declared crop/practice scenario curves. Enable bushels or tonnes only for a supported crop with a documented conversion and suitable Wisconsin benchmark. Unknown crop/reference combinations stay unavailable or visibly illustrative; changing the displayed terrain does not create another calibration. County yields may anchor scale but are not parcel measurements. Remove any unconditional regenerative premium; the chosen practice may help, do little or cost output depending on time, area and conditions.

FAO AquaCrop is a useful reference for required crop/soil/weather inputs, not a proposed new runtime dependency. A full process crop model would require a separate data/calibration effort.

## Flooding: hazard, exposure, vulnerability

Weather/upstream conditions create a hazard. Land conversion, occupied footprint, drainage connection, establishment and maintained storage change vulnerability. Severe events can cause disruption even under careful management; poor land use can worsen consequences under the same storm. Choices do not cause a punitive random storm.

First implementation: **illustrative heavy-rain disruption**, using explicit site exposure/connection groups and bounded damage/recovery rules. Evaluate cover, antecedent conditions and effective connected storage before determining affected sites. A remote or unbuilt swale has no protection. Storage has capacity and refill/recovery limits; do not reuse its full volume for every day without a declared recovery rule. Missing exposure or connection data must not silently mean zero risk.

An external/upstream hazard component may be an authored scenario input; local retention cannot eliminate it. Separate private crop loss from public infrastructure repair obligations. Add public repair costs only for explicitly defined public assets/responsibilities, never as an automatic cash penalty for all farm losses. Record affected sites, duration/season, output/service interruption and carryover condition with driver/rule IDs. No inundation map or real-world annual flood odds are implied.

Keep fixed-storm runoff as a management comparison. Its runoff volumes alone do not determine flood depths or damage. Actual hydraulic prediction would require catchment/routing, channel and exposure data beyond this slice.

## All three scores and cumulative trade-offs

Recompute Economic, Social and Environmental from the accepted primitive state after operations, resource/service scarcity, land displacement, damage and recovery. Every report includes the current 0–100 score, signed change versus the preceding accepted year (or opening baseline), raw components and the largest supported causes. Compare only pinned like-version rubrics; do not hide worsening shortages merely because a headline score has reached zero. Useful benefits cap at unmet demand; resource consumption, upkeep and adverse pressures accumulate over all active projects.

Required stress case: six data centers near the authored village, using legal sites and an explicitly funded multi-year/test fixture where the normal starting budget cannot afford six. Trace aggregate energy/water demand, constrained housing/services, displaced productive/vegetated land, funded operation and any supported environmental pressure. Add an explicit versioned proximity/connection/exposure rule before claiming that being "next to the village" changes outcomes. Any economic benefit is conditional too; no automatic tax income or guaranteed profit. Do not force scores below zero or claim a measured health impact. This is a required scenario check, not proof the current rubric captures all industrial externalities or that six fit every map.

## What the LLM receives and returns

Give it the accepted year, fictional display name/selected terrain and the explicit Coon Valley/Wisconsin numerical-reference label; this year's choices and relevant past choices with implementation dates; affected sites and carryover state; previous-year versus current outputs; accepted weather/event facts; itemized budget; all three score values/deltas and causal components; allowed causal rule/source IDs; and uncertainty/unavailable flags. Include a compact trend/history summary rather than an ever-growing raw transcript.

It returns a title, introduction and up to three cards explaining what changed, why, who/where was affected and what remains uncertain. Cite accepted fact IDs. It does not calculate the authoritative harvest, cash, scores, weather or damage. Any next-year advice is conditional on supplied facts; no invented alternatives or guaranteed forecasts. Continue schema/ID/length checks and add factual/causal consistency checks: valid fact IDs alone do not prove a sentence is true. Offline templates consume the same saved facts and history.

### Visible, automatic narration fallback

- Identify normal prose as **"AI-written explanation"**. Identify template prose as **"Local summary · saved simulation results"**; do not suggest a local language model or a simplified alternate simulation.
- On unreachable backend, timeout, missing credentials, quota/credit exhaustion, refusal or invalid prose after bounded repair, automatically display **"AI explanation unavailable · showing a local summary of your saved results. Your year and budget are saved."** Expose a concise actual reason in details; do not infer account billing from a generic quota response or expose provider secrets. Avoid auto-retry loops.
- Use the current accepted year's persisted transition/facts and matching hash. Preserve cash, output, scores, weather and history exactly; show report year/mode, including after reopening or reload. If a local template is chosen while AI is still pending, a late response must not silently replace that visible mode/report.
- Never substitute a previous year's values for a failed new-year simulation. Numeric resolution, validation or persistence failure remains pending/recoverable with an honest message. Prose fallback is available only when the matching numeric report exists.
- Current build status: manual offline selection; simulation and chapter acceptance are still coupled. The automatic behavior and disclosure copy above are specified implementation work, not delivered UI in this documentation refinement.

Illustrative example, not measured data or a model result: a field's healthy mature capacity is 10 output units; the saved seasonal response is 0.8 and its effective condition/management response is 0.9. Realized production is 7.2 units, with the loss attributed once. Public opening cash $500,000 minus $10,000 upkeep minus $20,000 public repair plus $100,000 appropriation equals $570,000 carried into the next planning year, assuming all other ledger entries explicitly zero. Farmer output adds no public receipt. The card explains the saved weather, affected field and funded response; it does not invent these numbers.

## Delivery and execution contract per unit

1. **Rules/data pack and annual contract.** Runs in offline preparation and pure C# validation; not the browser hot path. Rebuilding the same pinned inputs must derive identical bytes/checksums. A pack is a derivation before publication and a commitment once attached to a run. Source revisions may change before publication; pin versions/checksums so they cannot change an existing run. Deliver explicit crop/practice/weather/event parameters, provenance, shared scenario drivers/seed, per-site state independent of project assets, and schema/copy/hash/replay updates. Old runs stay on old definitions/save keys or receive an explicit migration; no silent resimulation.
2. **Annual resolution.** Runs in the existing pure C# engine in Unity and shared server code where needed. The first accepted transition is a commitment: a duplicate request returns the saved result with no extra payment, draw or advance. Queue edits, retries and server/client rule mismatches are possible actors; use frozen input/version/hash checks, detached staging and atomic publication. Crop response, spatial event effects, itemized endogenous repair liability and the single ledger resolve once. Replace asset-only baseline condition validation with rules for legitimate per-site damage/recovery; do not relax unknown-site/lifecycle checks globally. Update simulation replay to the same new provider; isolate legacy `AnnualChapters.Preview` on its old contract. Budget replay/validation cost for a ten-year small-map run; no network dependency for numeric resolution.
3. **Reports/narration.** Runs after saved simulation acceptance, in Unity templates and the optional local chapter service. Introduce a versioned report-only request/receipt containing accepted transition ID/hash, derived facts and rule/prompt versions. It cannot contain applicable numeric consequences or call `CompleteYear`. The current v1 `Freeze`/`Preview`/`Accept` flow requires Resolving and must remain isolated for legacy runs. Validate new facts against the saved report; never replay an old resolving snapshot to attach prose. Accepted report facts are a derivation from the saved transition; first accepted narrative is a cached commitment keyed to that transition and prompt/model contract. Duplicate calls return the cached narrative; concurrent responses cannot overwrite it. Provider availability, model/prompt revisions and late responses can change; pin/validate receipt identity and ignore stale replies. Adapt chapter save/restore and retry state to separate committed simulation from pending prose, including continuing or fast-forwarding before an older chapter completes. Show harvest, next opening cash and site-linked disruption with details on demand (TN-05/08, CS-01/04); no exact future harvest preview.
4. **Comparison and verification.** Runs in standalone checks plus the actual Chrome game. Comparisons derive from the pinned opening state/rules/exogenous schedule; accepted histories remain commitments. Repeating a manual/reload/fast-forward run must match exactly. Strategy changes affect only endogenous outcomes; another region, new pack or sensitivity setting creates a separately identified run. Compare player and no-new-action with shared conditions and validate geography/connection scope. Preserve other owners' scene work and the separate visual-quality gate.

## Checked and cleared

- Already solved: ledger, history, maturity, condition, reference runoff and offline templates exist; extend them instead of adding a second simulation store.
- Diagnosis/plan link: a larger prompt cannot replace missing drivers and response rules; the proposal adds those before enriching narration.
- Null hypothesis: keeping current rules is a valid scripted demo but cannot satisfy variable weather or crop-specific consequences.
- Smaller alternative: swapping the Year 3/7 constants for random multipliers would be cheap, but still couples unrelated outputs and gives no spatial/causal damage model.
- Acceptance trace: planning -> frozen drivers -> offline resolution -> persisted report -> chapter facts -> next planning and shared-weather comparison. Narration currently blocks acceptance and needs the explicit refactor above.
- Failure/retry/reversibility: version isolation, immutable receipts and old-save retention; no deployment, provider write or rewriting old histories needed for this decision brief.
- Scale/observability: bounded ten years and small site lists; report saved driver/rule IDs, pack version, ledger reconciliation, missing inputs and narration status. Measure transition/replay latency; no claim of a browser pass from C# checks.

## Required disproving cases before calling this simulation ready

- Same seed/actions/rules produce identical results across retry, reload, offline/online prose and manual/fast-forward paths. Invalid/stale requests publish no partial state.
- Different weather schedules can alter output/disruption; a changed unrelated queue cannot change the saved storm or random draws elsewhere.
- A crop/practice comparison under identical conditions reflects configured area, timing and condition; no permanent universal practice premium or double-counted damage.
- A maintained connected swale may reduce a supported disruption; a remote/unbuilt/unfunded one cannot. An extreme or upstream event may still disrupt well-managed land. Test storage recovery and late-season events.
- Damage persists on baseline/no-action farms as well as new projects. Endogenous repair liabilities vary with affected public assets; funded recovery changes next-year condition, while unpaid debt/damage persists. Private crop losses stay separate; opening/closing cash reconcile exactly and receipts never fund already missed service retroactively.
- Baseline farms remain productive unless affected; buildings, farms, solar and wind do not share one weather multiplier. Unsupported reference inputs remain visibly unavailable/illustrative.
- Three terrain choices retain separate geography and saves, with the same disclosed Coon Valley/Wisconsin weather/economic reference. Fixed-storm comparison remains fixed while realized annual events vary. Year 10, no-action comparison and ending all use accepted history.
- Every accepted report includes all three scores/deltas. The financed repeated-data-center/village fixture reveals cumulative resource/service pressures and land displacement under declared rules, rather than assuming benefit per placement. No unsupported adjacency/health penalty is generated.
- Narration cannot introduce unsupported amounts, causes, sites or future certainty. Backend/quota/credit failure automatically shows labelled local text from the same committed report; reload and late responses preserve exact numbers and clear mode. Failed numeric resolution/save remains pending.
- Chrome shows a complete three-year choice -> outcome -> adaptation slice plus a ten-year/reload/ending run. Code checks and visual acceptance remain separate.

## Where the evidence is thin

No site-ready weather station/period was selected or downloaded in this review. No local soil/crop calibration, surveyed drainage/exposure, public repair inventory, response curves or event thresholds were validated. NOAA documents the weather source and FAO/USGS document relevant dependencies; they do not supply this game's coefficients. Data preparation and sensitivity checks are first implementation work. The actual supplied hackpack was not located in the bounded local/Drive/public searches; its suitability remains unassessed. Independent weather/economic calibration for the other terrain choices is no longer required for this slice, but the shared Wisconsin reference must be disclosed. No future climate probability, precise flood loss, calibrated harvest or balance acceptance is claimed.

## Primary research inspected October 25

- [NOAA GHCN-Daily](https://www.ncei.noaa.gov/products/land-based-station/global-historical-climatology-network-daily): quality-controlled historical daily data, variable station coverage and versioned updates.
- [FAO AquaCrop](https://www.fao.org/aquacrop/overview/about-aquacrop/en): crop response to water, field-scale assumptions and limits; informs dependencies rather than justifying the game's coefficients.
- [USGS flood prediction inputs](https://www.usgs.gov/faqs/how-are-floods-predicted): storms, river conditions, soil moisture, snow and basin characteristics matter.
- [USACE CN tables](https://www.hec.usace.army.mil/confluence/hmsdocs/hmstrm/cn-tables): soil group, cover, treatment and hydrologic condition determine tabulated parameter choices.
- [NWS August 2018 event](https://www.weather.gov/arx/aug2818): historical regional precedent, not a frequency distribution or intervention experiment.

## Independent review and verdict

**Verdict: sound with changes incorporated; ready for data preparation and a bounded implementation spike.** Numerical crop/event tuning is not ready to freeze until the declared response curves, exposure and recovery parameters are authored and sensitivity-tested. No simulation implementation or deployment has been performed.

The initial `64384a0` approach received three independent read-only reviews of the premise, player-outcome trace, and execution/retry contract. Their material findings were verified against that code and incorporated above. The subsequent user refinements to shared reference data, scores and visible automatic fallback are requirements updates, not a repeated independent review or new runtime verification:

- **P1, baseline carryover:** `AnnualTown.cs` applies condition updates to assets and rejects baseline-site condition deltas. Added mandatory persistent per-site state and its full serialization/replay path.
- **P1, endogenous repairs:** current repair dollars come from the external driver and paid repair does not restore condition. Added asset-linked liabilities derived from damage, public/private responsibility, funding and next-year recovery timing.
- **P1, narration contract:** current `AnnualChapters.Preview`/session acceptance require Resolving; receipt import also rejects nonempty effects. Added report-only narration v2, isolated v1 compatibility, separate accepted/pending prose state, and late-response/crash checks. New report import/export must validate v2 rather than pass through v1's empty-effects restriction.
- **P1, matched randomness:** different branch run IDs would change draws even with matched weather. Replaced run identity in random keys with a shared scenario seed and stable site/driver/channel keys; branch IDs remain receipt identities only.
- **P2, adaptation scope:** committed sites currently reject redevelopment. Explicitly limited this slice to additional investments, configured rotation and funded recovery; a later practice conversion needs a costed command.

Reviewer limits: read-only source review; no reviewer ran programs or validated scientific coefficients. The primary agent ran the existing legacy-layout full-game suite at the inspected base: 608 assertions passed. No new tests were added for this documentation change; no browser, live provider, weather-data preparation or new-simulation test is claimed.
