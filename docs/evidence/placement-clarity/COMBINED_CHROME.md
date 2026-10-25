# Combined Chrome placement walkthrough

Build: `770d4c05b7f50f366275c036cd769a0c91414644`, Unity 6000.6.6f1. Frozen export copied from the combined release owner; all 21 file hashes verified in `combined-export.json`. Owned localhost origin: port 49881. Chrome viewport: 1728 × 940. Desktop reservation96, October25,2026. No new export performed here. This export predates the requested simplified globe and the two overlap fixes below.

## Observed results

- Arrived in Wisconsin, dismissed introductory Field Notes. Neutral Browse, no armed tool, available budget $10M and starting outcome values visible.
- Selected Public park. Tab selected protected woodland. BLOCKED and protected-area reason remained visible alongside the selected park card. Enter did not change revision0, cash1000000000 cents, queued0 or assets0.
- Tab selected an eligible plot; F focused it. Preview used a dashed 48 ×40m footprint within a separate reserved-field outline. Local Reserve$70,000 and inspector $50,000 upfront +$2,000/year ×10 were consistent. Preview left revision0 and cash unchanged.
- Enter queued one park; available-to-plan became$9.93M, queued label persisted in the world, selected tool cleared. Inspector offered Move/Remove. Headline accepted values stayed fixed with labelled forecast delta beneath.
- V began move and retained selected park identity; Tab reached a protected destination. Existing queued source remained marked. Escape canceled.
- Clicked Undo, then Redo. F10 readouts: revision2 queued0 and revision3 queued1; both cash1000000000 and assets0. Reservation is separate from committed cash.

Actual captures: `game-browse-1728.png`, `game-blocked-1728.png`, `game-preview-1728.png`, `game-queued-1728.png`, `game-move-blocked-1728.png`, `game-undo-1728.png`, `game-redo-1728.png`. Readouts: `combined-browser-log.json`.

## Defects found and handed off

1. Cards cover six pixels of the dock instruction line at standard/compact sizes. `56e1940` adds12px dock height and asserts actual card/instruction world-bounds clearance at every existing native layout and text size. Native rerun and rebuilt Chrome confirmation pending.
2. Generic camera/action toasts can cover the placement capsule fallback. `9b5e14a` gives candidate guidance priority in both display orderings and adds native assertions. It preserves action receipt suppression and unrelated score formatting. Native rerun and rebuilt Chrome confirmation pending.
3. Opening framing exposes a large pale area beyond the map right edge. P4 world presentation remains unaccepted; do not certify the scene based on the functional checks. The independent scenery work continues.

This bounded walkthrough is not the full acceptance matrix. Actual successful move, replacement, remove, whole-field/swale, responsive sizes,125% zoom, accessibility and quiet timing remain to be verified. The single existing stripped EASU shader warning was observed on load; this is not proof of a new placement failure. Human comprehension and broader Phase2A remain open.
