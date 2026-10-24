# Terra — phase status and evidence

**Last updated:** October 24, 2026, rural-first scope revision. This ledger tracks observed implementation; [the spec](../TERRA_PROJECT_SPEC.md) tracks intended vision and [the plan](../IMPLEMENTATION_PLAN.md) defines gates.

No full phase is certified complete yet. `SETUP.md` now records successful Editor/Play-mode checks, a Web export, local Chrome rendering, and debugger attachment. This documentation task did not rerun them. Reuse that evidence in phase 0 and finish the public deployment/input gate; do not redo completed setup merely because the full phase remains open.

| Phase | Status | Evidence / missing gate |
|---|---|---|
| 0 — Browser gate | Goal prepared, not started | Build logs/files and authenticated Vercel CLI checked; fresh interactive export and public Chrome/Safari verification pending; see PHASE_0_GOAL.md |
| 1 — Rural selection/foundation/contracts | Planned | Choose one rural AOI, then styled fixture, contracts, shared commit and handoff |
| 2 — One real rural landscape | Planned | Selected AOI's aligned pack and browser visual review required |
| 3 — Editing | Planned | Full reversible edit path required |
| 4 — Consequences | Planned | Runoff/retention integration and correctness evidence required |
| 5 — Rural mission | Planned | Balanced alternate designs on the same map and novice review required |
| 6 — Ending | Planned | Comparison/learning/continue loop required |
| 7 — Polish | Planned | Visual/interaction review and measured performance required |
| 8 — Release/refinement | Planned | Full single-location build, rehearsed demo, and focused refinement backlog |

## Current handoff

- **Next action:** user starts the prepared [phase-0 goal](PHASE_0_GOAL.md); reuse setup evidence and complete the interactive browser/deployment gate.
- **Active goal:** none started by this preparation; waiting for the user's next `/goal` message.
- **Parallel packages:** none assigned; start after the phase-1 baseline is shared.
- **Latest recorded local build:** `SETUP.md` reports a successful Web export and local Chrome rendering; artifacts in `Logs/Setup/` and `Builds/Web/`. Public URL and shared commit are not recorded here.
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
