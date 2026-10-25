# Package C3 — restrained audio kit

**October 24, 2026 · prepared for integration, not active in the game.** This delivery is the independent audio subset of package C. C1/C2 visual work remains dependent on the Phase 2A art selection. All sounds serve the existing Meadowbrook Valley location. The current visual captures remain rejected; this kit makes no visual acceptance claim.

Five mono PCM16 WAV assets live in `Assets/Terra/Audio/ExternalKit/Clips/`. They total **2,581,526 source bytes** (2.46 MiB), with 22.05 kHz sample rate. A decoded float32 copy of all samples would occupy about 4.92 MiB, excluding browser/engine overhead and duplicate buffers; this is arithmetic, not measured Unity memory. Unity Web download size is unmeasured. The four interaction clips together are only 21,614 bytes.

| Clip | Mapping proposal | Duration | Sample peak | RMS |
|---|---|---:|---:|---:|
| `place.wav` | Exactly one successfully committed edit; all intervention families share it | 0.110 s | −14 dBFS | −31.7 dBFS |
| `undo.wav` | Successful undo or redo, once per command | 0.064 s | −18 dBFS | −40.3 dBFS |
| `cancel.wav` | Explicit cancellation of an active placement; no cue for idle Escape | 0.148 s | −20 dBFS | −32.2 dBFS |
| `blocked.wav` | Attempted unavailable edit, alongside its visible reason; debounce repeats | 0.165 s | −21 dBFS | −37.2 dBFS |
| `ambient-wind.wav` | Optional single quiet bed after the first user gesture | 57.999 s loop + 0.046 s prefix guard | −19.4 dBFS | −36.0 dBFS loop |

These are sample peaks, not loudness scores or true-peak measurements. Auditory balance and expressive fit are **pending a listener review**. In particular, the very short undo cue may need a gain adjustment after hearing it in context; the measurements alone do not establish audibility.

## Reference and behavior

[TN-07](DESIGN_REFERENCE.md#tn-07--quiet-responsiveness-and-sensory-feedback) governs restrained feedback and silence preserving meaning. TN-08 supports letting players appreciate the scene. No per-site hover sound, ecological reward fanfare, extra soundtrack, or species-specific animal call is supplied. All successful tool actions use one timbre family; cue loudness does not rank their environmental worth.

The interface derivatives come from the project's existing **Kenney Interface Sounds 1.0 (CC0)**: drop, back, close, and error. The ambient source is **Luke.RUSTLTD's wind1 (CC0)**, described by its author as synthesized in PureData. It is decorative sound design, **not a recording from the Wisconsin AOI, evidence of local weather, or a species claim**. [Exact URLs, pinned hashes, licenses and transformations](../Assets/Terra/Audio/ExternalKit/manifest.json) accompany the files. No purchases, new generated images, or unlicensed soundtrack were used. Optional credit: “Interface sounds: Kenney. Synthesized wind: Luke.RUSTLTD. CC0; edited for Terra.”

## Integration boundaries and setup

The primary engineer owns the integration. No scenes, runtime code, HUD, packages, global audio/render settings, or existing assets were modified. New `.meta` files provide stable GUIDs only; **Unity has not imported these files and no importer settings have been applied or read back**. Let Unity generate its importer state, then configure through `AudioImporter`/`SaveAndReimport` and verify the resulting Web export.

Recommended initial import settings, not applied configuration:

| Setting | Short cues | Ambient |
|---|---|---|
| Load type for Web | Decompress On Load | Compressed In Memory |
| Preload | On, for prompt command feedback | Off until sound can start; load once and await `Loaded` |
| Source sample rate | Preserve 22,050 Hz | Preserve 22,050 Hz |
| Mono / normalization | Already mono; leave Force To Mono and Normalize off to preserve mastered gain | Same |
| Compression | PCM where the target supports it; verify Web output | Platform-supported compressed format; inspect actual Web AAC output |
| Source behavior | 2D (`spatialBlend=0`), no play-on-awake, no loop, no reverb | 2D, no play-on-awake, one loop instance, 1–2 s start/stop volume fade |

[Unity 6.6's Web audio manual](https://docs.unity3d.com/6000.6/Documentation/Manual/webgl-audio.html) documents AAC import, both supported load types, and a loop-start encoding caveat. The ambient WAV contains a 1,024-sample silent prefix and a RIFF `smpl` forward infinite loop spanning frames 1,024 through 1,279,899 inclusive. Its body crossfades the source tail/head over two seconds. The source-level seam is within ordinary adjacent-sample variation. **This does not prove an AAC-exported loop is glitch-free.** Check actual Chrome playback across at least two cycles after import. Do not assume native streaming behavior or choose Streaming as the Web strategy.

The current `TerraMain` source owns one placement AudioSource at volume 0.3 and checks `hud.Muted` only when playing a placement. Simply swapping in these attenuated files at that gain can make them too quiet, and a new continuous source must explicitly react to mute. Start an isolated mix audition at unity gain for these mastered clips, then tune once in context. Never double-play the old placement cue with the new one. The same sound setting must stop/silence the ambient source and suppress future one-shots; mute during load must prevent delayed playback. Browser tab/background pause handling should also stop or suspend the bed. These are adapter requirements, not implemented behavior.

## Verification and audition

- Run `python3 tests/audio-kit/verify_kit.py`. The standard-library audit checks source/derivative hashes, CC0 records, 22.05 kHz mono PCM16 format, duration, source budget, cue headroom and faded endpoints, loop guard/range/boundary statistics, and unique project-wide GUIDs.
- [Recorded audit](evidence/package-c3/audio-audit.json) and [delivery evidence](evidence/package-c3/README.md) distinguish measured source facts from untested game behavior.
- From the repository root, run `python3 -m http.server 8766 --bind 127.0.0.1` and open [the isolated audition](http://127.0.0.1:8766/docs/evidence/package-c3/audition.html) in Chrome. Its silent decode check, individual cue buttons, 16-second interaction sequence, loop seam button, and Stop button are independent of the game. The native-audio preview applies loop markers explicitly; it is not a Unity/WebGL build test.
- Reproduction: install `numpy` and `soundfile` into an isolated environment; download the pinned wind source from the manifest outside `Assets`; run `python3 tests/audio-kit/build_kit.py --wind-source /path/to/wind1.wav`. The builder rejects source-hash drift and recreates only this kit's assets/manifest/identity metadata. Existing Kenney originals stay unchanged. Converting their existing lossy Ogg source to WAV does not restore lost fidelity; it avoids another source-compression generation while mastering.

Remaining acceptance: listener review of candidates and the resulting sequence; Unity import/readback; actual Chrome game playback including user-gesture unlock, first cue latency, pause/mute/unmute, two ambient cycles, rapid edits/undo and audible balance against the scene. No phase gate passes from this source-only delivery.
