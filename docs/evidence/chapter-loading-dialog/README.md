# Await AI inside the annual dialog — October 25, 2026

Peter requested that End Year keep its dialog open with a loading bar, then show the AI response directly. The prior save-first path emitted an offline acceptance event, closed the dialog, and put the eventual AI response behind a separate button.

## Behaviour

The session now remains busy through local numerical acceptance as well as network transport. The numerical year and local fallback still save first. TerraMain shows a waiting dialog instead of the year overview until AI or fallback completes. A moving indeterminate bar shows activity without inventing a percent. Completion opens the explanation directly in the same modal surface, for both annual and Year-10 results. AI failure or **Use local summary** shows the saved local result. Explicit Escape/return-to-landscape remains possible.

Initial persistence logs `TERRA_ANNUAL_SAVED ... awaiting AI explanation`. The final Gemini/fallback receipt logs the terminal duration, including network wait. No numerical outcome is assigned by the model.

## Validation

Session regression tests cover busy-during-save, pending versus final logging, quota fallback, immediate send failure, success, stopping/continuing, reload and accepted Year-10 explanation. API compilation covers runtime sources against actual Unity/package assemblies. Native Unity compiler and modal lifecycle are checked during local sync; rendered acceptance remains separate from the wider Phase 2A scenery gate.

TN-06: brief contextual feedback; CS-01: one stable dialog surface and explicit local fallback. AX-01: test the actual native modal, including bar removal on success/failure and another modal, before claiming a rendered result. No production deployment.
