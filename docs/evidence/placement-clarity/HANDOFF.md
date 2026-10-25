# Placement clarity — implementation checkpoint

Owner: chat `01a17420-9b30-7261-a597-ddea2da523bb`, board task `placement-clarity-20261025d`, worktree `/Users/naman/Documents/GitHub/Terra-placement`, branch `codex/placement-clarity`. Base `13a15a43c43bab9e0eb6a2a90bded4bb4d7aba0c`, plus the Terra Nil research/plan commits. Resolve the exact candidate with `git rev-parse codex/placement-clarity`; this document ships with it.

## Status for integration

Active runtime changes, **not a completed or visually accepted implementation**. The user requested the full placement plan; this checkpoint must not be described as fulfilling it. The request to consolidate current branch work is acknowledged. A consolidated release must preserve this status and run affected Unity/Chrome checks before asserting readiness. No independent main push or deployment from this task.

Implemented: neutral browse entry; stable dock and fixed inspector; labeled selected tools; keyboard-accessible project explanations and reopenable placement help; accepted-only outcome readouts; explicit reservation language from the existing annual engine; exact footprint/whole-field preview boundaries and blocked cross; quiet eligible sites; queued boundaries/labels; explicit replacement review; opening camera composition candidate. No new time system or financial ledger.

**Shared-file handoff completed.** This branch now incorporates Field Notes runtime `6364b3c` / evidence `2bf54e6` (local cherry-picks `3c77ec4`, `5a78bc3`, `23e58cb`) and preserves its scroll/reset/layout hooks, consolidated budget and literal End Year caption. Input glue captures site/tool/mode/source/revision on world or Queue pointer-down, validates before queue/replace, clears pending commands on wheel/focus/cancel/keyboard input, suppresses previews during drag, and uses the new opening composition for Reset View. Focused keyboard button activation consumes the key instead of falling through to map placement. Annual catalog prices select the named Action price label explicitly.

Native checks are running under reservation **83** after76 was released to expand ownership. Neither the new input routes nor the presentation are visually accepted yet. Preserve incoming audio/guidance and action-feedback callbacks when combining branches; the latter has separate changes to Editing.cs and PlayFirst.cs. Its feedback must not replace accepted-only outcome labels with unlabelled forecasts.

## Evidence and outstanding gates

- `core-regression.log`: existing full-game suite, 608 assertions passed using `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/full-game/Terra.FullGameChecks.csproj`. This tests unchanged game rules, not new rendering/input.
- `git diff --check`: passed at checkpoint.
- `PlacementClarityChecks.RunBatch`: authored but not run yet. Reservation83 targets only the placement worktree; cold import is in progress. Covers all 17 recipes at 1024/1280/1440/1920, larger text, dock stability, input-intent guards, footprint geometry and shader import.
- Required next: finish Unity compile/checks; use the release owner’s final combined Web export; actual Chrome queue/inspect/cancel/move/replace/remove/undo/redo and failed-input checks; responsive/zoom/accessibility review; normal/close/overview before/after captures; preview/placement measurements.
- No Chrome, Unity compilation, performance or novice-comprehension acceptance is claimed by this checkpoint. Broader Phase 2A scenery remains separately gated.

## Plan traceability

P0: `docs/TERRA_NIL_INTERACTION_PLAN.md` and `docs/evidence/ux-reference-2026-10-25/index.html` contain six state layouts and direct supplied/video references (TN-03/04/05/06, CS-02/03/04, AX-01). The research HTML itself still needs a final browser layout check.

P1/P2/P3: active code in Main.Editing/Placement, HUD.BuildActions/AnnualLayout/PlayFirst/Placement, HudPlacementAnchor and LandscapeRenderer/Placement. P3 input glue is now connected; native and browser verification remain pending.

P4: opening camera hypothesis plus world cue hierarchy; accept only after actual Chrome comparison with references and normal/close/overview evidence (TN-01/02/09–12).

P5: scripted engineering walkthrough pending. Uncoached human exercise remains explicitly open if participants are unavailable; internal scores cannot substitute for it.
