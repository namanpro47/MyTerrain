# Phase 2A — prepared goal: Terra Nil quality for the existing play slice

**Prepared:** October 24, 2026. **Status:** planned; this brief does not start a goal or certify implementation. **Sequence:** Phase 2 → **2A** → Phase 3. Existing phase numbers remain stable.

**Outcome:** Meadowbrook Valley's existing choose → preview → place → undo loop looks, reads, sounds, and responds like an intentionally finished game at the standard of the selected Terra Nil references. Establish that bar before adding more mechanics. Cities: Skylines supplies selected information-design patterns, not the overall aesthetic or scope.

Read [AGENTS.md](../AGENTS.md), [the product spec](../TERRA_PROJECT_SPEC.md), [DESIGN_REFERENCE.md](DESIGN_REFERENCE.md), [the implementation plan](../IMPLEMENTATION_PLAN.md), [the art contract](art-direction.md), [the evidence ledger](PHASE_STATUS.md), and [ownership boundaries](PARALLEL_WORK.md).

## Copyable goal-loop kickoff

```text
Start a goal to complete Phase 2A using docs/PHASE_2A_GOAL.md. First confirm Phase 2's required engineering/browser gates are complete in docs/PHASE_STATUS.md; do not absorb unfinished geographic ingestion into this phase. Use docs/DESIGN_REFERENCE.md throughout implementation, with Terra Nil as the primary visual and experiential reference and Cities: Skylines as a secondary information-design reference. Bring Meadowbrook Valley's existing placement slice to the reference-calibrated visual, interaction, accessibility and browser-performance bar. Work through baseline audit, art direction, HUD and tool presentation, preview/feedback, camera/audio/motion, and actual browser verification. Fix the weakest material gap each iteration. Preserve the real AOI, honest data/model labels, current state semantics, uGUI/TMP and package ownership. Do not implement later-phase gameplay to fill missing UI slots. Record actual captures, an interaction recording, measurements, source/build identity, design scores with reasons, and honest human-feedback status. Complete the Phase 2A engineering gate, and record external usability/reference validation separately if testers are unavailable. Do not claim untested parity with either reference game. Stop before Phase 3; do not begin another phase, level, generator, automation or delegation automatically.
```

No token budget, scheduled task, external message, or new agent assignment is created by this plan.

## Entry gate and bounded scope

- Phase 2 must have its real-landscape, geographic alignment, provenance, final browser and performance evidence. Its current in-progress status is not a pass. If it remains incomplete, record the specific dependency, prepare independent designs if useful, and do not certify Phase 2A.
- Capture the **actual final Phase 2 build**. The research's initial local screenshots are historical; do not count already-fixed issues twice.
- Use the existing map, sites, canonical state/evaluator and currently working placements/undo. All three tool families may be presented to the extent they already exist. A swale's existing visual or credit accounting is not integrated runoff mitigation.
- Include loading/failure/retry, controls/settings, source disclosure, camera movement and a representative repeated placement path. These are part of the visible product, not optional polish.
- Exclude new geography, simulation math, full move/remove/redo/history implementation, mission state machine, final before/after mode, learning/end screens, wildlife systems, research trees, freeform zoning, and new gameplay tools. Those stay in Phases 3–6 or outside the product.
- Do not invent numerical runoff, completion meters, goal attainment, or disabled future menus just to fill a mockup. Later-state layouts may be documented with visibly labelled design examples, but must not ship as working features or acceptance evidence.

## Proposed design direction

**Landscape:** warm directional light, distinct cultivated/grass/woodland/water masses, readable relief, grouped canopy, restrained ground variation and a deliberate outer edge. The landscape should feel rich at the main camera and convincing at the closest supported play view. Keep geographic heights, mapped positions, source cover and analytical site boundaries intact.

**Interface:** compact deep forest/ink play surfaces; warm paper explanation pages; cream text on dark surfaces; restrained amber emphasis for selection; distinct shape/text for blocked actions. Retain familiar tool names. Use original consistent treatment illustrations and typography that remains readable over scenery. This is a starting direction to test, not a pre-approved pixel layout.

**Hierarchy:** small place identity; compact production/budget/neutral runoff status; three illustrated tools at the bottom; cultivation variants attached to their family; one structured contextual preview; quiet utility/help access. Guidance and transient feedback should share a deliberate region, not stack into multiple permanent bars. Use the existing spec's top/bottom/contextual layout; no framework or architecture migration is needed.

