# Terra: Shape Your World

**Product & Implementation Specification**  
**Status:** Active north star — rural-first, one-location scope  
**Date:** October 24, 2026  
**Target:** Hack for Social Impact — UNCCD youth land-use education challenge  
**Audience:** Ages 15–24  
**Platform:** Unity 6, playable in modern desktop web browsers; deployable through Vercel or another static host  
**Business model:** Free, nonprofit/public-benefit educational experience; no ads, purchases, subscriptions, or profit-driven features

---

**Scope revision — October 24, 2026:** Rural land use is the primary use case. Build and refine **one real rural location across the full game scope** before considering another location. This supersedes the previous Mission Dolores/city-first opening and the early multi-level/generator roadmap. The Phase 1 location is presented as **Meadowbrook Valley**, a fictional game name for the selected Wisconsin AOI; the frozen bounds, evidence and data gaps are recorded in [docs/location-selection.md](docs/location-selection.md).

**Naming decision — October 24, 2026:** Use **Meadowbrook Valley** in the game, active plans, demos and future content. This is a fictional display name, not a new geographic location. Retain Coon Valley / Brye Lane only where needed for honest source attribution, geographic lookup or historical evidence. Keep the existing `coon-valley-brye-v1` identifier, AOI bounds and data sources stable; never show that technical ID as the player-facing name.

## 0. Read This First — The Project in One Minute

**Terra turns a real rural landscape into a beautiful, playable environmental strategy game.**

A player lands in a stylized 3D reconstruction of one actual rural place. Terrain, vegetation, water features, paths, and any relevant small structures are grounded in geographic data, with sources and dates recorded. The selected Meadowbrook Valley location (game name) and its source evidence are documented in [the selection note](docs/location-selection.md).

The working opening scenario is **meeting a rural community's illustrative productive-land needs while restoring vegetation and managing rainfall runoff under a limited budget**. This is a design starting point to refine in phase 1, not a claim about the selected place's actual agricultural needs or yields. The player chooses an intervention, previews its effects, and places it on the world. The landscape visibly transforms; productive capacity, costs, land cover, and a transparent environmental estimate update immediately.

The interaction is intentionally simple, inspired by **Terra Nil**:

> **Choose → Place → Transform → Balance → Repeat.**

Depth comes from *where* interventions are placed and how they interact. It does not require crop seasons, harvesting, supply chains, city management, or many unrelated controls.

At the end, players compare their design with the baseline, answer a short transfer-of-learning question, and leave with a useful question about land use near them. They may **Continue Building** in the same landscape.

**One location is the complete initial product, not a throwaway tutorial.** Build the full editing loop, all three intervention families, consequences, mission, learning, replay, and presentation quality there. Improve that same location as systems evolve so changes do not require retuning several levels.

Additional rural places, city landscapes, and **Play Your Neighborhood** remain future possibilities. They are outside the active implementation phases and are not required to prove the first game works. Keep the geographic data structured, but do not build a multi-location platform now.

### One-sentence pitch

**What if you could reshape a real rural landscape and see how every land-use choice affects people, vegetation, and water?**

### Non-negotiable design principle

**A young person makes a meaningful choice, sees what changed, understands why, and can connect the lesson to a real land-use question.** If a feature does not improve that experience in the first location, it is not a core requirement.

---

## 1. The Actual Challenge and Our Interpretation

The provided UNCCD challenge is **“Engaging Youth in Sustainable Land-Use Decisions.”** Young people aged 15–24 need accessible and engaging ways to understand how land-use and urban-planning decisions affect land degradation, water scarcity, ecosystem recovery, livelihoods, and their communities.

The challenge explicitly prioritizes:

1. **Engagement and education**, not just maps or simulation dashboards.
2. **Meaningful choices and understandable environmental, social, and economic consequences.**
3. **Experimentation with real trade-offs**, including resource constraints.
4. **Relevance to local places and real participation.**
5. **Measurable educational learning**, distinct from merely measuring playtime.
6. **Scientific honesty:** evidence-informed simplified rules are acceptable, but not unsupported claims of exact forecasts.
7. **Feasibility:** a working hackathon vertical slice and a plausible path to an initial usable version in under three months.
8. **Public benefit without a profit motive.**

A professional GIS planning tool, advanced scientific hydrology model, or community-workshop administration system **is not the product goal**. Good geographic data and modeling serve the game and educational experience.

### Our response

- **A game first:** a focused, intuitive, polished placement-based strategy puzzle.
- **Real-world locations:** recognizable geography is a core differentiator.
- **One coherent mechanic:** place interventions on land; models and indicators update beneath that mechanic.
- **Real consequences, simplified appropriately:** explainable cause-and-effect, not arbitrary ecological “+10 points” bonuses.
- **Local relevance:** one concrete rural place and a lesson players can connect to land-use choices near them.
- **Education and action:** a short transfer-of-learning prompt and relevant next step at the end.

---

## 2. Scope Decisions — Fixed vs. Optional

### Fixed product decisions

- **Engine:** Unity 6 + C#; browser build is an early requirement.
- **Rendering:** URP, designed around browser constraints.
- **Hosting:** a simple static Unity Web deployment, trying Vercel first; use other storage only if measured limits require it.
- **Opening and only active location:** one **real rural geographic slice**, selected and recorded during phase 1. Rural is the primary use case, not a temporary substitute for a city.
- **Art:** a beautiful, stylized 3D landscape diorama, with recognizable terrain, vegetation, field/land-cover patterns, and water features where mapped. Dense streets and building inventories are not required.
- **Core gameplay:** one placement puzzle, one budget, and a few competing land-use objectives.
- **Working rural intervention families:** **Cultivate, Restore, Manage Water**. Cultivate is the rural productive-use counterpart to the earlier Develop tool; finalize its specific treatment recipes in phase 1.
- **Simulation:** one explainable direct-runoff model plus productive-capacity, land-cover, and budget accounting. Productive capacity is explicitly illustrative game accounting, not a crop-yield or food-security forecast.
- **Geography:** a deterministic offline GIS-to-WorldPack path for this one location. A small versioned format supports iteration and engineer handoffs; broad reuse is not an acceptance criterion.
- **First scenario:** productive land versus restoration and water-management trade-offs, with hypothetical sites, demand, and costs visibly labeled.
- **Full scope in one place:** all core tools, reversible editing, consequences, mission, comparison, learning, Continue Building, and polish share the same location.
- **Development order:** build the editable simulated world before mission framing. No separate player-facing sandbox mode is required.
- **Phases 0–1:** local to the primary computer. After that, parallel engineering packages improve this same game.

