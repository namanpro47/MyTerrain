# Take it local — browser companion

Implemented October 25, 2026 from the user’s [Point Reyes Figma prototype](https://www.figma.com/proto/Ee9mJ5GCrNBOiYIlUgFCYX/Take-it-local-%E2%80%94-Act-prototype--Point-Reyes-?node-id=4-2&starting-point-node-id=4%3A2).

## Delivery and release handoff

Base: freshly fetched `origin/main` at `6415767ce6a5db21a6daae1cbbbb9cbaaa3102b4`; isolated worktree `take-action-local/Terra`. Original checkout’s uncommitted changes were preserved.

All source changes are WebGL template assets. No Unity scene, simulation, ledger, save, or C# changes. `index.html` adds `take-local-link.js`; `take-local/` contains the companion. Unity’s existing custom template copies these into future exports. For an already frozen export, copy the new script and directory and apply the one script-tag addition to its own index, preserving that export’s concrete `TERRA_BUILD` hashes. No Unity rebuild is necessary for this package. Do not copy the templated index directly over a built index.

A separate native `LocalFollowup` module landed on remote main while this work proceeded (8a275b8/857530d). Preserve that work. This browser companion is a separate accessible resource surface and uses `myterrain.take-local.v1`; it does not read or overwrite the native plan or annual saves. Release owner holds integration reservation 172 and will reconcile the package into the consolidated release; this task does not compete for push or deployment.

## Implemented experience

A small main-screen link opens a new tab, retaining the game tab. All landscapes use the explicitly labelled Point Reyes pilot. Warm paper, large headings, four pastel action cards and six progress steps adapt the supplied design.

- **Where:** fixed Point Reyes resource area; no geolocation request or pretend global search.
- **Learn:** Tamalko Homes (54 homes), Trails Forever, former ranchland; linked primary project sources.
- **Habit:** six prototype choices: Route 68, trail feedback, local shops, volunteering, food waste, library.
- **Speak up:** three reasons, two tones, explicit draft regeneration, editable text retained across option changes/reload; copy and official County comment instructions. Nothing is submitted by the game.
- **Organize:** audience selection, editable invitation, downloadable printable HTML kit with five briefing cards, a group game activity and six discussion questions. No invented join codes or multiplayer rooms.
- **Plan:** custom trigger, optional selected date/time, downloadable calendar event, local save and text export.
- **Share:** PNG download/native share with clipboard fallback. Resource area off by default; draft, name and reminder time never included. No fabricated game scores.
- **Check-in:** optional two-week timestamp, due-link label on next game focus, local reflection and edit/clear. No email or background notification service is implied.

Primary content was checked October 25: [County housing update](https://www.marincounty.gov/news-releases/27-million-tax-credits-clears-path-affordable-housing-former-coast-guard-site), [Trails Forever](https://ptreyes.org/trails/), [TNC stewardship](https://www.nature.org/en-us/about-us/where-we-work/united-states/california/stories-in-california/point-reyes-path-forward/), [County submission instructions](https://www.marincounty.gov/departments/board/board-supervisors-meetings/submit-comment-board-supervisors-meeting), [meetings](https://www.marincounty.gov/departments/board/board-supervisors-meetings), [Route 68](https://marintransit.gov/routes/68), [library](https://marinlibrary.org/locations/mp/), [NPS participation](https://www.nps.gov/pore/getinvolved/index.htm). Ranchland acreage uses the current TNC approximately 17,000 acres rather than the mockup’s older 16,000. Avoid hardcoded survey-open promises or live decisions; readers check source pages.

## Verification

- `node --check Assets/WebGLTemplates/Terra/take-local/app.js` passes.
- `node tests/take-local/check.cjs` passes: malformed storage recovery, HTML escaping, preserved drafts, all branches, calendar serialization, share privacy, due check-in and unavailable storage.
- Actual Chrome at 1728×940: running globe main screen and link, new guide tab, all four branches, comment edits preserved across option changes, custom plan saved across reload, selected library habit, calendar and PNG and group-kit downloads. Downloaded files inspected at 474 bytes, 101105 bytes and 4064 bytes respectively. No comments sent and no public posts made.
- Game smoke used a private copy of the existing annual-feedback compiled Web export with the template patch (`Builds/TakeLocalPreview`, port 8875), not a newly compiled Unity player. Companion standalone preview is port 8874.
- Screenshots: `tests/take-local/evidence/main-screen.png`, `levels.png`.
- Patterns: TN-03 restrained main-screen entry; TN-06 contextual optional learning; CS-04 deeper source detail; AX-01 semantic HTML, focus outlines, labels, responsive CSS and reduced-motion support. Screenshot comparison follows the supplied Figma design. Existing scenery remains unchanged; this is not Phase 2A acceptance.
- Limitations: narrow viewport CSS is implemented but mobile device and screen-reader walkthroughs were not performed. Native game scores/plan synchronization and email reminders are intentionally absent. No production publish performed by this task.
