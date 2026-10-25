# Study 58 — grain geometry baked into the field surface, rejected

October 25, 2026. Source `a2339e1`; TN-02/09/12. The preceding corn layout still turns into small repeated marks. This experiment changes the rendering construction: a continuous terrain material derived from existing artist grain geometry, with sparse actual boundary plants. It does not add the raised canopy shell rejected in study52.

## Construction

The existing CC0 Quaternius-derived `FieldWheat` mesh (552 vertices, 306 triangles) is used in an offline periodic tile. A 20×20 cell interior plus a four-cell overlap border creates 784 combined instances, 432,768 vertices and 239,904 triangles **during the bake only**. A top-down 10 m orthographic footprint renders two 1024² maps: [albedo](study58-grain-albedo.png) and [normal/height](study58-grain-normal-height.png). The original mesh is not changed. [Bake script](study58-bake.cs.txt), [shader](study58-CropBake.shader.txt), [receipt](study58-bake.json).

These are deterministic source-geometry renders, not AI-generated artwork, and not a new set of in-game plants. An experimental copy of the ground shader samples them in the existing curved field coordinate, retains illustrative access lanes, and applies the existing terrain lighting. The terrain surface stays at its original display height. Of the original 265,147 crop transforms, 28,118 remain near the visual cover edge or in broad sparse pockets; no horizontal root displacement is introduced. [Application receipt](study58-apply.json). Active annual state remains Year1/Planning/revision0/cash1,000,000,000 cents/queued0/assets0.

## Rendered result and decision

[Normal](study58-surface-normal.png), [close](study58-surface-close.png): the material loses the raised-sheet edge failure but reads as a patterned ground plane, with conspicuous darker holes and disconnected stalk patches. One follow-up reduces low-height color contrast and normal intensity. [Softer normal](study58-soft-surface-normal.png), [softer close](study58-soft-surface-close.png). The floor-like appearance remains. **Reject this material as a completed crop construction.** A calmer image is insufficient when it loses convincing planted volume and close material form.

All captures are 1280×720 Unity camera renders without overlay HUD. Normal and close camera scripts are the [study57 controls](study57-normal.cs.txt), [close](study57-close.cs.txt); world/lighting/source masks remain the same. No browser, motion, performance, lifecycle or release acceptance is claimed. Further contrast/noise/tiling adjustments to this flat canopy bake would repeat the same failure.

## Technical limits and cleanup

The first visible shader variant exceeded the 16-sampler register limit and rendered magenta. Sharing the existing repeat sampler eliminated the Editor compiler error and allowed the shown comparisons. The initial empty ShaderUtil report was before the required variant compiled; it was not proof of a working rendered shader. No WebGL export was attempted, so texture-unit capacity and browser portability remain unproved. These experimental shaders are removed from Assets through AssetDatabase; replayable text is archived here.

Play was restarted after cleanup. [Restoration receipt](study57-restored.json) verifies zero study57/58 objects and unchanged live annual state. No source terrain, production shader, field placement algorithm, annual model or new image-generation dependency is retained. The next substantial construction should address cultivated soil and plant volumes at parcel scale together; do not return to scattered grain, a raised canopy sheet, a denser grass carpet, corn-row scaling, or texture-only canopy fills.
