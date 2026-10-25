# MyTerrain browser deployment

The current source exports the **ten-year annual game** on the real Wisconsin landscape, labeled Meadowbrook Valley (a fictional game name). The published alias now serves the verified annual-game export identified below, including the latest integrated perspective opening. Terrain, cover and mapped roads/water come from the documented source pack; management sites and individual vegetation are hypothetical/decorative. Unity **6000.6.6f1** with Web Build Support is pinned. The enabled scene is `Assets/Terra/Scenes/TerraMain.unity`; its bootstrap loads the real-pack JSON files through UnityWebRequest and binds landscape, input and HUD. `SampleScene` and the isolated toolkit gallery remain available but disabled in build settings. The Web wrapper forwards pointer/wheel events; Input System handles keyboard controls.

## Rebuild and preview

Save open scenes, then use **Terra → Build Web** in the Editor. The existing entry point is `TerraBuild.BuildWeb`. With the Editor closed, from the repository root:

```sh
"/Applications/Unity/Hub/Editor/6000.6.6f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -quit -projectPath "$PWD" -buildTarget WebGL \
  -executeMethod TerraBuild.BuildWeb -logFile /tmp/terra-web-build.log
python3 -m http.server 8080 --bind 127.0.0.1 --directory Builds/Web
```

Do not launch a second Editor on this checkout. A live Editor can run the same entry point through the installed Pipeline package:

```sh
unity command script eval --caller plugin --skill unity-cli --project-path "$PWD" --detach \
  'UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes(); TerraBuild.BuildWeb();' --format json
unity job status JOB_ID --project-path "$PWD" --format json
```

Builds stage in `Builds/Staging/Web`. Only a successful build replaces `Builds/Web`; its predecessor remains at `Builds/Web.previous`. `Assets/WebGLTemplates/Terra/index.html` is the responsive wrapper. The build packages all three fixed rural locations into StreamingAssets; for Editor Play after cloning, first run `python3 scripts/prepare-world.py --pack all`. The browser opens the [Living Atlas](ATLAS_SELECTION.md): click a Wisconsin, Point Reyes or Suusamyr miniature or globe marker to begin the native cinematic descent immediately. Settings and credits live behind the gear. Each region has an isolated save. Open <http://127.0.0.1:8080/> and exercise drag/arrow-key pan, scroll and +/− zoom, F site/project focus, R/button reset, resizing, focus loss/recovery, and reload. The current opening is perspective at a 34° pitch; Focus uses the calibrated 42° pitch. The camera keeps a fixed −32° heading, clamps its zoom parameter to 32–1000 and target X/Z to ±480/500 m. Also test B keyboard catalog navigation, Tab map selection, click/Enter queueing, Escape cancel, Z undo/Y redo of current-year proposals, and modal click isolation. Completed-year exploration must remain read-only.

## Publish

Public production URLs: **https://www.myterrain.io/** and **https://terra-browser.vercel.app/**. Share either production alias. Vercel's immutable deployment URL redirects anonymous visitors to its authentication flow; it is recorded only for deployment identity. The production alias was verified without authentication. Check the evidence ledger for the deployment ID and source SHA.

```sh
sh scripts/deploy-web.sh Builds/Verified/<accepted-source-sha>
python3 scripts/verify-web.py https://terra-browser.vercel.app --build Builds/Verified/<accepted-source-sha>
```

Replace `<accepted-source-sha>` with the actual tested frozen export; do not run the placeholder. Use the same directory for publication and anonymous verification. Omitting the directory retains `Builds/Web` as the development default, which can change after another build.

The script links **only the selected static export directory** to Vercel project `terra-browser` (`prj_PiYOquy8PBxGMztOM61NDOfN5aX0`), scope `namanpro47s-projects`, then creates a production deployment. Use the already authenticated Vercel CLI; credentials and `.vercel/` stay outside source control. Record the source commit before building, reconcile current main, and push validated phase checkpoints normally. Never deploy the repository root or a local chapter-service environment file. `verify-web.py` checks anonymous HTTP 200 responses, byte equality with the export, SHA-256, gzip/Brotli decoding, the WASM magic header, every packaged StreamingAssets file, and the bundled atlas scripts, imagery and licenses. It supplements, rather than replaces, Chrome interaction checks. Chrome is the project default and sufficient for acceptance. Only test another browser when the user explicitly requests it; do not launch Safari or ask for Private Browsing unlocks during routine checks.

