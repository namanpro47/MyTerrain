# Meadowbrook Valley learning content — package D v1

Package D is a ready-to-bind **content delivery**, not an integrated mission or ending. It adds 43 `ContentEntry` files for the one selected AOI, keeps the fixture intro unchanged, and supplies a data-only binding index and executable checks. No scene, UI, mission logic, scientific coefficients, scenario balance, shared contract, or build packaging is changed.

The baseline is `08092a1f19ceb064ab21c11c6f97dd221693d410`. Geography stays `meadowbrook-v1` / `coon-valley-brye-v1`, WGS84 `[-91.043,43.697,-91.031,43.706]`. Meadowbrook Valley is the fictional display name. The community-garden transfer prompt is a hypothetical text question, **not another authored location or level**.

## Files and contracts

- `content/education/meadowbrook-v1/*.json`: one object per file with exactly the existing C# `ContentEntry {id,title,body,choices[],sourceIds[]}` fields. Each choice has exactly `id,text,explanation,correct`. UTF-8, plain text, no UI markup.
- `content/education/fixture-intro.json`: unchanged, including its development-fixture/pending language.
- `content/education/bindings.json`: package metadata and the **explicit file list**. It is not a ContentEntry and must not be passed to `JsonUtility.FromJson<ContentEntry>`. This index does not change a shared DTO or automatically wire the game.
- `tests/education/validate_content.py`: standard-library validator plus a reference formatting/availability oracle for the future adapter.
- `tests/education/fixtures/`: deliberately malformed content, excluded from runtime packaging.

The current scenario already names `meadowbrook-intro` and `runoff-transfer-v1`; these files now resolve those references. `scenario-assumptions-v1` is the source for authored rules, fictional questions and model descriptions. `disclosure-geography` references the four existing USGS source IDs in the pack manifest. Content repeats the scope of those supplied records; it adds no external civic link, new survey claim, science coefficient or recommendation for a real farm.

## Sequence storyboard and design reference

**TN-06** keeps real geography, hypothetical choices and modeled estimates in separate short field-guide entries. Full source dates/URLs remain in the existing manifest/source presentation; these concise entries must not replace provenance detail. **CS-04** supplies one contextual instruction at a time. **TN-08** gives the valley and the player's design attention before the ending question. Terra Nil remains the primary visual/feel reference and Cities: Skylines the secondary information-design reference. The previous visuals are rejected; content makes no claim to match accepted browser captures. The visual integration owner must review it against the new accepted Phase 2A contract and Chrome captures.

| Moment | Content and trigger | Integration behavior |
|---|---|---|
| Arrival | `meadowbrook-intro`; then `meadowbrook-goals` only with a configured mission | Show the world immediately. Brief and help can be dismissed/reopened. Do not treat fixture targets as a shipped mission. |
| Optional initial prediction | `entry-prediction-v1` before placement | Allow skip. Record first response separately from a later retry if evaluating understanding. Do not make it a gate to editing. |
| First inspection | `hint-inspect` | Present once until the player inspects a site; skip it if they already have. |
| Tool choice | `hint-preview` and matching `tool-*` | Tool intent stays contextual; do not stack hints across the landscape. |
| Valid preview | `hint-place` | Show only when a valid preview can actually be committed. |
| First placement | `hint-compare` | Invite another site's preview, not a prescribed solution. |
| Revision needed or help reopened | `hint-revise` | Explain undo only when history is available; never require a particular treatment sequence. |
| Invalid action | `reason-*` | Cause first, one recovery action. Existing state remains unchanged. |
| Sources/help | `disclosure-*` | Distinguish map evidence, game assumptions, available estimates and management-site-only cover totals. |
| Goals met | `ending-invitation` | Only from the actual objective evaluator. Let the player choose review or keep building; no repeated celebration on every edit. |
| Review | `ending-accounting`, `ending-water`, `ending-reflect` | Same camera; baseline/proposal toggle must preserve proposal state. Numeric copy reads actual results. |
| Apply the idea | `runoff-transfer-v1` | One unseen text situation with feedback for every choice. Do not call it proof of learning or block Continue Building. |
| Takeaway | `take-home-choice` → matching `take-home-*` card | These are preferences, not a quiz. All three choices have `correct:false`; do not score this entry. Copy the selected card's text. |
| Return/replay | `ending-continue`, `ending-return`, `ending-retry` | Continue/return preserve the existing proposal. Retry text must appear before the destructive reset, with a back/cancel route. |

