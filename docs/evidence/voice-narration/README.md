# Voice narration evidence

Fresh base: `19e7eae835ed4c06a21d42887a9a53ed28cc4fc2`, fetched October 25, 2026. Isolated worktree: `elevenlabs-narration/Terra`. Existing local changes preserved.

- Nine Node tests pass (`node-checks.txt`).
- Explicit user approval received for the restricted key. Created **MyTerrain Narration** with TTS + Voices read only, 10,000 credits per refresh period, auto-disable on leak retained. Stored only in ignored mode-0600 `.env` and Vercel sensitive production/preview variables. No credential is in this receipt.
- Authenticated voice lookup confirms Matilda, female American, informative/educational (`voice.json`).
- Actual Chrome transport preview at `127.0.0.1:8997` played opening and year-one samples using the real gateway on8788. Observed playing→muted/paused→new passage→completed→closed/idle. No browser errors/warnings. `chrome-live-voice.png` shows the completed annual passage. This is transport evidence, not the Unity game itself.
- Preliminary C# compiler check records runtime source against the installed engine/package assemblies (`compiler-check.json`); it does not replace Unity import/export.

## Combined game check

The release owner combined narration into source `e74496bf55ffa77427be69dcb221bb3bd70f3b41`. The actual Unity compile revealed the missing built-in Web Request Audio module; installed through Unity PackageManager and committed in `3b9db89`. The combined native checks (1,316 + 16) and WebGL export then passed.

Chrome at `http://127.0.0.1:8998/`, 1728 × 940:

- Starting invitation read aloud: status 2, 82 characters. Its body click entered Wisconsin successfully; old speech stopped during the descent (status 0, zero characters).
- Opening tutorial read aloud: status 2, 379 characters. Voice off produced disabled/status 3; voice on and Replay returned to status 2.
- Advancing to run instructions replaced the previous passage: status 2, 371 characters.
- `chrome-opening.png` and `chrome-dialogue.png` show the actual Unity UI. Voice controls fit the existing modal header without covering its title or actions. This retains the quiet feedback and field-notes hierarchy (TN-06/07, AX-01); scenery acceptance is separate.
- `chrome-integration-logs.json` preserves the observations. The shader-stripping warning is existing renderer output, not a voice error.

**Release blocker found:** selecting “Begin · 10 decision years” caused WebAssembly `RuntimeError: unreachable` in the shared game start path. Handed the trace to the gameplay/release owners; production held. Yearly dialogue and sound-panel acceptance require the corrected export. No final production or visual-gate acceptance is claimed yet.
