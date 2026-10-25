# Contextual annual planning guidance

Initial candidate `53dd2e0` on refreshed main `ba59445` passes model, Unity and Chrome functional verification. Its actual browser audit identified an inherited 12px summary row that ignored the larger-text setting. The correction at `daa09ba` passes 399 actual Unity HUD checks and Chrome at 1280×720/1024×576, with 14px/15.68px text and unchanged Year 2 state. Main subsequently delivered an approved play-first HUD and native atlas; the reconciled candidate preserves those interfaces, uses shorter contextual prompts, and passes 783 guidance checks plus 809 existing HUD and 16 chapter checks; its export/browser verification is in progress. These isolated candidates are not published or accepted releases.

The existing catalog summary now describes the next annual step when no project is selected: inspect, queue without paying, review/commit, wait for the pending result, understand the report, or adapt. In the reconciled play-first HUD, selecting a project retains the existing outlined-field instruction and available count; full upkeep remains in its inspector. Cancelling restores contextual guidance. No extra panel, questionnaire, ledger, save format or rule change is introduced.

After the first accepted year, the prompt can identify an unmet community need and suggest a real affordable offer that can open before Year 10. It accounts for already committed capacity, including projects still under construction. Drinking-water scarcity is not presented as something wastewater treatment or waiting fixes. Unaffordable projects, occupied sites and late construction lead to an explicit Wait option with remaining obligations. Suggestions are inspection prompts, not net-score predictions or optimal strategies; the player still reads the preview and chooses.

Reference patterns: **TN-06** (brief contextual instruction and deeper information on demand), **CS-04** (consequences and actionable feedback), **CS-01** (preserve the catalog hierarchy), **AX-01** (actual-size legibility). The existing [connected HUD captures](../hud-connected/README.md) are the layout comparison; they do not establish Phase 2A scenery acceptance.

## Model evidence

Both current connected and retained legacy Wisconsin layouts pass **2,460 assertions**, including 61 new checks. The tests use the actual authoritative annual model and verify unchanged snapshots, full commitment affordability, vacant sites, pending/accepted boundaries, already funded housing/power, drinking-water honesty, no available sites and Year 10 construction limits. Existing 51-family/order routes and failure/reload recovery at every year still pass.

```sh
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/full-game/Terra.FullGameChecks.csproj -- --connected --stress --evidence Builds/AnnualGuidance/model-connected
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/full-game/Terra.FullGameChecks.csproj -- --stress --evidence Builds/AnnualGuidance/model-legacy
```

Receipts: [connected](model-connected.json), [legacy](model-legacy.json), [runtime/test source hashes](source-hashes.json). The underlying strategy/stress evidence and its limits remain in the [annual audit](../annual-completion/README.md).

## Unity verification and remaining browser check

The [disposable HUD check](hud-guidance-check.cs) passes **255 assertions with zero failures** in Unity 6000.6.6f1: actual summary binding, selected-project/cancel behavior, all authored fallback strings and catalog titles at 1024×576, 1280×720, 1440×900 and 1920×1080 with both text settings. The live annual engine remains byte-identical. [Actual result](hud-check-result.json). The 34 existing Unity lifecycle/JSON checks also pass; Play Mode advanced from frame 5 to 6,598 before testing. This is an isolated headless Editor, not browser visual evidence.

The initial export succeeds in 668.279 seconds. [Build result](build-result.json), [build inputs and import changes](build-provenance.json), and [31 byte-matched served files](served-files.json) pin the frozen output. No sibling Editor or shared Library is used.

Chrome at 1280×720 verifies initial instruction, project-specific upkeep, queued reservation without payment, Undo/Redo, explicit pending/offline recovery, three annual chapter cards, Year 2's affordable housing suggestion, cancellation and exact saved reload. The one accepted park leaves $10,950,000 cash and $20,000 commitments. At 1024×576 with larger text, the hint fits but still measures 12px; that confirmed readability defect prompted the correction. These captures are retained as functional evidence and a before comparison: [initial hint](chrome-intro-1280.png), [queued](chrome-queued-1280.png), [adaptation](chrome-adapt-1280.png), [suggested project preview](chrome-suggested-preview-1280.png), [compact larger-text defect](chrome-large-1024.png), [saved reload](chrome-reloaded-1280.png). Corresponding JSON files contain actual rendered UI geometry and annual diagnostics. Error-level console observations were empty; the deliberately unavailable chapter service used the visible manual offline recovery of this current runtime contract.

The correction changes only the existing row's font size, preserving its position and dimensions: 14px normally and 15.68px with larger text. The updated HUD check now requires positive measured text dimensions and the minimum font size as well as fit. Actual 125% browser zoom was not established and is not claimed. These bounded checks do not close the broader final accessibility/performance/visual gates, or implement the later report-only narration requirements newly specified on main `3562320`.

Human playtesting is unavailable by the user's instruction. No novice comprehension, play duration, audio listening or final visual acceptance is claimed.

## Corrected predecessor verification

`daa09ba` passes **399** HUD assertions, including all 17 selected-project summaries and positive rendered dimensions, with unchanged live state ([result](hud-readable-result.json)). Its export succeeds in **380.936 seconds** ([result](readable-build-result.json)); [provenance](readable-build-provenance.json) pins 32 output files (39,983,134 bytes) and [served verification](readable-served-files.json) matches all 31 served files. Its owned Editor was closed before browser inspection.

Actual Chrome confirms 14px at 1280×720 and 15.68px at 1024×576 (17.52px preferred height in the 20px row), including the selected Homes summary. Reloaded Year 2 remains revision 5, $10,950,000 cash, no queued project, one accepted asset. [Normal text](chrome-readable-1280.png), [compact larger text](chrome-readable-large-1024.png), [selected project](chrome-readable-selected-large-1024.png). Audit JSON accompanies each capture; [error-level console observations](chrome-readable-errors.json) are empty. These captures use the predecessor HUD; they are not evidence for main's newer native atlas/play-first integration.

## Reconciled play-first candidate

Candidate `cb9e37e` preserves main `67c0141` native MyTerrain atlas, icon-led HUD, project facts and short selected-field instructions. Idle suggestions now use short prompts such as “Housing need · inspect Homes”; longer explanations remain in existing details. The original summary row retains 14px/15.68px sizing. The disposable HUD matrix now exercises both open and closed inspectors across all four viewports and both text settings: **783 assertions, zero failures**, unchanged live state ([result](hud-combined-result.json)). All **809 existing main/play-first checks plus 16 chapter checks** also pass ([result](play-first-regression-result.json)). Both pure model layouts still pass 2,460 assertions.

The combined export succeeds (420.290-second Unity player build; 421.429-second job). Its frozen output has **20 files / 40,169,854 bytes**; all **19 served files** match. [Provenance](combined-build-provenance.json), [job result](combined-build-result.json), [served hashes](combined-served-files.json), [connected model](combined-model-connected.json), [legacy model](combined-model-legacy.json). The owned Editor and detached job are closed. Final combined Chrome confirmation is queued; do not treat predecessor browser captures as that pass.
