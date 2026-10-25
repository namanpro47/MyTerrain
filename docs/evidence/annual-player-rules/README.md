# Annual player rules v5 validation

Candidate base: `19e7eae835ed4c06a21d42887a9a53ed28cc4fc2` plus research commit `ed9eccb` (cherry-picked as `15d1497`). Worktree `Terra-annual-player-rules`, branch `codex/annual-player-rules-20261025`. No real provider call or deployment.

## Verified

- PlayerRulesChecks: **77 assertions**. All three regional schedules/replays; integer-cent ledger arithmetic; partial cancellation and completed-project zero refund; undo, retry, reload and rebuilding; negative cash, continued upkeep and funding repayment; six financed data centres yield a negative Environment score; no extra utility rationing; engine-owned event effects; persistent damage and one repair bill; neutral planning preview; Year-10 history context and cross-locale save replay. `checks.json` records selected computed values.
- ChapterSessionChecks: **13 assertions**, transport/Unity stubs. Numerical result and local chapter exist before transport; 429 fallback; prose-only AI success; restart; older chapter save cannot rewind newer annual results; stop/continue while AI is pending. These establish adapter logic, not real browser storage/JsonUtility behaviour.
- Existing connected full-game suite: **1,019 assertions** passed.
- Existing numeric-v4 simulation suite: **889 assertions** passed, including exact weather bytes, legacy hash/replay fixtures and cross-locale transitions. Its model was not activated in the current UI.
- Existing chapter suite: **50 assertions** passed; no live provider.
- All runtime C# sources compiled against actual Unity 6000.6.6f1 engine modules, existing package assemblies and DOTween via `UnityApi.csproj`: zero errors; existing obsolete-API/unused-field warnings. This is API compilation, not an Editor import, IL2CPP/WebGL build or rendered scene test.
- Chapter service builds with zero warnings/errors. `git diff --check` passes.

The new LF attributes preserve byte-pinned weather JSON on Windows; the numeric digest tests initially exposed CRLF conversion. No weather observations or equations were changed.

## Reproduce

From the repository root with .NET 8:

```
dotnet run --project tests/annual-player-rules/PlayerRulesChecks.csproj -- --connected
dotnet run --project tests/annual-player-rules/ChapterSessionChecks.csproj -- --connected
dotnet run --project tests/full-game/Terra.FullGameChecks.csproj -- --connected --evidence Builds/player-rules-legacy-checks
dotnet run --project tests/annual-simulation/Terra.AnnualSimulationChecks.csproj
dotnet run --project tests/chapters/Terra.ChapterChecks.csproj
dotnet build services/chapters/Terra.Chapters.csproj
```

Optional API compile: `dotnet build tests/annual-player-rules/UnityApi.csproj -p:UnityManagedDirectory=<Editor/Data/Managed/UnityEngine> -p:UnityPackageAssemblies=<matching imported project's Library/ScriptAssemblies>`. Package assemblies are read-only reference inputs; never share a live Library between projects. The Windows runner used Unity's bundled .NET SDK.

## Native UI handoff

TN-05: Browse accepted project → Cancel project → progress/refund confirmation → queued cancellation → End Year → free site next year. No finished-building preview should appear when cancelling an unfinished project. TN-06/CS-01: compact visible debt label, gated construction controls, deeper rules in confirmation/ledger; year overview labels **Local summary · no AI**, **Local summary · AI pending** or **AI explanation**. Negative score labels remain signed and meter geometry stays bounded.

AX-01 is still open: verify those controls in a fresh Chrome export, increased text size, final-year report and stop/continue during optional AI. No screenshot or visual acceptance is claimed. Compare with the latest accepted placement/year-overview captures; the renderer's broader Phase 2A visual gate is separate.
