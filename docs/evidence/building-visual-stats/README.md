# Visual building decisions

Implementation starts from 615459ac and includes the upstream cancellation-guidance startup repair 1330dc9. No prices, construction durations, establishment durations or scoring weights were changed.

## Behavior

- Selecting a project shows upfront contribution, annual upkeep, construction time and full-effect year as bars, with formatted values on the right. Catalog-wide money scales make comparisons meaningful; time tracks use year ticks.
- Selecting an eligible field adds signed Economic, Social and Environment score changes. Their fixed scale is -25 to +25 points; exact values remain visible when a bar saturates. These are conditional potential, explicitly labelled with their reference year, not earned points.
- Reference comparisons reuse the authoritative lifecycle and capped scoring evaluator, at actual project ages in the selected project's full-effect year (capped at Year 10), with current condition, funded operation and normal conditions. A new candidate compares against the current plan; a queued project against the plan without itself; a replacement against the queued project. Late projects show construction impacts and the after-run opening date. No annual replay or input mutation.
- Accepted projects show completed construction steps separately from Working at (usable operational capacity), plus understandable current facts from the most recent accepted report. Cancellation remains available through the existing confirmation flow. The overlay does not claim an arbitrary share of the town score for an existing building. The explicit industrial Environment penalty is shown as its configured point cost. Raw productivity and service indices are absent from the main view.
- Current Homes filled, Local jobs and Green land versus their targets remain visible near the budget. A short function-to-score link and a concrete benefit explain the project; detailed resource requirements, commitments and assumptions remain one click away.
- Preview results are cached by run, definition, revision, phase, field and action. The private detached evaluator reads the supplied rules and copies mutable assets; it cannot mutate the live town. Unsupported numeric-v4 previews display unavailable.

## Verification

Pure-core check: `dotnet run --project tests/project-preview/ProjectPreviewChecks.csproj -- --dense --evidence docs/evidence/building-visual-stats`.

211 assertions passed on the real 41-site parcel definition, including all 17 project types, capped needs, negative industrial impacts, queued/replacement identity, cancellation and rebuilding, late completion, lifecycle progress, and unchanged input hashes. Comparison values match two independently resolved neutral-weather runs. The 100-call uncached comparison sample averaged 0.18 ms; this is an isolated .NET measurement, not a browser frame-rate claim.

Native layout harness: `ProjectStatsChecks.RunBatch` checks three viewport sizes and both text settings, retained queue/cancel controls, and captures the actual uGUI overlay through a Unity camera. 7,018 assertions passed across all 17 types at 1440x900, 1280x720 and 1024x576, standard and larger text. The images in this directory are native offscreen UI renders; their flat background is intentional and is not a terrain capture. Inspected the compact larger-text data-center overlay and the accepted Year 3 construction state visually. The existing MainGameChapterChecks suite also passed 1,707 assertions plus 16 chapter assertions.

Design references: TN-03/04/05 (calm hierarchy and visible outcomes), CS-01/02/04 (cost, service and timeline comparisons), AX-01 (labels and values supplement colour). This task makes no scenery-quality or public-deployment claim.

Player-language feedback: Established, Output and Cover were rejected as unexplained labels. The main view now uses Working at, Local jobs and Green land, hides raw productive units, and links each project function to its score. Benefit colours match the three top meters. Longer explanations use the existing scrollable Field Notes reader.

## Exported game verification

Source commit `2e4d38c` passed `MainGameChapterChecks.BuildVerifiedWeb` and completed a successful WebGL export. Tested that export in installed Chrome 156.0.8078.48 (headless) at 1280x720 and 1024x576, served from the isolated worktree at `http://127.0.0.1:49871/`. No JavaScript page exceptions occurred. This is a local preview, not a public deployment.

Inspected the actual atlas-to-Meadowbrook flow, the no-field data-center catalog, blocked-field guidance, an eligible field's signed score estimates, optional explanations with scrolling, and the queued compact overlay. The data-center preview shows $3M upfront, $80K yearly cost, two construction years, full benefit/opening in Year 3, six local jobs, and positive Economic / negative Social and Environment estimates. The compact screenshot retains right-aligned values and queue recovery controls above the toolbar. Screenshots and structured UI audits are saved as `chrome-*` files here. Accepted-year lifecycle behavior is covered by the native harness; this browser pass does not claim an end-to-end ten-year playthrough or FPS measurement.

## Reconciliation with concurrent main changes

Fetched main `b01d6ce` after Chrome verification and merged it in `480441e`. Main introduced current-year cash reservations and conversion of accepted land uses. Preserved those rules: the visual overlay now calls the authoritative conversion-cost method, includes conversion delay, calls lifetime cost by that name, and shows the queued replacement instead of the old accepted use. The detached score comparison reuses `AssetsForPlan` as well as `EvaluateOutcomes`; conversion cover and lifecycle match the engine. No balance values were authored by this overlay change.

The reconciled source passed 228 preview assertions (including conversion timing, queued conversion, accepted converted progress, score parity and input immutability), 1,019 full-game assertions plus 350 annual-feedback assertions, and a compile of all runtime source against Unity assemblies with `UNITY_WEBGL` (zero errors; 12 existing warnings). The full-game harness uses its original smaller fixture; the dedicated preview harness uses the real 41-site map. Chrome evidence remains pinned to `2e4d38c`; it does not verify subsequent main changes. The local preview server likewise serves that pre-reconciliation export.

The optional repeat native layout check after reconciliation was queued behind another task and was not run before the user's request to finish and push. The added native conversion cases are present but unexecuted; native screenshots/check counts above remain from the earlier verified feature source. Combined-source compilation and model parity checks passed as recorded above.
