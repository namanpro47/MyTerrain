# Consistent visible score changes

Chrome verification of combined770d4c0 found Economic50.1→58.3 displaying +8.2 in its receipt but +8.1 in the permanent headline; Environmental38.8→39.0 similarly disagreed by .1. Both were valid calculations at different precision, but that is misleading player feedback.

Reconciled the feedback branch with combined770d4c0 before editing. A single `AnnualFeedback.DisplayDelta(before,after)` subtracts the values the player actually sees. `Score` uses the same one-decimal, away-from-zero rounding. AnnualState, PlacementAcceptedState and accepted ledger score rows now share those helpers, as does the transient observer. No scoring rules, stored values, layouts, audio or costs change.

Five new pure regressions cover positive/negative deltas, the observed .1 boundary, exact midpoint behavior and invisible changes without negative zero. The full connected-layout suite passes; feedback assertions increase345→350. `git diff --check` passes.

Ten new native UI assertions exercise real accepted annual state through the annual HUD, placement headline and ledger, using a deliberately varied display baseline to expose hidden-precision mismatches. They are included in ActionFeedbackChecks, which MainGameChapterChecks calls. These native assertions require the combined release owner's warm Editor run; they are not claimed passing here. Own idle Editor was closed before source reconciliation, avoiding a competing import/export.

Request before next release: run CheckSessionAndHud in the combined Editor, then verify that after receipt expiry the permanent score change agrees with the receipt. Chrome fixture from the preceding audit is own8877, larger text/reduced motion/muted Year1 accepted Traditional farmland. Its older frozen export remains intentionally unchanged.