**Feel:** immediate response, subtle settling, gentle interruptible camera easing, quiet sound with mute, and equally clear reduced-motion behavior. Avoid bouncing menus, heavy blur, expensive depth of field, noisy hover sounds and cinematic loss of player control.

## Work packages in dependency order

These are sequential slices for the primary implementation owner, not automatic subagent assignments. Package C may provide art/audio through its existing boundary if separately assigned. Any ownership transfer is recorded before edits.

| Slice | Work | Required output / local gate |
|---|---|---|
| 2A.0 — Freeze the comparison | Inspect current build, source and evidence; record implemented/unavailable actions, screenshots, performance, and top five remaining gaps against reference pattern IDs | Baseline manifest and gap table; exact source revision or dirty-tree fingerprint; no inherited “4/5” score accepted without fresh comparison |
| 2A.1 — Establish one coherent direction | Compose wide/close views; define palette, typography, icon perspective, material treatment and UI groups; prototype default + valid-preview + blocked state | In-engine/browser treatment of those three states; choose the strongest coherent version; update art contract to v2 only when implementation supports it |
| 2A.2 — Improve the landscape | Tune lighting/shadow/value structure, canopy clusters, cover/field/margin identity, edge treatment and close-up detail; preserve sourced geometry | Wide + close captures, geographic invariants unchanged, no new frame-time regression; no scene-only hero render |
| 2A.3 — Rebuild HUD presentation | Centralize practical style tokens, size/layout rules, reusable card/metric/button/preview/modal styles; create original tool art and complete control states | Functional current-data HUD at both baseline viewports; no mixed icon styles or detached variants; no new gameplay behavior hidden in a UI refactor |
| 2A.4 — Finish interaction feedback | Polish ghost, boundary, hover/selected/focus/blocked states; restructure cost/trade-off copy; make cancellation, existing undo and error recovery obvious | Pointer and keyboard choose/preview/commit/undo agree; zero UI/camera click-through; rejection changes no canonical state |
| 2A.5 — Finish the surrounding experience | Guide-style controls/sources, clear loading/retry, visible focus, larger text, reduced motion, mute, camera easing and licensed subtle audio | Complete core path with mute/reduced motion/keyboard; no clipped provenance; no inaccessible exit or focus loss |
| 2A.6 — Calibrate and close gaps | Export, inspect public browser build, compare matching reference states, record real interaction, measure budgets; fix highest-severity observed issue and recheck affected paths | Complete acceptance/evidence matrix, art contract v2, remaining limits and Phase 3 handoff; human review recorded separately |

Do not wait for a large art pack: make a small number of excellent treatment illustrations and locally relevant plant/ground assets. Preserve existing licensed assets where they meet the standard. Replace generic kit appearance where it is visibly the limiting factor. Do not introduce speculative UI frameworks, generalized theming engines or new render pipelines.

## Detailed acceptance criteria

### A. Reference-calibrated visual quality

Capture actual game canvas output at **1280×720 and 1440×900**, plus a **1920×1080** wide-layout check. Compare references at a comparable displayed size and gameplay state; keep aspect ratios and note source-version differences. Include actual play UI in the judgment.

Use the following anchors for each dimension: **1** broken/prototype; **2** functional but visibly unfinished; **3** coherent with a substantial reference gap; **4** convincing authored game with limited remaining differences; **5** no material quality gap for this bounded slice against the chosen reference frames. Half scores require an explanation. Aesthetic scores are internal judgments, not measurements of user preference or commercial-game equivalence.

| Dimension | What must be demonstrated |
|---|---|
| World composition and depth | Inviting wide view and useful close view; visible landform, cover groups and site context; main subject clear without hiding UI |
| World detail and treatment identity | Cultivation, margin, restoration and any existing swale appearance recognizable at play zoom; clustered detail; no floating/seamed/z-fighting geometry |
| HUD hierarchy and typography | Main numbers/actions easy to find; consistent tokens, readable labels, spacing and alignment; no prototype-looking loose buttons or panels |
| Tool and state legibility | Coherent original icons; distinct hover, focus, active, valid, blocked and committed states; state never conveyed by color alone |
| Interaction feel and feedback | Ghost/commit/undo causally connected; camera controllable and interruptible; no input fighting, false numbers or stale feedback |
| Coherence across surfaces | Loading, failure, help, sources and settings meet the same standard as the hero frame |

**Pass threshold:** every dimension at least **4/5**, mean at least **4.5/5**, plus every hard gate below. This deliberately exceeds the old generic 4/5 foundation target. Every score must link to actual evidence and describe the remaining gap. Two successive reviews of different states (normal play and adverse/recovery states) must pass; do not repeatedly rescore the same hero screenshot. No average can conceal a broken input path, unreadable control, misleading metric or conspicuous visual defect.

