# Terra visual rebuild — target, experiments, and resources

**Active direction, October 24, 2026.** Read the [rewritten Phase 2A goal](PHASE_2A_GOAL.md). The user rejected the current appearance and authorized replacing existing work. This document guides the rebuild; it does not claim that the target has been implemented.

**User correction: established assets first.** Stop generating images for this pass. The generated canopy/understory route consumed effort without proving whole-scene quality and exposed flat-card limitations. Its fixed-camera workaround is provisional. Evaluate artist-authored environment families using their supplied materials/shaders and demo composition before writing substitute shaders or compensating for inadequate assets. Choose on actual normal/close/moving views in Terra, not catalogue polish, pack size, price, or familiarity. A successful import is not visual acceptance.

## The intended visual experience

An inviting, painterly rural landscape that is worth looking around before touching a tool. Woodland has soft, varied crowns and layered edges; fields read as cultivated land with convincing surface rhythms; water has a distinct bed, bank, surface, and relationship to surrounding vegetation. Warm sunlight and cool depth separate forms without washing everything into the same sage color. Interventions visibly belong to this world and improve or change its character.

The play camera should reveal a landscape, not automatically display a one-kilometer data rectangle. A closer, landscape-filling view with readable foreground, middle distance, and wooded slopes is a leading hypothesis. A full-location overview can remain useful without being the mandatory opening or permanent play scale. This is one AOI viewed differently, not a second location.

The precise assets and rendering technique are open. Replacing the current faceted asset family, terrain/cover renderer, water ribbons, floating earth slab, lighting rig, and camera is preferable to defending their sunk cost.

## Primary research and reference frames

### VR-01 — Terra Nil's painterly direction

The developer's [commercial-development announcement](https://vfqd.itch.io/terra-nil/devlog/188191/the-future-of-terra-nil) explicitly describes a new painterly direction inspired by Studio Ghibli. This is evidence about the intended art style, not a specification of its shaders.

**Implication for Terra:** evaluate painted surface/foliage treatment and shape design together. More copies of a flat-shaded tree mesh will not produce that result. The existing TN-01/02/05 patterns still apply, with direct visual quality now dominant.

### VR-02 — Actual vegetation production

Contributing artist Astrid Bruce's [Terra Nil willow-tree portfolio](https://astridbruce.artstation.com/projects/zPEND2) describes painted textures, game-ready assets, and placement in Unity.

**Implication:** investigate a coherent textured vegetation family and authored placement. Do not assert that every Terra Nil asset uses this exact technique; the portfolio documents this contribution.

### VR-03 — Official scene references

