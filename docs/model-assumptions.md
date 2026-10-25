# Package B: deterministic direct-runoff model v1

Status: implemented and independently verified as a pure C# package. Not integrated into Core, previews, the HUD, objectives, or the public build. This document is package B's model contract/evidence, not Phase 4 acceptance. It applies only to Meadowbrook Valley and its existing technical fixtures.

## Formula and evidence

`RunoffEvaluator.ModelVersion` is `terra-direct-runoff-v1`. Rainfall `P` and storage `S` are millimetres; `S = 25400 / CN - 254`; `Q = 0` at `P <= 0.2 S`, otherwise `Q = (P - 0.2 S)^2 / (P + 0.8 S)`. Multiply depth by area in m² and divide by 1000 for m³. The implementation uses an algebraically equivalent expression that avoids squaring large values.

USDA SCS [TR-55, second edition, June 1986](https://infosys.ars.usda.gov/svn/code/third_party/climate/dat2par/tr55t.pdf), chapter 2, equations 2-1–2-4 (printed p. 2-1), supplies the formula in inches. Tables 2-2a–c provide these group-B, average-antecedent-condition examples: straight-row crops in good condition CN78; good grass/open space CN61; woods in good condition CN55. Meadow's separate category is CN58, so the game's visually named “Restore meadow” deliberately retains the frozen grass/open-space CN61 assumption. These are chosen educational assumptions, not observations of soil, management, or hydrologic condition at the selected place.

The source was downloaded from the USDA host and its formula/table text inspected on October 24, 2026. [Research evidence](evidence/package-b/source-review.json) records its SHA256 and page references. The USDA [AnnAGNPS reference note](https://www.ars.usda.gov/ARSUserFiles/60600505/AGNPS/Dataprep/TR55_AnnAGNPS.PDF) independently identifies TR-55 chapter 2 as average-condition curve-number data.

## Frozen parameter preparation

The integration owner derives scalar site CN before constructing `SimulationInput`. `CurveNumberMixture.AreaWeighted(cover, curveNumbers)` validates fractions, uniqueness and mappings, then computes area-weighted CN. It accepts an explicit mapping and has no hidden mapping defaults. Validated fraction sums within 0.0001 of one are normalized to remove float rounding; invalid sums are rejected.

| Existing assumption | CN |
|---|---:|
| cultivated / `cn-b-crop-78` | 78 |
| grass / `cn-b-grass-61` | 61 |
| woodland | 55 |
| 75% cultivated + 25% grass / `cn-b-margin-73-75` | 73.75 |

The frozen v1 rule is **mix CN first, evaluate runoff second**. At 50 mm rain and 1,000 m², the margin yields 8.32860257558 m³. Computing the two cover components separately and adding their volumes yields 9.31981694633 m³. The nonlinear formula makes these different. Neither the evaluator nor the helper silently switches aggregation conventions. The same rule must be used for baseline, preview, commit, restored history, and any unchanged context. Changing this rule is a versioned integration/science decision that requires retuning objectives.

**New mapping request to the contract owner:** actual `worlds/meadowbrook-v1/world.json` contains `sealed` cover in `reserve-12`, but the frozen runtime note does not define its CN. TR-55 table 2-2a lists CN98 for impervious areas. A disclosed illustrative `sealed=98` mapping is a supported proposal, not an accepted contract change in this package. Unknown mappings fail explicitly; do not omit that cover or substitute zero. The technical CN100 test is a numerical limit, not a proposed land-cover parameter.

## Retention and bookkeeping

Each input unit belongs to exactly one explicit `contributingAreaId`. A disconnected device uses a different group, never a blank group or a magic global capacity. For each group, sum generated runoff and capacity, then retain `min(generated, capacity)`. Attribute retained volume to source sites in proportion to their generated runoff. Therefore every source has `generated = retained + discharged`; unused capacity cannot benefit another group. Two devices never consume the same source volume twice.

Zero-area units are allowed for storage-only technical inputs; they generate no water. They still require valid CN and an explicit connection. Actual management footprints remain the geography owner's responsibility. Empty analyses are rejected instead of producing an apparently measured zero.

The CN already accounts for ordinary land-cover abstraction. A swale's changed cover is evaluated first; its capacity represents **additional effective event storage/infiltration beyond that cover's CN behavior**. Do not enter the CN's own storage value as device capacity or subtract the same device twice. These capacities and connection groups are scenario assumptions, not engineered dimensions, measured infiltration, terrain routing, or demonstrated local treatment performance. Retained volume is not decomposed into infiltration versus temporary storage.

## API, validation and determinism

`RunoffEvaluator.Evaluate(SimulationInput)` returns the frozen `SimulationOutput`. It references only Contracts and the standard library. No Unity objects, clock, random state, I/O, network, mutable static caches, or state-store access occur in runtime code. Double intermediates are converted once to float DTO fields. Sources/sites/facts are evaluated in ordinal site-ID order, including group summation, so an input-array permutation does not change serialized output. Caller arrays and objects are never modified or returned as writable aliases.

Invalid schema/revision, missing metadata or arrays, duplicate/blank/padded IDs, nonfinite or negative values, CN outside `(0,100]`, and initial-abstraction ratio other than the frozen `0.2f` cause an `ArgumentException` with a diagnostic. Values that cannot fit a nonnegative finite float output also fail; no partial result is returned. Soil/antecedent strings are provenance metadata: the evaluator cannot verify that scalar CN agrees with them. The integration owner must enforce a consistent parameter version and storm on both designs.

The scalar public `DirectRunoffDepthMm` permits unit verification. CN values below the rural table range are supported mathematically for numeric boundary tests, not endorsed as rural parameters. Float output mass-balance tolerance is `max(0.001 m³, 2e-7 × generated m³)`; independent double scalar values use tighter case-specific tolerances. Do not display spurious decimal precision or calculate percentages when baseline discharge is zero; use an absolute proposal-minus-baseline delta.

## Explanation facts

UI owns phrasing and formatting. All current facts have empty `treatmentId`: the input contains no treatment identity, so the evaluator must not fabricate one. Site-result production fields remain zero and are **not** production accounting; join hydrology fields by site ID into Core's existing results, preserving its production numbers.

| Code | Meaning | Source |
|---|---|---|
| `runoff-generated` | Generated m³ at source site | `usda-tr55-1986` |
| `runoff-retained` | This source site's share of group retention, m³ | input storm source |
| `runoff-discharged` | Source generation minus source retention, m³ | input storm source |
| `connected-runoff` | Total generated m³ in this site's group; repeated context, **not additive** across sites | input storm source |
| `retention-capacity` | Offered additional capacity at device site, m³ | input storm source |
| `retention-capacity-used` | Device capacity's proportional share of capped group retention, m³ | input storm source |
| `retention-capacity-unused` | Capacity minus device-attributed use, m³ | input storm source |
| `storm-rainfall` | One aggregate rainfall depth, mm | input storm source |

Source-site retention and device-capacity use are different attribution views; never add them together. For example, a zero-area device may use 3 m³ of capacity while its source-site retention is zero. Add `usda-tr55-1986` to the integrated source registry, resolving it to the USDA URL above. The existing scalar fixture's illustrative `group-capacity-capped` fact includes `water-swale`, which is absent from its input. Numeric fixture fields are reproduced; that unsupported attribution is intentionally not invented. Package B's tests verify the actual fact semantics separately.

## Remaining integration seams

1. **Core adapter:** construct inputs from current canonical covers, validated mappings, current offers and authored groups. Evaluate baseline and proposal with identical rainfall and model assumptions. Handle model diagnostics without mutating published state. Merge by site ID; sorted output must not be joined by input index. Make preview and commit use this same path.
2. **Analysis coverage:** v1 has one scalar CN and one group per site. The package cannot validate non-overlap, spatial connectivity, or whether unchanged context is included because there is no geometry/analysis-domain metadata. Feeding management sites alone means totals describe management sites only, not the whole AOI. Either label this accurately or provide disjoint unchanged-context units in the adapter. Do not treat cover patches as extra overlapping source areas.
3. **Structured provenance:** the frozen input lacks model-version, treatment-ID, group source-ID, cover-fraction, baseline, and per-capacity provenance fields. The helper validates fractions before the seam; Core owns area-change explanations. Storm source is the only available assumption source for capacity/connectivity facts. If independent device/source provenance or group-level fact IDs are required, request a versioned contract extension rather than smuggling IDs into another field.
4. **Presentation and acceptance:** UI explanations, same-storm comparison, budget/production trade-offs, objectives, actual Unity import/Web compilation and Chrome verification remain primary-owner Phase 4 work. This package does not pass Phase 2A, Phase 3, or Phase 4 gates and does not change any mission threshold.

## Reproduce independent verification

From the repository root with .NET SDK 10 installed:

```sh
dotnet run --project tests/simulation/SimulationChecks.csproj -- .
dotnet build tests/simulation/UnityApiCompatibility.csproj --nologo
python3 scripts/verify-simulation-example.py
```

The first command compiles the actual production source and frozen DTOs, then runs numeric, validation, invariance and conservation suites. The second compiles those same sources against .NET Standard 2.1 / C# 9 to check Unity-compatible APIs without opening or controlling the live Editor. It is not a Unity/IL2CPP build. No external test packages or paid resources are required. Evidence and the integration handoff are in `docs/evidence/package-b/`.
