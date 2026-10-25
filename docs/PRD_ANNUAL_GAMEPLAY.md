# Terra — play a place forward

> **Adoption decision — October 24, 2026:** The user has now requested that annual decisions, budgets and economic/social/environmental meters become the core game, with the full supplied Actions & Effects catalog. The current [canonical spec](../TERRA_PROJECT_SPEC.md), [implementation plan](../IMPLEMENTATION_PLAN.md), and [action contract](ACTIONS_AND_EFFECTS.md) record that decision. This document is retained as Peter’s source proposal; its original “not approved,” four-family-only V0, optional wider catalog, required LLM consequence engine and optional generated-image passages are historical proposal language, superseded where they differ from those active documents. The core ships with deterministic annual rules and authored outcome text; an LLM is optional, and image generation remains stopped. No runtime completion is implied.

**PRD draft · revised October 24, 2026 · government actions and evidence update · for team review**

> Start with a real landscape. Spend a limited budget to change it. Advance a year, discover the consequences, and adapt. See what your choices leave behind after ten years.

## 1. Decision and relationship to the existing game

This draft describes Peter's annual gameplay proposal. It is **not an implementation record or an approved replacement** for [TERRA_PROJECT_SPEC.md](https://github.com/namanpro47/Terra/blob/e355fa14144a6d0cc802543bc75d01526f61cf1d/TERRA_PROJECT_SPEC.md).

The current canonical spec describes an immediate rural placement puzzle, a deterministic direct-runoff calculation, and no required annual management or runtime AI. This proposal introduces annual turns, ongoing finances, contextual economic/social/environmental outcomes, and an LLM-assisted scenario engine. These are material product and contract changes.

Keep the existing real map, rendering, camera, site eligibility, provenance, and presentation work. Review this proposal with the engine owner before revising the canonical spec, implementation phases, or frozen v1 contracts. This document specifies proposed behavior; it does not implement it or start later gameplay phases.

**Selected setting:** keep the existing rural Wisconsin site near **Coon Valley / Brye Lane / Coon Creek, Vernon County**, displayed as **Meadowbrook Valley**. Meadowbrook Valley is a fictional game name, not a Madison neighborhood. Use county or statewide benchmarks when site-level data is missing, and label Madison/Dane County examples as regional comparisons. No location picker, live world generator, or second map. The player starts in the selected location rather than selecting among multiple levels.

## 2. Job to be done

For young people aged 15–24: **let me try consequential land-use choices in a recognizable place, see benefits and costs unfold, and understand what I would ask about a real proposal.**

The experience should feel like a small landscape strategy game. Learning comes through decisions, visible consequences, experimentation, and replay. Avoid interrupting every turn with prediction questions, confidence sliders, or reflection forms.

The player is a **fictional local-government decision maker** allocating a public land-use budget. Actions can build public assets, fund restoration, lease land, or contribute grants to farmer/developer projects. Each action specifies what government pays for, who owns and operates the result, and which benefits reach residents rather than the treasury. Authority over eligible sites, landowner participation and intergovernmental agreements are explicit scenario assumptions, not claims about powers or permissions at the actual location.

## 3. Inspected implementation baseline

Repository baseline inspected: `e355fa14144a6d0cc802543bc75d01526f61cf1d`. The inventory below describes that checkpoint, not a fresh certification of a teammate's subsequent work.

- Unity **6000.6.6f1**, URP, uGUI/TextMeshPro, browser delivery.
- Real terrain, mapped land-cover context, stream and road centerlines in `worlds/meadowbrook-v1`.
- Ten hypothetical editable sites and two scenario no-edit zones.
- Cultivation, living-margin cultivation, grass restoration and swale treatment offers; preview, placement, replacement and undo accounting.
- Immutable baseline and stable site IDs; budget and illustrative productive capacity.
- Runoff is currently unavailable. A serialized simulation seam and reference fixture exist; they are not an integrated simulator.

**Not currently implemented:** annual progression, ongoing finance, ecological establishment, contextual three-score assessment, LLM service, annual event resolution, ten-year ending, or final AI image.

The map does **not** contain measured yields for individual fields, household demand, cadastral rights, a building inventory, local soil surveys, a routed catchment model, or verified degradation condition. County-level yield and rent benchmarks are available separately in section 5. Source cover is dated; the opening is a data snapshot, not a live survey of the place today. Empty-looking or low-vegetation land is not automatically free or degraded.

