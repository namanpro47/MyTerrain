# Annual town foundation — implementation and handoff

**Scope authorized by Peter, October 24, 2026:** implement annual town state, a USD public ledger and three outcomes per year while the engine owner continues the basic game/presentation. Undo/refunds apply only before submitting the year. This is an additive foundation; it does not finish the live game's annual controls, action catalog, visual rebuild, LLM service or scientific modeling.

## Product input and project location

The updated [annual PRD](PRD_ANNUAL_GAMEPLAY.md) was copied from Peter's edited `outputs/Terra-Annual-Gameplay-PRD.md`. It specifies public contributions, USD with a price year, multi-year obligations and separate community/treasury effects. The original WorldPack v1, legacy placement state and fixed-storm simulation contracts remain unchanged.

Remote: `https://github.com/namanpro47/Terra.git`. Fetched `origin/main` before implementation at `e355fa14144a6d0cc802543bc75d01526f61cf1d`, fetched again before handoff, integrated the teammate's new `08092a1f19ceb064ab21c11c6f97dd221693d410` visual-rebuild checkpoint, and repeated the source/Unity checks on that latest base. The handoff documentation keeps both assignments; no scene/renderer/HUD changes were made by this module.

This task's isolated working project is `C:\Users\Peter\Documents\Codex\2026-10-24\https-www-blockbyblock-org-our-work-7\work\Terra-annual`, branch `codex/annual-town-state`. The sibling `Terra-source` holds Git metadata for the worktrees; keep it. `Terra-prd` retains the earlier documentation work. The repository's `AGENTS.md` now requires a fresh remote fetch before new work and protects dirty feature worktrees from destructive synchronization.

## State machine and transaction boundary

`Planning → Resolving → Reviewing → Planning (next year)`; the last completed year enters `Completed`.

- **Planning:** preview/queue/replace an unsubmitted action, remove it, or undo. Cash has not been spent. Reserve all known public installments and finite operating commitments, not just the first installment. Existing committed sites cannot be demolished/replaced/refunded through the queue API.
- **Resolving:** one request ID, revision, input hash, action batch, asset state and saved annual driver are frozen. Providers receive detached copies. Queue edits are blocked. A timeout/exception or invalid response leaves the request and cash intact for retry. Cancel before acceptance returns to the unsubmitted plan.
- **Reviewing:** one atomic annual report records payments, remaining obligations, physical/service measures, three scores and accepted evidence. Queue/undo history clears. Repeated resolution of the same ID returns the accepted result, without charging again or calling a provider. Reusing an accepted ID with different consequences is rejected.
- **Continue:** increment the year explicitly. Waiting uses an empty queue and follows exactly the same annual transition, obligations, construction and establishment rules.
- **Completed:** no Year 11, new planning or historical refund. Outstanding payments beyond Year 10 remain visible; the game horizon does not erase a longer agreement.

Action phases are derived from funded timing: `Building`, `Establishing`, `Operating`, `Suspended`, `Retired`. A completed year never goes back through proposal undo. V0 redevelopment/renewal of an already committed site needs its own future action recipe; it is not a refund shortcut.

## Public ledger

Money is **integer USD cents**, with `priceYear` in every annual ledger. The order is explicit:

1. Pay the submitted upfront public contribution once.
2. Pay carried arrears, then due program/operating installments in stable oldest-asset/site-ID order.
3. Pay this year's authored unexpected public repair expense as far as cash allows. Record any unpaid required amount as arrears.
4. Add the scheduled closing-year appropriation and explicitly configured operational public receipts.
5. Carry actual cash, arrears and remaining full-term public commitments into the next plan.

Future appropriations and private harvest/sales are not spendable treasury cash. New discretionary spending cannot use cash reserved for existing obligations. A shortfall never makes cash negative or silently cancels an obligation. An unfunded operation supplies no active output/service/retention benefit that year; condition declines by a disclosed illustrative 0.1 rule. Required unpaid amounts carry forward. Closing appropriations do not retroactively rewrite missed payments.

The PRD example is checked independently: $10,000 cash; $600 + $375/year for three years; reserve $2,925; Year 1 cash $9,025 with $1,950 still committed; Year 3 cash $7,075 after all three installments, before any other configured spending or receipts. Example area, term, participation and conditions are authored; the test is not verification of real program eligibility.

## Three annual outcomes

Each report contains raw site/aggregate measures, an explicit rubric version and explanatory text. The default rubric is configurable and illustrative:

- **Economic:** 80% production against a stated target + 20% the fraction of public operating cost funded.
- **Social:** 80% service units against a stated community target + 20% the share of management sites without an accepted disruption.
- **Environmental:** 80% vegetated share of management-site area + 20% operational configured retention capacity against a stated capacity target.

Each component is bounded before aggregation into 0–100. The weights, targets, service-unit proxy and effect bounds are scenario assumptions, not validated measures of actual welfare. There is no combined morality score. Cover totals refer to management sites, not the full AOI. Configured capacity is not modeled runoff, a flood footprint or flood protection. Neither ecological establishment nor receipts start before the configured operation year.

