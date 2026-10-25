# Study 68 — ground and vegetation as one surface system

October 25, 2026. Research/download stage; no accepted visual result. Source `f1b6f73`; refreshed `origin/main` inspected at `3562320`. Patterns TN-01/02/09/12. [Operative goal](../../../../PHASE_2A_GOAL.md), [reference library](../../../../DESIGN_REFERENCE.md), [previous rejected woodland construction](../study67-rendered/README.md).

The dominant failure is the entire field–meadow relationship: repeated isolated crops above highly regular soil strips, grass islands against bright empty lawn, and weak connections to woodland and water. Another tree replacement or grass tint cannot establish the required whole-scene quality. Investigate a shared terrain/vegetation surface system and artist-authored plant clusters before choosing the next field construction.

## Acquired resource

[RG Poly — Stylized Asia Environment Megapack, Unity URP](https://rg-poly.itch.io/stylized-asia-enviroment-megapack-unity-urp). The creator's page explicitly lists **CC0 assets**, **MIT code**, and no generative AI. Used the public “No thanks, just take me to the downloads” flow; no payment or account purchase. Local original archive: `Builds/ArtResearch/Study68/RGPoly-StylizedAsia-2022.3.6f1.unitypackage`. Original filename identifies Unity 2022.3.6f1. **56,723,476 bytes**, SHA-256 `b1d276bf3ec494e5e1ca598106f4fbb56308363b0a4d3e86688e89f889490b0c`.

[Inventory](archive-inventory.json), [inspection receipt](archive-audit.json). Actual contents: 107 FBX files, 108 prefabs, 32 materials, 25 PNGs, two scenes, two Shader Graphs, two subgraphs, terrain/layers and baked lighting. No C# scripts or project settings. The archive was unpacked outside Assets with traversal validation; original GUID metadata preserved. It contains no standalone license text; the creator's explicit license declaration is the acquisition basis.

The grass material and terrain grass layer reference the **same texture GUID** (`d7cfdb0d65d21aa4fa9d3796f54d0ec8`). The supplied graph contains triplanar texture sampling and position/time/sine displacement. This is a concrete workflow to inspect, not evidence that its rendering meets Terra Nil's quality. The grass texture is a broad green mottled surface; the soil texture has photographic-looking debris. Their suitability must be judged in the native scene and Terra's play camera. Buildings, characters and Asian setting are outside this experiment's product scope.

## Primary artist research

- [Antonin Pardon, Hidden Pathway workflow](https://80.lv/articles/breakdown-how-to-create-a-peaceful-fantasy-nature-environment): blends several plant forms through terrain layers and lets vegetation roots inherit terrain color. His scene-specific lighting is not a browser performance recipe. **Application:** test shared color/masks across ground and vegetation; upward normals alone are already present in Terra and insufficient.
- [Johan Handin, Forest of Slumber](https://johanhandin.com/stylized-environment): uses world-space masks across vegetation and terrain-related materials, with contrasting paths as visual rest areas. **Application:** carry surface relationships across asset boundaries rather than assign unrelated per-object colors. His HDRP implementation is reference material, not a dependency to install.
- [Karl Schecht, Stylized Baseball Field](https://karlschecht.artstation.com/projects/YanQyw): documents custom foliage normals for coherent lighting. **Application:** diagnose shape/shading separately; avoid treating this familiar technique as an untested new solution.

## Next bounded evaluation

Import the original resource into an isolated study directory, preserving native materials, graphs, scale and demo arrangement. Inspect its actual grass/terrain contact before adapting anything. No production scene, renderer, HUD, annual model or camera restriction changes are implied. Native and Terra normal/close/moving comparisons remain required. Keep prior flat canopy bakes, raised sheets, sparse corn and density-only forest trials rejected; this download does not reverse those decisions or pass Phase 2A.
