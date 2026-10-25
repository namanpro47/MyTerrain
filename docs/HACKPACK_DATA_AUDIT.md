# Hackpack and simulation data — October 25, 2026

## Decision

Use one **Coon Valley / Vernon County, Wisconsin** numerical reference for weather, farming and economic benchmarks across fictional scenario names and selected terrain. Terrain source provenance and per-location saves stay separate. County/state series retain their true geography; scenario budgets, construction prices, local exposure and response curves remain authored where evidence is missing.

Inspect supplied hackpack data first. Prefer it if it supplies suitable documented inputs with less preparation than an external source. Use NOAA historical daily weather when that is the quicker validated path. Do not block implementation on locating the bundle, and do not claim unseen data is unsuitable or absent.

## What was actually found

- `docs/HACKATHON_TOOLKIT.md` and `docs/toolkit-sources.json` inventory Unity tooling, Kenney assets/sounds and DOTween. They are not the organizer's environmental/economic dataset bundle.
- `worldgen/source-lock.json` inventories downloaded elevation TIFFs, NLCD cover and mapped water/road files, with sizes/hashes. These support terrain/cover/context, not a historical weather series or crop/economic response distribution.
- `worlds/meadowbrook-v1/provenance.json` preserves the Wisconsin map's source/assumption boundary. Other terrain packs preserve their own provenance; none becomes local economic/weather calibration merely by sharing game rules.
- `docs/evidence/package-b/source-review.json` records the inspected official TR-55 reference. It supports the existing educational direct-runoff model, not crop yield or actual flood damages.
- PRD evidence entries R1/R2/R5 identify USDA Wisconsin county rent/yield benchmarks, practice-specific public assistance rates and NOAA GHCN-Daily. They are source candidates/references, not a completed local weather pack or calibrated response model. No new price/rate verification was performed for this requirements update.

## Search boundary and unresolved bundle

At refreshed `origin/main` `b11a2fc`, searched repository documentation/source inventories for hackpack/dataset/weather references and checked top-level Downloads names for hack/terra/dataset/UNCCD/pack. No matching local bundle was found in those bounded locations.

Connected Google Drive searches for `hackpack`, `UNCCD`, `hack` and `land degradation` found no matching dataset bundle. The relevant document **Naman & Peter Hack!** was found; its readable fetch returned empty content, so it supplied no dataset inventory. Unrelated Drive results were not read. These searches do not prove the hackpack was never supplied or cannot be accessed elsewhere.

The public [2026 event page](https://www.hackforsocialimpact.com/events/sf-2026) describes the UN land-planning challenge but exposes no dataset manifest in the inspected content. The [linked challenge brief](https://hackforsocialimpact.notion.site/Engaging-Youth-in-Sustainable-Land-Use-Decisions-4002cb772f1e80649effe9041c2957bd) was inaccessible through the web read tool. No hidden/private bundle contents are inferred.

## Adoption checklist for each actual input

Record dataset name/provider, purpose, exact file/URL, geographic and temporal coverage, units, license/attribution, missing-value/quality flags, preparation steps, version/checksum and observed/benchmark/authored/simulated evidence label. Map each input to a real simulation field before adoption.

- **Weather:** precipitation/temperature and coherent season/year blocks near Coon Valley; station distance/elevation, coverage and missing days checked. NOAA GHCN-Daily is the fallback candidate. No selected station/period or data download is claimed yet.
- **Agriculture/economics:** regional crop and price/rent/program series where suitable; distinguish public contributions, private revenues and crop output. A county yield is not a field measurement; a single project quote is not a probability distribution.
- **Environmental/social context:** land cover/soil/condition or population/service information only within its supported resolution/scope. No dataset name by itself proves treatment causality or supports a per-building score penalty.
- **Authored gaps:** publish effect ranges, demand, exposure/connection groups, upkeep, repair responsibilities and score weights as game assumptions. Validate sensitivity; do not disguise arbitrary ranges as observed probabilities.

## Status

Dataset discovery and requirements only. No hackpack contents were downloaded or validated; no NOAA station selection, weather sampler, recalibrated economic rules, new scores, UI fallback or deployment was implemented. The available path is to proceed with the existing source register plus a validated NOAA weather subset, and adopt the supplied bundle if its actual files become available and fit the needed inputs.
