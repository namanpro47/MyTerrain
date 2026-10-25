> **Submission checkpoint:** See [SUBMISSION_HANDOFF.md](SUBMISSION_HANDOFF.md) for the current delivered/deferred split. Historical entries below retain their original tested builds. Unverified pin/camera changes are excluded from the delivery runtime.

# Placement acceptance ledger

Implementation checkpoint: `6e188ee`. Native fixture: base `13a15a4` plus Field Notes. Combined integration inspected: `4257a7a` (read-only source review). The combined owner preserved parcel-centered ResetView, audio interception, Field Notes scroll and accepted financial history while incorporating the placement input guard. Source review is not browser verification.

| Plan criterion | Current evidence | Remaining acceptance |
|---|---|---|
| 1. Neutral start and reopenable help | InitializeAnnual clears armed tool; native HUD creates help button and modal | Chrome770d4c0 neutral arrival/dismiss and dock help reopen verified |
| 2. All enabled recipes, names, icons, prices, eligibility | Native all17recipes ×4sizes ×2text sizes; selected card visible and labels fit | Actual catalog/keyboard navigation and unavailable-state explanation |
| 3. Real footprint versus whole field/site slot | Native building/field/swale geometry matches current rules; shader imports | Chrome770d4c0 park, whole-field farming and swale previews captured |
| 4. Blocked candidate and recovery | Native candidate title/reason/recovery assertion; inspection does not mutate ledger | Protected, occupied, physical-fit, disconnected and money rejection inputs |
| 5. Press identity, drag/wheel/focus/keyboard | Frozen site/tool/mode/source/revision checks; focus-loss cancellation; merged input source inspected | Actual press-drag/up, scroll while pressed, blur, UI crossing and keyboard routes |
| 6. Queue/edit/undo preserve command behavior | Existing full-game608checks; inspector nonmutation; existing HUD788checks | Chrome770d4c0 queue, move/cancel, replacement review/cancel/confirm, remove and undo/redo verified |
| 7. Responsive and stable layout | Native1024/1280/1440/1920, standard/larger text, everyrecipe; dock unchanged on valid/blocked transitions | Standard Chrome1280×720/1440×900/1920×1080 captured. Rebuilt overlap fix,125% zoom and compact larger text remain |
| 8. Accessible state grammar | Labels, dashed/solid geometry, blockedcross, selectedbadge, visiblefocus,44px primary targets in source | Keyboard settings navigation, reduced motion, larger text and mute verified. Final candidate inspection under all settings remains |
| 9. Preview/placement latency | Existing instrumentation retained; hover target changes only | Chrome≤100ms preview and≤200ms placement-stall measurements with recorded conditions |
| 10. World presentation | Legacy composition candidate; combined source preserves newer parcel settlement framing | Matched normal/close/overview Chrome captures, footprint visibility and model/thumbnail consistency |

Native total: **2,839 passed** =2,051placement/intent/layout assertions +788existing play-first HUD assertions. This total must not be represented as2,839browser interactions. Initial native setup failure and the discovered/fixed uppercase heading clip are recorded in HANDOFF.md.

P0 reference board contains the supplied screenshots and17sampled frames from the linked video. All six schematic states were clicked, captured and visually reviewed in Chrome; see reference-board-review.json and reference-*.png. A callout collision and ambiguous uncommitted reservation wording were fixed. The actual game and the reference schematic are distinct artifacts.

P5 uncoached human comprehension remains open if participants are unavailable, as permitted by the approved plan. The proposed4-of-5/60-second target is not an achieved result. The broader Phase2A scenery gate remains independent.

## Combined-source review follow-through

At combined tip `f819fac`, the release owner has addressed both placement findings: BeginMove retains `hud.Selected(treatmentId)` so the active project remains visible, and the receipt's visible condition excludes an active placement capsule so it cannot cover the fallback guidance. These were inspected in source; their real rendered behavior remains part of the Chrome walkthrough.

## First combined Chrome pass

[Actual walkthrough](COMBINED_CHROME.md) records Chrome770d4c0 at1728×940, reservation96. This is real game evidence, distinct from the P0 schematic board. Both the dock/card overlap and transient toast/capsule collision were reproduced; fixes56e1940 and9b5e14a are handed to the combined owner for native rerun and next export. Large empty map-edge framing remains a P4 observation, not an accepted scene. Preview left the ledger unchanged; protected Enter was rejected; queue/move-cancel/undo/redo retained the existing cash/commit semantics.

The second Chrome pass and DevTools input-error boundary are documented in COMBINED_CHROME.md. Combined native suite subsequently passed4,115 assertions (receipt inspected in release worktree at19e7eae); the owner added fixture cleanup before later combined checks. No quiet performance claim yet.

## Final export and remaining handoff — October 25

The frozen `19e7eae835ed4c06a21d42887a9a53ed28cc4fc2` export was copied byte-for-byte to the placement QA origin. `final-export.json` records all 22 hashes. No claim that the older screenshots verify this export.

A final source/capture audit found a remaining P1 instruction conflict: the dock still says “Hover a marked field” while the local capsule says “Click to queue.” The authorized Field Notes/HUD owner accepted this correction in the next presentation work: suppress the dock instruction while the capsule is active and restore it when hidden. This is a handoff, not a verified fix in frozen19e7eae.

The opening capture's large blank map-edge wedge was also handed to that owner as a P4 composition finding. Preserve settlement context and camera freedom; do not restore the obsolete legacy-parcel arrival target. The park-focused capture already makes the footprint, reserved field and creek context legible. Opening/close/overview acceptance remains open.

Queued identity currently uses a named world label and dashed boundary. The plan's miniature/check pin is not implemented and must not be claimed as delivered. Both this visual detail and the P4 composition need a coordinated follow-through; the frozen release is not being modified during its verification.

Final Chrome request123 follows short dialog/numeric checks and the community build. It reserves desktop plus heavy-work exclusion for two30-second F9 samples. No timing result is inferred from native assertions or from a busy-machine walkthrough.

### Queued pin candidate

Commit `d9df021` completes the miniature/check pin in source, on top of merged origin/main19e7eae. It reuses the catalog sprite shown by the tool, inherits the existing status label's billboard/scale/lifetime, draws a check without a font-glyph dependency, and adds no raycaster or command behavior. Repeated marker refresh updates the existing miniature; missing art retains the named ground boundary. `git diff --check` passes. Unity compilation and actual Chrome rendering are pending; this is not in the frozen19e7eae export. The next presentation owner has been offered the one-file candidate for coordinated validation. Final frozen-export verification is now request125 under `placement-pin-final-20261025`.

## Release walkthrough stills inspected

The `chrome-19e7eae` public walkthrough supplied actual1728×940 opening and Year2 housing-preview captures, copied with hashes in `release-walkthrough-capture-provenance.json`. Visual inspection confirms that the dock instruction now clears the project cards, closing standard-desktop spacing confirmation for that state. The selected miniature, local capsule and inspector are visible together. These stills do not prove toast recovery,125% zoom, compact larger text, pointer guards or timing. They also reproduce the large opening map-edge wedge and redundant dock/local instructions. The review board now separates this latest capture from its nine older770d4c0 interaction states.
