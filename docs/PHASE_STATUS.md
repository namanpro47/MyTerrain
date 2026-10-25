# Terra — phase status and evidence

**Browser preference — October 24, 2026:** The user confirmed Chrome is sufficient throughout this project. Use Chrome for current and future phase acceptance; do not launch or retest Safari or request Private Browsing unlocks unless explicitly asked. Phase 2 Safari retesting was waived. Earlier Chrome/Safari records below remain historical evidence, not recurring requirements.

**Last updated:** October 24, 2026, annual gameplay planning adopted; Phase 2A still unaccepted. This ledger tracks observed implementation; [the spec](../TERRA_PROJECT_SPEC.md) tracks intended vision and [the plan](../IMPLEMENTATION_PLAN.md) defines gates.

**Phase 0 is complete.** Existing Editor/Play-mode/debugger setup evidence was reused. A fresh interactive Web export and actual public Chrome/Safari checks now pass. Phase 1 is complete; Meadowbrook Valley is the game name for the selected Wisconsin AOI. Phase 2 is complete; Phase 2A is now in progress under its separately activated goal; later live-game gates remain unaccepted. Separately prepared editing/packages and remote annual/art work are recorded below; no new runtime work is certified by the planning revision.

**Current isolated visual candidate — study29c:** Shallow rendered swales replace raised ribbons; meadow colonies and quieter ground are in the Chrome build. Renderer reversibility/seam checks, exact serving checks and Restore/swale/undo pass. One moving path p95=31ms but max529ms/two>200ms; performance fails. Whole-scene visual gate still fails and the annual lifecycle path is not covered by these legacy renderer checks. [Current review](CURRENT_VISUAL_REVIEW.md) · [build/source/browser evidence](evidence/phase2a/visual-rebuild/study29-build.json) · [integration boundary](VISUAL_WORK_HANDOFF.md).

| Phase | Status | Evidence / missing gate |
|---|---|---|
| 0 — Browser gate | Gate passed | Current-source export, public anonymous access, Chrome/Safari interaction checks, captured evidence and source checkpoint recorded below |
| 1 — Rural selection/foundation/contracts | Gate passed | Selected AOI, styled synthetic fixture, public browser checks, 27 contract checks, frozen v1 contracts and runnable bundle |
| 2 — One real rural landscape | Gate passed | Real pack/provenance, 34 geography + 139 Unity checks, 12 sites, Chrome state matrix and measured public performance; [audit](PHASE_2_AUDIT.md) |
| **2A — Visual rebuild from first principles** | **Visual reset; current direction rejected** | [Rewritten goal](PHASE_2A_GOAL.md), [visual studies and resources](VISUAL_REBUILD.md), [persistent reference](DESIGN_REFERENCE.md); requires rebuilt scenery, matched browser comparisons, every visual category ≥4.5/5, and supporting delivery checks |
| 3A/3B — Annual integration / three-year browser loop | Revised target; not accepted | Reconcile existing editing and Peter’s remote annual module; one ledger, current-year queue, meters, End Year, report, Wait and reload in the live scene |
| 4A/4B — Full catalog/meters and consequences | Planned | All twelve chart families, operational building/service effects, complete rubric, spatial runoff and annual drivers required |
| 5 — Ten-year balance | Planned | Distinct viable strategies, stress cases, fast-forward equivalence and novice observations required |
| 6 — Future comparison and replay | Planned | Same-driver no-new-action future, final world/history, optional learning and new-run replay required |
| 7 — Polish | Planned | Visual/interaction review and measured performance required |
| 8 — Release/refinement | Planned | Full single-location build, rehearsed demo, and focused refinement backlog |

## Annual direction and action-table adoption — October 24, 2026

