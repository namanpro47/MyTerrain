# Visual project decision overlay

Base: refreshed origin/main 615459ac7bcc93e9e2b3f7b76b388e1f01c1fe30.

Diagnosis: the inspector hides lifecycle and score trade-offs behind prose and optional details; its accepted-project establishment percentage is not construction progress. Current top meters show accepted state, so selecting a project does not explain its marginal contribution.

Acceptance: before queueing, every project shows labelled cost/upkeep/time bars with correctly formatted right-side values, plus signed Economic/Social/Environmental comparison bars. A delayed accepted project shows completed construction steps and opening year separately from establishment. Standard/larger text at 1440x900, 1280x720 and 1024x576 must keep actions and exact values visible. Inspection must never mutate plans or saves. A saturated service must not advertise an uncapped gain.

Cheapest disproof: inspect the existing inspector and forecast paths. Existing purpose/one-benefit text and a hidden timeline do not show ESE. Reuse AnnualEconomy.Evaluate for demand-capped comparisons, rather than source /5 ratings or a new scoring formula. New main includes player-rules-v5 signed industrial pressure and cancellations; respect both.

Implementation:

1. A pure Core comparison evaluates two plans in the selected project's full-effect year, capped at Year 10. It reuses AnnualTown.EvaluateOutcomes (including AnnualEconomy.Evaluate) on a detached evaluator, with actual project ages, retained current condition, funded operation and normal conditions. Compare new candidate against current plan; same queued candidate against plan without itself; replacement against existing queued project. Late projects still show construction industrial pressure, with opening after this run clearly labelled. Accepted projects show actual lifecycle progress and delivered capacities instead of a counterfactual ESE attribution. Numeric-v4 comparisons return unavailable. No year replay, provider call or save write. Cancellations retain disturbed land exactly as existing rules.
2. Existing uGUI inspector becomes compact visual rows: upfront, annual upkeep, build duration/progress, establishment, and three diverging ESE bars. Icons and short labels remain. All bar values are right-aligned. Costs use fixed catalog maxima, time uses a common annual scale, signed ESE uses zero-centred fixed scales and exact signed point values. Full commitments, opening date, footprint and assumptions remain accessible. No hidden colour-only meaning.
3. UI caches derived comparisons by run/revision/site/candidate, never reevaluates in Update. The selected project heading, purpose and one/two numeric capacities stay visible. Optional details retain full resource and finance facts. Planned, blocked, replacement, accepted and completed states keep truthful labels and controls. Layout hooks must not overwrite the visual rows.

Execution contract: all work runs synchronously in the existing native HUD on selection/state changes. Pure derived results can be recomputed; no new commitments or persistent data. Repeated inspection has identical results and no effects. Current plan/revision, selected site, mode, year and rules can change; cache keys include them. No changes to prices, construction delays, crop ramp or ledger are included in this UI slice.

Validation: pure-core tests for all 17 types, capped needs, negative industrial impacts, replacement/queued identity, cancellation, late completion and input immutability; compile the native UI; actual rendered overlay inspection at target sizes and accepted construction states with isolated build outputs. Reserve editor/heavy/desktop and integration resources. TN-03/04/05, CS-01/02/04, AX-01. No public deploy or scenery acceptance claim.


## Adversarial review and corrections

Three independent plan reviewers checked premise, UI path and numerical semantics. Accepted their corrections: remove accepted-project baseline counterfactuals; show bars before a field is selected; reserve a 390px compact inspector; route Placement layout through the new authority; distinguish queued from replacement comparisons; use completed history for build progress; show town needs; use actual ages at one explicit reference year and cap to Year 10 instead of making all planned projects instantly mature. Parent confirmed these paths in source before implementation.


## Player terminology correction

Peter noted that Established, Output, Jobs and Cover do not explain what the player achieves. Replace the main-view jargon with Working at (usable operational percentage), Local jobs and Green land. Hide raw productivity units from the main overlay; show concrete functions and a short function-to-score link, such as Housing → Social or Business activity → Economic. Town goals say Homes filled / Local jobs / Green land. Accepted project facts retain only understandable counts (homes and jobs), vegetated share, rain-storage volume and the explicit industrial score penalty; raw service and productivity indices stay in optional details. Detailed meanings and assumptions use the existing scrollable Field Notes reader.