### B. Clear layout and accessibility

- At 1280×720 normal idle play, target at least **75% of the canvas unoccluded by persistent HUD panel rectangles**; during preview target at least **65%**. Measure rectangle union, not summed overlapping areas. Exclude deliberate full information modals. Record an annotated capture. These are Terra composition constraints, not measurements of Terra Nil; never achieve them by making controls illegible.
- The initial camera keeps important editable land clear of the dock. Focus-site framing places that site inside the unobstructed area. Inspect an edge site as well as a central site, and verify camera bounds.
- Primary interactive targets at least **44×44 effective CSS px**; body text approximately **16 px or larger**, essential secondary labels at least **14 px** at baseline viewports. Confirm effective canvas sizing, not just TMP reference-unit font sizes.
- Measure **4.5:1 text contrast** and **3:1 meaningful state/control contrast** for normal, selected, hover and focus styling against the actual backing surface. Avoid translucent backing if underlying scenery defeats contrast. See AX-01 for sources and scope.
- Default and **125% browser zoom**, plus a larger-text setting, must retain reachable controls and readable previews. Test 1920×1080 for unnecessary spreading of UI. Narrower/mobile layouts are not a new platform requirement; document the supported desktop minimum.
- Pointer and keyboard paths support choosing all existing treatments, site cycling, preview, commit, cancel, existing undo, settings, source pages and return. Visible focus distinguishes UI focus from world selection. Browser shortcuts and focused UI must not leak into world commands.
- Modal navigation has an obvious close/back action, Escape exits the top layer, background input is blocked, and focus returns predictably. No trapped tooltip, invisible focus, click-through placement, or camera drag that accidentally commits.
- Mute and reduced motion work throughout the path; information remains available without sound/animation. Test tab focus loss/recovery. Record physical-trackpad coverage honestly rather than equating injected wheel events with a person testing a trackpad.

### C. Honest and reliable feedback

- Every number comes from the existing evaluator/state. The illustrated preview agrees with the committed treatment; cost and production deltas match; current/baseline/proposed values are labelled unambiguously where shown.
- Keep runoff explicitly unavailable until its real integration. Do not style a proposed swale as verified downstream protection. Production is illustrative and credits are abstract.
- Valid preview, protection rejection, eligibility rejection, insufficient-credit rejection, already-placed state, cancellation, commit and undo each have concise meaningful copy and distinct visual treatment. If any state is unreachable in the frozen slice, explain why and use a clearly labelled technical fixture only for its component check; the main path must still use Meadowbrook Valley.
- Repeated placements/undo, rapid tool changes, and interruption during reveal leave both world and UI at the canonical state. Rejection leaves accounting untouched. Preserve existing semantic behavior; new history mechanics wait for Phase 3.
- Source pages keep source names/dates, coarse resolution, decoration, hypothetical sites and model gaps available without pretending the fictional game name is a geographic source name.

### D. Browser delivery and performance

- Current-source Web export succeeds; local smoke check precedes publication. Preserve the previous working build. Associate exact source/pack identity with exported hashes and the final public URL.
- Verify anonymous public loading and core interaction in **Chrome** on the demo Mac. Chrome is sufficient per the project browser preference; Safari or Firefox checks require an explicit user request. Do not request Safari unlocks. Record browser versions, hardware, viewport, device-pixel ratio, cache state and build identity. No blocking console/network error or missing font/shader/icon/audio reference.
- Run the same **30-second camera/preview/placement/undo path** before and after, with **three foreground runs** on the final build. Target p95 frame time at or below **16.7 ms**; a measured stable-30 fallback needs p95 at or below **33.3 ms**, documented rationale and no visible repeated hitch. Report p99, maximum, and counts above 33.3/200 ms rather than only average FPS.
- Target input-to-visible-preview p95 **≤100 ms**, and no observed placement stall **>200 ms**. Separate handler CPU time, DOM-to-rAF instrumentation and actual visible-render verification; none alone proves full display latency. State exactly what was measured. Use enough repeated events (at least 20 previews and 10 commit/undo cycles) to expose unstable behavior.
- Measure three fresh-cache loads on the documented connection. Target first usable interaction **≤15 s** and compressed initial download **<50 MB**. Disclose every miss; optimize the specific source rather than hiding it with a different cache condition.
- The hard gate requires meeting the stated preview/stall/load/download limits and either the 60 fps target or the documented stable-30 fallback. A disclosed miss is still an unmet gate, not permission to mark it passed. If a constraint proves unattainable, present the measured trade-off for a separate scope decision instead of silently lowering the threshold.
- Reduced graphics cost must not blur the HUD or destroy state readability. Use simple materials, shared meshes, restrained shadows/effects and small assets before adding complex systems.

