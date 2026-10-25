# Annual chapters and the Gemini connection

Peter requested the next-chapter mechanism and a cheapest/free LLM connection on October 24, 2026. This slice starts from refreshed `origin/main` at `e247431`, in the isolated `Terra-chapters` worktree, branch `codex/annual-chapters-llm`.

## What the next chapter does

Each chapter is one year in the existing Meadowbrook Valley run: queue actions → conclude year → read up to three cards → continue planning. Waiting advances the same saved scenario and obligations. **See my ten-year future** confirms the current plan, then continues without adding new interventions. Year 10 ends the run. No additional map or level is created.

`AnnualChapterSession` wraps the existing annual session. The developer window **Terra → Annual Town → Playtest Year Loop** offers offline chapters, optional Gemini chapters, retry, stop waiting, explicit offline fallback, next year, ten-year continuation, export/import and autosave restore. The engine owner can bind the same component to the main game; TerraMain, TerraHud, renderer and scenes are unchanged.

The existing annual ledger, lifecycle and saved drivers compute the outcome. The server replays that same C# code in a detached town, sends a compact set of computed facts to Gemini, validates the response, then returns the narrative and exact engine consequences. Unity validates the receipt before committing. Gemini cannot inject even an otherwise in-range numeric effect.

This is deliberately **AI narration over an illustrative annual simulation**, not the complete PRD §7 causal consequence generator. The current repository has no approved causal reference pack, calibrated weather sampler or grounded effect ranges. Arbitrary model-selected numbers would not fill that gap. A future effect pack needs separate authorship and tests before enabling AI-proposed numeric consequences. The existing definition's “No LLM” assumption describes its numeric simulator; the chapter layer may use an LLM for prose.

## Run locally

Requires .NET 8 SDK; no additional NuGet packages. On this Windows workstation Unity bundles a compatible SDK at `C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Data\DotNetSdk\dotnet.exe`. On another machine use `dotnet` from the installed SDK.

