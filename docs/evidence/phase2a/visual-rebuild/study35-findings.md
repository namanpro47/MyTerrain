# Study35 — lower vegetation and material/composition alternatives

Patterns TN-02/09/10/12. [Persistent reference](../../../DESIGN_REFERENCE.md), [visual goal](../../../PHASE_2A_GOAL.md). These are temporary Editor trials on the same source location, not accepted runtime changes. Play was stopped after inspection; all trial objects/materials and camera adjustments were discarded. No purchases, image generation, annual state changes or source geography edits.

## Why the native plant is not selected

The free TriForge ForestPlant_B_02 has 127 triangles, a 0.293m native height and roughly 0.48m width. A 360×360m representative woodland area received 30,764 plants in clustered colonies at 1.3–2.3 scale, constrained to source woodland and away from roads/streams. This is illustrative placement, not observed vegetation. [Replay](study35-understory.cs.txt).

[Native material at playable scale](study35-native-understory.png) shows essentially no contribution: its supplied distance fade is 40m, below the current camera distance. [Ground-close diagnostic](study35-native-ground-close.png) reveals dark specks under the trees. This diagnostic disables the main camera controller temporarily and is outside the playable zoom limit; it is not acceptance evidence. Extending only the cloned materials' fade distance to 600m yields [small dark speckles](study35-native-understory-distance.png), not useful broad vegetation form. Rejected; neither the pack nor extra geometry solves the missing layer by itself.

## Middle height layer

[579 young-tree instances](study35-young-layer.png), 3.5–6.5m illustrative heights derived from the native sapling's 1.4163m bounds, occupy canopy openings in the same area. They use the existing cluster shader and canopy footprints to avoid mature crown interiors. [Replay](study35-young-trees.cs.txt). The transition is more legible than the low plant trial, but it still repeats one shape and does not resolve the whole-scene gap. Not promoted as production source.

## Native material on clustered geometry

The original FWTreeLeaf material was copied only into runtime material instances, retaining the baked clustered geometry. [Unmodified native material](study35-native-cluster-material.png) makes the canopy much too dark in Terra's lighting. [Adapted source material](study35-native-cluster-adapted.png) uses Color=(0.8,1,0.8), BaseColorSaturation=1, AOIntensity=0.35 and VertexAOIntensity=0.4. It restores brighter tips and dark interiors but still produces heavy repeated clumps. The creator shader includes normal mapping, vertex AO and translucency; those features alone do not establish a stronger complete scene. No native leaf shader change is retained.

## Composition test and next alternative

After restoring the production leaf material and disabling both extra vegetation trials, two closer same-location camera studies expose the next dominant problem: [river/road/forest](study35-river-composition.png), target (-120,-20), zoom60, pitch36; [northern meander](study35-river-north.png), target (-180,100), same zoom/pitch. Source target height is terrain-relative, yaw remains -32. The grass surface between those features stays broad, flat and weakly articulated. Moving the camera does not solve it.

Stop adding small plant or tint variations to this construction. Next test a coherent, connected grass/meadow surface in one forest–field–water section, using the existing artist-authored grass family and its established ground-related shading, with readable height and patch transitions. Compare it at normal play scale before applying it across the AOI. Also fix the unreachable woodland groundcover condition (grouping<0.4 outside, >0.44 inside); no fix is claimed yet. Preserve clearings and source-cover exclusions, and measure the geometry/overdraw cost only after the visual change is substantial.

Frames advanced 1184→1198 before trial work; later evaluations ran at frames6028,7554,16140. Annual state remained Year1 / Planning / revision0 / cash1000000000 / queued0 / assets0. Source readback and screenshots establish temporary rendered trials, not a Chrome or performance pass.
