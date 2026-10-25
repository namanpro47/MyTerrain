# Actions & Effects — annual gameplay contract

**Decision date:** October 24, 2026. **Status:** approved product direction; recipe design and integration requirements, not shipped gameplay. Read with [the canonical spec](../TERRA_PROJECT_SPEC.md) and [delivery plan](../IMPLEMENTATION_PLAN.md). Source: the user's supplied Actions & Effects screenshot and transcription, plus [Peter's annual PRD](PRD_ANNUAL_GAMEPLAY.md). This document preserves the requested numbers; it does not validate them as real project quotes or outcome forecasts.

## The player's decision

**Inspect → queue funded actions → review budget and expected effects → End Year → see the landscape and three meters change → adapt.** Repeat through Year 10. Money, land, time, maintenance and community demand constrain choices. Buildings and land treatments use this same loop. All twelve chart rows are required full-game catalog coverage, not a deferred wishlist. Restoration and connected swales also remain as existing supporting actions.

The player allocates a fictional public budget in Meadowbrook Valley. Proposed development is hypothetical on the existing rural AOI; no city map or actual development approval is implied. Each recipe says whether the public pays for construction, a contribution, an agreement or a grant. Private income, public receipts, and community benefits are separate.

## Source table — preserve the design input

Prices are **provisional scenario USD in a 2026 price frame**, not researched local construction costs. The source did not specify a price year; 2026 is the game's authored frame. A row price buys one explicitly defined project bundle, not one arbitrary polygon or square metre. Bundle quantity/footprint must be frozen before an action is enabled. The ratings are the team's intended comparative profile on a 1–5 scale, including half points; they are not annual meter increments.

| ID / action | Upfront public budget input | Annual public receipt input | Economic profile | Social profile | Environmental profile |
|---|---:|---:|---|---|---|
| `housing-low` — Low-density residential | $1,000,000 | $20,000, tentative tax assumption | 3/5 · some population and taxes | 2.5/5 · homes | 1.5/5 · more land consumed per home |
| `farmland-traditional` — Traditional farmland | $100,000 | Unspecified | 3/5 · jobs and food production | 3/5 · local supply | 2/5 · soil/water pressures |
| `farmland-regenerative` — Regenerative farming / rotation | $160,000 | Unspecified | 4/5 · intended longer-term livelihood/output benefit | 4/5 · food and community stability | 4.5/5 · intended soil/ecosystem benefit |
| `public-park` — Public park | $50,000 | Unspecified | 1/5 · little direct production | 4/5 · recreation and community access | 5/5 · intended habitat benefit |
| `housing-high` — High-density residential | $5,000,000 | $150,000, tentative tax assumption | 4/5 · population and receipts | 4/5 · more homes per footprint | 2/5 · resource demand despite less land per home |
| `warehouse` — Warehouse | $2,000,000 | Unspecified | 2.5/5 · local jobs | 2/5 · job security, limited direct services | 2/5 · sealing and traffic |
| `data-center` — Data center | $3,000,000 | Unspecified | 4/5 · intended skilled employment/income | 2/5 · limited direct community access | 1.5/5 · electricity and water demand |
| `downtown-center` — Downtown / village center | $4,000,000 | Unspecified | 4.5/5 · commerce | 4/5 · retail/services and gathering space | 2/5 · sealing, runoff and heat pressure |
| `power-nonrenewable` — Non-renewable power plant | $5,000,000 | Unspecified | 4.5/5 · jobs and energy | 1/5 · power with pollution burden | 1/5 · emissions/pollution pressure |
| `power-renewable` — Wind or solar plant | **$3,000,000** | Unspecified | 5/5 · intended jobs and energy affordability | 4.5/5 · health/resilience intent | 5/5 · low-emissions intent |
| `stewardship-agreement` — Indigenous stewardship / land back / land reset | $150,000 | Unspecified | 3/5 · local resource economy | 4.5/5 · community authority and leadership | 5/5 · intended habitat restoration |
| `water-processing` — Water processing plant | $2,500,000 | Unspecified | 3/5 · reliable utilities | 5/5 · clean-water service intent | 4.5/5 · wastewater/pollution control intent |

