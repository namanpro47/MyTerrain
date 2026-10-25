# Phase 2A — rebuild the visuals from first principles

> **Annual-gameplay handoff update — October 24, 2026:** The user has revised the product around ten annual decisions and the full [Actions & Effects catalog](ACTIONS_AND_EFFECTS.md). Keep this phase focused on the existing visual rebuild; its gate is unchanged. Prepare the accepted visual language to support real-scale hypothetical buildings and distinct queued/construction/established states later. The next live gameplay gate is now Phase 3A/3B's annual integration and three-year loop, not an instant-placement-only mission. Do not expand 2A into those later implementations or treat this planning change as acceptance of current scenery.

**Rewritten October 24, 2026, following the user's explicit rejection of the current visual quality.** This is the operative brief for the active Phase 2A goal. The previous presentation/UX brief is [superseded history](evidence/phase2a/superseded-ui-first-goal.md). Phase 2A has **not passed**.

## The goal

> Rebuild Terra's visual presentation from first principles until the actual playable browser game has the environmental richness, composition, lighting, materials, vegetation, water, and transformation quality of the selected Terra Nil references. Treat the current implementation as disposable: replace assets, rendering, camera, scene composition, UI, or entire existing aspects when a stronger result requires it. Research and obtain suitable licensed resources and tools. Judge success primarily by the rendered game, not by UX checklists, small improvements over our prototype, or effort already spent. Establish this visual standard before adding more gameplay.

**Terra Nil is the primary art/feel benchmark. Cities: Skylines remains a secondary reference for presenting complex information.** A functional, readable interface supports the world; UI polish cannot compensate for an unconvincing landscape.

The user's direction is explicit: the game is early, existing work need not be retained, and substantial replacement is authorized. Do not ask for permission to replace an implementation merely because it exists. Keep recoverable checkpoints, then make the better version.

## Why the previous approach failed

The previous iteration concentrated on panels, fonts, contrast, keyboard checks, border rendering, and incremental additions to a generic low-poly landscape. It did not close the direct visual gap. The user rejected that result. The current captures are a **rejected comparison baseline**, not a nearly accepted art direction.

The material problems to solve are larger than individual UI details:

- The opening reads as a distant GIS slab with a large empty surround, rather than an inviting landscape to inhabit and transform.
- Repeated faceted canopy shapes and uniform placement look like a stock-asset field; changing their tint or count is insufficient.
- Ground, stream banks, water, fields, and woodland edges lack a coherent, authored material language and convincing transitions.
- Detail is not composed across foreground, middle distance, and background; the whole-AOI overview makes important forms too small.
- The visual effect of an intervention is much weaker than the transformation promise of the reference.

Do not solve these by adding another HUD pass, more random geometry, a grading filter over the same scene, or a decorative noise shader alone.

## First principles: purpose and freedoms

The product remains a browser-playable rural environmental strategy experience: make a choice, see the landscape transform, understand an honest land-use trade-off. Continue with the same selected real location. Its source provenance and the distinction between observation, illustration, and hypothetical gameplay remain valuable product requirements.

**These are not obligations to preserve old rendering or code.** The current renderer, meshes, asset kit, scene bootstrap, material system, lighting rig, camera model, floating-board presentation, tool illustrations, panel layout, effects, and implementation-specific tests may be replaced. Existing features may be simplified, removed, or reworked where they obstruct a much stronger visual result. Retain code only when it earns its place.

Geographic evidence and presentation are separate layers. The source model must remain traceable, but the displayed world need not expose raw raster cells or repeat the entire AOI at a tiny scale. Art-directed cover blending, illustrative foliage, richer banks, representative crop patterns, and a closer playable camera are valid. Record visual abstraction; do not invent observed species, surveyed structures, precise yield, runoff results, or geographic accuracy. A study of a section of the same AOI is an art experiment, not a second level.

Unity/browser delivery and the educational core remain the starting platform and purpose. Choose techniques on visual merit and measured browser feasibility. Existing uGUI components or package boundaries are not an artistic constraint; update affected contracts if interfaces change. Do not add unrelated gameplay, levels, city content, or a generator to avoid this visual work.

## Visual research and resource acquisition

Use [DESIGN_REFERENCE.md](DESIGN_REFERENCE.md) as the persistent library, expanded by [VISUAL_REBUILD.md](VISUAL_REBUILD.md). Keep source/version/scale context for every comparison.