## 4. Core gameplay loop

**Inspect → queue changes → review cost → End Year → watch changes → understand consequences → adapt.**

1. Arrive in Meadowbrook Valley with the baseline landscape, a public budget, Year 1, and a short community objective.
2. Inspect a site. See existing use, eligibility, area, relevant context, and uncertainties.
3. Queue several actions. Show a ghost of the proposed change, funded area/capacity, government's upfront contribution, future commitments, completion time, and likely benefit/trade-off. Distinguish a government contribution from total project cost.
4. Edit or undo this year's queue without penalty. Money is reserved in the preview; it is charged once on submission.
5. Press **End Year**. Freeze the submitted action batch and resolve one annual transition.
6. Show establishment/construction and affected places on the map. Present at most three important outcome cards, each answering **what changed · why · who/where it affected**.
7. Start the next year with the accepted state and an itemized fund balance. Build more, maintain, change strategy, or **Wait This Year**.
8. At any point, choose **See My Ten-Year Future**. Confirm that no further new interventions will be made, then process the remaining annual transitions through Year 10 with existing assets, maintenance policy and weather.
9. Show the final map, outcomes over time, a no-new-intervention comparison, an AI-written account and an optional imagined realistic picture. Offer replay and an optional proposal/question card.

Year 10 ends the run. Replay starts a separate run from the original snapshot. Previous years are a committed history; undo applies to unsubmitted choices in the current year, not to already experienced outcomes.

## 5. Updated actions, scale and evidence

**The player funds a specific intervention at a specific place.** Each action records its footprint or service capacity, government contribution, total project cost if known, owner/operator, annual public commitment, completion/establishment time, affected state fields and evidence. Benefits depend on the previous land use, location, demand, weather and maintenance. No action carries a permanent economic/social/environmental 1–5 rating.

### Evidence labels and action recipe

- **Observed/mapped:** geography or reported historical data, with location, source and date. A county average is not a measurement for an individual site.
- **Benchmark:** a regional price, program rate or comparable project. Record its geography, price year, units and inclusions/exclusions. A subsidy is not a full project cost; rent is not a purchase price.
- **Scenario assumption:** eligible land, ownership/consent, public budget, future demand, timelines, effect ranges and scoring weights where evidence is missing.
- **Simulated:** accepted annual outcomes generated under the versioned rules. A plausible outcome is not an observed fact or validated forecast.

Every recipe includes `actionId`, permitted `siteIds`, funded quantity/unit, implementation practice, contribution type, public cost/commitment schedule, total-cost status, owner/operator, operational year, effect drivers and bounds, maintenance conditions, source IDs, evidence level and uncertainty method. Fixed contract rates stay fixed for their term; uncertainty is applied only to specified variables. Recipe/source changes create a new version and do not rewrite an accepted run.

### Proposed V0: four action families

These keep the existing Cultivate / Restore / Manage Water interaction foundation. This is a proposed implementation subset, not confirmation that all updated recipes are supported by the current contracts.

**A. Support cultivation on an eligible site — the chart's traditional farmland action.**

- Government funds defined preparation/infrastructure or leases land; the recipe states which, rather than charging government for all private farm expenses.
- Upfront: pay the configured public contribution and account for displaced cover/production. Land access costs are separate from preparation and annual cultivation costs.
- Over years: operational land contributes illustrative agricultural output and operating/resource demands; weather and management affect outcomes. Farmer sales are community activity, not automatic treasury income.
- Evidence: county rent and yield benchmarks below. The chart's flat **$100K** conversion price remains unverified. Crop choice, preparation cost and site suitability need explicit recipes.

**B. Fund specified farming improvements — the chart's regenerative/rotation action.**

- Offer explicit variants: cover crop, conservation rotation, or an area-reserving living margin. Do not use these names interchangeably or stack subsidies automatically.
- Upfront/annual: reserve the program's public payment for its funded area and term. A living margin removes some cropped area; a seasonal cover crop does not automatically have that same footprint effect.
- Over years: represent establishment, participation, maintenance, production and supported soil/cover effects separately. Improvements do not guarantee higher yields, more jobs or higher income in every year.
- Evidence: Wisconsin cover-crop maximum payment benchmarks of **$35 / $60 / $75 per acre per year** and conservation-rotation maximum payment of **$10 per acre per year**, subject to the relevant practice and program conditions [R2]. These are public assistance benchmarks, not complete production costs or a promise of local eligibility. The chart's **$160K** bundle is not validated.

