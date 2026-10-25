# HUD and continuous fields — October 25, 2026

Task: replace the sprawling HUD with a reference-led interface and remove empty corridors between developed fields. Starting remote: `e0c887702db8cee8e6c26c0f2ed78404f1257e92`. Isolated worktree: `terra-hud-continuous`. Visual verification is in progress; this note does not certify Phase 2A or public deployment.

## Reference decisions

Patterns **TN-03/04/05, CS-01/02 and AX-01**. Inspected the [Terra Nil 1.0.1 desktop HUD](https://www.wsgf.org/f/u/contrib/dr/30168/ingame_16x9.png) in Chrome and the [official Cities: Skylines II proposed toolbar, January 29, 2026](https://www.paradoxinteractive.com/games/cities-skylines-ii/news/upcoming-visual-updates). These are visual observations, not playtests of those games. The existing [project reference library](../../DESIGN_REFERENCE.md) remains authoritative.

- Terra Nil: localized dark blue surfaces; strong values; pictorial tools with price badges; an uninterrupted central landscape.
- Cities: Skylines: compact bottom service/status controls, stable groups and distinct indicator colors.
- Application: compact location/budget group; equal-status Economic/Social/Environmental meters; slate surfaces, mint selection, amber/blue/green indicators; miniature project art with upfront prices; three category tabs; a single bottom command strip with emphasized End Year. Exact costs, commitments, reports, settings and keyboard paths remain reachable.
- Reference imagery remains outside game assets. No image generation, purchased resources or copied reference artwork.

## Connected parcels

The old eight southern fields were separate rectangles with 40–45 m context corridors. `scripts/prepare-connected-fields.py` generates a versioned runtime override from the frozen world. Eight southern parcels now share ten edges, without overlap or internal corridors. The stepped outside edge retains mapped water clearance and avoids woodland; isolated east/west/wind opportunities and protected zones remain as before.

The generator clips the original cover rings, checks their areas, checks mapped feature distances and rejects woodland/sealed overlap. `geometry.json` records per-site source areas and clearances. Runtime removes each added cover area from unchanged context before adding the wind opportunity, so the catchment still counts each square metre once. Observed terrain, cover polygons, roads, water, source pack and location identifier are unchanged. Planting uses shared world-coordinate rows without a per-parcel inset. Connected treatment surfaces reach their exact boundary rather than fading back to baseline on every edge.

Buildings keep their metric prefab/lot dimensions and terrain-fit rules; a management parcel is not an enlarged building footprint. Field project prices remain the authored public contribution for one named agreement. The new area and source cover feed the same annual model.

This is definition **3.2.0**, with separate town and chapter save keys. Existing 3.1 saves remain untouched. Arrival explicitly tells a returning player that connected fields begin a new run and the prior-layout save was retained; historic costs, receipts and outcomes are never recomputed or refunded.

## Verification so far

- Dependency-free geometry compiler/check: eight parcels, ten shared edges, zero overlapping area; source cover totals and road/water buffers pass.
- Connected layout: **608 full-game assertions**, all 17 variants, ten-year histories, bounded meters, budget/water conservation, save replay and manual/fast-forward equality. Two viable strategies retain no arrears: 86.2/99.0/52.1 and 94.2/91.0/61.5. These are authored scenario outcomes.
- Legacy layout: **608 assertions** unchanged; annual core **94** plus gameplay **15** pass. Runs used the installed .NET 10 runtime with `DOTNET_ROLL_FORWARD=Major` for the net8 projects.
- Unity initial GUI import exited unexpectedly. One controlled restart in the same isolated worktree, persistent batch mode with two workers, reached ready. Play-mode frames advanced 1258→1288. No other Editor was driven.
- Chrome captures, final export identity and interaction results: pending below.
