# Silent chapter teardown and retry preservation

October 25, 2026. Reproduced stack context is preserved in [study54’s original console receipt](reported-stack.json): `AnnualChapterSession.OnDisable` called the public `StopRequest`, publishing into TerraMain while its HUD RectTransform had already been destroyed. This is independent of grass appearance.

The session now separates transport cleanup from notification. Explicit player cancellation still publishes; disabling or destroying the component silently clears its transport and finishing state. It preserves the town's frozen request, ledger and accepted history so re-enabling/reloading can retry the same year.

Source base `d01846f` integrates main `8530aaf`, grass checkpoint `48154c4`, and material sharing `7b28274`. Unity6000.6.6f1 / CLI1.0.0-beta.14, isolated initialized Play session. No scene saved.

- `compile.json`: zero errors/warnings.
- `lifecycle.cs` / `.json`: **13 assertions**, using a temporary loopback listener that leaves a real UnityWebRequest pending. Component disable and GameObject deactivation release transport without callbacks; exact pending state and budget survive; re-enable/offline retry accepts once; duplicate resolution does not charge twice; explicit StopRequest still publishes; destruction emits no callback; live game remains unchanged. Listener and fixture are cleaned in `finally`. No external service was contacted and no fixture save was written.
- `model.json`: **16** existing chapter-model assertions.
- `after-stop-console.json`: no errors after stopping the actual TerraMain Play session; the previously reported destroyed-HUD callback did not recur. Original live state was Year1/Planning/revision0/$10M/no queue/assets.

The first test-script draft referenced a nonexistent autoSave member on AnnualTownSession. Compilation rejected it before execution; that assignment was removed after inspecting the actual session, which persists only through explicit Save. The chapter fixture uses autoSave=false. The successful receipt above is the executed test.

This closes the observed shutdown callback bug, not the visual quality or final browser/release gates. No network-provider acceptance or user research is claimed.