**Corrections and interpretation:** the latest user table sets renewables to **$3M**; Peter's source PRD mentions an older $2M figure. Use $3M. Preserve the tentative status of housing taxes. Unspecified receipts are not an assumption of zero costs or free operation; a recipe must explicitly choose and label receipts and upkeep. A score of 1/5 for a park's economic productivity means little direct contribution, not necessarily economic damage. Regeneration, density, data centers and renewables do not guarantee the benefits stated in the source prose. Keep those as design hypotheses, then express conditional effects and opportunity costs in the game.

## What each action must actually do

These are required rule channels and explanation obligations, not measured claims about this location. A recipe needs explicit bounded numbers before runtime activation. Unsupported science channels must be labelled **illustrative scenario pressure/condition**, not measured pollution, health, biodiversity or yield.

| Action | Effect drivers / trade-off | Required eligibility and interpretation |
|---|---|---|
| Low-density housing | Completed occupied homes serve unmet housing demand; consume more land per configured household than the high-density recipe; add service/water/energy load and configured public receipts | Define number of homes per $1M bundle, occupancy assumption, service cost, access, footprint and completion. The existing single-home art is not automatically the entire funded bundle. |
| Traditional farmland | Productive area and maintained operation support output; displace previous use/cover; draw resource demand and scenario soil-condition pressure | Explicit area, practice, baseline displacement and weather response. No harvested-tonnage claims from decorative crop rows. |
| Regenerative farming | A specified rotation/practice establishes over time; has higher initial cost and conditional soil/cover/resource effects | Choose a named recipe; living margin, cover crop and rotation are separate variants. Do not equate rotation with a guaranteed 25% land loss, income premium or free additional subsidy. |
| Park | Accessible recreation capacity meets a service need; maintained vegetated area can help cover/condition; upkeep and land compete with other uses | Separate paths/facilities from habitat treatment. Converting existing good habitat to a park cannot create habitat again. |
| High-density housing | More configured homes per occupied area; services/resource loads, construction delay and higher capital cost | Define the funded bundle and households; enough actual site area and services. No automatic cohesion or tax windfall. |
| Warehouse | Occupied storage/distribution capacity and sustained jobs; sealing, trips, energy and displaced land | Name the facility. Existing agricultural-storage art supports a rural warehouse variant only; do not silently claim a distribution hub. |
| Data center | Configured occupied capacity supports a limited job contribution; substantial configured power/cooling requirements and land take | Choose capacity/cooling and dependencies. No fictional high employment to erase its resource trade-off; employment is separate from income and tax. |
| Village center | Occupied shops/services serve unmet local demand; create gathering capacity and jobs; use land/resources | Rural service cluster on this map, not a downtown skyline or new city. Define shop/service bundle and residents served. |
| Non-renewable power | Operational generation serves demand; fuel/upkeep and authored emissions/exposure pressure reduce other outcomes | Choose a technology, capacity and connection. Do not translate a pressure index into diagnosed illness or a real emissions inventory. |
| Wind / solar | Operational generation meets demand with a different pressure profile; construction, land, variability and connections constrain benefit | Separate wind and solar variants within the one chart family. No blanket habitat-safe label. Use metric footprints and clearance; don't shrink the turbine to fit. |
| Stewardship agreement | Fund agreed governance/land-return and specified management work; benefits follow participation and work over time | Fictional consenting parties and agreed authority are prerequisites. $150K is a program contribution, not a purchase price for sovereignty or land. No automatic demolition, population removal, cultural stereotype or instant ecosystem reset. Any conservation restriction is explicit agreement content. |
| Water processing | Connected treatment capacity serves a defined need after commissioning; operation consumes energy and budget; bounded treated load affects a disclosed pressure measure | Default implementation is a wastewater-treatment upgrade, matching Peter's existing art/PRD. The source table combines drinking water and wastewater: retain its intent, but label the actual service correctly. Drinking-water purification requires a separate explicit variant, not an implied bonus. |

Existing `restore-grass` and `water-swale` remain supporting recipes. Their prices need a separate explicit scenario choice; do not assign them the stewardship/plant price or infer one from legacy credits. A swale retains connected runoff; a wastewater plant treats connected wastewater. They cannot substitute for each other.

## Budget and annual timing

Use Peter's annual ledger as the single financial authority. Money uses integer cents; never use a rounded `$K` label for arithmetic. Proposed initial tuning fixture: **$10M opening public cash and $1M closing appropriation per year**. Both are fictional design defaults to test in Phase 3, not approved local budgets or proof of balance. Pin a schedule including Year 10; show closing funds and remaining liabilities even though there is no Year 11. This replaces the legacy credit conversion as the intended scenario.

