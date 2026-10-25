# Selective terrain integration and swale query cost

2026-10-25. Isolated source `52856ea` combines visual checkpoint `a71bcc4` and main `fd35f8c`. No accepted landscape or public release is claimed. Unity6000.6.6f1, own advancing batch Editor, concurrent visual-owner Editor; measurements are local diagnostic samples, not Chrome latency percentiles.

## Annual/rendering integration

`annual-terrain-check.cs` exercises actual generated ground with an isolated annual model and restores the real scene in `finally`. The32 result records include terrain/state assertions and one timing observation. Both before/after receipts pass:592 lowered swale vertices, maximum1.149989m depth,309 coincident chunk-edge vertices with matching positions/normals, exact undo/cancel restoration, valid replacement/removal previews, rotation bands with1,281 groundcover instances, accepted Year10 save replay, unchanged ledger and read-only comparison/return. Full source-world JSON and the live annual session remain unchanged. `model-checks.json` additionally passes678Atlas/save and34annual lifecycle checks on the combined source after optimization.

## Measured cost and narrow fix

`preview-profile.cs` times actual Ghost construction, an explicit ground-only refresh, the ordinary surface+ground flush, and cancellation for south07/02/08. The ground-only call is diagnostic; it intentionally causes a second refresh, so do not sum all four timings as one user interaction.

| Site | Ghost before → after | Ground refresh before → after | Surface+ground before → after |
|---|---:|---:|---:|
| south-07 |89.9→73.2ms|40.4→7.7ms|41.5→9.9ms|
| south-02 |163.1→129.3ms|57.8→8.7ms|63.1→10.9ms|
| south-08 |149.8→125.4ms|60.6→8.2ms|58.0→10.9ms|

The separate whole integration fixture reports south07 preview+flush124.3→80.0ms. Larger previews still exceed100ms in these Editor samples: vegetation construction remains material and the browser gate stays open.

The swale query now rejects points outside the route's expanded bounds, skips segments whose bounding box cannot improve the nearest distance, and evaluates the existing basin formula with squared distance. Plant placement still takes the square root of the nearest route distance; source assets, shape, depth, plant density and random sequence are unchanged. The pre-change Swales file is identical on main and the visual branch, so this narrow query change can be integrated independently of unaccepted scenery.

`swale-equivalence.cs` compares the new private query against the original route-distance/profile formulas at78,000 seeded samples across all13sites, including positive basin samples and points outside every parcel. Maximum cut error0.0000002384m, gradient error0.0000002235 and distance error0.0000076294m. This is bounded floating-point equivalence, not byte-identical geometry or exhaustive proof. The post-change actual-ground annual test separately verifies seams and exact restoration.

Remaining: combined visual quality, actual Chrome preview/placement and frame/load checks, final export/publication and rehearsals. The ground/grass/field refinement remains with its visual owner.

## Independent main integration

Only the swale query fix was cherry-picked as `ed2055b` onto main `fd35f8c`; evidence follows as `aff7e3b`. The visual candidate, terrain cache, materials and Surface optimization are not part of this main integration. The same pre-change Swales file was verified identical on both bases.

On `aff7e3b`, the actual existing `annual-terrain-integration-check.cs` passes all31 result records (`main-annual-terrain.json`), covering592 lowered vertices,309 matching shared edges, undo/cancel, Year10 save replay and read-only comparison. `main-compile.json` reports zero errors/warnings and up-to-date compilation after Editor startup; `main-state.json` records an advancing player frame and original Year1/Planning state with $10M and no proposals/assets. The main-only preview+flush sample is111.35ms: the candidate's80ms result cannot be transferred to main, which does not have the display-height cache. No Chrome or accepted performance claim follows from either Editor sample.