**C. Fund ground-cover restoration on a scenario-eligible site.**

- Upfront: preparation/establishment contribution, plus acquisition or lease cost only if the scenario requires it. Account for any productive use displaced.
- Over years: appropriate vegetation establishes, with maintenance obligations and possible establishment setbacks. Effects are conditional and develop over time.
- Evidence: mapped cover provides context, not proof of degradation. Cost and establishment/benefit ranges remain authored until a comparable treatment estimate is obtained. Restoration does not imply demolishing homes or planting trees everywhere.

**D. Build a vegetated swale / bounded retention feature.**

- Upfront: public construction cost and occupied footprint; later: upkeep and repair costs.
- Over years: only the recipe's connected area receives bounded treatment effects after operation begins. Condition and available capacity matter.
- Evidence: connections and capacity are currently scenario assumptions, not surveyed drainage. Cost needs a comparable local estimate. No automatic irrigation supply, drinking-water benefit or guarantee of flood prevention.

### Wider catalog from the team's chart — conditional candidates

Keep these in the PRD for discussion. They do not silently expand V0 or imply their original prices/ratings have been validated.

**Low-density housing.** Specify homes, occupied area and whether government builds/owns the homes, supplies infrastructure or contributes financing. Benefits require unmet demand and occupancy; land and service costs depend on the site. Completion precedes housing delivery and any configured receipts. The original **$1M / +$20K annual tax** figures remain unverified.

**High-density housing.** Specify homes, building footprint, infrastructure capacity and funding/ownership arrangement. Evaluate housed households and land consumed alongside public contribution and service obligations. Neither community cohesion nor tax receipts follow automatically from density. The Madison financing example below is a regional contribution benchmark, not a Coon Valley construction quote. The original **$5M / +$150K annual tax** figures remain unverified.

**Public park.** Specify area, access, facilities, public capital cost and annual maintenance. Recreation/access are different outcomes from ecological restoration; biodiversity benefits require a supported habitat treatment and starting condition. The original **$50K** price and **5/5** environmental rating are not established.

**Warehouse / agricultural storage.** Define which facility and its storage area or capacity. A rural crop-storage facility is a different recipe from a distribution warehouse. Outcomes depend on use, occupancy, location and connections. Separate construction employment from ongoing jobs; include any funded roads/services and relevant footprint/traffic assumptions. The original **$2M** price is unverified.

**Data center.** Define electrical capacity, cooling technology, footprint, infrastructure requirements and the government's actual contribution. Jobs, water/electricity demand and local revenue require a supported project scenario. Neither a generic **$3M** price nor a fixed job/water-impact score is supported. Deferred from V0.

**Downtown / village center.** Define a package of shops, services and/or public space with floor area, demand, infrastructure and ownership. Record where its public benefit comes from. “Creates a vibrant community” is an aspiration, not a state update. The original **$4M** price is unverified. Deferred from V0.

**Non-renewable power.** Specify technology, generation capacity, connection and owner/operator before estimating construction, fuel, operating or emissions effects. Electricity delivered, public spending, receipts and community exposure are separate fields. The original **$5M** price and fixed ratings are unverified. Deferred from V0.

**Solar or wind.** Use separate technology recipes with generating capacity, footprint and grid connection; do not treat them as one interchangeable plant. Outcomes depend on siting, generation, demand and the land use displaced. Environmental impacts are conditional; large solar development can affect habitat and water use [R8]. The original **$2M** price and universal **5/5** benefits are unverified. Deferred unless one small, supported energy recipe is deliberately selected.

**Conservation and Indigenous stewardship agreement.** Model a specific, mutually agreed land-return/conservation or stewardship arrangement, with the parties' roles and consent as explicit fictional assumptions. Public costs can include acquisition, agreement delivery and funded management. In an agreement containing a conservation easement, restricted future development is an opportunity cost; it is not an inherent consequence of every land return. Track governance, public access and specified conservation work separately. Ecological outcomes follow the work and conditions, not the identity of the steward. The real Dane County example below supports a scenario pattern, not the chart's **$150K** price or a land-reset effect. Deferred from V0 unless the team authors this fuller context.