### Enhancements within the same location

Only add these when they improve the core experience: a short same-storm rain reveal, better vegetation/terrain detail, stronger animation/audio, clearer explanations, accessibility, and replay through different viable designs. Revisit the existing location after every systems change.

### Deferred until the one-location quality gate and a separate scope decision

- A second authored rural level or any city level, including the former Mission Dolores proposal.
- Location picking, geocoding, geolocation, live world generation, and Play Your Neighborhood.
- A general scenario/template engine, multi-region data adapters, or AI context selection.
- Additional erosion/drought/long-term model modules unless a later explicit decision makes one essential to the first location's lesson.

These are **not hackathon stretch tasks to begin automatically** after a phase passes. First finish and refine the entire game in one place. Expansion requires a new plan and an explicit decision to accept the additional content-maintenance burden.

**Not in the current game:** multiplayer/community workshop, full farming or city simulation, realistic zoning/ownership, exact flood inundation, seasonal harvesting/supply chains, complex ten-year management, real transactions, mandatory AI chat, or professional GIS reports.

---

## 3. The Core Game: One Loop, Not Ten Mechanics

### Genre

**Placement-based rural land-use puzzle / ecological landscape-builder.** A spatial puzzle with satisfying, direct world transformation. It should feel simple to control but thoughtful to solve.

### The single repeatable interaction

1. **Choose** a tool (Cultivate, Restore, Manage Water).
2. **Preview** its shape, price, eligibility, and likely direct effects on the land.
3. **Place** it with a click or drag; the world visually transforms.
4. **Observe** changes to a few on-screen measures, supported by a deterministic model.
5. **Balance** the remaining budget and objectives; choose the next placement, move/undo a previous one, or finish.

There is **no required separate test-design-investigate-redesign phase** and **no required advance-five-years mechanic**. A rainfall comparison can be a short cinematic view of results already computed. Long-term time-lapse is optional later.

### Why it should be fun

- The player can immediately transform recognizable land.
- Each placement has satisfying visual and audio feedback.
- Different sites are differently suitable, so positioning matters.
- Three understandable interventions create trade-offs rather than universally good upgrades.
- There are multiple valid ways to meet goals.
- Visible scenery improves while some social/environmental constraints may worsen.
- The player can undo, rearrange, replay, and improve their solution.
- The level rewards curiosity and local reasoning, not memorizing a textbook.

### The environmental complexity is under the surface

An action changes canonical land state. The simulation reads that state. The renderer and HUD show the result. Avoid hardcoded story-specific bonuses that contradict the modeled inputs.

```text
Player places an intervention
          |
          v
Geographic land-use state changes
          |
          +--> Cultivated ground / vegetation / water-management features update in 3D
          +--> Land-cover and illustrative productive capacity recalculate
          +--> Budget and protected-space constraints recalculate
          +--> Runoff model recalculates for the selected storm
          |
          v
Small, immediate, explainable feedback
```

---

## 4. Opening Location: One Rural Landscape — Working Title “Balance the Land”

### Select the location early, then stay with it

The user has asked to choose the specific rural location during early planning. Phase 1 should compare a small shortlist using data coverage and visual previews, select one bounded area, record the reason and source evidence, and then freeze that location for implementation. Candidate inspection is not level authoring: do not build multiple playable worlds to choose between them.

Prefer a compact rural landscape with readable terrain, a few contrasting land-cover areas, and a plausible water-management lesson. A field/grassland/vegetation mosaic with gentle relief is a useful starting profile. A mapped watercourse can strengthen composition, but must not be invented if absent. Avoid a site whose core lesson requires an unbuilt irrigation, erosion, drought, or crop-growth model.

Choose for:

- clear, attractive landscape composition at play scale;
- available elevation and land-cover data at appropriate resolution, with usable rights;
- a small number of meaningful candidate sites and protected/no-edit areas;
- scope that can be rendered well without a dense built environment;
- a rural lesson that works with the simple placement/runoff foundation.

### Geographic scope

Start around **0.5–1 km²**, adjusting the bounded area to data resolution and playable composition. This is a provisional size, not a reason to invent fine geographic detail.

Preserve relative topography, mapped cover/water/path boundaries, and major geographic anchors. Sparse buildings are contextual only where mapped and useful. Keep a small visual margin outside the editable boundary. Field subdivisions and opportunity sites may be authored hypothetical scenario geometry, but must be distinguished from surveyed parcels or observed land cover.

Protect mapped sensitive features where justified and any scenario-defined no-edit zones; disclose which protection rules are educational assumptions rather than legal designations. Do not imply that a real farmer's land is vacant, degraded, or available for conversion without evidence.

### Working scenario

> **This rural community needs productive land. Can you meet its illustrative production target while retaining vegetation and managing runoff from a heavy-rainfall scenario within a limited budget?**

This framing is the current design hypothesis. Confirm its fit during phase 1 without expanding into a farm simulator. The place is real; demand, candidate sites, treatment costs, production units, and thresholds are game assumptions, not local policy, measured yields, or recommendations for real farms.

### Provisional objectives

| Measure | Initial design intent | Notes |
|---|---|---|
| Productive capacity | Meet a configured minimum in **illustrative production units** | Simple area/treatment accounting; no claim of tonnes harvested, people fed, or economic income |
| Water response | Keep same-storm estimated direct runoff below a configured limit | Compare the same rainfall, soil, and cover assumptions |
| Budget | Stay within a finite number of abstract credits | Costs are game balance, not real money |
| Vegetation/protection | Preserve no-edit features and account for cover changes | Extra thresholds only if they strengthen the puzzle without cluttering the HUD |

