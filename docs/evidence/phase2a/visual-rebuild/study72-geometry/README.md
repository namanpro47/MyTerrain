# Study72 — original plant geometry and field edges

Rendered: original narrow-stalk geometry still fails the desired field appearance. Not adopted. Continues experiment5279dd5 after fetching main ea37af0. No production adoption. TN-02/09/12.

Study71 establishes that source normals alone do not fix the spatial-slice canopy: dark speckling or a flat slab remains, and cut lane walls expose the proxy construction. This study abandons those proxy slices for the representative close parcel. Test original CC0 Quaternius crop geometry with uniform scaling, rather than the older FieldWheat mesh that widened source plants disproportionately. Preserve stalk/head form and a natural silhouette along irregular access-lane edges. Hold the terrain, source-cover mask, light and camera fixed. Begin with one parcel so visual rejection is inexpensive. A useful close mesh is only a starting point: broad field massing, coherent distant representation, shadows, motion, lifecycle and Chrome performance remain required before integration.

## Frozen prepared checkpoint

The combined-release coordinator requested a bounded checkpoint while this test was waiting for desktop77. Reservation78 was cancelled before acquisition; no Editor was opened and none of these scripts executed. The geometry/planting script is preparatory only. In particular, its provisional .5m spacing must be assessed against imported bounds: raw FBX inspection shows a very narrow stalk along local Z before importer transforms. It does not establish a filled field, performance feasibility, or a successful render. [Raw source identity](source-preflight.json); [Unity inspection script](inspect.cs.txt); [prepared application](apply.cs.txt).

Prior goal turn produced useful actual study71 evidence and changed the next action from lighting to geometry. This turn identifies a concrete current-main integration conflict and preserves the next test without claiming visual progress from an unrendered script. See [the release checkpoint](RELEASE_HANDOFF.md). Goal remains active; Phase2A is not accepted.

## Actual geometry result

The prepared test was resumed under reservation80 after the release checkpoint. Actual renders use28,029 original Quaternius wheat plants in a24×24m patch, .14m spacing, uniform1.2m nominal height, native yellow material adapted to URP Simple Lit, jittered placement and irregular access-lane edges. Six spatial groups contain2,858,958 source triangles. The raw/native source and imported metric bounds are separately recorded; imported width is approximately.124m and depth.071m.

[Control normal](control-normal.png) · [control close](control-close.png) · [control detail](control-detail.png). [Native geometry normal](native-normal.png) · [native geometry close](native-close.png) · [native geometry detail](native-detail.png). All1280×720 Editor camera captures with HUD absent. Detail uses zoom10 at the same target, close35, normal105. The original entire south-01 baseline is temporarily hidden to isolate the patch; surrounding bare soil is diagnostic scaffolding, not a proposed field layout or claimed whole-scene design. [Hashes/frames](render-receipt.json).

**Reject this full-geometry direction as the field solution.** Correct plant proportions and real edges remove the slab wall of study70, but at close/normal scales the result remains granular, with no convincing grouped painterly form. The cost is already high for576m² and cannot simply be extended over the cultivation area. The test does not justify more density, tint, normal blending or the previous widened source mesh. There is no browser timing or motion claim.

The first application accidentally cancelled the FBX root-axis conversion, producing a horizontal stalk with20m depth. This was caught from the returned bounds and replaced before the assessed captures. The corrected script retains the source root transform, checks upright proportions before placement, and normalizes to1.2m height. The [invalid-axis receipt](application-invalid-axis.json) is diagnostic history; it is not evidence against the artist's geometry. [Correct application](application.json) records scale1.265832 and the unchanged source-world JSON/live annual state. No code or asset in production was changed.

[Cleanup](cleanup.json) and [check](check-result.json): Play stopped, TerraMain clean, zero study objects. Editor78510/launcher78499 closed and confirmed absent. Reservation80 released. No outstanding desktop/editor hold.

## Different free crop family acquired

Downloaded the free30-model edition of [dglopez's Farm & Market pack](https://dglopez.itch.io/low-poly-farm-market-asset-pack-30-free-3d-models) through its no-payment link. The creator labels it as made without generative AI. No purchase/account/terms-acceptance step. [Acquisition, archive hash and inventory](free-pack-acquisition.json); [download receipt](free-pack-download.png).

The actual wheat file has910 OBJ vertices,1,672 triangulated faces, bounds.8×1.12×.43m, and seven material regions separating stalk, leaves and grain highlights/shadows. [Source inspection](free-wheat-inspection.json). This is a different authored construction with much broader grouped form than the current single slender stalk. **It has not been imported or rendered, so no quality or performance claim follows.** Preserve its original material regions for an initial comparison; do not flatten all parts to the current wheat shader before seeing the supplied design.

The downloaded LICENSE permits use/modification in end products and private project-team sharing but prohibits standalone asset redistribution, including repositories. Original files, archive, license and manifest remain together in ignored local Builds storage; they are not added to Git or the runtime. Reproduction can obtain the free edition from the creator. A public source distribution must not expose these standalone assets. The paid Holotna alternative was excluded under the free-only constraint; generated-model search results were not used.

Next action is one native-material comparison of this different crop family in the same actual landscape, with a nearby unmodified object view to establish its authored form. If it does not improve the image, do not tune it repeatedly or import the rest of the pack. Phase2A's whole-scene gate remains unchanged.
