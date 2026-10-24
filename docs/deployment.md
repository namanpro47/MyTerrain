# Phase 0 browser deployment

This is a technical camera fixture, not the rural game. The pinned editor is Unity **6000.6.6f1** with Web Build Support. The enabled scene is `Assets/Scenes/SampleScene.unity`; `Phase0Smoke` adds only a platform and three primitives at runtime. Existing toolkit assets remain available but its preview scene is not included in this export. The Web wrapper forwards pointer/wheel gestures to the Unity camera; Unity Input System handles keyboard controls. A Resources material and `Assets/Terra/link.xml` preserve runtime primitive dependencies against build stripping.

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

Builds stage in `Builds/Staging/Web`. Only a successful build replaces `Builds/Web`; its predecessor remains at `Builds/Web.previous`. `Assets/WebGLTemplates/Phase0/index.html` is the responsive wrapper. Open <http://127.0.0.1:8080/> and exercise drag orbit, Shift/middle drag pan, scroll zoom, arrow-key orbit, +/− zoom, R/button reset, resizing, focus loss/recovery, and reload. Bounds: pitch 15–75°, distance 8–24, target X/Z ±2.

## Publish

Public production URL: **https://terra-browser.vercel.app/**. Share this production alias. Vercel's immutable deployment URL redirects anonymous visitors to its authentication flow; it is recorded only for deployment identity. The production alias was verified without authentication. Check the evidence ledger for the deployment ID and source SHA.

```sh
sh scripts/deploy-web.sh
python3 scripts/verify-web.py https://terra-browser.vercel.app
```

The script links **only `Builds/Web`** to Vercel project `terra-browser` (`prj_PiYOquy8PBxGMztOM61NDOfN5aX0`), scope `namanpro47s-projects`, then creates a production deployment. Use the already authenticated Vercel CLI; credentials and `.vercel/` stay outside source control. The initial source checkpoint is local only; no Git push or CI is needed. `verify-web.py` checks anonymous HTTP 200 responses, byte equality with the export, SHA-256, gzip decoding, and the WASM magic header. It supplements, rather than replaces, Chrome/Safari interaction checks.

`scripts/vercel.json` is copied into the successful export. Framework is Other, with no host build command; output is the uploaded directory. Files use revalidation so the stable URL does not retain an incompatible loader across builds. Unity gzip decompression fallback handles the `.unityweb` payloads in JavaScript. Do not set `Content-Encoding: gzip` on files served in this fallback mode unless changing and retesting the complete serving strategy. No cross-origin isolation or backend is required for this build.

References: [Unity decompression fallback](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityeditor/playersettings/webgl/decompressionfallback), [Vercel static configuration](https://vercel.com/docs/project-configuration/vercel-json).

## Verification identity

Build, URL, source checkpoint, browser captures and test results are recorded in [PHASE_STATUS.md](PHASE_STATUS.md) after the public browser gate. Generated build products and verbose logs are Git-ignored; concise evidence and selected browser captures are committed under `docs/evidence/phase0/`.

Phase 1 is not authorized by this deployment. No rural location has been selected.
