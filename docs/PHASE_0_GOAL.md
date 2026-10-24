# Phase 0 — prepared goal: browser build and deployment

**Prepared:** October 24, 2026. **State:** ready for the user's next `/goal` message; this preparation did not start a goal, rebuild Unity, or deploy anything.

Read [AGENTS.md](../AGENTS.md), [the north star](../TERRA_PROJECT_SPEC.md), [phase 0 in the implementation plan](../IMPLEMENTATION_PLAN.md), [SETUP.md](../SETUP.md), and [the evidence ledger](PHASE_STATUS.md). This brief scopes the first execution loop to the existing local setup and browser/deployment gate.

## Goal to start

```text
/goal Complete Phase 0 using docs/PHASE_0_GOAL.md. Reuse verified Unity setup work, finish the minimal interactive browser smoke scene, rebuild and deploy it, and verify the public URL in Chrome and Safari. Record reproducible build/deployment instructions and evidence. Stop when the phase-0 acceptance checklist passes; do not begin phase 1 or select/build the rural location.
```

The goal has an observable outcome and explicit completion evidence, following [OpenAI's goal guidance](https://developers.openai.com/cookbook/examples/codex/using_goals_in_codex). No token budget or recurring automation is configured by this brief.

## Evidence already available

| Item | Evidence and confidence |
|---|---|
| Unity and Web tooling | Editor 6000.6.6f1 exists. Web Build Support exists at `/Applications/Unity/Hub/Editor/6000.6.6f1/PlaybackEngines/WebGLSupport`. Setup notes record URP/Input System, license activation, and editor integration. |
| Import/configuration | `Logs/Setup/project-import.log.summary.txt` contains `TERRA_SETUP_COMPLETE`; SETUP.md records successful compilation. |
| Editor Play mode / debugging | SETUP.md records Play mode with zero Console errors/warnings and a successful VS Code attach. `Logs/Setup/unity-play-verified.png` exists. These were not repeated during goal preparation. |
| Existing Web export | `Logs/Setup/web-build.log.summary.txt` records success and 12,510,477 bytes. `Builds/Web/index.html`, loader, data, framework, and WASM files exist. |
| Export format | Preparation read the three `.unityweb` gzip streams' decoded prefixes and verified the WASM magic/version header. This is a file-format check, not proof of browser execution or a complete integrity audit. |
| Local browser run | SETUP.md records successful local Chrome/WebGL 2 rendering. Public deployment and interaction behavior are not established by that record. |
| Build command | `Assets/Editor/TerraBuild.cs` selects enabled scenes and exports `Builds/Web` using gzip and decompression fallback. `SampleScene.unity` is enabled in build settings. |
| Disk | Approximately 136 GiB available at preparation time; recheck only if needed during export. |
| Deployment access | `vercel whoami` succeeded for the current signed-in account. The CLI is installed. `.vercel/project.json` is absent in this checkout; no public URL is recorded in the project notes. This does not prove no Terra deployment exists elsewhere. |
| Source control | The project files are untracked at preparation time; preserve existing work and establish the phase-0 checkpoint deliberately. |

The setup build predates ongoing source/asset changes. Do not assume that existing output represents the current checkout. A fresh successful export after the final phase-0 changes is required.

## Remaining work, in order

1. **Reconcile the live checkout.** Inspect current project settings, scene, scripts, and any work added since this brief. Preserve useful installed assets and existing user work. Reuse the recorded setup evidence; do not reinstall or upgrade Unity, packages, IDEs, or the CLI merely because an update is advertised.
2. **Make the smoke scene meaningfully interactive.** Verify whether camera/input is already wired. If not, add the smallest clean orbit/pan/zoom or equivalent camera control with reset/help and mouse/trackpad support. Use the existing sample scene or a tiny technical fixture. Confirm visible input response, sensible bounds, UI click isolation where applicable, and no stuck input after losing focus. No rural map, placement system, branded full HUD, or gameplay is needed yet.
3. **Export current source.** Use the existing Terra build entry point and confirm the intended scene is enabled. Save work before closing an Editor for batch mode; do not launch competing Unity processes against the same project. Preserve the last successful build until its replacement succeeds. Capture the build command, final success, size, and any relevant shader warnings.
4. **Verify the new export locally over HTTP.** Check loader/data/framework/WASM requests, render and input, resize, zoom, focus recovery, and a reload. Inspect Console/network failures. Optional stripped URP shader messages already recorded in SETUP.md are not automatically blockers; disable unsupported effects if they cause actual rendering problems.
5. **Deploy the static export.** Prefer the existing authenticated Vercel route. Inspect Terra-specific project state before linking; reuse a clearly matching project or create a scoped Terra project if none exists. Deploy the generated Web output, not the repository root/caches/raw data. Keep deployment configuration repeatable and account credentials outside the repository. Start with the existing gzip decompression fallback; change compression/headers only in response to verified serving failures. An ordinary host URL is sufficient; no custom domain, backend, CI, live GIS service, or paid scaling setup is needed.
6. **Test the actual public URL.** A deploy command reporting success is insufficient. Open a fresh browser session and verify that a player can access the build without the developer's login. Inspect the network/Console and confirm the final deployed build loads, responds to camera/input, resizes, reloads, and recovers after focus changes. Check Chrome and Safari on this Mac, recording versions; test Firefox if readily available. If the hosting route fails, diagnose and repair the smallest configuration issue before considering another suitable static host.
7. **Record the handoff.** Create/update `docs/deployment.md` with exact build/deploy/preview steps, host/project identity, URL, serving configuration, and limitations. Update SETUP.md only where new evidence changes it, and add a phase-0 evidence entry to PHASE_STATUS.md. Record the source commit and associate it with the verified export/deployment. Review the intended files before making the local checkpoint; exclude generated builds, logs, caches, credentials, and unrelated work. Then stop.

## Acceptance checklist

- [ ] Current source exports successfully through the recorded command on the pinned Unity version.
- [ ] A small controllable 3D scene works locally over HTTP; input and resize/focus behavior have been exercised, not inferred from the presence of Input System assets.
- [ ] A real public URL serves that export and opens without the developer's authenticated session.
- [ ] Chrome and Safari load and render it and pass the core camera/input, resize, reload, and focus checks; browser versions and any differences are recorded. Firefox is optional and must be labeled tested or not tested.
- [ ] Critical loader/data/framework/WASM requests succeed with usable compression/serving behavior; no blocking Console or network errors remain.
- [ ] At least one actual browser capture and concise check results are saved with build/deployment identity. Record observed load behavior; do not apply the later rural game's polish/performance gates to this small technical fixture.
- [ ] Build and deployment can be repeated from the documented configuration, and the verified source checkpoint is recorded.
- [ ] PHASE_STATUS.md reflects the evidence, remaining nonblocking limitations, and a phase-1 handoff. No phase-1 implementation has begun.

Only mark phase 0 complete when these required checks pass. Local rendering alone, readable build files, or a successful upload alone do not satisfy the public-browser gate.

## How to iterate

Work through the smallest missing item, test it on the relevant surface, fix the concrete failure, and keep evidence current. Rebuild/retest after changes that affect the exported game or hosting configuration; avoid repeatedly running unrelated setup checks that already passed. Keep UI clean and the smoke controls understandable, but defer the full visual direction and rural location to phase 1.

If authentication, account ownership, host access, or required browser access genuinely prevents completion, identify the exact unmet check and what would unblock it. Finish independent local work and preserve the export/configuration/evidence. Do not invent a public URL, silently skip a required browser, or mark the phase complete. Follow the goal tool's lifecycle rules for any unresolved blocker; do not repeatedly retry an unchanged external condition.

This is local phase-0 work on Naman's computer. Do not delegate parallel packages, create a new chat, start an automation, select a rural AOI, build another location, or advance to phase 1 as part of this goal.
