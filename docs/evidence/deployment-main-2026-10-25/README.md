# Main production refresh — October 25, 2026

Published clean-source `913c79f` from a fresh isolated worktree to the existing production project. Runtime/build inputs under Assets, Packages, ProjectSettings, worlds and scripts remain identical through reconciled main `7e95434`; subsequent incoming work contained only tests, documentation and evidence. No experimental visual branch was merged.

Unity 6000.6.6f1 / CLI 1.0.0-beta.14 completed the fresh optimized WebGL build successfully, with no reported Editor errors. The frozen export is `Builds/Verified/913c79f` in this publication worktree. [Build provenance](build-provenance.json) records clean source and timestamps. Two orphan DOTween debug metadata files automatically removed during import were restored; runtime source is unchanged.

Production: https://terra-browser.vercel.app/ — deployment `dpl_DvwrgFpEd8YyYUevRgbyqLXqbR1Z`, Ready at 08:49 America/Los_Angeles. All **31 runtime files / 39,997,384 bytes** returned anonymous HTTP 200 and exactly matched the export; Brotli payloads decode and WASM magic passes. [Local files](local-assets.json), [public files](public-assets.json), [identity](identity.json).

Automated verification on source `913c79f`: **804 checks** across annual town (94), annual gameplay (15), chapter handling (42), full ten-year game (608), atlas packages (41), and compression (4). All three world packs passed packaging/checksums. [Counts](checks.json). Teammate stress-test evidence arriving later remains separately attributed.

Chrome smoke checks passed locally and publicly: atlas, Wisconsin arrival, landscape/current HUD, traditional farmland on West field, and undo. Queueing reserved $120,000 without spending cash; undo restored $10M available and zero reserved plan. Public onboarding reported that the previous-layout save remains retained. No error-level console logs. [Local capture](chrome-local.png), [public atlas](chrome-public-atlas.png), [public game](chrome-public-game.png), [public logs](chrome-public-logs.json). Both disposable tabs were closed after verification.

This refresh does not claim Phase 2A visual acceptance, final performance certification, a full public ten-year rehearsal or live AI verification. Public arrival overlapped anonymous file downloads and other work on the machine; recorded timing logs are not a controlled performance sample. The optional Gemini service remains local. Existing worktrees, unfinished visual candidates and the primary checkout's local Editor setting remain untouched.
