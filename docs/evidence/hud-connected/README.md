# HUD and continuous fields — October 25, 2026

Task: replace the sprawling HUD with a reference-led interface and remove empty corridors between developed fields. Starting remote: `e0c887702db8cee8e6c26c0f2ed78404f1257e92`. Isolated worktree: `terra-hud-continuous`. This bounded revision does not certify Phase 2A or public deployment.

## Reference decisions

Patterns **TN-03/04/05, CS-01/02 and AX-01**. Inspected the [Terra Nil 1.0.1 desktop HUD](https://www.wsgf.org/f/u/contrib/dr/30168/ingame_16x9.png) in Chrome and the [official Cities: Skylines II proposed toolbar, January 29, 2026](https://www.paradoxinteractive.com/games/cities-skylines-ii/news/upcoming-visual-updates). These are visual observations, not playtests of those games. The existing [project reference library](../../DESIGN_REFERENCE.md) remains authoritative.

- Terra Nil: localized dark blue surfaces; strong values; pictorial tools with price badges; an uninterrupted central landscape.
- Cities: Skylines: compact bottom service/status controls, stable groups and distinct indicator colors.
- Application: compact location/budget group; equal-status Economic/Social/Environmental meters; slate surfaces, mint selection, amber/blue/green indicators; miniature project art with upfront prices; three category tabs; a single bottom command strip with emphasized End Year. Exact costs, commitments, reports, settings and keyboard paths remain reachable.
- Reference imagery remains outside game assets. No image generation, purchased resources or copied reference artwork.

## Comparison with the previous implementation

Compared the actual Chrome candidate with the [latest incoming proposed-field capture](../full-game/tint-property-proposed.jpg). That historical image is engineering evidence, not Phase 2A art acceptance. Its tall budget block and scattered left/right commands become a short balance block and a single bottom strip; card prices replace repeated eligible-site captions; the three indicators gain a consistent current/forecast hierarchy. The unchanged default camera and terrain remain visible, while Focus can show two adjoining restoration fields as one meadow. This is a HUD/placement improvement, not a claim that the wider landscape now matches Terra Nil's complete art quality.

## Connected parcels

The old eight southern fields were separate rectangles with 40–45 m context corridors. `scripts/prepare-connected-fields.py` generates a versioned runtime override from the frozen world. Eight southern parcels now share ten edges, without overlap or internal corridors. The stepped outside edge retains mapped water clearance and avoids woodland; isolated east/west/wind opportunities and protected zones remain as before.

The generator clips the original cover rings, checks their areas, checks mapped feature distances and rejects woodland/sealed overlap. `geometry.json` records per-site source areas and clearances. Runtime removes each added cover area from unchanged context before adding the wind opportunity, so the catchment still counts each square metre once. Observed terrain, cover polygons, roads, water, source pack and location identifier are unchanged. Planting uses shared world-coordinate rows without a per-parcel inset. Connected treatment surfaces reach their exact boundary rather than fading back to baseline on every edge.

Buildings keep their metric prefab/lot dimensions and terrain-fit rules; a management parcel is not an enlarged building footprint. Field project prices remain the authored public contribution for one named agreement. The new area and source cover feed the same annual model.

This is definition **3.2.0**, with separate town and chapter save keys. Existing 3.1 saves remain untouched. Arrival explicitly tells a returning player that connected fields begin a new run and the prior-layout save was retained; historic costs, receipts and outcomes are never recomputed or refunded.

## Verification

- Dependency-free geometry compiler/check: eight parcels, ten shared edges, zero overlapping area; source cover totals and road/water buffers pass.
- Connected layout: **608 full-game assertions**, all 17 variants, ten-year histories, bounded meters, budget/water conservation, save replay and manual/fast-forward equality. Two viable strategies retain no arrears: 86.2/99.0/52.1 and 94.2/91.0/61.5. These are authored scenario outcomes.
- Legacy layout: **608 assertions** unchanged; annual core **94** plus gameplay **15** pass. Runs used the installed .NET 10 runtime with `DOTNET_ROLL_FORWARD=Major` for the net8 projects.
- Unity initial GUI import exited unexpectedly. One controlled restart in the same isolated worktree, persistent batch mode with two workers, reached ready. Play-mode frames advanced 1258→1288. No other Editor was driven.
- The actual Editor HUD passes **631 checks**: all 17 project variants, repeated layout changes at 1440×900 / 1280×720 / 1024×576, standard/larger text, an existing preview after resize, long site names and three budget balances. `hud-reflow-check.cs` is the replayable fixture; `hud-reflow-check.json` contains the result. This caught repeated-layout identity/price errors and larger-text overflow before delivery.
- Intermediate Chrome playtest: queue adjacent West/Central restoration, undo (one proposal), redo (two), review $154,000 payment / $36,000 future obligations, offline acceptance, three chapter pages, Year 2 planning and saved reload with $10,846,000 cash and two assets. Static localhost has no AI backend; its labelled offline fallback was used.
- Final-export captures and hashes are recorded below.

## Incoming atlas reconciliation

Fetched and merged `45a87481f01bf5360de169ee502e92697b5b68e4`, preserving the separately authorized three-location atlas. Only Wisconsin receives connected geometry and runtime 3.2 town/chapter keys. Point Reyes and Suusamyr retain their 3.1 packs, definition versions and save keys. The original Wisconsin 3.1 keys remain intact. Atlas navigation sits above New run, outside the compact catalog. Regional identity labels fit the shared HUD. The combined Editor checks pass **678** location/ten-year/replay/isolation assertions and **682** HUD assertions; package verification passes **41** checks.

## Final export and Chrome result

Runtime source: **`dce921982fb8b2e069500fd33399407ab53fbf20`**. Later merges/commits add evidence and documentation only. The final Unity 6000.6.6f1 WebGL RuntimeSpeedLTO export succeeds in **378.243 seconds**. All **30 served files / 45,006,470 bytes** match the local export byte for byte; [build identity](build.json), [job receipt](build-job.json), [served-file hashes](web-assets.json).

The final player restores Wisconsin at **Year 2 / Planning / revision 6 / $10,846,000 cash / two accepted projects**. Chrome verifies 1440×900, 1280×720 and 1024×576 with larger text, resizing while a preview remains open, then returning to standard text. All four recorded audits have zero text-height overflow. Costs and action buttons stay separate; the compact catalog clears the preview; the atlas return control clears both. No error-level console entries were recorded in the final run. [Final browser evidence](chrome-final.json).

- [Continuous accepted fields + redesigned HUD](chrome-connected-fields.png)
- [Larger text, 1440×900](chrome-larger-1440.png)
- [Larger text, 1280×720](chrome-larger-1280.png)
- [Larger text, 1024×576](chrome-larger-1024.png)

The preceding integrated export `e01ae06` additionally verified Wisconsin → Point Reyes → Wisconsin: Point Reyes has its own fresh $10M run; Wisconsin returns with unchanged year, money and accepted projects. Its zero-error logs and larger-text audits are in [integration evidence](chrome-integration.json). The only subsequent runtime change shortens the arrival copy, including the retained-save notice; the actual Editor regression checks both text sizes, bringing the HUD total to **682 passing checks**.

These are actual Chrome screenshots, cropped to the game viewport without compositing or scene retouching. The review viewport selector resizes the real Unity player; 1024×576 is a compact CSS viewport, not a claim of a separately tested browser zoom setting. No new performance benchmark, public deployment, additional location authoring or broader Phase 2A acceptance is claimed. The local review remains available at `http://127.0.0.1:49725/Review/` while its server runs.
