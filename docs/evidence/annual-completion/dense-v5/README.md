# Dense v5 strategy and recovery checks

**327 assertions pass** against frozen combined source `e74496bf55ffa77427be69dcb221bb3bd70f3b41`, using the actual dense parcel files, inherited settlements, physical action catalog and schema2 / definition5.0.0 / `annual-player-rules-v5.0.0`. The same saved seed, `phase5-dense-v5-20261025`, is used for each map's baseline and both strategies. This updates the older dense-route evidence, which used definition4.0.0. It does not validate a later source automatically.

[Exact annual commands, public cash, commitments, scores and definition hashes](routes.json) · [68 pinned source/data hashes and program hash](source.json) · [program](Program.cs.txt) · [preparation](prepare.py).

| Landscape | Route | Final economic / social / environmental | Final public cash |
|---|---|---|---:|
| Meadowbrook | No new action | 50.1 / 58.0 / 38.8 | $20,000,000 |
| Meadowbrook | Community and services | 80.9 / 99.0 / 48.1 | $5,115,600 |
| Meadowbrook | Land and livelihoods | 77.6 / 91.0 / 47.8 | $6,240,200 |
| Point Reyes | No new action | 57.0 / 58.0 / 65.0 | $20,000,000 |
| Point Reyes | Community and services | 87.3 / 92.4 / 73.5 | $3,728,200 |
| Point Reyes | Land and livelihoods | 86.1 / 91.0 / 66.9 | $6,247,800 |
| Suusamyr | No new action | 33.3 / 58.0 / 65.0 | $20,000,000 |
| Suusamyr | Community and services | 64.7 / 95.4 / 73.5 | $4,254,000 |
| Suusamyr | Land and livelihoods | 60.3 / 90.4 / 67.0 | $5,669,500 |

The strategies retain the original seven/eight-project priority lists and choose physically valid, normally affordable offers through `Preview` and `QueueAction`. No financed fixture, injected money, altered costs or rewritten events is used. Every planned project is eventually submitted. Both strategies finish with positive cash on all three maps, with distinct accepted outcomes. This is not a claim that both meet every target: neither clears the environmental target in Meadowbrook, and neither clears the economic target in Suusamyr. Both clear all three thresholds in Point Reyes for this seed. A positive treasury does not establish three-meter success.

The Point Reyes community route enters **−$11,000** public cash after its Year6 solar proposal and the accepted year's consequences. It queues nothing in Year7 and closes that year at **$923,800**. Negative public cash is an explicitly allowed v5 state. The first harness incorrectly retained the older per-year no-negative-cash assertion; it stopped at this case. The corrected acceptance checks preserve exact ledger accounting, assert no separate legacy arrears and require positive cash at the ending, while retaining every intermediate balance in the receipt. The runtime model was not changed to make the test pass.

Each accepted annual ledger reconciles in integer cents, and meter values remain within the declared v5 ranges. All nine strategy/baseline endings survive a System.Text.Json round trip and reject Year11. Separately on each map, a public-park opening is continued through three paths: manual resolution with reload after every year; automatic continuation; and a deliberate provider failure in Year6 after five accepted years, followed by reload and resume. The failed pending request hash remains unchanged. All three final snapshot hashes are identical on each map. These are eighteen completed model decades in total.

## Reproduce

From the repository root, run `python3 docs/evidence/annual-completion/dense-v5/prepare.py`. It stages the exact Git source/data under ignored `Builds/DenseV5Acceptance`. From that directory, build `DenseV5Acceptance.csproj` in Release and run its DLL with .NET8. This machine used Unity6000.6.6f1's bundled `Contents/Resources/Scripting/DotNetSdk/dotnet` for build and execution; its default dotnet launcher had only runtime10 available. The final build reports zero warnings/errors. The program writes `routes.json` in that staging directory; copy it into this evidence folder after a successful run.

## Limits

This is deterministic model evidence for one saved seed per map. It is not exhaustive balance optimality, human observation, Unity JsonUtility round-trip proof, WebGL storage recovery, visual acceptance or performance acceptance. Human novice playtesting remains unavailable by the user's explicit instruction. The e74496b browser startup crash remains a separate release blocker, and its pending fix must be verified in actual Chrome. These checks do not promote that frozen build or the inactive numeric schema3 adapter.

## Repeated-family stress under v5

An additional **2,091 assertions pass across 51 complete decades**: sixteen recipe-family pools plus an all-regenerative-variants pool, each on all three dense maps. Supporting restoration/swale treatments and separately labelled regenerative recipes account for the difference from the twelve source-table families; solar/wind share a family. Every route uses normal starting money, actual offers, unchanged prices, the same saved seed as the mixed strategies, and stable site-ID/action-ID ordering. Each year queues all affordable unoccupied candidates in its pool, then resolves through the real model. [Full commands/results](stress.json), [program](StressProgram.cs.txt), [pinned inputs](stress-source.json).

No tested repeated-family route meets all three final targets. All finish with positive cash; none enters debt during this particular sweep. The assertions cover exact annual public ledgers, unique occupied sites, correct v5 debt representation, meter ranges and exact full-history restore. The debt-rejection branch in the program is present but was not exercised by these 51 routes; the mixed Point Reyes route above is the observed negative-cash/recovery case. No global dominance or all-ordering proof is claimed.

Selected Meadowbrook endings:

| Repeated family | Projects | Economic / social / environmental | Final cash |
|---|---:|---|---:|
| Regenerative rotation | 32 | 100.0 / 56.2 / 43.3 | $13,659,200 |
| Low-density housing | 19 | 48.8 / 80.3 / 28.0 | $1,637,999.62 |
| Public parks | 29 | 69.0 / 62.6 / 49.5 | $18,010,500 |
| Data centers | 3 | 88.1 / 44.4 / −31.7 | $8,930,000 |
| Non-renewable power | 3 | 68.7 / 55.8 / −63.4 | $2,710,000 |
| Stewardship | 32 | 62.5 / 64.0 / 81.2 | $13,415,500 |

These are authored v5 game indicators, including the explicitly approved signed environmental pressure rule, not measured local impacts or predictions. No prices, thresholds, financing or hazard chances were altered to produce the results.

Reproduce with `python3 docs/evidence/annual-completion/dense-v5/prepare.py --stress`, then build/run the staged project in `Builds/DenseV5Stress` using the .NET8 procedure above. The final build reports zero warnings/errors and writes `stress.json`. Source comparison confirms the 68 pinned model/data inputs are unchanged through the subsequent `cd6a193` loading/storage reconciliation; this does not verify its browser bridge.
