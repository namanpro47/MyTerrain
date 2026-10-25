# Phase 3 — editable-world foundation

The user explicitly authorized Phase 3 alongside the ongoing Phase 2A visual rebuild on October 24, 2026. This is an exception to the sequencing gate, not acceptance of the rejected visuals. This work uses the same Meadowbrook Valley pack and leaves Phase 4 runoff integration, mission objectives and expansion outside scope.

## Editing contract

`WorldStateStore.Preview` and `Commit` share validation and full recomputation over the canonical site list. Commands carry a revision. Invalid, stale and no-op commands preserve state and both histories. Returned results and intervention arrays do not expose mutable store/history state.

- **Add:** requires an empty compatible site and sufficient credits.
- **Replace:** requires an existing proposal and a different compatible treatment; refunds its full old cost and charges the site's new offer.
- **Move:** preserves the source treatment, restores the source baseline and applies the destination's offer/area in one transaction. Occupied, protected, incompatible and unaffordable destinations fail without removing the source. No implicit swap or overwrite.
- **Remove:** restores the site's original cover and illustrative productive capacity; refunds its full cost. It does not turn the original land into empty land.
- **Reset proposals:** restores every baseline in one undoable transaction; camera reset remains a separate control.
- **Undo / redo:** snapshot history restores all accounting. Revisions remain monotonic across navigation. A successful new edit clears redo; a rejection does not.

Production is the sum of each site's **final** production, including unchanged baseline use. The existing v1 `SiteOffer.production` stores final illustrative units per treatment/site. Its effective units per productive square metre are `production / (analysisAreaM2 * treatment.productiveFraction)` for productive treatments; this is configurable fictional accounting, not measured agricultural productivity. No schema or balance change is needed. A living margin takes 25% of a site's productive area; restoration removes existing productive capacity. The HUD reports total production, net baseline change, current site cover and preview vegetation change. Site-cover totals include the protected sites in the canonical site list; they do not claim to measure all vegetation in the surrounding AOI.

Costs are abstract credits. Moves and replacements show the net credit charge/refund. Runoff remains visibly unavailable until Phase 4 despite package B being present in the repository.

## Interaction and rendering

TN-05 and CS-03 guide the stable inspected site, contextual preview and editing controls. TN-03 / CS-01 keep measures and tools in their existing groups. TN-02 guides visible plot/margin/meadow differences; TN-07 retains grounded placement reveals. AX-01 guides readable controls and larger-text checks.

Use 1 / 2 / 3 for treatments, M for living margin, Tab to inspect, Enter or the preview button to confirm, V for move, X for remove, Z for undo, Y or Shift+Z for redo. Move and remove also have contextual buttons. Escape or right-click cancels. Choose another treatment to replace an occupied site. Reset proposals has a visible button and can itself be undone. No confirmation modal is needed for a reversible reset.

Pointer travel into the preview preserves its site, allowing its buttons to be clicked. Pan gestures cannot place, and completing a pan restores the current preview. The selected site survives camera movement. Modal input stays captured.

Rendering reads `visualRecipeId` from the configured treatment. Only changed sites are rebuilt on commit/history navigation. A move preview shows the original source and proposed destination together; cancellation restores both. The new `LandscapeRenderer.Editing.cs` partial adapter owns only preview visibility and calls the existing renderer; it does not change scenery, materials, camera or asset recipes. Removing/replacing/undoing destroys the old proposal through the established renderer lifecycle, interrupting its reveal safely.

## Verification

Implementation checks and actual Chrome evidence are recorded in [the evidence folder](evidence/phase3/README.md). Visual quality remains owned by Phase 2A. Historical Phase 2 captures and rejected visual studies are comparison context, not a Phase 3 claim of visual acceptance.
