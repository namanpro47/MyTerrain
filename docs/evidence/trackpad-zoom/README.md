# Game view scroll zoom correction — October 25, 2026

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

The WorldAtlas reader is corrected in this candidate. The exact landscape
change is prepared as `landscape.patch`: `git apply --check` passes, but it
has not been applied to TerraMain.cs because the active
`parcels-final-20261025` task claims that file. The board rejected the overlap.
User permission to message its owning chat has been requested.

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
