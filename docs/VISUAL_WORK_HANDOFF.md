# Visual rebuild worktree handoff

The latest Chrome-verified visual checkpoint is commit `3684a40`, containing study19b. Its exact export was copied after hash verification into this worktree's `Builds/Web`. Local visual preview: http://127.0.0.1:49658/. This preview is separate from annual/editing integration. It now serves the exact verified study20 export (build job26ad6dcba3db4e80ab33aa302f289913).

Active visual work now lives at `/Users/naman/.codex/worktrees/terra-visual-rebuild/Terra`, branch `codex/visual-study20`. The primary checkout received gameplay merges during the visual run; do not reset or overwrite those changes. Drive only this worktree's own Editor for subsequent visual experiments, using explicit `--project-path`.

Keep: corrected displayed-pixel detail selection and shared per-frame camera parameters in `InstancedNature`; matching crop fade; `TerraMain.RenderView` updates `InstancedNature.DisplayPixelRatio` and shader `_TerraDisplayPixelRatio` from `hud.PixelRatio`. These are necessary rendering hooks when integrating another gameplay bootstrap. The existing illustrated camera/pan/zoom and creek geometry remain candidates.

Study19b: overview p95 19ms; mostly opening diagnostic p95 28ms; moving placement p95 41ms (fails). Cultivate/Restore/undo verified. One early-input startup null-reference exception, absent on the next passive reload, remains unresolved. Visual gate remains failed; no dimension certified at 4.5, no current public release. See CURRENT_VISUAL_REVIEW.md and the visual-rebuild checkpoint.

Study20 is Chrome verified, but not art/performance accepted: union route mask eliminates overlapping terrain-road strips; bridge decks retain explicit geometry with matching artist ground texture. The free Austinni bush was evaluated and rejected; imports/wrappers removed. Current candidate retains existing canopy while testing more readable lighting. No annual gameplay changes belong in this worktree. No image generation, purchases, new location or parity claim.

The original archive remains at `/Users/naman/Documents/GitHub/Terra/Builds/ArtResearch/AustinniForest/AustinniForest.unitypackage`. The initial primary-checkout import was removed after a simultaneous gameplay merge left the scene unable to load its pack. The isolated evaluation is recorded in study20; no bush selection or Chrome acceptance is claimed.

Study20 captures and runtime evidence are in the visual-rebuild ledger. Terrain-road seams removed, shadows more legible. Cultivate/Restore/undo pass;30s diagnostic p95=92ms fails with concurrent-build confound. No error-level messages on two loads, but19b early-input issue remains untriaged. Next: coherent planted bank/meadow surfaces, forest edge composition, treatment boundaries. No new art acceptance.
