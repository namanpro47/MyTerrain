# Atlas / full-game reconciliation — October 25, 2026

Refreshed and fast-forwarded the full-implementation worktree from `6b28c77` to main `45a87481f01bf5360de169ee502e92697b5b68e4`. The atlas owner's explicit user request authorizes Wisconsin, Point Reyes and rural Central Asia; this checkpoint reconciles that delivered feature. It does not adopt the separate, unaccepted study39 renderer or waive Phase 2A.

## Exact build

Reused the atlas owner's successful export, verifying every file against its committed receipt before freezing it locally at `Builds/Verified/45a8748`. No runtime input differs from main at this checkpoint. The [HTTP receipt](web-assets.json) independently verifies **30 files / 45,004,955 bytes** from `http://127.0.0.1:65251/`. This fresh local origin has independent test saves. No public deployment or new frame-time claim.

## Checks

- All **41 atlas package checks** passed; all three WorldPacks packaged with verified checksums.
- The running Unity 6000.6.6f1 Editor passed **675 atlas assertions**, including 17 variants per region, ten-year save replay, runoff conservation and cross-location save rejection.
- [Chapter replay generator](chapter-replay.cs) exports real Unity JSON for Year 2 and Year 10 in every region, after committing Year 1 farmland. All six replay through `tests/chapters --unity-fixture` on .NET SDK 10.0.402 with `DOTNET_ROLL_FORWARD=Major`. [Hashes/results](chapter-replay.json). The compact Year 10 requests are 240,924 / 273,177 / 279,442 bytes, below the existing 1 MiB endpoint limit. These are serialized replay checks, not live-provider calls or an exhaustive request-size maximum.

Reproduce the fixture export with `unity command script eval_file docs/evidence/full-game/atlas-reconciliation/chapter-replay.cs --caller plugin --skill unity-cli --project-path <checkout> --format json`. Then run `DOTNET_ROLL_FORWARD=Major dotnet run --project tests/chapters -- --unity-fixture Logs/AnnualTown/atlas-request-year10-point-reyes-v1.json` (and the other generated filenames).

## Actual Chrome sequence

Chrome at 1440×900, October 25 approximately 08:14–08:21 UTC. Every game action used the visible controls or documented keyboard shortcuts. Pattern references: TN-06/TN-08 annual consequence and comparison; CS-01 budget clarity; AX-01 keyboard return. The unchanged landscape remains below the separate art gate.

1. Wisconsin: fresh arrival → no-new-project ten-year future → **Year 10 / Completed, revision 19, $19.85M**, 42.3 / 58.0 / 42.0. [Ending](wisconsin-ending.png).
2. Point Reyes: independent **Year 1 / $10M** → queue Coastal meadow 1 restoration, reserve $95K → submit. With no chapter backend running, the labelled pending state retains **$10M, revision 1, one proposal**. Returning to the atlas and resuming leaves the exact diagnostic unchanged. [Pending](point-reyes-pending.png).
3. Explicit offline acceptance produces **Year 1 / Reviewing, revision 2, $10.923M, one asset, $18K obligations**. [Accepted](point-reyes-accepted.png).
4. Suusamyr: independent **Year 1 / $10M** → queue High pasture 1 traditional farmland, **$120K reserve**, revision 1, one unpaid proposal. [Queued](suusamyr-queued.png).
5. Switch back through every region: Wisconsin retains its exact completed diagnostic and ending; Point Reyes retains its accepted chapter and money; Suusamyr retains its uncommitted proposal and $10M. [Wisconsin restored](wisconsin-restored.png), [Point Reyes restored](point-reyes-restored.png), [Suusamyr restored](suusamyr-restored.png).

[Unfiltered captured console](browser-console.json) includes the original and restored annual diagnostics. No Terra error-level entry appeared. Five `removeChild` errors originate in an injected Chrome extension during navigation; they are retained and not misreported as a clean browser console. The inherited optional URP warning remains. The stopped chapter backend is intentional for the pending/offline recovery check. This does not verify hosted narration, performance, the incoming HUD work, or the final public rehearsal.
