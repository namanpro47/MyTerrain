# Parcel and inherited-community evidence

Base: refreshed `origin/main` `aee5bb6409ac3f20b9243b83a246c7e5a9854dbe`; isolated worktree `Terra-map-parcels`.

The three `*-layout.png` files are source-cover plan views, inspected for placement distribution and intact woodland/corridors. They are diagnostics, not game screenshots. H1–H4 are existing homes; F/V/P are the farm store, village square and recreation ground. W/L mark larger joined plots.

`model-checks.json`: **2,241** assertions over production code, across all three places: overlay binding, area partitions, standard-home fit on every open plot, all 17 variants, inherited capacities counted once, multi-project planning, ten accepted years, financial/water conservation, save restore and request-size bound. Python `test_geometry.py` additionally checks every emitted plot against every other plot and the source woodland/water/sealed cover, protected geography and mapped corridor buffers.

The pre-existing full-game suites pass **608** assertions for each of the legacy and connected Wisconsin definitions. They use the original source layout; they are compatibility coverage, not tests of the new parcel geometry. `legacy-connected/` retains the standalone receipt.

`unity-checks.json`: **69** Unity checks pass with real catalog prefabs on all three overlays. Seven baseline model groups exist on each map, their foundations fit and applying an unchanged baseline retains them. The `*-editor.png` captures inspect ground/model geometry in Edit Mode; instanced vegetation's play loop is absent. First-import URP converter USS/UXML path warnings and the existing stream shader warning are present in the local log. These model checks are not browser/playable performance or visual acceptance.

Local .NET narration service compiles with zero warnings/errors after raising the bounded loopback request cap to 16 MiB. No provider request, hosted backend write or production game deployment is claimed.

Final integrated export/browser receipt follows separately after reconciliation with the concurrently completed HUD.
