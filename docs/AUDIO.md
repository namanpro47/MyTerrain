# MyTerrain sound design

The October 25 sound pass covers the existing playable game from globe selection through the ten-year future. Its reference is **TN-07** (quiet responsiveness), **TN-08** (space to appreciate the result) and **AX-01** (no information conveyed only through sound). This is an audio delivery, not acceptance of the separate Phase 2A scenery gate.

## Listening direction

Two free, instrumental piano pieces alternate with six-second equal-power transitions. The first is Indieteur's *Mystical Piano*, trimmed at the creator's specified 95-second boundary; the second is *Vaporware* by The Cynic Project. Music continues through menus, atlas returns and region changes. Reports lower the bed, and important cues briefly duck the score. No score increase gets a moral victory fanfare: every completed decade receives the same reflective ending.

A quiet wind bed supports the landscape. Birds fade in for brief passages with long spaces between them. Wisconsin creek sound follows the camera target's distance to mapped water; it becomes quieter in overview. The mountain setting emphasizes wind and reduces birds. Other regions do not acquire fictional mapped streams. The atlas has a much lower wind bed and no birds. Nature assets are decorative: Thimras recorded in Adelaide, Australia; Luke.RUSTLTD's wind was synthesized. Neither establishes local species, current weather or geographic evidence.

## Coverage

| Moment | Sound |
|---|---|
| Atlas location choice, travel, arrival | Soft selection interval, rising travel gesture, open arrival chord |
| UI click, keyboard focus, tool choice, panel open/close | Short, restrained clicks and mallet intervals; priority coalescing prevents duplicate UI + action sounds |
| Traditional farming | Soil/grass press and a soft compacting impact |
| Rotation, living margin, cover crop | Different staggered grass and light wood gestures |
| Meadow restoration | Vegetation rustle with a soft grounded impact |
| Swale | Excavation, vegetation and a short water wash |
| Park | Light timber and vegetation |
| Low / high density housing | Timber/plank pair versus heavier masonry/timber/metal |
| Warehouse | Plank and structural metal |
| Data center | Small metal panels and a brief electronic interval |
| Village center | Timber rhythm with a small glass accent |
| Nonrenewable power | Heavy mechanical metal |
| Solar | Light panel/glass with a clean interval |
| Wind | Structural metal with an air wash |
| Stewardship agreement | Quiet vegetation/wood; no culturally stereotyped cue |
| Wastewater upgrade | Mechanical fitting plus water |
| Move, remove, clear, undo, redo, cancel | Separate reversible-edit gestures; payment is never implied |
| Invalid placement, load/save/request failure | Soft, rate-limited blocked cue |
| Submit year, accepted report, continue | Distinct commitment, report and next-year gestures |
| Disruption, arrears or suspended service in accepted report | Lower attention chord based on actual accepted state |
| Fast-forward | Intermediate report cues suppressed; one final decade cue |
| Replay | New-run gesture after the run actually changes |
| Focus/reset camera, comparison/ledger/help/learning controls | Camera cue or common UI feedback; no continuous pan/zoom noise |

All 17 placement recipes have three alternate material recordings, cycled without adjacent repeats and with very small pitch variation. Soft stereo positioning follows the placed site. Field hover and continuous camera movement are intentionally quiet, as required by TN-07.

## Controls and browser behavior

**U** opens Sound & music from either the atlas or landscape; both also have a visible Sound settings button. Independent overall, music, nature and effects levels have labelled 10% steps and a global mute. Tab/arrows and Enter/Space work alongside pointer controls. Escape returns to the previous screen. Settings persist under `myterrain.audio.v1.*`, separate from town saves, and survive reload and new runs.

No sound plays until a player pointer/key gesture. Losing browser focus, hiding the tab, pausing the application or muting stops effect tails and pauses all beds. Returning resumes the music position with a gentle fade. Sounds queued while silent are discarded. The director uses its own fixed six-voice pool, bounded effect headroom, per-cue cooldowns and a separate local random generator; it never consumes gameplay randomness or mutates town state. Its sources ignore the legacy atlas scene-listener pause and implement their own consistent focus/mute policy. There is only one director across scene changes.

Sound settings consume input before either atlas or game handlers. A press that closes the panel cannot place a project behind it. Existing dialogs remain open behind settings, and their game state is untouched. Reduced motion is independent of audio, and every existing text/visual consequence remains usable with sound off.

## Assets and reproduction

The complete bank is `Assets/Terra/Resources/Audio`: 79 clips, about 7.5 MiB source payload before Unity's platform encoding. Long tracks are compressed in memory with explicit lazy data loading; short cues decompress for prompt response. Browser playback follows Unity 6000.6's supported Web Audio path, not native-only mixer DSP or `OnAudioFilterRead`.

`manifest.json` records source URLs, creator/license, source and output hashes, processing, duration, encoded peak/RMS and loop boundary steps. `CREDITS.txt`, `LICENSE.txt` and `KENNEY-LICENSE.txt` ship with the bank. In-game Field Guide includes a dedicated sound credits page. Prior C3 assets and licenses remain intact for historical scenes.

Sources (all CC0): [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds), [Indieteur Mystical Piano](https://opengameart.org/content/mystical-piano), [The Cynic Project Vaporware](https://opengameart.org/content/calm-piano-1-vaporware), [Thimras Park ambiences](https://opengameart.org/content/park-ambiences). Existing Kenney Interface Sounds and Luke.RUSTLTD wind provenance is retained in the original ExternalKit manifest. New mallet gestures are composed and synthesized by `scripts/prepare-audio.py`, with no paid or credentialed service.

Rebuild using Python with `numpy scipy soundfile imageio-ffmpeg`: `python scripts/prepare-audio.py`. Downloads stay in ignored `Builds/audio-sources`; only mastered derivatives are checked in. The script uses FFmpeg for Ogg encoding because the host's libsndfile crashed on the longer music encode. Run `python tests/audio/verify_bank.py`, then Unity `TerraAudioChecks.Run()` and the actual Chrome checks in the evidence folder. F8 emits a read-only `TERRA_AUDIO_AUDIT` receipt for browser verification.

## Verification status

All17 project families and the full year/recovery/ending/replay flow were exercised through real Chrome input. Mute, independent controls, persistence, focus pause/resume and three responsive settings captures passed; the bank passes four signal/provenance suites and380 Unity checks. See [the exact source/export handoff](evidence/audio/HANDOFF.md) for receipts and combined-release status.
