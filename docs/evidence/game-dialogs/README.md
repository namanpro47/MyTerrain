# Game dialog redesign — October 25, 2026

Base: refreshed `origin/main` at `f4bd323506a8c9a904c784703de5ab664591ff17`, isolated `codex/game-overlay-design` worktree. The primary checkout's uncommitted Pipeline configuration is untouched.

## Design intent

The user rejected the white document-like arrival panel and similar screens. TN-03/06/08, CS-01/04 and AX-01 guide this bounded native uGUI change. The approved play-first HUD remains the surrounding visual reference; the supplied arrival screenshot is the rejected dialog baseline.

- Dark slate dialog surfaces match the existing landscape HUD. A lighter backdrop leaves the actual world visible. Mint primary actions, a quiet corner close control and separate heading/body establish hierarchy.
- Arrival uses live definition values for funds, homes and jobs, a short play loop, and one prominent Explore action. Scenario details and explicit ending targets remain available in Your brief & goals.
- The same readable, content-sized surface serves help, sources, annual confirmation, project details, recovery, comparisons and stories. Long content scrolls at full text size; short messages no longer occupy a fixed 650-pixel sheet. Page Up/Down, wheel and visible scroll controls are supported.
- Story titles lead the page; year, narrative source and page number move to the eyebrow. Meter colors match the HUD.

No prices, simulation, ledger authority, save keys, geography, renderer, provider behavior or scene assets are changed. No generated art or purchased assets. This does not certify Phase 2A scenery or publish a deployment.

## Verification

`unity run <worktree> -- -executeMethod MainGameChapterChecks.BuildVerifiedWeb -buildTarget WebGL` passes **889 main HUD/session checks**, plus the existing chapter suite. Checks cover preserved accepted state, all proposal costs, readable story rows, correct scroll origin, primary action callback, source labeling, opening facts and long generic dialogs.

The initial check exposed the old fixed-height assumption that every chapter must overflow. The new content-sized surface permits short pages to fit; verification now checks top alignment when content actually overflows. Scroll origins are explicitly reset after inserting meter rows.

Actual Chrome verification and final source/export identity are recorded below after the build completes.
