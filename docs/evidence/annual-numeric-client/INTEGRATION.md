# Numeric integration handoff after the frozen combined release

The current game remains on its validated schema-2 annual path. This document records the concrete wiring needed after the standalone numeric codec/Unity/WebGL checks pass; it is not activation or an acceptance claim. Reviewed against combined candidate770d4c0, reconciled locally in7b7a004. Preserve other owners' later placement, community, Field Notes, audio and atlas changes.

## Required boundary changes

- `TerraMain.Editing.InitializeAnnual` currently constructs schema2, loads PlayerPrefs and attaches `AnnualChapterSession`. Numeric initialization must build schema3 using `AnnualSimulationFactory.Create(..., baselineWorld: world)` with the actual loaded dense source world and pinned weather inputs, then await `InitializeNumericAsync`. The exact numeric definition/save codec is `AnnualNumericJson`. Do not route schema3 through old `Initialize`, `SaveAnnual`, or the chapter snapshot transport.
- Add an acknowledged per-place active-run directory/new-run selection. `AnnualNumericBrowserStore` currently addresses one explicit definition/run key only; it does not discover the current run or atomically select a replay run. Keep previous runs and all legacy keys. A failed read must not be treated as absent storage or a fresh game.
- Placement, move, remove, undo, redo, continue and fast-forward must await their session transactions. UI busy state must cover a pending save even while the last accepted phase remains Planning. Publication follows acknowledgement; failures require recovery. A repeated End Year uses its frozen request ID/revision. Planning recovery may reveal an already-written command; refresh saved state before offering another edit.
- `AnnualGameplay.Forecast` is called by `Refresh`, End Year confirmation and future confirmation. Numeric unplayed exact forecasts are intentionally rejected. Use accepted/current outcomes for actual meters and `AnnualSimulation.PlanningRange` for certain reservations and explicitly conditional bounds. Do not compute hidden future weather or relabel old exact forecasts as numeric predictions.
- `AnnualChapterSession`, `TerraMain.Chapters`, the overview and Field Notes currently consume v1 chapter receipts. Numeric transport sends only the saved canonical `ReportNarrationRequest` to report-v2, validates the response, and awaits `AcceptNumericNarrationAsync`. Known provider failures use `UseLocalNumericNarrationAsync`; missing canonical projection uses `PrepareNumericNarrationAsync`. Numeric acceptance may already be durable while narration is unprojected/pending. The first visible saved mode remains immutable; late replies may complete an older report without moving the current year/view.
- A ten-year future must await each committed year and persist each continuation. Comparisons use the same pinned rules/weather, never mutate the saved run and never expose a Year11 transaction. New run selection needs a new acknowledged run key, not the old synchronous Initialize/reset calls.

## Render and feedback alignment

`DisplayAnnual` includes condition in its visual cache key, but its current `landscape.Apply`/`AnnualStage` calls primarily pass maturity and lifecycle. Verify that persistent numeric condition/damage/recovery is actually reflected where claimed, including parcels without newly purchased assets. Do not imply a crop loss after its saved harvest date or show permanent repair before payment. Preserve landscape-first TN-01/02, anticipatory feedback TN-05, information hierarchy CS-01/02 and AX-01. The separate Phase2A visual gate remains open.

## Save/transport activation prerequisites

1. Pin Unity package `com.unity.nuget.newtonsoft-json@3.2.2` directly via UPM in the integration owner's project; the current root project only receives it transitively through Pipeline. Preserve the numeric DTO linker file. Keep legacy JsonUtility behavior separate.
2. Exact actual Unity and linked IL2CPP/WebGL checks must pass the prepared `.NET` ten-year vector, new saves, conflict/recovery and reload. JavaScript-only 25MiB IndexedDB success is not numeric model identity evidence.
3. Dense reports now pass432 pure assertions/30years, with17–20 canonical facts and maximum measured full JSON envelope12,685,309 bytes. Test real dense game latency/memory/quotas; pure source fixtures are not browser performance.
4. Final combined Chrome coverage must include all three separate places, pending/accepted reload, save errors, provider/local mode and late response, manual/fast-forward ending agreement, repeated runs and retained prior history. Report real limitations and keep the independent visual/performance gates visible.

Session API definitions live in `Assets/Terra/Scripts/Gameplay/AnnualTownSession.Numeric.cs`. The implementation remains explicit and inactive until this handoff is integrated and verified by the shared Main/HUD owners.

## Submission deadline boundary — October 25

Keep the released schema-2 game active. Main now contains the separate schema-2/v5 player rules (debt, cancellation, inferred regional events and signed Environment); these must not be silently removed or passed through a schema-3 factory. Pending annual-feedback planning/conversion changes also need an explicit owner review before numeric activation.

The actual Unity/IL2CPP sparse fixture passed, but the 340-site ten-year player exhausted WebAssembly memory. Commit22a68bd supplies frame-boundary validation checkpoints and removes a repeated initialization replay. Its source checks pass; this alone does not establish that dense WebGL memory is fixed. See README for the latest retry outcome.

The acknowledged run-directory controller and C#/JavaScript adapter are available on `codex/numeric-directory-browser` (adapter2cf446b). They preserve exact definitions and prior runs, and remain inactive. Source/model tests and installed Unity API compilation pass; actual linked directory/browser acceptance and gameplay integration remain open. Its database-v2 upgrade requires both bridges together. Directory Open accepts a cancellation-aware checkpoint; the integration owner must supply a main-thread next-frame callback in WebGL.
