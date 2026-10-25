# Action feedback — October 25, 2026

Base: refreshed `origin/main` `13a15a43c43bab9e0eb6a2a90bded4bb4d7aba0c`, isolated `codex/action-feedback` worktree. Incoming hosted-chapter work through `7382272` is preserved. No changes to costs, score rules, accepted history, location content or save schemas.

## Behavior

- Queue/replace/move/remove/clear/undo/redo produce one signed action receipt. The prominent amount is the exact change to **available funds**, including the full reservation term. It is explicitly labelled reserved/released; planning does not claim a cash payment.
- Economic, Social and Environmental rows show signed one-decimal changes and their resulting values. Plan edits compare successive year-end forecasts; accepted years compare accepted outcomes. Continuing to a new planning year explains changed forecasts from conditions/project ages.
- Changed indicator labels show an adjacent signed badge for seven seconds; available money briefly changes color. A short entrance/fade is removed under reduced motion. Signs and text preserve meaning without color or sound.
- Distinct original, synthesized two-note cues cover reserving funds, releasing funds, neutral plan edits, positive/negative/mixed annual outcomes. A dedicated source replaces rapid cues instead of stacking them. Existing mute, focus and user-gesture behavior applies.
- Budget → Recent changes retains the last eight receipts, with before/after funds, cash delta and accepted paid/income totals. A modal or background tab pauses receipt visibility; validation errors take priority. Receipts do not intercept pointer input.
- Initial load, saved reload, report navigation and new runs do not generate fake changes. Repeated refreshes and chapter retries do not duplicate outcomes. Fast-forward is summarized from the previously observed accepted state.

## Verification

- 345 new deterministic feedback assertions across all 17 action variants; full-game suite totals 953 assertions with connected-field fixtures.
- 837 Unity HUD/session assertions, including 28 new receipt/audio checks and 787 existing project/overview checks. Checks cover readable standard/larger text, signed values, mixed outcomes, history bounds, input passthrough, error priority and nonclipping/click-free cue samples.
- 50 chapter assertions after reconciliation. The old HUD test wording was updated to the incoming “local summary” labels.
- Unity 6000.6.6f1, CLI 1.0.0-beta.14: compilation passes without errors/warnings.
- Actual Chrome captures and export receipt are recorded below after browser verification.

Commands (the local machine has .NET 10; the suites target .NET 8):

```sh
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/full-game/Terra.FullGameChecks.csproj -- --connected --evidence /tmp/terra-feedback-checks
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/chapters/Terra.ChapterChecks.csproj
```

Presentation references: **TN-03 / TN-05 / TN-07 / CS-01 / AX-01**. Compared with the existing play-first/annual HUD captures: the previous generic “plan updated” toast and tiny forecast line are replaced by precise feedback with an action name. Scenery remains unchanged. This verifies the feedback implementation, not Phase 2A visual acceptance or a human usability study. No public deployment is implied.