`scripts/vercel.json` is copied into the successful export. Framework is Other, with no host build command; output is the uploaded directory. Files use revalidation so the stable URL does not retain an incompatible loader across builds. Unity Brotli decompression fallback handles the `.unityweb` payloads in JavaScript. Do not set `Content-Encoding` on files served in this fallback mode unless changing and retesting the complete serving strategy. No cross-origin isolation is required. The optional hosted chapter service is configured during export; service failures produce explicitly labelled local summaries over the same saved annual inputs. See [hosted service configuration](HOSTED_GAME.md). Brotli payload verification uses Node.js’s built-in decoder (Node must be on PATH); historical gzip exports remain supported using Python’s standard library. Release exports use lossless Brotli, RuntimeSpeedLTO, a 256 MiB initial heap and geometric growth. Performance receipts and their hardware limits are in the full-game evidence.

References: [Unity decompression fallback](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityeditor/playersettings/webgl/decompressionfallback), [Vercel static configuration](https://vercel.com/docs/project-configuration/vercel-json).

## Verification identity

**Latest publication: October 25, 2026 — consolidated latest-main release.** Deployment `dpl_AuQvPLnSKxNAh9CGY9Z2ptGdDyuA` serves source `eb8cf166d76ba3436099467d31d07f96314c0cf3`, frozen at `Builds/Verified/eb8cf16` in the terra-release-consolidated worktree. It includes all completed branch checkpoints, Peter’s planning/forecast/render-preview optimization, the save bridge and exact-number restore repairs, project statistics, narration, community studies, loading artwork and both Take it local surfaces. Native2,138+16 checks and the requested short Chrome11-assertion Begin/project/EndYear/reload smoke pass. All32 public static files /61,535,463bytes match anonymously on both public aliases; live narration GET and audio POST pass. [Exact identity, browser and public receipts](evidence/release-consolidated-20261025/README.md). Unfinished regional reconstruction and the unapplied texture-compression experiment remain separate. The download target and Phase2A scenery gate remain open.

**Previous publication: October 25, 2026 — minimal native selector combined release.** Deployment `dpl_8oY5izzcxAmi7nGXD86LXFiuLuNc` serves runtime source `19e7eae835ed4c06a21d42887a9a53ed28cc4fc2`, frozen in `Builds/Verified/myterrain-19e7eae` in the native-unity-atlas worktree. All 21 public files / 57,113,354 bytes match both https://terra-browser.vercel.app and https://www.myterrain.io. Chrome verifies the minimal native globe, direct Point Reyes flight/arrival and return. Local Chrome additionally verifies all three destinations, drag/zoom, compact layout, settings and accepted Year 2 preservation after reload. The export includes all previously coordinated finished checkpoints; subsequent community/dialog/new annual requests remain separate. [Build, public bytes and screenshots](evidence/myterrain-combined-release-oct25/README.md). The <50MB transfer target and separate Phase 2A scenery gate remain open.

**Custom-domain chapter status (refreshed October 25):** `/session` and `/chapter` preflights now return204 with the exact requested origin for the Vercel alias, `www.myterrain.io` and `myterrain.io`. The earlier403 limitation is resolved at preflight level. [Current receipt](evidence/release-consolidated-20261025/chapter-origins.json). The quick release Chrome check uses labelled local summaries and does not claim a new live-AI chapter rehearsal.

**Previous publication: October 25, 2026, 08:49 local.** Deployment `dpl_DvwrgFpEd8YyYUevRgbyqLXqbR1Z` serves fresh clean-source `913c79f`; runtime/build inputs match reconciled main `7e95434`. The frozen export is `Builds/Verified/913c79f` in the publish-main-oct25 worktree. All 31 runtime files / 39,997,384 bytes match anonymous public responses. Chrome verifies the Living Atlas, Wisconsin arrival, updated HUD, placement and undo with no error-level logs. [Identity, build and verification evidence](evidence/deployment-main-2026-10-25/README.md). Phase 2A acceptance and final public rehearsals remain separate.

**Previous publication: October 24, 2026, 22:44 local.** Deployment `dpl_8q32u7n8v6PgGneUJhD1nFVzUYv7` publishes the exact verified `7e2117d` export (41,420,708 bytes). Runtime/build inputs match main `13c1942`; intervening commits contain documentation/evidence only. The production alias now includes the full annual catalog, chapters, ten-year loop and perspective opening. [Identity and browser checks](evidence/deployment-annual-2026-10-24/identity.json), [anonymous byte verification](evidence/deployment-annual-2026-10-24/public-assets.json), [Chrome saved Year 2](evidence/deployment-annual-2026-10-24/chrome-live.png), and [chapter capture](evidence/deployment-annual-2026-10-24/chrome-chapter.png). Chrome verifies manual End Year, all three offline chapter cards, next-year planning and unchanged cash after reload, with no error-level logs. This is the requested production refresh; Phase 2A acceptance and full public release rehearsals remain open.

**Previous publication: October 24, 2026, 18:12 local build.** Deployment `dpl_BgtA89SrWRVm8WXkE5Cr7dMeQ4x9` serves the freshly completed primary-checkout export, including current visual work. [Build/identity evidence](evidence/deployment-fresh-2026-10-24/identity.json), [public byte verification](evidence/deployment-fresh-2026-10-24/public-assets.json), and [Chrome capture](evidence/deployment-fresh-2026-10-24/chrome-live.png). The frozen published copy is `Builds/Publish/2026-10-24-1812`. Chrome render, placement and undo passed. This supersedes the older deployment records below; visual acceptance and future gameplay integrations remain separate gates.

Earlier hosting refresh (October 24, 2026): deployment `dpl_FQ2Yv3sQyZ7M6VPDEfPaUDpYWVxW` publishes the completed 15:41 local Web export. [Identity](evidence/deployment-2026-10-24/identity.json), [anonymous asset checks](evidence/deployment-2026-10-24/public-assets.json), and [Chrome render/undo capture](evidence/deployment-2026-10-24/chrome-live.png). Ongoing visual-rebuild source changes are not certified as part of this export. No new project, Git integration, or Unity rebuild was needed.

Build, URL, source checkpoint, browser captures and test results are recorded in [PHASE_STATUS.md](PHASE_STATUS.md) after the public browser gate. Generated build products and verbose logs are Git-ignored; concise evidence and selected browser captures are committed under `docs/evidence/phase0/` and `docs/evidence/phase1/`; Phase 2 evidence is in `docs/evidence/phase2/`.

Original Phase 1 runtime source: `a78f49cf401a6ff602676b35f23c84fb5866ae93`; [deployment identity](evidence/phase1/identity.json). The final build started from `fbab39f` and Unity added the Input System preload; `a78f49c` records that exact resulting setting. Subsequent documentation/evidence commits do not change the export. Phase 1 is complete; publishing this fixture does not start Phase 2 or authorize real-geography ingestion.

Historical display-name update source: `25067bcf36b575d3fafc12100d179f1a8bc19404`. That export introduced Meadowbrook Valley; [build/public/copy verification](evidence/naming-2026-10-24/checks.json). No contracts or mechanics changed.

Current Phase 2 runtime source: `35fb7e8edf7c92b37acebfee5656b0bf4991544b`. Deployment `dpl_GKsY1WYbABEgANa7at2uDsq3JRs8`; [exact build, pack and browser identity](evidence/phase2/identity.json), [anonymous asset verification](evidence/phase2/public-assets.json), [acceptance audit](PHASE_2_AUDIT.md). Later documentation/evidence commits do not change this export.

## Current local gameplay checkpoint

The newest combined engineering candidate is `Builds/Verified/2f8543d` in the full-implementation worktree: 31 runtime files / 46,425,461 bytes. It combines the provisional study55 scenery with shared materials and small nature-batch consolidation. [Exact build, bytes and Chrome receipts](evidence/full-game/consolidation-browser/README.md) record three stable-30 fallback paths, three fresh-entry timings, and annual/Year10/reload/comparison/replay checks. Its runtime and art are not yet the accepted main release; main contains the evidence handoff. The newest main-based control remains `Builds/Verified/7db6a5d`, with [its own Brotli receipts](evidence/full-game/web-compression/README.md). Preserve both exports without rebuilding over them.

Historical recovery exports remain `Builds/Verified/peter-7e2117d`, `Builds/Verified/e25c5d3` and `Builds/Verified/589b655`. Their camera, content and measurements apply only to their recorded identities. Final release must use a new recorded clean-source export after visual integration, verify the exact public bytes and complete the [two rehearsals](full-game-demo.md).


### Arrival timing after the atlas

`TERRA_INTERACTIVE_MS` is retained as the historical navigation-to-landscape-ready clock. It includes time spent choosing on the globe and does not establish Explore-to-play latency. `TERRA_ENTRY_TIMING` now records the entry handler through the visible, focused game, including the cinematic transition, with `fresh-player` versus `resume` labelled. Cancelled or failed entries emit no successful sample. Cross-location reload/automatic-entry timing starts in the new page’s entry handler and does not include the preceding page reload.

For the final three-arrival gate, use fresh origins/caches, verify viewport on every tab, record both clocks, and judge first playable entry with the new metric. Candidate `2f8543d` now has actual Chrome emissions on three separate fresh local origins at verified 1280×720: **12,306.3 / 12,342.1 / 12,013.1 ms**. All are labelled `fresh-player` and include the cinematic. [Versioned evidence](evidence/full-game/consolidation-browser/README.md) retains both clocks and environment limits. These are local warm-machine observations, not cold-device/network guarantees. Repeat on the final accepted export; do not relabel older frozen exports as containing this instrumentation.
