# Wisconsin weather reference v1

This is an **offline data preparation artifact**, ready for a versioned annual-engine consumer. It does not activate new gameplay, rewrite existing saves, calibrate crop responses or certify a browser build. The existing 3.x simulation remains unchanged.

## Source and selection

The shared numerical reference is Coon Valley / Vernon County, Wisconsin for all three authorized terrain choices. The [hackpack audit](../../docs/HACKPACK_DATA_AUDIT.md) found no accessible dataset bundle in its bounded search. This pack therefore uses NOAA GHCN-Daily. It makes no claim about the contents of the unseen hackpack.

Selected station: **USW00014920, La Crosse Municipal Airport**, 43.8792°N, 91.2531°W, elevation **198.1 m**. Its great-circle distance from the existing Wisconsin AOI envelope center (43.7015, −91.037) is **26.29 km**. The AOI terrain spans approximately 213–361 m. This is a regional station reference, not weather measured on those fields, Point Reyes or Suusamyr. No elevation adjustment, station blending or missing-day imputation is applied.

Metadata for every station within 40 km was inspected. The nearest Coon Valley stations offer precipitation only or a short old record. Four nearby stations with PRCP, TMAX and TMIN extending through 2024 received a full daily audit for 2001–2024:

| Station | Distance | Elevation | Complete contiguous ten-year windows |
|---|---:|---:|---|
| La Crosse WFO, USC00474373 | 18.30 km | 398.4 m | 2015–2024 only |
| Viroqua, USC00478827 | 20.43 km | 382.5 m | None |
| Genoa Dam 8, USC00473038 | 21.25 km | 194.8 m | None |
| La Crosse Municipal Airport, USW00014920 | 26.29 km | 198.1 m | Fifteen starts, 2001–2015 |

The airport provides **8,766 complete calendar days / 26,298 accepted daily element values** over 24 years. It supports different coherent schedules without splicing stations or repeating years. The closer WFO has only one eligible decade; the other audited stations have missing, flagged or presumed-zero observations. Full per-year reasons are in [coverage-audit.json](coon-valley-v1/coverage-audit.json). This is a practical source choice, not proof that airport weather best represents the valley's microclimate. Stations beyond these four were not individually downloaded/audited.

