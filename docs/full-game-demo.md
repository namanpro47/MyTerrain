# Ten-year Meadowbrook demo

This rehearsal follows the full annual game. It is a script to verify, not a claim that the visual or release gates have passed. Run in Chrome against the recorded release export. The exact public URL and source/build hashes belong in the release evidence.

## Three to four minutes

1. **0:00–0:25 — One real place.** Begin on the Living Atlas, choose Meadowbrook Valley and Explore. Stay in Wisconsin for this entire run. Reset the camera. Open the field guide briefly: Wisconsin terrain and mapped cover are real; the community, sites, budgets and future conditions are authored scenarios. Close it. Point out the three independent indicators and the public budget.
2. **0:25–1:10 — A public decision.** Select a farm action and an eligible field. Read its cost, reserved upkeep, opening time and services. Queue it, undo it, and queue again. Add a park or housing proposal on another fitting site. Review all proposals and submit Year 1. Read the annual chapter cards; if narration is unavailable, explicitly select the labelled offline result. Do not wait for a service during the demo. Money is paid once; accepted projects cannot be refunded with Undo.
3. **1:10–1:50 — Consequences and adaptation.** Read a concrete ledger change and one causal meter explanation. Continue to Year 2. Inspect construction versus operation. Choose a different priority or Wait. Reach the saved Year 3 wet condition and explain why the affected output differs.
4. **1:50–2:45 — Ten-year future.** Review the current plan, open Ten-year future, read the confirmation and advance. Show closing cash and obligations alongside all three scores. A mixed result still receives an ending. Open the timeline and compare the opening, chosen future and no-new-action land from the same camera.
5. **2:45–3:30 — A transferable question.** Open the optional learning question. Explain capacity versus resources/services, a benefit and a cost. Open New run, then cancel to show that the result is preserved. If demonstrating replay, confirm and verify Year 1 returns with $10M.

## Rehearsal record

Record two actual runs with start/end times, selected projects/sites, mistakes and fixes, source SHA, export identity, Chrome version, dimensions and evidence paths. Do not count an automated model run as a rehearsal or novice session. Keep the last verified local export available if the network fails. Any recorded fallback must show real gameplay from that export and be labelled as recorded, with its date and version.

## Current local checkpoint (not the accepted release)

The newest engineering backup is `Builds/Verified/7db6a5d` in the full-implementation worktree: 31 runtime files / 39,988,802 bytes, three fresh Chrome arrivals under 15 seconds, and an actual annual/fast-forward/replay smoke test. [Exact compression/build/browser evidence](evidence/full-game/web-compression/README.md). This main-based export does not accept the separate visual rebuild.

The prior water/performance control is `Builds/Verified/67f867e` in the full-implementation worktree: the Living Atlas, connected Wisconsin fields, current HUD and verified stream material. Its 31 served files total 45,867,155 bytes; [exact identities and Chrome checks](evidence/full-game/water-flow/README.md#corrected-chrome-decision--retain-bounded-improvement). It is not the final visual release. Prior checkpoints below remain historical recovery options.


The later project-edge export is frozen at `Builds/Verified/bb4d976`, pinned by [its file identities](evidence/full-game/project-edge-checkpoint.json), with actual Chrome lifecycle evidence. It remains an engineering backup, not the accepted release. The earlier perspective export is frozen at `Builds/Verified/peter-7e2117d` in the full-implementation worktree, matched to [Peter’s final export identity](evidence/peter-sync/final-build.json). Its [foreground checks](evidence/full-game/perspective-performance-summary.json) meet the documented stable-30 fallback on this machine. The preceding integrated export from `e25c5d3` is preserved at `Builds/Verified/e25c5d3` in the full-implementation worktree. Its ten files / 41,415,454 bytes are pinned in [checkpoint identity](evidence/full-game/e25-checkpoint-identity.json). Serve that folder with the same local HTTP command as `Builds/Web`; do not rebuild over it. The earlier export and explicitly non-final recording remain available. This backup is usable for engineering review, but does not replace the still-open scenery acceptance, two final rehearsals, public complete run or final recorded fallback.

## Claims to avoid

No local economic forecast, measured employment or population, legal consent, drinking-water production from wastewater treatment, downstream flood prevention, water-quality improvement, biodiversity or carbon estimate. Financial returns and service units are scenario rules. The fixed-storm calculation reports generated, retained and discharged volume within its bounded domain.


## Publish the accepted frozen export

After the scenery and final whole-game checks pass, freeze the tested output under `Builds/Verified/<source-sha>` and retain its file hashes. Record any generated build inputs separately from the commit. Serve and verify that frozen folder before publishing it. The deployment script accepts a repository-relative or absolute export directory and retains `Builds/Web` as its default:

```sh
sh scripts/deploy-web.sh Builds/Verified/<accepted-source-sha>
python3 scripts/verify-web.py https://terra-browser.vercel.app --build Builds/Verified/<accepted-source-sha>
```

Do not run those commands with the placeholder or count this prepared procedure as publication. Four mocked deployment-path checks verify default, relative, absolute and missing-export handling without publishing; [receipt](evidence/full-game/frozen-deploy-check.json). The [acceptance index and exact current-layout replay routes](FULL_GAME_ACCEPTANCE.md) collect the remaining release checks. The actual release receipt must include the deployment ID/URL, anonymous byte comparison, complete public Chrome run, two timed rehearsals and a matching recorded fallback. The opt-in `?capture=1` player records 60 seconds of actual canvas frames after F8; download the resulting WebM through Chrome, label it recorded with its date/source, and verify its duration and visible content. This canvas recording excludes the HTML atlas and does not establish recorded audio. Keep load/frame-time checks separate from recording overhead.