Balance numeric targets against the actual site list. There must be multiple reachable solutions; never silently change rules to force a cinematic outcome.

### Candidate sites

Author roughly 8–12 hypothetical management sites initially, with stable IDs, area, baseline cover, illustrative existing productive capacity, allowed treatments, protected flag, slope/connection attributes, and cost/capacity settings. Keep data-supported fields separate from assumptions. Use curated polygons, not a system that infers agricultural suitability or legal rights.

### Tool A — Cultivate

**Action:** select a compatible site, preview a productive-land treatment, and commit it with the same placement gesture as the other tools.

**Working variants:** a standard cultivated plot and a plot that reserves a vegetated margin. Define productive area, cover fractions, resource costs, and visual recipe explicitly. The margin occupies land and may change runoff under the model's documented inputs; it does not get an arbitrary global environmental bonus. Neither variant is universally superior. The exact names and numerical trade-offs are phase-1 design decisions and later balance work.

**Effects:** changes illustrative productive capacity and cover, uses credits, and updates the selected rainfall estimate. Existing productive use counts toward the baseline; restoration or replacement can reduce it. Do not count gross added capacity while ignoring the capacity displaced.

**Visual:** cultivated ground patterns and simple crops/vegetation establish within the site boundary. This is a proposed land-use state, not a simulated growing season or instant real-world harvest.

### Tool B — Restore

**Action:** restore suitable vegetated ground cover on a compatible site.

**Effects:** changes cover and runoff inputs under stated assumptions, costs credits, and can give up productive area. Choose vegetation appropriate to the eventual location; tree planting is not automatically the right treatment everywhere.

**Visual:** ground materials and vegetation transition into a clearly labeled conceptual established state. Do not imply observed restoration success or instantly mature real trees.

### Tool C — Manage Water

**Action:** place a vegetated swale or small retention feature at an eligible site/orientation, as appropriate to the chosen landscape.

**Effects:** finite treatment capacity applied only to a defined contributing area, with explicit footprint and cost. Capacity is an educational engineering assumption, not a site-specific design recommendation. Do not claim flood protection, irrigation supply, or water-quality removal from a direct-runoff-only model.

**Visual:** a readable landscape feature and optional contributing-area highlight. Any rainfall/flow animation must match the limits of what is actually modeled.

### Safeguards and success state

Restoring every site cannot satisfy a legitimate productive-land objective. Cultivating every site cannot evade budget, protected-area, or water constraints. A distant swale cannot absorb unrelated runoff. Every benefit and cost must arise from state and documented assumptions.

On success offer **See Before & After**, **What Did You Learn?**, **Take It Home**, **Continue Building**, and **Retry / Try Another Strategy**. Replay explores different choices in **this same location**; there is no “next level” requirement.

---

## 5. Twelve-Minute First-Time Player Experience

This is a designed journey, not a mandatory scripted solution. All of it takes place in the one rural location.

| Minute | Player action | Experience / lesson |
|---|---|---|
| 0–1 | Arrive in the landscape, explore, read the brief | Beautiful real terrain; a clear productive-land/water trade-off |
| 1–2 | Inspect two hypothetical sites | Cover, capacity, cost, and connection differ by place |
| 2–3 | Choose Cultivate, compare treatments, place one | Ground transforms; productive units, credits, and runoff respond |
| 3–4 | Preview another site or treatment | The same tool can have different effects by location |
| 4–5 | Notice an unmet budget or water constraint | Maximizing production alone is insufficient |
| 5–6 | Restore suitable cover | Environmental benefit competes for space and credits |
| 6–7 | Place a connected swale/retention treatment | Water management has bounded, spatial effects |
| 7–8 | Move, replace, or undo | Experimentation is safe and comparisons are legible |
| 8–9 | Meet balanced goals | More than one design can work |
| 9–10 | View same-camera/same-storm before and after | See what changed and why |
| 10–11 | Answer a question about an unseen land-use example | Apply the concept beyond the exact solution |
| 11–12 | Take a planning-question card; continue editing or retry | Connect to real decisions and explore another design in the same place |

The storm comparison and learning ending support the repeated choose/preview/place loop. They do not introduce additional management games.

---

## 6. Educational Design

### Intended learning outcomes

After playing, a user should be better able to:

1. Explain that land has competing uses and that community needs are legitimate constraints.
2. Predict that changing land cover and impervious surface affects stormwater runoff **under relevant assumptions**.
3. Recognize that the **location** of productive land use, restoration, and mitigation changes the result.
4. Distinguish avoiding environmental damage from mitigating it afterward.
5. Identify one informed question to ask about a real local land-use decision.

### How the game teaches

- **Immediate feedback:** after each placement, show a compact “benefit / trade-off” explanation.
- **Cause-and-effect inspection:** optionally highlight the affected footprint or contributing area.
- **Multiple solutions:** don't encode one ideological or planning outcome as the sole “correct” solution.
- **Credible simplification:** make rules legible and show their assumptions without requiring GIS expertise.
- **Applied ending:** use a new example, not trivia about the exact level just played.

### Sample one-line feedback

- **Cultivate:** “This treatment adds productive capacity and changes ground cover. The preview shows the estimated runoff difference for this storm.”
- **Restore:** “More vegetated ground can reduce runoff in this scenario, but this site also uses land that could serve another purpose.”
- **Manage Water:** “This swale retains some runoff from its connected area, up to its modeled capacity.”
- **Budget:** “The plan meets the productive-land target, but there aren't enough credits left for its water-management needs.”

Whenever possible, link explanations to **actual changed variables**, not hand-authored outcomes that ignore the simulation.

### Lightweight learning assessment