1. Create a Gemini API key in [Google AI Studio](https://aistudio.google.com/apikey). For zero API spend, use a project on the free tier without enabling paid billing. Copy the repository-root `.env.example` to `.env`, then paste the key after `GEMINI_API_KEY=`. The local chapter server automatically reads this file on startup. It is ignored by Git and excluded by `.vercelignore`. Never put the key in a scene, Unity field, browser code, public export or committed config. Restart the service after editing the file.
2. From the repository root run:

   ```powershell
   & 'C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Data\DotNetSdk\dotnet.exe' run --project services/chapters/Terra.Chapters.csproj
   ```

3. Read `http://127.0.0.1:8787/health`. `configured: true` only confirms a key exists; a successful chapter is the actual provider check.
4. Open **Terra → Annual Town → Playtest Year Loop**, start a run and enable **AI chapter (Gemini service)**. The default endpoint is `http://127.0.0.1:8787/chapter`.
5. Queue actions, conclude a year, inspect the chapter and ledger, and continue. If no key/quota is available, the year stays pending; retry or choose **Use labelled offline chapter**.

Optional server variables: `GEMINI_MODEL` (default `gemini-2.5-flash-lite`), `TERRA_CHAPTER_CACHE` (defaults to `services/chapters/data` when launched with this project), and `TERRA_ALLOWED_ORIGINS` (comma-separated exact origins, default localhost/127.0.0.1 port 8080). Existing process variables take priority over `.env`. The reader supports plain or quoted values, comments and an optional `export` prefix, without executing shell commands or expanding variables. It reads only the four supported chapter settings. On Vercel it skips local-file loading entirely.

### Keeping the provider key private

The intended public flow is browser → our backend → Gemini. Only the backend reads `GEMINI_API_KEY`; responses contain the chapter or a sanitized error. The local ASP.NET service has no static-file middleware and does not serve `.env`. Keep the root `.env` outside `Assets`, `StreamingAssets`, `Builds/Web` and any public directory. The file is plaintext on this computer; Git/deploy exclusions do not protect it from someone with local filesystem access.

For a hosted backend, add `GEMINI_API_KEY` as a project **Secret** in the relevant Vercel environment, then deploy a new backend version. Vercel encrypts environment values at rest and makes Secret values write-only in its dashboard after saving. Those settings do not prevent application code from leaking a value into a response, log or browser bundle. Never use `NEXT_PUBLIC_`/`VITE_` prefixes or embed the key in the Unity Web build. [Vercel environment variables](https://vercel.com/docs/environment-variables), [Secret variables](https://vercel.com/docs/environment-variables/sensitive-environment-variables).

The current Vercel export contains static Unity files only; adding a Vercel variable alone does not deploy or connect the chapter backend. Public hosting also needs request authorization/session controls, server-side rate and token limits, durable storage and provider quota/spending controls. CORS is a browser policy, not authentication: someone can call a public API directly even when their website origin is rejected. A server compromise or accidental log/response leak can still expose a secret; rotate the key if that happens. This follow-up adds local dotenv loading and exclusions, not public hosting or a complete abuse-prevention system.

## Provider choice and cost

Official documentation checked October 24, 2026:

- [Gemini pricing](https://ai.google.dev/gemini-api/docs/pricing): Gemini 2.5 Flash-Lite lists free text input/output on the free tier; paid text rates are **$0.10 per million input tokens / $0.40 per million output tokens**. This is a practical free starting option, not a claim that every provider/model was compared. For illustration, 3,000 input + 800 output tokens would cost about **$0.00062 per chapter** on those paid rates, excluding repairs. Actual usage/latency remains to be measured.
- [Gemini rate limits](https://ai.google.dev/gemini-api/docs/rate-limits): check the project's actual limits in AI Studio. Do not promise a fixed daily free allowance. Quota errors do not trigger an automatic paid-provider fallback.
- Free-tier requests may be used to improve Google's products; send game facts only, not personal player information. This adapter sends neither run IDs nor complete save history to the model.
- [Groq limits](https://console.groq.com/docs/rate-limits) also show free plans, but this slice implements one provider rather than a routing framework.
- [Gemini REST generation API](https://ai.google.dev/api/generate-content) supports structured JSON output. The adapter uses the documented `generateContent`/`responseSchema` compatibility route for 2.5 Flash-Lite. The real endpoint still needs a credentialed smoke test; mocked tests cannot establish current model access.

## Failure, persistence and ownership

- Each request freezes run/year/revision/action input through the existing domain transaction. Planning remains locked while pending. No cash/time is committed on a provider failure.
- Chapter v1 has a fixed rules version. IDs, facts, card count, field types, text lengths and markup are validated. The engine's exact numerical consequences are enforced on the server and client. Prose quality/causal accuracy still requires live review; valid references do not prove every sentence is faithful.
- At most one initial model request and two schema/validation repairs, within a **30-second server deadline**. Quota, refusal and truncated output fail explicitly. Unity allows 35 seconds for transport. The PRD's proposed ten-second latency target is not yet measured.
- The server saves the first validated receipt before responding and reuses it after retry/restart. Same run/request ID with different input is rejected. A single service process serializes generation. It is a local prototype, not a distributed store.
- The client autosaves the frozen request before networking, then the accepted year/chapter. Reload restores a pending request or completed chapters without regenerating them. Stopping a request leaves the plan frozen. Explicit offline fallback aborts the pending client transport and uses the same deterministic outcome.
- The server preserves Unity/Mono's existing numeric hash formatting rather than modern .NET's shorter `R` representation. The captured Unity request in `tests/chapters/fixtures/unity-pending-year.json` checks interoperability without changing existing Unity save hashes.
- Chapter export/import uses its own versioned envelope, including town history and chapter receipts. It does **not** accept legacy bare-town JSON. Existing `AnnualTownSession` bare-town import/export remains available separately. PlayerPrefs is local unencrypted storage, not cloud backup or tamper-proof accounting.
- If fast-forward fails, accepted earlier years remain saved and the failed year remains pending. Retry that year, then resume the remaining years. No speculative Year 11 is created.
- The service listens only on **127.0.0.1:8787**, rejects unknown Host/Origin headers and limits request bodies to 256 KiB. Public deployment is separate work: add authentication, per-user quotas, durable shared storage, same-origin HTTPS routing and provider spending controls. A public static Unity build cannot use this local server as its hosted backend.
- The teammate owns final main-game integration and presentation. Use **TN-03** (compact outcome groups), **TN-08** (inspect the result before continuing) and **CS-01** (details on request). The editor fixture does not certify Phase 2A or production browser layout.

## Checks

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Data\DotNetSdk\dotnet.exe' run --project tests/annual/Terra.AnnualChecks.csproj
& 'C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Data\DotNetSdk\dotnet.exe' run --project tests/chapters/Terra.ChapterChecks.csproj
```

Unity: **Terra → Annual Town → Run Chapter Checks**, or batch `-executeMethod AnnualChapterChecks.RunBatch`. The chapter tests cover the real map adapter, Unity JSON, pending/accepted/completed restore, PlayerPrefs, exact-once acceptance, explicit offline chapters and Year 10.

See [the verification receipt](evidence/annual-town/chapter-verification.json) for actual completed checks and remaining limits.

## Next implementation gaps

Bind the annual controller and scaled build catalog into the engine owner's main UI; author an approved causal pack if AI-proposed effects are desired; add the paired same-weather no-action comparison; then complete ending presentation and browser/novice checks. These are separate from this chapter connection. The wider catalog is not automatically a validated annual price/effect book.