### E. Human evaluation, without an indefinite wait

Recruitment is not automatically authorized messaging. If people are available through the user, aim for **three uncoached first-time players**. Give each the same short task: choose a treatment, inspect a compatible site, explain the shown cost/production trade-off, place it, undo it, recover from one blocked action, then find source/assumption information. Observe first valid action, hesitation, mistakes and recovery.

Proposed usability targets: at least **2/3** make a valid placement within **60 seconds** of gaining control and all can undo without verbal coaching; at least **2/3** explain the preview and identify the hypothetical nature of the choices. These are product targets, not research findings. Ask which matched reference/implementation screens feel unfinished and why; do not ask leading “does this look as good?” questions.

If no testers are available, finish the engineering and internal visual gate, record **“engineering gate passed; external usability/reference validation pending”**, and hand off. This preserves the existing plan's human-feedback exception. Never claim a full human-validated quality pass, and do not hold a goal open just to wait for recruitment. Any later phase still requires separate user authorization. New adverse human findings reopen the affected design gap.

## Required evidence bundle

Create `docs/evidence/phase2a/` when execution starts, not empty success files during planning.

| Artifact | Required contents |
|---|---|
| `baseline.md` | Final Phase 2 build identity, comparison captures, implemented/unavailable action inventory, prioritized gaps and reference pattern IDs |
| `design-review.md` | Reference links/version, annotated matched comparisons, score reasons, rectangle-union UI occupancy, weakest issue before/after, deliberate deviations |
| `captures/` | Both baseline sizes: idle hero, close site, each available tool/variant, valid/blocked/already-placed, commit, undo, help, sources, settings, loading/error/recovery; additional zoom/wide/focus/motion examples |
| `interaction.mp4` or `.webm` | Actual 45–90 second browser sequence: choose, hover, blocked recovery, commit, camera, undo, guide, return; capture method stated; no edited-in UI or mock gameplay |
| `checks.json` or `.md` | Browser/input/state/contrast/scale checks, viewport/DPR, console findings, tested path and failures fixed; unavailable checks explicitly marked |
| `performance.json` or `.md` | Before/after run data and timing definitions, foreground/caching conditions, load sizes, frame statistics, latency measures and limitations |
| `build.md` | Source revision or exact working-tree fingerprint, pack hash, Unity version, build report, deploy identity, URL and exported-file verification |
| `playtest.md` | Actual participants/tasks/observations and retests, or clear pending status; no identifying personal data needed |

Update `art-direction.md` to the accepted v2 tokens/component/camera/motion rules and asset-license references. Update `PHASE_STATUS.md` with separate engineering, visual and human statuses. Link the accepted wide/preview/blocked captures from the art contract so future work has an immediate benchmark. Preserve `DESIGN_REFERENCE.md` as the project-wide reference and add new evidence without turning observations into universal rules.

## Iteration and stop rules

1. Read the current ledger and reference; choose the highest-impact remaining discrepancy, not the easiest cosmetic task.
2. Implement the smallest coherent slice that addresses it. Prefer one polished tool/preview path to spreading unfinished decoration everywhere.
3. Verify correctness where changed; capture the actual browser states and compare them to the selected reference patterns.
4. Record what improved and the weakest remaining issue. Rebuild/retest affected paths after material changes; reuse evidence only when the relevant build and behavior are unchanged.
5. If repeated cosmetic tweaks do not improve the comparison, revisit composition, information hierarchy or asset direction. Do not relax the bar or add features to avoid the difficult visual work.
6. Once all engineering/hard gates and internal visual thresholds pass, record human status and prepare the bounded Phase 3 handoff. If a non-human prerequisite or required browser check is unavailable, record the exact unmet gate; do not mark completion. Follow the goal tool's lifecycle rules if a goal is active.

**Phase 3 handoff:** accepted art/UI v2, reusable current components, benchmark captures/video, explicit unimplemented features, focus/input rules, known limits and any pending human feedback. Every later player-facing change must cite the relevant design-reference patterns and retain this quality. Phase 7 verifies the completed game's consistency; it cannot excuse a weak Phase 2A foundation.