- **Before play:** one prediction (“If we change the cover on a field, what would we need to know to estimate runoff during a heavy storm?”).
- **After play:** a different rural land-use proposal requiring the player to identify relevant factors and missing information.
- **Scoring:** recognize trade-offs and explain why location, surface cover, and community needs matter. Do **not** insist that one placement is always best.
- **Interpretation:** improved answers provide preliminary evidence of understanding; do not claim measured long-term behavior change.
- **Privacy:** avoid collecting names or unnecessary identity data; minors are part of the audience.

### Take It Home

After the level, show a lightweight prompt:

> “A change in land use is being proposed near you. What would you want to know before supporting it?”

User selects an issue (productive land, vegetation, water, habitat, access). Generate a brief shareable card such as:

> **My planning question:** “How would this land-use change affect productive land, vegetation, and runoff, and what assumptions support that estimate?”
>
> **What I learned:** Land-use changes have environmental consequences, and design choices can avoid or reduce some of them.
>
> **Possible next step:** Look for a real local land-management proposal and an official opportunity to provide feedback.

In MVP, do **not** invent official public-consultation links. Verified local URLs may be added later.

---

## 7. Visual & UX Direction

**Design priority — October 24, 2026:** Use **Terra Nil as the primary reference for visual and experiential quality**, with **Cities: Skylines as a selective reference for contextual information and tool organization**. The persistent [design research and reference library](docs/DESIGN_REFERENCE.md) contains dated visual sources, reusable patterns and Terra-specific adaptations. Consult it throughout implementation. It supports this specification rather than replacing the one-location scope, rural livelihoods, or scientific-honesty requirements.

Establish that quality on the existing placement slice in **Phase 2A, immediately after Phase 2 and before further gameplay expansion in Phase 3**. Treat world composition, tool artwork, readable HUD, placement feedback, camera, motion, sound, accessibility and browser responsiveness as one experience. Carry the accepted standard through all later phases; Phase 7 is a whole-game consistency pass, not the first opportunity to make the game feel finished. Detailed tasks, evidence and status belong in the [Phase 2A brief](docs/PHASE_2A_GOAL.md) and evidence ledger. This is a quality ambition, not a claim that parity has already been achieved.

**Visual reset — October 24, 2026, explicitly requested by the user:** Phase 2A is a first-principles visual rebuild. Direct visual quality takes precedence over further UX refinement. The current implementation is not a constraint: assets, renderer, materials, camera, floating-board framing, UI, and entire existing aspects may be replaced or simplified when that produces a stronger game. Research and acquire appropriate licensed resources and tools. Judge actual browser imagery against Terra Nil, not against the prototype or a checklist. The existing one-location educational purpose and honest distinction between geographic evidence and illustration remain; raw raster-cell rendering and the distant full-AOI camera are not product requirements. The [rewritten goal](docs/PHASE_2A_GOAL.md) supersedes the earlier incremental presentation plan.

### Visual identity

**Beautiful, readable 3D diorama based on actual geography.** Draw inspiration from Terra Nil's vivid transformations and readable miniature landscapes. Rural simplicity must produce stronger composition and interaction, not a sparse unfinished scene.

- Strong terrain silhouette, vegetation masses, field patterns, and inviting rural composition.
- Real relative topography; no arbitrary invented hills.
- Aligned terrain, mapped cover/water/path features, and any relevant sparse structures.
- Stylized terrain and vegetation materials, readable cultivation patches, and limited contextual structures.
- Repeated grasses/shrubs/appropriate trees, ground-cover variation, paths, and restrained ambient motion.
- Atmospheric lighting, tasteful shadows, ambient sound, gentle camera easing.
- Placement ghost previews, smooth transformations, contextual visual effects.
- Accessible high-contrast UI, labels plus color, simple keyboard and mouse controls.

### Screen layout

The following is a starting arrangement, not a frozen contract. Recompose or replace it around the new visual direction; prioritize a compelling playable world.

- **Center:** the 3D world occupies the great majority of the screen.
- **Bottom:** three clear placement cards, with sub-variants inside a tool.
- **Top:** a compact objective strip—productive capacity, runoff, budget, and optional cover indicator.
- **Near cursor:** area/footprint, price, and immediate benefits/trade-offs before committing.
- **Right / contextual:** brief selected-site detail only when helpful; no giant permanent GIS dashboard.
- **Post-completion:** Before & After / Continue Building / Retry.

### Camera & interaction

- Pan and zoom with forgiving controls and recognizable camera bounds. The Phase 2A visual rebuild may replace free orbit with a deliberate illustrated camera when that produces a stronger playable world; camera freedom is not more important than visual coherence.
- Click/drag or site selection to place interventions.
- Ghost geometry in green/amber/red to indicate eligibility; never rely on color alone.
- Undo/redo, move, replace, delete and reset.
- Selection outlines stay geographically aligned when the camera moves.
- Prefer one main editing gesture for all tools.
- Fast post-placement response; expensive analysis can be cached or incrementally recalculated.

### Visual integrity

- Distinguish **existing geography** from **proposed interventions**.
- Keep geography to scale; use artistic shading and texturing, not fabricated site positions.
- A procedural tree can be a visualization of mapped tree cover; it is not a precise tree inventory unless sourced.
- “After restoration” renders are conceptual proposed states, not observed imagery.
- If an intervention's scientific result is uncertain, avoid extremely precise glowing flood boundaries.

---

## 8. Real Geography, One Location

The first game uses **one specific real rural slice**, selected and documented during phase 1. Do not build a fictional landscape and label it with a real place name.

| Active content | Location | Main lesson | Status |
|---|---|---|---|
| **Balance the Land** (working title) | One rural AOI, to be selected during early planning | Productive land, vegetation/restoration, rainfall runoff, finite resources | **Only active game location** |

The previous SF opening and named multi-country campaign sequence are superseded. No second location, level selector, or live location generator is part of the active roadmap. When mechanics change, iterate the same map, sites, mission, visuals, and balance together.

### One-location quality gate before expansion

