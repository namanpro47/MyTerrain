# Phase 4 — connected environmental consequences

October 24, 2026. The completed Phase 3 delivery (`c49a3c2`, runtime `b91b568`) is the foundation for this phase. The primary ledger had not yet incorporated that delivery when this goal began. The user's request to complete the next phase while Phase 2A continues therefore advances Phase 4. The editing delivery is integrated into the current committed visual baseline in an isolated checkout; the active visual owner's uncommitted work is untouched.

## Model and spatial contract

The game now constructs `LandscapeRunoff` from the canonical WorldPack and `hydrology.json`, then injects it into `WorldStateStore`. Every preview, commit, move, replace, remove, clear, undo and redo recomputes the same deterministic result. Hydrology joins by site ID without replacing productive-capacity fields. The baseline and proposal use a cloned, fixed storm: 50 mm, assumed soil group B, average conditions, initial abstraction 0.2. No weather service, growth time, flood simulation or additional location is added.

The model uses the independently tested package B evaluator (`terra-direct-runoff-v1`). The integration configuration is `terra-landscape-runoff-v1`. **Within each management site**, cover fractions produce the frozen v1 area-weighted CN. **Across sites and unchanged context**, compute each unit's runoff first and sum volumes; never average the entire landscape's CN. This retains the existing scientifically disclosed convention and the 73.75 CN living-margin fixture. It does not silently switch to component-wise runoff, which would produce different results.