Before queuing, show cash, existing future commitments, the proposed project's full public commitment, and the spendable remainder. Reserve known installments and finite upkeep terms; do not rely on future unreceived appropriations or tax receipts. A $5M card may require more than $5M available once upkeep is included.

Use the foundation's explicit order unless deliberately versioned:

1. At End Year, pay submitted upfront contributions once.
2. Pay carried arrears, then due program/operation installments in stable asset/site order.
3. Pay authored required repairs as funds allow; record unpaid obligations.
4. Add the closing appropriation and eligible operational public receipts.
5. Carry cash, arrears and remaining commitments to the next planning year.

No borrowing in the first release. Insufficient funds block a discretionary proposal. An unexpected shortfall records arrears and suspends the affected funded service under a disclosed policy; it cannot quietly invent cash, delete debt or leave every benefit active. Closing receipts do not retroactively fund operations missed earlier that year.

Housing receipts begin only in an accepted operational year with the configured occupancy and service prerequisites. The $20K/$150K amounts are provisional annual receipts for each **defined bundle**, not returns on every visible house mesh. No other private output becomes treasury cash automatically. Show upfront cost, ongoing expenditure and income separately; a high Economic meter does not mean a full treasury.

Current-year queue edits, move/replace/remove and undo/redo release or recompute reservations. Accepted past years are immutable. Removing a built asset later is a separately costed conversion/decommissioning action, never a historic full refund. The first release may disable such conversion explicitly while allowing new actions on uncommitted sites; this limitation must appear in the UI. Fast-forward and Wait use exactly the same financial rules.

## Three meters with inspectable reasons

Keep **Economic, Social, Environmental** as three equal-status 0–100 game indicators, with current value, previous accepted year delta, and a short causal explanation. Budget and Year are separate controls. No combined moral score. A raw measure is unavailable when unsupported, not zero.

The source /5 ratings inform recipe priorities and balance review; they must not be summed, multiplied by 20 or awarded every year. A completed park contributes service/cover while functioning, not “+5 environment” forever. Recompute from current accepted state; do not accumulate stock benefits repeatedly. Diminishing returns come from limited unmet demand, footprint and capacity, with age, condition, operation and events applied before scoring.

**Proposed target rubric v2** (authored game balance; freeze numerical targets and definitions with Phase 4 recipes):

| Meter | Normalized components and weights | Inspectable raw facts |
|---|---|---|
| Economic | 40% productive-output need met + 30% sustained local-job target met + 30% due operating obligations funded | Illustrative output units, scenario jobs, operating payments/due. Construction jobs are separate temporary facts; no invented real local income. |
| Social | 40% housing need met + 40% configured essential/recreation service needs met + 20% disruption resilience | Scenario occupied households, unmet demand, separately capacity-capped energy/water/recreation/access components, affected-site fraction. Governance/consent is an eligibility fact, not purchasable points. |
| Environmental | 40% cover/condition target + 25% connected runoff management target + 20% low-emissions supply target + 15% water-demand/treatment pressure target | Vegetated area and condition, generated/retained/discharged m³ for the same reference storm, supplied energy mix, demand and connected treatment capacity. Non-runoff pressure indicators are authored game abstractions. |

For each beneficial component `s = clamp(value / target, 0, 1)`; for a pressure component `s = 1 - clamp(pressure / limit, 0, 1)`. Define targets/limits as positive, document units, scope and numerator, then `meter = 100 × sum(weight × s)`. For obligations, use actual funded/due fraction; no dues is 1, arrears still remain visible and count toward their due payments. Disruption resilience is `1 - disruptedEligibleSites / eligibleSites`, with a fixed nonzero eligible-site denominator. Freeze service subweights in scenario data; do not let housing compensate for missing drinking-water capacity or claim a wastewater facility supplies drinking water.

All declared components must have an implemented input before enabling a rubric; do not silently renormalize missing components or plug in fake zeroes. Before full-catalog integration, use a visibly labelled **starter rubric v1** from Peter's foundation (production/funded operation; service/disruption; vegetation/configured retention capacity). It is a limited prototype rubric, not sufficient to evaluate power plants, data centers or housing. Show rubric version and scope in details; compare only like versions. The v2 migration must add the required primitive measures, update copies/validation/save replay and pin the new definition. It must not reinterpret accepted v1 histories.