The first location must support all three tools, reliable reversible editing, deterministic consequences, multiple viable solutions, onboarding, comparison, learning, Continue Building/retry, polished visuals/audio/UI, and a reliable deployed browser experience. Fix the highest-impact observed usability and balance issues and stabilize those rules before considering content expansion.

Even after that gate, a new location requires an explicit scope decision. Multiple test fixtures and alternative winning proposals within the existing world are not additional player-facing levels.

### Facts, assumptions, estimates

1. **Observed/mapped facts:** terrain, cover, water features, paths, and any mapped structures with sources and dates.
2. **Scenario assumptions:** production demand, management sites, credits, costs, capacity, and no-edit rules where not sourced.
3. **Modeled estimates:** direct runoff under disclosed rainfall/soil/cover assumptions.

Never present hypothetical interventions as approved projects, surveyed ownership, agricultural advice, or observed environmental improvement.

---

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

## 10. WorldPack: A Small Data Format for the First Rural Location

Use a deterministic converter from the selected GIS inputs to a small versioned game-world package. This separates geographic truth, scenario rules, and presentation, and lets the other engineer improve data without editing the main Unity scene. **The goal is fast iteration on one world, not a general multi-location platform.**

### Current pipeline

```text
One selected, documented rural AOI
    → offline fetch/ingest and cache geographic inputs
    → project, clip, align, simplify, and validate
    → export terrain, cover, relevant context, and hypothetical management sites
    → write one versioned WorldPack with provenance
    → load in Unity and apply the authored rural scenario
```

### Geographic correctness

- Record the geographic bounds in WGS84 and choose a metric projected CRS appropriate to the actual site; do not retain a San Francisco-specific projection by default.
- Export coordinates relative to a local origin near the scene center. Freeze axis, elevation-datum, raster-row, and winding conventions in phase 1.
- Keep all layers aligned through simplification and verify reference points/coordinate round trips.
- Record editable and context boundaries. Keep analytical site areas/cover separate from decorative meshes.
- Store optional layer availability explicitly. An absent building layer must not prevent a landscape with no needed structures from loading.

### Suggested package

```text
worlds/
  rural-pilot-v1/             # provisional ID until site selection
    manifest.json
    terrain.json             # small height grid; binary only if measured size requires it
    landcover.json
    water.json               # mapped features when present; otherwise explicit empty/absent
    paths.json
    buildings.json           # optional sparse context
    vegetation.json
    play_sites.json          # authored hypothetical management sites
    scenario.json
    provenance.json
```

The schema must be versioned, documented, and validated, but only needs to support this game's actual data. No remote world catalog or regional source-adapter framework is required.

### Example `scenario.json` (illustrative, not local agricultural data)

```json
{
  "schemaVersion": 1,
  "id": "rural-balance-v1",
  "worldPackId": "rural-pilot-v1",
  "title": "Balance the Land",
  "fictionalScenario": true,
  "intro": "Meet the productive-land target while restoring vegetation and managing rainfall runoff.",
  "budgetCredits": 100,
  "rainfallScenarioId": "rural-rain-educational-v1",
  "objectives": [
    { "type": "productiveCapacity", "operator": ">=", "value": 60 },
    { "type": "runoffRelativeToBaseline", "operator": "<=", "value": 1.05 },
    { "type": "creditsSpent", "operator": "<=", "value": 100 }
  ],
  "toolIds": ["cultivate", "restore", "manage-water"],
  "protectedFeatureTags": ["no-edit"],
  "educationalAssumptionsNote": "Production units, demand, costs, thresholds, and management sites are hypothetical."
}
```

Every numeric value here is a balance placeholder. Productive capacity is total current illustrative capacity including existing uses; deltas account for displaced productive cover. It is not a crop model. Use an absolute runoff-delta rule when a ratio is undefined because baseline runoff is zero.

### Runtime state

Keep **BaselineWorld** immutable; **ScenarioDefinition** holds rules and objectives; an ordered **Intervention** list records proposals; **WorldState** and **SimulationResult** are derived; **Snapshot** stores undo/retry state. Preview and commit use the same evaluator. Recompute the small world directly before introducing incremental caching.

Use multiple tiny technical fixtures for correctness, but maintain only one authored player-facing location and mission. Test data is not a second level.

---

## 11. Future Locations and Play Your Neighborhood — Explicitly Deferred

A future game could let players enter a place or coordinates and load another real landscape. City scenarios may also be valuable later. **Neither is required for the initial goal, hackathon demonstration, or completion of the active phases.**

Do not build a second WorldPack to prove reuse now; do not add a level picker, geocoder, geolocation prompt, generation service, or AI scenario selector. The initial product opens directly into its single rural experience. A versioned local data file and clean separation of baseline/state are enough architectural preparation.

Expansion can be reconsidered only after the one-location quality gate in section 8 and an explicit decision to take on more content. At that point create a new scoped plan rather than treating the old multi-level roadmap as queued work.

If future location generation is approved, preserve these constraints: deterministic GIS data rather than invented maps; vetted compatible scenarios; honest missing-data/failure states; bounded external processing; optional source-grounded AI context only; and opt-in geolocation with manual input and minimal storage. The finished first location must remain usable without those services.

Until then, engineering and design time goes into the first location's complete experience and repeated refinement.

---

## 12. The Environmental and Game-State Model

### Modeling philosophy

- Start with **one well-explained causal environmental relationship** and get it working end to end.
- Prefer reproducible simplified calculations over attractive but ungrounded numbers.
- Model **runoff generation / stormwater response**, not exact flood depths or urban sewer performance.
- Distinguish geographic facts, estimated model inputs, and fictional rules.
- Run the **same rainfall and assumptions** for baseline and proposal.
- Keep the current model isolated from presentation; do not build a future-model framework or add erosion/vegetation simulations before the first game's actual lesson needs them.

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

### Game-state calculations

After a placement:

1. Validate eligibility and the available credit budget.
2. Apply a reversible intervention overlay to the affected site/grid cells.
3. Recalculate total illustrative productive capacity, including lost/displaced existing capacity, affected area, cover, and remaining credits.
4. Update runoff parameters in affected sites/cells.
5. Recompute site or aggregate direct runoff for the chosen rainfall profile (or update the cached affected subarea).
6. Apply any spatially bounded green-infrastructure retention treatment.
7. Re-evaluate goals and produce a short evidence-linked explanation.
8. Render the new world and UI state.

At this small scale, preview and commit should use the same deterministic evaluator and match. Optimize only if profiling shows an actual problem.

### Why this is sufficient for MVP

An illustrative productive-land target represents a community need; its units are explicitly game assumptions rather than predicted crop yields. Land conversion and restoration affect environmental outputs. Budget creates economic scarcity. Geographic site suitability makes placement strategic. The player experiences an **environmental + social + economic** trade-off through one mechanic.

### Optional storm reveal

A short cinematic before/after visualization may show rain and modeled runoff contributions or pathways. However:

- an aggregate runoff model doesn't calculate detailed flood depths;
- rural ditches, soils, channels, and management practices can affect real outcomes beyond the simplified model;
- don't fabricate precise inundation maps or claims that a specific building would flood;
- clearly label animated runoff paths as illustrative unless flow routing is explicitly implemented and tested.

### Future model modules

- Erosion and sediment exposure for highland levels (potentially RUSLE/InVEST after proper data prep).
- Water scarcity and vegetation suitability in drylands.
- Ten-year establishment and maintenance assumptions for restoration.
- Multi-scenario comparisons and uncertainty sensitivity.

These are **deferred possibilities**, not required to complete the rural game. Evaluate improvements to the same location before considering a new biome or level.

---

## 13. Unity Architecture

### Recommended runtime stack

| Layer | Choice | Responsibility |
|---|---|---|
| Game | Unity 6 / C# | Rendering, input, state, simulation, objectives, UI |
| Renderer | URP | Web-compatible lighting, materials, vegetation, VFX |
| Front-end web shell | Lightweight HTML/JS wrapper or Unity Web template | Loader and browser presentation for the one game |
| GIS prep | Python + GDAL/Rasterio/GeoPandas/Shapely | Fetch, clip, project, validate, simplify and package GIS |
| World data | Static files | One packaged rural WorldPack |
| Hosting | Vercel for web shell/static Unity output; CDN/object storage as required | Shareable URL |

### Repository layout

Keep the existing Unity project at the repository root: `Assets/Terra/`, `Packages/`, and `ProjectSettings/`. Add `worldgen/` for offline preparation, `worlds/rural-pilot-v1/` for the one pack, and `docs/` for contracts, provenance, assumptions, and evidence. The detailed plan defines the exact module ownership and handoff paths.

Use code-owned, data-driven systems rather than one large hand-edited scene. Commit Unity `.meta` files, preserve GUIDs, and avoid simultaneous edits to shared scenes/prefabs. Small direct implementations are preferred to generalized frameworks.

### Key code modules

- **`WorldPackLoader`** — validates schema, opens packaged geography, reports missing layers.
- **`GeoTransform`** — projects/matches world data to Unity local coordinates.
- **`TerrainRenderer`** — constructs terrain mesh/material layers; LOD/mesh quality.
- **`FeatureRenderer`** — vegetation, cover patterns, water/path context, and optional sparse structures.
- **`PlacementController`** — shared hover/preview/commit/undo logic.
- **`InterventionRules`** — one source of truth for allowed sites, cost, footprint and effects.
- **`WorldStateStore`** — immutable baseline plus reversible user interventions.
- **`RunoffSimulation`** — deterministic baseline/proposal calculation.
- **`ObjectiveEvaluator`** — evaluates active scenario conditions.
- **`FeedbackPresenter`** — turns changed variables into brief understandable feedback.
- **`ScenarioLoader`** — loads authored objectives/tutorials from data.
- **`BeforeAfterView`** — world overlay/camera transition and result comparison.
- **`ProgressionPresenter`** — level intro, success, learning check, continue building.

### Web constraints and deployment

- **Build and deploy a minimal Unity Web project before doing large-scale world work.** This must be an early feasibility gate.
- Target modern **64-bit desktop browsers with WebGL 2 and WebAssembly**; verify the exact engine/browser compatibility and test the deployed build in Chrome. Chrome is sufficient for project acceptance; test Safari or Firefox only when the user explicitly requests it. Do not request Safari or Private Browsing unlocks for routine verification.
- Use URP features and shaders that work in Unity Web builds; don't assume all desktop GPU effects will work.
- Watch memory usage, static asset sizes, network transfer, shader variants, batching and GPU draw calls.
- Package only the one rural world; measure and reduce its load size before adding loading infrastructure.
- Configure `Content-Type` and `Content-Encoding` correctly for `.wasm`, `.js`, and compressed Unity output. Use a fallback compression configuration if the host cannot serve the required headers.
- Check the chosen static host's actual upload/asset constraints. Reduce the one-world bundle first; use separate asset storage only if needed. Offline GIS preprocessing is sufficient, and a live external worker is outside the current scope.
- Never mark success based on Unity Editor alone; the **deployed browser build** is the actual product.

Performance targets are goals, not claims: smooth camera/editing on a typical laptop, acceptable initial download and loading feedback, no frame-long freezes for simple placements, and graceful quality settings. Measure real browser performance after the first Web build.

---

## 14. Development Order — One Location Across the Full Game

[IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) is the detailed execution companion. Its phase numbering is canonical; the outline here uses the same phases. The editable-world foundation still precedes campaign framing.

