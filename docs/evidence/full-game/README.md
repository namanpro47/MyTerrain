# Full annual implementation — work in progress, October 24, 2026

Base: refreshed `origin/main` at `7fe7adc6f330d5b1a9b9e0b0e808b0218b7ebfac`, isolated `codex/full-implementation` worktree. The user authorized all remaining phases, direct main pushes between deliveries, and reconciliation with Peter's additions. Existing Phase 2A work stays with its active owner; no visual acceptance is inferred from these engineering results.

## Phase 4A/4B engineering slice

Peter's `AnnualTown` remains the only ledger/time/history authority. Schema 2 adds explicit nullable service contracts; the live `3.1.0` definition rejects missing capacities. Null extension fields retain legacy hashes. Original starter/demo definitions remain available for compatibility tests. The full game uses a separate `terra.meadowbrook.annual-full.v3.1` save key and does not reinterpret accepted starter history.

All 12 chart families and 17 variants have costs, terms, capacities, physical eligibility, metric art and operating outcomes. Full project quantities, site offers, conditions and rubric targets are in [definition.json](definition.json). Every effect quantity beyond the existing reference-storm model is an **authored educational scenario assumption**. All prices retain the supplied chart, including $3M wind/solar. Housing receipts depend on operation, occupancy and supplied resources. Private output never enters the treasury.

Economic weights: output/jobs/funded dues 40/30/30. Social: occupied households/services/undisrupted sites 40/40/20, with equal energy, drinking water, wastewater, recreation and local-service subweights. Environmental: cover/reference-storm discharge reduction/low-emissions supply/water pressure 40/25/20/15. Demand caps prevent stock-benefit stacking. Failed operation suspends benefits; due arrears count in the funding component. Year 3 and Year 7 use disclosed saved adverse conditions; Year 7 also requires a $150K public repair.

The wind option needed an additional **same-AOI hypothetical opportunity**, not a smaller turbine or another map. Its 150×150 m envelope is at local (375,-495)–(525,-345), clear of all existing sites and buffered mapped routes/water. Source-cover intersections remove exactly 22,500 m² from context; the drainage domain stays unchanged. It has a 16×16 m level foundation, distinct from full rotor clearance. [Geometry evidence](wind-opportunity.json); `python3 scripts/verify-annual-opportunity.py` reproduces the cover/overlap checks. This is not legal consent, engineering siting approval or a surveyed parcel. The source pack is unchanged; the extension is applied only to the annual runtime copy.

The three action categories use four visible cards per page. Reports show actual ledger entries and three causal summaries, with full meter details available separately. Pattern IDs: TN-03/04/05/06/08, CS-01/02/04, AX-01. UI/normal/close/late-year comparison against the eventual accepted Phase 2A benchmark is still required.

## Verification so far

- `dotnet run --project tests/full-game/Terra.FullGameChecks.csproj`: **608 assertions**. Each of 17 variants reaches an operating Year 10 result; every yearly ledger reconciles; conservation, meter bounds, save replay, missing inputs, changed rules, overspending, tax timing, saturation, maintenance, power and water-service separation checked.
- `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/annual/Terra.AnnualChecks.csproj`: **94 annual + 15 gameplay assertions**.
- `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/chapters/Terra.ChapterChecks.csproj`: **38 assertions**, including old fingerprints; no provider call.
- Unity 6000.6.6f1 compiled with zero errors/warnings and passed **34** full-catalog lifecycle/JSON replay checks across **127 eligible offers**. The follow-up call briefly disconnected during domain reload; waiting for readiness restored the connection. Existing consequences suite passed **1,372 assertions**. The attached headless Editor is running successfully. The user unlocked the Mac and actual Chrome verification resumed; see the browser checkpoint below.

**Not a phase-completion or release claim.** The direct visual gate, actual browser evidence, final balance/ending/accessibility/performance and release rehearsal are not yet accepted. Human novice observations remain unavailable.

## Phase 5 balance and transition checkpoint

The same standalone suite now passes **608 assertions**. It executes two distinct plans against the real expanded offers and cash reservation rules, then compares manual continuation (reloading after every year) with the exact fast-forward transition sequence. Snapshots are identical. Both plans encounter the saved Year 3/7 adverse conditions, fund every selected bundle and finish with no arrears and positive uncommitted cash.

| Actual plan | Year 10 Economic / Social / Environmental | Closing public cash |
|---|---|---:|
| [Community and services](community-and-services.json): apartments, rotation, park, village center, solar, wastewater, restoration | 85.4 / 99.0 / 54.8 | $5,240,000 |
| [Land and livelihoods](land-and-livelihoods.json): wind, farm storage, two rotations, restoration, apartments, wastewater, park | 94.2 / 91.0 / 66.6 | $6,163,000 |

These are reproducible engineering balance examples, not novice playtest findings or a claim that every family is equally strong. The tested no-action, saturated services, insufficient funding, adverse conditions and resource-intensive data-center cases remain part of the suite. The source chart prices have not been changed. No elapsed-time or 8–12-minute human-comprehension claim is made. Full-browser adaptation, confirmation and timing remain to be verified.


## Future, replay and whole-game browser checkpoint

The 3.1.0 scenario explicitly stores final targets of Economic 75, Social 75 and Environmental 60. Its separate save key preserves earlier version-3 test history without reinterpreting accepted scores. Confirmed fast-forward saves the frozen request and every accepted year, yields between years, and uses the same transition path as manual play. The final comparison computes a no-new-action run through the same engine and saved drivers. Opening/reference land is read-only and identified; budget and meters remain the player's accepted result. Timeline, optional transfer question, takeaway and confirmed baseline replay are connected.

Chrome 155.0.8059.85 on the local Web export has exercised the actual annual controls: two proposals, undo one, Year 1 acceptance, reload of the accepted report, a different Year 2 park decision, Year 3 wet conditions, and confirmed continuation to Year 10. The first browser route ended at $19,664,000 with meters 55.5 / 61.0 / 42.0 and $4,000 future obligations; no duplicate payment occurred after reload. Both timeline pages, the optional question, replay cancellation and confirmed replay were exercised. These are engineering observations, not novice findings.

