# Take it local — implementation and verification

## Main delivery — 2026-10-25

Peter tested the local Unity implementation and approved delivery to main after the missing-icon fix. Status, toggle, back and external-link icons now use native `HudGlyph` geometry rather than unsupported TMP characters. The local Unity recompile after the icon fix reported 0 errors and 0 warnings. Visual acceptance belongs to Peter's local QA; no live WebGL acceptance or public deployment is claimed.

The approved feature and icon commits were integrated onto refreshed main `b9c668613a78735414e3aadd927716b62859e29c`. Fresh integration checks passed: 13 plan assertions, 7 browser-bridge assertions, 1,019 full-game assertions, and runtime WebGL source compilation with 0 errors / 12 existing warnings. The incoming start nudge was preserved alongside the expanded follow-up menu. The compile harness now references the Unity Newtonsoft assembly required by the incoming save codec. The earlier candidate-stage limitations below are retained as historical evidence.

## Initial candidate record

Base: refreshed `origin/main` at `83a03d972e0e0ca64faeb3d15b86a3a2234e53a5`.
Branch: `codex/local-followup`. No public deployment.

## Reference and implementation

Peter requested a post-game follow-up encouraging players to get help, also accessible from the home/menu screen. Figma connector access hit its plan quota. Peter explicitly chose browser screenshots as the alternative; Chrome opened the existing shared file successfully. The attached PNGs are **Figma references**, not implementation captures or runtime assets.

Reference: https://www.figma.com/design/Ee9mJ5GCrNBOiYIlUgFCYX/?node-id=0-1
Inspected: Where, Level, Learn, Habit, Speak up (draft/send), Organize, Plan, Share, Check-in, and Unity handoff rules. Local captures preserve the opening, four-level screen, send screen, check-in and handoff rules.

Pattern IDs: TN-06 (contextual help), TN-08 (optional ending), AX-01 (clear controls). The user's specific Figma design governs this follow-up's light sage surfaces and blue/teal/orange/purple path colors. The existing TMP font and native glyph library are reused; this is a design adaptation, not a pixel-identical asset/font port.

Implemented in native uGUI/TMP:
- Persistent home-screen button and home settings-menu entry, available before playing.
- `Take it local` on the ten-year results; Escape still returns to the landscape. Comparison/history remain available.
- One reusable optional flow: Where → Level → Action → Plan → Share → Check-in.
- Four paths: learn from official resources; choose a practical habit; prepare/copy a question; copy a group invitation.
- Actual final game indicators in the result context; no invented prototype scores and no claimed real-world impact.
- General guidance without a city; user-triggered official-resource search for other places; Marin-specific official links for Point Reyes / 94956 / Marin.
- One local plan, separate PlayerPrefs key `myterrain.local-action.v1`; no town-save mutation. Resume, adjust and completion state.
- Optional in-game check-in after fourteen days, plus user-downloaded calendar file. No email service, background scheduler or auto-submission.
- Optional share text with scores/action/city controls; city off by default. Copy only; PNG export/native share sheet and group join codes are not implemented.
- City and comment draft remain session-only; no geocoder, live project feed, unverified deadline, survey availability or promise of current project updates.
- Input blocked from reaching atlas/map while open and on the closing frame; native mouse input, WebGL pointer routing, keyboard and gamepad focus paths.

## Sources

Verified official starting points on 2026-10-25:
- https://www.marincounty.gov/departments/cda/planning — planning help, policies and participation resources.
- https://www.nps.gov/subjects/parkplanning/get-involved.htm — official guidance on participating in park planning.

The Figma prototype's specific housing/survey dates and recipient deadlines were not copied into the runtime. The game links to current source pages for users to check.

## Checks and limits

- 13 local-plan assertions pass: validation, opted-in/out reminder dates, completion, calendar escaping and UTF-8 line folding.
- 7 JS browser-bridge assertions pass: download lifecycle and selectable fallback after clipboard denial. This is mocked bridge verification, not Chrome runtime proof.
- Full runtime source compiles against installed Unity 6000.6.6f1 WebGL engine/package assemblies: 0 errors; 11 existing warnings. See `source-checks.txt`.
- Existing full-game model suite passes **1,019 assertions**. Its generated out-of-scope evidence files were restored; gameplay logic was not changed.
- The older `tests/annual/Terra.AnnualChecks.csproj` could not compile because it omits existing `TownSimulationState` dependencies; this is separate from the passing current full-game suite.
- `git diff --check` passes.

**Native rendered/UI and live WebGL interaction verification remain pending.** The shared Unity-heavy slot was occupied, with a second task ahead in the queue. No source compile, Figma screenshot or test stub is presented as rendered acceptance. This candidate should not be deployed before that verification.

Reproduce source checks with `tests/local-followup/Check.ps1` (override UnityRoot / PackageAssemblies on other machines), then run the bridge test from the repository root with Node.
`PreparePreview.ps1` prepares an isolated minimal Unity project under ignored `.preview/`, using the exact follow-up source, existing TMP assets and a no-op sound/atlas stub. `RenderProbe.cs.txt` is the prepared rendering harness; run it only with the shared editor + unity-heavy reservation. It is not yet a passing result. Full home/results integration and browser input still need checks in the actual game.
