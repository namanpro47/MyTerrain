# Regional natural hazards and saved annual results

Research started October 25, 2026. Source base: refreshed `origin/main` `13a15a4`.
Scope: source-backed event inventory and likelihood research for the three existing regions; no runtime, probability activation, new action, UI or deployment changes.

## Decision in plain English

The **engine produces the numbers; the LLM explains the saved numbers**. Each region needs a small catalogue of possible hazards with an event definition, geographic scope, severity, evidence and likelihood measure. A catalogue entry is not an event that happened.

We can identify useful regional hazards now. **The researched sources do not yet justify one exact annual chance of damaging every game site for each hazard.** They provide a mixture of broad planning classes, historical occurrences, multi-year shaking probabilities and weather-condition frequencies. The accompanying [research catalogue](../simulation-data/hazards/regional-hazards.research.json) preserves those distinctions and leaves engine probabilities unset. Null means unverified, not zero risk.

Peter's latest decision makes hazard research region-specific. The shared Coon Valley/Vernon County economic reference remains. The existing Wisconsin crop-weather pack remains a separately identified benchmark; it is not evidence of Point Reyes or Suusamyr disaster frequency. Before activation, select one authoritative driver per hazard channel so a sampled wet year and an independent flood roll do not charge the same event twice.

## How values advance into next year

The model-generated `title/introduction/cards` response is presentation text. It does not need to contain mutable ESE scores or treasury values because those already belong to the engine's accepted result.

1. Freeze choices, progress, upkeep policy, persistent site state and the saved external conditions.
2. The engine resolves refunds, paid upkeep, output, event effects, recovery, the public ledger and Economic/Social/Environmental scores using pinned rules.
3. Validate and persist one accepted transition and its resulting state. Failed numeric resolution/save stays pending; it cannot be covered up with old results.
4. Request LLM prose using that transition's facts, relevant accepted history and regional hazard context. On provider/credit failure, show a labelled local summary of the same saved facts.
5. Continue into the next planning year using the accepted closing cash, remaining obligations, assets/site condition and scores as the comparison baseline. No second refund, appropriation, event draw or score bonus occurs just because Continue was pressed.

Illustrative accepted numeric record, in integer USD cents; not an implemented new contract or a real disaster simulation:

```json
{
  "transitionId": "example-run/year-3",
  "year": 3,
  "ledger": {
    "openingCashCents": 50000000,
    "capitalRefundCents": 20100000,
    "upkeepPaidCents": 1500000,
    "repairsPaidCents": 1000000,
    "appropriationCents": 10000000,
    "otherPublicReceiptsCents": 0,
    "closingCashCents": 77600000
  },
  "scores": {
    "economic": {"value": 61, "change": -2},
    "social": {"value": 60, "change": -5},
    "environmental": {"value": 52, "change": -6}
  },
  "nextPlanningYear": 4,
  "nextOpeningCashCents": 77600000
}
```

This compact example omits the required full site state, remaining commitments, causal fact IDs, rules/input hashes and accepted events. Production saves need those as well as year-by-year reports and optional chapter text. Cancellation is a new transaction in the current year: the agreed progress-based refund applies to eligible public capital, without rewriting previous years or refunding consumed upkeep. Upkeep-funded operating benefits are the chosen simplification; this research does not add a separate utility-allocation simulation.

Year-end prose explains the accepted year's values. The ten-year prose uses all accepted reports and the same-conditions comparison. Local browser/Unity persistence must save numeric results independently of narrative availability; another device or cleared browser storage is not an implied recovery mechanism.

## Coon Valley / Vernon County, Wisconsin

Reference game centre: 43.7015, -91.037. County evidence is broader than the small editable landscape.

