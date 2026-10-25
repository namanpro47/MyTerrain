# Field Notes ownership / release handoff

Runtime candidate: **6364b3c**, branch `codex/field-notes-ux`, from `origin/main` 13a15a4. Local export `Builds/Web` in the field-notes-ux worktree completed successfully. Checks: 2,399 core; 102 narrative; 859 main HUD + 16 chapter. Browser acceptance remains in progress until the README/browser receipt says otherwise. No independent main promotion or deployment.

Shared glue is minimal: `TerraHud.cs` calls `ResetFieldNotes()` at ShowModal entry and `LayoutFieldNotes()` at Layout end; `TerraMain.cs` forwards wheel input to `hud.ScrollFieldNotes(amount)` before rejecting other modal scroll. Preserve these with the audio changes. `TerraHud.Annual.cs` uses `FieldNotesBudget(...)` and the literal `End Year` planning caption; preserve incoming annual guidance lines. Existing test click selection now filters to the open dialog.

Placement handoff: the pending placement work may take these three shared files after the Field Notes board claims are released. Preserve the hooks, consolidated budget, narrative/bar partials and existing source history. No placement changes are part of this export or current release. No runtime edits are pending at this checkpoint; visual browser checks may still identify a necessary correction.
