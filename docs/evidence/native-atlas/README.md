# MyTerrain native Unity atlas — verified October 25, 2026

User correction: the earlier HTML landing page was rejected. All globe rendering, location selection, controls and descent now belong to Unity. Browser HTML is only a canvas loader. This directory records the replacement; historical evidence under `../atlas` describes the rejected browser implementation.

Patterns: TN-01/02/03/06, AX-01. Verify the full globe at 1440×900 and 1280×720, all three location pins, drag/keyboard selection, smooth descent, cancellation, same-location resume, cross-location restoration, reduced motion, and MyTerrain branding. The broader Phase 2A landscape-quality gate is separate.

Editor and package checks:

- Native C# compilation passed; first live Editor frame counter advanced.
- Found and repaired globe mesh winding and longitude orientation during rendered review.
- 33 package/source integrity checks pass (`python3 scripts/verify-atlas.py`).
- All three WorldPacks packaged with checked hashes.
- Browser player JavaScript passes `node --check`.

- All three native journeys reached their correct scene and Year 1 annual state.
- Same-location resume preserved the live scene entity; a replay-style scene reload restored the selected location and completed entry.
- Live checks passed for one persistent EventSystem, application branding, west/east orientation, sphere normals, pointer drag and travel cancellation. [Receipt](editor-lifecycle.json), [repeatable live checks](verify-live.cs).
- No Editor runtime errors or warnings after the three-location pass. [Console receipt](editor-errors.json).
- Actual Editor captures: [Wisconsin globe](editor-globe-wisconsin.png), [Asia globe](editor-globe-asia.png), [1280×720](editor-globe-1280.png), arrivals in [Wisconsin](editor-wisconsin-arrival.png), [Point Reyes](editor-point-reyes-arrival.png), [Suusamyr](editor-suusamyr-arrival.png).

## Frozen export and Chrome verification

Source candidate **af925de**, Unity **6000.6.6f1**, successful WebGL export (0 errors, 7 build warnings), 20 shipped files / 40,143,580 bytes. [Build result](build-result.json), [report](build-report.json), [source hashes](build-source.json), [export hashes](export-files.json). Every served file matched the frozen export after fault recovery: [HTTP receipt](served-files.json). No legacy atlas directory remains; HTML contains a single Unity canvas plus the startup loader.

Chrome at **1280×720** and **1440×900**:

- Full native globe, atmospheric rim, correct geographic orientation, pins and native location cards reviewed. [1280 capture](chrome-globe-1280.png), [1440 Asia capture](chrome-globe-1440.png), [final MyTerrain capture](chrome-myterrain-final.png).
- Pointer drag visibly rotates with inertia; scroll zooms; the Wisconsin pin selects its place; native Focus recentres rotation. Keyboard selection and Enter travel work. A started flight cancelled with Escape returns to the globe.
- Native orbit/approach, loading veil and descending gameplay camera reached Wisconsin, Point Reyes and Suusamyr. [Wisconsin](chrome-wisconsin-arrival.png), [Point Reyes](chrome-upgraded-save.png), [Suusamyr](chrome-suusamyr-arrival.png).
- World atlas button and Shift+Escape return to selection. Point Reyes retained its plan after travel through both other regions.
- A deliberately removed local Suusamyr manifest produced a native error/retry panel. Restoring the exact file and clicking Try again reached Suusamyr. [Expected error capture](chrome-load-recovery.png). The one `TERRA_PACK_ERROR` in the console is this deliberate fault, not an unexplained failure.
- Field notes remain readable and block Enter/number-key background actions; Escape closes them. [Modal capture](chrome-field-notes.png).
- Reduced motion skips the sustained orbit/descent, remains set after a browser reload, and was reset to full motion at the end.
- MyTerrain appears in the native selector, in-game field notes, startup loader and browser title. Internal Terra identifiers remain intentionally compatible.

## Save upgrade

On the same private local origin, first ran the prior HTML-selector export and queued Traditional farmland at Point Reyes. Captured Year 1 / Planning / revision 1 / one queued proposal / $10,000,000 cash / $120,000 plan / $9,880,000 available. Replaced the served files with the frozen native MyTerrain export, reloaded, and entered Point Reyes through Unity. The same state restored, and remained intact after visiting Wisconsin and Suusamyr. [Before](chrome-legacy-save.png), [after](chrome-upgraded-save.png), [console receipts](chrome-console.json). This verifies the shipped WebGL save upgrade; it does not claim migration of standalone desktop save folders.

## Limits and cleanup

The existing URP FSR-upscaling shader warning also occurs in the old export and is recorded in the console; no unexpected runtime errors occurred in the native pass. Unity build warnings include TMP code-generation size notices and toolchain warnings. This was functional/visual verification while another isolated build could run, not a quiet-machine performance benchmark. The earlier HTML atlas is a rejected baseline; the comparison here confirms its replacement by a real Unity scene, with TN-01/03/06 and AX-01 expressed in the actual player. The broader landscape Phase 2A gate remains open.

Own Editor stopped/closed, Chrome tab closed, viewport override reset and desktop reservation released. Only the local preview server remains available. No public deployment was performed.