**Upgrade wastewater treatment — simplified replacement for “water processing plant.”** Keep one clear purpose: treat a defined wastewater flow/load for connected users. Record upgrade capacity, public capital contribution, ongoing operation/energy, completion time and treatment-performance assumptions. Benefits are limited to the defined service and receiving environment; this does not create drinking-water supply or remove every contaminant. Drinking-water treatment remains a separate, deferred action [R7]. The original **$2.5M** price is unverified pending a comparable rural project.

**Always available:** inspect, wait, maintain funded assets, and revise an unsubmitted plan. Waiting advances operations, commitments, establishment and external conditions. It does not generate a free improvement or pause the world.

### Verified starting benchmarks — not a complete cost book

**Local county reference [R1]:** USDA reports **2025 non-irrigated cropland cash rent of $155/acre/year in Vernon County**, compared with **$233 in Dane County**. Use Vernon for a county-level land-access reference at the selected setting, and only when the government action actually leases land. Neither figure includes cultivation costs or buys the land.

**County yield reference [R1]:** Vernon County corn-for-grain yields were **140 bushels/acre in 2023** and **177.9 in 2024**. These describe historical harvested acreage across the county, not the selected fields, every crop, or an effect of a particular conservation practice. They can anchor an illustrative output scale; two years do not define a probability distribution, and newly developed land cannot simply inherit the county yield.

**Statewide program references [R2]:** cover-crop and rotation payment rates above are published maximum assistance benchmarks with practice/program conditions. For example, a configured qualifying program at **$60/acre/year over 100 participating acres** would commit **$6,000/year** in public payments, excluding administration and other costs. The 100 acres are an arithmetic example, not a claim about the current editable sites or actual farmer participation. Eligibility, funding availability and payment term are scenario inputs to verify before implementation.

**Regional housing reference [R3]:** Dane County's October 6, 2025 announcement allocated **$4M toward 80 proposed homes** at United Residences in Madison: **$50,000/home in county financing**, calculated from that announcement. This is not total development cost, a completed-project invoice, a generic per-home entitlement, or a rural construction quote.

**Regional conservation agreement [R4]:** Dane County's May 15, 2026 announcement described an approved **$6.41M purchase of approximately 165 acres** for conservation protection and intended donation to the Ho-Chunk Nation. The agreement includes a permanent conservation easement restricting development and preserving public access. It is a specific land-acquisition/partnership example, not a unit price to transfer to ordinary Vernon County farmland.

All benchmarks retain their original year and scope. Record any inflation adjustment and its source before comparing them in a common price year. Costs for restoration, swales, parks, rural housing construction, warehouses, data centers, village centers, power facilities and wastewater upgrades remain **unverified**; so do the chart's annual tax figures and universal impact ratings. Those gaps must remain visible in any demo configuration.

## 6. One money budget, three impact summaries

### Public budget

Use **USD with an explicit price year and evidence label** for the government ledger. Starting appropriation and future appropriations are authored scenario settings, not measured local spending authority. A compact $K display is acceptable; optional game credits must show their conversion and cannot turn an unverified cost into a sourced price.

`next opening balance = current opening balance − upfront public payments − program payments due − public operating/repair costs + scheduled appropriations/grants + configured public receipts`

- Separate total project cost, government's contribution and third-party financing. A subsidy does not purchase all of an asset's output or transfer ownership automatically.
- Reserve known multi-year commitments before discretionary spending. Show current cash and committed future spending. On End Year, pay the installment due once; do not charge the full term and each annual installment again.
- Publish payment, appropriation, operation and receipt timing before submission. Receipts begin only when their configured prerequisites are met. Farmer sales, household income, employment and developer revenue are community outcomes; only specifically configured public receipts enter the ledger. The chart's tax bonuses need verification or explicit fictional labels.
- V0 uses no borrowing. Disclose a priority policy for an unexpected shortfall: defer new discretionary spending, seek an explicitly authored emergency allocation, or suspend optional services where allowed. Required obligations cannot disappear, and a failed payment cannot leave its benefit fully active. Record unresolved commitments and effects without inventing money or silently deleting homes.
- During fast-forward, preserve the same commitment, maintenance and shortfall rules.

### Economic · social · environmental

