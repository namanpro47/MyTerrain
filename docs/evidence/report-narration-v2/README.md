# Report-only narration v2 — October 25, 2026

Base: main `67c0141`, isolated `codex/report-narration-v2`. This additive service is ready for the new annual engine's accepted-report adapter. It does **not** activate that engine, change existing saves, accept a year or modify a budget. The old `/chapter` endpoint and its v1 resolving-snapshot contract remain intact.

## Behavior and truthful presentation

`POST /report-chapter` receives canonical explanatory blocks derived by the numeric adapter from an **already saved accepted report**. An optional model selects and orders one to three existing fact IDs. The shared helper copies the corresponding title/text verbatim. The model cannot provide free prose, amounts, sites, effects or a `CompleteYear` command. Extra fields, unknown/repeated IDs and invalid selections are rejected, with at most two repair attempts before local fallback.

This deliberately narrows the earlier free-prose proposal. Valid fact IDs alone cannot detect reversed causality or a fabricated claim; selecting canonical blocks makes consistency deterministic. The accurate label is **“AI-organized explanation”**, rather than claiming the model authored verified canonical words. Details can explain that AI selected highlights from saved explanations. Local mode is **“Local summary · saved simulation results”**, selecting the first three facts in canonical priority order. The adapter must place essential scores/deltas and saved status in the fixed introduction or surrounding accepted-report UI, regardless of selected cards.

Missing configuration, transport failure, timeout, quota/rate limit, refusal and invalid output automatically produce a local receipt from those same facts. Safe typed reasons never expose provider bodies, headers, keys or guessed billing/credit status. “Quota” means the provider reported a quota/rate limit; it does not diagnose the account's finances. Model mismatch and provider unavailability are separately identified. Numeric-resolution/persistence failures are not handled by this presentation service.

## Shared contract

[DTOs](../../../Assets/Terra/Scripts/Contracts/ReportNarrationContracts.cs), [portable helper](../../../Assets/Terra/Scripts/Core/ReportNarration.cs), [service](../../../services/chapters/ReportNarrationService.cs), [provider adapter](../../../services/chapters/ReportGeminiNarrator.cs).