Assumed coefficients: cultivated 78, good grass/open space 61, woods 55, and high-runoff cover 98. [USACE's reproduction of USDA TR-55 tables](https://www.hec.usace.army.mil/confluence/hmsdocs/hmstrm/cn-tables), inspected October 24, 2026, supports the coefficients under those stated conditions. `sealed=98` is now explicitly accepted as an **educational high-runoff approximation**. The NLCD display grouping also includes developed and barren classes, so it is not a measured impervious fraction or a claim that every such cell is paved. The visual “Restore meadow” recipe keeps the previously disclosed grass/open-space CN61, not the separate meadow CN58.

## Each area counts once

The already-delivered `analysis-units.geojson` partitions the entire 1,486,800 m² rendered context into 12 management/protected sites and 1,699 unchanged pieces. It preserves holes and multipart geometry. The context includes a visual margin outside the approximately 0.97 km² selected editable AOI; neither domain is a delineated watershed.

`scripts/prepare-hydrology.py` collapses only unchanged pieces sharing one cover coefficient into four area aggregates. Since these have no treatment capacity, their aggregation preserves the sum of volumes exactly apart from float rounding. Sites are excluded before aggregation. The runtime checks total area and rejects missing mappings, duplicate memberships and invalid configurations. The loader verifies the raw world SHA256 so stale hydrology cannot silently attach to a changed world.

The HUD shows total same-storm discharge and its **absolute** baseline delta. The Field guide reports the editable/protected sites' generated contribution separately from the unchanged surroundings. Baseline generation/discharge is about 9,702 m³, of which the test evidence records the exact managed/context split. No percentage objective is introduced or divided by a zero baseline. A zero-rain fixture remains finite zero.

## Hypothetical connections and finite storage

`hydrology.json` owns explicit scenario connections; it supersedes the Phase 2 site-only placeholders for integrated hydrology without modifying observed geography. These are teaching assumptions, **not inferred field drainage**:

| Group | Members | Eligible device |
|---|---|---|
| Southern plots | Lower west field, Lower central field, Meadow edge, Southern field | Meadow edge |
| Eastern plots | East field, Lower east field, Brye-side field | Brye-side field |
| Isolated | West field, Central field, Western terrace, Wooded slope, Stream corridor, each separately | None |
| Unchanged context | All remaining cover, isolated from every device | None |

Each swale retains **at most 8 m³ per event**, limited to its group's generated volume. Its grass-cover effect is calculated first; the 8 m³ is additional assumed effective storage/infiltration beyond that CN behavior. Source retention and device-capacity use are different attribution views and are never added together. There is no global water bonus. Two devices sharing a group in the technical test conserve water and cannot exceed group generation. No change to the existing costs, production or capacity balance was needed: living margins trade more credits and lower production for lower runoff; restoration gives up productive use; swales cost more than restoration for a modest additional connected storage benefit. Phase 5 will balance mission targets against these transparent results.

## Presentation and design references

TN-03/05, CS-02/04 and AX-01 guide the changes. The existing production/budget group gains same-storm runoff, baseline and retained volume. Contextual preview shows absolute runoff change and a short cover-based cause. Swale previews show capacity used/available and highlight only the members of the hypothetical connection group. The outline is group membership, not flow direction, inundation, measured catchment boundaries or water depth.

The original renderer remains in place; `LandscapeRenderer.Runoff.cs` adds only cancelable, scale-aware membership outlines. No new assets or generated images. Field guide copy distinguishes observations, scenario assumptions and estimates, cites the parameter source, and excludes yield, flood and water-quality forecasts. Larger text and motion/sound settings remain available.

These engineering/UI checks do not certify the separately rejected Phase 2A visual direction. Compare against the inherited study baseline for regressions; the latest accepted Phase 2 evidence remains historical and there is no accepted Phase 2A benchmark yet. Phase 2A must assess the combined presentation after integration.

## Reproduce

```sh
python3 scripts/prepare-hydrology.py --check
python3 scripts/prepare-world.py
# Use the existing Shapely environment for the geometry audit:
.venv-worldgen/bin/python scripts/verify-hydrology-geometry.py
dotnet run --project tests/consequences/ConsequencesChecks.csproj -- .
dotnet run --project tests/simulation/SimulationChecks.csproj -- .
```

In this checkout's own Unity Editor, run `Phase1Checks.Run()`, `Phase2Checks.Run()`, `Phase3Checks.Run()` and `Phase4Checks.Run()`. Switch a fresh Editor to Web before calling `TerraBuild.BuildWeb()`. Browser/export evidence and any limitations are recorded in [the Phase 4 evidence](evidence/phase4/README.md).

The existing v1 DTOs, fixture and world geometry remain unchanged. The pure Core assembly now references Simulation. The fourth runtime JSON is explicitly packaged by both prepare-world and the Unity build hook. Geography regeneration preserves the primary-owned hydrology configuration; regenerate it with its own script after an intentional world change. Model/connection changes require corresponding evidence and a version decision. Mission objectives, onboarding/learning integration and production deployment remain separate later work.

## Integration handoff

Branch `codex/phase4-consequences` contains the original Phase 3 transactions (cherry-picked as `2f74c40` and `d17925d`) and the consequences integration. The checkout is `/Users/naman/.codex/worktrees/phase3-editing/Terra`; its directory name predates the discovery that Phase 3 was already complete. The superseded independent editing study is preserved on `codex/phase3-review-study`, not part of this delivery.

Carry the pure Core/Simulation adapter, hydrology configuration and package hooks into the visual branch. Preserve the Phase 3 command/history hooks. Main/HUD/renderer changes are narrow integration points: construct the evaluator, show its exact result and preview, and render hypothetical membership. Adapt those hooks to the accepted visual design instead of replacing the visual worker’s files wholesale. `Terra.Core` must reference `Terra.Simulation`; all four StreamingAssets JSON files must ship together. Re-run the model/Unity checks and Chrome route after merging.

The isolated build preserves the inherited scenery and contains no new assets. The visual reference remains **Terra Nil first** (TN-03/05 for calm information and visible consequences), with Cities: Skylines CS-02/04 for sourced estimates and AX-01 for readable controls. Phase 2A still owns whole-scene quality and the performance gate. Do not interpret this handoff as visual acceptance or begin Phase 5 automatically.
