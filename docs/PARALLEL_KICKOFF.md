# Parallel implementation assignments — October 24, 2026

The user requested that every implementation-ready task in the plan be started in parallel. The Phase 1 contracts and completed Phase 2 geography permit packages B, D, and the independent audio portion of C to start now. These are implementation assignments, not completion claims or authorization to bypass the visual acceptance gate.

## Active owners

| Work | Owner / checkout | Scope and delivery |
|---|---|---|
| Phase 2A visual rebuild | Existing chat, **Plan a UI/UX quality phase**; primary checkout | Landscape, materials, renderer, camera and presentation. Its current captures remain unaccepted. |
| Phase 3 editable world | Existing chat, **Implement remaining Phase 2 worktree** | The user clarified that this worker should implement Phase 3 alongside 2A. It owns reversible editing/Core/UI integration; this assignment does not duplicate that work. |
| B: simulation | `simulation_package_b`; `/Users/naman/.codex/worktrees/terra-simulation-b/Terra` | Pure C# runoff evaluator, connected bounded retention, explanation facts, numerical fixtures and model assumptions. |
| D: learning/content | `learning_package_d`; `/Users/naman/.codex/worktrees/terra-learning-d/Terra` | Structured brief/hints/reasons/disclosures, entry prediction, unseen transfer question, takeaway/ending copy, validation and integration storyboard. |
| C3: audio | `audio_package_c3`; `/Users/naman/.codex/worktrees/terra-audio-c3/Terra` | Small free licensed ambient/interaction kit, provenance, import recommendations and measured asset audit. No main-scene wiring. |

All three new worktrees start from `08092a1f19ceb064ab21c11c6f97dd221693d410`. They use the committed contracts and canonical `worlds/meadowbrook-v1` pack; they intentionally do not copy active uncommitted visual experiments. The internal selected-location ID stays `coon-valley-brye-v1`; player-facing copy uses Meadowbrook Valley.

## Ownership and review

- B edits only Simulation, dedicated simulation tests, `docs/model-assumptions.md`, and package-B evidence. No Core, contracts, scenario parameters, UI, or runtime integration changes.
- D edits only `content/education/`, dedicated education tests, `docs/learning-content.md`, and package-D evidence. Preserve existing fixture compatibility and scenario content IDs. No mission logic or layout edits.
- C3 edits only `Assets/Terra/Audio/ExternalKit/` and required metadata, dedicated audio audits, `docs/asset-kit.md`, and package-C3 evidence. Free licensed assets only; no generated images or purchases. No renderer, scene, packages, or global settings changes.
- Each owner records actual checks, limitations, a reviewable commit, and a handoff. Package completion is distinct from live-game integration and browser acceptance. Do not take over the active primary Unity Editor or publish a build from these side packages.
- TN-06/TN-08/CS-04 guide D; TN-07 guides C3. Preserve the [Terra Nil reference](DESIGN_REFERENCE.md) and [current art contract](art-direction.md). Audio and copy acceptance do not establish landscape quality.

## Why other work is not assigned again

| Plan item | Dependency / current disposition |
|---|---|
| A: geography | Already implemented for the same AOI. No new geography gap identified for this kickoff. |
| C1/C2: visual environment/intervention kit | Art direction, shaders and rendering are actively being replaced by the Phase 2A owner. A separate kit based on historical tokens would duplicate or constrain that work. |
| Phase 3 | Already assigned and underway in a separate chat following the user's clarification. |
| Phase 4 live consequences | Needs the Phase 3 state/editing result and B. B can ship independently; final preview/commit integration, analysis partition, water presentation and honest UI need coordinated integration. |
| Phase 5 mission | Needs integrated consequences before targets can be balanced and two winning configurations proved. D prepares its content now. |
| Phase 6 ending | Needs the mission lifecycle and actual comparison results. D prepares its text/question assets now. |
| Phases 7–8 | Require the complete playable flow for performance/usability audits, final release and rehearsal. C3 prepares its reusable audio now. |

## Integration decisions to preserve

The current simulation seam supplies one curve number per input unit, while the implementation plan requires per-component runoff aggregation rather than averaging nonlinear parameters across the landscape. B must document the frozen mixture approximation and show how a future adapter can supply disjoint subareas without double counting. Do not silently change DTOs or imply the present scalar seam is a complete full-AOI hydrology adapter.

Keep runoff marked unavailable in the live game until a verified adapter supplies baseline and proposal results under identical storm assumptions. A tested evaluator alone does not enable that UI claim. Mission thresholds remain illustrative and unbalanced until Phase 5 verification.

Review package commits independently before integrating with the Phase 2A/3 work. Reconcile shared presentation files through their existing owners; preserve active changes and historical evidence. This kickoff does not certify Phase 2A, add another location, start a live generator, or publish a release.

## Delivery status

All three packages are complete at their independent ownership boundaries, tested together and merged into the primary checkout at `1c6a1b0b8fe91a89cf7f0da1d95fafad55848c38`. [Integration report and phase boundaries](PARALLEL_INTEGRATION.md).

| Package | Delivery commits | Result |
|---|---|---|
| B | `b2630f1`, scalar edge fix `8597181` | 19 suites and API compilation pass; Unity execution matches frozen numerical fixture |
| D | `e8ca939`, Phase 3 compatibility/provenance fix `01a45ad` | 49 authored entries + fixture; 12 tests pass; both Core variants validate; all 50 entries round-trip in Unity |
| C3 | `db07d52`; import configuration included in integration commit `9304ba8` | Five licensed clips, 94 source checks, Chrome audition checks and all five Unity imports pass |

Combined Unity verification: 141 package checks plus the existing 139 state/geography checks. Integration preserved all 120 pre-existing modified/untracked files byte-for-byte. Package worktrees remain attached for recoverable history. Live Phase 4–6 adapters and final in-game audio mixing still belong to their planned phases; no phase acceptance or public deployment was implied by merging these packages.
