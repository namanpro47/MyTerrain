# Exact visual-runtime checks for combined release

Requested by the release owner October25. These are the existing checked-in entrypoints from the frozen visual checkpoint; no study70 runtime adoption is required.

Run against the release owner's explicit project path, under its own Editor reservation. Start Play, enter Wisconsin through the new atlas, verify TerraMain is initialized and frames advance. Preserve the active save. All three scripts below require Play. Copy `.cs.txt` to an ignored `.cs` path before invoking the CLI.

1. **Community / study65:** `docs/evidence/phase2a/visual-rebuild/study65-check.cs.txt` → e.g. `Builds/Verification/study65-check.cs`. Prior expected35checks. It checks four homes, source-cover/water/site exclusions, owned garden meshes, unchanged community through preview/apply/remove, and a separate temporary no-action Year10 display followed by RefreshLandscape. It asserts unchanged live diagnostic and source JSON. This does not actually submit or save a year.
2. **Collector:** `docs/evidence/phase2a/submission-cells-01/checks-192.cs`. Prior expected15checks. It sets ComparisonCellSize192 and checks batching identity, transforms, bounds,500capacity, clearing and disable behavior in a temporary fixture. This is the production candidate's expected grouping, not the unpromoted32m crop grouping experiment.
3. **Material ownership:** `docs/evidence/full-game/material-sharing/lifecycle.cs`. Prior expected28checks. It creates temporary synthetic fixture materials/meshes, exercises shared leases and lifecycle tint isolation, destroys its fixture and checks original cache/state. `appearance.cs` is a historical before/after comparator, not a required standalone pass against changed scene content.

Invocation shape (substitute actual reservation/project/script paths):

```sh
python3 scripts/terra-work.py run RESERVATION -- unity command script eval_file --caller plugin --skill unity-cli --project-path /absolute/release/project --format json -- --file /absolute/release/project/Builds/Verification/study65-check.cs
```

Inspect the inner `data.result.success` and check count, not just CLI exit0. Afterward read console/scene state and stop Play. Existing compilation, annual, atlas, and full-game checks remain release-owner scope. These narrow checks cannot establish browser performance, new audio/guidance compatibility, source/export identity, or visual acceptance. The unfixed early-return warning in production TerraGround is separate from the study69 experimental shader fix; no automatic claim that production warning is cleared.
