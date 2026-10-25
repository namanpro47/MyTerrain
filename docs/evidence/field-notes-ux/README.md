# Field Notes usability — October 25, 2026

Requested scope: simple itemized Pay now arithmetic, no unlocatable lot names in notes, one proposal review, narrative offline chapters with graphical score changes, readable third chapter page, and a plain End Year landscape button.

Started from freshly fetched `origin/main` **13a15a43c43bab9e0eb6a2a90bded4bb4d7aba0c** in an isolated worktree. Original checkout's modified Unity editor config is untouched. The annual guidance owner explicitly released `TerraHud.Annual.cs`; incoming guidance must be preserved during reconciliation. Shared editor/build/desktop use follows `docs/WORK_COORDINATION.md`.

## Implementation

The budget lists every queued project, grouping repeated project titles and quantities, then adds actual current-year running/program/repair/arrears payments. Public tax receipts and annual funding appear separately, with the opening-cash arithmetic and expected closing cash. Future reservations are labelled separately. Integer-cent ledger values remain authoritative; no finance or simulation rules change.

Offline stories are a read-only presentation of accepted reports, including existing saves. Three short narrative cards cover the public budget, community life and land/conditions. Economic/social/environmental changes use paired Before/Now progress bars on the relevant page; raw score-delta sentences and output-multiplier jargon are absent from offline prose. Provider-written text and saved receipts remain unchanged.

The cream Field Notes surface keeps its existing controls and visual language. It reflows at actual screen size, uses 20px body text (22.4px in larger-text mode), clips overflow into a scrollable reading viewport and retains visible wheel/keyboard-reachable scroll controls. The main End Year caption is constant while planning, regardless of queue count.

References: **TN-06/08** for short contextual reports, **CS-01** for a single clear financial hierarchy, **AX-01** for readable text and smaller-window checks. Compared with the historical third-page capture at `docs/evidence/deployment-annual-2026-10-24/chrome-chapter.png` and the accepted play-first interaction evidence. This is a bounded UI revision, not scenery/Phase 2A acceptance.

## Verification

- Pure full-game regression: **2,399 assertions**, all 17 variants and 51 strategy/order routes. [Receipt](core-checks.json).
- New narrative: **102 assertions** over 20 accepted years across two strategies; schema-valid text, no score prose/lot names, developed paragraphs and unchanged saved reports. [Receipt](narrative-checks.json), [check source](narrative-checks.cs).
- Unity/browser verification is pending. Do not treat this source checkpoint as a rendered acceptance result.
