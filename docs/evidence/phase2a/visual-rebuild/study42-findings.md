# Study42 — older Quaternius grass trial rejected

The [download and import preflight](study42-preflight.md) records the CC0 source family and exact archives. This trial evaluates grass and occasional bushes, not the imported trees. No production adoption, visual score, new browser export or performance acceptance. Patterns TN-02/09/10/12 remain the review frame.

## Fair import before judging art

The first wrappers preserved the FBX's 100× scale and axis correction on their root. InstancedNature intentionally removes a prefab root transform when composing its parts, making this wrapper unsuitable. The first `native-normal/close` captures therefore show a **failed import control**, not a meaningful asset comparison. An identity root with the original FBX nested below it now preserves the source transform correctly. [Wrapper replay](study42-wrap.cs.txt) corrects all nine preview prefabs. Production InstancedNature was not edited.

After restarting Play, frames advanced 161→171. The controlled opening was set at frame194. The bounded trial replaced 15,944 groundcover placements in 127 tiles. Grass selection uses the extruded, large and small source meshes; 1.5% of placements use Bush. Grass width/height scales are4.5/2, with0.8–1.2 size variation; Bush uses1.1 with the measured native minimum-y offset. This is illustrative vegetation, not surveyed plants. Current terrain, route masks and annual state are unchanged.

## Actual comparison

[Corrected native normal](study42-fixed-normal.png) and [corrected native close](study42-fixed-close.png) show thin dark blades that produce strong visual noise. The ordinary MeshRenderer [detail control](study42-ordinary-control.png) is dark too, so the evidence does not support blaming instancing alone. Default RenderParams reported probe usage Off, ordinary renderer BlendProbes; [Unity documents](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rendering.LightProbeUsage.html) that Off still supplies the ambient probe. No lighting-probe fix was justified or made. The detail control uses a diagnostic camera outside ordinary zoom, not an acceptance frame.

A single adapted lighting control uses the existing soft-groundcover shading with the correct source mesh Z axis for height and world-scale pixel measurement. Source pixels and geometry remain unchanged, but shading/palette behavior is deliberately adapted. [Adapted close](study42-adapted-close.png) and [adapted normal](study42-adapted-normal.png) lighten the blades. They still look wiry and noisy; isolated pale bushes do not produce a coherent meadow. Shader compilation returned no messages for this control. The main grass direction is rejected. The imported tree shapes have not been evaluated and should not be declared accepted or rejected based on this grass trial.

All captures are 1280×720 Unity camera renders. At frame20,189 the state remained Year1 / Planning / revision0 / cash1000000000 cents / queued0 / assets0. The controller was disabled for camera control, so these are visual experiments rather than gameplay checks. Play mode was stopped and the runtime replacement discarded. Temporary adapted shader source is preserved here and removed from Assets. Source models and preview wrappers remain local outside Resources pending any justified further investigation.

## Next construction decision

The shell experiment (study41) and multiple source-mesh swaps now rule out continuing the same scatter-plus-large-paint approach as a sufficient visual upgrade. Next build a representative connected ground/vegetation community with an explicit hierarchy: continuous terrain-related base, broad middle-scale plant masses, sparse fine detail. Evaluate actual shape and ground contact before adding full-map density. Existing source meshes can be assembled and simplified into those masses, but another isolated plant purchase/download or tint tweak is not the next milestone. Keep source cover/accounting honest, retain a normal/close/wide comparison, and reject any construction that simply hides defects with a camera restriction.

The water owner separately reports a native MIT shader trial; inspected comparison shows visible bed detail and blue water, but its repeated stone pattern remains conspicuous. That work has not been integrated or browser-verified in this branch.
