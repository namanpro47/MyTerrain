# Terra

Unity starter project for a land-use simulation.

The intended product and roadmap are documented in [TERRA_PROJECT_SPEC.md](TERRA_PROJECT_SPEC.md); this setup does not implement those gameplay features.

## Project vision — start here

[TERRA_PROJECT_SPEC.md](TERRA_PROJECT_SPEC.md) is the project's north star and primary reference for product vision, gameplay, scope, priorities, and design constraints. Consult it when planning features or making implementation decisions, and update it when the team explicitly agrees to change the vision.

**Current focus: one excellent rural location across the full game scope.** Choose the exact site during early planning, then refine its gameplay, simulation, UI/UX, learning, and replay together. Additional locations, city levels, and live world generation are deferred until this experience is complete and a separate expansion decision is made.

The spec describes the intended product; it is not a record of completed features. This README covers running and building the current project, and [SETUP.md](SETUP.md) records setup and verification status.

Follow [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) for the ordered build phases, design standards, and goal-loop prompts. [Parallel work packages](docs/PARALLEL_WORK.md) define handoffs for another engineer after phases 0–1; [PHASE_STATUS.md](docs/PHASE_STATUS.md) records implementation evidence and remaining gates.

## Open and run

1. Open **Unity Hub → Projects → Terra**.
2. Use **Unity 6.6, 6000.6.6f1, Apple Silicon**. Keep this version aligned with the team.
3. Open `Assets/Scenes/SampleScene.unity` and press **Play**.
4. Double-click a C# script to edit it in VS Code. Use **Attach to Unity** (F5) in VS Code to debug the running editor.

The project uses the official **Universal 3D** template, URP, and the Input System. The sample scene now runs the Phase 0 camera smoke fixture: drag to orbit, Shift/middle drag to pan, scroll to zoom, and R to reset. Land-use simulation mechanics are not implemented yet.

The verified browser fixture is at [terra-browser.vercel.app](https://terra-browser.vercel.app/). See [deployment instructions](docs/deployment.md) and [Phase 0 evidence](docs/PHASE_STATUS.md). Phase 1 has not started.

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

[Hackathon toolkit guide](docs/HACKATHON_TOOLKIT.md): installed Unity agent skills/MCP, DOTween, prepared nature prefabs, sound cues, licenses, and the isolated asset preview scene.
