# Web download budget audit — October 25, 2026

Read-only follow-up to the full release acceptance. Runtime source `19e7eae835ed4c06a21d42887a9a53ed28cc4fc2` and its frozen export are identified in [the byte/hash receipt](baseline-19e7eae.json). The subsequent consolidated release is being built independently; none of these measurements certify its final output.

| Required Unity payload | Bytes |
|---|---:|
| Packed asset data | 47,357,441 |
| Compiled WebAssembly | 8,062,251 |
| Loader JavaScript | 118,806 |
| Framework JavaScript | 68,455 |
| Total | 55,606,953 |

The required Unity payload alone exceeds the plan's 50,000,000-byte target by 5,606,953 bytes (11.2%). This is not merely an all-map export-size accounting issue. The data file accounts for 85.2% of this payload. Map JSON and optional community imports are separate. These are exact frozen file-body sizes, not a new cold-cache network timing measurement.

The repeatable `TerraBuild.BuildWeb` already applies Brotli with decompression fallback, hashed output names, RuntimeSpeedLTO and 256MB initial memory. Source settings show engine stripping enabled and explicit exception support; active replay/error recovery uses exceptions. Do not blindly disable exceptions or reduce scene quality to satisfy a byte target. No live Editor settings were queried or changed while another owner held the build resources.

A static source inventory found 197 files under Resources and 37 texture candidates reachable through YAML GUID references. This is an investigation aid, not authoritative build packing: imported/binary dependencies and shader usage require Unity's actual BuildReport. Source PNG/JPEG sizes do not measure imported or Brotli-compressed contributions. Many large source tree textures already have smaller default import limits, so ranking original files alone would be misleading.

The release owner was asked to preserve `BuildReport.packedAssets` content paths and packed sizes after the current export. Next action: use that report to identify actual contributors, then prepare an isolated candidate targeting unnecessary data or import compression. Compare the same scene/camera in Chrome and measure the resulting export before accepting a change. Preserve the current frozen release and its original evidence.

Read Unity 6.6's [Distribution size and code stripping](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-distributionsize-codestripping.html) (`webgl-distributionsize-codestripping.html`) and [Deploy a Web application](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-deploying.html) (`webgl-deploying.html`). They distinguish release compression, asset/code stripping, matching server headers and JavaScript decompression fallback. The project already has the repeatable build script for applying selected settings; any future adjustment belongs there rather than a competing blanket optimizer.

This download-budget investigation is separate from the PlayerPrefs replay quota failure. It does not claim a size reduction, performance acceptance, or Phase 2A visual acceptance.
