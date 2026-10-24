# Terra — phase status and evidence

**Last updated:** October 24, 2026, Phase 0 public-browser gate passed. This ledger tracks observed implementation; [the spec](../TERRA_PROJECT_SPEC.md) tracks intended vision and [the plan](../IMPLEMENTATION_PLAN.md) defines gates.

**Phase 0 is complete.** Existing Editor/Play-mode/debugger setup evidence was reused. A fresh interactive Web export and actual public Chrome/Safari checks now pass. Phases 1–8 remain planned; no rural location has been selected.

| Phase | Status | Evidence / missing gate |
|---|---|---|
| 0 — Browser gate | Gate passed | Current-source export, public anonymous access, Chrome/Safari interaction checks, captured evidence and source checkpoint recorded below |
| 1 — Rural selection/foundation/contracts | Planned | Choose one rural AOI, then styled fixture, contracts, shared commit and handoff |
| 2 — One real rural landscape | Planned | Selected AOI's aligned pack and browser visual review required |
| 3 — Editing | Planned | Full reversible edit path required |
| 4 — Consequences | Planned | Runoff/retention integration and correctness evidence required |
| 5 — Rural mission | Planned | Balanced alternate designs on the same map and novice review required |
| 6 — Ending | Planned | Comparison/learning/continue loop required |
| 7 — Polish | Planned | Visual/interaction review and measured performance required |
| 8 — Release/refinement | Planned | Full single-location build, rehearsed demo, and focused refinement backlog |

## Current handoff

- **Next action:** stop here. Phase 1 requires a separate user request; its first task is rural AOI selection, then foundation/contracts.
- **Goal outcome:** requested Phase 0 work completed; no Phase 1 work started.
- **Parallel packages:** none assigned; start after the phase-1 baseline is shared.
- **Latest verified build:** `Builds/Web/`, 12,777,098 bytes; [public fixture](https://terra-browser.vercel.app/), source `7ae591c54d1796997fcf83ce2f3b99a3b8b8e4c1`. Local checkpoint only; no Git push performed.
- **Scope decision:** one rural location is the primary product. Complete and refine all game systems, UI/UX, learning, and replay there through phases 0–8. Former expansion phases 9–11 are removed from the active roadmap.
- **Location decision:** the user asked to choose the rural AOI during early planning; record one selection in phase 1. No place has been selected yet.
- **Expansion:** additional rural/city levels and live generation are deferred until the full one-location quality gate and a separate explicit decision.
- **Working scenario:** illustrative productive land versus restoration and runoff; confirm treatment details in phase 1. No production scalability or seasonal farm simulation is required.

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
