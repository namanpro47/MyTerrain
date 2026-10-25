# Combined-release handoff

Candidate branch: `origin/codex/action-feedback`. Runtime `d6d9959`; checks `d6e0a80`. Exact full candidate SHA is `d6e0a80993dbcd81bed35cfa31a5170f1f6c2578`. Base was `13a15a4`, and the branch includes hosted chapter main through `7382272`. No independent main push or deployment.

## Active versus isolated

This is active main-game feedback, wired in TerraMain.Refresh / ConfirmEdit / History. No new ledger, simulation, save schema, renderer or scene. Additive implementation is `AnnualFeedback.cs` (pure observer) and `TerraHud.Feedback.cs` (receipt/history/metric badges). `ActionFeedbackChecks` and the standalone `AnnualFeedbackChecks` exercise those behaviors.

## Required reconciliation with sound / placement / Field Notes

The branch was implemented against the prior small audio system. **Keep the comprehensive audio owner's TerraMain.Audio.cs and TerraAudio director**, rather than keeping this branch's standalone generated-chime AudioSources. There must be only one master/effects/mute system in the combined game.

- Preserve the sound branch's `PlacementCue(treatmentId,editMode,site)` before clearing editMode/treatmentId, and its undo/redo requests. Preserve placement owner's hit testing/confirmation changes.
- Preserve this branch's feedbackCause capture before the edit; pass it to `Refresh(feedbackCause)` after a successful command. Replace generic success toasts with the exact receipt; leave rejection/error toasts and blocked cues intact. No-op undo/redo should return without success feedback.
- Preserve sound's early AudioTick, panel input guard and cleanup. Keep `hud.TickChangeFeedback(Time.unscaledDeltaTime)` once per ready landscape frame. Remove this branch's `DisposeFeedbackAudio` call when selecting sound's Audio.cs.
- Keep `hud.FeedbackSound += ChangeCue` and add the following small adapter to the sound-owned TerraMain.Audio.cs. The release owner requested priority 1 for every feedback fallback and no model-driven Gesture: existing placement, undo and annual cues take precedence. Do not retain the generated `ChangeChime` clips or independent source alongside TerraAudio.

```csharp
void ChangeCue(Terra.Core.AnnualFeedbackTone tone)
{
    var audio = Terra.Audio.TerraAudio.Ensure();
    switch (tone)
    {
        case Terra.Core.AnnualFeedbackTone.Decline:
        case Terra.Core.AnnualFeedbackTone.Mixed: audio.Request("year-attention", 1); break;
        case Terra.Core.AnnualFeedbackTone.Outcome: audio.Request("year-report", 1); break;
        case Terra.Core.AnnualFeedbackTone.Release: audio.Request("undo", 1); break;
        default: audio.Request("ui-confirm", 1); break;
    }
}
```

- Retain the receipt/input/history assertions in ActionFeedbackChecks; remove its final standalone `ChangeChime` sample loop when switching to the bank. The comprehensive audio suite owns bank signal checks.
- `TerraHud.PlayFirst.cs` routes the existing Budget button to `ShowPublicBudget`, which offers Recent changes. Preserve placement's inspector/dock fixes around that callback.
- `TerraHud.cs` has one small Toast hook: nonempty rejection/navigation toasts dismiss the transient receipt, preserving its inspectable history. Keep Field Notes's text-layout changes.
- MainGameChapterChecks now calls ActionFeedbackChecks after PlayFirstHudChecks. This branch also fixes its old offline-label expectations to incoming “local summary”; preserve newer Field Notes expectations when merging.

## Two final integration edge cases

1. Fast-forward currently opens the comparison modal immediately, which defers its receipt until the modal closes. Prefer the existing `ShowAcceptedReport()` at the successful FinishYears handoff, then the existing “See ten-year future” button; this gives the aggregate signed receipt a visible map/report surface, matching manual Year 10. On a partially completed fast-forward error, refresh the accepted landscape/HUD before showing recovery.
2. The final bounded follow-up now implements focus-independent receipt visibility, focus-paused timing and a `.1f` frame-time cap in the owned `TerraHud.Feedback.cs`. Two extra UI assertions cover visibility and stalled-frame reading time. Use the branch tip for those files; the Web export below predates this follow-up. No map/modal input is intercepted.

## Verification already completed

345 new feedback assertions across all 17 variants; full-game connected-field suite 953 total; Unity UI/session 839 total including 30 receipt/audio checks; 50 chapter assertions; zero compile errors/warnings. A cold WebGL export of d6e0a80 was already in flight when the shared board was adopted; it is finishing in this worktree. No new heavy jobs will start while release reservation74 is held. Chrome local receipt verification is queued as desktop79 behind Field Notes77. Final browser/build evidence will be an evidence-only follow-up unless a concrete visual defect requires a bounded correction.

Local served output: `/Users/naman/.codex/worktrees/action-feedback/Terra/Builds/Web`, port8876. Responsive test wrappers are ignored files in Builds, not production HTML. Do not substitute this standalone export for the release owner's combined export.

## Final browser handoff

Standalone Web export succeeded (20 files, 40,149,226 bytes), source d6e0a80; hashes in build.json. Chrome screenshots verify queue, undo, accepted-year and recent-history signed changes at 1440×900. Reload at 1024×576 was silent; reduced motion, mute and larger text toggles were confirmed, and a second-year restoration was queued. The compact screenshot caught the unfocused-visibility issue and predates the final timer fix, so it is not proof of the corrected compact receipt. The final F10 browser call timed out and the test tab was gone on recovery; no compact console capture is claimed. Wide console contains no errors and one pre-existing stripped EASU shader warning. See README for captures. Desktop79 released. The combined release owner remains responsible for merged audio routing, fast-forward handoff, compact receipt and public verification.
