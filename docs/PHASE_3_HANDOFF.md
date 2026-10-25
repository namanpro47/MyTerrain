# Phase 3 editing handoff

The user clarified the October 24, 2026 goal: implement Phase 3 editing in a separate worktree alongside the other worker's Phase 2A visual rebuild. This overrides the implementation ordering for this isolated slice. Phase 2 was already complete; the 34 geography checks were rerun successfully before starting Phase 3.

Branch: `codex/phase3-editing`. Base: `08092a1f19ceb064ab21c11c6f97dd221693d410`. The original checkout and its live Unity instance were not edited. The worktree has a separate Unity Library and Editor; all CLI calls target its full path.

## Integrating with the visual rebuild

Retain the Phase 2A worker's scenery, assets, camera, lighting, and current source/credit presentation. The rendering and UI in this branch are inherited from the base checkpoint and are not a newly accepted visual target. Phase 3 changes are concentrated in these files:

- `Core/WorldStateStore.cs`: authoritative add/replace/move/remove/reset, undo/redo, full refund and destination repricing, stale revision rejection, and history isolation. No shared DTO or WorldPack changes.
- `Gameplay/TerraMain.Editing.cs`: selection, editing intent, command preview/commit, history, and minimal renderer synchronization. `TerraMain.cs` becomes partial, wires the HUD callbacks and keyboard shortcuts, and moves its old editing methods into this file. Keep the other worker's camera/render setup when resolving the main-file hunk.
- `UI/TerraHud.Editing.cs`: contextual move/remove/confirm controls, redo/clear controls, and managed-site vegetation accounting. `TerraHud.cs` becomes partial and makes room in the existing preview card; it shows net illustrative production and documents the added shortcuts. Adapt these hooks to the final Phase 2A layout while retaining the same command semantics.
- `Rendering/LandscapeRenderer.Editing.cs`: one cancelable removal preview, using the existing `ghostSiteId` / `baselines` / `proposals` lifecycle. No new art, asset family or shader. If the renderer changes those members, implement the same show-baseline/restore-proposal behavior in its new preview lifecycle.
- `Assets/WebGLTemplates/Terra/index.html`: accessible control description only.
- `Assets/Editor/Phase3Checks.cs`: real-pack transactions, eligibility, cost, area and history checks.

Source paths above are under `Assets/Terra/Scripts/` unless stated otherwise. Preserve `.meta` files. No scene, package manifest, global render setting, geography, or scenario balance change is needed.

## Verification after integration

Run the three editor checks (`Phase1Checks.Run()`, `Phase2Checks.Run()`, `Phase3Checks.Run()`) in the integrated checkout's own Unity instance. Rebuild with `TerraBuild.BuildWeb()`, then test in Chrome:

1. Place West field (17 units, 36 credits); replace with living margin (14/30); remove (6/60), undo and redo.
2. Move West field to Meadow edge: source baseline returns, current total becomes 18/36. One undo reverses the entire move.
3. Reject occupied, protected and ineligible move targets without changing either site. Reject overspend. Cancel moves and removals with Escape/right-click.
4. Clear all proposals, undo once to restore the whole design, redo, then branch history with a new edit.
5. Pan/zoom, open the guide, and use pointer/keyboard editing. Camera drags and modal input must not place. Check both target viewports, larger text and reduced motion.
6. Compare the actual integrated visuals with the latest **accepted** Phase 2A benchmark. This branch's editing evidence does not certify the separate visual gate or novice usability.

The runtime contract documents exact command semantics. Runoff remains unavailable, and mission/learning work remains outside this phase.