Keep three compact summaries, with contextual measures beneath them. They are **game indicators**, not measured local welfare or ecosystem forecasts. Numeric state remains necessary; the improvement over the old chart is explaining it in context.

- **Economic:** illustrative productive output, operating burden and weather-related loss. Explain the difference between community outcomes and the government balance. A profitable private business does not necessarily improve public finances.
- **Social:** progress toward a defined community need and the number of scenario households/sites affected by disruption. Do not invent real residents, employment or health outcomes.
- **Environmental:** cover/condition, connected retention state and exposure-related effects supported by this scenario. Do not claim biodiversity, soil carbon or drinking-water improvements merely because vegetation increased.

For a compact 0–100 display, define a versioned rubric with named components, units, ranges and weights in scenario data. The engine derives summaries from accepted component state. The LLM does not freely award “environment +15.” Show raw values and reasons on inspection. Do not combine the three into a single morality score.

Before balancing, select the social objective: **working draft = maintain an illustrative community production/service target while avoiding repeated disruption**. Housing supply becomes a component only if housing is added.

## 7. What the LLM does

**Use an LLM-assisted educational scenario engine, without building a separate calibrated crop or flood predictor for V0.** This still needs a small explicit game model: legal actions, accounting, persistent state, time, effect bounds and event rules.

