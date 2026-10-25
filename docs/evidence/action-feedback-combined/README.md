# Combined feedback verification — October 25, 2026

Read-only verification of the immutable combined WebGL export at source `770d4c05b7f50f366275c036cd769a0c91414644`. All 21 file SHA-256 hashes match the release owner’s manifest; copied to this task’s ignored `Builds/FeedbackCombined770d4c0` and served on isolated origin localhost:8877. No second export or shared output write.

The combined source preserves exact available-funds receipts, forecast versus accepted score baselines, undo/redo, inspectable history and focus-paused lifetime. Unified TerraAudio fallback requests use priority 1 with no model-driven Gesture; category/undo/annual cues retain precedence. Fast-forward ends at the accepted report before the optional future comparison.

The release owner’s `final-native-checks.json` records successful actual Unity execution: HUD/session 881, audio 380 (79 clips / all 17 action families), chapters 16 and atlas/ten-year/replay 678. Counts overlap with earlier feature suites. The main-game HUD suite calls ActionFeedbackChecks, including focus and stalled-frame lifetime checks.

This export predates a newly requested globe redesign. Feedback verification here does not imply that redesign is complete, that main has been pushed, or that production has been deployed.

## Actual Chrome 1024×576 check

- `queued-1024.png`: Traditional farmland reserves -$120,000; Economic +8.2 to58.3 and Environment +0.2 to39.0. Popup and adjacent headline badges are readable beside the inspector and above the dock.
- `undo-larger-1024.png`: +$120,000 released; -8.2 /0.0 /-0.2 score reversals, with larger text and reduced motion.
- `audio-muted-1024.png` and `redo-muted-larger-1024.png`: unified audio mute works in the UI; the full visual receipt remains after redo. The first Y immediately after closing the sound modal was intentionally swallowed by the modal dismissal guard; a subsequent Y worked.
- End Year reached accepted state with an observed +$1,000,000 available /+$898,000 cash receipt and +8.2 /0.0 /+0.2 actual changes. The saved `accepted-larger-1024.png` was taken after the transient receipt expired, so it records the permanent headline instead.
- This exposed inconsistent old rounding: permanent headline showed +8.1 /+0.1 after the popup's +8.2 /+0.2, despite visible before/after scores agreeing with the popup. A bounded shared display-rounding fix was requested from the release owner before its next export; do not consider this mismatch resolved by these captures.
- `chrome-log.json`: no error-level messages; captures actual F10 audit and state logs. No human listening or performance claim.

Desktop94 released and own Chrome tab closed. New globe redesign and final main/public release remain under the integration owner.
