# Play-first annual HUD

Peter approved the three-screen icon-led mockup on October 25, 2026: Cities: Skylines information clarity, with Minecraft's approachable learning-by-doing feel. This implements that direction within the existing rural game, uGUI/TMP and annual ledger. It does not change the scenery contract or introduce a general city simulator.

## Read the landscape first

- Keep year, available public funds and three named indicators visible. Cash, commitments and queued reservations move into the Budget button.
- Keep illustrated project cards, upfront prices and three categories. Selecting a project highlights every currently eligible field through the existing authoritative rejection rules.
- Keep Undo, Redo, View, Guide, Settings and End Year visible. More holds the last report, future, clear-plan and new-run actions; O opens it from the keyboard.
- Move the project tray left when an inspector occupies the right side; show three cards per page when the inspector and World atlas control leave insufficient room for four. Preserve operating dates and full-term reservations at compact sizes.

## Inspect a choice

Use the existing project miniature and original vector icons, with short labels beside every unfamiliar symbol. The inspector shows actual public upfront cost, operating upkeep and term, separate recurring program payment where applicable, one capacity, one resource/land trade-off, opening year, establishment period and full-term reservation. Capacities are authored game units, not guaranteed useful services or claimed meter increments. Details retains ownership, receipts, costs, all services, assumptions and scientific limits.

Blocking reasons, protected fields, accepted-project permanence, move/remove, and unavailable actions retain explicit text. A valid replacement is labelled REPLACE and requires Replace; blocked replacements show the real queued project for Move/Remove. The More menu focuses an enabled action and consumes keyboard input. Completed operating projects have no repeated map label; construction, growth and warnings retain short status labels. Icons do not replace these explanations. Existing save keys, simulation inputs, prices, receipts, construction delays and transactional approval stay unchanged.

## See the accepted year

The first year-end view is nonmodal. Up to three concise highlights use the saved accepted results and ledger; clicking a site highlight outlines and focuses that location. Unpaid commitments and interrupted/suspended projects take priority. Quiet years show a balance without inventing changes. Financial highlights open the ledger. No AI prose is used as the source of a factual change.

The full saved chapter and budget remain optional. Plan next year continues immediately without reading story pages; Year 10 hands off to the existing future. Tab/Shift+Tab selects highlights and actions, Enter activates, Escape returns to the map. Panning and focusing remain available. Browsing a report must not edit a queued plan, spend funds or advance time.

## Verification

- Core regression: `dotnet run --project tests/full-game/Terra.FullGameChecks.csproj -- --connected --stress --evidence Logs/PlayFirst/core` — 2,399 assertions passed.
- `MainGameChapterChecks.BuildVerifiedWeb` includes `PlayFirstHudChecks`: all 17 project variants in standard/larger text and compact/normal layout, visible full commitments and dates, no spend from inspection, new construction/opening/quiet-year/arrears highlights, map focus without state mutation, and immediate continuation.
- Runtime rendering, browser evidence and final build identity are recorded in `docs/evidence/play-first/README.md` when verified.

Reference patterns: TN-03/04 (quiet hierarchy), TN-05 (preview before commit), TN-06/08 (change and short reports), CS-01/02/04 (progressive disclosure, spatial constraints and informed trade-offs), AX-01 (backed readable labels). This is a UI delivery, not Phase 2A scenery acceptance or a novice usability study.
