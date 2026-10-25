# Annual completion: balance and recovery audit

October 25, 2026. Fresh isolated worktree from refreshed `origin/main` **913c79fcfae6da89142762e747550fa8aa2cc240**. This delivery extends Phase 5/6 engineering evidence without changing the annual rules, prices, saves, HUD, scenery or build output. The active visual and rendering owners retain those paths. [Input identity](identity.json) pins the test and runtime sources.

The user explicitly asked to continue while recording **human playtesting as unavailable**. No novice comprehension, 8–12-minute duration, replay preference or educational effectiveness is inferred from these tests. Final visual acceptance, integrated Chrome checks, public full run and two timed rehearsals remain separate requirements in the [acceptance index](../../FULL_GAME_ACCEPTANCE.md).

## Reproduce

From the repository root, with .NET SDK 10.0.402 / runtime 10.0.13 used for this receipt:

```sh
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/full-game/Terra.FullGameChecks.csproj -- --connected --stress --evidence Builds/AnnualCompletion/connected
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/full-game/Terra.FullGameChecks.csproj -- --stress --evidence Builds/AnnualCompletion/legacy
```

`--evidence` directs generated results to a new directory instead of overwriting historical receipts. `--stress` adds 1,791 assertions to the existing 608, for **2,399 passing assertions on each layout**. The connected definition is the current Wisconsin 3.2 layout; the legacy definition retains 3.1 geography. This is two executions of the same suite on different definitions, not 4,798 independent invariants.

## Balance and same-conditions comparison

There are **51 ten-year routes per layout**: the 16 recipe family IDs plus an explicit pool of all three regenerative variants, each under stable-ID, smallest-site-first and largest-site-first priorities. Each year queues every affordable eligible unoccupied opportunity in that pool, then advances through the real engine. Wind and solar share the renewable family. The two supporting treatments and regenerative variant IDs explain why this count differs from the 12 source-table families.

Every route retains nonnegative cash, no arrears, unique occupied sites and capped housing/recreation/local services. All use exactly the no-new-action trajectory's drivers and appropriation schedule. **None meets all three final targets (75/75/60).** This detects these particular repeated-family exploits; it does not prove global optimal balance or enumerate every mixed strategy or site order.

Selected connected-layout results (stable-ID priority):

| Route | Projects | Economic / Social / Environmental |
|---|---:|---|
| No new action | 0 | 42.3 / 58.0 / 38.9 |
| Public parks | 8 | 48.1 / 64.0 / 42.4 |
| Traditional farming | 10 | 90.0 / 58.0 / 32.1 |
| Regenerative rotation | 10 | 98.3 / 58.0 / 47.2 |
| Restoration | 10 | 36.3 / 58.0 / 77.1 |
| Warehouses | 8 | 100.0 / 52.5 / 30.3 |
| Low-density housing | 9 | 41.9 / 77.8 / 32.4 |

The existing materially different mixed strategies still pass: community/services **86.2/99.0/52.1, $5.240M cash**, and land/livelihoods **94.2/91.0/61.5, $6.163M cash**. Their previously recorded interactive routes remain in the acceptance index. No prices or thresholds were changed to manufacture these results.

## Interrupted futures and late obligations

For **each Year 1–10**, a provider deliberately fails before acceptance. The engine retains that year's frozen request and only the prior accepted years. A mismatched response is rejected without any state change. JSON save/reload preserves the exact request; resumed fast-forward yields the **byte-identical serialized final snapshot** of the uninterrupted run, including all reports, requests, ledger, assets and revision.

Finishing a completed run does nothing. Re-delivering all ten accepted consequences does not change history or money. This is pure engine/provider verification; it does not claim transport, browser storage or Unity chapter-adapter fault injection.

High-density housing queued in Year 10 remains under construction, earns no premature tax and retains **$600,000** of operating obligations beyond the horizon. The ending ledger and authoritative commitment total agree.

## Adverse funding and adaptation

Two deliberately extreme test-only repair schedules exercise funding policy. **Neither replaces the production schedule**, whose Year 7 repair remains $150,000.

- A $20M Year 2 repair exhausts funds. The farm suspends in Year 3, no new discretionary offer becomes affordable, and waiting still reaches Year 10 with the exact **$1.270M arrears** retained. Reload reproduces the shortfall ending. No invented Year 11 or hidden debt forgiveness is used.
- A $12M Year 2 repair also suspends the farm in Year 3, but a useful park becomes affordable and is actually queued/accepted in **Year 4**. The run ends with **$7.668M cash and no arrears**. Each year's detached forecast leaves the live state untouched and matches the subsequent accepted ledger and outcomes.

Full routes, precise scores, every recovery-year cash/debt observation and the 10 interruption cases are in [connected results](connected-stress-checks.json) and [legacy results](legacy-stress-checks.json). [Connected count](connected-checks.json) and [legacy count](legacy-checks.json) include the original catalog/strategy suite.

## Handoff

Phase 5's requested stress and resume matrix now has reproducible coverage. Phase 6's shortfall and late-construction endings are verified at the model boundary. Keep the existing browser receipts versioned; these new model checks do not accept changed scenery or replace actual UI verification. This chat will own the final public manual run and rehearsals after the accepted visual export is ready; the existing implementation chat owns rendering performance/integration, and the deployment chat owns its current-main refresh. No deployment occurred here.
