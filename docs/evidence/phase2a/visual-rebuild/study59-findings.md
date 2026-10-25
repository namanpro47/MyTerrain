# Study59 — cultivated soil beds (unfinished checkpoint)

Saved at the user's commit-and-push request. Patterns TN-01/02/09/10/12 remain the visual reference; Phase 2A remains unaccepted. This is a runtime-only Editor experiment, not a retained gameplay change or a Chrome-tested candidate.

The Wisconsin West field (`south-01`) replaces its baseline crop display temporarily with 29 shaped soil strips and 4,311 young plants from the existing CC0 Kenney `crops_leafsStageB` prefab. The soil mesh has 37,584 vertices / 18,792 triangles. Beds follow the existing curved field coordinate and taper near parcel edges. The existing ground bank texture supplies the soil material. These authored planting forms do not claim observed crops or farming practice.

[Corrected soil close view](study59-corrected-soil-close.png) preserves the latest actual 1280×720 Editor render. The shaped beds provide larger cultivation forms, but repetitive planting and whole-scene quality still need review. No visual scores or acceptance claims are raised.

The [initial normal view](study59-initial-normal.png) and [initial close view](study59-initial-close.png) used the old golden ground beneath the beds: the diagnostic initially passed the nonexistent `cultivate` action to the surface painter. The corrected setup uses `cultivate-standard`; the [correction receipt](study59-soil-mask.json) reports 2,064 painted pixels. The archived setup script contains that correction, while the original setup receipt predates it. A corrected normal/overview comparison is still outstanding.

Both receipts report Year 1 / Planning / revision 0 / cash 1000000000 in stored units / zero queued projects / zero assets. The fixture modifies rendering directly; it does not submit annual actions. No production renderer, save, or ledger change is included in this checkpoint.

The archived setup, camera, capture and soil-mask scripts plus the experimental shader preserve the work outside Unity's compiled Assets tree. Replay requires the existing Play-mode scene and restoring the shader to its temporary `Assets/Editor/Study59/CultivatedSoil.shader` path. This is diagnostic source, not a standalone automated test. [File hashes](study59-checkpoint.json).

Still required before adoption: corrected whole-scene views, plant-extent/parcel checks, post-render shader diagnostics, lifecycle checks, actual Chrome/HUD/movement review, and source-matched performance verification. No new tests or build were run for this documentation-only checkpoint. Previously recorded validation remains bounded to its original source.
