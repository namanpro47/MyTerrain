# Branch integration — October 24, 2026

User request: merge the delivered branches and resolve conflicts. This is integration and regression verification, not execution of the remaining annual roadmap.

## Inputs and preservation

- `origin/main` at `e247431` (initial delivery; supplemental merge through `514d312` below): annual state/public ledger, annual Editor playtest, 17 metric action assets and categorized placement UI. `origin/codex/annual-town-state` is an ancestor.
- `codex/annual-gameplay-integration` at `ee7e0ef`: prepared main + Phase 3/4 integration.
- `codex/phase4-consequences` at `5e9621b`: completed runoff delivery with Phase 3 carried as `2f74c40` / `d17925d`.
- `origin/codex/phase3-editing` at `c49a3c2`: original delivered editing branch; its history was merged after verifying the carried patches with `git range-diff`.
- The annual integration worker subsequently reported `7b21ddd` / `fee1f68` with a new annual player loop, explicitly **not a ready handoff** while Web/Chrome verification continues. Those later, in-progress commits are excluded from this merge.
- B/D/C3 and `codex/parallel-package-integration` were already in the primary history.
- `codex/phase3-review-study` is an abandoned isolated review experiment and is superseded by the completed Phase 3 delivery; deliberately excluded.
- Primary uncommitted visual studies, free assets, latest annual spec/plan and existing evidence were saved in `3684a40`. Prepared integration merged as `d74c8d3`; original Phase 3 ancestry merged as `6add1df` without reverting later changes. Other worktrees were read only. Concurrent new visual-study files and Pipeline configuration changes were left outside follow-up staging.

## Resolutions

Retained the current annual product direction, twelve-action contract and adoption note, together with delivered module documentation and historical verification receipts. Retained current terrain/canopy shaders, display-pixel ratio, tree attribution, catalog UI, reversible editing, and runoff connections.

The prepared integration had dropped invocation of the placement validator and the runoff HUD initialization/refresh calls. Restored those seams, including recoverable loading errors. Added callback add/replace/move regressions and catalog-plus-runoff conservation checks. Updated help and accessible descriptions to include editing commands and describe the integrated storm estimate. Chrome exposed the catalog's 320px preview conflicting with the runoff layout; restored the 374px editing/consequences panel and shortened its cost line. Added annual contracts to the consequences test project because its Core wildcard now compiles annual sources too.

## Verification

- Unity CLI beta.14 (latest reported); Unity 6000.6.6f1; final script recompile: zero errors/warnings.
- [Unity invocation](unity-run.json): **3,073 assertions** across [Phase 1](phase1.json) (27), [Phase 2](phase2.json) (139), [editing](phase3.json) (1,763), [runoff](phase4.json) (98), [annual](annual.json) (94), and [catalog with runoff](catalog.json) (952).
- [Standalone checks](standalone.json): 1,372 consequences assertions and 94 annual assertions. Annual project targets .NET 8; this host has .NET 10, so the run explicitly used `DOTNET_ROLL_FORWARD=Major`.
- [B/D/C3 verification](parallel-checks.json): simulation, Unity API compatibility, frozen scalar example, education schema/tests and audio audit all pass.
- `.venv-worldgen/bin/python scripts/verify-hydrology-geometry.py`: 1,711 disjoint source units, 1,486,800 m², no gaps or outside area; overlap within 0.1 m² tolerance. System Python lacks Shapely, so the project's existing GIS environment was used.
- Original evidence written by legacy test entry points was restored after saving these new receipts.

## Browser and delivery

Final [Web build](final-build-summary.json) succeeded in **240.39 s**, **0 errors / 8 warnings** (obsolete lookup, unused Phase 0 field, TMP splitting and Web toolchain warnings). [All 12 local assets](final-local-assets.json) returned HTTP 200 and matched export bytes, totaling **36,688,161 bytes**. The BuildReport's `totalSize` field reports only 973,040 bytes; use the actual verified export file sum for transfer size. Runtime/editor source hashes remained unchanged between final build and verification.

Chrome 155 at a **1728 × 940** CSS viewport rendered the retained study19b landscape with the merged category dock, history controls and runoff totals. [Initial browser log](initial-browser-log.json): placement 6/60 → 17/36, exact undo/redo; keyboard B/Right/Enter opens Residential; Utilities shows all seven actions; ineligible swale gives a restriction; Meadow edge swale spends 28 credits, retains 8 m³ and changes discharge 9701.926 → 9647.406 m³, with undo restoring baseline. The initial [overlapping preview](preview-before-fix.png) was fixed by restoring panel height and shortening the catalog summary. [Final preview](final-preview.png) has separated rows and a readable Place button. [Final browser log](final-browser-log.json) verifies fresh initialization, placement, undo and redo after the fix, with **no console errors**. Existing optional FSR shader/Content-Length warnings remain. Final readiness was 13.641 s on this busy machine; this is not a controlled performance benchmark. Other viewport sizes and the full annual browser loop were not retested.

The first build completed despite Pipeline HTTP-service errors during a timed-out status query; the final build reported zero errors. No public deployment is part of this request. The primary branch and `main` receive the source merge through a normal fast-forward push; ongoing unverified annual UI commits remain on their worker branch.

## Product limits and design reference

The annual session is present as a module and Editor playtest. The player-facing annual queue, USD budget/meters and End Year/report loop still need their separately scoped integration. The live scene continues prototype credit accounting. This merge does not claim Phase 3A/3B or full catalog economic effects are complete.

TN-01/03/04/05, CS-02/04 and AX-01 govern retained scenery, catalog, previews and feedback. Study19b is the latest pre-merge visual evidence, a rejected baseline rather than an accepted Phase 2A benchmark. Browser smoke testing can establish merge compatibility; it cannot establish Terra Nil visual parity, performance acceptance or novice usability.


## Published work arriving during verification

The final pre-push fetch found `origin/main` advanced to `514d312` via `a82c74f`: annual chapter contracts/session, offline chapters, a local optional Gemini narration adapter and ignored dotenv loading. Merged these published commits as well. Only documentation conflicted; retained the current annual product direction and appended their bounded module handoff. No provider credentials were read and no real provider request was sent. This is distinct from the unverified annual UI branch mentioned above.

The Web export/Chrome evidence above is pinned to `49f86e9`, before this supplemental chapter merge. The chapter additions do not modify TerraMain, TerraHud, scenery or the enabled scene. They receive fresh Unity compilation and chapter/annual/consequences tests; no second browser-build claim is made for them.
