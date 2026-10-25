# Globe-first opening — October 25, 2026

Base: refreshed `origin/main` at `ce27355f6bd7b1c55e1c53c2db00b1b748ccf926`, isolated `codex/globe-first-loading` worktree. The primary checkout's Pipeline settings edit is untouched.

The user rejected the empty centered loading screen and requested immediate world-page scenery. The browser now renders a lightweight native-globe preview, the same licensed display font and a restrained progress line before Unity loads. No minimum dwell, confirmation or extra click. The preview stays until both native atlas readiness and the Unity instance resolve, followed by two animation-frame callbacks so Awake cannot expose an unpainted canvas. Download/startup errors retain the scenery and expose keyboard-accessible retry. Native controls only become focusable at handoff. Reduced motion disables the progress shimmer.

Patterns: **TN-01 / TN-03 / TN-06 / AX-01** — scenery dominates, minimal chrome, continuity into play, responsive readable status. Compared with the user's two screenshots and `../myterrain-combined-release-oct25/chrome-selector.png`. Globe image is a crop of that real native render, 41,772 bytes; Lilita One is the existing unmodified 27 KB licensed font. No generated art or external font/CDN dependency. Provenance/crop recipe is in `Assets/Terra/Resources/Atlas/CREDITS.md`.

## Verification

- `node --check Assets/WebGLTemplates/Terra/player.js` passes.
- `node tests/web/bootstrap.cjs` passes: both readiness orders, paint boundary, loader failure, runtime rejection and error during queued handoff. These are transport mocks, not runtime acceptance.
- Actual Chrome, local port 49176: updated template and transport with the unchanged published Unity export from runtime source `19e7eae`. Native atlas readiness then `MYTERRAIN_WORLD_VISIBLE` at 16,291 ms on the first local run. This is one observation, not a network/performance benchmark or a speedup claim.
- Actual keyboard selection and entry reaches `MYTERRAIN_ARRIVAL: point-reyes-v1`; no error-level messages on the normal player tab. Existing informational audio/stripped shader messages remain.
- `chrome-ready.png`: actual interactive Unity selector after handoff.
- `chrome-loading.png`, `chrome-loading-compact.png`, `chrome-loading-narrow.png`: actual Chrome at 1728×940, 1024×576 and 390×844. To keep the short-lived opening inspectable, these layout captures use a local-only copy of the exact template with player.js omitted. The live player was separately observed showing this same preview at download progress 90% before handoff. The local fixture is not shipped.
- `chrome-retry.png`: actual missing-loader failure in a local-only fixture; recovery replaces the missing-loader configuration with the real player before pressing Try again. Viewport overrides reset after testing.

Only the Web template changes. Unity payload, download size, scene/selector logic and annual state are unchanged; no Unity rebuild or public deployment is claimed. Static preview does not rotate until the runtime is ready. This does not accept the separate Phase 2A landscape gate. The next Web export copies the template assets, including globe preview and font.
