# Terra — parallel engineer handoffs

**Status:** Phase 1 handoff verified. No package has been delegated or started. Phase 2 and package implementation require separate authorization.

Read the [north star](../TERRA_PROJECT_SPEC.md) and [implementation plan](../IMPLEMENTATION_PLAN.md). The primary engineer owns the running game. These packages let another engineer work in a separate checkout without touching the main scene, UI, or integration state. **Every package improves the same selected rural location across its full game scope.** Do not create additional levels, city assets, or a live location generator as parallel work.

## Start condition and recommended assignment

Phases 0–1 are local to Naman's computer. All packages use **Meadowbrook Valley** for player-facing copy and planning. This is a display-name change only; preserve the existing AOI, source attribution and internal ID. See [naming policy](location-selection.md#name-and-provenance).

Frozen Phase 1 baseline:

| Handoff item | Value |
|---|---|
| Shared baseline commit SHA | `a78f49cf401a6ff602676b35f23c84fb5866ae93` (frozen Phase 1 runtime); naming-only runtime update `25067bcf36b575d3fafc12100d179f1a8bc19404`; bundle branch includes both and subsequent evidence |
| Repository / remote access | No Git remote is configured. Portable bundle: `Builds/Handoff/terra-phase1.bundle`; no credentials or public source push required. |
| Unity version | 6000.6.6f1; recheck against shared commit |
| Selected rural AOI, bounds, and source-review note | **Meadowbrook Valley** (game name); `coon-valley-brye-v1`; WGS84 [-91.043,43.697,-91.031,43.706], EPSG:26915; [selection and evidence](location-selection.md) |
| Contract/schema version | WorldPack/runtime/art v1; DTO schemaVersion 1; uGUI + TextMeshPro |
| Fixture path/version | `worlds/fixture-v1`, 1.0.0; exact bytes in `checksums.sha256` |
| Browser preview URL | [Public Phase 1 fixture](https://terra-browser.vercel.app/); [build/browser evidence](PHASE_STATUS.md) |
| Contract owner / integration owner | Primary engineer |

**Best first assignment: package A, the geography generator.** It is a distinct Python/data task and has the earliest dependency. Deliver a small usable pack for the selected rural AOI first, then improve its layers. **Best second assignment: package B, simulation**, which can be developed against tiny pure-data fixtures while the primary engineer builds the world and interactions.

If the other engineer uses several agents, divide A, B, C, and D by the ownership table below. If they work alone, do A → B, then use spare time for C/D. Do not assume all four packages will finish simultaneously. The primary engineer should absorb a delayed package's minimum required work if necessary, after explicitly transferring ownership.

## Ownership and integration boundaries

| Package | Owns | Must not edit | Earliest start | Needed by |
|---|---|---|---|---|
| A: Geography | `worldgen/`, `worlds/`, geography tests, `docs/geography.md`, generated attribution/provenance | Runtime loader/renderers, main scene, shared contracts | Phase 1 exit | Phase 2; refined throughout 3–8 |
| B: Simulation | `Assets/Terra/Scripts/Simulation/`, dedicated simulation tests, `docs/model-assumptions.md` | State store, placement, objectives, UI, site geometry, shared contracts | Phase 1 exit | Phase 4 |
| C: Art/audio kit | `Assets/Terra/Art/ExternalKit/`, `Assets/Terra/Audio/ExternalKit/`, isolated preview scene, `docs/asset-kit.md` | Main scene, global URP settings, runtime/HUD, packages | Phase 1 exit | Early kit in 2; final kit in 7 |
| D: Learning/content | `content/education/`, `docs/learning-content.md` | HUD layouts, objective logic, numerical science parameters | Phase 1 exit | Phases 5–6 |

All paths above are planned ownership boundaries. The main engineer owns Contracts, Core, Geography runtime, Gameplay, UI, main Scenes, build scripts, `ProjectSettings/`, `Packages/`, shared documentation, and release assembly. Within `worlds/`, A owns geography layers and may deliver an initial scenario fixture; after its first integration, the primary engineer owns `scenario.json` and numerical mission balance. A's regeneration command must preserve that authored scenario or emit its seed separately. Packages submit requested contract changes separately before depending on them.

Independence means each package can compile/validate/show its result against the frozen fixture without a live game checkout changing underneath it. Integration is deliberately a small adapter step; it is not zero work.

## A — Real-geography WorldPack generator

**Deliverable:** a reproducible command that produces an aligned pack for the single chosen rural location and its provenance, plus a small ready-to-use pack the primary engineer can consume immediately.

**Inputs:** phase-1 schema, local-axis convention, agreed AOI and context bounds, footprint/mesh limits, and source-use requirements. Use the precise rural AOI and selection note already recorded in phase 1. Validate its source coverage, but do not independently select another place or broaden the map. Escalate a fundamental data gap to the primary engineer before replacing the chosen AOI.

**Implementation sequence:**

1. Establish a pinned Python environment and one CLI entry point. Separate downloaded raw data, cached intermediate data, and distributable outputs. Keep large raw datasets out of Git.
2. Verify actual source availability, export formats, dates, usage terms, and coverage. Record URLs and processing steps. Source suggestions in the spec are research starting points, not guaranteed APIs.
3. Produce a thin real pack: terrain, cover/vegetation context, mapped water/path features where present, origin/bounds, and provenance. Sparse buildings are optional context, not a required ingestion/rendering system. Ship this before enriching every vegetation or land-cover field.
4. Project layers into an appropriate metric CRS, clip, simplify, and export local coordinates according to the contract. Preserve geographic layer alignment and ring topology; provide explicit behavior for holes/multipolygons.
5. Add vegetation and editable sites. Sites are authored hypothetical rural management opportunities, must respect mapped sensitive features and scenario no-edit zones, and must reference stable features/analysis units. Do not infer ownership, legal conversion rights, agricultural suitability, or measured crop yields. Distinguish fictional protection rules from actual legal designations.
6. Partition analysis areas into non-overlapping units, including unchanged context. Record areas, baseline cover fractions, data/assumption status, and model-parameter references. Coordinate with B through the contract owner on parameter IDs, not by changing B's evaluator.
7. Validate export determinism, required fields, finite coordinates, bounds, terrain dimensions, valid polygons, cover fractions, source IDs, and required provenance. Record missing-layer behavior explicitly.

**Independent acceptance:**

- One documented CLI command rebuilds the same normalized pack from the same cached inputs; volatile timestamps are excluded from determinism checks.
- A lightweight map/plot shows terrain, cover, water/path context, and optional structure alignment with annotated known reference points.
- Coordinate round-trip/control-point checks satisfy a documented tolerance appropriate to source resolution. Do not claim centimeter accuracy from coarse data.
- A tiny golden fixture passes schema/coordinate tests; malformed bounds, missing required layers, and unsupported versions fail clearly.
- A source manifest includes observed dates, licenses/attribution, cover/vegetation/soil/optional structure assumptions, and data gaps.
- The primary engineer can open the delivered pack through the phase-1 importer without path or schema edits.

**Merge slices:** A1 thin rural pack and validator → A2 curated sites/analysis units/provenance → A3 enriched vegetation and geometry cleanup. The primary engineer integrates A1 during phase 2 rather than waiting for A3.

**Allowed shortcuts:** manually download a dataset once; cache locally; simplify geometry offline; manually curate sites; use disclosed cover/vegetation assumptions. **Excluded:** production geospatial API, custom GIS editor, second playable pack, city data pipeline, live generation UI.

**Copyable assignment:**

> Implement package A in docs/PARALLEL_WORK.md against the recorded phase-1 baseline and v1 contracts. Own only the listed geography paths. Deliver the smallest geographically correct pack for the rural AOI already selected in phase 1 first, then finish curated sites, analysis units, and provenance. Include reproducible commands, validation fixtures, an alignment preview, and all data/assumption attribution. Keep the Unity game and shared contracts unchanged; propose required interface changes separately. Prepare small mergeable deliveries A1/A2/A3. Keep improving that same pack; do not author a second location or generalize the pipeline across regions.

## B — Deterministic environmental model

**Deliverable:** a pure C# evaluator that accepts frozen input DTOs and returns deterministic result DTOs and explanation facts, with numerical fixtures and a concise assumptions note.

**Inputs:** disjoint analysis units, current cover fractions/parameter mappings, area in m², one rainfall depth in mm, connected green-infrastructure capacity in m³, and a fixed model version. The Core layer owns illustrative productive-capacity/budget transactions; B owns environmental math. The main game combines their outputs into one view model.

**Implementation sequence:**

1. Verify an authoritative reference for the chosen Curve Number-style educational method and the actual parameter assumptions. Document the formula, units, valid ranges, soil group, storm, and applicability limits. Do not treat the same cover label as a universal CN.
2. Implement `Evaluate(inputs) -> result` without `UnityEngine`, scene objects, networking, random state, clock reads, or mutable static caches. Keep the input/output types in the already frozen Contracts assembly.
3. Calculate direct runoff per analysis subarea/cover component as agreed. Convert depth to volume with `volumeM3 = depthMm / 1000 * areaM2`; aggregate volumes. Keep baseline/proposal under identical rainfall and parameter versions.
4. Allocate retention only within explicit contributing-area groups. For v1, sum effective treatment capacity in each disjoint group and cap retention by that group's generated runoff. If per-device attribution is needed, allocate in stable ID order and report the remaining volume. No source volume is consumed twice.
5. Define how a swale/retention treatment's land-cover change and storage/infiltration capacity interact so one physical benefit is not subtracted twice. Track generated, retained, and discharged volumes separately.
6. Return numeric explanation facts such as changed cultivated/vegetated cover area, connected runoff, utilized capacity, and unmet capacity. Main UI owns player-facing sentences and formatting.

**Independent acceptance:**

- Compare to independently hand-calculated cases, not tests that duplicate the implementation algorithm.
- Include zero rain, the initial-abstraction boundary, CN range validation, zero-area/invalid input, unit conversion, cover-fraction validation, and nonnegative outputs. Include rural cover/treatment fixtures from the agreed assumptions; numerical unit fixtures such as CN 100 are not representations of the actual rural site.
- Simple fixture: at CN 100, 10 mm rain over 1,000 m² yields 10 m³ generated runoff; a connected 3 m³ capacity retains at most 3 m³ and leaves 7 m³. A disconnected device leaves those 10 m³ unchanged.
- Test two devices drawing from one group, a capacity larger than all available runoff, and a cover change combined with retention. Assert `generated = retained + discharged` within a stated tolerance.
- Same inputs give same results; input objects remain unchanged. Compare results from original, edited, and restored fixture states.
- Include a zero-baseline result and absolute deltas so the UI/objective layer can avoid undefined percentage math.

**Merge slices:** B1 validated direct-runoff evaluator + assumptions → B2 connected retention → B3 per-unit explanation facts and integration fixtures. Each should be independently testable.

**Allowed shortcuts:** simple loops, full recomputation, assumed disclosed soil, authored connection groups. **Excluded:** visual flood simulation, asynchronous model service, full drainage networks, alternative erosion/drought systems.

**Copyable assignment:**

> Implement package B in docs/PARALLEL_WORK.md against the recorded phase-1 Contracts types and fixtures. Own Simulation and its dedicated tests/model-assumptions note only. Deliver a pure deterministic educational runoff evaluator, then bounded connected retention and structured explanation facts. Verify formula/parameter references, units, conservation, no double counting, and disconnected cases. Do not modify UI, placement/state management, objective evaluation, geometry, or shared contracts.

## C — A cohesive miniature-world art and audio kit

**Deliverable:** a small visually consistent modular kit, a contact sheet/preview capture, and an isolated Unity preview scene. The primary engineer applies it to the world.

**Inputs:** frozen palette/type/style sheet; local units; footprint and pivot rules; target lighting; material/shader compatibility; representative terrain/cover/sites from the fixture.

**Scope:** cohesive terrain/ground-cover materials, cultivated-plot patterns and a vegetated-margin variant, a small locally appropriate grass/shrub/tree set, restoration transitions, one swale/retention module, placement accents, and restrained ambient/interaction audio. Sparse structures are optional context; do not make a city building kit. Prioritize the one landscape's opening camera and common placements.

**Independent acceptance:**

- Ground-level pivots, meter scale, documented footprint bounds and front direction. Assets do not relocate mapped landscape features. Ground-cover/cultivation patterns clip to the canonical site geometry rather than changing its area; optional structures retain their mapped placement.
- Shared materials, restrained texture sizes, simple URP-compatible shaders, and documented mesh/texture counts. The primary engineer validates final draw/triangle cost in context.
- Prefabs import with their `.meta` files and no missing references. No global project setting or package changes.
- Show default, preview/ghost, selected, and reduced-motion-friendly placement appearance on the fixture. Assets remain readable at normal play zoom.
- Include origin/license/credit information for every third-party visual/audio asset. No unlicensed soundtrack dependencies.

**Merge slices:** C1 terrain palette/materials/vegetation + one cultivation treatment → C2 full intervention kit → C3 audio and finishing details. Do not wait to finish a large asset library.

**Copyable assignment:**

> Implement package C in docs/PARALLEL_WORK.md using the phase-1 art direction and fixture. Work only in the ExternalKit art/audio paths and your isolated preview scene. Deliver a small cohesive rural landscape kit for the selected location with correct scales/pivots, shared materials, credits, and preview captures. Prioritize a few excellent visible assets. Do not edit the main scene, HUD, runtime code, packages, or global render settings.

## D — Learning, onboarding, and ending content

**Deliverable:** structured content files and a short storyboard that can be bound to existing UI by stable IDs.

**Scope:** concise hypothetical rural productive-land brief, progressive tutorial hints, tool intents, invalid-action reason copy, model/fiction disclosures, one entry prediction, one unseen transfer question with feedback, and a few Take It Home question-card choices. Use result placeholders rather than hardcoded calculated values.

**Independent acceptance:**

- Plain, age-appropriate language for 15–24-year-olds; short enough to understand while looking at the world. Avoid a sequence of long modal lectures.
- The transfer question uses a new situation and tests reasoning about competing needs, cover, or connected mitigation; it does not merely repeat the winning move.
- Answer feedback explains the trade-off and does not imply one universal land-management practice. Productive units are hypothetical game accounting; do not equate them to real harvests, people fed, or income. No unsupported flood predictions or claims of measured educational effectiveness.
- Facts, scenario assumptions, and modeled estimates are distinguished. External civic links are included only if verified; useful generic planning questions do not require a live link.
- Content validates against the agreed schema, resolves required placeholders, and provides fallback text for unavailable measurements. Main engineer reviews final wrapping/layout.

**Copyable assignment:**

> Implement package D in docs/PARALLEL_WORK.md. Deliver concise structured onboarding, tool/reason copy, a transfer question with explanations, and Take It Home content for the single rural game. Follow the frozen content schema and use result placeholders. Keep fiction/model disclosures accurate and factual claims sourced. Do not modify the UI or numeric game balance. Include a short sequence storyboard and all stable content IDs.

## No expansion package in the current goal

The former neighborhood-service package E is removed from the active handoffs. No engineer or agent should build a second location, city pipeline, level selector, geocoder, generator service, or AI context system as speculative parallel work. The first rural location must reach the full-game quality gate before a separate explicit expansion decision.

Extra capacity should improve package A's existing landscape/inputs, B's model accuracy/explanations, C's same-location visual/audio quality, or D's onboarding/learning. Keep a single set of authored sites and mission balance while those systems evolve.

## Merge protocol

1. Branch from the recorded phase-1 baseline, using a descriptive branch such as `codex/worldgen-rural` or `codex/runoff-model`. Use separate working copies; never share a live Unity `Library/` directory.
2. Keep PRs small and within owned paths. Send a thin usable artifact early. Include `.meta` files for Unity assets, preserve existing GUIDs, and exclude caches/build products/raw source downloads.
3. Each delivery includes its contract version, exact verification command, expected output, a fixture/artifact, assumptions/limitations, and any new dependency. No unrelated package upgrades or project-setting changes.
4. The primary engineer reviews the contract delta first, then validates the package, then integrates the narrow adapter in the game. Do not resolve schema disagreements with silent producer/consumer hacks.
5. After each integration, repeat the tiny fixture check and the affected browser path. For A/C inspect geography/rendering; for B inspect preview/commit/conservation; for D inspect copy/layout and ending flow.
6. Record merged commit and evidence in `PHASE_STATUS.md`. Keep the previous working build available until the integrated browser slice passes.

Shared scenes, shared contracts, input action assets, package manifests, and global rendering settings always have one integration owner. Independent assets/data and plain simulation code are the preferred merge surfaces.


## Runnable local handoff

No Git remote is configured in this repository. The portable bundle supplied with the completed checkpoint is a full Git checkout source, not the multi-GB Unity Library/build cache. Transfer the bundle to the other engineer through the team's chosen channel; this task does not message another person or create a remote repository.

```sh
git clone -b codex/unity-setup /path/to/terra-phase1.bundle Terra
cd Terra
# The branch includes the frozen runtime, Meadowbrook Valley copy update and evidence/docs.
# Use a78f49cf401a6ff602676b35f23c84fb5866ae93 for the exact exported runtime.
python3 scripts/prepare-world.py
python3 scripts/verify-simulation-example.py
```

Open this folder in **Unity 6000.6.6f1** with Web Build Support. Packages are locked; TMP Essentials, Kenney assets/licenses and DOTween are included. Open `Assets/Terra/Scenes/TerraMain.unity` and enter Play for the fixture, or `Assets/Terra/Toolkit/ToolkitPreview.unity` for the isolated art gallery. The build entry packages the fixture automatically. Browser/build commands remain in [deployment.md](deployment.md).

With the Editor connected, `unity command script eval --caller plugin --skill unity-cli 'return Phase1Checks.Run();' --format json` verifies contracts without opening or modifying TerraMain. Package A validates against manifest/world/scenario DTOs and the invalid examples; package B runs the scalar I/O example independently and implements the pure seam; C opens the gallery independently. Shared-contract changes go through the primary engineer. Only one package owner edits a path at a time.

The next allowed implementation after this handoff is Phase 2/package A **only when separately authorized**. This Phase 1 goal does not start it.

The source baseline was cloned from a complete Git bundle and passed packaging/checksum and simulation-example checks with a clean Git status: [handoff check](evidence/phase1/handoff-check.json). Final bundle regeneration includes the committed audit and evidence. The generated bundle is local and ignored by Git; no remote push or delivery to another person is claimed.
