# Open-land parcels and an inhabited opening

Peter authorized a denser uniform layout across all three locations on October 25, 2026. Forest clearing, building over rivers and redevelopment of the existing community remain outside the game.

## Parcel layout

The base grid is **60 × 60 m** on every map. Full polygon intersections exclude any woodland, water or sealed source cover; mapped feature corridors have a width-aware extra 8 m buffer. Original protected polygons are retained. A cell also needs at least one standard home foundation with no more than 3 m relief. Each project's existing footprint/foundation check still applies: an open slot does not promise that every building fits there.

Three 2 × 2 groups and one 3 × 3 group are joined without changing their underlying grid alignment. One 2 × 2 group holds the inherited village square; two remain available for larger works. The 3 × 3 envelope fits the existing 150 m wind clearance. This avoids shrinking models or adding overlapping selectable sites. Larger envelopes can accept any physically suitable project; choosing a small project there spends that larger opportunity.

- Meadowbrook: 56 qualifying base cells → 39 joined/single plots → **32 available**, seven inherited. Two original protected sites remain separately intact.
- Point Reyes: 329 qualifying base cells → 312 plots → **305 available**, seven inherited.
- Suusamyr: 357 qualifying base cells → 340 plots → **333 available**, seven inherited.

Different counts reflect the open area in each pack. Wisconsin has extensive sourced woodland and mapped corridors. Regional cover is explicitly authored and mostly open; it is not a survey or a claim of actual development availability. The overlay covers the source terrain domain, including open areas outside the original tiny opportunity rectangle.

## Story and starting buildings

Before this change all locations shared the same annual opening text and 4-home/4-job baseline. They had geographic labels, but no separate narrative setup or inherited building inventory.

Version 4 supplies three short **fictional** briefs: “Farms beside the creek” (Meadowbrook), “Room for a rural community” (Point Reyes), and “A foothold in the valley” (Suusamyr). These describe the selected landscape and available choices without inventing local statistical calibration, historical claims or a new economy.

Each map begins with four visible homes, one farm store, one village square and one recreation ground, on seven reserved plots. Existing metric prefabs and foundation checks are reused. These objects visualize the already configured 4 homes, 4 jobs, 4 recreation and 4 local-service units; they are not seven purchased actions and do not award new-project capacities, receipts or costs. Existing utilities remain abstract external baseline supply.

Building lots keep source cover outside the bounded footprint; authored grass/sealed fractions replace the footprint in the opening runoff baseline. The source terrain, cover polygons, features, source packs and their checksums are unchanged. The source cover partition outside managed plots is conserved, and each new parcel receives a disclosed isolated runoff group. Grid swale capacity is an authored 100 m³/ha assumption, not an observed connection or routed catchment.

## Compatibility and generation

Layout/rules version **4.0.0** uses separate annual and chapter save keys for each location. The previous 3.1/3.2 keys are retained. A changed polygon layout cannot reinterpret or overwrite accepted old histories.

The original loopback .NET chapter service accepts a bounded 16 MiB request for the expanded catalog and history. The measured complete ten-year dense-map inputs are approximately 7.74/8.42 MB with indented JSON; the former 1 MiB cap would reject them. This is local prototype compatibility, not a hosted backend deployment or provider-connection check. The separate compact JavaScript narration work remains owned by its chat.

Run `python scripts/prepare-parcels.py` with Python 3 and Pillow. This deterministic, offline preparation reads canonical world/hydrology inputs and writes `Assets/Terra/Resources/Parcels/*.json` plus labelled plan views. The runtime requires the overlay's source-world SHA to match the loaded pack, preserves original protected polygons and verifies area conservation before applying it. Terrain and source cover are never regenerated.

Validate with `dotnet run --project tests/parcels/ParcelsChecks.csproj`. The harness exercises the production parcel adapter, all 17 project variants, inherited building fit/protection, financial/water conservation, ten years and save restore on each map. Source/model checks do not establish browser appearance or performance. See `evidence/map-parcels/` for current receipts and diagnostic plan views.

Design references: **TN-01/TN-02** preserve readable landscape masses and intact woodland/water; **TN-05/CS-02** expose valid destinations through the existing authoritative availability highlighting; **TN-06** keeps each fictional opening brief concise. Actual rendered acceptance remains separate from plan-view inspection.
