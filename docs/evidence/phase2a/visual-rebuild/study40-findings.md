# Study40 — AssetQuest forest family rejected

The free AssetQuest demo was evaluated in the Wisconsin scene and rejected as the primary vegetation family. It does not improve whole-scene quality enough to replace the current candidate. Phase 2A remains open; no visual score is raised. Relevant reference patterns: TN-02/09/10/12 in [DESIGN_REFERENCE](../../../DESIGN_REFERENCE.md).

## Source and setup

See [acquisition and license preflight](study40-resource-preflight.md), the copied demo license/readme (line endings and trailing whitespace normalized), and [import hashes](study40-import-manifest.json). Only the free CC0 demo was acquired; no purchase or image generation. Ten source meshes and the original plant atlas/palette were imported outside Resources. The local research archive remains available under `Builds/ArtResearch/AssetQuestForestDemo/` with its previously recorded SHA-256. Imported preview assets remain local and are not adopted by production code.

The temporary Play-mode trial used source positions for 2,272 mature and 4,981 young trees; 473 mature trees used the pine mesh. Source-height scaling preserved the broad cohort structure. A bounded understory trial replaced 15,944 placements with the demo grass, green patch, fern and blackberry meshes. Analytical cover, terrain heights and annual accounting were unchanged. Replay scripts are saved alongside this note; their absolute paths target the original research worktree and need adjustment elsewhere.

Materials followed the source readme: alpha cutouts, double-sided foliage, zero metallic, original atlas pixels, and alpha-coverage mipmaps. Initial materials used URP/Lit. The subsequent volume-lighting control used the existing TerraArtistLeaves shader without changing source geometry or texture pixels.

## Findings

All retained captures are 1280×720 **Unity camera captures**, not Chrome acceptance evidence. The normal views use the same reset camera; close views examine the understory. The material-detail view is a separate diagnostic camera, not a proposed play camera.

- [Before](study40-before.png) and [native woodland](study40-woodland.png): fine branch/leaf cards lose coherent mass at play distance and introduce dark speckling.
- [Native family normal](study40-family-normal.png) and [close](study40-family-close.png): ground plants add busy small marks without a convincing continuous planted surface.
- [Shadow control](study40-shadow-control.png) and [ordinary-mesh detail](study40-material-detail.png): disabling received shadows did not materially resolve the dark forms; a non-instanced mesh also exhibited thin, dark-backed foliage. This does not establish an instancing defect.
- [Volume-lit close](study40-volume-close.png) and [normal](study40-volume-normal.png): adapted shading lightens dark backs, but repeated olive forms and noisy ground contact remain. The family still fails the intended whole-scene bar.

Play mode was stopped after evaluation, discarding temporary scene changes. No runtime source was changed, no new Web export was made, and no performance or gameplay acceptance is claimed for study40. Study39 remains the last exported candidate, with its visual and performance failures unresolved.

## Checkpoint boundary

This commit preserves evidence, replay scripts and provenance. Generated Editor settings, font cache changes, broad unused TriForge imports, and local AssetQuest preview imports are excluded. The latest remote main observed before this checkpoint is `5960a6590f9963bb2d3e59b596ef79b5ca0999fe`; it has not been merged into this visual branch. The existing study39 candidate must not be presented as validated main integration.