Sources: [NOAA GHCN-Daily](https://www.ncei.noaa.gov/products/land-based-station/global-historical-climatology-network-daily), [fixed-width data/flag documentation](https://www.ncei.noaa.gov/pub/data/ghcn/daily/readme.txt), and [NOAA's federal dataset/license record](https://catalog.data.gov/dataset/global-historical-climatology-network-daily-ghcn-daily-version-3). Attribution: NOAA National Centers for Environmental Information, GHCN-Daily Version 3, DOI [10.7289/V5D21VHZ](https://doi.org/10.7289/V5D21VHZ); retrieved October 25, 2026. NOAA public data / CC0-1.0. These are Terra-derived summaries, without NOAA endorsement.

## Exact inputs and reproducibility

[source-lock.json](coon-valley-v1/source-lock.json) records exact URLs, full upstream byte counts/hashes, retained subset hashes and selection rules. `source/` retains unchanged PRCP/TMAX/TMIN monthly lines for 2000–2024 from all four audited stations, including original measurement/quality/source flags. Year 2000 supplies warmup observations; it is not a selectable first game year. Nearby metadata/inventory rows are retained from NOAA's official S3 distribution. All retained sources together are 1,001,756 bytes.

NOAA's live station files, metadata and version endpoint are fetched separately. They are **not an atomic archive snapshot**. The observed version string is contextual; exact source hashes pin this release. The mutable NOAA source may later differ, so rebuilding uses these committed bytes and never automatically refreshes them. New observations or derivation rules require a new pack identity. The original NCEI metadata downloads timed out; those incomplete files are excluded. Complete S3 metadata inputs are the ones recorded in the lock.

Run from the repository root with Python 3.9+; no packages or network are needed:

```sh
python3 scripts/prepare-weather.py --check
python3 -m unittest discover -s tests/weather -v
python3 scripts/prepare-weather.py --schedule coon-valley-demo-1
```

Without `--check`, the script regenerates the four products from the pinned sources. `--output /path/to/temp-folder` rebuilds elsewhere. JSON encoding is ASCII-escaped, sorted keys, two-space indentation, LF and a final newline. [pack-lock.json](coon-valley-v1/pack-lock.json) records the exact **87,113-byte** [weather-pack.json](coon-valley-v1/weather-pack.json), SHA-256 **5bf2c305473f2d1fb4b3a589f91995032bcd44b4551c0c4278745b08786540de**. The audit and pack also pin the exact source-lock hash. No Unity project or browser export is required to prepare it.

The pack pins a **2,109,320-byte daily JSON companion**, [daily-observations.json](coon-valley-v1/daily-observations.json), SHA-256 `ce1128972981490c28c74bb4e291725345e7c0a4674781c460a4d987fbbbc840`. It contains 9,132 ordered days over 2000–2024, grouped by year, with 1-based ordinal day, integer precipitation/minimum/maximum temperature and the three original flags for each element. Year 2000 is explicitly warmup-only. Crop-specific calendars can derive pre-/post-harvest windows from these exact observations under separately versioned rules, rather than applying an annual aggregate to an already completed harvest. Both files must be verified; the daily companion is preparation data until a runtime consumer packages it.

## Derivation and scientific boundary

Every eligible calendar day must contain all three elements, a nonmissing value, blank quality flag, nonblank source flag, nonnegative precipitation and minimum temperature no greater than maximum. Presumed/unreported zero measurement flags `P` and `I` are rejected. The preceding seven precipitation days must also be valid. Any failure excludes the entire year; no zero fill, interpolation or nearest-station replacement occurs. Trace flags remain recorded; their numeric zero is an unquantified trace, not proof of no precipitation. Station observation-day conventions are retained; this is not an hourly UTC reconstruction.

The hashed pack declares the following **authored analysis rules**, separate from observed inputs:

- May 1–September 30 inclusive is the fixed analysis window (153 days). It is not measured crop phenology or a universal growing season.
- Precipitation stays in integer **tenths of millimetres**; temperature stays in integer **tenths of degrees Celsius**.
- A dry day has precipitation below 1 mm. Longest dry spell resets at May 1 and ends at September 30. A hot day has maximum temperature at least 35°C. Both are descriptive thresholds, not calibrated damage responses.
- An event is one calendar day with at least 25 mm liquid-equivalent precipitation. All-year events remain ordered and include date, ordinal day, month, analysis-window membership, temperature and the sum of the **preceding seven days**, excluding the event day. Consecutive events stay separate; the runtime must not reuse full storage capacity for each without its own recovery rule.
- December observations carry into January antecedent sums. Complete adjacent historical years prevent artificial calendar boundaries. No annual soil-water stock is calculated here.

Daily PRCP includes liquid-equivalent precipitation and does **not** establish rain/snow phase, subdaily intensity, inundation, flood probability or damage. Any warm-season rain interpretation and site damage/recovery/production response is separately authored and versioned by the consumer. Winter or post-harvest events must not silently reduce an earlier harvest. Solar/wind drivers and economic benchmark data are not supplied by this pack. The required shared-reference disclosure mentions economics because it is the product contract; this weather artifact alone is not evidence for any economic coefficient.

The 24 analysis-window totals range from 310.7 to 834.2 mm, longest dry spells from 7 to 28 days, and hot-day counts from 0 to 13. These describe this retained station sample, not future probabilities. Overlapping decades are correlated, finite scenarios and do not represent future climate change.

## Runtime handoff

The consumer must verify pack bytes against the pinned SHA-256 and reject an unknown schema/derivation version. Do not calculate a different checksum from a reserialized C# object. `records` contain stable `<station>:<year>` IDs. `eligibleWindows` contain sorted start years and the actual ten record IDs, with no wrapping or missing-year bridging.

Sampler `terra-weather-contiguous-v1`:

1. Encode `terra-weather-contiguous-v1\n<lowercase pack SHA-256>\n<shared scenario seed>` as UTF-8, with no final newline. The seed is a nonempty string, maximum 256 UTF-8 bytes, used exactly without trimming or Unicode normalization.
2. SHA-256 that byte sequence. Interpret its first four bytes as an unsigned big-endian 32-bit integer.
3. Take its remainder modulo the number of sorted eligible windows (15 in this pack). This is deterministic scenario selection, not an empirical probability model.
4. **Persist the selected ten IDs, pack hash, sampler version and shared seed before accepting the run.** Validate them on restore; retries and comparison branches reuse them. Branch/run IDs and queue contents are never seed inputs. A different sensitivity or rule version is separately pinned.

Golden example: seed `coon-valley-demo-1` selects **2015–2024** for the exact pack above. [schedule-vectors.json](../../docs/evidence/weather-reference/schedule-vectors.json) provides additional ASCII and Unicode seed fixtures for the C# consumer. Replaying the same choices reuses the saved drivers; a new scenario may select another existing window. Weather availability does not authorize resimulating an accepted year, changing old save keys or assigning one weather multiplier to farming, buildings, solar and wind.

Player-facing implementation should preserve this disclosure separately from fictional names and terrain provenance:

> Fictional scenario · weather and economic references: Coon Valley, Wisconsin · other parameters are authored game assumptions.

The annual-engine owner will consume this artifact under a new isolated version. Crop/practice curves, persistent baseline-site state, exposure/storage/recovery, endogenous public repair, score deltas and report-only narration remain separate implementation work.
