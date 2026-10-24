# Terra — Unity setup

Setup completed on October 24, 2026, on an Apple M1 Max Mac running macOS 26.5.2.

## Installed tools

| Tool | Installed configuration |
| --- | --- |
| Unity Hub | 3.22.4, signed in with an active Personal license |
| Unity Editor | **6000.6.6f1**, native Apple Silicon ARM64, changeset `d20c9ac777da` |
| Build modules | Mac standalone support and **Web Build Support** |
| VS Code | Existing ARM64 installation at `/Applications/Visual Studio Code 2.app` |
| VS Code extensions | Unity 1.3.2, C# 2.160.4, C# Dev Kit 3.40.215, .NET Install Tool 3.2.0 |
| .NET SDK | 10.0.402 ARM64 in `~/.dotnet`; `dotnet` available on PATH |
| Unity CLI | 1.0.0-beta.14; `unity` available in interactive zsh |

Unity is installed at `/Applications/Unity/Hub/Editor/6000.6.6f1/Unity.app`. Downloads came from official vendor sources; Unity installer integrity and Apple signing/notarization were checked. Unity terms were accepted with the user's explicit approval.

**6000.6.6f1 is a reasonable choice for this project**, especially when teammates use the same version. Keep the exact editor and package versions aligned across the team. This setup pins the requested version rather than automatically upgrading it.

## Project configuration

- Project folder: `/Users/naman/Documents/GitHub/Terra`, registered in Unity Hub as **Terra**.
- Official Universal 3D template with URP 17.6.0 and Input System 1.20.0.
- Web is the active build target. `Assets/Terra/Scenes/TerraMain.unity` is enabled for the current fixture export; the initial `SampleScene` is preserved.
- VS Code is configured as the external script editor; Unity's Visual Studio Editor package 2.0.28 supplies the supported integration.
- Text asset serialization and visible meta files are enabled.
- A local Git repository and Unity `.gitignore` are initialized. Generated caches, IDE project files, local settings, and builds are excluded.
- **Terra → Build Web** produces `Builds/Web` with gzip compression and decompression fallback for simple static servers.

This is a working development environment and clean starter scene. Land-use gameplay has not been implemented by this setup. The canonical vision remains in [TERRA_PROJECT_SPEC.md](TERRA_PROJECT_SPEC.md).

## Verification

- Initial project import and C# compilation completed successfully in batch mode (exit 0).
- The scene opened in Unity 6000.6.6f1 and entered Play mode. The Editor Console showed **0 errors and 0 warnings**.
- A real Web build completed successfully (exit 0), reporting **12,510,477 bytes**.
- The browser build loaded in Chrome through a local HTTP server and rendered the sample scene using WebGL 2. Unity, Input System, and physics initialization were observed.
- VS Code loaded the generated solution and C# project with no reported problems. **Attach to Unity** was tested successfully: the debugger connected and displayed running Unity threads, then disconnected normally.

The browser logs include unsupported/stripped optional URP post-processing shader messages, including depth of field, Panini projection, and spatial upsampling. They did not prevent the starter scene from rendering. Revisit these effects when selecting the game's actual browser graphics settings.

Local verification artifacts are in `Logs/Setup/` (Git-ignored). Build output is in `Builds/Web/` (also Git-ignored). See [README.md](README.md) for opening the project, debugging, rebuilding, and restarting the local preview server.

## References

- [Unity 6000.6.6f1 release](https://unity.com/releases/editor/whats-new/6000.6.6f1)
- [Unity release support](https://unity.com/releases/unity-6/support)
- [Official VS Code Unity setup](https://code.visualstudio.com/docs/other/unity)
- [Project FigJam](https://www.figma.com/board/YIf37AibaJxqrrGPYpyl2T/Hack-for-Social-Impact-2026-UN?node-id=11-329)

## Hackathon toolkit additions

The official Unity agent plugin 0.1.8-beta and Pipeline package 0.9.1-exp.1 are installed. Direct CLI access to the running Editor works, and a local Unity MCP entry is configured for Codex. The plugin supplies Unity skills on the next turn.

DOTween Free 1.3.045 is configured, along with 24 Kenney nature models, 24 prepared prefabs, shared URP materials, and 10 interface sounds. Complete asset archives are retained in `~/Downloads/TerraToolkit`. The isolated `ToolkitPreview` scene completed all 24 animations in Editor Play mode with no Console errors.

The portable [art contract](docs/art-direction.md) records asset locations, conventions, licenses and the separate preview. The pre-existing local `docs/HACKATHON_TOOLKIT.md` has additional machine-specific setup notes and is not part of this phase checkpoint.

The toolkit Web build succeeded (12,913,018 bytes; zero build errors/warnings). Chrome verified the gallery, animation replay (24/24 complete), and a sound playback request after clicking Play. There were no observed browser errors; the optional URP spatial-upscaling warning remains. Evidence: `Logs/Setup/toolkit-verification.json` and `Logs/Setup/toolkit-browser.png`.

## Phase 0 browser gate — October 24, 2026

The current sample scene exports a controllable technical fixture. The final Web build succeeded in 65.68 seconds, reporting **12,777,098 bytes**, zero build errors, and one nonblocking CS0414 warning for the Editor-only pan field in the Web compilation. Shader/material and primitive-collider stripping errors found in initial browser runs were fixed with an explicit Resources material and linker preservation rules.

[The public build](https://terra-browser.vercel.app/) passed Chrome **155.0.8059.85** and Safari **26.5.2** checks, including anonymous Safari Private Browsing, orbit/wheel and keyboard zoom, reset, focus recovery, resize, and reload. No browser Console errors remain. Optional URP shader messages persist; Safari also logs the non-portable `WEBGL_polygon_mode` extension warning. Firefox was not installed and was not tested.

Reproduction, serving configuration, and deployment identity: [docs/deployment.md](docs/deployment.md). Committed captures, request hashes, browser logs, and acceptance evidence: [docs/PHASE_STATUS.md](docs/PHASE_STATUS.md). Setup and toolkit checks were reused. This is the historical Phase 0 checkpoint; subsequent work is recorded below.

## Phase 1 foundation — October 24, 2026

The pinned toolchain and existing assets were reused. TMP Essentials and its Liberation Sans license are included, and `com.unity.modules.unitywebrequest@1.0.0` is enabled for URL-based pack loading. Run `python3 scripts/prepare-world.py` before first Editor Play in a fresh checkout. The Web build packages automatically and uses `Assets/WebGLTemplates/Terra/index.html`.

TerraMain Play mode, 27 serializer/accounting checks, the independent simulation I/O example, and the separate 24-specimen/24-animation toolkit preview passed. The final export is **13,678,532 bytes**, **60.46 seconds**, **0 errors / 4 warnings** (one unused Phase0 field and three TMP compile-splitting notices). Public Chrome and private Safari render and interact successfully; exact captures, warnings and coverage limits are in [the Phase 1 ledger](docs/PHASE_STATUS.md). No toolchain reinstall or second Editor was required. Phase 2 is not started.