Public grants affect the ledger, not the community's output measure. Private production affects illustrative economic/social measures, not automatic taxes. The offline wet-year scenario scales output by an explicit saved factor; it does not claim a forecast or hydrologically calibrated response.

## Configuration and consequence seam

`AnnualTownDefinition` contains versioned sites, per-site action offers, costs/payment terms, operating lifetime, construction/establishment timing, funded quantity/unit, public contribution versus total-cost status, operator, source/evidence labels, annual drivers and rubric. Costs stay fixed for the run. All arrays/returned snapshots are detached copies.

The store has one writer. In Unity, call state mutations on the main thread; marshal future asynchronous provider completions back to that owner. Request IDs/revisions reject repeated or stale logical operations; they are not a cross-thread lock or a distributed database transaction.

`AnnualTownDemo.FromWorldPack` supplies a **labelled compatibility example**, not a validated action catalog. It translates v1 fictional credits to USD at **$1,000 per credit**, uses **5% annual public operating cost for ten operating years**, a **$20,000 closing-year appropriation**, and an **authored Year 3 output factor of 0.8**. These are unverified game settings. The broader housing/energy/storage/stewardship/wastewater actions in the PRD are not silently enabled. Replace the demonstration definition with supported, site-eligible recipes as the team implements those actions.

`BeginResolution` produces a frozen `TownYearRequest`. `CompleteResolution` accepts only matching run/year/request/input hashes and bounded per-site effects. Unknown/duplicate sites, unknown source IDs, nonfinite/out-of-range numbers and condition gains on unfunded/nonoperational interventions are rejected atomically. Current effect bounds are production factor 0–2 and condition delta −0.25 to +0.15. Those bounds are authored, not calibrated uncertainty intervals. An effects array can be empty. Money, clock, sites and land area cannot be changed by a consequence payload.

The provider interface is currently synchronous for offline examples. A future server/LLM integration should use separate Begin/Complete calls around its asynchronous request. No API key, network provider, stochastic calibration, NOAA sampler, new flood/yield model or external side effect is implemented here.

## Engine integration

Use `AnnualTown` directly from plain C#, or add **`AnnualTownSession`** to an engine-owned controller. It is not automatically added to TerraMain or any scene.

1. After loading the world, call `InitializeFromWorldPack(world, scenario, packId, runId)` for the labelled demo, or `Initialize(yourDefinition, runId)` for the team's authored recipes.
2. Render `State`, `AvailableCents`, `CommittedCents` and the three measures from the most recent report. Subscribe to `Changed` for detached snapshots.
3. Bind preview/queue/remove/undo to the current-year plan; pass the displayed revision so stale clicks are rejected.
4. Bind End Year to Begin/Complete, or explicitly use `EndYearOffline(requestId)` for the demo. Keep the request ID stable across retries; use a new ID only for a new year.
5. Bind the report view to `State.history`; Continue uses `ContinueYear(revision)`. Disable planning in Resolving/Reviewing/Completed.
6. Call `Save()` at accepted state boundaries and choose a run-specific `saveKey`. Export/import JSON is also supported. Saving is explicit: this component does not overwrite another run or create an account automatically.

`AnnualTownDemo.Finish` / `AnnualTownSession.FinishOffline` process the remaining years through those same transactions without adding new actions. They preserve the current submitted/queued plan and maintenance rules, record each year, resume a pending request, and stop on provider failure rather than skipping it. The engine owner must confirm the player's choice before binding a fast-forward button.

Restore replays saved accepted requests/consequences with the same pinned definition, without calling the provider. It rebuilds derived reports and checks cash/assets/history consistency. It preserves an unfinished queue, undo frames and a pending resolution. A changed definition/version/data hash rejects the save rather than silently reinterpreting the past. This is integrity checking for a local educational game, not signed or tamper-proof competitive storage.

The existing `WorldStateStore` still describes the live immediate placement proposal. Do not let both stores spend the same money. The engine owner should use it only for provisional spatial preview or replace its UI binding with the annual queue when integrating. Legacy credits/instant visuals must not be presented as the new annual public ledger.

## Verification

Run the plain source specification without Unity or external packages:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Data\DotNetSdk\dotnet.exe' run --project tests/annual/Terra.AnnualChecks.csproj
```

In Unity: **Terra → Annual Town → Run Foundation Checks**, or batch `-executeMethod AnnualTownChecks.RunBatch`. It also exercises the actual map adapter, `JsonUtility`, the Unity session and ten-year restore. Evidence is written under ignored `Logs/AnnualTown/`.

**Verified on the latest base:** 81 plain-source assertions pass; Unity passes 86 annual assertions (including those 81), actual-map adapter validation with 32 existing offers, JSON/session round-trips and a ten-year saved history. The existing 139 Phase 2 contract/site/legacy-accounting assertions also pass. Unity batch exits successfully. [Verification receipt](evidence/annual-town/verification.json) records hashes and scope. No new annual HUD or browser interaction is claimed by a state-only check. Existing presentation remains with the engine owner; applicable design patterns are TN-03/05/08 and CS-01/04 when they bind these states to their new interface.
