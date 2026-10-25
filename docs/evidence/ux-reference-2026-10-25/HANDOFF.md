# Stable documentation checkpoint for combined integration

**Content commit:** `4d8e91f` — Study Terra Nil placement UX and define reference-led interaction plan.

**Branch:** `codex/terra-nil-interaction-study`, pushed to origin. No PR; no deployment or runtime promotion from this chat.

**Scope:** complete research/placement-design draft after the user's explicit correction. No new time element, “watch the world respond” sequence, progression, simulation, map or generated image. Preserve the current annual mechanics. Additive documentation only.

**Files:** `docs/TERRA_NIL_INTERACTION_PLAN.md` and `docs/evidence/ux-reference-2026-10-25/` (illustrated board, 17 timestamped video captures/log, three user references, validation receipt).

**Verification at content checkpoint:** Markdown/local HTML links resolve; JS passes `node --check`; queried IDs exist with no duplicates; all 17 video captures verified as correctly named JPEGs; `git diff --check` passes. Current-game source audit is pinned to `061fe5191a5f44acfb1ec232e0924b6a5c49721f`. Existing integrated Chrome HUD captures are `b4693ac`; no fresh game build/playtest is claimed.

**Integration:** cherry-pick the content commit plus later documentation verification/handoff commits, or merge the feature branch normally. No Unity rebuild is necessary for this package. Suggested two-line index additions are in README for the owner of the central plan/reference documents. Do not overwrite concurrent plan edits. Follow normal fast-forward main delivery; no force-push.

**Still outside this package:** actual runtime implementation, human comprehension evidence, final scenery/performance acceptance and all simulation/audio/provider work. Board Chrome review was pending the shared desktop queue at this checkpoint; consult `verification.json` on the final branch tip for its latest status. No unready runtime experiment exists on this branch.