- **Status:** planning/context revision complete; no annual HUD, new recipes, runtime schema change, Unity build, browser test or deployment performed in this task. Phase 2A remains failed/unaccepted. Historical evidence below stays intact.
- **User direction:** make Peter's annual loop core, including public action budgets, effects and three meters. Updated the canonical spec, rewrote future phases, and added [ACTIONS_AND_EFFECTS.md](ACTIONS_AND_EFFECTS.md) with all 12 rows, original prices/ratings, $3M renewables correction, tentative tax assumptions, timing/commitments, conditional effects and versioned meter design.
- **Remote inspection:** fetched `origin/main` at `1bcc254`. Read annual contracts/runtime handoff, source PRD and action-art handoff/catalog. `a4ba01e` provides annual state/ledger, `080f692` accepted summaries/Editor playtest, `1bcc254` metric action art. These remain remote modules, absent from this local branch; only the PRD text was restored locally with an adoption notice. Reported remote tests were not rerun here.
- **Existing local packages/owners:** [PARALLEL_INTEGRATION.md](PARALLEL_INTEGRATION.md) records B/D/C3 delivered; [PARALLEL_KICKOFF.md](PARALLEL_KICKOFF.md) records separately authorized Phase 3 editing. They require reconciliation with the annual model, not a second dispatch. No message was sent to another worker.
- **Material decisions:** one annual authority; queue-only undo/refunds; all chart families required in the same AOI; no fixed /5 meter rewards; deterministic offline core; optional narration, no generated ending artwork. Existing prices/effects remain labelled game assumptions rather than local science/cost forecasts.
- **Next handoff:** finish the current visual gate; review/integrate annual and editing work for Phase 3A, then prove three years in Chrome (3B). Phase 4 completes the catalog/rubric and spatial consequences before ten-year balance (5) and comparison/replay (6). Existing independently authorized work may proceed without claiming live-phase acceptance.
- **Design references:** TN-01/03/04/05/08, CS-01/02/04, AX-01. No new browser comparison is claimed for a documentation-only revision; later gates explicitly require real Chrome output versus an accepted Phase 2A benchmark.
- **Verification:** [planning audit](evidence/planning/annual-gameplay-2026-10-24.md) records source/table reconciliation, local-link checks, diff hygiene and preservation of unrelated working changes. No runtime test counts are inferred from this document review.

## Current Phase 2A checkpoint — study28e

[Current visual review](CURRENT_VISUAL_REVIEW.md) keeps actual Chrome output beside Terra Nil. Free JulioVII material family, alpha-safe foliage and matching shadow/depth passes, closer orthographic framing and nonrepeating paint sampling tested in Chrome.28d Restore/undo passes; moving samplep95=123ms initially, then33ms after superseded visual tabs closed; full performance gate pending.28e archives21 unused resources without deleting them; exact checked transfer36.79MB. **Visual gate remains failed:** composition, sparse communities, mechanical cultivation and swale construction are substantial gaps. No4.5 ratings or current public/annual acceptance. [Evidence](evidence/phase2a/visual-rebuild/README.md) · [integration handoff](VISUAL_WORK_HANDOFF.md).

## Fresh-build publication — October 24, 2026, 18:12 local

