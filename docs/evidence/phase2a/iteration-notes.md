# Phase 2A iteration notes

## Iteration 1 — actual Chrome, 1280×720 / DPR 1

**Not accepted.** [Wide](captures/iteration-01-wide.png), [preview](captures/iteration-01-preview.png). Primary references: TN-01–07, CS-01/04, AX-01 in [the reference library](../../DESIGN_REFERENCE.md).

The valley is substantially larger in frame; the dark HUD, illustrated treatment tiles and clustered woodland improve hierarchy. The guide retains actual source limitations and has a manually controlled keyboard focus loop. The original 139 contract checks pass again; [result](contract-checks.json). Editor Play mode advanced from frame 32116 to 32178 during the server-side wait, with no Console errors. Editor checks supplement browser checks; they do not replace them.

The actual browser exposed material defects: Manage water's title wraps into its subtitle; the unbacked instruction loses contrast over the field; the flat preview does not distinguish treatments; ground surfaces remain too uniform at close range. These prevent acceptance. The next pass widens the tool layout, backs the instruction, adds treatment-specific proposal patterns and subtle procedural surface grain, and tests camera/outline motion.

Reloading the old local origin after replacing its export produced `Class::FromIl2CppType` / stack overflow before the scene started. The same new export ran successfully at a fresh local origin. This is evidence of a cache/version mismatch, not a proven engine defect. Future exports use Unity's supported hashed-filename setting to keep data/framework/WASM versions together. Verify upgrade/reload on the final build before closing this issue.

During early inspection, both viewport rendering and gameplay changed between agent actions. The user confirmed concurrent play and then yielded the browser for controlled verification. This is not evidence of a gameplay accounting bug. Restore the explicit viewport and a known proposal baseline before measurements; exclude the disturbed intermediate captures from acceptance. Controlled evidence uses Chrome's viewport and DOM keyboard APIs, with actual canvas dimensions recorded.

No public Phase 2A deployment or final visual/interaction/performance pass is claimed by these iteration notes.