The first browser pass exposed reference-map tooltips showing accepted projects, larger-text help overflow, a cramped 125% layout and a cropped tall wind project. Corrections suppress reference tooltips, retain an explicit map caption, shorten help/year headings, adapt the compact dock, and frame selected projects using their real catalog bounds. Full-site land treatments now use terrain-following plants instead of a tiny model sample on a flat parcel-sized pad. Wind keeps its real rotor envelope and a separate level 16 m foundation. Existing free audio is gesture-triggered, fades in, obeys mute/focus and includes action cues.

The compact browser audit at 125% zoom (1024×576 CSS pixels in a 1280×720 physical capture) found no text overflow. Wind is rejected on an undersized field and accepts its dedicated same-AOI opportunity, reserving $3.4M. Standard captures use explicit viewport clips to avoid the Chrome full-window screenshot scaling artifact. Historical first-pass captures and `large-text-initial-issues.json` document findings rather than final acceptance. Final correction checks, operating-family review and foreground performance results will be added before phase acceptance.

Pattern IDs: TN-03/04/05/06/08, CS-01/02/04, AX-01. No Phase 2A acceptance, final visual benchmark, novice comprehension, controlled performance, public release or rehearsal completion is claimed here.


The subsequent Chrome run reproduced the complete **land-and-livelihoods** strategy through actual catalog/site controls: seven Year 1 projects, waiting through Year 5, apartments on Lower west field in Year 6, the Year 7 setback, then confirmed fast-forward from Year 8. The ending exactly matches the standalone model: **94.2 / 91.0 / 66.6**, **$6,163,000** cash, **$612,000** beyond-horizon obligations and no arrears. Year 7 review correctly includes **$270,400** pay-now ($120,400 operation plus $150,000 repair). The no-action reference visibly removes projects, labels the map and suppresses accepted-project tooltips; returning to the future restores the accepted map/result. Evidence: `chrome-land-strategy-future.png`, `chrome-year7-repair-review.png`, `chrome-no-action-reference.png`.

Larger-text help passed its corrected text-height audit at 1440×900 and at 125% zoom / 1024×576 CSS. The larger-text ending also fits the compact modal (`chrome-large-ending-125.png`). The 1920×1080 larger-text pass identified a three-pixel accepted-tooltip excess; its redundant closing sentence was shortened. These checks do not establish screen-reader equivalence for the Unity canvas.


Final correction export `1e172d1aad5c4a4abc90fb3f7c745cda` succeeded in 108.391 s. It restores the completed strategy unchanged, correctly frames the operating 165 m turbine (`chrome-operating-wind.png`), and emits no browser errors in the inspected log. Two mute/unmute cycles changed Chrome's audio-playing indicator as expected; reduced-motion controls changed state. This verifies control/output activity, not a human listening-quality assessment. Standalone full-game checks remain 608/608; the Unity lifecycle entry point completed successfully. The latest local asset receipt pins the tested bytes. Diagnostics now separately report annual-transition duration and native/managed memory observations for the remaining performance work.

## Whole-game browser coverage and performance baseline

Chrome 155.0.8059.85 on this M1 Max / macOS 26.5.2 completed the community-and-services strategy manually through all ten years: **85.4 / 99.0 / 54.8**, cash **$5.240M**, future commitments **$797K**. The earlier land-and-livelihoods strategy reached **94.2 / 91.0 / 66.6**, cash **$6.163M**, commitments **$612K**. Reload restored completed results. Together with the remaining-family run, all seventeen variants were queued, accepted and operated in the live game. Close captures cover cover crops, gas power, data center, low-density homes, traditional farming, swales, stewardship and margins. This establishes gameplay coverage, not acceptance of every asset's visual finish.

The remaining-family run correctly rejected a data center when only $1.95M was available against its $3.8M full reservation. After waiting, it became affordable in Year 3. The completed result was **91.7 / 58.9 / 36.2**, cash **$8.753M**, commitments **$573K**: a mixed ending with a lower environmental result than no new action. Completed views now explicitly say read-only and every ending reports cumulative public spending, including repairs and paid arrears.

The pre-chapter integration build measured manual End Year including world/report refresh at **23.1–54.4 ms**; eight-year fast-forward took **308.6 ms**, yielding between years. These are offline timings, not provider latency. Native memory remained approximately **116–122 MB allocated / 214–222 MB reserved** across the recorded future/replay observations; this short sample does not establish a long-session leak guarantee.

Three foreground 30-second paths at 1440×900 recorded **zero unfocused frames**:

| Path | Average fps | Frame p95 / p99 | Maximum | Result |
|---|---:|---:|---:|---|
| Late land strategy camera | 50.5 | 31 / 41 ms | 51 ms | Stable-30 frame budget met; 60 fps target missed |
| Close editing, preview, queue, undo/redo | 33.7 | 48 / 62 ms | 117 ms | Stable-30 budget missed |
| Transformed landscape camera | 46.5 | 35 / 46 ms | 54 ms | Stable-30 budget missed |

Raw JSON accompanies this note. No placement CPU stall exceeded 200 ms; sampled DOM-to-next-animation-frame placement latency was 30.6–34.7 ms. Preview included a 117.4 ms maximum outlier, with the other measured samples at 16–56.2 ms. These measurements indicate a presentation opportunity, not physical display latency. Other existing Unity Editors and Chrome work were left running; measurements describe this actual shared demo setup.