The five hints teach gestures, not a winning script. The 43 entries are alternatives, optional information, states and fallbacks; they are **not** 43 consecutive screens. Character budgets are 48 title, 300 body, 130 choice, 220 feedback, and 380 rendered body characters. These are copy budgets, not evidence of browser fit; wrapping, font size, truncation, viewport and input behavior remain integration checks.

## Thin integration adapter

The primary engineer owns this adapter in Phases 5–6. No dependency on the old HUD layout is required.

1. Package only index-listed content files as build-served data. Load through the project's Web-safe URL path, deserialize each entry with the existing DTO, index by `id`, and reject duplicates/missing scenario IDs. Keep index metadata separate from ContentEntry serialization.
2. Resolve the current scenario's `introId` and `learningId`. Do not rewrite scenario values to make this package load. Bind treatment IDs to `tool-<treatmentId>`; when runoff is unavailable the swale intent uses its unavailable variant.
3. Use `{name}` as the package's sole supported placeholder syntax. Runtime substitution is **not already implemented**. The index maps every token to an existing numeric DTO field. The test renderer is a reference for the adapter, not runtime code or a model evaluator.
4. Read numbers from one consistent state revision. Never substitute a missing/nonfinite value with zero. Check guards **before** formatting; if any token is unavailable use the entire fallback entry. Do not show half a comparison with a fictional other half.
5. Format illustrative units with one decimal, signed unit change with an explicit sign except zero, credits as integers, volumes with one decimal and the label “estimated.” Locale-aware separators may replace the oracle's separators. Display rounding conveys no measurement precision. Numeric limits/wrapping remain the integration owner's responsibility.
6. Route exact frozen reason strings using `reasonBindings`. For protected sites, the binding currently matches the canonical scenario reason only. Do not replace a future specific legal/source reason with generic scenario protection. Unknown reasons use `reason-unknown`; preserve the raw diagnostic for debugging. Phase 3 may add reasons: update this mapping/test intentionally when integrating that revision; do not silently classify new failures by substring.
7. Score only `questionIds`. Each has one best reasoning answer and individual explanations. Ignore `correct` for `preferenceIds`; use `choiceTargets` for the takeaway card. An incorrect answer receives explanation, not a punishment or loss of design.
8. Hide capability-guarded text/actions when unsupported unless a fallback is supplied. A data file cannot grant an unimplemented operation or declare success. The formatter returns `None` for such hidden entries, and a complete fallback body for unavailable measurements.

### Placeholder and guard mapping

| Tokens | DTO input | Required guard |
|---|---|---|
| `productionTarget`, `budgetCredits` | `Scenario.objectives.productionMin`, `Scenario.budget` | `missionConfigured`: mission integration explicitly enables these authored goals. Mere numeric defaults do not qualify. |
| `production`, `netProduction` | Current `WorldResult.production`, `.netProduction` | `currentResultAvailable`: valid coherent current accounting. Net change includes displaced baseline capacity; do not recompute in prose. |
| `spentCredits`, `remainingCredits` | Current `WorldResult.spentCredits`, `.remainingCredits` | Same current accounting revision. |
| `baselineDischargedM3`, `dischargedM3` | Baseline and current instances of `WorldResult.dischargedM3` | `matchedRunoffAvailable`: **both** results have `runoffAvailable=true`, same storm, same model/parameter version and analysis domain, with current result matching displayed state. |

Guard names are adapter predicates, not newly invented DTO fields. DTOs alone cannot establish all comparison provenance; the simulation integration owns the matching check. Missing match metadata means use `ending-water-unavailable`. A valid available zero baseline is displayed as an absolute volume with no division or percentage improvement. A pending default zero is never displayed as a result.

Other guards: `runoffAvailable` selects modeled swale/disclosure copy; `objectivesMet` comes from the real mission evaluator; `continuePreservesProposal` and `retryAvailable` require working corresponding actions. The current scenario has `runoffEnabled:false`, so this package does **not** activate a water goal or claim full-game completion. Use pending disclosure until the simulation/main-game integration supplies real estimates.

All fallbacks are self-contained and have no placeholders. Whole-entry fallbacks cover target availability, accounting, water comparisons, swale intent and model disclosure. No generated/retained/discharged quantities or objectives are hardcoded into player-facing content.

