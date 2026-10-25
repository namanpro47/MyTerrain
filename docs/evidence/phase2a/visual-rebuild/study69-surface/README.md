# Study 69 — a shared meadow surface

**Prepared, not rendered or adopted.** Continuing the visual rebuild from `e768e3c`; refreshed main inspected at `67c0141`. Incoming annual-status label changes were reviewed and remain with their owner; this study touches no annual/HUD implementation. TN-01/02/09/12; [operative goal](../../../../PHASE_2A_GOAL.md), [visible reference review](../../../../CURRENT_VISUAL_REVIEW.md).

Studies67–68 reject another forest/grass family swap. The next construction treats the meadow as a shared painted surface with plant geometry emerging from it, rather than separate dark grass islands on bright lawn. It uses existing licensed Julio Sillet painted grass and existing artist grass meshes; no image generation or new asset search.

The proposed construction has two parts, evaluated in order so a density change cannot conceal a failed material comparison:

1. Ground and grass roots sample the same artist paint at the same world coordinates through one HLSL function. A common palette transform replaces the unrelated constant ground color and dark root gradient. The original source textures are unchanged.
2. A low, overlapping base layer replaces isolated mapped-grass patches, with a smaller amount of taller growth for structure. Its LOD must consider footprint width so low plants do not disappear solely because their height is small. Source cover, road/water clearances, site exclusions and annual state remain authoritative.

[Shader preparation](prepare-shaders.py) writes experimental shaders to ignored `Builds/ArtResearch/Study69/Shaders`, outside Assets. [First application stage](apply-materials.cs.txt) keeps every existing plant transform, camera and light unchanged, isolates material copies to mapped meadow, and checks unchanged source-world JSON/annual diagnostic. Both scripts still need compilation and actual rendering. New placement is not yet implemented and should proceed only if the first stage gives a visible normal-view benefit. No visual, performance or lifecycle check has passed for this study.

Judge the normal full image first, then close/wide and motion. Reject obvious repeated texture stamps, dark detached roots, a flat shell, excessive geometry or a simple tint improvement that leaves the same dominant failure. Farmland remains a separate substantial gap: meadow improvement cannot pass the field/ground category alone. Do not promote this experiment merely because it is more coherent than a rejected baseline.