| Phase | Scope | Gate |
|---|---|---|
| **0 — Browser gate, local only** | Verify setup, minimal scene/input, actual Web export and deployment | Reproducible working browser URL |
| **1 — Foundation, local only** | Select one rural AOI through a short data/visual review; establish camera/UI/placement fixture; freeze small contracts and handoff baseline | One chosen location, coherent interaction style, runnable contracts/fixtures |
| **2 — Rural hero landscape** | Load that location's real terrain/cover/context, author management sites, compose lighting/vegetation/camera | Attractive geographically aligned landscape with provenance |
| **3 — Editable-world foundation** | Cultivate/Restore, preview/commit/move/replace/remove, history, budget and productive-capacity accounting | Satisfying reversible edits on the same real map |
| **4 — Connected consequences** | Direct-runoff model, bounded Manage Water, same-storm comparison and explanations | Correct, spatially meaningful trade-offs and matching preview/commit |
| **5 — Complete rural mission** | Onboarding, balanced goals, multiple solutions, recoverable failure, novice review | Understandable and achievable challenge in the same location |
| **6 — Full ending/replay loop** | Before/after, transfer question, Take It Home, Continue Building and retry | Complete learning and replay experience without changing maps |
| **7 — Refinement** | Improve scene/UI/audio, accessibility, performance, balance and usability; retest after changes | Consistent high quality across the entire first location |
| **8 — Release and continued refinement** | Deploy, rehearse, verify complete playthrough, document reproduction and limits | Dependable single-location game and a focused improvement backlog |

Phases 0–1 stay on the primary computer. Later parallel data, simulation, art, and content work all feeds **the same rural location**. Revisit earlier systems as needed; the phases are not a reason to abandon quality issues after their first pass.

There is **no active phase 9 for another world**. After release, iterate the first location's full game scope and observed issues. Expansion requires the section-8 quality gate plus a separate explicit decision; do not create more content merely because the next implementation task is difficult.

---

## 15. Single-Location Definition of Done

### Required full game scope

- [ ] One real rural location, accurately grounded and beautifully presented in a deployed Unity Web build.
- [ ] A shared choose/preview/place/transform/adjust loop with **Cultivate, Restore, and Manage Water**.
- [ ] Reversible editing: move, replace, remove, undo/redo, and retry, without corrupting baseline state.
- [ ] Linked productive-capacity, finite-budget, cover, and same-storm runoff consequences.
- [ ] Connected, bounded water treatment and clear eligibility/protected-area rules.
- [ ] A balanced mission with multiple materially different winning designs in the same map.
- [ ] Clear onboarding, immediate explanations, readable UI, and forgiving camera/input.
- [ ] Before/after comparison, one transfer question, a useful takeaway, and Continue Building.
- [ ] Strong visual/audio consistency, accessibility basics, and measured browser performance.
- [ ] Provenance and a clear distinction between real geography, hypothetical agricultural/game assumptions, and estimates.
- [ ] Reproducible build instructions, an accurate demo, and reliable fallback assets.

### Optional polish within this location

A short rain reveal, extra environmental detail, richer placement feedback, or better comparison presentation may be added when they improve the tested experience. They must not replace correctness or meaningful choices.

### Outside this definition

A second world, city content, a campaign/level selector, live location generation, AI context, and broad scalability are **not required, not current stretch goals, and not automatically next**. If time is short, simplify effects/variants/site counts first. Any cut to required full-game scope must be recorded as incomplete rather than hidden behind a new map demo.

---

## 16. Verification & Testing

### Automated tests

- Geographic coordinate projection/inverse and layer alignment.
- WorldPack schema, version compatibility, bounds and missing-data behavior.
- Intervention eligibility, credits and occupied-space accounting.
- State determinism: same baseline + interventions + assumptions ⇒ same result.
- Baseline immutability and undo/redo equivalence.
- Productive-capacity arithmetic, displaced existing use, and protected-land restrictions.
- Direct-runoff numerical cases, nonnegative outputs and unit consistency.
- Green infrastructure capacity and contributing-area boundaries.
- Model comparisons using the identical event/assumptions.
- Scenario reachability: test at least two scripted valid solutions to the main objectives.

### Manual quality tests

- Site selection doesn't jump relative to geometry when camera or zoom changes.
- Terrain/cover/water/path layers line up after import.
- Short tooltips explain why placement is invalid.
- Web build loads without unsupported shaders or missing asset requests.
- A player can understand the three tools without a developer standing beside them.
- No false exact flood claims, fake local permits, or invisible environmental bonuses.
- Basic keyboard navigation, captions/non-color indicators, and reduced effects option.

### Educational user test (small pilot)

Recruit a handful of new players in or near the 15–24 group if permitted and available. Observe whether they can:

1. Place and modify land-use changes without instruction.
2. Explain at least one benefit and cost of a choice.
3. Answer a new planning question using the concepts learned.
4. Suggest one real community-planning question after playing.

Record observations and improve the design; **do not claim formal educational effectiveness from a small hackathon pilot.**

---

## 17. Hackathon Demo Script — 3–4 Minutes in One Place

**0:00 — Reveal the landscape.** Show the actual selected rural place, its name and source overlay, and a composed camera view. Describe only geographic layers that really came from data.

**0:30 — Present the human need.** “This hypothetical rural community needs productive land, but land and resources are limited.” Show production, rainfall-runoff, and budget goals with their assumption labels.

**0:50 — Make a meaningful choice.** Preview a cultivation treatment, place it, and show the terrain/vegetation transformation and the real metric changes.

**1:20 — Reveal the trade-off.** Compare another site or treatment. Explain the actual cover/connection/cost difference rather than a scripted bonus.

**1:50 — Adjust the proposal.** Restore a compatible site or add connected water management; show its bounded effect and a balanced result.

**2:20 — Compare.** Use the same camera and same rainfall assumptions for before/after. Do not present runoff as precise flood depth or crop-yield change.

**2:50 — Connect learning.** Show the unseen-example question and the useful land-use question card.

**3:10 — Demonstrate depth in the same place.** Continue Building, undo a choice, or briefly show a second valid proposal on this map. Show that players can experiment; do not switch to another level or spend the finish promising a generation platform.

**3:30 — Close.** “Terra makes rural land-use trade-offs visible, understandable, and playable.”

The judge takeaway is one complete, polished game: real landscape, meaningful choices, honest consequences, and an educational ending. Future locations can be mentioned as possibilities only if useful; they are not presented as implemented features.

---

## 18. Roadmap After the First Release — Deepen the Same Game

