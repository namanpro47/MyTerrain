# Shared Wisconsin weather preparation — October 25, 2026

Source base: refreshed main `7c322e9`, isolated `codex/weather-reference` worktree. Scope: immutable reference data, offline derivation, coverage audit and deterministic ten-year selection. No annual runtime, HUD, scene, save, provider or deployment change.

## Result

The [data contract](../../../simulation-data/weather/README.md) records source selection, exact inputs, units, thresholds, no-infill policy, geographic limitations and the consumer handshake. La Crosse airport supplies 24 complete years (2001–2024), 8,766 days, 26,298 accepted values and 15 contiguous ten-year windows. It is 26.29 km from the Wisconsin AOI center, not an on-site measurement. Closest available multi-element alternatives were audited and their failures retained.

The 87,113-byte pack is pinned at SHA-256 `5bf2c305473f2d1fb4b3a589f91995032bcd44b4551c0c4278745b08786540de`. Its 2,109,320-byte daily JSON companion is also pinned and preserves all 9,132 days (including warmup year 2000), integer values and original flags. No missing/quality-rejected day becomes a dry day. Complete historical order, including preceding December for January event antecedent, is retained. All-season liquid-equivalent observations remain distinguishable from the authored May–September analysis window; the companion allows separately versioned crop calendars to distinguish pre-/post-harvest weather.

## Verification performed

`python3 -m unittest discover -s tests/weather -v`: **19 tests passed**. Meaningful failure cases include missing observation, rejected/presumed-zero flag, TMIN above TMAX, missing warmup, malformed/duplicate fixed-width rows, impossible calendar days, altered source hash, invalid seed and noncontiguous/duplicated schedule. Boundary fixtures cover leap day, trace, dry/heat thresholds, analysis-window reset, consecutive heavy-precipitation days and a seven-day antecedent across New Year without double-counting the event day.

The committed products rebuild byte-for-byte from pinned inputs; `python3 scripts/prepare-weather.py --check` passes. The station audit independently identifies 15 complete airport windows, only one WFO window, and no eligible decade for Genoa/Viroqua. A retained 2018 observation regression checks August 27 precipitation (77.2 mm), the preceding seven days (18.6 mm) and date ordinal against the raw daily row. Across 256 test seeds every eligible window is selectable. Golden [schedule vectors](schedule-vectors.json) define exact byte/seed behavior for the runtime consumer. This is not a statistical randomness or future-climate validation.

## Limits and handoff

The annual-gameplay chat owns the new numeric/report-v2 consumer and accepted-history compatibility. The agreed contract pins the actual ten IDs and checksum before a run, keeps branch/run identity out of weather draws, and requires separately versioned authored crop/exposure/recovery coefficients. It must not apply all-year PRCP as measured warm-season rain, reuse full storage per adjacent event, infer solar/wind generation from the crop factor, or reinterpret legacy saves. Runtime and Chrome integration are **not tested by this data-only slice**.

No hackpack bundle became available; the earlier bounded search is preserved. No new economic/price dataset, site exposure calibration, hydrological prediction or crop benefit claim is made. Phase 2A and the combined public release gates remain open.
