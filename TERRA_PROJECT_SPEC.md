# Terra: Shape Your World

**Product & Implementation Specification**
**Status:** Active north star — three rural locations, annual land-use strategy
**Revised:** October 25, 2026, by explicit user direction
**Target:** Hack for Social Impact — UNCCD youth land-use education challenge
**Audience:** Ages 15–24
**Platform:** Unity 6 + URP, desktop browser delivery
**Business model:** Free, nonprofit/public-benefit; no ads, purchases, subscriptions or profit-driven features

**Simulation-reference decision (October 25):** Peter chose one numerical reference base: **Coon Valley / Vernon County, Wisconsin**, for weather, agriculture and economic benchmarks, even when the fictional scenario name or selected landscape differs. Keep the three authorized terrain packs and separate saves; terrain provenance is separate from the shared simulation reference. Do not imply Wisconsin statistics are measurements of Point Reyes or Suusamyr. Label the experience: **"Fictional scenario · weather and economic references: Coon Valley, Wisconsin · other parameters are authored game assumptions."** County/state benchmarks retain their true geography, units and source year; unsupported costs/response curves remain authored. Inspect the supplied hackpack first and use NOAA for weather when no suitable, accessible hackpack series is quicker to validate. See [simulation approach](docs/SIMULATION_APPROACH.md) and [dataset audit](docs/HACKPACK_DATA_AUDIT.md). This is a requirements decision, not completed runtime wiring.

**Location-selection decision (October 25):** The user authorized a polished draggable Earth, cinematic entry, and three supported places: Meadowbrook Valley (the existing Wisconsin AOI), Point Reyes, California, and Suusamyr Valley, Kyrgyzstan. The fixed catalog and each place’s separate ten-year run replace the earlier blanket one-location/no-selector limit. New locations use sourced elevation with clearly authored cover, opportunities and shared educational USD rules. This does not authorize a live world generator or certify Phase 2A. See [atlas design and implementation](docs/ATLAS_SELECTION.md).