The user requested the latest source build because production still showed the older export. A primary-checkout Web build was already running when this task began; its latest Unity BuildReport confirmed success in 87.95 seconds, zero errors and eight warnings, ending at 18:12 America/Los_Angeles. Published a frozen copy at `Builds/Publish/2026-10-24-1812` to the existing project, deployment `dpl_BgtA89SrWRVm8WXkE5Cr7dMeQ4x9`, at [terra-browser.vercel.app](https://terra-browser.vercel.app/).

All 11 anonymous game-file checks passed exact byte equality (35,019,037 bytes), gzip decoding and WASM validation. Chrome displayed the newer sculpted trees, layered creek, ground detail and close camera, consistent with the local study19 capture. Tab/Enter changed production/credits from 6/60 to 17/36; Z restored 6/60. No console errors were captured. Existing FSR shader and Content-Length warnings remain; the fresh reload reported 11,980 ms to interaction. This is a publication smoke test under TN-01/02/05 and AX-01, not visual acceptance or an annual-gameplay integration claim.

[Build report](evidence/deployment-fresh-2026-10-24/build.json) · [Deployment identity](evidence/deployment-fresh-2026-10-24/identity.json) · [Exact public asset checks](evidence/deployment-fresh-2026-10-24/public-assets.json) · [Chrome capture](evidence/deployment-fresh-2026-10-24/chrome-live.png) · [Browser log](evidence/deployment-fresh-2026-10-24/browser-log.json). Earlier statements below about an unpublished candidate are historical; this publication supersedes the earlier hosting refresh.

## Hosting refresh — October 24, 2026 (historical)

At the user's request, republished the latest completed `Builds/Web` export (15:41 local) to the existing `terra-browser` Vercel project. Production: [terra-browser.vercel.app](https://terra-browser.vercel.app/), deployment `dpl_FQ2Yv3sQyZ7M6VPDEfPaUDpYWVxW`. [Identity](evidence/deployment-2026-10-24/identity.json) · [anonymous asset verification](evidence/deployment-2026-10-24/public-assets.json) · [Chrome capture](evidence/deployment-2026-10-24/chrome-live.png).

All 11 checked files returned HTTP 200 and matched the local export, including gzip decoding and WASM validation. An initial check during alias propagation saw the previous HTML; the subsequent complete check passed. Chrome rendered the landscape; Tab/Enter placed West field (6→17 illustrative units, 60→36 credits), and Z restored 6/60. No console errors were captured. The Unity-ready log reported 16,845 ms; this is a startup observation, not a performance acceptance pass. This hosting task did not rebuild Unity or certify ongoing visual-rebuild source changes or Phase 2A acceptance. Existing phase evidence below remains historical.

## Historical Phase 2 handoff (superseded by current records above)

- **Next action:** rebuild Phase 2A visuals from first principles; compare new scene/camera/asset studies before refining the interface. Phase 3 is not authorized.
- **Goal outcome:** Phases 0–2 complete. Phase 2A implementation is in progress; later phases have not started.
- **Parallel packages:** package A geography work was completed locally by the primary engineer for Phase 2; none delegated.
- **Phase 2 result:** real `meadowbrook-v1` pack ingested; 34 source/alignment/determinism checks and 139 Unity contract/site checks pass. Final Chrome reviews at 1280×720 and 1440×900, transformations/rejections/undo, source panel, recovery and measured performance pass. [Geography](geography.md), [requirement-by-requirement audit](PHASE_2_AUDIT.md).
- **Latest verified build:** `Builds/Web/`, 14,696,090 bytes; [public real landscape](https://terra-browser.vercel.app/), runtime source `35fb7e8edf7c92b37acebfee5656b0bf4991544b`, deployment `dpl_GKsY1WYbABEgANa7at2uDsq3JRs8`. [Identity](evidence/phase2/identity.json). Portable checkout `Builds/Handoff/terra-phase2.bundle`; no Git remote/push.
- **Scope decision:** one rural location is the primary product. Complete and refine all game systems, UI/UX, learning, and replay there through phases 0–8. Former expansion phases 9–11 are removed from the active roadmap.
- **Location decision:** **Meadowbrook Valley** (fictional game name), `coon-valley-brye-v1`, approximately 0.97 km² in Wisconsin. [Selected bounds, data path and source previews](location-selection.md).
- **Expansion:** additional rural/city levels and live generation are deferred until the full one-location quality gate and a separate explicit decision.
- **Working scenario:** illustrative productive land versus restoration and runoff; frozen v1 treatment details are in worldpack-format.md. No production scalability or seasonal farm simulation is required.

## Phase 2A visual reset — October 24, 2026

**Historical pre-creek candidate: study16.** A free sculpted-oak sample replaces the noisy card canopy in the candidate renderer; artist maps remain intact in matte material copies. Chrome normal/close/overview and Cultivate/Restore/undo checks passed; art credits are visible. One 30-second route returned **9ms p95**, but it covered only one placement and does not certify the full performance suite. Exact export checks passed. The **visual gate still fails**: repeated crowns, mechanical fields, flat water, basic ground/road edges and dense shadows. [Captures, limitations and next comparison](evidence/phase2a/visual-rebuild/README.md#study-16--solid-sculpted-oak-family-playable-candidate-gate-still-failed). Public deployment remains the earlier build.

The user explicitly rejected the current visual quality as far below the references and asked for a first-principles rebuild, with permission to replace entire existing aspects. The [rewritten goal](PHASE_2A_GOAL.md) prioritizes direct rendered quality. [VISUAL_REBUILD.md](VISUAL_REBUILD.md) tracks the new direction and resource investigation. No visual acceptance, reference parity, or phase completion is claimed.

New [visual rebuild studies](evidence/phase2a/visual-rebuild/README.md) include licensed assets, actual Chrome comparisons, continuous blended ground, inset illustrative water/banks, a perspective camera and original painted trees. Study 08 fixed a missing browser shader variant and verified a real click/undo transaction, but steep tilt visibly flattened the tree cards. Study 09 therefore tests replacing free orbit with a fixed illustrated angle and pan/zoom, plus matching original painted shrubs. These are active art decisions, not an accepted contract. Field repetition, stream/bank integration, whole-scene consistency and final performance/delivery remain open.

The earlier compact HUD, vector tool miniatures, canopy grouping, procedural ground grain, keyboard/contrast/viewport work and captures remain recoverable experiments. They are **a rejected baseline**, not an accepted art contract. The earlier benchmark and 139 state/geography checks are historical engineering observations, not evidence that the visuals pass. The previous brief is [archived](evidence/phase2a/superseded-ui-first-goal.md).

**Latest user correction:** free resources only, no further image generation in this pass. Study10 evaluated Elijah Cobden's supplied Nature demo in live Unity; its birch canopy was not selected. Study11 exported and was checked in Chrome: textured wheat failed at elevated scale, while restore/undo returned the expected 5/42 then 6/60 measures. A startup exception remains untriaged; no clean-console or performance certification. Study12 replaces the candidate default canopy with free 3D oak meshes using supplied shader/textures and adapted proportions/palette, and tests free geometric wheat. Required art credits are in the Field guide. Its browser export succeeded in 65.236 seconds and Chrome default/close/placement/credits checks are recorded. Cultivate/undo returned 17/36 then 6/60; 139 contract checks passed. The close canopy and sparse wheat still fail the visual bar; one 25.15-second load exceeds the target. A stationary 30-second diagnostic is not final performance acceptance. [Current study ledger](evidence/phase2a/visual-rebuild/README.md). Generated cards were a historical toggle in study12; study14 removes that toggle and archives those resources. The scene remains below the gate; primary agent owns this rebuild, with no parallel assignment or later-phase work.

## Phase 2A planning and persistent design research — October 24, 2026

- **Requested change:** establish a Terra Nil-led visual/UX quality gate after Phase 2, before more gameplay features, and keep a reusable game-design reference visible throughout implementation.
- **Prepared:** [DESIGN_REFERENCE.md](DESIGN_REFERENCE.md), with dated source register, inspected reference visuals, TN/CS/AX pattern IDs, Terra gap assessment and later-phase review procedure; [PHASE_2A_GOAL.md](PHASE_2A_GOAL.md), with scoped work slices, copyable kickoff, hard gates and required evidence.
- **Project integration:** AGENTS, README, spec section 7, implementation dependency map, art contract and package handoffs point to the reference. Phase 3 now depends on Phase 2A; Phase 7 preserves the established quality across the full game.
- **Research evidence:** official Terra Nil product material and sampled publisher tutorial; versioned WSGF desktop captures; original Cities: Skylines manual interface diagram; dated Cities: Skylines II official redesign articles/image; W3C contrast/target guidance. Reviewed the existing initial Phase 2 wide/close browser captures and relevant HUD/gameplay/rendering source. This is desk/visual research, not installed-game testing or a new Terra browser acceptance run.
- **Execution status:** no Phase 2A implementation, export, deployment, goal or delegation started by this planning task. Phase 2's ongoing implementation remains intact. Internal/external reference-quality acceptance is **not yet tested**. Historical Phase 1 scores do not satisfy Phase 2A's new rubric.
- **Planning verification:** checked local Markdown links/anchors and code fences across all ten affected planning/reference documents, Phase 2 → 2A → 3 ordering and diagram dependencies, all 14 reference pattern headings, and `git diff --check`; all passed. No runtime test result is claimed for these documentation changes.

## Location naming update — October 24, 2026

The user requested a new name. **Meadowbrook Valley** replaces the old player-facing name across the runtime and active plans. The real AOI, stable internal ID and provenance are unchanged; [naming policy](location-selection.md#name-and-provenance). Earlier evidence and screenshots below retain the names actually shown at capture time. This is a copy update after Phase 1, not the start of Phase 2.

Recompiled with 0 errors/warnings; Web rebuild succeeded in 63.44 s (13,679,471 bytes, 0 errors, 7 nonblocking compiler/toolchain warnings). Published deployment `dpl_EqdnXtyTrgUJg3NKyDXjnrWWc3mP`; all public assets match the export. Chrome 1280×720 confirms the new header and full modal copy fit, with no Console errors. [Verification](evidence/naming-2026-10-24/checks.json), [header](evidence/naming-2026-10-24/header.png), [modal](evidence/naming-2026-10-24/modal.png). The portable handoff bundle was refreshed with the naming policy and source update.

## Phase-0 preparation check — October 24, 2026

- Read setup import/build summaries; the recorded export succeeded at 12,510,477 bytes.
- Confirmed Editor/Web Build Support directories, enabled sample scene, build entry point, and exported loader/data/framework/WASM files.
- Read gzip-decoded prefixes and verified the WASM header; this did not rerun the browser.
- Vercel CLI authentication succeeded. No project link exists in this checkout and no public deployment URL is recorded; deployment remains unverified.
- Existing local Chrome and Editor/debugger checks are supported by SETUP.md's prior record. A fresh export from current source, usable camera/input, and public Chrome/Safari checks remain for the goal.
- Prepared [PHASE_0_GOAL.md](PHASE_0_GOAL.md). No goal, build, deployment, or phase-1 work was started by this preparation.

## Phase 0 — acceptance evidence, October 24, 2026

- **Status:** gate passed. Reused the recorded toolchain, Editor Play-mode, debugger, license, and toolkit setup. No reinstall, package upgrade, delegation, or location selection was needed.
- **Working behavior:** a platform with three colored primitives; bounded camera orbit/zoom, pan implementation, reset/help, keyboard alternatives, and a responsive browser wrapper. It is explicitly labeled a technical fixture. The enabled scene remains `SampleScene`; the toolkit gallery is preserved separately.
- **Source checkpoint:** `7ae591c54d1796997fcf83ce2f3b99a3b8b8e4c1`. Subsequent changes are deployment verification scripts/docs/evidence, not the exported runtime. Initial baseline `2432b7d` preserved existing Unity assets/settings. Generated builds, logs, Burst intermediates, performance-test metadata, and host links are ignored. Separate toolkit documentation/scripts were left as existing unrelated work.
- **Build:** live Editor command in [deployment.md](deployment.md), job `88e23fe8ef624732b98f3361dfbaf732`; Unity 6000.6.6f1; succeeded in **65.68 s**, **12,777,098 bytes**, **0 errors / 1 warning**. [Build report](evidence/phase0/build.json). The previous successful export remains in `Builds/Web.previous`.
- **Deployment:** project `terra-browser` in `namanpro47s-projects`; production [https://terra-browser.vercel.app/](https://terra-browser.vercel.app/), deployment `dpl_9RFbj2UR246HW2zEb5VEfb6RT2de`. [Identity including immutable deployment URL](evidence/phase0/identity.json). Only generated Web files were uploaded.
- **Serving checks:** `python3 scripts/verify-web.py http://127.0.0.1:8080` and `python3 scripts/verify-web.py https://terra-browser.vercel.app` passed. HTML, loader, data, framework, and WASM all returned **200**, exactly matched the export, and passed gzip/WASM format checks. [Local hashes](evidence/phase0/local-assets.json), [public hashes and headers](evidence/phase0/public-assets.json). The host serves `.unityweb` as `application/vnd.unity` without Content-Encoding; Unity's gzip fallback successfully decodes it in both browsers.

| Browser / surface | Observed acceptance checks |
|---|---|
| Local Chrome 155.0.8059.85 | Rendering, drag orbit, scroll zoom, 1280×720 resize, help click isolation, keyboard focus recovery, reset, and reload passed. [Checks](evidence/phase0/local-checks.json). |
| Public Chrome 155.0.8059.85 | Fresh tab loaded the actual URL. Orbit changed yaw/pitch from 35/35 to 85/45; wheel changed distance 15→20.2; keyboard zoom reached both 8 and 24 bounds. Help-focused arrow input left the camera unchanged; Reset restored the default, and refocused arrows moved it. Reload restored default state. Reviewed 1280×720 and 1440×900 captures. [Checks](evidence/phase0/chrome-checks.json), [Console](evidence/phase0/chrome-console.json), [1280 capture](evidence/phase0/chrome-public.png), [1440 capture](evidence/phase0/chrome-public-1440.png). |
| Public Safari 26.5.2 | **Private Browsing, without Vercel login**, loaded/rendered the same export. Native drag and scroll produced 85/45 and distance 20.2; keyboard zoom reduced distance to 12.9. Help-focused arrows left state unchanged; Reset and keyboard focus recovery worked (yaw 62.4). Public reload returned 35/35/15. Docking/closing Web Inspector resized the viewport; rendering remained proportional and keyboard input worked in the reduced view (yaw 59). [Private capture](evidence/phase0/safari-public.png), [resized view/network capture](evidence/phase0/safari-resized.png), [orbit](evidence/phase0/safari-orbit.txt), [focus](evidence/phase0/safari-focus.txt), [Console](evidence/phase0/safari-console.txt), [network](evidence/phase0/safari-network.txt). Native window captures are 2982×2170 physical pixels; no exact CSS viewport size was recorded. |
| Firefox | Not installed; not tested (optional). |

- **Errors and fixes:** initial shader-by-name lookup was stripped and caused magenta rendering/startup failure. An explicit Resources material fixed it. Native primitive factory collider dependencies were then preserved with `link.xml`. Browser pointer gestures now use CSS-pixel DOM events forwarded to Unity, fixing the observed Safari drag/scroll issue. Final Chrome and Safari Consoles show **no errors**; all critical network requests finish successfully.
- **Nonblocking warnings/coverage:** one CS0414 build warning for `panning`, used only in the Editor input branch; optional URP depth-of-field/Panini/upscaling shader messages; Safari's `WEBGL_polygon_mode` portability warning. None prevents rendering/input. The accepted minimal camera path is orbit + zoom + reset. Shift/middle-drag pan is implemented but was not independently verified with the available modifier-drag automation. Scroll events were exercised; a person using a physical trackpad was not separately tested. No novice or educational playtest is claimed for this technical fixture.
- **Observed load behavior:** on the M1 Max / macOS 26.5.2, public Chrome logged loader readiness at **8.018 s** initially and **3.736 s** after reload; scene-ready followed on the next advancing Unity frame. Safari logged **2.785 s** on the observed public reload. Safari Inspector showed data 1.07 s, WASM 2.15 s, and framework 969 ms on a further reload. These are individual observations with differing cache/background states, not a controlled benchmark or the later rural-game performance gate.
- **Visual review:** colored shapes, intact shadows, readable help/reset/status, and a proportional canvas at tested sizes. This passes the small Phase 0 fixture scope; full rural art direction and game HUD remain Phase 1+ work.
- **Acceptance:** all required items in [PHASE_0_GOAL.md](PHASE_0_GOAL.md) pass. Rebuild and publication are documented, the source/deployment/hash association is recorded, and captures are committed. **Stop before Phase 1.** No AOI, gameplay, contracts, or additional level was authored.

## Phase 1 — acceptance evidence, October 24, 2026

- **Status:** gate passed; [requirement-by-requirement audit](PHASE_1_AUDIT.md). No Phase 2 work started and no packages delegated.
- **Selected location:** Coon Valley / Brye Lane, Wisconsin, `coon-valley-brye-v1`, WGS84 `[-91.043,43.697,-91.031,43.706]`, about 0.97 km²; offline export CRS EPSG:26915. Three candidates were compared, with source previews and actual USGS elevation product availability, NAIP imagery and NLCD coverage checks. [Selection, sources, rights and remaining ingestion gaps](location-selection.md). Candidate previews are research, not authored levels. No real elevation raster was ingested.
- **Working slice:** `TerraMain` loads the versioned three-file JSON pack through URLs, renders an explicitly synthetic hillslope/cover/path/connection fixture with three editable sites and a protected woodland, and binds a uGUI/TMP HUD. Hover/keyboard preview, ghost/blocked outlines, placement, cancel and undo work. Baseline production, displaced capacity, credits and cover areas are calculated; runoff remains visibly **Pending**. Open plot, 75% productive living margin, meadow restoration and an eligible connected-swale recipe are frozen as fictional test assumptions, not real farm prescriptions.
- **Source/export identity:** `a78f49cf401a6ff602676b35f23c84fb5866ae93`. Runtime implementation is `fbab39f`; the next commit preserves the Input System preload automatically added during the final build. [Build report](evidence/phase1/build.json): Unity 6000.6.6f1, **60.46 s**, **13,678,532 bytes**, **0 errors / 4 warnings** (one unused Phase0 pan field, three TMP compilation-splitting notices). Later commits contain documentation/evidence only.
- **Publication:** [public fixture](https://terra-browser.vercel.app/), deployment `dpl_ANHXMJ3bUG2NWv3LUybAH4ZfcVc5`, existing Vercel project/scope reused; [identity](evidence/phase1/identity.json). Only generated Web output was uploaded. HTML, loader/data/framework/WASM and all three pack files returned anonymous HTTP 200 and matched local bytes exactly: [local check](evidence/phase1/local-assets.json), [public check](evidence/phase1/public-assets.json).
- **Automated correctness:** **27 checks pass** through the actual Unity serializer and pure core, including nine malformed packs, round trip, preview purity/equality, stale revisions, protection, overspend, replacement/displaced production, area conservation and exact undo: [results](evidence/phase1/contract-checks.json). The independent scalar simulation I/O example passes at 25.42162632835775 m³ generated, 8 m³ retained, zero disconnected retention: [result](evidence/phase1/simulation-example-check.json). This checks the interface example, not an integrated simulator.
- **Editor/independent art:** main Play-mode frames advanced and the fixture initialized at 6 units/60 credits. The separate toolkit preview shows 24 specimens, 24 completed animations and 10 clips; [check](evidence/phase1/art-preview-check.json), [capture](evidence/phase1/art-preview.png). It is not an enabled export scene. Editor remains stopped with TerraMain open.
- **Loading/error:** real loading capture and a temporary unsupported-version file in the generated local export produced a readable error; restoring the exact file and pressing Retry recovered 6/60. Authored JSON was unchanged. [Error/recovery record](evidence/phase1/error-recovery.json), [error capture](evidence/phase1/invalid-pack.png), [loading](evidence/phase1/loading.png).

| Browser | Actual final-export evidence |
|---|---|
| Chrome 155.0.8059.85, public | Reviewed **1280×720 and 1440×900** default/hover/blocked/placed/modal states. Pointer open plot 6/60→18/36; keyboard margin 6/60→15/30; undo restores baseline. Protected/ineligible placement rejected; eligible swale 2 units/32 credits. Modal isolation, cancel, reduced motion/mute, drag isolation, orbit/wheel/reset, resize and reload pass. Refocused keyboard commit/undo works after reload. [Checks](evidence/phase1/browser-checks.json), [Console](evidence/phase1/chrome-console.json). **0 errors**. |
| Safari 26.5.2, public Private Browsing | No Vercel login. Render, pointer Valley-field placement (12/36), Tab/M/Enter margin placement (21/6), two undos (6/60), Escape cancellation and drag/wheel/reset work. [Placed](evidence/phase1/safari-placed.png), [undo](evidence/phase1/safari-undo.png), [camera](evidence/phase1/safari-camera.png), [Console](evidence/phase1/safari-console.txt), [empty errors filter](evidence/phase1/safari-errors.txt). Native captures 2982×2170; exact CSS viewport not recorded. Native window focus was intermittent; the later address-bar focus test is inconclusive and is not claimed. |

**Visual review:** [1280 default](evidence/phase1/public-1280-default.png), [hover](evidence/phase1/public-1280-hover.png), [blocked](evidence/phase1/public-1280-blocked.png), [placed](evidence/phase1/public-1280-placed.png), [modal](evidence/phase1/public-1280-modal.png); [1440 default](evidence/phase1/public-1440-default.png), [hover](evidence/phase1/public-1440-hover.png), [blocked](evidence/phase1/public-1440-blocked.png), [placed](evidence/phase1/public-1440-placed.png), [modal](evidence/phase1/public-1440-modal.png). [Condensed actual interaction GIF](evidence/phase1/interaction.gif), [capture timing/method](evidence/phase1/interaction/README.md).

The weakest initial areas were scene composition and feedback readability: sparse protected woodland, weak field identity, unbacked small feedback and success styling that resembled rejection. The final iteration clusters trees over the protected footprint, adds field rows and raised banks, reframes the terrain, backs the feedback/hint with cream panels, and separates success from blocked states. Final internal scores: **scene appeal 4/5, legibility 4/5, feedback 4/5, ease 4/5, coherence 4/5**. Terrain remains dominant and modal/tooltips fit both sizes. This is a successful foundation, not final-game art or human usability research.

- **Measured load observations:** public Chrome loader-ready **7.283 s**, reload **4.256 s**; Safari **2.801 s**. Scene-ready follows Unity's startup/splash. These are single runs with differing cache/background states, not a benchmark. No controlled FPS/latency or novice/educational playtest claimed; those are later gates.
- **Nonblocking limits:** optional URP postprocess/upscaling shader warnings; Safari WEBGL_polygon_mode warning; Chrome UnityCache Content-Length advice for streamed responses. Byte checks and actual browser execution pass. Firefox unavailable; physical trackpad and Shift/middle pan not separately tested. Move/remove/redo, real geography, mission/learning flow and integrated hydrology remain later-phase work.
- **Frozen handoff:** WorldPack/runtime/art **v1**, DTO schema **1**, fixture **1.0.0**, three canonical hashes in `worlds/fixture-v1/checksums.sha256`. [WorldPack](worldpack-format.md), [runtime](runtime-contracts.md), [art](art-direction.md). [Ownership and runnable commands](PARALLEL_WORK.md). A complete source bundle cloned cleanly and passed packaging/checksums and scalar simulation example; [handoff verification](evidence/phase1/handoff-check.json). The final local bundle also includes this evidence. No remote or message to another engineer was created.
- **Stop boundary:** Phase 1 complete. Phase 2/package A is the next separately authorized task on this same selected AOI; it has not begun.

## Evidence entry template

Copy this section for a phase when work starts. Keep notes brief and link concrete files/captures/logs rather than claiming success from a checklist alone.

### Phase N — title

- **Status:** planned / in progress / awaiting dependency / engineering gate passed, human feedback pending / gate passed.
- **Objective and scope:**
- **Working behavior:**
- **Changed paths / integrated packages:**
- **Commit and deployed build URL:**
- **Automated correctness checks:** command, result, relevant cases.
- **Browser checks:** browser/version, viewport, input path, console/network findings.
- **Design review:** capture paths, weakest item, improvements, final assessment.
- **Design reference:** relevant TN/CS/AX pattern IDs from DESIGN_REFERENCE.md, matched reference/build states, retained Phase 2A benchmark, deliberate deviations; separate internal visual judgment from external human review.
- **Measured performance:** hardware, build, download/load time, frame time and interaction latency where relevant.
- **Human playtest:** tester count/context, observed difficulty, revisions, retest. Write “not tested” when unavailable.
- **Assumptions, known gaps, and blockers:**
- **Acceptance gate:** passed criteria and any unmet criterion; do not mark passed while required evidence is absent.
- **Next handoff:** next bounded task, fixture/contract versions, dependency owner.

## Tooling setup evidence — October 24, 2026

This setup pass adds tooling and an isolated asset preview; it does not certify gameplay phases or public deployment. The earlier setup-status mismatch is resolved in `SETUP.md`.

- Official Unity agent plugin/skills, Pipeline, and local MCP installed. MCP handshake and a C# eval returning editor version succeeded.
- DOTween Free, 24 nature prefabs with shared URP materials, and 10 sound clips imported. Full source archives and licenses retained.
- Editor compilation: zero errors/warnings. Live preview: 24/24 animations completed while frames advanced.
- Toolkit Web build: succeeded, 12,913,018 bytes, zero build errors/warnings. Chrome rendered the assets and replayed all 24 animations; a sound button triggered playback. No browser errors observed; optional URP spatial-upscaling shader warning recorded.
- Guide: `docs/HACKATHON_TOOLKIT.md`. Evidence: `Logs/Setup/toolkit-verification.json`, `toolkit-build.json`, `toolkit-mcp.json`, and `toolkit-browser.png` (local, Git-ignored).
- A later read-only Editor query timed out while another build was running; that build was left untouched.


## Phase 2 — real landscape completed October 24, 2026

[Full acceptance audit](PHASE_2_AUDIT.md) maps every numbered requirement and the visual/performance gates to actual source, browser and handoff evidence. The final pack contains real terrain/cover/mapped context, ten editable hypothetical sites and two scenario no-edit zones. Source geography and illustrative decoration/management are distinguished in the two-page Data & Assumptions panel.

- **Build/public identity:** `35fb7e8edf7c92b37acebfee5656b0bf4991544b`; 64.452 s, 14,696,090 bytes, 0 errors/7 nonblocking toolchain warnings. Deployment `dpl_GKsY1WYbABEgANa7at2uDsq3JRs8`. All 11 [anonymous public assets](evidence/phase2/public-assets.json) match [local export hashes](evidence/phase2/local-assets.json).
- **Correctness:** [34 geography checks](evidence/phase2/geography-checks.json), [139 Unity checks](evidence/phase2/contract-checks.json). The original contract fixture still passes; schema v1 remains frozen. One selected AOI; no extra playable level.
- **Actual Chrome review:** both target viewport state matrices, wide/close crop/margin/restore/swale views, protected/ineligible rejection, exact preview/commit/undo accounting, source modal isolation, resize/reload/refocus, camera input and local invalid-pack Retry pass. [Browser checks](evidence/phase2/browser-checks.json), [Console](evidence/phase2/chrome-console.json), [sampled interaction sequence](evidence/phase2/interaction.gif). No Console errors. Chrome is sufficient; Safari was not retested.
- **Measured performance:** final 30.008 s foreground run, 120.03 average fps, p95 9 ms / p99 10 ms / max20 ms; no frames >33.3/200 ms. Preview handler ≤0.6 ms, placement3.5 ms; sampled DOM-to-rAF preview≤15.2 ms, placement13.6 ms. First usable interaction8.282 s on the changed export. [Methods and limitations](evidence/phase2/public-performance.json); these are single-machine observations, not physical display latency or a controlled benchmark.
- **Internal foundation scores:** scene appeal4, legibility4, feedback4, ease4, coherence4. Initial framing, Web render scale/shadows and later header-over-trees contrast were fixed. No claim of Terra Nil parity or novice evaluation; the prepared Phase 2A gate addresses compact HUD hierarchy, richer canopy/treatment detail, illustrated tools, grounded reveal, accessibility and broader validation.
- **Handoff:** [current runnable bundle instructions](PARALLEL_WORK.md#current-phase-2-local-handoff), [fresh-clone evidence](evidence/phase2/handoff-check.json). Raw source cache/Library/builds/credentials excluded; normalized ready pack included.
- **Stop:** Phase 2A, Phase 3 editing, Phase 4 runoff and later mission/learning remain unstarted. No automatic expansion or parallel delegation is authorized by this completion.
