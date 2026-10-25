# User-supplied loading artwork — October 25, 2026

Follow-up to the globe-first bootstrap. Refreshed remote base `1330dc966267ba68f0e4f13dca7a0ec67a4e464a`; continuing isolated worktree `codex/globe-first-loading`. The user explicitly selected their attached MyTerrain / “Shape your world.” illustration for the true loading screen.

The supplied 1536×1024 PNG (2,939,463 bytes) is exported to `Assets/WebGLTemplates/Terra/loading-landscape.webp` at original dimensions, WebP quality 88. It fills wide screens with the title anchored at the top; narrower aspect ratios preserve the entire artwork over a dim blurred backdrop using the same cached image. A bottom shade keeps the small progress/retry controls readable. No duplicate visible wordmark, new generated image, new screen, added minimum wait or runtime changes. The prior preview and browser-only font copy are removed; the native atlas and its font remain intact.

Patterns **TN-01 / TN-03 / AX-01**: scenery dominates, compact status, readable progress. The user's selected attachment is the direct visual reference for this revision. This does not certify the illustration as actual gameplay or accept Phase 2A scenery.

Verification: `node tests/web/bootstrap.cjs` and `git diff --check`; Chrome at 1728×940 and 390×844. `chrome-wide.png` and `chrome-narrow.png` show a local copy of the shipping HTML with player.js omitted solely to hold the loading composition for inspection. The unchanged production engine export (`19e7eae`) is separately exercised with the updated HTML and original bootstrap for the transition into the interactive globe. No Unity rebuild or public deployment is claimed. Browser viewport override reset.