The release optimization audit found WebAssembly `BuildTimes` mode and a 32 MiB initial heap. The build entry now requests `RuntimeSpeedLTO` and a 256 MiB initial heap based on the observed reserved memory, retaining geometric growth, gzip, fallback decoding, cache and exception handling. No scene density, shadows, resolution or material quality was reduced. Re-measurement is required before declaring an improvement. Relevant Unity 6000.6 guidance: [Web Player optimization](https://docs.unity3d.com/6000.6/Documentation/Manual/web-optimization-player.html). Phase 2A and release acceptance remain open.

## Reconciliation with Peter's chapter integration

Fetched `9da54a5d07985e1d248f0ae1a91e3d585713947e` and merged its local chapter integration without replacing the annual ledger or either set of UI changes. The full-game suite passes **608 assertions**, and the chapter suite passes **40** using `DOTNET_ROLL_FORWARD=Major` with the installed .NET 10 runtime. Strategy fixture fingerprints were regenerated with Peter's corrected Mono-compatible numeric hashing; balances and outcomes are unchanged. Unity compilation passes. The next Web export includes both changes; previous browser evidence remains pinned to its prior export.

## Runtime optimization retest

Export job `063e05ac9e4544cc9bda625b72f09a81` completed in **358.399 s**, with RuntimeSpeedLTO and the 256 MiB initial heap. Verified local assets total **39,617,826 bytes**. No art or render-quality setting was reduced.

| Foreground path, 1440×900 | Average fps | p95 / p99 | Max | Frames over 33.34 ms |
|---|---:|---:|---:|---:|
| Accepted transformed landscape | 58.2 | 20 / 22 ms | 25 ms | 0 |
| Close preview / queue / undo / redo | 37.9 | 33 / 37 ms | 66 ms | 50 / 1,138 |
| Opening landscape, one test player | 57.9 | 20 / 24 ms | 162 ms | 3 / 1,737 |

All three recorded paths have zero unfocused frames and meet the documented p95 ≤33.3 ms fallback, while the 60 fps target remains unmet. The editing sample has eighteen preview presentation-opportunity samples below 33 ms and two placements at 32.4/60 ms; no >200 ms placement stall. This result is specific to this machine/export and must be reconsidered after integrating the visual owner's next scene.

An intervening opening-path sample with multiple test players open and another Unity build active recorded p95 **277 ms**; it is preserved as `performance-optimized-opening.json`. Closing this task's duplicate players and repeating the same movement yielded the one-player row above. Do not discard that interference case or imply performance is independent of concurrent GPU/CPU load. No teammate application was closed.

Fresh-origin arrival observations so far are **6.854 s** and **12.324 s** with `Cache-Control: no-store`. The first originated at native window size before resizing, the second at 1440×900. A third tab did not yield a readiness receipt before closure and is **not** counted as a pass; three complete controlled arrivals remain required.

The combined game restores an existing mixed ending unchanged and now displays **$11.427M public spending**, $8.753M closing cash, $180K receipts and $573K future commitments. Chapter failure leaves Year 1 pending with $10M unchanged. Reload restores that submitted state; retry remains safe; explicit offline acceptance produces exactly $11M closing cash for a waiting year. Captures accompany this note. The generic offline budget card discovered here is being replaced by actual accepted ledger amounts and all three indicator results; prior saved chapters retain their original receipt text.

### Chapter recovery, compact text and repeated runs

Export `47d3ac9481664b41b580b818173b66cb` completed in **361.928 s**; `chapter-assets.json` pins its bytes. A previously accepted offline chapter restores without another appropriation/payment. A pending Year 2 can return to editing with its cash unchanged, then accept a different park plan. The new offline card reports its exact $50K payment, $1M appropriation, $11.95M closing cash and $20K commitments. Economic/social and environmental/conditions cards now include accepted scores and actual quantities. Old saved chapter receipts retain their original text. The chapter suite passes **42 checks**, the Unity main-game HUD/session check passes **17**, and existing Unity chapter checks pass **16**.

Larger-text chapter cards and the spending-inclusive ending fit at **125% Chrome zoom / 1024×576 CSS pixels** with no text-height audit overflow. The park-only run ends at **42.9 / 61.0 / 42.6**, cash **$19.784M**, public spending **$216K**, obligations **$4K**. The measured offline Year 2 acceptance took **48.6 ms** including persistence/report refresh; the remaining eight years took **641.5 ms** including yielded frames and final rendering refresh.

Two confirmed replays and ten-year continuations within the same player produced the exact no-action result **42.3 / 58.0 / 42.0**, cash **$19.85M**, spending **$150K**, obligations **$0**. Reserved native memory stayed at **221,708,288 bytes**; settled allocation was **122,492,568 bytes**, with **277 meshes / 689 materials**. Managed memory fell to **22,949,888 bytes** after settling. This is a bounded repeated-run observation, not a proof of indefinite leak freedom.

**Foreground measurement qualification:** subsequent native Chrome inspection found the other visual-work chat using another tab while this task's automation could still interact with its player. Unity's `Application.isFocused` alone does not prove browser-tab visibility. The earlier performance rows therefore remain provisional foreground evidence. F9 diagnostics now record DOM hidden/focus status, visibility transitions and blur events; eight mocked restart/visibility/listener-cleanup checks pass. Repeat the three paths with those diagnostics before certifying the foreground gate.


### Visibility-confirmed measurements and project stories

Final local export `b7554282467b478bbc2a2246937b62dc` completed in **276.357 s**. `local-assets.json` pins this output. The user authorized coordination with the visual-work chat; that chat left Chrome foreground available during these tests. Each 30-second sample records DOM visibility/focus at both ends, no visibility transitions, no blur events, and zero Unity unfocused frames. One test player was open. These measurements supersede the provisional foreground rows above for this build.

| Path at 1440×900 | Average fps | p95 / p99 | Maximum | Frames over 33.34 ms |
|---|---:|---:|---:|---:|
| Opening camera | 59.9 | 18 / 20 ms | 24 ms | 0 / 1,796 |
| Close editing / preview / queue / undo / redo | 60.0 | 18 / 18 ms | 28 ms | 0 / 1,800 |
| Transformed eight-project valley | 57.8 | 21 / 23 ms | 30 ms | 0 / 1,734 |

`foreground-opening.json`, `foreground-editing.json` and `foreground-transformed.json` retain the actual receipts. The strict p95 16.7 ms / 60 fps target remains unmet; the documented stable-30 fallback passes on this machine and export, without reducing art or render quality. Placement CPU maximum was **16 ms**; two DOM-input-to-feedback presentation opportunities were **38.8 / 34.4 ms**. Eighteen preview samples were **16.4–27.4 ms**, with **2.2 ms** maximum preview CPU. No frame or interaction exceeded 200 ms. These are presentation opportunities, not physical display latency.

Three sequential first visits to separate `Cache-Control: no-store` localhost origins, using one tab/player, reached the actual interactive receipt in **4.416 / 4.145 / 4.059 seconds**. See `foreground-fresh-origin-loads.json`. This is local delivery, not public network or low-end hardware evidence. Existing Unity Editors were preserved; no claim of an otherwise idle machine is made.

The ending timeline now leads to each accepted project's short story: committed year, first operating year, final lifecycle/maturity, production/service units and configured retention, with explicit scenario limits. Inspect focuses the actual metric footprint without allowing edits/refunds. Chrome traversed all eight stories, verified cover crop Year 1 → operating Year 2 and data center Year 3 → operating Year 5, inspected the final site, and returned to the unchanged future. A no-project run presents the existing-use trajectory instead of indexing an empty list. The story fits larger text at 125% zoom / 1024×576 CSS with **zero overflow** in `project-story-large-125-audit.json`. Captures: `chrome-project-story.png`, `chrome-project-story-last.png`, `chrome-project-story-inspect.png`, `chrome-project-story-large-125.png`, `chrome-no-action-story.png`. No browser error was present in the inspected final log.

Pattern IDs **TN-06 / TN-08 / CS-01 / AX-01**: truthful accepted-history narrative, site-level visual payoff and compact legible field notes. The active visual owner reports substantial remaining scenery gaps; this checkpoint does **not** accept Phase 2A or the full Phase 7/8 gate. All seventeen variants have functional browser coverage; final visual integration, matched scenery review, release rehearsals and the public complete run remain open. Novice/hardware/physical listening assessments remain explicitly untested.


### Remaining operating families and release preparation

A fresh-origin community run on the chapter-integrated export repeated Years 1–7 through the actual plan/chapter controls, added the village center in Year 8 and fast-forwarded to the same **85.4 / 99.0 / 54.8**, **$5.240M** ending, **$797K** commitments. Closing accounts show **$15.960M public spending**, **$1.200M receipts** and **$10M appropriations**, reconciling with the $10M opening cash. Close captures now include the six remaining operating variants: solar, wastewater, village center, apartments, restoration and rotation. These supplement the earlier family coverage; no scene/save injection was used.

The additional close review is **not a visual pass**: the village pad's near edge is partly hidden by the action dock on Focus, project-pad boundaries are abrupt, and restoration/rotation planting remains patterned. These concrete findings were sent to the authorized visual owner, including the requirement to carry the rebuilt surfaces into the actual annual lifecycle renderer. None is waived by correct simulation or asset count.

The tested export is copied unchanged to `Builds/Verified/589b655` and all **10 files / 39,619,415 bytes** are hashed in `checkpoint-identity.json`. The existing Vercel CLI can inspect the intended `terra-browser` project in `namanpro47s-projects`; the MCP connection lacks that scope. No deployment or access-setting change was made. Current README/deployment instructions distinguish local full-game source from the earlier published site and optional local Gemini service.


### Loading and retry correction

A dedicated local server deliberately returned HTTP 503 for the world manifest while serving the unchanged export. The real error/retry path recovered successfully, but exposed old credits/category controls behind the message. Source `10a6a0b` adds an opaque raycast-blocking loading backdrop and shows Retry only after a failure. Export `a33c9852ff114d978a09167bfda25c53` completed in **279.549 s**; `loading-fix-assets.json` pins the bytes. Chrome now shows only the recovery card, and clicks at former toolbar locations produce no action or exception. The intentional pack errors are retained; they are not unexplained runtime errors.

An opt-in actual-canvas recording completed and Chrome saved a 21,093,659-byte WebM. It is retained locally under `Builds/Recordings/589b655-recording-check-not-final.webm`, not presented as the final fallback. Its story page exposed a misleading use of the legacy `serviceUnits` channel (a functioning park appeared to have zero services). The next source replaces that row with the financed bundle and a capacity-versus-delivery explanation. Final video/rehearsal evidence must use the corrected, visually accepted export.


### Visual study29c / annual renderer reconciliation

Integrated the published `663a5ec` visual-study boundary over main `1fb5ec3`, retaining the annual authority and build catalog. The new orthographic opening, lighting, artist ground family, tree forms, continuous surface masks and shallow terrain swale are adopted. Annual land recipes use those surfaces/plants instead of old raised flat patches. Rotation's three meadow/crop bands share the planting geometry; building proposals preserve source ground outside their bounded model foundations. Remove previews now restore both ground geometry and paint. Shared prefab meshes remain intact when proposal groups are destroyed. Landscape/plant credits are split into two readable pages with the actual creator terms.

The [isolated running-Editor fixture](annual-terrain-integration-check.cs) passes the [geometry and annual checks](annual-terrain-integration-check.json): 446 lowered swale vertices, deepest 1.149994 m, 309 matching shared-edge vertices; exact position/normal restoration on cancel and undo; valid replacement/removal previews; annual queue/redo; three rotation surface bands; accepted Year 10 replay; read-only baseline comparison and return. Analytical height/cover and the live session remain unchanged. Its 47.7 ms Editor preview measurement is **not browser latency**. Standalone checks remain **608 full-game, 42 chapter, 94 annual + 15 gameplay**; Unity checks remain **34 lifecycle/serialization, 17 HUD/session, 16 chapter**. Compilation has zero errors/warnings.

This is integration of an explicitly unaccepted visual study, not Phase 2A completion. The visual owner continues field/groundcover work. Chrome checks, exact export receipt and integration camera/credit review are in progress. Pattern IDs TN-02/05/09/10/11/12, CS-02 and AX-01.


Combined export `785bfb2056284501bdaccb27b2045dba` completed in **402.965 s**; [local byte checks](visual-integration-assets.json) cover **41,402,770 bytes**. Chrome 1440×900 verifies the annual swale preview, queued geometry, remove-preview, cancel/undo/redo, ten-year acceptance and reload. The swale-only result is **42.3 / 58.0 / 45.0**, **$19.723M** cash and **$3K** commitments. Ten-year continuation took **779.2 ms** in this functional pass, not a controlled foreground performance sample. The existing seven-project community save restores unchanged at **85.4 / 99.0 / 54.8**, **$5.240M** cash, **$797K** commitments. Updated stories and both credit pages render; no error-level log after this build's arrival. Captures are prefixed `visual-integration-`.

That Chrome pass exposed two presentation failures: narrow rotation bands generated no plant colonies, and Focus still clipped the near edge of the village footprint behind the dock. The source correction uses parent-polygon meadow generation clipped to each band (**584 groundcover instances** in the test field), and fits actual source-model/parcel bounds into the clear space between HUD panels. Status labels face the current camera; accepted meter deltas now say “last yr”. [Focus checks](focus-framing-check.cs) project every eligible offer's source renderer bounds in both camera modes: **254 cases each** at [1920×1080](focus-framing-check.json), [1440×900](focus-framing-1440.json), [1280×720](focus-framing-1280.json) and [1024×576](focus-framing-1024.json). Editor tests use the actual Game-view canvas dimensions because `Screen.width` inside an Editor eval describes its invoking window. Temporary test sizes were removed and the original view restored. Two existing ground-shader “potentially uninitialized variable (PaintedSample)” warnings are recorded; no shader errors. The corrected export and actual Chrome framing/compact-text retest are pending at this source checkpoint.


### Framing verification and study31 reconciliation

Export `e09a55fb892645b9afaee9f8d543f63d` from `355b4a8` completed in **479.166 s**; `framing-assets.json` pins its bytes. Chrome confirms the whole village footprint lies above the action dock, the rotation strips contain planting, and the existing community ending restores unchanged. Both landscape and plant/audio credits fit larger text at **1024×576 CSS**, with zero text-height audit overflow. These captures verify the framing correction, not whole-scene visual acceptance.

Reconciled main `c4c437a` (available-field highlights) and the published study31b renderer/shader set, including its study32 evidence-only follow-up. The annual terrain fixture still restores geometry/paint exactly and preserves the live ledger; narrow rotation bands now contain **850 groundcover instances**. One existing ground-shader `PaintedFilteredSample` warning remains, with no shader error. The refreshed camera checks pass **254** planning and **254** completed-view projections at 1920×1080. An initial invocation in the Editor's 321-pixel-wide Free Aspect panel was outside the supported layout and failed; the fixed-resolution test passed and Free Aspect was restored.

Completed Year 10 now hides the build catalog and inactive undo/redo/clear/review controls. Exploration, field guide, settings, last report, comparison and replay remain available; compact exploration controls move out of the hidden dock. Focus uses the newly cleared landscape space. Replay restores planning controls. **22** main HUD/session and **16** chapter checks pass, including both completed layouts and restored catalog interaction. Pattern IDs **TN-05 / TN-08 / CS-01 / AX-01**. New merged-export Chrome and performance checks remain pending; Phase 2A and the final release gate remain open.


Export `e46184c317244573b281ea04ee992340` from `48a3e41` completed in **348.299 s**. `study31-completed-assets.json` verifies the delivered bytes. Chrome restores the community ending unchanged, hides completed construction controls, fits the village in the freed landscape space, and retains compact settings/exploration. Pressing B does not trap read-only map navigation. The compact Focus feedback message overlapped the site panel; the follow-up narrows and wraps it between panels. Completed inspection now uses a neutral selected outline and describes unchanged land, rather than showing a placement rejection. The 22 HUD/session + 16 chapter checks remain passing; corrected-export browser verification is pending. No foreground performance sample is counted while Chrome's active window is changing.


### Integrated study31 foreground performance and replay

The user provided an uninterrupted Chrome window for three 30-second paths on the `48a3e41` export. At **1440×900 CSS/render pixels**, each receipt confirms DOM visible/focused at both ends, zero visibility changes/blur events, and zero Unity unfocused frames. This task had one active test player and no build running during the samples; teammate Unity work remained open. See `study31-performance-summary.json` and its three raw foreground receipts.

| Path | Average fps | p95 / p99 | Maximum | Frames over 33.34 ms |
|---|---:|---:|---:|---:|
| Opening camera | 60.0 | 18 / 18 ms | 18 ms | 0 / 1,800 |
| Close preview, queue, undo and redo | 59.9 | 18 / 18 ms | 40 ms | 1 / 1,799 |
| Transformed seven-project valley | 59.9 | 18 / 18 ms | 35 ms | 1 / 1,799 |

The stable-30 fallback passes; the strict p95 16.7 ms target remains unmet. Nineteen preview presentation opportunities span **12.5–24.2 ms**; two placements are **39.9 / 35.2 ms**. Preview CPU maximum is **3.3 ms**, placement CPU **23.3 ms**. No frame or interaction exceeds 200 ms. These are input-to-first-rAF presentation opportunities, not physical display latency. The compact feedback follow-up and future scenery work still require their own appropriate verification.

Actual Chrome continuation of the two queued cover-crop projects reaches **65.3 / 58.0 / 44.1**, **$19.550M** closing cash, **$450K** public spending and zero future obligations. Functional ten-year continuation takes **853.9 ms**, including yielded frames and final refresh. Canceling New run preserves the result; confirming archives it and restores Year 1, **$10M**, zero commitments, visible catalog and functional queue/undo. The replay capture shows a new **$150K reservation** with cash unchanged. These are browser integration checks, not final rehearsals or novice research.


### Compact follow-up and rejected foundation-material study

Export `307ec5386a024e2bbd1c989614f72f3f`, source `e25c5d3`, completed in **393.733 s**. `compact-feedback-assets.json` pins its bytes. Actual Chrome at **1024×576 CSS** verifies neutral completed-field selection and the existing-land description. Standard and larger text fit; Focus feedback stays in the clear column between budget and site panels. `compact-feedback-larger-audit.json` records **zero text-height overflow** and six enabled completed-view controls. Captures `compact-feedback-fixed.png` and `compact-feedback-larger.png` show the actual player. This is a layout pass, not a new scenery acceptance.

A bounded running-Editor trial cloned only the village grass/path mesh UVs and materials, assigning the existing licensed Julio Meadow/Earth color and normal textures. The metric source meshes and live session remained unchanged. Frames advanced from 22,068 at fixture setup to 27,500 before teardown. Matched 1920×1080 scene captures (`project-ground-rejected-before.png` / `project-ground-rejected-after.png`) show an excessively saturated lawn and muddy, conspicuously tiled plaza. **Rejected:** no runtime or shared-prefab change was adopted. The script is retained for reproducibility, not production use. Play Mode ended, temporary objects were discarded and the original Free Aspect view restored. Pattern IDs TN-02/09/10: coherent material values and authored whole-scene quality remain more important than adding surface detail.


Three sequential first arrivals at separate no-store local origins reached the actual interactive game in **8.393 / 6.546 / 9.086 s** on the `e25c5d3` export. Each rendered the onboarding/planning controls; the inspected logs contain no error-level entry for these origins. `e25-fresh-origin-loads.json` preserves the receipts and limitations. One tab/player was used, own Editor Play Mode was stopped, and teammate Editors were preserved. These are local-delivery observations; neither a controlled foreground load benchmark nor public network/low-end hardware evidence is claimed.


The current export is frozen unchanged at `Builds/Verified/e25c5d3`: **10 files / 41,415,454 bytes**, each compared byte-for-byte to the tested build and recorded in `e25-checkpoint-identity.json`. The earlier `589b655` export and explicitly non-final recording remain intact. No new public deployment occurred. Final scenery review, the two release rehearsals, complete public run and matching final recorded fallback remain open.


### Concurrent Peter synchronization reconciled

Before pushing the backup receipt, refreshed main advanced to `de7f83b` with Peter's synchronization, including the study32 perspective opening and calibrated 42-degree Focus angle. Merged it without conflicts and preserved all its source/evidence. `git diff origin/main HEAD -- Assets Packages ProjectSettings scripts` was empty after integration: executable source matches that incoming validated checkpoint exactly. Its [final export and Chrome verification](../peter-sync/README.md#concurrent-final-remote-update) cover the perspective/compact combination. Our earlier performance and frozen-export receipts retain their original source hashes; they are not relabelled as measurements of the new camera. No force-push, public deployment or visual-gate acceptance.


### Reconciled perspective performance

The exact Peter-sync `7e2117d` export was copied to `Builds/Verified/peter-7e2117d` after verifying all ten hashes against `docs/evidence/peter-sync/final-build.json`. `perspective-served-assets.json` verifies actual HTTP delivery. Runtime source under Assets/Packages/ProjectSettings/scripts is identical to main `22e5a84`; intervening changes are documentation/evidence only. The prior `e25c5d3` backup remains intact.

Three actual 30-second Chrome paths at **1440×900 CSS/render pixels** now cover the new perspective camera. Each records visible/focused DOM at both ends, no visibility changes/blur events, and zero Unity unfocused frames. Own test tab only; other user/teammate tabs and Unity Editors were preserved. No own build or Play Mode was active. These are single-machine observations, not an isolated-machine causal comparison with earlier timings or low-end-hardware certification.

| Path | Average fps | p95 / p99 | Maximum | Frames over 33.34 ms |
|---|---:|---:|---:|---:|
| Opening camera movement | 51.5 | 26 / 36 ms | 80 ms | 37 / 1,545 |
| Close preview, queue, undo and redo | 50.8 | 23 / 26 ms | 36 ms | 1 / 1,524 |
| Seven-project village → opening → overview | 46.6 | 32 / 36 ms | 66 ms | 27 / 1,397 |

The documented stable-30 fallback passes; strict p95 16.7 ms remains unmet. Editing contains 14 preview CPU events (maximum **1.0 ms**) and one placement (**18.7 ms** CPU). Eleven preview presentation opportunities span **17.6–27.4 ms**; placement is **35.2 ms**. No frame or interaction exceeds 200 ms. Input-to-first-rAF measures a presentation opportunity, not physical display latency. [Summary and methods](perspective-performance-summary.json) link the three raw receipts.

The existing seven-project ending restores **85.4 / 99.0 / 54.8**, **$5.240M cash**, **$797K commitments**, revision 26. Its completed village fits the cleared landscape space in `perspective-completed-village.png`; no browser error-level entry was returned. The temporary viewport override was reset and the test tab closed. Material, vegetation and whole-scene gaps remain visible; this performance result does not accept Phase 2A, final rehearsals or public release.


### Matched project ground material — implementation checkpoint

Patterns **TN-02/09/10/12**. The second bounded foundation trial uses the existing Julio meadow/earth textures with the landscape’s controlled color range, filtered brush detail and translated triangular sampling. Grass and earth/path parts retain their original metric meshes; an instance-only material adapter replaces the flat ground colors, including the bounded wind pad. No new asset family, source geometry, budget or global terrain/forest shader change. Two shared project materials are owned by the landscape renderer and disposed with it. `_BaseColor` remains available for annual proposed/suspended/establishing tinting.

Matched running-Editor captures `project-ground-matched-before.png` / `project-ground-matched-editor.png` show the direct texture trial’s bright lawn/muddy repeated plaza replaced by controlled landscape values and subtler brushwork. An initial on-demand capture showed earth shading on both parts; a later capture, after checking the distinct material properties and waiting for rendering to settle, showed the intended separate surfaces. The exact transient cause is unverified; only the settled view is the visual comparison. Actual browser output is recorded below. Hard pad edges, simple buildings and decorative ball trees remain known gaps; this is not whole-scene visual acceptance.

[Project-ground checks](project-ground-check.cs) pass **168 assertions across all ten building/utility variants**: shared metric meshes unchanged, correct surface kinds, exactly two cached materials, proposed/suspended per-instance tint without shared-material mutation, construction hiding, cancellation safety, source-prefab material/color preservation, unchanged live annual state and no project-shader errors. After Play Mode stopped, an independent material census found **zero** remaining project cache materials. Free Aspect was restored. The following Web export verifies the retained shader in the actual player.


Web job `820677aed9674b07a3e360522b17764e` successfully exported source `f78e416` on October 25 at 05:58:32 UTC. [HTTP asset hashes](project-ground-web-assets.json) identify the exact served bytes. Actual Chrome 155 at **1440×900** shows distinct green meadow and muted earth surfaces on the completed village, apartments, park and wastewater project; their screenshots are `project-ground-web-{village,apartments,park,wastewater}.png`. The initial Editor material-selection transient did not appear in these browser views. The completed save remains Year 10, revision **26**, seven assets, zero proposals, **$5.240M cash / $797K obligations**, and **85.4 / 99.0 / 54.8**. [Console receipt](project-ground-web-console.json) contains no error-level entry. Browser viewport override was restored after inspection. This check covers material compilation, representative rendering and unchanged saved state; it does not relabel earlier export timings as performance evidence for this shader. Hard pad edges, simple building/ornamental tree forms and broader scenery gaps remain open.


### Project edge grading — implementation candidate

Patterns **TN-02/09/12**: the project pads' vertical dark walls read as detached blocks. A reversible apartment/village trial replaced the outer three metres of grass/path/asphalt with a slope meeting the existing terrain, wholly inside the original metric footprint. Buildings and other model meshes remain shared and unchanged; only instance ground meshes are rebuilt on a common two-metre clipping grid. Source terrain, layout eligibility, maximum fill rule, occupied area and public finances are unchanged. Wind retains its bounded engineering pad. The original foundation wall stays available during construction; completed projects expose the graded edge instead. The renderer owns and releases generated meshes on cancellation/replacement/disposal.

[Running-Editor checks](project-edge-check.json) pass **394 assertions across all ten building/utility variants**, including exact horizontal bounds, unchanged central elevations and source meshes/materials, finite normals, unchanged analytical terrain and annual state, construction visibility, instance tints and mesh ownership cleanup. Compilation has zero errors/warnings. A post-Play census found **zero** retained graded meshes. Ground geometry ranges from 3,139 vertices for the house to 20,629 for the data center (wind unchanged); the apartment uses 4,728, compared with 64,512 in the rejected recursive-subdivision trial. Individual Editor Apply observations for graded variants span **7.23–14.78 ms**; these are diagnostic CPU observations, not browser input latency or a performance acceptance claim. The actual Web checks below now verify the retained candidate.


Web export `dd7bb36a8c744e3c99a64a55e5d81a2c` succeeded from **`bb4d976`**, completing at **2026-10-25 06:17:57 UTC** (CLI execution 284.691 s). [Anonymous HTTP hashes](project-edge-web-assets.json) pin the actual player. Chrome 155 at **1440×900** verifies the completed apartment and village with their sloped terrain contact (`project-edge-web-apartments.png` / `project-edge-web-village.png`). The seven-project ending remains revision 26, **$5.240M / $797K obligations** and **85.4 / 99.0 / 54.8**. A fresh actual-UI run verifies apartment preview, proposal tint and **$5.600M reservation** without cash payment; undo removes the project and restores the full **$10M** availability; redo restores the proposal. End Year initially remains pending when the chapter request fails (its underlying service cause was not diagnosed), and explicit offline acceptance pays exactly **$5M**, with **$6M** closing cash and the original construction foundation visible. Fast-forward then restores the operational building with graded ground at Year 10: **$15.570M cash / $120K future obligations**, **42.3 / 85.3 / 35.8**, revision 22. Captures `project-edge-{proposed,undo,construction,lifecycle-complete}.png` and [lifecycle console](project-edge-lifecycle-console.json) preserve the observed sequence. No error-level entries were returned for either player. The initial offline acceptance took 46.8 ms including persistence/refresh; this is not a controlled frame-time benchmark. Viewport override was restored and both test players closed.

The earlier browser-verified material export is frozen at `Builds/Verified/f78e416`: [ten-file identity](project-ground-checkpoint.json), **41,431,393 bytes**, including local Vercel configuration. Prior checkpoints remain available. This bounded terrain-contact improvement does not close the forest/whole-scene visual gate, final integrated performance, public complete-run verification or final rehearsals.


### Instanced vegetation annual status

The annual status pass previously reached ordinary Renderer components only, leaving instanced grass/crops unchanged during proposed, establishing and suspended states. A separate InstancedNature appearance API now updates its already-owned material copies, preserving original colors for idempotent updates and reset. Alpha coverage stays unchanged. AnnualStage applies this only to land agreements and wind meadow; remaining source cover around buildings keeps its original appearance. No forest batching/LOD logic, source materials, terrain, finances or save schema changes.

The running Editor passes **209 assertions** across all **eight vegetated agreements** (seven land actions plus wind) and **nine remaining-cover building/utility cases**. Checks include repeated status changes without color drift, full maturity/reset, unchanged alpha, source asset isolation and byte-identical live annual/terrain state. Compilation has zero errors/warnings. [Fixture and results](annual-vegetation-check.json). The game advanced from frame 187 to 1284 during verification.

Matched `annual-vegetation-{mature,establishing}-editor.png` captures use a separate completed cover-crop fixture, then a display-only 25%-establishing override. The camera is the same 1600×900/FOV42/pitch42/yaw−32/distance175 view, paused at frame4652 with explicit instanced batch submission. They verify a subtle foliage-color change; the green ground surface remains unchanged. These are diagnostic Editor views, not a whole-scene transformation or Chrome acceptance claim. Play Mode was stopped afterward. Actual Chrome verification follows below. Patterns TN-05/08/12.

A separate [dry-swale study](swale-study/README.md) rejects narrow soil/planted shoulders and a fully vegetated bed. No swale production change was retained. The prior Chrome-verified edge export is frozen at `Builds/Verified/bb4d976`, with byte identities in `project-edge-checkpoint.json`. Phase 2A and final release gates remain open.


Web job `1ec2480e786641db84d82e8e78d8894a` exported **`e5b8658`** successfully in **291.949 s**, completing **2026-10-25 06:48:15 UTC**. [Served asset identities](annual-vegetation-web-assets.json) match the local export. Fresh Chrome 155 at **1440×900** verifies West field cover-crop preview, queue ($150K reserved / $10M cash unchanged), undo to source crops/zero reserve, redo, accepted Year 1 at **50% establishment**, and mature Year 10. Offline Year 1 closes at **$10.877M / $27K obligations**; Year 10 is **$19.700M / zero obligations**, **53.8 / 58.0 / 43.1**, revision **22**, one asset. The same-camera proposed, establishing and mature captures show the subtle foliage status distinction; the unchanged ground paint still dominates the image. This fixes the missing batch tint, not the broader transformation-quality gate.

The optional local chapter request failed before acceptance. A subsequent read-only health/preflight probe found **no server listening on 127.0.0.1:8787**; no service configuration was changed. Labelled offline acceptance recovered and paid once. [Console receipt](annual-vegetation-web-console.json) contains no error-level entry. Its single-run diagnostics report offline Year 1 acceptance **47.5 ms** and nine-year fast-forward **724.2 ms**, including yielded frames and final refresh; these are functional observations, not a controlled performance benchmark. The current work does not include the visual owner's unpublished forest candidate or a new public deployment.

Reload restores the same Year 10 / revision22 / $19.700M / one-asset ending, with no error-level entry in `annual-vegetation-reload-console.json`. The viewport override was reset and the test tab closed. The local chapter service remains stopped; offline play was the verified path.


### Crop form studies — rejected before integration

The [solid wheat and connected-row comparison](cozy-crop-study/README.md) tests free CC0 COZY FARM wheat with its intended unlit palette against current crop cards at matched close/normal/wide Editor views. The new solid asset remains a repeated bright grid at normal scale; connected standing-row geometry creates artificial ribs and adds 2.58M vertices. Both are rejected. Only reproducible evidence/source inputs outside Assets are retained; no crop production change or Web acceptance claim. Main gameplay/annual state is unchanged, and Phase 2A remains open.


### Native shader property compatibility

The visual owner's native bark shader declares `_BaseColor` as a texture. Annual tint code previously checked only the property's name, then read/wrote it as a color. Both the instanced-material path and ordinary Renderer path now require `Material.HasColor`; unsupported native properties and existing property blocks stay intact. Supported color materials keep the previous appearance and alpha behavior. No imported asset or rendering/batching change.

[15 focused Editor assertions](tint-property-type-check.json) exercise a real shader with a texture named `_BaseColor`, alongside a color material: texture/source isolation, supported tint/reset/idempotence/alpha, no invalid color-cache entry, unchanged ordinary-renderer property blocks through all three tint statuses, and byte-identical annual state/terrain. [209 existing annual assertions](tint-property-type-regression.json) also pass; game frames 3367→3385 advanced. CLI recompile reports zero errors/warnings; the current Editor console has zero errors/warnings (its retained historical buffer includes earlier build/import warnings). Temporary fixture shader was removed after Play stopped. Replay sources: `terra-tint-fixture-import.cs` then `terra-tint-type-check.cs`. Browser export verification follows separately; earlier player captures do not verify this source change.


The [property-type Web export](tint-property-build.json) succeeded in415.126s, completing at2026-10-25 07:24:23 UTC. Runtime source matches **3ce7df9**; the recorded generated TMP fallback input is the only substantive local asset difference. [Nine anonymous served files](tint-property-web-assets.json) match exactly (**41,411,377 bytes**, excluding hosting configuration). Actual Chrome155 at1440×900 verifies West-field restoration preview, $95K queued reserve with unchanged $10M cash, undo to source crops/full availability, redo, failed chapter request without payment, explicit offline Year1 acceptance,33% establishment, confirmed fast-forward,100% maturity and reload. Year1 closes at **$10.923M / $18K obligations**, scores **41.3/58/43.6**; Year10 closes at **$19.755M / $0 obligations**, **41.3/58/46.6**, revision22, one asset and no proposals. Reload preserves that exact state. [Console](tint-property-web-console.json) and [reload receipt](tint-property-reload-console.json) contain no error-level entry. Full-resolution JPEG captures are `tint-property-{proposed,undo,establishing,mature,ending,reload}.jpg`. Current plant tint remains subtle; this is not a scenery-quality acceptance or performance sample. Native texture-name collision is covered by the Editor fixture; the visual owner's bark assets are not part of this export.

The run exposed a separate confirmation-copy issue: from Reviewing, Ten-year future calls the general Forecast helper, which returns the already accepted report. It labels that accepted balance as expected next-year closing cash (`tint-property-review-future.jpg`). The transition/ledger completes correctly; the confirmation forecast needs a detached next-planning-year preview. This is an open follow-up, not a failed tint regression. Prior e5b8658 export is frozen at `Builds/Verified/e5b8658`, with [file identity](annual-vegetation-checkpoint.json). Viewport restored and test tab closed.
