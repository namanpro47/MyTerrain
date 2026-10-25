# Phase 4 evidence — October 24, 2026

The isolated consequences slice is complete on `codex/phase4-consequences`. [Implementation and integration handoff](../../PHASE_4_CONSEQUENCES.md). Phase 2A's visual rebuild and combined performance gate remain open.

## Correctness

- `integration-checks.json`: **1,372** production C# integration assertions, including full preview/commit equality, independent scalar values, connected/disconnected effects, bounded shared capacity, unchanged context, frozen storm, deterministic order, zero baseline and 20 reset cycles.
- Package B: **19** existing suites pass, including 1,000 randomized conservation cases. The Unity API compatibility build passes with zero warnings/errors.
- `partition-check.json`: all **1,711** original geometry pieces cover **1,486,800 m²** once. Gap/outside = 0; overlap is numerical noise below 0.1 m².
- `unity-regression-run.json`, `regression-summary.json`, `unity-checks.json`: **2,022** actual Unity assertions (27 + 139 + 1,758 + 98). The final export reran the 98 Phase 4 checks. No primary Editor or scene was modified.
- `source-hashes.json` records source identity; `hydrology.json` records world/partition hashes and model provenance.

## Final Web build and Chrome

`web-build.json`: Unity 6000.6.6f1, final export completed at **2026-10-25 01:06:59 UTC** (October 24 local), **92.989 s**. `local-assets.json`: all **9** served files returned HTTP 200 and matched the export; **20,745,733 bytes**, including all four WorldPack JSON files, decoded gzip and valid WASM. No public deployment was made.

Chrome tested [the local build](http://127.0.0.1:8093/?phase4=final) at **1280×720 and 1440×900**, standard and larger text. `browser-checks.json`, `browser-logs.json`, `browser-audits.json` preserve the route and observations. All 13 recorded transactions/history operations match the independent expected values within 0.01 m³. These are internal test precision, not scientific precision claims; the player sees rounded estimates.

| Design / operation | Production | Credits | Discharge m³ | Held m³ |
|---|---:|---:|---:|---:|
| Baseline | 6 | 60 | 9701.926 | 0 |
| Cultivate Southern field | 18 | 36 | 9754.669 | 0 |
| Add Meadow edge swale | 18 | 8 | 9700.148 | 8 |
| Move swale to Brye-side | 18 | 8 | 9687.283 | 8 |
| Remove swale | 18 | 36 | 9754.669 | 0 |
| Clear all | 6 | 60 | 9701.926 | 0 |
| West field living margin | 14 | 30 | 9678.104 | 0 |
| Replace margin with restoration | 5 | 42 | 9633.404 | 0 |

Mouse buttons confirmed cultivation, swale and replacement. Undo/redo restored exact prior results; clear returned the baseline. Insufficient-credit and protected-site Enter attempts left the design unchanged. Modal input did not place an intervention. Preview and blue membership outlines were inspected at close site scale; cancel/remove clear the overlay. Moving between hypothetical groups changes measured consequences, and independent model checks prove disconnected plots/context cannot benefit from that storage.

## Captures and layout review

- [Baseline](baseline-1280.png), [cultivation preview](cultivation-preview-1280.png), [connected swale](connected-swale-preview-1280.png), [move](move-swale-preview-1280.png), [remove](remove-swale-preview-1280.png).
- [Budget rejection](budget-rejected-1280.png), [protected rejection](protected-rejected-1280.png), [reset with larger text](reset-large-text-1280.png), [living margin](living-margin-large-text-1280.png), [replacement](restore-replacement-large-text-1280.png).
- [Runoff guide, larger text at 1280](runoff-guide-large-text-1280.png), [guide at 1440](runoff-guide-large-text-1440.png), [final swale preview](connected-swale-final.png).

TN-03/05, CS-02/04 and AX-01 informed the information hierarchy and estimate disclosure. Actual output was compared with the inherited Phase 3 presentation and the rejected Phase 2A study baseline for regressions; this is not Terra Nil visual parity. The inherited scenery is unchanged by Phase 4. The first review found the feedback banner overlapping measures and large-text guide overflow; both were corrected, the vegetation scope was clarified, and mouse confirmation remains visible. All new labels fit. Five long audit log messages were truncated after their complete labels arrays, which were recovered offline. The inherited location title reports a 0.78 px preferred-height excess with larger text but has no visible clipping in these captures.

`initial-browser-logs.json`, `layout-review-logs.json` and `intermediate-assets.json` are explicitly **intermediate** review evidence, not the final acceptance build. The final browser recorded zero console errors and an inherited warning about stripped FSR upscaling; it does not certify optional rendering passes. Startup to interactive was 9,277 ms in this run. No sustained performance benchmark or human novice study was performed. The initial automation tab stalled before establishing a JS context; a fresh Chrome tab worked. No Safari checks were run.

## Scope limits

Hypothetical soil, storm, connection groups, costs, productive units and capacity remain disclosed. No surveyed drainage, calibrated flood depth, yield, water-quality prediction, new location, new assets or live weather. Do not sum source-retention attribution and device capacity use. The unchanged context contribution is separately reported; future mission targets must not conceal it. Combined visual/performance acceptance and Phase 5 remain separate work.
