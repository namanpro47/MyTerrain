# MyTerrain native Unity atlas — verification in progress

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

The final WebGL build and Chrome pass remain pending. The small final motion-preference synchronization and field-note sizing changes will be included in that build.
