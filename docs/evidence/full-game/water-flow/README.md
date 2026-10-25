# Lightweight stream-surface candidate

Patterns TN-09/11/12. This follows the rejected full native adapter in `../water-study/`. The dominant failure being tested is flat surface shading without a readable moving normal. Generated channel meshes lack tangents, so the single supplied normal texture is sampled explicitly in world space instead of using a tangent-space Shader Graph path.

The existing one-pass shader gains two moving texture samples, restrained direct-light highlights and an illustrative sky-color response. It retains drawn currents and opaque water so the repeated bed texture does not dominate. Full and reduced motion use the existing `_MotionTime` clock. The dry-swale branch retains the previous output. No camera depth/color copy, refraction, planar reflection, new geometry, annual change or global quality change. The MIT source notice ships with the atlas and is linked from About; source pixels are unchanged.

Matched Editor diagnostics: actual connected-field Wisconsin scene, frame716,1600×900,FOV42°,pitch42°,yaw−32°,center(-166.295,8.8599,-134.294),distances55/145/450m,clock12.5s. Baseline and candidate are captured while paused at the same frame. Close water reads more clearly as a reflective surface; regular banks and repetitive nearby ground/vegetation remain unresolved. No Phase2A score is assigned.

- Control: [close](control-55.jpg), [normal](control-145.jpg), [wide](control-450.jpg).
- Candidate: [close](candidate-55.jpg), [normal](candidate-145.jpg), [wide](candidate-450.jpg).

The initial trial replay names temporary Editor-only asset paths. The final shader is `Resources/TerraWater.shader`, with its default texture bound through ShaderImporter to `Art/ExternalKit/StreamSurface/SurfaceNormal.png`. [Running-Editor check](editor-check.json) confirms the shared runtime material has the real1024×1024 texture, shader compilation succeeds, no extra camera passes are requested, motion advances/freezes, and the annual diagnostic is unchanged. Actual frames advance667→716 in the study and1948 after final binding.

Exact WebGL build, Chrome appearance/motion and matched cost checks are pending. This is an unaccepted local candidate, not a completed phase.
