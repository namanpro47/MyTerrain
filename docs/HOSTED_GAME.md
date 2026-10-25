# Hosted game and separate narration backend

The Unity game remains in [namanpro47/Terra](https://github.com/namanpro47/Terra), targeting Naman's `terra-browser` Vercel project. The JavaScript backend is in the private [lakys-openclaw/terra-backend](https://github.com/lakys-openclaw/terra-backend), linked to Peter's `terra-chapters` Vercel project. No personal clone of the game is published.

## WebGL configuration

`HostedChapterBuild` configures the temporary WebGL build scene: an existing loopback chapter endpoint becomes `https://terra-chapters.vercel.app/chapter`. Explicit HTTPS endpoints are preserved; non-HTTPS public WebGL endpoints fail the build. Source scenes and desktop/Editor developer endpoints retain their local settings. The build logs `TERRA_HOSTED_CHAPTER_ENDPOINT` so the artifact's configuration can be checked. Run the existing `TerraBuild.BuildWeb` export with this processor included. A previously published player is not updated by a backend deploy or source push; it must be rebuilt and published in Naman's project.

## Public access and saved results

The browser first posts to `/session` from the allowed game origin. It keeps a short-lived opaque guest token in memory, then sends it with `/chapter`. Tokens are bound to their allowed origin, with hashed state and per-IP/session limits in Redis. No shared API key, login or third-party cookie enters the game. On expired authentication the client refreshes its guest session once and retries the same frozen year.

These guest capabilities are not player identity or strong abuse prevention. Scripts can imitate origins, shared networks share limits, and the backend's durable global cap of **20 provider attempts/day** is the spending bound. Attempts include prose repairs and failures; cached receipts spend no extra provider quota. At best this budget covers about two ten-year demo runs daily. Existing preview and production use the same Redis receipt namespace and shared cap; no paid store or auto-upgrade was added.

`AnnualChapterSession` saves the frozen submitted year before networking. On service failure, quota, timeout or invalid output, it produces the existing local text template over those same saved inputs and validates it through the normal receipt path. It does not reroll weather, add effects or change ledger arithmetic. The result says **AI unavailable** and the story UI labels a **local summary**, with no AI used. Manual local summaries and explicit retry/recovery remain available. `fallbackWhenUnavailable=false` retains manual recovery for developer tests. Accepted receipts retain existing save formats; reload requires no provider request.

The current design still accepts the financial/year transition together with a validated online/local chapter. This wiring does not implement the future report-after-commit simulation redesign or new weather/yield rules.

## Verification

See `docs/evidence/hosted-game` for the exact current candidate, source checks, build and public API evidence. TN-06/08 and CS-01 remain the relevant short-story and evidence-disclosure patterns: fallback text identifies its source while the accepted budget/meters remain available. API tests do not establish interactive browser/visual acceptance or Naman's final production artifact.
