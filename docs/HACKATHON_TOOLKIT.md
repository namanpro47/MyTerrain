# Terra hackathon toolkit

Installed October 24, 2026. These additions support the rural, one-location game. The gallery is an asset compatibility check, not a geographic level or completed gameplay.

## Ready to use

| Addition | Version / contents | Location |
| --- | --- | --- |
| Official Unity agent plugin | 0.1.8-beta; Unity-authored skills | Installed and enabled in Codex |
| Unity CLI | 1.0.0-beta.14, ARM64 | `~/.unity/bin/unity` |
| Unity Pipeline | 0.9.1-exp.1, pinned in the package manifest and lockfile | `Packages/com.unity.pipeline` |
| Kenney Nature Kit | 24 selected FBX models: trees, bushes, grass, rocks, flowers, crops, fences | `Assets/ThirdParty/Kenney/NatureKit/Models` |
| Prepared nature prefabs | 24 native-scale prefabs; 14 shared URP material copies with a rural palette | `Assets/Terra/Toolkit/Prefabs` and `Materials` |
| Kenney Interface Sounds | 10 short mono clips | `Assets/ThirdParty/Kenney/InterfaceSounds/Audio` |
| DOTween Free | 1.3.045; setup complete, standard modules and module assembly definition enabled | `Assets/Plugins/Demigiant/DOTween` |

Unity remains **6000.6.6f1**. No paid subscription or asset purchase was added.

## Preview the assets

Open `Assets/Terra/Toolkit/ToolkitPreview.unity` and press Play. All 24 objects animate into view. **Replay animation** tests DOTween again; **Play** cycles through the ten sounds after each click. Showcase objects are normalized for display, while reusable prefabs retain the source model scale. Choose the final game scale after selecting the rural location.

The isolated Web preview is built to `Builds/ToolkitCheck`. To serve it:

```sh
python3 -m http.server 8081 --bind 127.0.0.1 --directory Builds/ToolkitCheck
```

Open http://127.0.0.1:8081. This is separate from the game's `Builds/Web` output. The main build scene list is not replaced with the gallery.

To rebuild only the gallery while the Editor is open and stopped:

```sh
unity command build run --caller plugin --skill unity-cli \
  --project-path /Users/naman/Documents/GitHub/Terra -- \
  --target WebGL --outputPath Builds/ToolkitCheck \
  --scenes '["Assets/Terra/Toolkit/ToolkitPreview.unity"]' --confirm true
```

This queues an asynchronous build; `unity command build status --caller plugin --skill unity-cli` reports its result when run from the project folder.

## Use the tools

- Drag a prepared prefab from `Assets/Terra/Toolkit/Prefabs` into a scene. Shared materials can be recolored together. Original Kenney FBX files are retained; URP mappings are in their `.meta` files.
- Use `using DG.Tweening;`, then, for example, `transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);`. Scripts in custom assembly definitions that use extension modules should reference `DOTween.Modules`.
- Sound filenames are preserved. Suggested starting roles: `select_001` for tools, `drop_001` for placement, `back_001` for undo, `error_001` for invalid placement, and `confirmation_001` for completion. Tune volume and select the final cues by listening in context.
- Full, unmodified downloads are in `/Users/naman/Downloads/TerraToolkit`. Add further assets only as needed. Alternate formats and thousands of preview images were kept out of Unity's import database.

## Agent connection

The official plugin supplies the Unity skills on the next Codex turn. Its installed path is `~/.codex/plugins/cache/unity-agent-plugin/unity/0.1.8-beta/skills`.

The CLI is ready now and targets this project:

```sh
unity status --project-path /Users/naman/Documents/GitHub/Terra --json
unity command script eval 'return UnityEngine.Application.unityVersion;' \
  --caller plugin --skill unity-cli \
  --project-path /Users/naman/Documents/GitHub/Terra --json
```

Codex also has an enabled `unity` MCP entry pinned to this project. It launches the same official CLI locally. The MCP tool list is limited to `eval`, `eval_file`, `recompile`, `recompile_status`, and `screenshot`: the current server's unfiltered catalog repeats several tool names across command groups. This avoids ambiguous MCP names; the full command set remains available through the CLI. Existing sandbox/network settings were not changed. A running Editor is required. If an existing chat has not refreshed its MCP list, use the CLI or restart the chat/session.

`scripts/prepare-toolkit.cs` records how the gallery and prefabs were generated through the live Editor API. It deliberately refuses to overwrite an existing gallery. It is setup evidence, not a build-time dependency.

## Sources and licenses

- [Kenney Nature Kit](https://kenney.nl/assets/nature-kit) and [Interface Sounds](https://kenney.nl/assets/interface-sounds): CC0. Original license files are included alongside each import. The Nature archive's license identifies the kit as 2.1.
- [DOTween](https://dotween.demigiant.com/download.php): Free edition, Demigiant's DOTween license. Its original readme and a saved license copy are included. Vendor code was not edited; its own setup generated the settings and module assembly definition.
- [Official Unity plugin](https://github.com/Unity-Technologies/unity-agent-plugin): Unity Companion License; distributed by Unity Technologies.
- Exact download URLs, archive SHA-256 hashes, and selected filenames are recorded in `toolkit-sources.json` beside this document.

Keep all asset `.meta` files, licenses, package locks, and generated prefabs in source control. Builds, logs, and local caches remain ignored. Geography tools/data are deferred until the rural site and source formats are chosen.

## Verification

The initial Editor check compiled with zero errors/warnings. The preview completed all 24 DOTween animations while frames were advancing, with no Console errors. MCP initialization and the filtered tool catalog were verified. The Web build succeeded at 12,913,018 bytes with zero build errors/warnings. Chrome rendered the models, completed all 24 animations again after Replay, and logged a sound playback request from the Play button. No browser errors were observed; an optional URP spatial-upscaling shader warning remains and does not prevent rendering. Evidence is in `Logs/Setup/toolkit-build.json`, `toolkit-mcp.json`, `toolkit-verification.json`, and `toolkit-browser.png`.
