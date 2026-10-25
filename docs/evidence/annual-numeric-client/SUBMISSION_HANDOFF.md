# Submission handoff — October 25, 2026

The user requested a clean stopping point within 30 minutes at approximately 20:05 UTC. The full implementation plan is **not complete**. This checkpoint preserves completed source work and records the remaining acceptance gaps without activating an unverified runtime.

## Pushed work

- `codex/annual-numeric-client`: exact numeric JSON codec, actual Unity/IL2CPP sparse evidence, dense OOM evidence, cancellation-aware frame checkpoints, and removal of a repeated initialization replay. Memory candidate `22a68bd`; stronger checkpoint regression `d021fdc`.
- `codex/numeric-directory-browser`: atomic retained-run directory/controller, immutable definitions, acknowledged browser adapter, v2 IndexedDB bridge and optional validator checkpoints. Adapter `2cf446b`, reconciled branch `b1282a9`.
- Both branches retain incoming main work. Numeric-client is reconciled through main `615459a`; directory through `ef874af`. The later main change only corrects the separate chapter receipt comparison and records evidence.

No Main/HUD activation, public export replacement, force push or deployment is part of this wrap-up. The integration branches remain inactive and are not submission-runtime acceptance.

## Verified

- 78 numeric transaction assertions; 40 adapter assertions.
- 149 pure directory assertions; 81 modeled IndexedDB checks; 33 per-run bridge fault checks.
- Installed Unity 6000.6.6f1 API compilation passes with zero warnings/errors, including the WebGL checkpoint and directory adapter.
- Prior actual Unity Editor: 23 checks pass. Prior sparse IL2CPP/Chrome: 8 fresh-run checks and 4 page-reload checks pass with exact saved/report hashes.
- Prior dense 340-site, ten-year WebGL fixture exhausted memory. The new frame-checkpoint mitigation is source-tested; its rebuilt dense-player retry is pending at this checkpoint. See the latest README entry for the final queue/retry outcome.

## Still incomplete

1. Dense WebGL memory acceptance, then linked directory-v2 create/select/reload/fault acceptance.
2. Numeric schema-3 gameplay wiring and reconciliation with schema-2/v5 debt, cancellation, regional events and pending annual-feedback planning/conversion work. Preserve the existing annual authority and prior saved runs; do not silently discard new rules.
3. Phase 2A visual acceptance and combined foreground performance/load budgets. Correct state/accounting does not close the visual gate. Human novice observation remains unavailable, not passed.
4. Final public release walkthrough/rehearsal evidence belongs to the separate acceptance owner; use its final committed report, not source-only assumptions.

## Submission runtime

Release owner confirmed the frozen public runtime `19e7eae`, release evidence `ce27355`, at https://terra-browser.vercel.app. Newer source commits are not automatically that deployed export. Custom-domain AI chapter requests have a backend-origin limitation; the local summary remains available. The public-playtest owner reached Year 10 during wrap-up; final replay/rehearsal results must be read from that owner's evidence.

## Resume safely

Read [integration boundaries](INTEGRATION.md), [runtime evidence](README.md), and the directory branch's `docs/evidence/annual-numeric-directory-browser/README.md`. Fetch current main, inspect the coordination board, and claim the relevant paths before new work. Rebuild the isolated numeric harness before testing the memory candidate; preserve the recorded failing baseline. Ship both v2 browser bridges together only after linked acceptance. No additional asset purchase or image generation is required.