- **River/flash flooding and stormwater:** primary first-slice candidate. Vernon County's 2023-2028 plan rates flooding **Likely**. NWS documents the August 2018 Coon Valley flood. No parcel-level annual damage probability is established. [County plan, printed pp. 24-29](https://cms8.revize.com/revize/vernoncountywi/Vernon%20County%20HMP%20-%20WEM-%20FEMA%20Approved%20v2-%20FINAL.pdf#page=32), [NWS event](https://www.weather.gov/arx/aug2818).
- **Drought:** plan rating **Possible**; retain a seasonal condition, rather than treating each dry day as another disaster. [Plan section 3.15, beginning on PDF p. 54; summary also on PDF p. 57](https://cms8.revize.com/revize/vernoncountywi/Vernon%20County%20HMP%20-%20WEM-%20FEMA%20Approved%20v2-%20FINAL.pdf#page=54).
- **Severe thunderstorms/hail:** thunderstorms are **Highly Likely**, hail **Likely**, at county scale. Group linked wind/hail damage under its originating storm. Tornadoes are **Possible**, not interchangeable with every thunderstorm. [Plan, printed pp. 14-23](https://cms8.revize.com/revize/vernoncountywi/Vernon%20County%20HMP%20-%20WEM-%20FEMA%20Approved%20v2-%20FINAL.pdf#page=22).
- **Heavy snow/ice:** both **Likely** in the plan; candidate for construction/asset interruption rather than automatically reducing a completed summer harvest. [Plan, printed pp. 34-38](https://cms8.revize.com/revize/vernoncountywi/Vernon%20County%20HMP%20-%20WEM-%20FEMA%20Approved%20v2-%20FINAL.pdf#page=41).
- **Lower-priority checks:** wildfire is **Possible** with low overall county risk in this plan; retain heat/cold and slope failure as later candidates needing separate event definitions and exposure. These are not declared impossible or assigned zero probability.

The plan's probability criteria are broad and contain mixed horizon wording (printed p. 12, PDF p. 20). Preserve the original category; do not turn a 1-10% band into an invented 5% site-level constant. A risk score such as 30 points is not a 30% annual likelihood.

### Existing weather evidence we can reuse

Current main already includes [24 complete La Crosse airport years, 2001-2024](../simulation-data/weather/README.md), with station/source/pack hashes and coherent ten-year windows. Airport observations are regional reference data, not readings at the game fields. This research recomputed the following **sample frequencies**, using the pack's authored May-September window:

- At least one day with 25 mm liquid-equivalent precipitation: **24/24 years (100%)**.
- At least one day with 50 mm liquid-equivalent precipitation: **16/24 (66.7%)**.
- At least one day with maximum temperature at least 35 C: **20/24 (83.3%)**.
- A run of at least 21 days below 1 mm/day: **5/24 (20.8%)**.

These are reproducible weather-threshold counts, **not flood, heat-disaster or drought-damage probabilities**, and not independent draws to layer over an already selected historical schedule. Threshold choice is authored. The [machine-readable derivation](../simulation-data/hazards/wisconsin-weather-proxies.research.json) records qualifying years and exact source hashes. Canonical Git blobs match the weather pack's published byte pins; this Windows checkout has CRLF differences, with identical parsed observations. Counts were independently checked against the pinned canonical daily companion. Do not package hash-mismatched working copies or silently change a runtime hash check; no weather files were edited here. If runtime uses the existing historical blocks, derive these conditions from those blocks rather than drawing them again.

## Point Reyes Station / western Marin, California

Reference game centre: 38.066, -122.810. The pack is near Point Reyes Station; a coastal park-wide hazard does not establish exposure on this inland footprint.

- **Wildfire:** documented locally, including the 2020 Woodward Fire. Keep ignition, hazardous fire weather, spread/exposure and damage distinct. Two named fires do not define an annual probability. [NPS Woodward Fire](https://www.nps.gov/pore/learn/nature/wildlandfire_woodwardfire.htm).
- **Earthquake:** strong regional relevance. The USGS **2016 publication** gives a **72%** chance of at least one M6.7+ event somewhere in the Bay region during **2014-2043**, and **22%** on the San Andreas Fault in that same window. These are archival, threshold/area-specific estimates, not current annual Point Reyes damage probabilities; do not restart their clock in 2026. An updated primary local shaking estimate remains to be verified. [USGS fact sheet](https://pubs.usgs.gov/fs/2016/3020/fs20163020.pdf), [publication record](https://www.usgs.gov/publications/earthquake-outlook-san-francisco-bay-region-2014-2043).
- **Drought, storm flooding and severe wind:** supported county-level candidates. Local channels/exposure and a severity threshold are needed before converting a county occurrence to damage on a game site. [Marin hazard maps](https://www.marincounty.gov/departments/fire/office-emergency-management/disaster-preparedness/local-hazard-maps).
- **Landslide/post-fire debris flow:** retain slope/connection dependence and causal linkage to its storm/fire/earthquake, rather than drawing unrelated full losses on top of the triggering event. County plan category: **Occasional**.
- **Exposure-gated/deferred:** tsunami/coastal flooding require actual shoreline/inundation overlap with the playable footprint; sea-level rise is a background trend rather than a yearly random catastrophe. No tsunami activation solely because the map is called Point Reyes.

### Important county-plan limitation

The [Marin 2023 plan](https://storage.googleapis.com/proudcity/marindpwca/2024/05/Marin_County_MJHMP_2023_Final_resized1.pdf) Table 3.4 (PDF p. 5, printed iii) labels wildfire, earthquakes, flooding and drought **Highly Likely**. Its rubric on PDF p. 117 (printed 109) defines this as 90-100% occurrence in the next year, while its earlier definitions differ in recurrence wording. The earthquake severity table also includes small felt events. **This cannot be used as 90-100% annual chance of a destructive earthquake, destructive tsunami or a burned game field.** Preserve the broad planning class; require event severity/exposure before selecting numeric gameplay odds. PDF pages 5 and 117 were rendered and visually checked.

## Suusamyr Valley, Kyrgyzstan

Reference game centre: 42.18, 73.96. Ministry material places Suusamyr within Chuy's mountain geography. Chuy-wide screening mixes very different elevations and landscapes.

- **Earthquake:** local primary research documents the 1992 Suusamyr event and associated rupture/landslide effects. GFDRR ThinkHazard's Chuy screen is **High**, described as **more than 20%** chance of potentially damaging shaking over **50 years**. This is a screening lower bound, not an exact annual rate for this game footprint. [Primary fault study](https://academic.oup.com/gji/article/216/1/703/5151338), [GFDRR screen](https://thinkhazard.org/en/report/147294-kyrgyz-republic-chuy/EQ).
- **Landslides/rockfalls and debris-flow/flooding:** the Kyrgyz Ministry describes Suusamyr-related slope and water hazards; ThinkHazard screens Chuy landslides **High**. Rain, snowmelt and shaking can share an event chain. The current-game AOI's affected slopes/channels are not established. [Ministry regional assessment, Russian](https://www.mchs.gov.kg/ru/structures_old/kr-okm-chui-oblusu-boyuncha-bashkarmalygy/), [landslide screen](https://thinkhazard.org/en/report/147294-kyrgyz-republic-chuy/LS).
- **Avalanche/heavy snow:** the Ministry explicitly describes avalanche exposure on slopes/routes in the Suusamyr basin. Treat this as a slope/connection-dependent winter candidate, not a universal hazard on flat valley parcels. No complete local denominator for annual site impact was obtained.
- **Frost/short-season conditions:** Ministry material notes mountain growing-season frost; use a defined crop-season threshold if activated. Do not transfer lowland Chuy heat frequencies to this high valley.
- **Wildfire:** Chuy's GFDRR screen is **High** and describes **greater than 50% annual chance of weather supporting a significant wildfire**. This is fire-weather suitability across a broad region, not an ignition, burn or loss probability at Suusamyr. Keep it as a regional screening lead rather than an enabled game roll. [GFDRR wildfire screen](https://thinkhazard.org/en/report/147294-kyrgyz-republic-chuy/WF).

The Ministry page includes old statistics/forecasts tied to roughly 2014-2015; it is useful for hazard identity and geographic context, not a verified 2026 probability model. Its emergency-type shares, e.g. an earthquake share of reported emergencies, are not the probability of that event in a year. Linked Ministry PDF filenames returned 404 during direct retrieval; search snippets were not promoted to verified rates.

ThinkHazard main/alternate deployments showed differing Chuy classifications. The source IDs pin the actual inspected main-site pages and their measure wording; no profile is activated from a conflicting or regional-only screen. An urban-flood page also had inconsistent hazard/return-period wording and was excluded from numeric use.

## Small event system that stays within scope

Researching a hazard does not require implementing all its mechanics. Recommended first candidates are water stress/heavy-rain disruption for Wisconsin, wildfire/storm disruption for Point Reyes and shaking/slope-or-snow disruption for Suusamyr, subject to usable evidence and an explicit source-to-game rule. These are a prioritized backlog, not enabled probabilities or a new approval to implement every hazard.

For each enabled event, require:

- A precisely named phenomenon and severity threshold, source region/period and probability denominator (event, event-year, exceedance, weather suitability or planning category).
- A sourced likelihood suitable for the selected area/time window, or an explicitly authored game parameter that is not labelled a real probability. Unsupported exact rates stay null. Do not silently choose the middle of a broad band.
- One source of the hazard driver, saved with the scenario seed/schedule before its effects resolve. Multiple observations from one storm are not automatically independent disasters.
- Explicit affected-site exposure/connection and vulnerability rules. Broad occurrence in a county/province does not hit every parcel. A swale may affect connected storm runoff; it cannot lower earthquake occurrence.
- Bounded effects on output, condition, construction, repair/upkeep obligations and ESE components, with each loss applied once. Event damage magnitudes and score weights remain authored until separately supported.
- User-understandable choices and previews under the existing annual scope. This research adds no physical utility network, hydraulic inundation model, road simulation or health model.

## Next research steps and status

1. **Define severity first:** select which version of a flood, storm, dry season, fire or shaking event the game represents. The broad label alone has no single honest percentage.
2. **U.S. likelihood extraction:** use an official NOAA event-year extract for Vernon/Marin with a complete period and explicit county/forecast-zone filters; distinguish unique years/episodes from warning counts and duplicate reports. For wildfire, inspect official burn-probability data and actual AOI overlap; for earthquakes, inspect a primary point-shaking product with a stated threshold/window. These extracts were not completed in this initial pass.
3. **Suusamyr:** obtain local mapped/threshold-specific data or a complete official event history with geographic coverage; broad Chuy ratings and historic anecdotes remain research leads. Source language and year need review before any probability activation.
4. **Connection/exposure:** check the actual playable footprints against appropriate source layers before enabling coastal, slope or channel-specific losses. Missing exposure is unavailable, not zero.
5. **Engine handoff:** attach enabled profiles and accepted event facts to the existing annual simulation owner. Save a complete numeric result before asking for yearly/ten-year narration; no change to the numeric ledger is inferred from prose.

Delivered: this initial research document, 18 structured hazard candidates across three regions, 11 source entries, downloaded-source metadata and reproducible Wisconsin proxy counts. [Consistency checks](../simulation-data/hazards/validation.research.json) verified all source references, disabled/null runtime probabilities, canonical weather/PDF hashes and the illustrative ledger; all four weather counts were independently reproduced from the daily companion. The check explicitly records the Windows working-copy byte mismatch above. No runtime-ready exact annual damaging-site probability is certified by this pass. Hazard rolls remain disabled; the existing weather pack and authored scenarios retain their stated status. No Unity/browser/provider test or production deployment was performed.
