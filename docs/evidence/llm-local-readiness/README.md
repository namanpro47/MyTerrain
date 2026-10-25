# Local Unity LLM readiness — October 25, 2026

Peter reported year-end dialogs not returning AI results and requested the open local Unity project be updated to current main before his interactive QA.

## Findings and repair

- The open project was `C:/Users/Peter/Terra-Unity`, branch `codex/unity-local-playtest`, at `ce27355`, behind the annual v5 rules on `06f51b2`.
- Editor defaults pointed at `http://127.0.0.1:8787/chapter`, but nothing listened on that port. The default client also used hosted JavaScript facts protocol; the local C# endpoint requires the original frozen annual request. TerraMain now chooses the protocol from the endpoint: loopback uses the C# request; hosted uses the existing facts/guest-session flow.
- Fast-forward intentionally created local summaries and never requested AI for Year 10. It now saves every year locally and then requests optional ending prose from the original accepted Year-10 evidence. The live town is never reopened, charged or rerolled.
- Accepted v5 results can now request an explanation after reload/fast-forward. Local fallback narrative correctly describes cancellation and negative public cash.

## Verification

- **19 session checks** passed using Unity/transport stubs, including protocol selection, accepted-Year-10 reconstruction with nine earlier years, quota fallback and prose-only replacement.
- **77 core player-rule checks** passed.
- Chapter service builds with zero warnings/errors.
- Real requests to the local service returned **HTTP 200 / mode gemini / three cards** for Year 1 and Year 10. Provider: `gemini-3.5-flash-lite`; times **1.86s** and **1.97s**. These were real generated responses, not offline templates. See `provider-smoke.json` and the receipts. The service validates schema/fact references before responding.
- Existing local Gemini configuration is used only in the service environment. No credential is copied into source, command arguments, receipts or the Unity player.

These are backend, adapter and compilation checks. Peter retains interactive Unity year-end/ending QA; this evidence does not certify the rendered dialog or browser/WebGL deployment. No production deployment was performed.

## Local handoff

Keep the loopback service running on port 8787. Unity's normal local endpoint works without a scene override. Test End Year, then wait for **AI explanation** on the year overview. For the ending, use the ten-year future shortcut and wait for the optional Year-10 explanation. Numerical results/local summary remain usable if AI times out or becomes unavailable.

## Native Editor handoff

The open `C:/Users/Peter/Terra-Unity` checkout was fast-forwarded to main with its four pre-existing Unity-generated local changes preserved. A recovery patch was written under its ignored `Logs/AnnualSync` folder.

Native recompilation exposed a cross-assembly CS0122 error in the earlier combined-assembly check: gameplay referenced the internal `TownCopies` helper. Commit `2eedec9` replaces that use with the existing public-contract JsonUtility comparison. After syncing that fix, the actual Unity 6000.6.6f1 Editor reports **compilationFailed=false, compiling=false, consoleErrors=0**, with ten existing warnings. Play Mode is stopped and the Editor is ready for Peter's QA. The local Gemini service remains running on `127.0.0.1:8787`.
