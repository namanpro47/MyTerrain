# Planning guidance cancellation regression — October 25, 2026

Peter's screenshot shows `ArgumentException: Choose an accepted project that has not already been cancelled.` at `AnnualTown.CancellationRefund`. The local Editor log traces the call through `AnnualTown.Preview` → `AnnualPlanningGuidance.Create` → `TerraHud.AnnualState` → `TerraMain.Refresh` on landscape entry.

The local checkout remained on `615459a` after the prior sync; its reflog shows no subsequent update when the exception occurred. At inspection, the other registered tasks owned building-overlay and performance worktrees, and the only Editor reservation targeted the separate performance project. This error is a planning-guidance regression in the new cancellation integration, not evidence of another chat overwriting the open local project.

## Fix and checks

Guidance now excludes `cancel-project` from vacant-site construction offers, handles negative cash before attempting construction previews, and excludes cancelled markers from occupied sites and committed service capacity. The cancellation validator still rejects invalid cancellation requests normally.

A fresh-run guidance test reproduced the exact exception before the fix. Afterwards **81 player-rule assertions** pass, including new cases for opening HUD guidance, no state mutation, negative-budget guidance and rebuilding on a cancelled site. The existing connected full-game suite also passed **1,019 assertions**, including 61 planning-guidance checks.

Work is isolated in `C:/Users/Peter/Terra-planning-cancellation-fix`. No Play Mode, scene, local checkout sync or Editor automation is performed for this repair while Peter is testing. User-facing layout/LLM interaction acceptance remains with Peter.