**Annual-gameplay decision:** The user asked to make [Peter's annual proposal](docs/PRD_ANNUAL_GAMEPLAY.md) the core loop and incorporate the full supplied [Actions & Effects table](docs/ACTIONS_AND_EFFECTS.md). This replaces the former immediate-only placement puzzle, abstract-credit economy, optional time progression, three-tools-only completion gate, and Continue Building after an untimed win. Annual commitment, ongoing public finances, economic/social/environmental meters, and a ten-year result are now required product behavior. This is a vision change, not a claim of implementation.

**What stays fixed:** rural land-use strategy; Meadowbrook Valley as its fictional display name; the selected Wisconsin AOI, source provenance and stable `coon-valley-brye-v1` identifier; accessible youth education; scientific honesty; Terra Nil-led whole-scene quality; free licensed assets only; no further image generation in the current pass; Chrome verification. No extra locations beyond the authorized three, city level, live generator, real transactions or mandatory online AI service.

**Document roles:** this spec governs vision; [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) governs sequence and acceptance; [PHASE_STATUS.md](docs/PHASE_STATUS.md) records observed implementation; [ACTIONS_AND_EFFECTS.md](docs/ACTIONS_AND_EFFECTS.md) preserves the chart and defines catalog/budget/meter requirements. Peter's PRD remains the source proposal, with an adoption notice identifying superseded draft restrictions and optional extensions.

## 0. Read This First — The Project in One Minute

**Terra lets you shape a real rural place over ten years, live with the consequences, and try another future.**

You are a fictional local-government decision maker in Meadowbrook Valley. Each year, inspect the land and community's needs, choose projects you can afford, review their future costs and effects, and commit the plan. The world changes as projects are built, landscapes establish and services operate. See what happened to the economy, community and environment, then adapt the next year's choices.

> **Inspect → Plan → Budget → End Year → See consequences → Adapt.**

Placement is the spatial interaction inside this loop. Budget, limited land, construction time, recurring commitments, demand and changing conditions give placement meaning. A larger treasury is not the same as a thriving community; a strong single meter is not the whole objective.

At Year 10, see the recognizable place you shaped, compare it with its opening and a same-conditions no-new-action future, understand the biggest benefit and trade-off, and replay. The scene is the emotional payoff. A short optional transfer question and takeaway connect play to a real planning question.

**One-sentence pitch:** What would ten years of your land-use decisions leave behind for a real community, its economy and its environment?

**Non-negotiable:** a young person makes a meaningful choice, sees what changed, understands why and can connect that lesson to a real question. Numbers and scenery must agree.

## 1. Challenge and learning purpose

The UNCCD challenge is engaging youth in sustainable land-use decisions: competing livelihoods and community needs, land degradation, water scarcity, recovery and local participation. Terra is an approachable strategy game, not a professional GIS or planning forecast.

Learning comes from benefits, costs, uncertainty and repeated decisions. Geography is grounded in data; future public budgets, projects, demand, ownership/consent assumptions and outcomes are explicitly hypothetical. A small pilot can show preliminary comprehension and replay interest, not prove long-term behavior change.

## 2. Scope decisions

### Required in the first full game

- Three selectable rural landscapes: **Meadowbrook Valley**, **Point Reyes**, and **Suusamyr Valley**. Each run stays in its selected location for all ten years; saves are separate. Wisconsin retains its source pack and identity. New locations distinguish real terrain from authored land cover and game opportunities. Buildings remain hypothetical rural development.
- Ten annual turns with an editable current-year queue, explicit End Year, annual report, Wait, persistence, and fast-forward using the same transitions.
- USD public budget: upfront contributions, recurring commitments, upkeep, configured public receipts, cash and arrears. Private output and community benefits stay separate from the treasury.
- Three visible **Economic / Social / Environmental** meters derived from accepted state, with inspectable raw components and reasons.
- All **12 action families** from the user's table, including traditional/regenerative farming, parks, both housing densities, warehouse, data center, village center, non-renewable/renewable power, stewardship and water processing. Supporting restoration and connected swales remain useful. Deliver subsets in stages, but do not call the full game done with only the original three tools.
- Coherent real-scale visual assets, project lifecycle states, conditional effects, site suitability, demand limits, maintenance and opportunity costs.
- Deterministic annual rules and authored explanations that run without credentials. Existing fixed-storm runoff is one environmental component, not the annual game engine.
- Multiple viable ten-year strategies, short onboarding, same-conditions comparison, a useful ending and replay.
- Reproducible Unity Web build and honest provenance; no essential live GIS dependency.

### Deliberate limits

No detailed crop seasons/harvesting, supply chains, traffic simulation, construction logistics, realistic land markets, ownership inference, municipal legal simulator, exact flood inundation or calibrated ten-year yield forecast. Ten years means ten concise decisions, not an elaborate management dashboard. Do not implement a city zoning/road network to support the chart.

Locations beyond the authorized three, geocoding and a live generator require another separate decision. The original Wisconsin scenery quality gate remains open. Optional LLM narration may explain accepted facts later; it must not be needed to play or become an unbounded author of scores, cash, weather or causality. Generated ending imagery is not part of this pass.

## 3. Core game loop and commitment boundary

1. **Arrive:** see the land, Year 1, available public budget, a concise community brief and three clearly labelled indicators.
2. **Inspect:** select a site, see its existing use, eligible projects, actual area, connections and scenario assumptions.
3. **Plan:** queue several actions with ghost footprints. See upfront cost, reserved future obligations, occupied land, completion year, expected benefit and trade-off. Preview meters are distinct from accepted state.
4. **Revise:** move/replace/remove and undo/redo unsubmitted proposals without spending. A camera drag never commits an action. Conflicting/overlapping/ineligible proposals explain their rejection.
5. **End Year:** review the batch and commitments; freeze one request; validate and resolve it once. Accepted years become history. Clicking twice, retrying or reloading cannot spend twice or reroll conditions.
6. **See consequences:** reveal construction/establishment/operation on the map and update budget and three meters from accepted results. Show at most three outcome cards: **what changed, why, who/where was affected**. Details remain inspectable.
7. **Adapt:** Continue starts the next planning year with carried assets, cash and obligations. Maintain funded projects, choose a different investment, or **Wait This Year**. Empty queues still advance the world.
8. **Finish:** after Year 10, show the completed history and paired comparison. **See My Ten-Year Future** can submit a reviewed current queue and process remaining years without further new interventions, after an explicit in-game confirmation.

State machine: `Planning → Resolving → Reviewing → Planning`, ending in `Completed`. Recoverable errors retain the same pending request and state. Save at accepted boundaries and preserve pending plans/requests. Replay starts a separate run from the original baseline. Year 11 and retroactive construction refunds are not part of the initial product.

Current-year undo is safe experimentation; post-submission consequences give choices weight. Any later conversion/removal of built assets needs a separately priced recipe. No instant full refund for demolishing last year's project.

## 4. Rural scenarios and a complete action catalog

The original Wisconsin AOI is fixed in [location-selection.md](docs/location-selection.md); `worlds/meadowbrook-v1` retains its canonical geography. The two additional bounded packs are documented in [ATLAS_SELECTION.md](docs/ATLAS_SELECTION.md). Its ten current management sites and two scenario no-edit zones are a starting structure, not a reason to exclude the new catalog. Refine or subdivide hypothetical opportunities within the same AOI if necessary, without double-counting area, changing source evidence or implying surveyed parcels/consent.

> **Can you meet this community's needs over ten years while staying financially viable and caring for its land and water?**

The player is allocating a fictional public contribution to a defined project. Each recipe records funded quantity, owner/operator, completion, upkeep and what the government actually buys. Costs in the chart are retained as provisional game-balance inputs; they are not local quotes. The $20K/$150K housing receipts are tentative scenario assumptions. Renewables use the user's latest **$3M**, overriding the PRD's older $2M reference.

[ACTIONS_AND_EFFECTS.md](docs/ACTIONS_AND_EFFECTS.md) is the detailed action contract, including all costs, /5 source profiles, effect channels, funding rules, meter rubric, art mapping and required tests. The /5 profiles express design intent. The live meters depend on operating state and unmet needs; they do not award fixed points for each placement or compound benefits indefinitely.

The chart's dense housing, service center and industrial projects are explicitly permitted hypothetical options on this rural map. Their scale, siting, resource use and displaced land are part of the lesson. They do not authorize an urban setting or citywide simulation. No project may fit by shrinking its prefab or erasing protected geography.

Stewardship requires an explicit fictional agreement, consent and specified work; land back is not a purchasable ecological reset. Water processing initially means a clearly named wastewater upgrade; drinking-water supply is a separate service, not an automatic benefit. These interpretations preserve the table's intent while avoiding misleading claims.

## 5. First-time experience — target 8–12 minutes, validate in playtests

| Stage | Player experience | Lesson |
|---|---|---|
| Opening minute | Explore the valley; inspect one need and two sites | A real place, hypothetical choices, limited resources |
| First plan | Compare traditional/regenerative farmland and a community investment; queue two, revise one | Price, land, time and opportunity cost |
| First End Year | Watch the accepted change; read budget and three meter deltas | Planning differs from completed operation |
| Years 2–3 | See upkeep, establishment or a disclosed adverse event; adapt or wait | Consequences persist; timing and maintenance matter |
| Remaining years | Make short annual decisions or choose confirmed fast-forward | Competing strategies over the same horizon |
| Year 10 | Compare opening / your future / no-new-action future; inspect history | Outcomes depend on decisions and shared conditions |
| Closing | Optional transfer question and takeaway; replay another strategy | Apply the reasoning beyond the exact map |

No mandatory prediction form between turns. Keep report cards short, skippable animations and details on demand. Replay changes strategy in the same location; it never discards a run silently.

## 6. Educational design

Teach competing land uses, separation of public spending from community benefit, lagged effects, spatial connections and uncertainty. No single action or ideology is a universal winning answer. The same investment can help at an underserved site and waste budget at an already saturated one.

Use actual changed variables in explanations: “This project is still being built, so homes and tax receipts have not started”; “The park meets recreation demand but adds annual upkeep”; “This swale only retains runoff from its connected area.” These are templates; fill them from accepted facts, not invented numerical prose.

At the ending offer one unseen-situation transfer question and a planning-question card. An optional pre-play question can support a small learning pilot, but it is not a required interruption. Do not collect identity data unnecessarily. Only include civic links after verifying them; never submit feedback or contact others automatically.

## 7. Visual and interaction direction

**Terra Nil is the primary visual/feel reference; Cities: Skylines is secondary for information organization.** Read [DESIGN_REFERENCE.md](docs/DESIGN_REFERENCE.md), [art-direction.md](docs/art-direction.md), [PHASE_2A_GOAL.md](docs/PHASE_2A_GOAL.md) and [VISUAL_REBUILD.md](docs/VISUAL_REBUILD.md). The current visual baseline was rejected. Phase 2A remains mandatory before accepting new gameplay in the live scene; parallel prepared modules do not waive it.

Assets, renderer, materials, camera and HUD are replaceable. Use coherent free licensed asset families with their intended materials/shaders and evaluate actual Chrome output. Stop image generation for this pass. Do not constrain the camera just to conceal an asset's limitations. Landscape quality must hold in normal, close, overview, moving and transformed views. Each Phase 2A visual dimension must meet its reference-calibrated gate independently; UX scores cannot compensate for weak scenery.

### Screen and interaction hierarchy

- Landscape dominates. Compact Year / cash / committed funds group; three equal-status meters with changes and expandable reasons (TN-01/03, CS-01).
- Stable action categories: **Land & Nature**, **Homes & Community**, **Work & Utilities**. Show available variants inside each; never display twelve huge permanent cards (TN-04, CS-01).
- Selected-site preview: matching object/footprint, price, ongoing costs, expected timing, benefits/trade-offs, eligibility reason and Queue action (TN-05, CS-02/04).
- Persistent **End Year** with queue count, accessible plan review, Wait and secondary confirmed fast-forward. Disable edits during resolution; show progress/retry honestly.
- Map-linked annual report, short outcome cards and clear Continue; meter details and full ledger stay dismissible (TN-08, CS-01).
- End view: same-camera comparison, timeline, optional question/takeaway and Replay. Inspecting the final map cannot mutate the run.

Use labels, icons and outlines as well as color. Pointer/keyboard share the command path; UI blocks world clicks; Escape has a predictable cancel/back action; focus is visible and restored after dialogs. Preserve mouse/trackpad camera usability, mute and reduced motion. Body text, controls, zoom and viewport checks follow AX-01. Animations follow state; they never commit money or time.

Buildings retain metric scale, terrain contact and whole-footprint eligibility. Proposed/building/establishing/operating/suspended visuals must differ; restored land is an illustrative simulated state, not observed imagery. Price, output or capacity cannot be inferred from decorative geometry. Current and queued worlds must never be confused.

## 8. Geography, assumptions and the expansion gate

Use the selected rural landscape throughout all ten years of each run; changing locations switches to a separate saved run. Preserve topography, mapped water/path alignment, source resolution and source dates. Decorative trees, hypothetical building sites, authored demand and consent are disclosed. Empty-looking land is not automatically vacant, degraded or legally available.

Four evidence classes: **observed/mapped**, **benchmark**, **scenario assumption**, **simulated**. A county benchmark is not a measurement for a site; a subsidy is not total construction cost; an illustrative pressure index is not a health or ecosystem forecast. Missing quantities remain unavailable, never silently zero.

The October 25 decision authorizes the three-location selector and regional scenarios alongside the existing annual loop. Each must load correctly, preserve separate saves and identify evidence honestly. The full action catalog, finances, meters, consequences, ending/replay, visual quality and accessibility gates remain required; selector completion does not accept the broader scenery rebuild. Any fourth location still needs an explicit separate decision.

## 9. Geographic Data Sources & Licensing

Select sources for the chosen rural AOI. Prefer useful local/regional data when available; verify coverage, resolution, date, export method, and reuse terms before choosing the location. A rural setting reduces built-environment work, but does not make missing geographic data acceptable.

### Required and optional layers

| Layer | Purpose / selection rule |
|---|---|
| Elevation | Required: terrain with sufficient resolution to support the play scale and visual composition |
| Land cover / vegetation | Required: baseline cover context; authored assumptions fill explicitly disclosed analytical gaps |
| Water features | Include where reliably mapped and useful; do not invent a stream, catchment, or precise drainage path |
| Paths / roads | Context where available; preserve alignment without building a full road simulation |
| Field boundaries | Use actual boundaries only when sourced; authored management-site polygons are hypothetical |
| Buildings | Optional sparse context; no dense footprint inventory or facade pipeline required |
| Soil / treatment parameters | Source if suitable; otherwise choose a disclosed educational assumption after model review |

### Research starting points

- Regional/open-government elevation and land-cover portals for the selected AOI.
- [USGS 3DEP / The National Map](https://www.usgs.gov/the-national-map-data-delivery/gis-data-download), if a US site is chosen and coverage is suitable.
- [ESA WorldCover](https://esa-worldcover.org/en/data-access) for possible cover context, with exact product/version and resolution checked against the AOI.
- [NASA SRTM](https://developers.google.com/earth-engine/datasets/catalog/USGS_SRTMGL1_003) as a possible terrain fallback where its resolution is adequate.
- [OpenStreetMap](https://www.openstreetmap.org/copyright) for available mapped water/path/structure context, subject to attribution and reuse terms.
- Authoritative soil/conservation references appropriate to the selected region for model assumptions; verify before assigning numerical values.

These are candidates, not verified downloads or promises of fine-grained coverage. The initial implementation should ingest/cache selected files offline. Advanced tools such as Trends.Earth or InVEST are not initial runtime dependencies. Conflict-related datasets are outside this lesson.

### Provenance requirements

The pack and Data & Assumptions panel expose exact bounds, layer sources/dates, licenses/attribution, processing, resolution, missing features, decorative/inferred assets, authored scenario rules, and model limitations. Never silently equate a decorative vegetation patch with a surveyed habitat or an authored site with a real parcel.

Do not compare products/versions as observed land-cover change without checking methodological compatibility.

---

## 10. WorldPack and annual scenario data

Keep `worlds/meadowbrook-v1` as the authored geographic source, packaged through the existing build script. WorldPack v1 and the fixed-storm simulation seam remain versioned and separate from annual contracts. No second authored world is needed to test annual logic; tiny fixtures are allowed.

WorldPack records immutable terrain/cover/context, stable site IDs, metric coordinates, source bounds and provenance. Annual scenario data separately pins action recipes, eligible offers, cost schedules, household/service/output demand, rule/rubric version, external event schedule, public appropriation schedule and ten-year horizon. Art maps stable action/variant IDs to geometry and icons.

Do not silently reinterpret old credits as production USD. Peter's credit conversion is a compatibility fixture, not the new catalog. Pin every run to its definition/hash and rules version. Reject or explicitly migrate incompatible saves. Runtime history records accepted requests and results so reload never rerolls or rewrites the past.

## 11. Future locations and live generation — deferred

No level picker, second map, geocoder, geolocation prompt, GIS server or live world generator in the active plan. Refine the one valley as mechanics change. Runtime AI location selection and generalized scenario infrastructure do not improve the required first game.

## 12. Annual model, meters and scientific limits

The engine owns eligibility, footprint, public ledger, lifecycle, primitive measures, year/revision, saved conditions, derived scores and accepted history. One transaction accepts all changes or none. Full recomputation over the small site list is preferable to infrastructure.

Apply lifecycle and funded operation before benefits. Construction and establishment delay effects; receipts need explicit prerequisites; upkeep shortfalls affect condition/service. Cap useful output by demand, avoid duplicate area/service claims, and account for displaced baseline land use. The three indicators are game summaries, not forecasts of local welfare.

The [action contract](docs/ACTIONS_AND_EFFECTS.md#three-meters-with-inspectable-reasons) defines a starter rubric and proposed full-catalog v2 rubric. Final component units, targets, limits and weights must be pinned in tested scenario data before activation. Runtime effects must not be inferred from the chart's /5 ratings. Unsupported components block rubric activation rather than producing fake scores.

Use saved authored events first: a normal sequence and disclosed adverse conditions sufficient to prove adaptation. They are scenario choices, not empirical probabilities. Player and no-new-action runs use the same external sequence and initial state, including equal public appropriation policy; choices change exposure, costs and recovery. No-action still operates baseline services and any baseline obligations. A future historical-weather sampler requires its own source/coverage review.

The next simulation slice uses the shared Coon Valley, Wisconsin reference above. Each accepted year must include production/harvest, an itemized closing public ledger, persistent site condition/damage/recovery, and all three Economic/Social/Environmental scores with signed year-on-year changes. Recompute scores after cumulative demand, displaced land use, resource scarcity, service disruption and supported local exposure pressures. Useful benefits cap at demand; resource use, upkeep and adverse pressures do not disappear when demand is met. Keep headline scores at 0–100; show negative changes and raw deficits without clipping them out of the explanation. A six-data-center stress case near the scenario village must disclose aggregate land, water, energy and service trade-offs, subject to site eligibility, financing and explicit authored proximity/exposure rules. It is a conditional stress test, not a guaranteed score penalty or measured health prediction.

AI narration is optional presentation over persisted accepted results. On backend failure, timeout, refusal, invalid text, missing credentials or exhausted quota/credits, automatically show a local template from the same saved year's results and visibly identify the mode and reason. Suggested status: **"AI explanation unavailable · showing a local summary of your saved results. Your year and budget are saved."** Never replace a failed new year's calculation with an older year's values, silently reroll, or recommit the year. Numeric simulation/validation/persistence failure remains an honest pending/recovery state; it cannot be hidden by prose fallback. These are new requirements: current chapter acceptance still gates the year on an online or explicitly chosen offline receipt.

### Core environmental calculation: direct runoff

Use a documented **NRCS Curve Number-style educational model** as one defensible approach:

\[
S = \frac{25400}{CN} - 254
\]

\[
Q = \begin{cases}
0, & P \leq 0.2S \\
\frac{(P-0.2S)^2}{P+0.8S}, & P>0.2S
\end{cases}
\]

Where:

- `P` is the chosen storm rainfall depth (mm);
- `CN` is a site-appropriate runoff parameter based on cover, soil group/condition, and stated assumptions;
- `S` is potential retention (mm);
- `Q` is estimated **direct runoff depth** (mm).

Calculate runoff volumes from cells or site areas, then aggregate for the defined analysis area. Baseline/proposal differences should not arise from different storms.

**Critical limits:** A Curve Number is not an inherent constant of a cover label; values need defensible soil/cover/condition mappings. Exact soils, field drainage, infiltration, and management practices may be unavailable. In that case the game should explicitly label its hypothetical soil/management assumptions and call outcomes educational estimates. Do not infer erosion rates or crop yields from this runoff calculation.

### Green infrastructure treatment

The Manage Water tool gets a **bounded capacity** applied only to a defined contributing area or connected runoff source. Use a documented approximate retention/infiltration abstraction. Do not simply subtract a universal percentage from total landscape runoff for each water-management feature.

### How runoff fits the annual game

Runoff remains a bounded direct-storm estimate. Compute from accepted/preview land cover and active connected treatment, not nominal asset capacity alone. Use disjoint site/context analysis areas, sum volumes and avoid double treatment. Compare baseline and proposal under identical rainfall and parameter assumptions. Show zero-baseline absolute deltas when ratios are undefined.

An annual reference-storm result is not total yearly runoff, flood depth, water quality, crop yield, disease or precise biodiversity. Energy, water demand and soil/condition pressure can use transparent authored rules; label them game abstractions. Do not require a calibrated scientific model for every meter, and do not present an authored abstraction as one.

### Optional language assistance

The first complete loop works offline with deterministic effects and authored templates. An optional later LLM can summarize accepted history using vetted facts, bounded output and fallback text. Peter's proposed LLM consequence engine is not a first-release dependency; adding model-authored numeric consequences would need a separately reviewed versioned contract, semantic validation and evaluation. Neither a valid JSON response nor plausible narration is evidence of causality. No generated ending image is required or authorized by this plan.

## 13. Unity architecture and integration

Reuse Peter's annual foundation after reviewing and integrating its commits; do not write a second time/budget store. `AnnualTown` / `AnnualTownSession` own accepted state and annual transitions. Adapt the existing placement path to the current-year queue. `WorldStateStore` may provide spatial preview while migrating, but must never independently debit the annual budget or refund committed history.

Keep Contracts/Core plain data and rules; Simulation pure; Geography loading separate; Gameplay/UI/Rendering present snapshots and commands. Extend annual contracts narrowly for jobs, households, energy/water/services and new rubric inputs. Preserve geographic and fixed-storm v1 compatibility. Update validation, detached copies, hashes, save/replay and fixtures together when annual schema changes.

Unity remains at the repository root with URP and uGUI/TextMeshPro. Keep `.meta` GUIDs, licensed sources and reproducible assets. No ECS, generic simulation framework, mandatory database or competing UI framework. One integration owner coordinates shared Core, HUD and scene changes. Plan/evidence documents identify remote work separately from local/live integration.

### Web constraints and deployment

- **Build and deploy a minimal Unity Web project before doing large-scale world work.** This must be an early feasibility gate.
- Target modern **64-bit desktop browsers with WebGL 2 and WebAssembly**; verify the exact engine/browser compatibility and test the deployed build in Chrome. Chrome is sufficient for project acceptance; test Safari or Firefox only when the user explicitly requests it. Do not request Safari or Private Browsing unlocks for routine verification.
- Use URP features and shaders that work in Unity Web builds; don't assume all desktop GPU effects will work.
- Watch memory usage, static asset sizes, network transfer, shader variants, batching and GPU draw calls.
- Package the three authorized rural worlds. Load only the selected pack into the scene; measure transfer and memory before adding more loading infrastructure.
- Configure `Content-Type` and `Content-Encoding` correctly for `.wasm`, `.js`, and compressed Unity output. Use a fallback compression configuration if the host cannot serve the required headers.
- Check the chosen static host's actual upload/asset constraints. Reduce the bounded three-world bundle first; use separate asset storage only if needed. Offline GIS preprocessing is sufficient, and a live external worker is outside the current scope.
- Never mark success based on Unity Editor alone; the **deployed browser build** is the actual product.

Performance targets are goals, not claims: smooth camera/editing on a typical laptop, acceptable initial download and loading feedback, no frame-long freezes for simple placements, and graceful quality settings. Measure real browser performance after the first Web build.

---

## 14. Development order — annual play is the spine

[IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) defines exact tasks and gates. Historical phase numbers and evidence remain stable; Phases 3–6 are deliberately rewritten around the annual game.

| Phase | Deliverable |
|---|---|
| 0–2 | Existing browser, contract and real-geography foundation; preserve evidence |
| 2A | Required first-principles visual rebuild on this same AOI |
| 3A | Reconcile Peter's annual foundation with existing editing work; one state/ledger owner and versioned recipes |
| 3B | Playable three-year Chrome loop: plan, budget, End Year, world lifecycle, meters/report, Wait and reload |
| 4A | Full 12-family catalog, real-scale building integration, service/resource trade-offs and complete meter rubric |
| 4B | Spatial runoff, delayed/conditional effects, authored events and causal explanations |
| 5 | Balanced ten-year mission, adaptation, fast-forward and more than one viable strategy |
| 6 | Same-conditions future comparison, ending, optional learning/takeaway and replay |
| 7 | Whole-run visual/input/audio/accessibility/performance and novice refinement |
| 8 | Reproducible public release, rehearsal and truthful demo |

Independent packages may be prepared within existing authorizations, but acceptance of live Phase 3 requires Phase 2A. Do not restart completed geography or duplicate existing annual work. This revision starts no goal, deployment or message to another worker.

## 15. Single-location definition of done

- [ ] Beautiful, geographically grounded Meadowbrook Valley in the deployed Unity Web game, with Phase 2A quality preserved.
- [ ] All twelve chart families plus supporting restoration/swales implemented with explicit quantity, site eligibility, timing, public costs, upkeep and effects.
- [ ] Current-year queue editing/undo/redo; committed history immutable; safe wait/retry/reload; no double spending.
- [ ] Ten annual turns, lifecycle and maintained services, reconciled USD ledger, obligations/shortfall handling and no Year 11.
- [ ] Three explainable meters derived from primitive accepted state; no flat /5 bonuses, missing-as-zero or score farming.
- [ ] Connected runoff and declared other scenario effects; visible spatial, resource and demand trade-offs.
- [ ] Multiple viable strategies plus adverse/no-action cases; understandable onboarding and adaptation.
- [ ] Confirmed fast-forward using identical rules; paired no-new-action future, year history, same-camera ending and replay.
- [ ] Optional transfer question and useful takeaway; no unsupported claims of real behavior change.
- [ ] Accessible controls and truthful loading/error states; tested Chrome performance, source/license attribution and reliable build/demo fallback.

Optional polish, online narration and further maps cannot replace these requirements. If time forces a reduced demo, label it a partial annual slice and record missing scope; don't quietly redefine completion as three legacy tools.

## 16. Verification

Test exact ledger arithmetic and receipts, commitment reservation, lifecycle timing, shortfall, immutable baseline, stale revisions, atomic rejection, retry/reload idempotency, no-action years, save compatibility, Year 10 stop and fast-forward equivalence. Test real-map footprints, displaced production/cover, active versus unbuilt services, capped demand, meter recomputation and bounded connected runoff.

Use at least two distinct complete strategies and no-action/overspend/overbuilding/maintenance/event stress cases. Compare player and no-action runs under the same saved drivers and pinned rules. Numbers alone do not certify game balance or visual quality.

In Chrome inspect loading, action families, planning, rejected actions, End Year, resolution/retry, reports, later-year scenery, final comparison and replay at 1280×720 and 1440×900, with 1920×1080/125% zoom layout checks. Record reference patterns, matched captures, input path, Console/network, build identity and measured load/frame/interaction timing. Do not claim browser coverage from an Editor window.

Observe novices when available: can they queue/revise/submit unaided, explain one benefit/cost/uncertainty and choose to replay? Missing participants may remain explicitly pending; missing engineering/visual evidence and known user rejection cannot be waived.

## 17. Hackathon demo — 3–4 minutes

| Time | Actual playable demonstration |
|---|---|
| 0:00 | Reveal Meadowbrook Valley; explain real geography versus the fictional ten-year scenario |
| 0:30 | Show Year 1, public budget, community need and three meters |
| 0:50 | Compare two actions; queue farming and a community project, then revise the plan |
| 1:20 | End Year; show construction/establishment, reconciled spending and a reason for a meter change |
| 1:50 | Continue; react to upkeep or a saved adverse condition; show a different site/action trade-off |
| 2:20 | Confirm fast-forward through actual annual transitions to Year 10 |
| 2:50 | Compare your future with the same-conditions no-new-action future; show the largest benefit and cost |
| 3:20 | Show the optional takeaway and replay invitation; explain what another strategy could change |

Keep a reproducible seed/action sequence and clearly labelled recording/local build fallback. Do not replace actual gameplay with a narrated animation or claim a partial catalog is complete.

## 18. After release

Refine the same valley's balance, visuals, data assumptions, onboarding, accessibility and replay from observed feedback. Additional maps, sophisticated models or AI services require separate scope decisions. The hackathon aim is a polished complete small game, not a simulation platform.

## 19. Risks and guardrails

| Risk | Response |
|---|---|
| Attractive scenery with weak decisions | Prove a three-year playable annual loop before broad catalog integration |
| Table becomes arbitrary points | Conditional operating effects, raw components, saturation and explainable versioned meters |
| New catalog becomes a full city simulator | Curated hypothetical projects and service capacities on the same rural AOI; no road/traffic/zoning simulation |
| Projects have no ongoing consequences | Construction/establishment, funded upkeep, future commitments and saved events |
| Public/private finances confused | One public ledger; private activity is not automatic tax income |
| Two stores spend/refund the same money | Annual state is authoritative; legacy placement is only a preview adapter |
| Unverified prices or science presented as fact | Keep chart inputs and unsupported effect ranges visibly authored; preserve source provenance |
| User's visual rejection buried under checklists | Preserve Phase 2A's direct scene-quality gate and compare actual Chrome output |
| AI or online failure breaks the game | Deterministic offline rules and authored explanations first |
| Scope silently reduced | Track every required chart row and full-game gap explicitly |

No purchases, mandatory AI dependency, generated art for this pass, extra location, real land-rights inference or exact flood/health/economic forecast.

## 20. Sources & Primary Technical References

These are **implementation research starting points**, not a substitute for checking the exact export, version, date, usage terms, and geographic coverage when downloading data.

### Challenge context

- Official hackathon: https://www.hackforsocialimpact.com/events/sf-2026
- Exact UNCCD youth-planning challenge brief: supplied by challenge organizers and reproduced in the project discussion.

### Geographic data

- USGS The National Map / 3DEP: https://www.usgs.gov/the-national-map-data-delivery/gis-data-download
- ESA WorldCover: https://esa-worldcover.org/en/data-access
- NASA SRTM: https://developers.google.com/earth-engine/datasets/catalog/USGS_SRTMGL1_003
- OpenStreetMap data license: https://www.openstreetmap.org/copyright
- Trends.Earth: https://docs.trends.earth/en/latest/
- InVEST: https://naturalcapitalproject.stanford.edu/software/invest

### Platform / hosting

- Unity Web browser compatibility: https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/intro/browsercompatibility
- Unity Web optimization guidance: https://activation.unity3d.com/how-to/profile-optimize-web-build
- Unity Web server compression configuration: https://docs.unity3d.com/Manual/web-server-config-apache.html
- Vercel configuration and custom headers: https://vercel.com/docs/project-configuration/vercel-json
- Vercel platform limits: https://vercel.com/docs/limits
- Vercel function payload limits: https://vercel.com/docs/functions/limitations

### Design inspirations (not scientific sources)

- **[Persistent design reference](docs/DESIGN_REFERENCE.md):** source register, inspected visuals, pattern IDs, limits and review procedure for all future player-facing work.
- **Terra Nil:** primary visual/feel reference; choose/position/restore/progress, immediate visual transformation, landscape prominence, illustrated tools and quiet feedback.
- **Cities: Skylines:** secondary reference for stable tool categories, contextual guidance and spatial information; not a mandate for city content or dense simulation dashboards.
- **Rural landscape dioramas:** readable land-cover patterns and spatial trade-offs without dense city-building complexity.
- **Geographic digital twins:** anchored real-world geometry with a more approachable, gamified interface.

---

## 21. Implementation guidance

Work within the requested phase. Read this spec, the plan, action contract, current status, design reference and art contract. Respect active ownership and dirty work; fetch/review remote additions before integrating and preserve recoverable history. Never treat a roadmap as authorization to implement every phase or contact other workers.

Prioritize complete slices: current-year choice → reserved cost → annual commitment → lifecycle and consequences → report → next decision. Reuse Peter's state/ledger and action art where suitable; update contracts explicitly. Keep all rules and display values grounded in accepted data and transparent assumptions. Verify the actual browser, record evidence and distinguish remote package tests from live acceptance.

**The intended outcome is one beautiful rural place, ten years of consequential choices, and a clear account of what those choices left behind.**