| Period / gate | Main work | Outcome |
|---|---|---|
| Hackathon | Complete one rural game through phases 0–8 | Public browser experience with the full interaction and learning loop |
| Early follow-up | Review novice play, balance, source/model assumptions, accessibility, load/input performance | A stronger version of that same location |
| Further refinement | Improve the existing landscape, interventions, feedback, ending, and replay; resolve the highest-impact issues first | Stable core rules and a consistently polished single-location game |
| Only after the quality gate and an explicit expansion decision | Scope one additional location or capability based on actual needs | A separate plan with an understood maintenance cost |

There is no calendar promise to add a second location in a particular week. Avoid duplicating authored content while the common mechanics and UI are still changing. Scientific/educational review should precede stronger claims about real outcomes; production scalability work is not an initial requirement.

---

## 19. Risks, Guardrails & Explicit Non-Goals

| Risk | Mitigation |
|---|---|
| Beautiful demo, weak game | Prioritize choose/preview/place/feedback and test with a novice before adding effects |
| Many features, no coherent loop | One placement mechanic, three intervention families, one budget, a few competing goals |
| Real geography but imaginary accuracy | Date every layer; identify stylized/inferred content; separate real geography from fictional scenario |
| Treating rural land as unused or legally convertible | Curate hypothetical management sites and distinguish scenario assumptions from mapped facts |
| Inaccurate flood claims | Estimate direct runoff only; do not predict inundation or sewer system performance |
| Water-management features have magic global effects | Constrain action by site, contributing runoff, physical footprint and capacity assumptions |
| Multiple levels multiply rework | Maintain one canonical rural world until all core systems and the full experience are refined; expansion needs a separate decision |
| AI hallucinates local data | AI only selects vetted scenario templates and drafts sourced explanations |
| Unity Web build too heavy | Browser-first iteration, URP web-compatible effects, asset LODs and incremental world loading |
| Vercel deployment limitations | Correct WebAssembly/compression headers, use separate object storage/worker as required |
| Permission/location privacy | Ask explicitly, minimize storage, permit manual inputs |
| Educational impact asserted without evidence | Short before/after transfer question; distinguish understanding from behavior change |
| Premature geographic expansion | Ship and refine **one complete rural game**; no second map, city, or live generator in the active phases |

Do not build: multiplayer workshop, detailed construction logistics, professional planning exports, detailed zoning/ownership predictions, citywide flood maps, climate-conflict predictors, a required ten-year management minigame, speculative finance systems, or paid content.

---

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

## 21. Codex Implementation Guidance

This section guides work **within the user's requested phase or goal**. It does not authorize starting the whole roadmap merely because this file is present.

### Mission

Build the highest-quality playable **single rural location** described above. Rural land use is the primary product use case. Perfect the full experience there before considering additional rural or city content.

### Rules for implementation

1. Inspect and preserve useful existing repository/setup work; follow the detailed plan and evidence ledger.
2. Verify a real browser build early, then keep validating browser behavior after meaningful changes.
3. Work in complete slices: real landscape → placement → state change → consequence → understandable feedback → mission/learning.
4. Choose and document the rural AOI during phase 1, then keep one canonical playable location throughout iteration.
5. Keep data and state separate from presentation for easy handoffs, without building a generalized platform.
6. Derive visible numbers from actual state. Label illustrative productive units and model assumptions; do not invent local yields, hydrology, or legal rights.
7. Make design/UI quality a gate throughout the work, not a final skin. Complete Phase 2A before Phase 3, consult docs/DESIGN_REFERENCE.md for every player-facing change, and preserve its accepted benchmark through later phases. Test the opening view, tools, invalid states, and ending in the browser.
8. Let parallel engineers contribute data, math, assets, and content for this same location; avoid shared-scene ownership conflicts.
9. Implement all three tools and the full edit/mission/comparison/learning/replay scope before claiming the game complete.
10. Prefer direct code, small authored site lists, offline data prep, and simple hosting to scalability work. No AI runtime dependency.
11. Keep the current location working as mechanics change. Update its balance, visuals, and tests instead of cloning it into more levels.
12. Do not begin a second location, city, level selector, live generator, or extra biome/model without a new explicit scope decision after the one-location quality gate.
13. Record implementation evidence and honest gaps; distinguish completed behavior from the vision.

### First concrete tasks

1. Verify the existing Unity setup and deploy a minimal browser prototype.
2. During local phase 1, compare a short rural shortlist using source coverage and visual previews, choose one AOI, and establish the interaction/art/contracts baseline.
3. Generate one validated rural WorldPack with terrain, cover, relevant water/path context, and hypothetical management sites; render it attractively.
   Before the next gameplay step, complete Phase 2A's first-principles visual rebuild. The current implementation is replaceable; the direct rendered quality must meet the Terra Nil-led gate.
4. Implement Cultivate/Restore and reversible placement with budget, productive-capacity, and cover accounting.
5. Integrate runoff and Manage Water, then finish the mission, comparison, learning, replay, and repeated polish **on that same map**.

### Completion

The complete first game runs in a real browser; a new player can make and revise choices, see and understand their consequences, complete one balanced scenario in multiple ways, learn from it, and continue experimenting. The real rural geometry, hypothetical rules, and model limitations are transparent. Visuals, UI, input, and sound form a coherent experience. The build and data preparation are reproducible. No second location is needed to meet this definition.

---

## Final Product Summary

**Terra: Shape Your World** is a free educational rural land-use strategy game for ages 15–24. Its first product is one real rural landscape, beautifully rendered and fully playable in the browser.

The working scenario balances illustrative productive land, restoration, water management, and finite resources through one shared placement loop. Players preview, transform, revise, compare, learn, and continue experimenting in that same location.

Structured geographic inputs and deterministic state make the game easy to iterate and collaborate on. They do not require a world-generation platform. Rural is the primary use case; urban settings and additional locations are deferred until the entire first experience has been refined and a new expansion decision is made.

**One rural location. Full game scope. Repeated refinement before more levels.**