Preview separates **certain reservation/footprint changes**, **expected operational effects/timing** and **conditional event effects**. Queueing does not move accepted meters. Show ghost/preview values separately, and explain why weather-conditioned final outcomes can differ. After resolution, explain at most three important changes with affected sites and rule IDs. Inspecting a meter opens its component breakdown, assumptions and year history.

## Runtime recipe contract and integration map

Before activation every site/action offer must declare:

- Stable family, variant, site and visual IDs; eligible sites/conditions and rejection reasons; footprint, access and connection requirements; baseline displacement.
- Funded quantity/unit and what the public pays for; owner/operator; source/assumption labels; total project cost known/unknown separately from the public contribution.
- Exact upfront cents, installments/term, annual operation and receipt schedules, operational prerequisites, construction/establishment years, lifetime and shortfall behavior. Explicit zero is allowed only as a documented assumption; absent is not zero.
- Effect channels, units, bounded parameter values, timing, demand saturation, event response and explanation IDs. One source of truth feeds preview, accepted state and summary.
- Rules/catalog/rubric version, price year, source IDs, visual recipe and evidence status. Pin the definition to a run and reject incompatible saves rather than changing past results.

Peter's inspected `TownActionDefinition` already covers costs, annual commitments/receipts, lifecycle, quantity, owner, evidence and simple production/service/cover/retention fields. It **does not yet cover** full footprint eligibility, households, jobs, energy/water demand, emissions pressure, service subtypes or this v2 rubric. Extend narrowly with an explicit annual schema revision; keep fixed-storm `SimulationInput`/`SimulationOutput` and geographic WorldPack v1 intact. Do not turn the /5 table into those missing physical inputs.

| Gameplay ID | Existing remote art ID / intended adapter |
|---|---|
| `housing-low`, `housing-high` | `low-density-housing`, `high-density-housing` |
| `farmland-traditional`, `farmland-regenerative` | `cultivation`; `conservation-rotation` (cover-crop/living-margin stay separate variants) |
| `public-park`, `data-center` | same IDs |
| `warehouse`, `downtown-center` | `agricultural-storage`, `village-center` |
| `power-nonrenewable`, `power-renewable` | `non-renewable-power`; `wind` or `solar` |
| `stewardship-agreement`, `water-processing` | same stewardship ID; `wastewater-upgrade` |
| Supporting restoration / water | `ground-cover-restoration`, `vegetated-swale` |

Verify exact art IDs against the imported catalog before binding; this table is a proposed adapter, not a claim it is compiled. Remote kit checkpoint `1bcc254` has 17 metric assets and matching icons. Reuse/evaluate these before making replacements, but independent gallery validation does not establish in-scene visual acceptance. Match the actual funded quantity and occupied footprint, not just a prefab pivot.

## Delivery and acceptance

Phase 3 proves a **three-year playable browser slice** with queue, reservations, End Year, operational visuals, three starter meters, report, Wait and restore. Include both farming strategies, supporting restoration/swale, park and low-density housing; housing/park require their actual primitive services rather than pretending crop output measures them. Phase 4 integrates all remaining chart actions, the complete v2 rubric, spatial runoff adapter and the full recipe catalog. These are staged deliveries of one game, not extra levels. Phase 5 balances ten years and fast-forward; Phase 6 supplies the paired ending and replay.

Acceptance evidence must cover:

1. All 12 rows map to an enabled, documented recipe family; wind/solar variants and all cost overrides are visible. Required unavailable actions are unfinished scope, not silently removed.
2. Every family has at least one genuinely eligible placement in the same AOI, and clear invalid examples. Check whole rotated footprints, slopes, protected land and service connections. Author additional hypothetical sub-sites within the same AOI if necessary; preserve baseline analytical area and stable historical IDs.
3. Building count/size matches funded quantity; queued, building, establishing, operating and suspended visuals are distinguishable. Never resize a building to conceal a poor fit.
4. Exact upfront/annual money and commitments reconcile; taxes begin at the configured time; no repeated submission, reload, undo or fast-forward creates money. Preview and committed deterministic quantities match.
5. Meter contributions plateau at demand/condition limits; identical facilities cannot generate unlimited points. Removing baseline production/cover has a cost. No action is best in cost, land, time and all three outcomes across all sites.
6. Same saved external conditions drive player and no-new-action comparisons. No remote swale protection or water-service category confusion. Invalid effects do not partially apply.
7. At least two distinct ten-year strategies remain feasible, plus wait-only, overspend, poor maintenance, overbuilding and adverse-event cases. Use actual map geometry and public ledger, not a mocked success card.
8. Actual Chrome at 1280×720 and 1440×900 shows the table-derived action menu, selected-site preview, budget, three meters, annual report and ending without overwhelming the land. Also check 1920×1080 and 125% zoom for layout. Pattern IDs: **TN-01/03/04/05/08, CS-01/02/04, AX-01**. Compare against the eventual accepted Phase 2A benchmark; current rejected captures cannot pass this gate.

## Starter implementation — October 24, 2026

The integration branch implements `starter-rubric-v2` with seven recipes. All prices/upkeep below are authored gameplay assumptions, not estimates for this real location. Each land agreement covers the named field's explicit managed area for ten operating years. Annual public receipts are explicitly zero except the tentative $20,000 for one completed, occupied low-density home. Private output is never treasury income.

| Recipe | Upfront | Annual upkeep / ten operating years | Construction / establishment | Funded bundle and modeled effect |
|---|---:|---:|---|---|
| Traditional cultivation | $100K | $2K | 0 / 1 years | Named field; existing authored offer output and 0.25 food-service units per output unit |
| Conservation rotation | $160K | $3.2K | 0 / 3 years | Named field; gradual establishment toward 1.2× the field’s configured conventional-cultivation capacity, 0.3 food-service units per output, 30% vegetated cover |
| Living margin | $125K | $2.5K | 0 / 2 years | Existing margin offer and cover, gradual establishment |
| Meadow restoration | $75K | $2K | 0 / 3 years | Named field; gradually restored vegetation, no invented crop/recreation service |
| Connected swale | $100K | $3K | 1 / 1 years | Eligible named field; existing configured storage only after commissioning |
| Small public park | $50K | $2K | 1 / 1 years | One 48×40 m park; 5 illustrative recreation units; 85% vegetated footprint, remaining field keeps baseline cover |
| Low-density housing | $1M | $10K | 1 / 1 years | One serviced home on its 26×32 m lot; four illustrative occupied places; 35% vegetated lot, remaining field keeps baseline cover |

Economic: 80% output / 40 (capped) + 20% funded operation. Social: 80% authored service units / 18 (capped) + 20% undisrupted sites. Environmental: 80% management-site vegetated share + 20% operational storage / 100 m³ (capped). Individual indicators are bounded 0–100. These are starter comparison indices, not the full employment, resource, health or biodiversity model. The Year 3 wet-year assumption scales crop output and food services by 0.8; home/recreation services remain independent. All other years currently use reference growing conditions. Source /5 profiles are preserved separately and never added directly to meters.

Fixed-storm runoff uses the existing disjoint catchment/connection groups and TR-55 model. Annual cover follows establishment; building/park conversion is bounded to its real footprint, and unbuilt/operating storage is separated. This compares management states under the same storm, not changing yearly rainfall or hydraulic flood risk. Construction disturbance and crop-specific process calibration are not modeled.

Save key `terra.meadowbrook.annual-playable.v2` separates this rule set from Peter's Editor demo and legacy credits. Invalid/incompatible saves are retained under `.recovery` with visible feedback; a new run preserves the preceding run under `.previous`. There is no silent credit conversion or replay under changed recipe hashes.

### Connected-field layout revision — October 25, 2026

The user's no-gap placement request is implemented in definition **3.2.0**. The eight southern management fields now share ten boundaries within the same AOI, with explicit areas from **0.78 to 1.56 ha**. Their versioned polygons and source-cover accounting are compiled into `ConnectedFields.json`; [geometry and verification](evidence/hud-connected/README.md) document the exact quantities. A land-action price still funds one named ten-year management agreement. Buildings retain their existing metric lots and funded bundles; expanding the surrounding management field does not enlarge a building or create extra homes. Old definition 3.1 town/chapter saves remain under their original keys, and the new layout begins a separate run rather than recalculating committed history.
