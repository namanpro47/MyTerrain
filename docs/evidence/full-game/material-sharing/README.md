# Shared vegetation materials, isolated candidate

October 25, 2026. Source base `a14f339` combines main through `8530aaf` and visual road checkpoint `7d4cdcc`. This change is confined to InstancedNature's material construction, ownership and lifecycle tinting; the visual owner retains grass construction. Patterns TN-02/05/12 guide preservation of vegetation and transformation appearance. Phase 2A remains unaccepted.

## Change and measured result

The opening scene previously allocated **1,566** material objects for **666** InstancedNature components. It now uses **14**, a reduction of **1,552** (99.1%), with the same 14 content groups. The observed opening submission/culling counts remain 753/1,388. This is a material-object reduction, not a claim about total memory, draw-call reduction or Chrome frame rate.

Immutable material instances are shared by source asset identity and the exact effective tint, soft-cover texture and plant bounds. No CRC is used as a cache key. Each component holds one lease per shared material regardless of repeated Add calls or LOD reuse. A nonzero lifecycle tint copies that site's colored materials and remaps all its near/medium/distant batch references before mutation. Later tint changes still blend from the original color and preserve alpha. The final borrower releases the shared material; private copies are released by their owning component. Resource materials are immutable inputs, not mutated cache entries.

## Verification

Unity 6000.6.6f1, CLI 1.0.0-beta.14; isolated batch Editor, initialized TerraMain scene, advancing Play frames.

- `compile.json`: zero errors/warnings.
- `before.json` / `after.json`: 1,566 → 14 unique submitted-material references, same component and batch submission counts. Their early aggregate digests are **not** the equality proof: equal GameObject names and multiple components made their traversal order ambiguous.
- `appearance.cs` fixes the inspection identity with sibling indices and component ordinals. It enumerates all shader property values, texture asset paths/tiling, keywords, queue, instancing, batch transforms, mesh metadata, bounds and LOD parameters. The original construction code was restored temporarily in this isolated checkout for the control, then the shared implementation restored. `appearance-comparison.json` reports **203,931 unique records, zero missing keys and zero differing values**. Both canonical streams have SHA256 `416f1ce58a68a0c1a4bc27278adb02c29ce5b8fdad04bf1849a17ff2da4dc419`; the common stream is preserved in `matched-appearance.txt.gz`. Mesh geometry source and draw logic were unchanged; this is not a pixel comparison or a Chrome review.
- `lifecycle.cs` / `.json`: **28** focused assertions covering shared leases, repeated Add, no-op tint, neighbor isolation, alpha, noncumulative/status changes, restoration, later sites, source identity, exact near-equal color keys, soft-cover bounds, disable/destroy, Add after tint, distinct LOD materials and cleanup. Color read/write storage differs by under 1e-7 in the synthetic fixture; RGB comparison uses 1e-6 tolerance while alpha and source CRC remain exact. The first exact-float assertion rejected this harmless storage round-trip; its observed actual/expected values remain in the successful receipt.
- `model.json`: **678** Atlas/model/save assertions and **34** annual lifecycle/JSON checks.
- `annual-terrain.json`: all 31 prior terrain/annual invariants plus an Editor timing observation; same exact undo/restoration, connected swales, rotation cover, accepted save replay and read-only comparison. Its timing is not browser latency.
- `console.json`: no errors; `final-state.json`: original Year 1/Planning, revision 0, $10M, no queued/accepted projects. No scene file saved.

A transient reconnect after recompilation prevented one control Play request from reaching the Editor; the Play guard rejected that inspection. After confirming the same Editor ready/stopped, the control was restarted and its advancing frames verified. The first property-inspection script also had a local variable-name compile error; it was corrected before producing the matched evidence. Neither attempt counts as a pass.

## Remaining acceptance

The changed shared-material implementation still needs a combined Web export and Chrome visual/frame-time verification with the visual owner's grass checkpoint. Whole-scene quality, final public export/run, two rehearsals and labelled recorded fallback remain open. No public deployment, final acceptance, total-memory or FPS claim is made here.

## Combined grass checkpoint

The subsequent merge with visual `48154c4` retains **414** InstancedNature components and **15** unique material objects (`combined-inventory.json`), with687 submitted /1,236 culled batches in the opening Editor view. The new grass construction adds one material configuration; this is not an old/new frame-time comparison. The grass root fixture was rejected before execution because its archived `.cs.txt` filename is not accepted by eval_file (`combined-grass.json`); no new root-placement pass is claimed. The owner’s source-matched placement receipt remains the placement evidence. Combined Chrome validation is next.