1. Inspect actual Terra Nil gameplay frames at wide, normal play, close, and transformed states. Study silhouette, value grouping, color relationships, ground treatment, foliage structure, water/bank geometry, contact shadows, light direction, atmosphere, and motion. Distinguish observed appearance from speculation about its implementation.
2. Study primary artist/developer material. The developer describes the commercial game's painterly direction; a contributing artist documents painted textures and game-ready vegetation. This supports researching a different asset/material approach, not just increasing polygon density.
3. Search for usable models, textures, shaders, vegetation/rendering tools, and add-ons. Download and inspect promising free/licensed resources proactively. Keep their sources, actual downloaded contents, license files, hashes, scale, material requirements, and browser implications in an asset register. Marketing images are selection evidence, not proof of our result.
4. Adapt a small coherent family of resources to one art direction. Replace or customize assets whose silhouette, surface treatment, or scale breaks that direction. Do not assemble an inconsistent collection simply because assets are free.
5. **Existing resources first; no further image generation in this pass.** The user rejected the image-generation-heavy approach on October 24. Evaluate coherent artist-authored asset families with their intended materials, shaders, lighting examples, and demo scenes before inventing replacements. Verify the complete result in our playable camera and browser. Generated tree cards and textures already present are provisional failed/unaccepted studies, not commitments. Do not keep adjusting the game to disguise their limitations. Later original art must solve a specific gap that existing resources cannot reasonably fill; generation is not the default production pipeline.

**Free resources only.** The user declined the Quaternius Source purchase on October 24. Obtain and evaluate free licensed resources; do not purchase assets or treat paid content as a prerequisite. Keep the same visual bar and prioritize stronger free art, supplied materials, composition and technical integration.

## Rebuild sequence

| Step | Concrete work | Advance only when |
|---|---|---|
| 0 — Reset the comparison | Preserve the rejected build/captures; identify its dominant visual failures beside matched Terra Nil frames; inventory assets worth retaining | The brief describes a visibly different destination, not a list of prototype touch-ups |
| 1 — Establish the visual target | Create two or three materially different compositions/material/foliage studies of the same location; compare a closer landscape-filling play view with a deliberate overview | One direction clearly improves shape, depth, richness, and atmosphere; record why weaker directions were discarded |
| 2 — Prove a representative scene | Build a coherent forest–field–water section with the intended camera, lighting, ground transitions, vegetation silhouettes, water/banks, and shadows; use new assets or rendering where needed | Actual Unity and Chrome output is strong in both normal and close play, before spending time on broad UI polish |
| 3 — Rebuild the full location | Apply the chosen visual grammar across the same AOI; compose varied canopy groups, openings, edges, cultivated patterns, water details, and distant context; remove obsolete rendering paths | Quality holds while panning/zooming, at region boundaries, and outside the single hero frame |
| 4 — Make transformation beautiful | Re-author the visual treatment and reveal of existing land interventions; make baseline, proposed, and established land distinct and satisfying | The change has clear visual causality and richness at normal play scale, while remaining honest about hypothetical outcomes |
| 5 — Fit the interface to the world | Keep or rebuild only the HUD, tools, text, and controls the new composition needs; preserve reachable actions and honest measures | The UI frames the world without dominating it; it no longer dictates the art direction |
| 6 — Validate and finish | Compare matching reference/game states, measure browser costs, fix conspicuous weaknesses, publish and capture the real build | All visual dimensions and delivery safeguards below pass; remaining differences are specific and honestly stated |

These are work slices for the current agent, not authorization to spawn agents or start unrelated phases. Do not wait for a perfect research catalogue before trying the best available direction in-engine. Equally, do not import an entire pack and declare the art solved.

### Avoid another incremental-polish loop

Before each study, name the visible failure and the alternative being tested. A new texture, denser scatter, or slightly different light on the same composition is an iteration within one direction, **not a materially different art direction**. If two successive iterations leave the dominant failure intact, stop tuning that approach and test a substantial alternative: different scene framing, asset construction, surface treatment, scale hierarchy, or rendering technique. Keep promising pieces only after the whole image benefits.

The immediate milestone is a compelling playable forest–field–water scene, viewed beside the reference at comparable displayed size. If an existing capability such as unrestricted camera orbit prevents a coherent visual result, replace it with a simpler deliberate design and verify that the whole playable location remains usable. Do not preserve a feature at the expense of the image merely because it was already implemented. Do not spend a run completing secondary checklists while that scene is still clearly weak. Record abandoned approaches and why they failed so future goal loops do not repeat them.

## The visual acceptance gate

The object of judgment is the **actual playable game in Chrome**, at normal play scale and during camera movement. Capture 1280×720, 1440×900, and a 1920×1080 layout check. Use comparable displayed sizes and explain differences in zoom and scene scale.

Evaluate the world with minimal UI as a diagnostic, then evaluate the complete game with UI present. A beautiful UI-hidden render cannot conceal a weak playable composition; a good HUD cannot raise the landscape score.

