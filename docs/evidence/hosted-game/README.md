# Hosted chapter wiring evidence

Backend runtime commit: `448a75573d2d984bfd1614af41d35ff20df08aec` in the separate private `lakys-openclaw/terra-backend` repo. Vercel confirms the Git link to that repo, production branch `main`, and a Ready production deployment from that exact commit at `terra-chapters.vercel.app`. GitHub Actions passed all 29 Node tests for both feature and main pushes.

Public HTTP checks used no Vercel bypass: health 200, guest issuance 200, CORS preflight 204, fresh genuine Gemini chapter 200, missing token 401 and disallowed origin 403. The new chapter consumed one provider attempt; its exact cached retry consumed zero. The retained protected preview still redirects (302). No error-level production runtime records were returned during the 15-minute post-release query. Exact non-secret receipt metadata is in `backend-production.json`.

The actual public Gemini receipt was checked by the C# adapter against the original Unity request and exact engine consequences. Standalone C# chapter checks passed 46 assertions, with no provider call. A native Unity batch run passed 10 fallback assertions plus 16 existing chapter assertions: cash and history match the detached engine result, repeated resolution does not advance again, fallback disclosure survives save/reload, and disabling fallback leaves the same year pending. Transport failures in this batch are injected; it does not claim live Unity HTTP coverage.

`HostedChapterBuild` now configures the temporary WebGL build scene, avoiding changes to another task's currently claimed `TerraMain.Chapters.cs` or editable scene assets. Additional export configuration checks and the WebGL build are recorded separately once run. No final game artifact is claimed by the earlier checks.

Design reference: TN-06/08, CS-01. Story labels explicitly distinguish AI prose from a local text template using saved game results. The financial/rule engine and accepted receipt/save formats remain unchanged.

The personal CLI has no access to Naman's Vercel scope (`projects ls --scope namanpro47s-projects` reports scope unavailable). A Git push to Naman's game repo is distinct from publishing its Unity WebGL export; neither the healthy backend nor these checks prove the currently public game uses the new adapter.

## Large map narration

The parcel owner currently has 312/340 plots on the regional maps. The v1 backend intentionally accepts at most 64 facts. `NarrationFacts` retains all seven full-map aggregates, then selects up to 57 unique site details, prioritizing current submitted choices and the most recent prior player projects. It explicitly discloses the subset in `limits`; aggregate totals and engine replay still include every site. Four standalone assertions cover a synthetic 340-site fact list, exact required aggregates, priority, distinct count and no mutation of original facts. Small existing request fixtures remain byte-compatible.

## Final export and CORS follow-up

The backend's current production commit is `43e69e084d9fb963aff47e42503832f22735f3aa`; it adds Unity's automatic `X-Unity-Version` request header to the explicit CORS allowance. All 30 backend tests passed locally and in GitHub Actions. Both public `/session` and `/chapter` preflights return 204 with Content-Type, Authorization and X-Unity-Version allowed (`unity-cors.json`). Origin/token checks and spending limits are unchanged from the initial verified production release.

Game source `f701ebf8fce52f4724003c3cd3f433b3cfb15a99` reconciles current remote main `13a15a4` with this adapter. The final Unity batch run passed **14 hosted assertions + 16 existing chapter assertions**, then successfully produced the optimized Brotli WebGL export. The latest standalone C# suite passed **50 assertions**. `webgl-export.json` records all 20 runtime files / **40,087,410 bytes** with SHA-256 hashes. The production endpoint was found in the actual serialized asset bytes after Brotli + UnityFS/LZ4 unpacking, not merely in an Editor log. Unity's two automatic orphan debug-metadata deletions were restored; no unrelated import/settings changes are committed.

Local frozen export: `Builds/Web`. Shareable ZIP: `Builds/Terra-hosted-game-f701ebf.zip` (root contains index.html and vercel.json). Publish it in **Naman's existing terra-browser project**, not Peter's backend project. The personal CLI cannot access Naman's scope; there is no claim this game export has been publicly deployed or interactively accepted. The saved source scene retains its developer endpoint; the WebGL build processor supplies the hosted endpoint.
