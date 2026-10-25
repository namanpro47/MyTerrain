# Sound implementation and release handoff

Runtime candidate: **e8ac9eb** on **codex/terra-sound**, based on refreshed main **67c01417906b74a8f024ae9975158cb1dda6979d**. Remote main13a15a4 was inspected during continuation; the combined release owner integrates newer main/atlas/guidance work. This chat does not independently promote main or deploy.

## Verified checkpoint

- 79 encoded clips, 7,827,093 bytes: 17 placement families with three separate takes, 23 interface/gameplay gestures, three nature beds and two music tracks. All sources are free CC0 with source/output hashes and processing in the bank manifest.
- Bank tests pass all four suites: all supported actions, distinct recorded takes, decodable finite signals/headroom/DC/loop seams, runtime cue references and provenance.
- Unity6000.6.6f1 imported and compiled source04549ad, passed **380 checks**, and completed a Web export. `unity-import.json` records the checks. `served-assets.json` records exact locally served export bytes and decoded sizes at http://127.0.0.1:8916.
- Chrome155.0.8059.85 background functional tests used the real exported game, regular user input and the real Web Audio destination. No autoplay bypass or game-state injection was used. A request abort exercises the actual offline recovery path. `chrome-receipts.json` contains read-only runtime snapshots plus measured audio output.
- All17 actual project placements produced their respective cues. Repeated farm placements cycled0→1→2→0. Undo, redo, move, remove, clear, invalid placement, camera reset, atlas travel/arrival, year submit, failed chapter, offline report, continue, decade completion and new run were exercised.
- No sound before first gesture. Mute measured zero output and persisted on reload; overall volume persisted. Music/nature at0 with effects enabled measured silent background; all buses at0 also measured zero. Normal playing output was nonzero; sampled peak during the walkthrough was0.203, below clipping. This is bounded sampling, not a peak measurement of every possible mix.
- Music automatically transitioned from Mystical Piano to Vaporware during gameplay. Ending fast-forward produced one ending cue instead of a cascade of yearly cues. Sound settings Enter/Escape left the pending town revision/cash/queue unchanged.
- No JavaScript exceptions. Unity emits startup metadata warnings while deferred audio loads; playback and later transitions succeed. These bounded load warnings are recorded separately, not concealed as clean logs.

**Audio feature signoff:** passed. Foreground Chrome focus loss measured zero output; return resumed at24.46→25.46seconds after roughly one second, with fade-in. Disable Playwright focus emulation before this check; its default reports every tab focused and cannot test actual focus behavior. Pointer volume/mute/Done controls passed. Settings were visually inspected at1440×900,1280×720 and1280×720 at deviceScaleFactor1.25, with readable labels and unobstructed controls. See chrome-headed.json and settings-*.png.

The combined release owner confirmed e8ac9eb compiles and passes380audio checks alongside HUD809, chapters16, atlas678, annual34 and community35 checks. No sound runtime changes were needed after the browser walkthrough. The standalone browser export is source04549ad; e8ac9eb adds native/rebound callback coverage and modal drag cleanup. The release owner owns the final combined-export smoke, main promotion and deployment; this handoff does not claim that production is already updated.

## Ownership and integration

Both atlas files, TerraMain.cs, TerraMain.Editing.cs and TerraHud.cs are frozen for this audio feature and explicitly released to their current owners. The combined release chat has integrated e8ac9eb. Preserve these narrow hooks while integrating newer visual and UI work:

- Initialize the persistent audio director before atlas attachment. Keep AudioTick before the atlas early return, lifecycle subscription cleanup, and the browser `AudioVisibility` bridge.
- Atlas selection/travel/arrival/failure request their semantic cues; buttons request common clicks. Keep a visible sound settings entry and the U shortcut.
- Sound settings consume pointer/keyboard/wheel input before atlas/game handling, including the frame they close, and clear a held map drag. Field Notes wheel forwarding belongs after this guard.
- Successful edits request placement/move/remove/clear sounds after the actual accepted plan operation. Undo/redo/failure keep separate gestures. Annual cues observe `AnnualTownSession.Changed`; they never mutate the model or use gameplay randomness.
- TerraAudioButton supports native clicks even when dialogs replace onClick callbacks; Web and custom keyboard dispatch also request common feedback. The director coalesces duplicates by priority.
- Field Notes owner may add ResetFieldNotes/LayoutFieldNotes/wheel hooks. No scenes, source rendering, financial rules or save schema are changed by audio.

Run `TerraAudioChecks.Run()` on the combined source, then test the actual combined Chrome export. No paid asset, account, API credential, plugin or package is required. This sound verification does not certify the separate Phase2A scenery gate.

## Reproduce browser checks

Use a fresh Chrome context and unique local origin. Install `tests/audio/chrome-instrumentation.js` before page load to observe the existing destination without adding audible output. `AudioAudit` on TerraMain emits a read-only JSON snapshot; F10 emits chunked UI rectangles and annual state. Parse `TERRA_UI_PART` chunks when the full log exceeds the browser log limit.

1. Before input, inspect `AudioAudit` and destination samples. Press U, test labelled controls with Tab/Enter and pointer; wait between key presses so each reaches a Unity frame. Test mute, all bus controls and reload persistence.
2. Enter Wisconsin, dismiss the introduction, choose each catalog project, Tab to an eligible field, Enter to queue, then Z to release the reservation. Test all17 projects across all three categories/pages, plus Y, V, Delete/Enter, X/Enter, R and invalid targets. Three repetitions verify alternate takes.
3. Review E, submit, test a failed chapter and actual offline recovery, continue, T/confirm the future, and start a new run. Confirm appropriate actual-state cues and one ending cue.
4. Leave/return to the owned tab in headed Chrome; verify zero output while unfocused and resumed music position. Capture the settings at desktop and compact viewports. Keep separate evidence for any final combined export.

References: TN-07 quiet responsiveness, TN-08 appreciation, AX-01 sound-independent information. See docs/AUDIO.md for the design, coverage, controls, licenses and reproduction.

## Delivery status

All audio-owned implementation and functional verification is complete. Desktop67 and heavy50 were released after their actual jobs finished; owned browser sessions are closed. Runtime is frozen at e8ac9eb. Evidence-only follow-up commits may be merged without rebuilding. Preserve the combined release owner’s final export identity rather than replacing it with the standalone test export.