| Dimension | Required result |
|---|---|
| Composition, scale, and depth | Deliberate foreground/middle/background, compelling play framing, readable relief and landmarks, no accidental distant slab or empty surround |
| Vegetation and environmental forms | Convincing varied silhouettes, layered canopy/understory/edges and natural grouping; no obvious repeated-stock-mesh carpet |
| Ground, fields, water, and transitions | Coherent painterly/stylized material language; rich close views; grounded contact; intentional cover transitions and believable water/bank relationships |
| Lighting, color, and atmosphere | Controlled values, attractive hue relationships, useful shadows, depth and separation; no muddy flat wash or indiscriminate effects |
| Intervention appearance and transformation | Beautiful, legible changes that belong to the same world; no placeholder stripes or flat color patches standing in for finished treatments |
| Whole-scene consistency | Quality survives wide/close views, panning, alternate sites, treated/untreated states, and the full HUD; no single-camera showcase trick |

Anchors: **1** broken; **2** prototype; **3** coherent but a substantial reference gap remains; **4** convincing authored game with limited gaps; **5** no material gap for this bounded slice against the selected frames. Half scores need specific reasons.

**Every visual dimension must reach at least 4.5/5.** No average, UX score, completed task count, or performance headroom can compensate for a weaker visual category. Record at least two reviews across different scenes/states, actual matched captures, the strongest remaining criticism, and how each dominant baseline failure was resolved. Ratings are internal judgments, not external research or a mathematical proof of parity.

**A known substantial visual gap is a failed gate.** The user's rejection cannot be waived because novice testers are unavailable. Do not describe the old direction as accepted, nearly equivalent, or complete. If a comparison still looks materially inferior, return to composition/assets/materials rather than rescore it upward. Present the rebuilt game and its comparisons visibly to the user; any further visual rejection reopens the affected work.

## Supporting safeguards, after the visual direction is proven

These remain necessary product safeguards, but they are not the purpose of the phase:

- Preserve a playable choose → preview → transform → undo path, or document an intentional improved replacement. Test retained mechanics; update tests when behavior is deliberately replaced rather than preserving obsolete implementation to satisfy them.
- Keep essential controls legible, keyboard-reachable, and usable at 125% browser zoom/larger text; no blocked exits, click-through, false feedback, or silent accounting corruption. Honest unavailable model labels and source/assumption disclosure remain accessible.
- Keep the existing browser budget as an engineering target: three 30-second foreground paths, p95 ≤16.7 ms target or documented stable-30 p95 ≤33.3 ms fallback; preview presentation-opportunity p95 ≤100 ms, no placement stall >200 ms; three fresh-cache loads usable ≤15 seconds and initial compressed transfer <50 MB. Record p99/max/long frames and what latency instrumentation actually measures. Optimize the chosen visual result without silently degrading it below the art gate.
- Verify the current-source public export anonymously in Chrome, with exact source/pack/export identity and no blocking missing assets/shaders/errors. Preserve a recoverable working build.
- External novice usability testing may remain explicitly pending. That exception applies to unavailable participant research, not to known visual inadequacy.

The previous detailed interaction checklist remains available in the [superseded brief](evidence/phase2a/superseded-ui-first-goal.md) as a regression aid. Its order, fixed layout, keep-everything assumptions, and mixed UX/visual scoring no longer govern the goal.

## Evidence and completion

Maintain `docs/evidence/phase2a/visual-rebuild/` with the rejected baseline, art studies and selection rationale, resource/license manifest, actual wide/normal/close/transformed captures, two reference comparison reviews, 45–90 seconds of actual browser interaction, delivery/performance checks, and remaining limitations. Preserve earlier evidence as historical, not final acceptance.

Update [art-direction.md](art-direction.md) to the new accepted art language only after implementation supports it. Surface the final reference/capture index in README, AGENTS, the implementation plan, status ledger, and future handoffs. Remove superseded implementation prescriptions that would drag later work back to the prototype.

The phase is complete only when the rebuilt visuals meet the gate and the real browser product works. Stop before Phase 3. Do not claim user-validated or commercial-game parity without evidence.

## Copyable revised goal prompt

```text
Complete the rewritten Phase 2A visual rebuild in docs/PHASE_2A_GOAL.md. The user has rejected the current visual quality and authorized replacing entire aspects of the early implementation. Start from the desired player experience and Terra Nil's actual visual quality, not from preserving our renderer, assets, camera, UI, or previous work. Stop generating images for this pass. Research and evaluate coherent existing game-asset families with their intended materials, shaders, and demo workflows; obtain suitable licensed resources and test their actual quality in-engine. Existing generated artwork and camera compromises are disposable. Build materially different visual studies, select a coherent direction, and rebuild the same rural location around it. Prioritize composition, environmental forms, painterly/stylized ground and foliage, lighting, water, and beautiful transformations. UI and regression checklists support the result; they cannot substitute for it. Do not call the phase complete while a substantial visual gap remains. Verify and show actual Chrome gameplay, matched reference comparisons, source/asset provenance, and browser performance. Preserve the educational purpose and scientific honesty, record intentional replacements, and stop before Phase 3.
```
