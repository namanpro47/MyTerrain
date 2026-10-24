# Terra

A browser-based rural land-use game in development. Phase 1 provides a synthetic interaction fixture and frozen v1 handoff contracts.

The intended product and roadmap are documented in [TERRA_PROJECT_SPEC.md](TERRA_PROJECT_SPEC.md); this setup does not implement those gameplay features.

## Project vision — start here

[TERRA_PROJECT_SPEC.md](TERRA_PROJECT_SPEC.md) is the project's north star and primary reference for product vision, gameplay, scope, priorities, and design constraints. Consult it when planning features or making implementation decisions, and update it when the team explicitly agrees to change the vision.

**Current focus: one excellent rural location across the full game scope.** Coon Valley / Brye Lane, Wisconsin is selected; refine its gameplay, simulation, UI/UX, learning, and replay together. Additional locations, city levels, and live world generation are deferred until this experience is complete and a separate expansion decision is made.

The spec describes the intended product; it is not a record of completed features. This README covers running and building the current project, and [SETUP.md](SETUP.md) records setup and verification status.

Follow [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) for the ordered build phases, design standards, and goal-loop prompts. [Parallel work packages](docs/PARALLEL_WORK.md) define handoffs for another engineer after phases 0–1; [PHASE_STATUS.md](docs/PHASE_STATUS.md) records implementation evidence and remaining gates.

## Open and run

1. Open **Unity Hub → Projects → Terra**.
2. Use **Unity 6.6, 6000.6.6f1, Apple Silicon**. Keep this version aligned with the team.
3. Run `python3 scripts/prepare-world.py` from this folder, then open `Assets/Terra/Scenes/TerraMain.unity` and press **Play**.
4. Double-click a C# script to edit it in VS Code. Use **Attach to Unity** (F5) in VS Code to debug the running editor.

The project uses URP, Input System, uGUI and TextMeshPro. TerraMain loads the tiny JSON fixture: hover a site, click to place, Z to undo; 1/2/3 choose tools, Tab cycles sites and Enter commits. Drag orbits, scroll zooms, R resets the camera. The original `SampleScene` remains a separate Phase 0 smoke fixture. Real geography and runoff simulation are not implemented yet.

The verified browser fixture is at [terra-browser.vercel.app](https://terra-browser.vercel.app/). See [deployment instructions](docs/deployment.md), [Phase 1 evidence](docs/PHASE_STATUS.md), and the [runnable handoff](docs/PARALLEL_WORK.md). Phase 1 is complete; Phase 2 has not started.

## Build for the browser

Install **Web Build Support** for the same editor version, then choose **Terra → Build Web**. The output goes to `Builds/Web/`. Alternatively, use **File → Build Profiles → Web → Build And Run**.

For a local preview of an existing build, run this from the project folder:

```sh
python3 -m http.server 8080 --bind 127.0.0.1 --directory Builds/Web
```

Open [localhost:8080](http://localhost:8080). Use a web server rather than opening `index.html` directly. Builds use gzip with Unity's decompression fallback for simple static hosting.

For a repeatable command-line build, close the editor first:

```sh
"/Applications/Unity/Hub/Editor/6000.6.6f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -quit -projectPath "$PWD" -buildTarget WebGL \
  -executeMethod TerraBuild.BuildWeb -logFile /tmp/terra-web-build.log
```

## Source control

Commit `Assets/` including every `.meta` file, `Packages/`, and `ProjectSettings/`. Unity asset serialization is text-based, and meta files are visible. The `.gitignore` excludes generated caches, local settings, and builds.

Move or rename assets inside Unity so their `.meta` files follow them. Avoid upgrading Unity or packages independently of teammates.

See [SETUP.md](SETUP.md) for installation and verification status.

## Art and development toolkit

[Art contract and isolated preview](docs/art-direction.md): included DOTween, prepared nature prefabs, sound cues, licenses and independent gallery. Machine-specific toolkit setup is recorded in SETUP.md.
