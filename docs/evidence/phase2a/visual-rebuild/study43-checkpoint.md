# Study43 — unfinished connected-meadow experiment

Checkpoint requested by the user; this is not visual acceptance or production adoption. Review against [DESIGN_REFERENCE](../../../DESIGN_REFERENCE.md) and [the visual goal](../../../PHASE_2A_GOAL.md), particularly TN-02/09/10/12. Phase 2A remains open.

## Source and verification

The visual branch merged refreshed `origin/main` at `18016cf5f67d30972a8f241087cab29a52c12611` in `b04bb3d`, preserving the incoming atlas, connected Wisconsin fields and annual HUD. Compilation returned zero errors/warnings and the staged regional packs loaded in the Editor. Follow-up verification passed 678 atlas/save assertions and 34 full annual lifecycle/JSON checks on the merged source. A new Chrome export has not yet been run on this merge. Study39 remains this branch's last exported candidate with unresolved performance. No deployment or main integration is certified here.

## Experiments preserved

The [shadow trial](study43-shadow.cs.txt) disabled casting on 206 instanced components containing groundcover. Mixed components also affected their other batches, so this is not an isolated grass-only control. The [before](study43-before-close.png) and [after](study43-no-cast-close.png) showed no material improvement. The temporary source toggle is preserved as a [replay patch](study43-shadow-toggle.patch.txt) and removed from production source; its default was true.

The [community replay](study43-communities.cs.txt) removed 14,308 old groundcover submesh transforms in the bounded region and placed 6,842 low plants, including 1,729 on coarse sealed context already illustrated as grass by the terrain shader. This is decorative planting, not a change to source cover, geography, surveyed vegetation or financial accounting. Mapped roads/banks and project polygons are excluded. Heights follow broad terrain-related variation with coherent orientation. [Execution receipt](study43-communities.json).

[Normal view](study43-community-normal.png) and [close view](study43-community-close.png) are saved and reviewed: the reduced, wider plants remain too small to form readable masses at normal scale, while the close view is still dominated by oversized painted grass. This construction is not selected. All captures are 1280×720 Editor camera renders with the controller disabled for controlled framing; they are not browser gameplay or performance evidence. The source paint remains at its previous scale. Annual state stayed Year1 Planning, revision0, cash1000000000 cents, queued0 and assets0 during the trial.

Play mode was stopped for this checkpoint. Runtime replacements were discarded; replay scripts and captures preserve the experiment. Unused local asset imports and editor-generated settings remain outside this commit. Free source acquisition/license receipts for the earlier rejected packs are already recorded in studies40 and42.

## Resume

A single isolated ground-scale control is prepared to distinguish oversized paint from geometry problems. Separately evaluate the already imported three Quaternius tree forms with their source textures; they were not covered by the rejected grass trial. Do not infer quality improvement from plant count, compilation or the checkpoint itself. The merged source still requires combined checks and actual Chrome verification before any validated delivery to main.

## Ground-scale control

[Normal](study43-ground12-normal.png) and [close](study43-ground12-close.png) change only the painted grass UV scale from40m to12m after replaying the same community geometry. This removes oversized brush marks but leaves the meadow flat. It is a useful supporting correction, not sufficient art direction. The temporary shader and replay are preserved; production ground source is unchanged.
