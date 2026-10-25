# Current visual branch — combined-main checkpoint

Owner: 01a16fec-59df-7ee2-85b2-c0f846d0b786. Branch `codex/visual-field70`; previous pushed tip `5279dd5`. This handoff's enclosing commit is the bounded new tip. Current remote main fetched and reviewed at `ea37af047f4d8d167f88b802e76827c82a6c8183`. User's latest all-branches/main request is confirmed in the audio chat's human messages; the existing atlas release owner holds integration74. This chat makes no competing main push or deployment.

## Include the branch history, reconcile runtime deliberately

- `codex/visual-study20` at `dc3a2e8` is an ancestor of this branch. Its runtime and preceding renderer/art dependencies are therefore included by reconciling this tip; do not separately replay overlapping commits blindly.
- Last production runtime change on this branch: `3b28015` (study65 fixed Wisconsin homes). Last exported and Chrome-tested visual source: `e3f4905` (Web64). These remain separate facts; no final combined export or visual acceptance is claimed.
- `449341a` / `1f2eba3`: study70 actual crop-volume evidence and visible review. Useful massing direction, rejected filtering/coverage and close geometry; **no production runtime change**.
- `2fb4c49` / `5279dd5`: study71 nine actual Editor captures, source-normal validation, shader checks and visible review. Full normals speckle; softened normals remain flat. **Rejected as finished scenery; no runtime change**.
- Study72: prepared scripts/source preflight and this handoff only. **Not executed**, no new active feature or model.

All tracked finished evidence belongs on main with the reconciled history. Preserving rejected comparisons does not require activating their rejected runtime experiments. Large imported study folders remain untracked and unused by production. The archived scripts/shaders, source acquisition/license records and hashes preserve the experiments; do not sweep unrelated imported packs, generated arrays or longstanding local dirt into the release.

## Concrete reconciliation required with current main

Main `e80ce36` introduced `LandscapeRenderer.StartingSettlement.cs`: `BuildStartingSettlement(UniformParcelPack)` instantiates each pack's `initialBuildings` in its baseline. Main's `TerraMain.ShapeLandscape` calls it after `landscape.Build()` and applies `UniformParcels` beforehand.

This visual branch independently calls `BuildExistingCommunity()` from `LandscapeRenderer.Build()` and adds four fixed Wisconsin homes in `LandscapeRenderer.Community.cs`. **Do not retain both active startup calls.** Preserve the current map-driven initialBuildings as the authoritative inherited community. During the combined merge remove/supersede the older fixed-four-home startup invocation, retaining any useful grounding helpers only if explicitly adapted to map-driven buildings. Do not overwrite newer parcel definitions or annual baseline capacities with this branch's older assumptions.

This also changes validation scope: the historical study65 check's expectation of exactly four legacy homes is obsolete for the new pack-driven scene. Do not edit current behavior just to pass that historical count. Run main's `ParcelSceneChecks`/map-parcel checks and verify initial building count and identity against each current pack, no duplicate legacy home objects, ground contact, preview/remove/undo and accepted-year/save/reload preservation. Any reused foundation/driveway/garden treatment needs new current-map captures.

Preserve newer native MyTerrain atlas, dense parcel selection, zoom changes, annual/client/hosted chapters, Field Notes, placement and audio changes. This old visual worktree is not a replacement snapshot of current main. Reconcile conflicts rather than copying all files over the release checkout.

## Verification already complete and remaining

Study71: all48 raw normal slices exactly match color alpha masks; 1,299,243 covered pixels retain unit normals within8-bit precision. Both candidate shaders supported with zero messages; source-world JSON and live annual diagnostic unchanged. Actual normal/close/wide renders at three normal weights are archived. Play stopped, scene clean, zero study objects; own Editor71190/launcher71187 confirmed absent. Study72 never opened an Editor. Reservation78 cancelled, no resource held.

The collector15 and material-ownership28 test paths remain in [integration checks](../study70-field/INTEGRATION_CHECKS.md). The community35 historical test is superseded where it asserts fixed-four-home composition, as above. Run affected checks in the actual combined source and validate its frozen Chrome export. Latest prior bounded performance remains failing at34/36ms p95 versus33.3ms. Whole-scene Terra Nil quality, final motion/three-viewports, public export and performance gates remain open. Shipping current work does not certify Phase2A.
