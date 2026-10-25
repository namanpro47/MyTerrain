# Visual rebuild worktree handoff

The committed study20 checkpoint is `4d4dff8`, based on study19b `3684a40`. Current candidate21d is Chrome verified, with source/evidence in this worktree; no art or performance acceptance. Local preview http://127.0.0.1:49658/ serves exact study21d export `b8d6811bc45a4235aeb5c43fc6c6dde6`. This remains separate from annual integration.

Active visual work now lives at `/Users/naman/.codex/worktrees/terra-visual-rebuild/Terra`, branch `codex/visual-study20`. The primary checkout received gameplay merges during the visual run; do not reset or overwrite those changes. Drive only this worktree's own Editor for subsequent visual experiments, using explicit `--project-path`.

Keep: corrected displayed-pixel detail selection and shared per-frame camera parameters in `InstancedNature`; matching crop fade; `TerraMain.RenderView` updates `InstancedNature.DisplayPixelRatio` and shader `_TerraDisplayPixelRatio` from `hud.PixelRatio`. These are necessary rendering hooks when integrating another gameplay bootstrap. The existing illustrated camera/pan/zoom and creek geometry remain candidates.

Study19b: overview p95 19ms; mostly opening diagnostic p95 28ms; moving placement p95 41ms (fails). Cultivate/Restore/undo verified. One early-input startup null-reference exception, absent on the next passive reload, remains unresolved. Visual gate remains failed; no dimension certified at 4.5, no current public release. See CURRENT_VISUAL_REVIEW.md and the visual-rebuild checkpoint.

Study20 is Chrome verified, but not art/performance accepted: union route mask eliminates overlapping terrain-road strips; bridge decks retain explicit geometry with matching artist ground texture. The free Austinni bush was evaluated and rejected; imports/wrappers removed. Current candidate retains existing canopy while testing more readable lighting. No annual gameplay changes belong in this worktree. No image generation, purchases, new location or parity claim.

The original archive remains at `/Users/naman/Documents/GitHub/Terra/Builds/ArtResearch/AustinniForest/AustinniForest.unitypackage`. The initial primary-checkout import was removed after a simultaneous gameplay merge left the scene unable to load its pack. The isolated evaluation is recorded in study20; no bush selection or Chrome acceptance is claimed.

Study20 captures and runtime evidence are in the visual-rebuild ledger. Terrain-road seams removed, shadows more legible. Cultivate/Restore/undo pass;30s diagnostic p95=92ms fails with concurrent-build confound. No error-level messages on two loads, but19b early-input issue remains untriaged. Next: coherent planted bank/meadow surfaces, forest edge composition, treatment boundaries. No new art acceptance.


Study21d integration: `LandscapeRenderer.Channels.cs` adds illustrative low bank masses; `InstancedNature.cs` supports the dedicated groundcover material and distance submission cutoff. Include `Resources/TerraGroundcover.shader` and **`Resources/Nature/Groundcover.mat` with instancing enabled** plus their metas. Runtime shader swapping alone caused stripped instancing in21b; the serialized material corrected it in21c/21d. Keep the shared camera/display-ratio hooks above. Do not remove the serialized material as unused. Existing artist mesh/texture files are unchanged.

Study21d: Restore/undo verified, p95=48ms fails at3440×1352 CSS/buffer; no error-level messages returned. Captures and failures retained in ledger. Next change canopy/woodland composition materially; bank-only refinements have not solved the dominant visual gap. Incidental PC_RPAsset prefilter changes and missing DOTween mdb metas are not visual source edits.
