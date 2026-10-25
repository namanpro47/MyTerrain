# Verified annual branch merge — October 24, 2026

The original integration was delivered through `2387393`. The annual gameplay owner then handed off `d9d6523` on `codex/annual-gameplay-integration`, with its Editor closed and Chrome verification complete. This merge combines that branch with the already-published chapter/narration module and the retained study19b visual checkpoint. It continues the user’s request to merge delivered branches and resolve conflicts.

## Conflict decisions

- The live controller/HUD now use `AnnualTownSession` as the sole financial authority. Current-year queue editing replaces the legacy credit/refund path; accepted years stay immutable. Retained display scaling, recoverable runoff-loading errors, the catalog and annual runoff adapter. Removed the obsolete `ConfigureRunoff` initialization because the annual HUD owns that presentation.
- Retained the chapter/session/service APIs and the primary branch’s footprint-validation fixes. Combined annual instructions, the current spec/plan, and historical evidence without presenting the complete catalog or Phase 2A gate as finished.
- Integration tests exposed a real schema seam: the new service-coupling field changed v1 definition hashes. Hashing preserves original v1 behavior only when the new field has its original `true` meaning; changed behavior still invalidates the hash. Absent/empty redo stacks share the old chapter cache identity; a populated redo stack is fingerprinted. Version 2 recipes retain their complete hash.
- The exact old fingerprint was independently reproduced from `2387393` contracts/hash code and is now a regression assertion. The merged three-year accepted run is structurally identical to the worker’s fixture, including definition hash `16772e745befb2f946e5bd7de36de780ecc086ecea3e77debeafc4db76a3494d`.

## Checks

Unity 6000.6.6f1 / CLI beta.14: recompile reported up-to-date, zero errors/warnings. [Unity checks](unity-checks.json) pass 84 real-map annual assertions, 16 chapter/session assertions and 94 annual model assertions. The real-map check writes the [three-year run](three-year-run.json) and [recipes](recipes.json), tests water conservation, lifecycle, retries, serialization, accepted-year undo guards, and completion at Year 10. Original handoff fixtures were restored after collecting these new receipts.

[Standalone results](standalone.json): 94 annual, 15 gameplay, 38 chapter/service, and 1,372 consequences assertions pass with `DOTNET_ROLL_FORWARD=Major` on installed .NET 10. Three new chapter cases cover the exact legacy fingerprint, changed service rules and nonempty redo. No real provider request was made. Earlier full geography/editing/catalog/B/D/C3 results remain in [initial integration evidence](../branch-integration/README.md); they are not re-labelled as fresh tests here.

## Limits and references

Seven starter recipes are playable; all twelve source rows are retained. Remaining catalog effects, complete ending/comparison, provider connection to the main HUD and performance/accessibility certification are outside this merge. The chapter playtest/service remains additive. No website deployment or new visual-study merge is included. Concurrent local Pipeline settings and the visual handoff file remain untouched.

Patterns TN-03/04/05/06/08, CS-01/02/03 and AX-01 guide the retained annual presentation. Compare with the worker’s [three-year Chrome evidence](../annual-gameplay/README.md). Study19b remains an unaccepted visual baseline; this merge does not establish Terra Nil parity or Phase 2A acceptance.

## Supplemental published chapter update

The pre-push fetch found `724ad02` on main: a supported Gemini model default, clearer unavailable-model errors, prompt refinement and the owner’s live-provider evidence. Merged that published update as well, preserving both status entries. The combined standalone chapter suite now passes 38 checks. This merge did not repeat a provider call or read the credential. These service/docs/test changes do not alter the Unity build sources.

## Combined browser export

[Build result](build-result.json): success in 156.36 seconds, zero errors and five warnings. [Local asset verification](local-assets.json): 13 files, 36,717,606 bytes; every file returned HTTP 200 and matched the exported bytes. BuildReport’s smaller total-size value does not describe the full export. [Source hashes](source-hashes.json) remained unchanged through build and browser checks; supplemental service changes are outside the player sources.

Chrome at 1728 × 940: queued Traditional farmland at West field, leaving cash at $10M and reserving $120K. Undo removed the reservation; redo restored it. The review showed $102K due, $1M appropriation, $10.898M closing cash and $18K future obligations. End Year accepted exactly one operating project, cleared the queue and produced the [Year 1 report](year1-report.png): Economic 54.0, Social 38.9, Environmental 27.4. Plan Year 2 then Undo preserved the accepted project and cash (revision stayed 5).

Reload restored Year 2 Planning at revision 5, cash $10,898,000, zero queued actions and one accepted asset. [Browser logs](browser-logs.json) contain no error-level events. Existing optional shader and persistence warnings remain; no performance or broader viewport acceptance is inferred from this smoke test.
