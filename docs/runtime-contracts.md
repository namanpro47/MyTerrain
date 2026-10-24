# Runtime seams v1

Plain DTOs: `Terra.Contracts` (no Unity references). Accounting/history: `Terra.Core` (no Unity references). Scene/HUD/loading/rendering remain primary-owned. Package B creates its own pure `Terra.Simulation` assembly referencing only Contracts; no scene searches, UI writes, randomness, wall clock, transforms or Unity physics in simulation.

## Commands and state

`EditCommand { operation, siteId, destinationSiteId, treatmentId, expectedRevision }`. Stable IDs, integer revision. Operations frozen for integration are `add`, `replace`, `move`, `remove`. `destinationSiteId` is required only for move (empty otherwise); treatmentId is required for add/replace, ignored for remove, inherited for move. Phase 1 implements **add/replace and undo only**; move/remove return an explicit unavailable reason until Phase 3. No silent fallback.

`WorldStateStore.Preview(command)` returns `Evaluation { valid, reason, result, interventions[] }` without changing published state or history. `Commit` evaluates identically, rejects stale revisions, then publishes one incremented revision and snapshots the old ordered interventions. An intervention is `{siteId,treatmentId}`; one per site. Immutable baseline+scenario plus these proposals fully determine accounting. Returned proposals are not mutable store references. Invalid/protected/ineligible/unaffordable actions leave history and state unchanged. Undo restores the last proposal list and increments revision; empty undo does nothing.

Production is total final capacity, including baseline at untouched sites. Net production subtracts the entire baseline. Replacing recomputes total final costs; undo restores prior costs. Later remove restores site baseline and refunds its proposal, move does both source and destination atomically. Visual animation follows the new state; it cannot change accounting. Browser and keyboard previews use the same evaluator as commit.

## Result contract

`WorldResult`: `revision,spentCredits,remainingCredits,production,baselineProduction,netProduction,runoffAvailable,generatedM3,retainedM3,dischargedM3,coverAreas[],sites[],explanations[]`.

- `coverAreas`: `{coverId,areaM2}` over management sites only; do not imply these are full-AOI cover totals.
- `sites`: `{siteId,production,productionDelta,generatedM3,retainedM3,dischargedM3}`. Deltas relative to each site's baseline. Hydrologic values are meaningful only when `runoffAvailable=true`.
- `explanations`: structured `{code,siteId,treatmentId,value,unit,sourceId}`. Empty site/treatment IDs are allowed for aggregate facts. UI owns phrasing, not the calculation.
- Objective inputs are final production, available discharge, remaining credits and protection/eligibility. The objective evaluator is deferred, not replaced by hardcoded UI success.

Phase 1 sets `runoffAvailable=false`; numeric hydrology DTO defaults are **unavailable**, never a displayed measured zero. UI displays “Pending.” Preview displays exact net production/cost relative to current state. Baseline remains separately accessible for later comparison.

## Simulation package seam and examples

`SimulationInput`: `schemaVersion=1,revision,storm,sites[]`. Each input site has `siteId,contributingAreaId,areaM2,curveNumber,retentionCapacityM3`. Package B takes already-derived cover/model parameters; it does not edit geography or budget. `SimulationOutput`: schemaVersion/revision, generated/retained/discharged m³, per-site results and explanation facts. Retention is grouped by contributingAreaId and capped at group generation; allocate group retention proportionally to generated site runoff for consistent per-site accounting. Unconnected groups receive no benefit. Reject nonfinite, negative, missing or out-of-range input, with a diagnostic to the caller. No partial result on invalid input.

The checked example is `tests/fixtures/simulation/input.json` → `expected.json`: three 1,000 m² areas, two connected east, one west, a 50 mm storm, CN 78/61/78, one 8 m³ capacity. The western site's discharge stays unchanged. These are interface acceptance examples, not a production simulator or real soil measurements. Tolerance: 1e-5 m³ absolute for the reference double calculation, 1e-3 for serialized Unity floats. Phase 4 expands correctness tests (zero rain, CN bounds, monotonicity, mass balance, deterministic repeat, disconnected/capped capacities).

Direct runoff convention: S=25400/CN−254 mm; Q=0 when P≤0.2S, otherwise Q=(P−0.2S)²/(P+0.8S); m³=Q×area/1000. This is an educational direct-runoff estimate, not flood depth, routing, timing, erosion or water quality. Recipe curve numbers are documented scenario assumptions pending package B review.

## Content seam

Player-facing location copy uses **Meadowbrook Valley**. Keep the existing location/source IDs stable, and retain real geographic names in provenance only; see [naming policy](location-selection.md#name-and-provenance).

`ContentEntry {id,title,body,choices[],sourceIds[]}`; choice `{id,text,explanation,correct}`. Stable content/source IDs and plain UTF-8 text. No layout, Unity paths, mission state, hidden science constants or numeric calculations in prose. Empty choices allowed for introductory/information entries. Fixture intro is under `content/education/fixture-intro.json`. Package D authors supported claims and transfer questions for the same location; the primary engineer binds them in later phases.

## Independent verification

With the project open and imported, run `unity command script eval --caller plugin --skill unity-cli 'return Phase1Checks.Run();' --format json`. It needs no loaded main scene or Play mode. It exercises real serialization, pack failures, displaced baseline capacity, preview purity, revision rejection, protected sites, budget rejection, replacement, cover conservation and undo. The standalone simulation fixture verifier is `python3 scripts/verify-simulation-example.py`. The isolated art preview is documented in art-direction.md. None requires a parallel engineer to edit TerraMain.

Model parameter IDs freeze illustrative soil-B coefficients: cultivated 78, grass/open space 61, woodland 55. A living margin uses the explicitly area-weighted composite 0.75×78+0.25×61=73.75. These assumed coefficients follow the categories in [USDA TR-55, chapter 2](https://infosys.ars.usda.gov/svn/code/third_party/climate/dat2par/tr55t.pdf); “Restore meadow” is the visual recipe, using the conservative grass/open-space coefficient rather than claiming the separate meadow table condition. Package B must expose and test the mixture rule; it must not infer measured soil or crop management from these IDs.
