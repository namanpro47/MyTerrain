# Parcel and inherited-community evidence

Base: refreshed `origin/main` `aee5bb6409ac3f20b9243b83a246c7e5a9854dbe`; isolated worktree `Terra-map-parcels`.

The three `*-layout.png` files are source-cover plan views, inspected for placement distribution and intact woodland/corridors. They are diagnostics, not game screenshots. H1–H4 are existing homes; F/V/P are the farm store, village square and recreation ground. W/L mark larger joined plots.

`model-checks.json`: **2,241** assertions over production code, across all three places: overlay binding, area partitions, standard-home fit on every open plot, all 17 variants, inherited capacities counted once, multi-project planning, ten accepted years, financial/water conservation, save restore and request-size bound. Python `test_geometry.py` additionally checks every emitted plot against every other plot and the source woodland/water/sealed cover, protected geography and mapped corridor buffers.

The pre-existing full-game suites pass **608** assertions for each of the legacy and connected Wisconsin definitions. They use the original source layout; they are compatibility coverage, not tests of the new parcel geometry. `legacy-connected/` retains the standalone receipt.

`unity-checks.json`: **69** Unity checks pass with real catalog prefabs on all three overlays. Seven baseline model groups exist on each map, their foundations fit and applying an unchanged baseline retains them. The `*-editor.png` captures inspect ground/model geometry in Edit Mode; instanced vegetation's play loop is absent. First-import URP converter USS/UXML path warnings and the existing stream shader warning are present in the local log. These model checks are not browser/playable performance or visual acceptance.

Local .NET narration service compiles with zero warnings/errors after raising the bounded loopback request cap to 16 MiB. No provider request, hosted backend write or production game deployment is claimed.

## Integrated local browser preview

The final functional export is source `dbd848a`, including current-main HUD/weather-preparation reconciliation and the modal pointer guard. `local-export.json` freezes **19 byte-verified served files / 44,717,714 bytes**, built in **188.387 seconds** with BuildTimes/Gzip. This is a functional local preview, not a release performance sample. An earlier RuntimeSpeedLTO attempt was stopped during its expensive linker step; no successful optimized release export or production deployment is claimed.

Actual Chrome at 1707 × 876 CSS pixels verified the three native atlas entries, distinct opening briefs and the **32 / 305 / 333** available counts. Seven inherited properties render on every map. Point Reyes rejects replacement of an existing home and leaves $10M available. The first Wisconsin browser pass exposed a modal-dismissal click-through; `dbd848a` consumes the gesture. A fresh-origin Suusamyr pass confirms the Explore click leaves revision 0, $10M cash and zero queued/assets. A new home then reserves exactly $1.1M, leaving $8.9M available; reload and re-entry retain revision 1 and the one queued home, with the seven inherited properties unchanged. No error-level browser console logs were observed. Browser captures were inspected in the chat; the saved `*-editor.png` files remain separately labelled Edit Mode captures.

The transferred native/Editor zoom correction is compiled separately in the combined source. It changes only code excluded from the WebGL player, so it does not replace the frozen browser input bridge or require another WebGL export. See `../trackpad-zoom/` for scope and physical-input limits. Ten-year progression is covered by model tests; this browser pass does not claim a new manual ten-year run, provider invocation, visual-quality acceptance or a production publication.
