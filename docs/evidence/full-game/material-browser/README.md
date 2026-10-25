# Combined grass/material Chrome checkpoint

October 25, 2026. Frozen **`16691f28cbab07e673da81d617d1e12729faff88`** combines visual `48154c4`, material sharing `7b28274`, and the chapter teardown fix now on main as `b4bc7d8`. This is an isolated engineering/visual candidate, **not a Phase 2A pass or public release**. Patterns TN-02/05/09/12 and AX-01 remain applicable; material sharing does not resolve the known crop, ground and treatment-consistency gaps.

## Export and inputs

Unity6000.6.6f1 / CLI1.0.0-beta.14 WebGL/Brotli export succeeded from clean tracked source (`build.json`, `provenance.json`). `Builds/Verified/16691f2` preserves the output. `identity.json` pins **31 runtime files /46,414,718 bytes**, and `web-assets.json` anonymously verifies every served byte and decodes the Unity payloads. This total is an upper bound on initial body transfer and is below50MB; it is not total memory or an internet download-speed measurement.

`ignored-assets.json` audits32 ignored files under Assets. There is no ignored C#, shader, DLL, prefab, asset or scene file. All12 generated world-pack JSON files match their committed `worlds/` sources byte-for-byte; remaining items are their metadata and standard Performance Test package metadata. This differs from the visual owner's baseline checkout, which reported an ignored wind script; the two builds are not a controlled single-variable benchmark.

## Actual Chrome performance

All three performance samples ran in the first tab at **1280×720**, normal text/full motion. Telemetry confirms visible/focused start and end, no visibility changes or blur events, and zero Unity unfocused frames. No error-level console messages were recorded.

| Route | Seconds | Frames | Average FPS | p50 / p95 / p99 ms | Max ms | >33.34 / >200ms |
|---|---:|---:|---:|---:|---:|---:|
| Opening camera |30.026|1462|48.69|18 /35 /52|77|116 /0|
| Restoration editing |30.021|1054|35.11|25 /46 /69|84|311 /0|
| Year10 camera |30.012|1389|46.28|19 /34 /53|76|80 /0|

**All three fail the stable30 p95≤33.3ms limit.** Sharing material objects has not established a frame-time improvement. The two camera routes use R reset followed by F9 and repeated Right/Down/Left/Up holds of1.2seconds, two three-cycle groups with an observation between. Tool gaps and per-frame event counts differ, so these are actual path samples, not exact deterministic benchmark replays. Submission counts at sample completion were856/991/814, not reduced merely by material reuse.

Editing includes restoration queueing, catalog undo/redo, a second restoration and camera movement. Unity reports12 preview events (max11ms CPU) and2 placements (max24.3ms CPU). DOM input→next-rAF feedback records preview maximum **39.3ms** and placement maximum **46.2ms**. These bounded input checks pass; they measure presentation opportunities, not physical display latency. Tab initially remained in catalog focus; Escape and site cycling returned to the world before the second placement. Both final proposals are visible in `editing.jpg`.

## Annual flow, persistence and appearance

Actual inputs queued restoration on West field and Central field with cash still$10M and$190K reserved. Confirmed fast-forward processed the same annual transitions through Year10: **$19.66M**, meters **40.3 /58.0 /52.6**, two accepted assets and no future obligations. Reload restored Year10/Completed/revision25 with the same cash and assets. The opening comparison retained the accepted budget/meters; returning restored the accepted land. Confirmed New run returned to Year1/Planning/revision0/$10M/no assets and onboarding. `full-run.json` and the screenshots retain those checks.

Year10's diagnostic reports127 total runtime material objects,128,915,220 allocated bytes and192,610,304 reserved bytes. Later reports include114–115 material objects through comparison/replay. These are Unity counters, not a controlled total-memory reduction or browser/GPU heap accounting; the managed-memory counter varies substantially and should not be interpreted as a hardware budget proof.

Full-HUD opening, focused restoration, accepted land, comparison and replay rendered without missing-material/browser errors. The restored fields still use finer scatter than the new mapped meadow; repeated crop marks and weak transitions remain conspicuous. No new reference-calibrated visual score is assigned. Screenshots are unedited JPEG bytes returned by the browser, saved with matching extensions.

## Arrival observations and limits

| Origin | Navigation→interactive | Viewport | Limitation |
|---|---:|---|---|
|61253|17.169s|1280×720|Fresh origin; includes manual Explore delay; exceeds15s|
|61254|14.281s|1728×940|Fresh origin; viewport had reverted to native size|
|61255|20.095s|1280×720|First Unity load after atlas reload to restore viewport; atlas resources may be warm; exceeds15s|

These observations **do not pass the three fresh-arrival gate**. The current `TERRA_INTERACTIVE_MS` clock starts at navigation and includes time spent on the globe. Add explicit Explore→landscape-ready instrumentation alongside the historical metric, then repeat on the eventual final candidate with size verified on every tab. Do not silently discard these observations or relabel them as cold, matched, click-to-ready measurements.

All test tabs and local servers were closed; temporary viewport override reset. No public deployment, novice/low-end test, final rehearsal or recorded fallback is claimed. Remaining work: whole-scene quality, rendering/frame pacing, reliable final arrival measurement, exact accepted public export/run, two rehearsals and labelled actual-gameplay fallback.
