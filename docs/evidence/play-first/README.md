# Play-first HUD — October 25, 2026

Peter approved the icon-led map, project inspector and year-end mockups. This bounded delivery implements those interactions in the existing Unity uGUI/TMP game. [Behavior contract](../../PLAY_FIRST_HUD.md).

## Candidate and scope

Runtime candidate `607e34e` builds on main `b11a2fc`. [Source identity and input hashes](build-source.json) distinguish committed source from Unity-generated import/build settings. The export is local at `Builds/Web`, served on `http://127.0.0.1:49832/`; this receipt does not establish a public deployment.

- Quiet year/budget header, three named meters, compact icon-labelled commands, and illustrated priced project cards.
- Inspector with actual upfront cost, upkeep and term, separate program payments when applicable, capacity/trade-off, opening date and full-term reservation. Detailed assumptions remain available.
- Responsive three-card catalog beside the inspector at compact sizes, clear of the existing World atlas control.
- Nonmodal accepted-year highlights, map focus, optional story/ledger and immediate continuation. Warnings and blocked-action reasons retain explicit text.
- Valid replacements clearly require Replace. More consumes menu input and initially focuses an enabled action. Operating projects no longer repeat a floating status label.

No simulation parameters, annual ledger rules, save keys, scenery assets or provider configuration changed. Eligible-field outlines still use authoritative placement rejection. Reference patterns: TN-03/04/05/06/08, CS-01/02/04 and AX-01.

## Automated checks

- [Core checks](core-checks.json): **2,399 assertions**, including [1,791 stress checks](stress-checks.json), all 17 variants, deterministic history and interrupted-future coverage.
- [Editor checks](editor-checks.json): **809 main HUD checks**, including **787 new play-first checks**, plus **16 existing chapter checks**. The final candidate passed before export.
- New checks exercise all 17 project variants at compact/normal sizes and standard/larger text, full commitments and dates, label height, menu focus, valid/blocked replacements, construction/opening/quiet-year/arrears highlights, read-only focus, immediate continuation and Year 10 handoff.

## Browser verification

The optimized WebGL export succeeded. [Frozen export manifest](export-manifest.json): **32 files / 39,977,460 bytes**, every served file SHA-256 matched the local output. Chrome **156.0.8078.48** (headless desktop Chrome, actual WebGL rendering) passed [32 browser assertions](browser-checks.json) at 1440×900, 1280×720 and 1024×576.

- No visible-label height overflow or native-control overlap with World atlas in the captured planning/inspector/year-highlight states. Larger text fits, including the three meters. Compact inspector pagination shows three cards.
- More → Enter opens the enabled future confirmation without queuing or spending. Valid replacement inspection is clearly prospective and preserves the existing queued park.
- Two accepted offline years show construction then the actual Operating transition. Highlights focus the accepted site; opening story/ledger does not change state. Direct continuation, historical edit guards and reload pass.
- Final reloaded state: Year 3 Planning, revision 5, cash **$11,948,000**, zero queued proposals, one accepted park. No uncaught browser exception; the only two console errors were the two deliberately aborted local provider requests. Failure remained pending with the same year, revision, cash and queue until the visible offline action was chosen.

Final rendered captures and matching native UI audits:

- [Planning, 1440×900](planning-1440.png) · [audit](planning-1440.json)
- [Inspector, 1440×900](inspector-1440.png) · [audit](inspector-1440.json)
- [Larger inspector, 1280×720](inspector-1280-larger.png) · [audit](inspector-1280-larger.json)
- [Larger inspector, 1024×576](inspector-1024-larger.png) · [audit](inspector-1024-larger.json)
- [Construction highlights, 1024×576](year1-1024-larger.png) · [audit](year1-1024-larger.json)
- [Opening highlights, 1440×900](year2-1440-larger.png) · [audit](year2-1440-larger.json)

The captures were visually inspected: the map remains the primary surface, named icons avoid an unfamiliar-symbol puzzle, full costs/dates remain visible, and the green continuation action is prominent. This is an implementation inspection, not Peter's final visual acceptance.

The first actual Chrome trial at source `e8a9d12` found three regressions: disabled initial More focus with Enter fallthrough, misleading QUEUED replacement identity, and larger meter text exceeding its height by 5.26 px. [Trial findings](trial-browser-findings.json) and `*-trial.png` captures preserve that intermediate evidence. These are not final-build screenshots. Fixes are in the candidate named above.

## Reproduce the UI route

1. Open the frozen local WebGL export in Chrome with a fresh origin/save. Enter Wisconsin and dismiss the introductory guide. Inspect 1440×900 planning.
2. Select Homes & Community → Public park and a valid field. Check price, upkeep, opening date and reservation. Open More with O, then Enter; the menu must open an enabled action without queuing a project.
3. Queue the park. Select housing on the same field; its prospective replacement must read REPLACE and require Replace. Inspection alone must leave the park and budget unchanged.
4. At 1280×720 and 1024×576, use larger text, inspect labels and confirm that the compact catalog clears both the inspector and World atlas.
5. End Year 1. For the bounded offline test, block the local provider endpoint and explicitly choose Use offline chapter. Inspect highlights, focus the changed field, open/close Budget and Year story, then continue directly to Year 2. Reopening the historical overview must not allow editing.

## Limits

This verifies a bounded UI change, not a novice usability study, performance benchmark, Phase 2A scenery acceptance or the final public ten-year walkthrough. The browser provider-failure route is intentionally offline; no live provider integration claim is made. Unity import/build settings were restored after the export so they do not enter the source change. Incoming main `7c322e9` changes documentation/evidence only, including a future simulation/fallback proposal; this UI delivery does not claim that separate proposal is implemented.
