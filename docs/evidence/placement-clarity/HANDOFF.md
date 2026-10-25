# Placement clarity — implementation checkpoint

Owner: chat `01a17420-9b30-7261-a597-ddea2da523bb`, board task `placement-clarity-20261025e`, worktree `/Users/naman/Documents/GitHub/Terra-placement`, branch `codex/placement-clarity`. Base `13a15a43c43bab9e0eb6a2a90bded4bb4d7aba0c`, plus the Terra Nil research/plan commits. Resolve the exact candidate with `git rev-parse codex/placement-clarity`; this document ships with it.

## Status for integration

Active runtime changes, **not a completed or visually accepted implementation**. The user requested the full placement plan; this checkpoint must not be described as fulfilling it. The request to consolidate current branch work is acknowledged. A consolidated release must preserve this status and run affected Unity/Chrome checks before asserting readiness. No independent main push or deployment from this task.

Implemented: neutral browse entry; stable dock and fixed inspector; labeled selected tools; keyboard-accessible project explanations and reopenable placement help; accepted-only outcome readouts; explicit reservation language from the existing annual engine; exact footprint/whole-field preview boundaries and blocked cross; quiet eligible sites; queued boundaries/labels; explicit replacement review; opening camera composition candidate. No new time system or financial ledger.

**Shared-file handoff completed.** This branch now incorporates Field Notes runtime `6364b3c` / evidence `2bf54e6` (local cherry-picks `3c77ec4`, `5a78bc3`, `23e58cb`) and preserves its scroll/reset/layout hooks, consolidated budget and literal End Year caption. Input glue captures site/tool/mode/source/revision on world or Queue pointer-down, validates before queue/replace, clears pending commands on wheel/focus/cancel/keyboard input, suppresses previews during drag, and uses the new opening composition for Reset View. Focused keyboard button activation consumes the key instead of falling through to map placement. Annual catalog prices select the named Action price label explicitly.

Native validation is complete: **2,839 assertions passed**, including788 existing play-first checks and a focus-loss cancellation case. Shared input paths compile. Browser interaction and visual acceptance remain pending. Preserve incoming audio/guidance and action-feedback callbacks when combining branches; the latter has separate changes to Editing.cs and PlayFirst.cs. Its feedback must not replace accepted-only outcome labels with unlabelled forecasts.

## Evidence and outstanding gates

- `core-regression.log`: existing full-game suite, 608 assertions passed using `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/full-game/Terra.FullGameChecks.csproj`. This tests unchanged game rules, not new rendering/input.
- `git diff --check`: passed at checkpoint.
- `PlacementClarityChecks.RunBatch`: passed2,839 assertions under reservation84 (Unity6000.6.6f1 / CLI1.0.0-beta.14). Covers all 17 recipes at 1024/1280/1440/1920, larger text, dock stability, input-intent guards, footprint geometry and shader import.
- Required next: use the release owner’s final combined Web export; actual Chrome queue/inspect/cancel/move/replace/remove/undo/redo and failed-input checks; responsive/zoom/accessibility review; normal/close/overview before/after captures; preview/placement measurements.
- No Chrome, performance or novice-comprehension acceptance is claimed by this checkpoint. Broader Phase 2A scenery remains separately gated.

## Plan traceability

P0: `docs/TERRA_NIL_INTERACTION_PLAN.md` and `docs/evidence/ux-reference-2026-10-25/index.html` contain six state layouts and direct supplied/video references (TN-03/04/05/06, CS-02/03/04, AX-01). The research HTML six-state Chrome review is now complete; see reference-board-review.json and reference-*.png. This is schematic evidence, separate from actual game acceptance.

P1/P2/P3: active code in Main.Editing/Placement, HUD.BuildActions/AnnualLayout/PlayFirst/Placement, HudPlacementAnchor and LandscapeRenderer/Placement. P3 input glue is connected and native guard checks pass; actual browser verification remains pending.

P4: opening camera hypothesis plus world cue hierarchy; accept only after actual Chrome comparison with references and normal/close/overview evidence (TN-01/02/09–12).

P5: scripted engineering walkthrough pending. Uncoached human exercise remains explicitly open if participants are unavailable; internal scores cannot substitute for it.

## Frozen source / combined-release reconciliation

The final native run is `Builds/placement-checks-4.log`; the committed receipt is `structural-checks.json`. First run failed because edit-mode tests needed EventSystem.OnEnable; that setup was fixed. The broader regression then caught the uppercase Environmental heading clipping at14px; title-case headings fixed it without shrinking text. Final output: PLAY_FIRST_HUD_CHECKS_PASSED788 and PLACEMENT_CLARITY_CHECKS_PASSED2839.

**Preserve dense main `ea37af0` parcelPack ResetView and starting-settlement framing.** `PlacementArrivalView()` in this branch is a legacy13a15a4 composition hypothesis only. On combined main use it only for nonparcel fallback, if useful; never override parcel-centered framing. These native fixtures cover the legacy model plus Field Notes; the combined parcel game still requires its own input/render checks.

User explicitly authorized coordination with the release chat. It owns the one combined Web export/Chrome/deployment stage; this task releases shared file claims after freezing the tested source. No additional standalone export. Preserve its audio and action-feedback callbacks, Field Notes hooks and accepted history.
