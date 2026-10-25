# Peter release compatibility review — October 25, 2026

The user requested Peter’s latest pushes be reconciled and included in another coordinated production release. This check started from refreshed main `ea37af0` in an isolated worktree. The primary checkout’s local Editor setting was left untouched.

The existing “Build animated globe map selection” chat already owns the combined build/push/release. Candidate `4257a7a` includes all Peter commits through `ea37af0`, with the hosted build endpoint and guest-session/fallback adapter unchanged. The new numeric-schema guard prevents inactive numeric years using the legacy endpoint. Field Notes retains the AI-unavailable disclosure and labels local summaries without changing accepted receipts.

Independent checks: **50 chapter assertions pass** on the refreshed main base, with no provider call. Public backend health reports ready/configured Gemini; `/chapter` CORS preflight returns 204 for the production game origin and allows Content-Type, Authorization and X-Unity-Version. [Exact receipt](compatibility.json). These are source and HTTP compatibility checks, not final combined native/browser acceptance.

Production before this request was `dpl_3wg8P1goPxRPsVREjysW3iHh1eaZ`. The final combined artifact and public browser evidence belong in [the release owner’s record](../myterrain-combined-release-oct25/README.md). No second build or competing deployment was started by this review.

## Independent production verification

Deployment `dpl_8oY5izzcxAmi7nGXD86LXFiuLuNc` is Ready on the existing production alias. At 12:40 local on October 25, all **21 public files / 57,113,354 bytes** matched the frozen `19e7eae` export byte for byte, including `player.js`, both licenses and `release.json`. All files returned anonymous HTTP 200 with revalidation; Brotli payloads decoded and the WASM header passed. The separate Vercel configuration makes 22 local export files. [Complete independent hash receipt](public-files.json).

Source `19e7eae` retains Peter’s commits through `ea37af0`; the hosted build processor and chapter session are unchanged. The release owner records actual Chrome checks and remaining visual/performance limits in its linked release evidence. This verification does not claim activation of the experimental numeric simulation.
