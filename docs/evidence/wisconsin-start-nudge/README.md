# Wisconsin opening invitation — October 25, 2026

Requested scope: a small default bubble pointing to Wisconsin, with at most two sentences explaining the game/scenario. The user explicitly removed the proposed “Start in Meadowbrook” line.

Copy: **Shape a Wisconsin valley over ten years, balancing people, nature and your budget.**

The native uGUI bubble is anchored above the existing Wisconsin miniature, with a small cream pointer and soft shadow. It reuses the atlas typography/colors. Clicking the bubble takes the existing immediate Wisconsin entry path; there is no extra confirmation or Enter button. A small dismiss control removes it for the rest of the player session. Starting any location also dismisses it, including if travel is cancelled or the player returns to the globe. Options/notes or choosing another destination hide it without blocking normal navigation. A page reload can show the invitation again; no save keys or annual state are changed.

Reference patterns: TN-03 (one compact UI group), TN-06 (short context), AX-01 (backed readable text and optional dismissal). The preceding minimal-selector Chrome captures remain the visual comparison; the globe shader is unchanged in this narrowly requested copy/UI change.

Fresh base: origin/main06f51b2e4ee5e1ca55d0b8d8b90a5e6bf321aa82. Candidate237e1cf. Native atlas package checks33pass; actual rendered/native/Chrome verification pending. No deployment claim yet.

## Native verification

Fresh main initially failed compilation at AnnualChapterSession.cs150 because it referenced internal TownCopies. The already-published narrow correction2eedec9 was applied as2cd9e45; the owned Unity6000.6.6f1 Editor then compiled successfully. A private APFS copy of the previous task's cache was used, not a shared writable Library. The initial Editor exited1 on the main compile error; the corrected owned Editor started normally.

Actual Play Mode advanced from frame214 to2330 and3960. Twelve native invitation checks pass: visible by default, exact single-sentence copy, three rendered text lines without overflow, hides for other selections/options/notes, actual dismiss callback, stays dismissed on return/cancel, direct Wisconsin flight callback and hidden during flight. Actual EventSystem raycasts distinguish the bubble body from its dismiss control. These receipts do not claim browser or screenshot acceptance.

The subsequent full Wisconsin arrival exposed a separate incoming-main regression: guidance previews the new cancellation action on empty land and throws before LandscapeReady. Exact stack begins CancellationRefund → ProposedQueue → AnnualPlanningGuidance.Create24 → TerraHud.AnnualState118. Native-arrival.json retains the incomplete state. The annual/Peter integration owners were notified; the nudge does not change that path or suppress the exception. No new player was published with this failure.

## Combined release handoff

The annual integration owner confirmed nudge runtime237e1cf is included in combined sourcea6e555f together with upstream guidance correction1330dc9 and narrationad2c093. The owner will verify default/compact presentation, dismiss, direct Wisconsin entry and return in the combined player; final publication belongs to the user-authorized release coordinator. The standalone native-capture request146 was cancelled to avoid duplicating that work. No standalone export/deployment or browser visual acceptance is claimed here. The owned Editor closed gracefully and all reservations were released; generated metadata, font cache and machine configuration changes were restored.