[Free Lives press kit](https://freelives.net/terra-nil-press-kit/) supplies official scene images. These complement the dated desktop HUD frame already in [DESIGN_REFERENCE.md](DESIGN_REFERENCE.md#visual-reference-board). Press images establish visual appearance; they do not prove current runtime performance or mechanics.

![Terra Nil tropical foliage and water quality reference, Free Lives](https://freelives.net/uploads/2024/04/Terra-Nil-Screen-2.webp)

![Terra Nil bank, ground and restored-edge quality reference, Free Lives](https://freelives.net/uploads/2024/04/Terra-Nil-Screen-3.webp)

Inspected in Chrome on October 24: Screen 2 is a tropical scene with overlapping textured crowns, detailed shoreline growth, visible underwater forms and layered water. Screen 3 shows barren/restored ground with materially distinct surfaces and irregular, planted banks. These establish surface/edge richness; tropical species, coral, stepped terrain and machinery are not proposed features for Wisconsin. Screen 1 is a handbook view, so it is not used as the landscape target.

Use these as attributed comparison images only, never as Terra's shipped textures, models, or screenshots. Record which frame and displayed scale each study addresses.

## Rejected baseline

The earlier 2A iteration is useful evidence of what is insufficient:

![Rejected Phase 2A wide view](evidence/phase2a/captures/1280-default.png)

![Rejected Phase 2A close view](evidence/phase2a/captures/1280-close.png)

These captures predate the user's reset and are not accepted art targets. The world still depends on repeated faceted canopy, coarse visible cover blocks, sparse fields, thin stream ribbons, and a distant floating map composition. A clean HUD and fast frame time do not remedy those shortcomings. Some boundary rendering experiments also remain unfinished; finishing that micro-detail is no longer the priority.

## Studies to build before committing to a replacement

| Study | Distinct hypothesis | Must demonstrate | Reject if |
|---|---|---|---|
| A — Painterly valley at play scale | A closer elevated camera, textured organic foliage, continuous ground treatment, strong forest–field–creek composition | Readable individual canopy groups and land interventions; layered depth; landscape fills the play frame | It is merely the old board enlarged, or looks like unrelated store assets |
| B — Authored miniature landscape | A carefully framed overview with deliberately designed perimeter, canopy masses, field patches and water detail | A strong whole-place silhouette and rich miniature quality at useful gameplay scale | The AOI shrinks back into an empty surround or relies on a showcase-only camera |
| C — More spatial rural view, if needed | A restrained perspective/oblique view that gives slopes, banks, and vegetation more presence | Strong spatial depth with clear targeting and controllable navigation | Foreground clutter hides decisions or the browser cost is disproportionate |

Build at least two materially different studies, then select using actual rendered comparisons. An art study may use a section of the same location and minimal temporary UI. Do not build additional authored levels. Record camera/lens, material family, light rig, assets and dominant weaknesses so the selection is reproducible.

## Replacement architecture: hypotheses to test

- Separate analytical geographic data from the visual surface. Preserve source identity and relative geography while replacing raw cell colors with art-directed transitions and detail. Explicitly label visual interpolation and decoration.
- Use a coherent textured foliage family with varied silhouettes, branch/crown structure, meaningful clearings, understory, and edge composition. Inspect leaf opacity, normals, texture filtering, and shadows in URP/Web rather than trusting a marketing render.
- Design ground layers for near, middle, and wide scales: broad value/color masses, field rhythms and transitions, then restrained fine detail. Avoid uniform high-frequency noise and repetitive tiny marks across every surface.
- Rebuild water as an integrated landscape feature: mapped route plus illustrative bed/bank geometry, surface shading, vegetation contact, and restrained motion. No fake hydrological claim follows from better-looking water.
- Author lighting and atmosphere as part of composition. Test shadows, ambient separation, color grading and distance depth together; keep the result clear during play. A post-process filter alone is not an art direction.
- Design interventions as complete land treatments, including edges and their visual transition, rather than a flat patch plus repeated object rows.
- Select LOD, batching/instancing, texture resolution and draw distance after measuring the representative visual scene. Do not begin by reducing the target to the old renderer's capabilities.

These are design/technical hypotheses, not findings about the reference game's implementation. Consult applicable Unity skills and installed URP documentation when implementing them.

## Resource register

Research date: October 24, 2026. **User budget: free resources only; Quaternius Source purchase declined.** Paid entries below are historical research, not planned acquisitions. Archive contents and real import results take precedence over catalogue claims.

| Resource | Why inspect it | Rights / acquisition | Status |
|---|---|---|---|
| [Quaternius Stylized Nature MegaKit](https://quaternius.com/packs/stylizednaturemegakit.html) / [creator's download page](https://quaternius.itch.io/stylized-nature-megakit) | Textured, varied foliage/plant/rock family; substantially different from the current faceted kit direction | Creator lists CC0. Free Standard tier; paid Source tier adds engine integration/shaders. Do not assume free models include those shaders | Downloaded and archive inspected: 68 models, 20 source textures, CC0 license included; eleven-model subset imported and rendered in Chrome studies; no final acceptance. [Acquisition record](evidence/phase2a/visual-rebuild/assets.json) |
| [Quaternius Ultimate Stylized Nature Pack](https://quaternius.com/packs/ultimatestylizednature.html) | Alternative textured nature family and seamless material sources | Creator lists CC0 and FBX/OBJ/glTF/Blend formats | Researched candidate; not downloaded or accepted |
| [Free Stylized Nature Environment, Ink Phantom](https://marketplace.unity.com/packages/3d/environments/free-stylized-nature-environment-96371) | Alternative tree/grass/flower treatment to compare | Store lists free under Asset Store EULA; old original Unity version means conversion must be evaluated | Researched candidate; not acquired or accepted |
| [Kenney Nature Kit](https://kenney.nl/assets/nature-kit) | Existing asset baseline and potential secondary props | CC0; already present with license | Current dominant canopy treatment rejected; no obligation to keep it |
| [Elijah Cobden Stylized Nature Pack](https://elijahcobden.itch.io/stylized-nature-pack) | Coherent foliage, wheat, rocks, supplied Shader Graph materials and small forest demo; evaluate the complete artist setup | Name-your-price download; creator page lists CC BY 4.0, so preserve attribution and modification notices | Version 1.1 downloaded and unpacked outside Assets: 30 FBX, 37 prefabs, 20 materials, 3 shader graphs, one demo scene. No executable scripts in archive. Old URP 2020 setup needs Unity 6 verification; not imported or accepted |
| [Quaternius MegaKit Source tier](https://quaternius.itch.io/stylized-nature-megakit/purchase) | Re-evaluate the existing model family using the creator's engine setup rather than our substitute materials | Listed at $14.99 on research date; not purchased | Candidate comparison. Current free Standard import does not test this complete setup |
| [LMHPOLY Stylized Poly Nature Environment](https://www.lmhpoly.com/game-assets/stylized-poly-nature-environment) | Integrated trees, plants, ground, water, LODs, URP demo lighting and materials | Listed at $49.99 on research date; not purchased | Candidate coherent environment workflow; creator lists Unity 6 support. Catalogue quality is not proof of Terra Nil fit or browser performance |

For each acquired package record source URL, tier/version, download date, SHA-256, included license, file/texture/model inventory, and selected imports in `evidence/phase2a/visual-rebuild/assets.json`. Stage archives outside `Assets/` so evaluation does not bloat the shipped build. Import only the coherent selected subset. Retain required attribution/license files.

Current actual output and strongest criticisms: [visual study evidence](evidence/phase2a/visual-rebuild/README.md). The first study was rejected; later studies remain in progress. An [original generated meadow texture](../Assets/Terra/Resources/MeadowPaint.png) is now tested in the terrain material; [exact prompt and provenance](evidence/phase2a/visual-rebuild/generated-meadow-prompt.md).

### Original foliage experiment

[PaintedTrees.png](../Assets/Terra/Resources/PaintedTrees.png) is an original four-tree transparent atlas made with the built-in image-generation tool; the [exact prompt and provenance](evidence/phase2a/visual-rebuild/generated-trees-prompt.md) are retained. It tests painted silhouettes on terrain with 3D shadow proxies, a materially different construction from the stock mesh forest. Study 07 revealed a missing player rendering variant, fixed and verified in Chrome in study 08. That study also exposed unacceptable flattening during steep camera tilt. Study 09 tests a deliberate fixed illustrated camera with pan/zoom, replacing free orbit, and adds [matching original understory](../Assets/Terra/Resources/PaintedUnderstory.png) with [recorded prompt/provenance](evidence/phase2a/visual-rebuild/generated-understory-prompt.md). The [study ledger](evidence/phase2a/visual-rebuild/README.md) records the export repair and actual comparison. No reference-game artwork is imported into the game.

## Iteration discipline

### Next study: evaluate supplied art before adapting it

Use the downloaded Elijah Cobden family as a bounded evaluation, not a selected production direction. Inspect the supplied forest demo and materials in Unity; retain its intended foliage and lighting relationships during the first render. Check the silhouette from Terra's elevated normal/close views: the tall cylindrical birch crowns in the creator's screenshots may be unsuitable even if they look good from ground level. Then test a small forest–field–water section of the existing AOI using the same family, including its wheat. Compare against TN-01/02/09–12 and official Terra Nil frames. Reject early if the complete image remains generic or incompatible; do not spend several iterations rescuing it with generated art. Evaluate Quaternius's intended source setup or a more complete environment kit if this family fails. No candidate is accepted yet.

Start each visual review with the strongest criticism of the current image. Compare matching reference and game states at comparable size. Choose the highest-impact change: composition, asset family, material language, lighting, or transformation. Keep an experimental branch/checkpoint so replacing work is inexpensive. Do not repeat tiny polish passes when the whole visual approach is wrong.

Actual in-engine/Chrome studies and a resource/import record now exist, but remain below the bar. The next milestone is a coherent, materially stronger landscape and treatment sequence across playable views. The final deliverable is the rebuilt playable location, not this document, a concept image, an asset download, or a favourable self-score.
