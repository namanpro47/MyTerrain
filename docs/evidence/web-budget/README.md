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

Reproduce this subtotal with `python3 docs/evidence/web-budget/measure-export.py /absolute/path/to/frozen/export --source RECORDED_COMMIT`. The helper reads the four files referenced by `window.TERRA_BUILD` in the export's actual wrapper, rejects missing/escaping/duplicate paths, and reports byte sizes and SHA-256 hashes. It reproduced all four baseline hashes and the 55,606,953-byte total exactly. An under-budget Unity subtotal alone cannot pass the whole initial-transfer gate: wrapper artwork, scripts and required map data must also be measured in Chrome.

The repeatable `TerraBuild.BuildWeb` already applies Brotli with decompression fallback, hashed output names, RuntimeSpeedLTO and 256MB initial memory. Source settings show engine stripping enabled and explicit exception support; active replay/error recovery uses exceptions. Do not blindly disable exceptions or reduce scene quality to satisfy a byte target. No live Editor settings were queried or changed while another owner held the build resources.

A static source inventory found 197 files under Resources and 37 texture candidates reachable through YAML GUID references. This is an investigation aid, not authoritative build packing: imported/binary dependencies and shader usage require Unity's actual BuildReport. Source PNG/JPEG sizes do not measure imported or Brotli-compressed contributions. Many large source tree textures already have smaller default import limits, so ranking original files alone would be misleading.

The release owner was asked to preserve `BuildReport.packedAssets` content paths and packed sizes after the current export. Next action: use that report to identify actual contributors, then prepare an isolated candidate targeting unnecessary data or import compression. Compare the same scene/camera in Chrome and measure the resulting export before accepting a change. Preserve the current frozen release and its original evidence.

Read Unity 6.6's [Distribution size and code stripping](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-distributionsize-codestripping.html) (`webgl-distributionsize-codestripping.html`) and [Deploy a Web application](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-deploying.html) (`webgl-deploying.html`). They distinguish release compression, asset/code stripping, matching server headers and JavaScript decompression fallback. The project already has the repeatable build script for applying selected settings; any future adjustment belongs there rather than a competing blanket optimizer.

This download-budget investigation is separate from the PlayerPrefs replay quota failure. It does not claim a size reduction, performance acceptance, or Phase 2A visual acceptance.

## Completed combined export e74496b

The successful combined export's actual wrapper references **57,950,802 bytes** of Unity payload, 7,950,802 bytes above the target. [Measurement and hashes](baseline-e74496b.json): data 47,992,122; WebAssembly 9,768,004; framework 71,870; loader 118,806. This is 2,343,849 bytes more than the earlier baseline, largely compiled code (1,705,753 additional bytes). No attribution to a specific code change has been established.

Unity's [content size summary](content-size-e74496b.txt), tied to the [preserved BuildReport](content-report-receipt.json), now supplies actual build attribution. Its **uncompressed** packed content totals 107.3 MiB: textures 81.3 MiB (75.8%), meshes 12.1 MiB, audio 9.9 MiB, shaders 1.5 MiB and text assets 1.1 MiB. These category sizes cannot be added to or directly compared with the Brotli transfer subtotal.

The largest texture entries are the TriForge leaf texture, four Julio terrain normals and Earth globe texture, each reported at 5.3 MiB. Four terrain color maps each occupy 2.7 MiB. Source inspection shows Crunch disabled on these candidates. This supports testing selected texture import compression at unchanged resolution before considering resolution reduction. It does not prove a savings estimate or visual equivalence. Both forest families and the terrain maps have active resource loads; no asset is declared unused solely from the inventory. Keep changes isolated from this frozen release, compare the same scene and camera, and remeasure the real export.
