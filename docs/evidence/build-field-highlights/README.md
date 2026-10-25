# Build field availability — October 24, 2026

Base: refreshed `origin/main` at `589b655e4093c191ca08debeee75ef50a5c67ead`; isolated branch `codex/build-field-highlights`.

Before delivery, rebased without conflicts onto `d0d502a` to preserve the incoming loading-recovery fix and tree-audit work. The combined runtime C# compile passed. The local WebGL/Chrome evidence below is from the original base plus this feature; that export does not include the subsequent unrelated loading/audit commits.

Selecting a build action outlines every currently valid destination in bright teal, with a matching count and instruction in the catalog. Hover retains yellow valid / red blocked feedback. Availability uses the same `TerraMain.Rejection` path as confirmation: physical fit, protected zones, accepted projects, queue replacement/move rules, annual phase and full-term affordability all apply. A zero-result selection explains that the player can inspect a field for its rejection reason.

Eligibility is cached by run, revision, phase, tool and edit mode, so pointer/camera changes do not repeatedly calculate every destination. Existing outline geometry/material caching is reused; widths scale with the camera. Cancellation, committed edits, undo/redo, read-only comparison and annual transitions clear availability. Fast-forward clears it before advancing.

Reference patterns: TN-05 (anticipate placement), CS-02 (show suitability on the land), AX-01 (contrasting backed outlines plus a text count). This is a focused interaction improvement, not Phase 2A visual acceptance.

Verification:

- Production runtime C# compiled against installed Unity 6000.6.6f1 assemblies; only the existing `FindFirstObjectByType` deprecation warning.
- Existing full-game harness: 608 assertions passed, including all 17 variants and both ten-year strategies. Used Unity's bundled .NET SDK because `dotnet` on PATH has no SDK.
- `git diff --check` passed for the implementation.
- Unity 6000.6.6f1 WebGL export succeeded (batch process exit 0). Loader: `d793cbab659f54ebe5831630bc96928b.loader.js`; log: local ignored `Logs/field-highlights-build.log`.
- Chrome local preview at `http://127.0.0.1:8791`: visually inspected broad farmland highlights, switching to exactly two swale destinations, Escape clearing availability, placement clearing the selected tool, reselecting swale excluding the queued site, and undo restoring the empty plan / $10M available. No browser errors in the inspected console log.
- Compared against the historical `../full-game/chrome-compact-125.png` capture. The checked viewport was 858 x 925 using the existing compact HUD; bright outlines stayed readable at close and wide map zoom. The compact HUD already hides catalog summaries, so the new summary count was compile-checked but not visually verified in the wider HUD. Existing narrow-viewport header overlap is outside this change. No broad responsive-layout or performance acceptance is claimed.