## Question review and honest interpretation

The entry prediction asks which inputs are needed to compare a change in field cover. The best answer keeps storm/soil assumptions shared and considers both cover and area; alternatives expose “cover alone” and “money determines runoff” misconceptions.

The transfer question changes the context to a community garden with unknown drainage and competing growing/vegetation/storage needs. It asks what evidence is needed before choosing, not which in-game placement won. Each alternative tests a distinct reasoning failure: production alone, unlimited/global storage benefit, or dismissing productive needs. Feedback explicitly recognizes legitimate needs, finite connected retention within the game, missing local evidence and the absence of a universally best design. This is an authored reasoning rubric, not a validated educational instrument. Automated tests verify structure/coverage; a human still needs to judge comprehension.

Neither answer should imply the actual Wisconsin site has surveyed drainage, measured yields or an official restoration need. If assessing understanding, keep responses local or in anonymous pilot notes, allow skipping, and distinguish first attempts from retries. No analytics, identities, persistence or measured improvement is implemented here.

## Verification and integration gate

From the repository root:

```sh
python3 tests/education/validate_content.py
python3 -m unittest discover -s tests/education -v
```

The validator reads **actual** C# field declarations and checks every ContentEntry/ContentChoice field and JSON type, IDs, source references, current scenario/treatment content, placeholders and DTO paths, fallback coverage, reason mapping, question structure, preference/card mapping and concise length budgets. It does not claim to execute Unity's serializer. Tests exercise malformed fixtures, duplicate JSON keys, stale/missing package references, missing feedback, boolean-vs-string errors, wrong placeholder paths, reason drift, unavailable and real-zero results, negative production change and template rendering. Evidence is in `docs/evidence/package-d/`.

After merge, the primary engineer must test actual Unity deserialization/build packaging and Chrome opening/help/preview/blocked states plus the Phase 5–6 review/learning/continue/retry loop. Check each supported viewport against accepted Phase 2A captures and verify state and comparison values. Novice comprehension and browser layout remain **unverified**. No browser or UI integration is claimed by this content-only delivery.

## Stable ID inventory

| ID | Title |
|---|---|
| `meadowbrook-intro` | Make space. Keep balance. |
| `meadowbrook-goals` | Your planning challenge |
| `meadowbrook-goals-unavailable` | Explore the choices |
| `hint-inspect` | Start with a site |
| `hint-preview` | Look before you place |
| `hint-place` | Try your proposal |
| `hint-compare` | Try another possibility |
| `hint-revise` | Make room for a different plan |
| `tool-cultivate-standard` | Open plot |
| `tool-cultivate-margin` | Living margin |
| `tool-restore-grass` | Restore meadow |
| `tool-water-swale` | Vegetated swale |
| `tool-water-swale-unavailable` | Vegetated swale |
| `reason-stale` | Preview again |
| `reason-unavailable` | Action unavailable |
| `reason-site` | Choose a site |
| `reason-protected` | Keep this zone unchanged |
| `reason-ineligible` | Try another site |
| `reason-occupied` | A proposal is already here |
| `reason-missing` | No proposal to change |
| `reason-unchanged` | Already placed |
| `reason-budget` | Not enough credits |
| `reason-unknown` | Cannot apply this change |
| `disclosure-geography` | Real geography |
| `disclosure-scenario` | Hypothetical choices |
| `disclosure-model` | Modeled estimates |
| `disclosure-unavailable` | Water estimates unavailable |
| `disclosure-cover` | What the cover total includes |
| `entry-prediction-v1` | Before you try a plan |
| `runoff-transfer-v1` | A new planning question |
| `take-home-choice` | Take a question home |
| `take-home-production` | My planning question |
| `take-home-vegetation` | My planning question |
| `take-home-water` | My planning question |
| `ending-invitation` | Take in your design |
| `ending-accounting` | How your plan changed the land |
| `ending-accounting-unavailable` | Compare the landscape |
| `ending-water` | The same storm, two designs |
| `ending-water-unavailable` | Water comparison unavailable |
| `ending-reflect` | What changed—and what did it cost? |
| `ending-continue` | Continue Building |
| `ending-retry` | Try another strategy |
| `ending-return` | Back to your plan |
| `fixture-intro` | Make space. Keep balance. |
