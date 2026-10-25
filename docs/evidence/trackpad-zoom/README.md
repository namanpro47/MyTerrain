# Game view scroll zoom correction — October 25, 2026

**Integration update:** Peter authorized coordination with the parcel chat. Both real source readers are now corrected in the combined parcel/HUD candidate: WorldAtlas from `4c54ece`, plus the applied `landscape.patch`. Unity's bundled Roslyn compiles the actual combined runtime sources using this worktree's current `2000b0aE` Editor response file, with no errors and one existing obsolete-object-lookup warning. The older `1900` response is not the combined verification input. `combined-compile.json` records the exact source hashes. The WebGL input branches are excluded from these changes; the existing browser preview remains applicable to the map feature. Physical trackpad feel and updating Peter's separate open checkout are not claimed.

Base: refreshed `origin/main` at `13a15a43c43bab9e0eb6a2a90bded4bb4d7aba0c`.
User reports two-finger trackpad zoom barely changing the running game's view.
The open Editor is Unity 6000.6.6f1, project `C:/Users/Peter/Terra-Unity`.

## Cause and bounded correction

`Packages/manifest.json` selects Input System 1.20.0. Its installed
`InputSystem/Runtime/InputSettings.cs` documents normalized scroll deltas as
the default, with the legacy Windows 120-unit range available only through
`KeepPlatformSpecificInputRange`. No project InputSettings asset or runtime
override was found. Both native/Editor input readers nevertheless divide by
120, attenuating the normalized input a second time.

Pass the negative normalized Y delta directly to the existing `Scroll` and
`Zoom` methods. Preserve fractional trackpad deltas, direction, camera bounds,
smoothing and modal guards. At landscape zoom 105, one normalized outward
unit should change the target by 7.875, versus the old 0.065625. A fractional
0.1-unit delta should change it by 0.7875, versus 0.0065625.

The original preparation candidate corrected WorldAtlas and supplied the
landscape change as `landscape.patch`. Its shared-file handoff is now complete
as described in the integration update above.

The browser has a separate wheel bridge in
`Assets/WebGLTemplates/Terra/player.js`; the changed input readers are gated
by `!UNITY_WEBGL || UNITY_EDITOR`. No browser wheel conversion change is
needed for this Editor report. Design reference: AX-01 (accessible input).

## Verification and limits

- `git diff --check`: pass.
- `git apply --check docs/evidence/trackpad-zoom/landscape.patch`: pass.
- Runtime C# compilation: pass using Unity's bundled .NET SDK/Roslyn and the
  open project's generated response-file references. All Assets sources
  resolve into this isolated worktree; TerraMain.cs resolves to a temporary
  copy under `Logs/TrackpadZoom` containing the prepared correction.
  One existing obsolete FindFirstObjectByType warning; no errors.
- No Editor session, synthetic scroll event, physical trackpad test, WebGL
  build, browser check or deployment was performed. The currently open
  project is unchanged and still has its original slow zoom.

After ownership is resolved, apply the landscape patch, repeat compilation
against the actual candidate, and integrate through the repository's queue.
Physical two-finger scroll feel still needs Peter's test in the updated
Editor project. Do not present the temporary compile as a live input test.
