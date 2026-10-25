# Field Notes ownership / release handoff

Runtime candidate: **6364b3c**, branch `codex/field-notes-ux`, from `origin/main` 13a15a4. Local export `Builds/Web` in the field-notes-ux worktree completed successfully. Checks: 2,399 core; 102 narrative; 859 main HUD + 16 chapter. Chrome behavioral verification now passes 30 assertions, including reload; final screenshots remain queued. No independent main promotion or deployment.

Shared glue is minimal: `TerraHud.cs` calls `ResetFieldNotes()` at ShowModal entry and `LayoutFieldNotes()` at Layout end; `TerraMain.cs` forwards wheel input to `hud.ScrollFieldNotes(amount)` before rejecting other modal scroll. Preserve these with the audio changes. `TerraHud.Annual.cs` uses `FieldNotesBudget(...)` and the literal `End Year` planning caption; preserve incoming annual guidance lines. Existing test click selection now filters to the open dialog.

Placement handoff: the pending placement work may take these three shared files now that the Field Notes board claims on Main.cs, Hud.cs and Annual.cs are released. Preserve the hooks, consolidated budget, narrative/bar partials and existing source history. No placement changes are part of this export or current release. No runtime edits are pending at this checkpoint; visual browser checks may still identify a necessary correction.
