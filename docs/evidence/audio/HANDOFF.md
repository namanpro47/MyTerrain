# Audio checkpoint for the combined release

Source candidate: **442278b** on **codex/terra-sound**, based on refreshed main **67c01417906b74a8f024ae9975158cb1dda6979d**. The release chat asked for a feature-branch handoff and will own combined main promotion/deployment; this audio chat will not independently deploy or promote main.

## Current validation

- Encoded-bank checks pass: 79 valid clips; all 17 actual action IDs map to distinct material families with three separate takes; all literal runtime cue references resolve; source/output hashes, CC0 licenses, signal headroom, DC offsets and nature loop boundaries verified. Bank is 7,827,093 bytes.
- Unity import, compile and Web export are running in the owned isolated worktree under reservation 40. Chrome/runtime verification is pending. **Do not treat this source-only checkpoint as release-ready until the final receipt below is updated.**

## Atlas ownership handoff

The audio hooks in `WorldAtlas.cs` and `WorldAtlas.Visuals.cs` are frozen at 442278b. No further visual/layout edits are planned here. Preserve these hooks when replacing the selector UI:

- All atlas button callbacks call `TerraAudio.Ensure().Click()` before the existing action.
- Selection change requests `atlas-select`; StartJourney requests `travel`; successful descent requests `arrival`; LoadFailed requests `blocked`.
- Atlas Update returns while `TerraAudioPanel.ConsumedInput` so settings keys cannot operate the globe behind it.
- The visible Sound settings button opens `TerraAudioPanel.Show` (U also works from the main input path).

These calls require the complete additive `Assets/Terra/Scripts/Audio` runtime and Resources/Audio bank from the same commit. Main bootstrap now starts the persistent director before attaching the atlas. Its own mute/focus policy works while the atlas pauses the legacy scene listener.

The board currently retains the atlas file claim only because the running reservation cannot be released mid-build. As soon as the owned build finishes, this chat will finish/re-register its narrower task without the atlas files and record the release here. Research/new selector partials can proceed independently meanwhile.

## Integration steps

1. Integrate the complete verified feature branch, retaining newer annual and visual work. Audio never changes annual arithmetic, saved rules or scene data.
2. Keep narrow callbacks in `TerraMain.Editing.cs` (success after a plan edit, undo/redo, invalid actions). `TerraMain.Audio.cs` observes accepted phase changes through `AnnualTownSession.Changed`.
3. Preserve main input guards for sound settings and the `player.js` browser focus bridge.
4. Run `TerraAudioChecks.Run()` on the combined source and Chrome smoke the actual combined export. A scene-only confirmation is insufficient for browser audio.
5. No new external account, API key, paid asset, plugin or package is needed. The source bank and license material ship in the repository.
