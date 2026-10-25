# Phase 3 editing audit — October 24, 2026

The isolated Phase 3 implementation is complete and its editing correctness is verified. The user authorized this slice alongside the separate Phase 2A rebuild. This is not acceptance of the inherited visuals or browser frame rate, and the integrated presentation still needs the Phase 2A gate. No Phase 4 simulation or mission work was started.

Runtime checkpoint: `b91b568`, branch `codex/phase3-editing`, based on `08092a1`. [Build identity and source hashes](evidence/phase3/build.json) identify the tested export. [Integration handoff](PHASE_3_HANDOFF.md) describes the small presentation hooks to port into the other worker's rebuild.

## Requirements and observed results

| Phase 3 requirement | Implementation and verification |
|---|---|
| Authoritative transactions and recomputation | `WorldStateStore` evaluates the complete small set of sites; invalid, stale and no-op commands do not mutate state or history. Returned intervention arrays cannot mutate the store. |
| Configurable cultivation and restoration | Existing real-pack standard plot, living margin and meadow restoration offers remain data-driven. Existing productive use is displaced, not added twice. West cultivation gives 17 total units; moving the same proposal to previously nonproductive Meadow edge gives 18. Restoring West loses its original one unit. |
| Preview, commit, move, replace, remove, cancel | One command path computes previews and commits. Moves restore their source and reprice the destination atomically. Remove previews original cover; Escape/right-click restores the proposal. Removal and clear require explicit confirmation. |
| Budgets, cover and honest metrics | Full refunds, destination prices, managed-site vegetation hectares, total illustrative capacity and net change are displayed. Protected/occupied/ineligible targets and overspend reject without side effects. Runoff remains explicitly unavailable. |
| Visual lifecycle and input | Changed sites are rebuilt from authoritative state; ghost overlays clear before undo/redo/replacement; reduced motion skips settling. Selection survives camera movement. Middle click, Shift-click and drags no longer trigger placements. Guide controls capture editing keys. |
| History and repeated reset | Undo/redo handle each move or clear as one action. Valid edits discard abandoned redo history; invalid edits preserve it. Tests cover 25 complete history cycles and 272 valid real-site/treatment moves. |

## Evidence

- [1,758 transaction checks](evidence/phase3/transaction-checks.json), [27 Phase 1 regression checks](evidence/phase3/phase1-regression.json), [139 Phase 2 regression checks](evidence/phase3/phase2-regression.json): 1,924 Unity checks passed.
- [34 geography checks](evidence/phase3/geography-recheck.json) passed against the existing real data cache. Real and fixture pack preparation and the existing scalar simulation fixture also passed; this does not integrate runoff into the game.
- Unity 6000.6.6f1 Web build succeeded in 112.30 seconds, zero errors and five build warnings. Export: 12 files, 22,566,745 bytes. [Local asset verification](evidence/phase3/local-assets.json) passed.
- [Chrome action checklist and performance](evidence/phase3/browser-checks.json), [raw final console](evidence/phase3/final-browser-log.json). Actual pointer/keyboard actions exercised the final build at 1280×720 and 1440×900. No console errors; one inherited optional URP FSR shader warning remains.
- [Move preview](evidence/phase3/final-move-preview.jpg), [move redone](evidence/phase3/final-move-redone.jpg), [replacement preview](evidence/phase3/final-1280-replace-preview.jpg), [removal preview](evidence/phase3/final-1280-remove-preview.jpg), [occupied target](evidence/phase3/final-occupied-move.jpg), [protected target](evidence/phase3/final-protected-move.jpg), [ineligible target](evidence/phase3/final-ineligible-move.jpg), [budget rejection](evidence/phase3/final-budget-rejected.jpg), [modal isolation](evidence/phase3/final-modal-isolation.jpg), [history branch](evidence/phase3/final-history-branch.jpg).
- Larger-text/reduced-motion checks: [1280 help](evidence/phase3/final-large-help.jpg), [1280 clear](evidence/phase3/final-large-clear.jpg), [1440 clear](evidence/phase3/final-1440-clear.jpg). Their [help](evidence/phase3/large-help-audit.json), [clear](evidence/phase3/large-clear-audit.json) and [1440](evidence/phase3/final-1440-audit.json) audits contain no label height overflow. An initial long clear explanation did overlap; the final build uses shorter text and passed the retest. Initial captures are retained as diagnostic history.

## Reference and integration limits

Design patterns **TN-05** (anticipate transformation), **CS-03** (separate object selection from its tool), and **AX-01** (measure the rendered browser) informed the contextual controls and verification. The [persistent design reference](DESIGN_REFERENCE.md) and [art contract](art-direction.md) remain the integration reference. There is no accepted Phase 2A capture to claim parity with; the inherited scene is a rejected visual baseline. These screenshots certify editing behavior and layout only.

The 30-second final Chrome sample at 1440×900 measured **9.75 FPS**, p95 frame time **129 ms**, maximum **157 ms**, with zero unfocused frames. It does **not** meet the project frame-rate target. The sample included five preview events and one placement, no camera events; it is not a complete performance acceptance route. Maximum measured preview CPU was 1.2 ms and placement CPU 1.9 ms. Five DOM-input-to-next-presentation-opportunity samples ranged 82.5–101.9 ms; these are not physical display latency. The inherited scene and concurrent development workloads were not isolated, so this evidence does not establish a regression cause. Reprofile the integrated Phase 2A scene before claiming performance acceptance.

A separate human novice session was not performed. The agent exercised the game for several minutes without state corruption, but this does not substitute for human usability evidence. The preview runs locally at `http://127.0.0.1:8083/`; no public deployment or merge into the active Phase 2A checkout was performed.
