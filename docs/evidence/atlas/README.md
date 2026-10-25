# Living Atlas verification — October 25, 2026

Source: initial fresh base `e0c8877`, atlas implementation `cf8c823`, reconciled main through `8cfb3ad` in merge `8c3c128`. Final browser-wrapper/evidence follow-up is recorded in Git. No primary checkout work was overwritten and no public deployment was performed.

## Scope and visual assessment

The new entry is a rendered, interactive Earth with a quiet ivory/sage layout, original illustrated cards and a short camera descent. **TN-01 / TN-02 / TN-03 / TN-06 / AX-01** guided the whole-globe focus, palette, sparse controls, short invitation and keyboard/motion behavior. The new atlas meets this feature's visual target: coherent lighting and cartography, readable destination hierarchy, stable marker positions, no UI collisions and complete destination choices at 1280×720. It is not an acceptance of the existing in-game scenery. The rejected Phase 2A captures remain historical evidence, not the atlas's visual target.

The browser opens three real rural places. Wisconsin keeps its original source identity and save keys. Point Reyes and Suusamyr contain distinct sourced elevation, explicitly authored cover/sites, the shared educational USD rules and isolated saves. They are not surveyed parcels or localized economic forecasts. [Sources and design](../../ATLAS_SELECTION.md).

## Engineering evidence

- `unity-checks.txt`: **675 assertions**, all three packs, all 17 action variants, ten-year operation, exact cash/history replay, runoff conservation and rejection of cross-location saves.
- `scripts/verify-atlas.py`: **41 package checks**, terrain distinction, checksums, complete area partitions and bundled assets.
- JavaScript syntax checks pass. The existing chapter service suite passes **42 checks** using the installed .NET 10 runtime with `DOTNET_ROLL_FORWARD=Major`; no paid model request was made.
- Final WebGL build succeeded in **488.914 seconds** (`build.json`). The last browser-only CSS and Shift+Escape changes were copied from the template with the existing hashed player filenames; no compiled player input changed. `web-assets.json` verifies **30 served files / 45,004,955 bytes** against the exact export, including all three packs. The browser runs the integrated export at `http://127.0.0.1:49174/`.

## Chrome interaction checks

The first integrated build established all three runtime routes. Point Reyes accepted a farmland proposal after undo/redo, submitted Year 1 through the explicit offline chapter and restored exactly **$10,898,000** cash with **$18,000** obligations after reload. Suusamyr began independently at **$10,000,000**, accepted a proposal with **$120,000** reserved, and preserved it when returning. Wisconsin retained its original landscape and independent Year 1 state.

A return-to-atlas test exposed a ResizeObserver delivery loop. Globe buffer resizing now happens in the next animation frame, outside observer delivery; repeated return/entry and cancellation then ran without that error. The World atlas control was moved beside the identity panel after checking both large and 1280×720 game layouts. Initial-load cancellation was checked again after Unity finished in the background: card selection and arrow keys remained with the atlas. Reduced motion omits the camera flight; the choice persists.

Desktop, compact and phone layouts were inspected in Chrome. Exact-size iframe viewports are used only as local verification fixtures; their controls are excluded from captures. The phone check covers the atlas and its destination cards, not full mobile gameplay acceptance. Drag, zoom, reset, pins, card arrows, source dialog, cinematic entry, Escape cancellation, same-location resume and cross-location runtime replacement were exercised. Final-build entry and return succeeded for all three destinations. Point Reyes restored its accepted ledger and Suusamyr restored its pending proposal. Shift+Escape returns keyboard control to the atlas. `final-browser-console.json` contains no error-level entries; its three warnings are the inherited optional URP shader warning, one per region. Final captures document this export.

- [Finished desktop atlas](chrome-atlas.png) and [Central Asia selection](chrome-suusamyr-atlas.png)
- [1280×720 atlas](chrome-1280.png), [phone opening](chrome-phone.png), [phone choices](chrome-phone-cards.png)
- [Wisconsin runtime](chrome-wisconsin-game.png), [Point Reyes restored ledger](chrome-point-reyes-restored.png), [Suusamyr restored proposal](chrome-suusamyr-game.png)
- [Camera-flight frame](chrome-flight.png)

## Limits

Local static hosting has no `/api/chapters` service, so manual submission deliberately offers labelled offline completion; this is the existing supported path. The optional stripped URP upscaling warning is inherited and does not prevent the game from rendering. Broader scenery acceptance, device performance claims, novice playtesting and public deployment remain separate work.
