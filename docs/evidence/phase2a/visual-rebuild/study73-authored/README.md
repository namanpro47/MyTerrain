# Study73 — differently authored crop family

Rendered patch comparison: clearer close forms, insufficient field-level coherence. Not adopted. Continues95d882e, main ea37af0 fetched/reviewed. Previous turn made progress: original slender geometry was actually rendered at three scales and rejected, and this different free family was acquired and inspected. TN-02/09/12.

Test the wheat from dglopez's free Farm & Market30-model edition, with its native geometry and seven material regions. Preserve the original stalk/leaf/grain color distinctions; adapt shader compatibility to URP Simple Lit only. Compare a single specimen and a24m source-eligible parcel patch using the same terrain/camera/light as study72. Uniform1.2m height is illustrative, not an observed crop measure. No new gameplay or crop-species claim.

The source geometry has broader grouped form (.8×1.12×.43m) than the former narrow stalk. It is1,672 triangles, so a successful close result would still require a deliberately measured distance representation. Do not extend the full mesh over the map or spend another pass only adjusting tint/density if its silhouette fails.

The downloaded license allows use/modification in end products and private project-team sharing, but forbids standalone asset redistribution. Original files, notices and manifest remain in ignored local storage and the untracked FieldMassStudy/Dglopez import folder. Only reproduction scripts, receipts and rendered evidence belong in this study directory. Source: https://dglopez.itch.io/low-poly-farm-market-asset-pack-30-free-3d-models . Acquisition and hashes: ../study72-geometry/free-pack-acquisition.json . No paid resource or generated art.

## Actual comparison

Unity6000.6.6f1 Metal. The source FBX imports1,672 triangles into seven submeshes with4,657 split vertices. All seven native imported colors are retained, with Simple Lit compatibility and matte response. The patch contains1,161 uniformly scaled1.2m plants, six spatial groups and1,941,192 triangles. [Native material inspection](source-inspection.json), [application/source-state guard](application.json).

[Control normal](control-normal.png) · [control detail](control-detail.png). [Authored patch detail](authored-detail.png) · [close](authored-close.png) · [normal](authored-normal.png). All1280×720 actual Editor camera renders, HUD absent. The24m patch is isolated by hiding the original entire south-01 baseline; the surrounding bare field is scaffolding, not a proposed layout. Controls use the original retained scenery. [Hashes and frames](render-receipt.json).

The broader grouped source and separate green/gold surfaces produce more legible leaf/stalk/grain forms in the detail view than study72's slender single stalk. At normal play scale, however, the field still fragments into bright separate marks. **Do not promote this as a finished field or extend its full-resolution meshes across the map.** A new asset family alone has not solved the massing and scale problem. TN-02/09/12 remain open. This result is a useful close-form candidate only; no whole-scene score, browser performance or motion claim follows.

### Invalid specimen capture and correction

The [single-specimen image](specimen.png) is invalid: the plant is outside the frame. Source inspection identifies the cause: `TerraMain.RenderView` enforces camera Y at terrain+35m even when a diagnostic sets zoom2 and targets the1.2m specimen. This does not mean the mesh or material failed. The receipt marks it invalid. A later specimen inspection should directly position a separate diagnostic camera with a look-at target and record the actual position/frustum; do not infer that setting the game zoom field achieved that framing. The production camera remains unchanged. Detail/close/normal patch captures visibly contain the patch and remain valid at their actual framing; zoom10 is not a claim that the gameplay camera supports that zoom.

## Decision and next rendering question

Stop single-model swaps and density/tint tuning as the main field strategy. Study70 showed useful continuous field massing but poor plane/coverage behavior; studies72–73 show that dense full plant geometry is costly and still becomes fine marks at normal scale. The next substantial alternative must explicitly separate a coherent distant field surface from the close plant/edge representation, with the latter preserving this source family's distinct forms. Build and judge those scales together, including movement and transition; another isolated specimen cannot pass the landscape gate. Do not simply restore the flat bright slice canopy or the old vegetable-dot rows and declare completion.

The source remains a local licensed evaluation asset. No original/modified standalone model or embedded source vertex data is redistributed in this evidence. The original FBX/notices/manifest remain in untracked FieldMassStudy/Dglopez and the acquired archive remains in ignored Builds storage. Reproduction downloads the free edition from the creator, imports it locally, and uses these archived scripts. [Source identity](source-input.json).

## Cleanup and release scope

[Cleanup](cleanup.json) and [check](check-result.json): Play stopped, TerraMain clean, zero study objects. No compiler/shader/exception/pack-error markers found in the scoped [log scan](log-check.json). Original source-world JSON and annual diagnostic remain unchanged at year1, Planning, revision0, USD1,000,000,000, zero queued actions/assets. Editor84218 and launcher84213 closed and confirmed absent; reservation89 released after the bounded comparison. No main runtime/HUD/scene/annual change and no new asset activation in frozen release770d4c0. The release owner requested the desktop for combined/public review; no additional capture was queued over that release work.
