# Study 56 — reject grass-as-grain and exposed field interiors

October 25, 2026. Source `e516141`; continuing the existing isolated visual worktree after fetching `origin/main` at `7f5b292`. TN-02/09/12. The previous goal turn verified the pushed checkpoint but did not advance the visual implementation. This study produces new rendered evidence and rules out two crop constructions; Phase 2A remains open.

The dominant foreground failure remains the crop's fine repeated marks. The test uses the actual Wisconsin scene, terrain, field coverage, light and two fixed cameras at 1280×720. These are Unity camera renders without the overlay HUD, not Chrome or release evidence. Live annual diagnostics remained Year 1 / Planning / revision 0 / cash 1,000,000,000 cents / zero queued actions and accepted assets. No annual state or source geography was changed.

## Overlapping source grass as a grain mass — rejected

[Normal control](study56-control-normal.png). The existing field contains 265,147 card-cluster transforms. A diagnostic replaces their render components with 62,830 roots selected from those existing positions, using a staggered 2.6 m cell selection. Root selection introduces no horizontal displacement. It retains the previous root eligibility but does not prove that enlarged leaf extents remain inside every parcel or lane.

The existing CC0 Quaternius `Grass_Common_Short` geometry is broadened to a 4.2 horizontal multiplier, with height varying over broad patches. There are 48 components / 149 batches. The [initial image](study56-initial-normal.png) reveals an excessively bright tint and sparse reduced grass. A controlled shader variant uses an explicit straw palette and full source geometry, avoiding the inappropriate green-to-gold tint and removing the grass LODs for this diagnostic. [Normal](study56-stands-normal.png), [parcel focus](study56-stands-focus.png). The result still reads as dark scattered marks, with no convincing cultivated mass.

A single overlap comparison increases horizontal extent by 1.9 and vertical scale by 1.45, rebuilding batch bounds. [Overlapping focus](study56-overlap-focus.png) fills the surface but creates an indiscriminate spiky carpet and swallows access lanes. The visible failure survives both constructions. **Do not continue density/width/tint tuning of this grass-as-grain approach.** The source grass remains useful for restoration; this rejects its proposed crop role.

## Calm interiors with edge and lane detail — rejected

A materially different control returns to the existing wheat geometry, retaining only roots within 5 m of the visual crop boundary or within 3.8 m of the shared illustrative lane centre (the original lane exclusion still applies). It retains 59,580 transforms. [Normal](study56-edge-bands-normal.png), [parcel focus](study56-edge-bands-focus.png).

This reduces pervasive stippling, but the exposed earth/grass mixture reads as an empty textured floor, with narrow decorative strips. It does not communicate a convincing planted field and is not an acceptable replacement. The lesson from Terra Nil's broad readable surfaces is not simply to delete interior plants: the surface and its edge forms must read as one finished material. No shell was added, and no production material was changed.

## Stronger source geometry for the next comparison

The creator's [Ultimate Crops page](https://quaternius.com/packs/ultimatecrops.html) and creator-linked public folder were checked again. Existing research had acquired only the wheat models. Two additional free CC0 files, `Corn_3.fbx` (21,900 bytes) and `Corn_4.fbx` (37,660 bytes), and the creator's license were downloaded. [Source URLs and SHA-256](study56-crop-sources.json), [license](study56-crop-license.txt), [binary FBX inspection](study56-fbx-inspection.json).

The files contain solid leaf geometry and embedded material slots: the earlier plant has 197 control points / 180 polygons / one green material; mature corn has 535 / 529 / three materials. These are source FBX counts and raw local-coordinate bounds, **not imported Unity vertex counts, triangles, metric dimensions or performance evidence**. They have not yet been imported or rendered. Species/size would remain illustrative, not a claim that the source location grows a measured corn crop. Test broader leaf forms and coherent row/stand structure in the actual playable scene before retaining anything; a different asset alone cannot certify quality.

All diagnostic source changes were confined to Play mode, plus one temporary shader subsequently deleted through AssetDatabase. Play mode was stopped and this worktree's Editor closed gracefully to leave the machine clear for the separate performance comparison. Production crop rendering is unchanged. Scripts and shader text are archived with these findings; no image generation, paid acquisition, new build, browser timing or visual pass is claimed.
