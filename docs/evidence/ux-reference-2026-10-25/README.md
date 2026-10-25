# Terra Nil placement UI/UX study — October 25, 2026

[Illustrated reference board](index.html) · [Implementation-ready placement plan](../../TERRA_NIL_INTERACTION_PLAN.md) · [17 timestamped video observations](video-observations.json)

## User request and scope correction

The user requested a deep study of Terra Nil HUD/interactions and a plan full of direct visual references. They then narrowed it explicitly to **placement UI/UX and a more game-like world**, with no need for a time element or “watch the world respond” work. The delivered plan preserves current mechanics/annual flow. It proposes no new simulation, progression, timed reveal, growth/fire/wildlife system, map or generated image.

## Research

- Refreshed origin/main at **061fe5191a5f44acfb1ec232e0924b6a5c49721f**; fresh isolated worktree `/Users/naman/Documents/GitHub/Terra-ux-study`, branch `codex/terra-nil-interaction-study`.
- Original checkout's dirty `ProjectSettings/Packages/com.unity.pipeline/EditorPipelineConfig.json` was untouched.
- Read current spec, implementation plan, design reference/art contract, phase gate/rebuild, action/annual contracts, play-first HUD contract, evidence and relevant placement/HUD/renderer code.
- The current integrated Chrome captures are **b4693ac**, linked from the existing play-first receipt. They were visually inspected, not recaptured or re-certified. No fresh current-game playtest or build is claimed.
- In Chrome, visually inspected 17 paused samples of Aliensrock's linked 13:20 video, concentrated on the opening placements and spanning 00:10–12:00. Not continuous full-video viewing or soundtrack analysis. Exact frames/observations/limits are in JSON. Native UI versus editorial enlargement/held tooltips is explicitly distinguished. A buffering-contaminated 10:00 capture was replaced after reinspection.
- Preserved all three user-supplied images separately; timestamps/version unknown. Attribution is explicit. External reference images are documentation only, not production assets.
- Official Free Lives press kit and official Steam listing corroborate authorship/premise only. No invented timings, current-version assertions or hidden-mechanic claims.

## Delivered design

- Current-code audit distinguishes existing capabilities from missing design coherence.
- Six state diagrams in the board: browse, active tool, valid preview, blocked, queued and replacement. Stable palette/inspector geometry.
- Detailed state/command contract, first-cause rejection/recovery copy, footprint-versus-site-slot distinction, local cost semantics and neutral opening.
- World composition requirements tied to existing TN/CS/AX pattern IDs and direct imagery.
- Ordered P0–P5 work slices and concrete Chrome/accessibility/input/comprehension acceptance criteria.
- Proposes park + whole-field farming + connected swale as the first representative placement slice. No runtime work starts from this document.

## Validation

- All 17 video image paths exist; captures are JPEG files named accordingly. User images remain their original PNGs.
- Board JavaScript passes `node --check`.
- Local board references and plan links checked; reference thumbnails point to existing project images.
- The finished research board has not received Chrome visual/interaction verification: the shared desktop remained occupied/queued. Static validation passed; this limitation is recorded in `verification.json`. The source video itself was inspected in Chrome earlier in the study. No game acceptance is claimed.

## Integration handoff

This is a documentation-only package. Preserve the placement-only scope correction if integrating with other work. There are **no changes under Assets, Packages, ProjectSettings, worlds, services or runtime tests**. No new game build or deployment is required for these documents.

Suggested small index additions for the current document owner:

- At the top of `IMPLEMENTATION_PLAN.md`: link `docs/TERRA_NIL_INTERACTION_PLAN.md` as the October 25 placement-clarity follow-up. Explicitly preserve current mechanics and annual flow; P0–P5 are proposed UI/world-presentation slices, not newly authorized runtime execution or scenery acceptance.
- In `docs/DESIGN_REFERENCE.md`: link the new study beside TN-03/04/05 and CS-02/03/04. The original three-frame tutorial study remains historical; this new video sample log is separately attributed and does not replace the art reference library.

Shared index files were not edited while other owners integrate current work. This additive document is ready to be linked without overwriting their revisions. The release chat requested a stable feature-branch checkpoint and will integrate the documentation with its combined release; this chat does not deploy or independently promote a combined runtime release.

Any runtime implementation, final scenery quality, novice comprehension and final-game performance remain unverified by this package. No Phase 2A gate is closed.
