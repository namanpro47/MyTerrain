# Terra browser deployment

The current export is the Phase 2 **real Wisconsin landscape**, labeled Meadowbrook Valley (a fictional game name). Terrain, cover and mapped roads/water come from the documented source pack; management sites and individual vegetation are hypothetical/decorative. Unity **6000.6.6f1** with Web Build Support is pinned. The enabled scene is `Assets/Terra/Scenes/TerraMain.unity`; its bootstrap loads the three real-pack JSON files through UnityWebRequest and binds landscape, input and HUD. `SampleScene` and the isolated toolkit gallery remain available but disabled in build settings. The Web wrapper forwards pointer/wheel events; Input System handles keyboard controls.

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
unity command script eval --caller plugin --skill unity-cli --detach \
  'UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes(); TerraBuild.BuildWeb();' --format json
unity job status JOB_ID --format json
```

Builds stage in `Builds/Staging/Web`. Only a successful build replaces `Builds/Web`; its predecessor remains at `Builds/Web.previous`. `Assets/WebGLTemplates/Terra/index.html` is the responsive wrapper. The build copies the canonical `worlds/meadowbrook-v1` JSON files into StreamingAssets; for Editor Play after cloning, first run `python3 scripts/prepare-world.py`. Open <http://127.0.0.1:8080/> and exercise drag orbit, Shift/middle drag pan, scroll zoom, arrow-key orbit, +/− zoom, R/button reset, resizing, focus loss/recovery, and reload. Bounds: pitch 32–72°, orthographic zoom 150–1000, target X/Z ±480/500 m. F focuses the hovered/selected site; R restores the authored valley view. Also test hover, click/Enter placement, Tab selection, Escape cancel, Z undo and modal click isolation.

## Publish

Public production URL: **https://terra-browser.vercel.app/**. Share this production alias. Vercel's immutable deployment URL redirects anonymous visitors to its authentication flow; it is recorded only for deployment identity. The production alias was verified without authentication. Check the evidence ledger for the deployment ID and source SHA.

```sh
sh scripts/deploy-web.sh
python3 scripts/verify-web.py https://terra-browser.vercel.app
```

The script links **only `Builds/Web`** to Vercel project `terra-browser` (`prj_PiYOquy8PBxGMztOM61NDOfN5aX0`), scope `namanpro47s-projects`, then creates a production deployment. Use the already authenticated Vercel CLI; credentials and `.vercel/` stay outside source control. The initial source checkpoint is local only; no Git push or CI is needed. `verify-web.py` checks anonymous HTTP 200 responses, byte equality with the export, SHA-256, gzip decoding, the WASM magic header, and every packaged StreamingAssets file. It supplements, rather than replaces, Chrome interaction checks. Chrome is the project default and sufficient for acceptance. Only test another browser when the user explicitly requests it; do not launch Safari or ask for Private Browsing unlocks during routine checks.

`scripts/vercel.json` is copied into the successful export. Framework is Other, with no host build command; output is the uploaded directory. Files use revalidation so the stable URL does not retain an incompatible loader across builds. Unity gzip decompression fallback handles the `.unityweb` payloads in JavaScript. Do not set `Content-Encoding: gzip` on files served in this fallback mode unless changing and retesting the complete serving strategy. No cross-origin isolation or backend is required for this build.

References: [Unity decompression fallback](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityeditor/playersettings/webgl/decompressionfallback), [Vercel static configuration](https://vercel.com/docs/project-configuration/vercel-json).

## Verification identity

Latest hosting refresh (October 24, 2026): deployment `dpl_FQ2Yv3sQyZ7M6VPDEfPaUDpYWVxW` publishes the completed 15:41 local Web export. [Identity](evidence/deployment-2026-10-24/identity.json), [anonymous asset checks](evidence/deployment-2026-10-24/public-assets.json), and [Chrome render/undo capture](evidence/deployment-2026-10-24/chrome-live.png). Ongoing visual-rebuild source changes are not certified as part of this export. No new project, Git integration, or Unity rebuild was needed.

Build, URL, source checkpoint, browser captures and test results are recorded in [PHASE_STATUS.md](PHASE_STATUS.md) after the public browser gate. Generated build products and verbose logs are Git-ignored; concise evidence and selected browser captures are committed under `docs/evidence/phase0/` and `docs/evidence/phase1/`; Phase 2 evidence is in `docs/evidence/phase2/`.

Original Phase 1 runtime source: `a78f49cf401a6ff602676b35f23c84fb5866ae93`; [deployment identity](evidence/phase1/identity.json). The final build started from `fbab39f` and Unity added the Input System preload; `a78f49c` records that exact resulting setting. Subsequent documentation/evidence commits do not change the export. Phase 1 is complete; publishing this fixture does not start Phase 2 or authorize real-geography ingestion.

Historical display-name update source: `25067bcf36b575d3fafc12100d179f1a8bc19404`. That export introduced Meadowbrook Valley; [build/public/copy verification](evidence/naming-2026-10-24/checks.json). No contracts or mechanics changed.

Current Phase 2 runtime source: `35fb7e8edf7c92b37acebfee5656b0bf4991544b`. Deployment `dpl_GKsY1WYbABEgANa7at2uDsq3JRs8`; [exact build, pack and browser identity](evidence/phase2/identity.json), [anonymous asset verification](evidence/phase2/public-assets.json), [acceptance audit](PHASE_2_AUDIT.md). Later documentation/evidence commits do not change this export.