- `ReportNarrationRequest`, schema 2: `year` (1–10), `acceptedTransitionId`, lowercase SHA-256 `acceptedTransitionHash`, `rulesVersion`, `reportVersion`, `promptVersion`, `modelVersion`, canonical `title`/`introduction`, and 1–64 ordered `facts`.
- Each fact has a unique `id`, canonical `title`/`text`, and explicit `siteIds`/`causalRuleIds` arrays (empty when absent, at most eight each). Identifiers are bounded ASCII; title ≤100 UTF-16 code units, introduction ≤500, fact text ≤600. Controls other than newline, rich-text opening brackets and invalid Unicode are rejected. The numeric adapter must check site/rule membership against the accepted report; this helper only validates their bounded representation and identity.
- Provider selection contains only `factIds`, one to three distinct supplied IDs. Structured JSON and [Gemini's documented schema feature](https://ai.google.dev/gemini-api/docs/structured-output) constrain shape; the local validator remains authoritative. The provider envelope is streamed with a 128 KiB limit and selection text has a 4 KiB limit. No live model capability/availability claim follows from mocked transport checks.
- Receipt echoes transition identity, request hash, rules/report/prompt/model/contract versions, mode/reason, canonical summary and selected `{factId,title,text}` cards. There are no applicable consequences or town snapshots.

Constants: `ContractVersion = report-selection-v2.0.0`, `PromptVersion = report-highlight-selection-v2.0.0`, default model contract `gemini-3.5-flash-lite` (existing configured model, not a claim that it is the latest or available to every key). `modelVersion` is pinned by the request; a mismatched service configuration consumes no quota. Local receipts preserve the expected model contract for identity but clearly mark that no AI selection was used.

Public helper surface: `ValidateRequest`, `CopyRequest`, `RequestHash`, `CacheKey`, `ValidateSelection`, `BuildReceipt`, `Local`, `ValidateReceipt`, `ModeLabel`, `ReasonMessage`. `CopyRequest` detaches arrays before any service await; provider input is another detached copy. A receipt's title, introduction and card wording must match the saved projection exactly, including amount, direction and cause. Validation promises consistency with the supplied projection, **not scientific truth or proof that an arbitrary caller actually committed it**. The client owns that accepted-state check.

## Hash and first-receipt rule

The request hash is SHA-256 of an explicit binary encoding, not serialized JSON. Every integer is signed nonnegative 32-bit little-endian. Every string is a 32-bit byte length followed by strict UTF-8 bytes, with no BOM or Unicode normalization. Arrays start with their count and retain order. Fields are written in this order:

1. ContractVersion string; schemaVersion integer; year integer.
2. acceptedTransitionId, acceptedTransitionHash, rulesVersion, reportVersion, promptVersion, modelVersion, title, introduction.
3. Fact count, then each fact's id/title/text, siteIds array, causalRuleIds array.

The [synthetic contract fixture](fixture-request.json) hashes to **06070d5012ba1d2e1a2850c49a0598abb9db225be0f03ae499448493e9ea0f86**, independently reproduced in Python and C#. It is not a gameplay/save fixture or observed model output.

Cache key uses the same encoding over `report-narration-cache-v2`, ContractVersion, acceptedTransitionHash, promptVersion and modelVersion. Transition ID and full facts remain in the request hash: reusing a cache identity with different input fails with HTTP409. Per-path gates serialize duplicates across service objects; atomic file publication handles a separate-process race without replacing the first verified file. The first AI **or local** receipt is stored under a separate `report-v2/` directory before returning. Later provider recovery cannot silently overwrite a local choice. Corrupt caches fail without regeneration or more quota. This is a single-machine prototype, not a multi-host service or a power-loss durability certification.

Provider work is limited to 30 seconds across initial selection and two repairs; the existing HTTP client timeout is 25 seconds. Caller cancellation propagates without publishing a new receipt. Waiting duplicate requests use caller cancellation and then reuse the first completed receipt. Disk failure returns503; the client can still show its own local summary from its validated, saved numeric report. HTTP422 rejects invalid projections, and no endpoint call commits or replays simulation state.

## Client handoff

The annual-engine owner must:

1. Commit and save the numeric transition atomically first, derive canonical facts from that exact accepted report, and pin identity/versions. Never substitute an older report when numeric resolution fails.
2. Save the pending narration identity separately. Send this projection, not a resolving snapshot. Continue/fast-forward may proceed without waiting for prose.
3. Validate each response against the corresponding saved projection. Persist the first visible receipt/mode. Ignore stale replies, different hashes and late AI responses after local mode was chosen.
4. If the service is missing/unreachable or rejects a projection that the client can independently rederive/validate, use `ReportNarration.Local` over the matching saved projection. A failed numeric calculation/save is still pending, not a prose-fallback success.
5. Show report year, mode and concise reason; provide full report details beyond the three highlights. Use `siteIds` only after accepted-site validation for optional map focus.

## Checks performed

- **95 standalone assertions**: canonical amounts/causes/direction; identity/version/hash changes; malformed and duplicated IDs; limits and Unicode; concurrent duplicate and separate service ownership; restart caching; typed provider failures; bounded repair; timeout/cancellation; caller/provider mutation isolation; corrupt/persistence-failed cache; unknown/extra free prose; malformed/oversized provider envelopes; safe error handling. No live provider calls.
- **42 existing v1 chapter checks passed**, preserving legacy behavior.
- **14 actual loopback HTTP checks** at port61298 with the key explicitly empty: automatic local summary, byte-identical duplicate, conflicting facts409, extra snapshot/schema rejection422, origin/Host rejection403, legacy valid missing-key503 and v2 rejection on `/chapter`. [Receipt and checks](http-checks.json), [actual local response](fixture-local-receipt.json). The test server was stopped afterward.
- The shared DTO/helper builds with C#9 against **.NET Standard2.1**, zero warnings/errors. The service targets net8.0; executable checks ran on installed **.NET10.0.13** with `DOTNET_ROLL_FORWARD=Major` because .NET8 was unavailable. This is not an actual Unity/IL2CPP or browser-client test.

Reproduce with the existing SDK:

```sh
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/report-narration/ReportNarrationChecks.csproj
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/chapters/Terra.ChapterChecks.csproj
```

Omit roll-forward when a compatible .NET8 runtime is installed. `TERRA_CHAPTER_PORT` optionally selects another port1024–65535 while keeping the bind on127.0.0.1; default8787 and existing origin controls remain. It is a shell environment setting, not read from `.env`. No service was deployed, no browser result or live-provider success is claimed. The numeric v4 adapter, client persistence/late-response UI, combined gameplay/browser acceptance and Phase2A remain separate work.