Structured Outputs can enforce a response shape; they do not establish that its values or causal claims are correct. OpenAI explicitly documents that structured outputs can contain mistakes and that refusals require handling. [Official Structured Outputs guide](https://developers.openai.com/api/docs/guides/structured-outputs).

### Engine owns the facts of the run

Site IDs and areas · current uses/assets · construction age · action eligibility · costs and cash · year/revision · event schedule · rules and bounds · accepted annual history · score calculations.

### LLM proposes contextual consequences

Given a compact state snapshot, submitted actions, lifecycle facts, external events, and an approved causal reference pack, return bounded changes to allowed condition/output components plus short explanations. Each consequence identifies affected site IDs, causal driver IDs, who benefits or bears a cost, and whether its basis is sourced, authored or uncertain.

The LLM cannot create new sites, modify ownership, invent external weather, set its own prices, edit the clock, or rewrite the ledger. It cannot invent measured yields, flood depths or calibrated probabilities.

### Annual transition contract — proposed v2, not the frozen v1 seam

**Input:** contract/rules/prompt versions; run ID; year; expected state revision; input hash; site states; accepted action batch; engine-computed lifecycle/finance facts; saved external events; permitted effect fields and ranges; vetted reference IDs; prior accepted consequences still in force.

**Output:** matching identifiers/version; a list of proposed effects `{siteId, field, delta, driverIds, basis, uncertaintyNote}`; contextual event consequences; up to three outcome cards; assumptions and unavailable quantities. Field names and basis labels use fixed enums. Unavailable quantities remain unavailable, never fabricated zeroes.

**Application:** check the schema, identifiers, finite values, ranges, source/driver references, lifecycle timing, spatial connections and cross-field constraints. Compute derived values and score changes in the engine. Accept all changes atomically or accept none. A sensible-looking JSON object alone is insufficient.

For invalid semantic output, permit at most two repair attempts with diagnostics. On continued failure, timeout, refusal or incomplete response, retain the current year and offer retry or an explicitly labelled offline demo path. Do not silently invent a successful year.

Key every transition by run/year/revision/action batch. Save the first accepted response and its resulting state. Retries, double-clicks and reloads reuse it; they cannot reroll weather or charge the player twice. API credentials stay in a small server-side service, never the Unity browser build. This server is new work; static hosting alone does not provide it.

## 8. Uncertainty and the flooding example

**Yes, wetland loss can matter.** Wetlands can temporarily store water and vegetation can slow its movement. Their effect depends on location, available storage, prior saturation and the storm. They reduce some flood impacts; their presence does not prevent every flood. [EPA explanation](https://www.epa.gov/wetlands/why-are-wetlands-important); [Washington Ecology on the conditions and limits](https://ecology.wa.gov/water-shorelines/wetlands/education-training/functions-values-of-wetlands).

For this map, the current contributing groups are authored, not a delineated catchment. Therefore V0 may show a **fictional heavy-rain disruption scenario**, with explicitly authored exposure and protection assumptions. It must not claim that an identified real farm will flood, draw a precise inundation boundary, or attribute a specific flood solely to missing vegetation.

### Uncertain outcomes with consistent rules

Separate **real-world variability** from **missing knowledge**. Weather varies; uncertainty about a treatment's effect size is a different issue. Do not convert both into an LLM's arbitrary surprise event.

1. **Weather input:** use nearby quality-checked NOAA daily observations to build versioned historical season/year blocks [R5]. Check station coverage, distance, missing days and period. Sample coherent blocks rather than independent daily rainfall draws so dry spells and wet sequences remain meaningful. Record the sampled historical period and run seed. This is a proposed method; a site-ready weather series and sampler have not yet been prepared.
2. **Economic variation:** use multi-year observed price/cost series or genuinely comparable project evidence where available. A single project cost is a benchmark, not a distribution. Keep contracted government payment rates fixed within their term. Disclose any authored bid-overrun range or price scenario.
3. **Response to conditions:** accepted effects depend on funded/participating area, implementation, weather, site context, age and maintenance within the allowed causal rules. Do not add a fixed yield premium because a treatment is called regenerative, or a universal biodiversity bonus for a park.
4. **Uncertain parameters:** document a plausible range and source level for each effect. If evidence is insufficient, use labelled low/central/high sensitivity scenarios or an authored range without calling it an empirical probability. Preserve correlations between effects sharing the same driver; a wet year cannot independently become a dry year for the narrative.
5. **Persistence:** store sampled drivers and the accepted annual transition. New runs may differ; retrying the same committed year returns the same result. LLM output variation is not the source or calibration of weather probabilities, prices, land area or money.

A curated weather/event schedule may remain an explicitly labelled offline demo fallback. Player choices change vulnerability and recovery within scenario bounds; they do not summon a storm as punishment.

**Observed local precedent [R6]:** the National Weather Service recorded severe flooding and evacuations in Coon Valley on **August 28, 2018**, along with impassable roads. This supports including heavy-rain disruption scenarios. It does not establish an annual probability, validate a flood footprint for our sites, or show how much a proposed intervention would have prevented. History-based sampling is a scenario method, not a forecast of future climate trends.

Use the same external weather and background trends for the player's run and the no-new-intervention comparison. Compare the interventions under those shared conditions; two different LLM stories are not evidence of an intervention effect. For V0, precompute the no-action trajectory for each supported demo seed using the same validator and rules, then freeze it. Label comparison results illustrative and test their sensitivity before presenting stronger claims.

## 9. Worked example — invest, discover, adapt

**A sourced-rate accounting example, with explicitly authored scope and future conditions.** Starting budget, areas, program availability, farmer participation and timing below are hypothetical. They are not measured conditions or approved programs at the selected sites. Validate actual editable areas before turning this fixture into a map scenario.

**Year 1 — make the plan.** The fictional government has **$10,000**. Queue a qualifying single-species cover-crop payment on **10 acres at $60/acre** (**$600**) and a qualifying multi-species payment on a separate **5 acres at $75/acre** (**$375**), using the assistance benchmarks in [R2]. The queue reserves **$975 for the first year**. For an authored three-year agreement, show the full **$2,925 commitment**, of which **$1,950 remains due after that first installment**; the example term is a scenario choice, subject to program conditions. End Year pays $975 once. Cash becomes **$9,025 before any other configured public spending or receipts**. It is not all freely spendable while future commitments remain. Administration and other expenditures must be configured separately; they are not assumed costless in the final scenario.

**Year 2 — live with it.** An accepted sampled weather block contains heavy rain. The government pays the next committed installment when due; private harvest losses do not automatically debit that same ledger. Establishment/condition and exposure affect illustrative consequences within the authored response rules. A card explains the actual site and weather drivers, rather than declaring a universal crop-yield or flood-prevention benefit.

**Year 3 — respond.** Inspect the outcome and remaining obligations. Maintain or extend a supported program, save money, or queue a separately costed connected water-management or restoration project. Any unsourced construction estimate stays visibly provisional. The player sees public spending, community benefits and opportunity costs before choosing.

**Year 10 — see the place you shaped.** Compare land uses, cover/condition, public cash and obligations, illustrative output/services and disruptions with the opening and frozen same-weather no-new-intervention trajectory. Narrative and image follow accepted history. The account can explain an illustrative scenario effect; it cannot claim a real flood was prevented.

## 10. Ending and emotional payoff

The final map is the primary result. Show recognizable before/after views, three contextual outcome summaries, the biggest benefit and trade-off, and a short year-by-year history.

Generate the account from accepted state and event history only. For the optional realistic image, use the actual final camera/layout as a reference and include establishment age, cover, buildings if present, and recorded damage/recovery. Label it **“An imagined illustration of your simulated future.”** Never add a thriving forest, new buildings, or clear water that the game state does not support. Use a renderer capture plus text when image generation fails.

After the payoff, offer one optional reflection question and a saved proposal/question card. External participation links must be verified; the game does not submit comments or contact anyone automatically. Follow-up reminders, surveys and workshop administration are outside V0.

## 11. Acceptance criteria

- A new player can inspect, queue at least two actions, undo one, and end a year without a facilitator explaining every control.
- One complete ten-year run and fast-forward both work; a no-action year still advances existing assets, finances and events.
- Money reconciles each year; invalid or repeated requests change neither money nor time. Reload restores the accepted history.
- Establishment and completion timing affect both appearance and outcomes. A remote treatment cannot protect an unrelated site.
- Every impact card points to accepted state changes and disclosed assumptions. Missing data is visible.
- Every action's public price identifies funded quantity/unit, source year, contribution versus total cost, annual commitments, owner/operator and evidence level. Unsupported prices or tax receipts are not labelled local facts.
- Benchmark checks reproduce the source arithmetic; assistance payments are not confused with full project costs, land rent with acquisition, or county yields with site measurements.
- Test the same committed year twice, different saved seeds, and paired player/no-action conditions. Fixed rates stay fixed; shared weather drivers remain consistent. Any authored uncertainty is labelled, and no empirical probability is claimed from two yield years or one historic flood.
- At least two materially different strategies remain playable; no action is a universal improvement across cost, land and all outcomes.
- The final comparison uses the same saved external conditions, and image/text agree with the final state.
- Test malformed and semantically wrong AI output, unknown site IDs, duplicate year requests, refusal, timeout, missing credentials and offline fallback.
- Run a small repeated-input evaluation before tuning: check inconsistent directions, delayed effects, unsupported causal claims and rule violations. Record failures; a valid schema is not a simulation-quality pass.
- Inspect the actual Unity Web build in Chrome at 1280×720 and 1440×900, including loading, annual resolution, failure/retry and ending. Code checks do not certify visual quality.
- In a small youth/novice pilot, observe whether players can name one benefit, one cost and one uncertainty, and want to try another strategy. Report comprehension and replay observations separately from claims of real behavior change.

## 12. Integration order and unresolved decisions

**First handoff:** retain the clarified government-player role and existing rural Wisconsin setting; agree the annual product change, proposed four-family V0 subset and three-score rubric with the engine owner. Confirm that the wider catalog is a backlog rather than an automatic scope expansion. Keep existing presentation work usable. Reference patterns: TN-01/03/05/08 and CS-01/04 in [DESIGN_REFERENCE.md](https://github.com/namanpro47/Terra/blob/e355fa14144a6d0cc802543bc75d01526f61cf1d/docs/DESIGN_REFERENCE.md): landscape first, compact HUD, useful previews, satisfying ending and contextual teaching. This PRD deliberately changes the reference's immediate-only time-control guidance; it does not certify any new UI.

**Then:** version annual state and action recipes → implement a recorded/offline three-year slice → add server LLM adapter and validation → complete ten-year continuation/comparison → ending narrative/image → full browser and novice verification. Retain the existing Phase 2A presentation gate; revise later implementation phases explicitly after direction is agreed.

Do not overload `SimulationInput`/`SimulationOutput` v1: those describe a fixed-storm runoff seam. Introduce separate annual contracts and an adapter from the current WorldPack. Preserve immutable geography and stable IDs. Current undo/refund behavior describes a reversible proposal, so it cannot be reused unchanged as a historical spending ledger.

**Team decisions still open:** social objective/rubric; public appropriations, program terms and commitment/shortfall policy; verified cost recipes for construction/restoration; whether a housing or service action earns V0 scope; supported effect ranges; historical-weather station/period and sampler; provider/model and API cost/latency budget; save persistence; accepted offline fallback. USD and the government role are the revised draft defaults. Proposed service target: a normal year resolves within 10 seconds, with visible progress and a retry state after a bounded timeout; measure before committing to that target.

**Not implemented by this document update:** annual runtime behavior, model calibration, downloaded weather/cost series, new GIS layers, a new location, public deployment or validated real crop-yield/flood predictions. The sourced references improve the inputs; the remaining uncertainty and cost gaps are still explicit.

## References and status boundaries

- [Existing canonical product specification](https://github.com/namanpro47/Terra/blob/e355fa14144a6d0cc802543bc75d01526f61cf1d/TERRA_PROJECT_SPEC.md), [runtime contracts](https://github.com/namanpro47/Terra/blob/e355fa14144a6d0cc802543bc75d01526f61cf1d/docs/runtime-contracts.md), [geography and data gaps](https://github.com/namanpro47/Terra/blob/e355fa14144a6d0cc802543bc75d01526f61cf1d/docs/geography.md), [implementation evidence](https://github.com/namanpro47/Terra/blob/e355fa14144a6d0cc802543bc75d01526f61cf1d/docs/PHASE_STATUS.md).
- [Gameplay FigJam](https://www.figma.com/board/YIf37AibaJxqrrGPYpyl2T?node-id=68-378).
- [Challenge page supplied by Peter](https://hackforsocialimpact.notion.site/Engaging-Youth-in-Sustainable-Land-Use-Decisions-4002cb772f1e80649effe9041c2957bd). Requirements here use the supplied screenshot/context; this draft does not claim a fresh read of inaccessible page content.
- The latest action chart and Peter's annual-loop description inform this proposal. They are design inputs, not validated economic/environmental datasets.

### Evidence register for this revision — checked October 24, 2026

- **R1 — USDA NASS, Wisconsin 2025 Agricultural Statistics:** [official bulletin](https://www.nass.usda.gov/Statistics_by_State/Wisconsin/Publications/Annual_Statistical_Bulletin/2025AgStats_WI.pdf). Printed page 5: county 2024/2025 cash rents; printed page 19: county 2023/2024 corn-for-grain yields. Historic county averages; not parcel forecasts or full farming costs.
- **R2 — Wisconsin DATCP conservation practice rates:** [practice-specific information](https://datcp.wi.gov/Pages/ATCP-50-Update-Practice-Specific-Information.aspx) and [April 2024 partner presentation](https://datcp.wi.gov/Documents2/CF2024PartnerMtgs.pdf), maximum-rate section. Maximum payment benchmarks with practice/program conditions; not universally available grants or complete implementation costs. The information page was searchable but its direct fetch failed during research; corroborating official presentation/county material supports the rate types. Confirm current eligibility and terms when implementing a live program analogue.
- **R3 — Dane County, October 6, 2025:** [housing awards](https://link.countyofdane.com/PressDetail/11715), United Residences project D. $4M county financing for 80 proposed homes in Madison. A regional public-contribution comparison, not a rural construction-cost estimate.
- **R4 — Dane County, May 15, 2026:** [Herro property conservation partnership](https://zoo-donations.countyofdane.com/PressDetail/11877). Approved $6.41M purchase of approximately 165 acres, planned protection and donation, with a conservation easement. A particular agreement and property; not a generic land-return price or ecological score.
- **R5 — NOAA NCEI:** [GHCN-Daily](https://www.ncei.noaa.gov/products/land-based-station/global-historical-climatology-network-daily) and [Climate Data Online](https://www.ncei.noaa.gov/cdo-web/). Sources for historical daily weather; no selected/validated site series or probability calibration is claimed yet.
- **R6 — National Weather Service La Crosse:** [August 27–28, 2018 event summary](https://www.weather.gov/arx/aug2818). Records severe flooding/evacuations in Coon Valley. A historical event, not a future annual event probability or intervention-effect estimate.
- **R7 — Water-service scope:** [Washington Ecology wastewater overview](https://ecology.wa.gov/water-shorelines/water-quality/wastewater) and [CDC drinking-water treatment explanation](https://www.cdc.gov/drinking-water/about/how-water-treatment-works.html). Supports separating service purposes; supplies no local plant cost/capacity or guaranteed contaminant-removal rate.
- **R8 — U.S. EIA:** [solar energy and the environment](https://www.eia.gov/energyexplained/solar/solar-energy-and-the-environment.php). Supports technology/siting-dependent land, habitat and water considerations; not a local generation, cost or impact forecast.
