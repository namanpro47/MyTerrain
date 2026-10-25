# Spoken dialogue

User requested October 25, 2026: every tutorial/instruction and turn-by-turn dialogue should be read aloud, using their ElevenLabs account and a female voice. This adds optional spoken presentation; game rules, saves and text do not depend on the provider. Design references: TN-06 field notes, TN-07 quiet feedback, TN-08 annual consequences, AX-01 sound-independent information.

## Runtime

`TerraHud.Narration.cs` reads the existing visible modal, scrolling field notes (including the whole body), and annual highlight cards. `WorldAtlas.Narration.cs` reads the atlas field-notes popup and the starting invitation. `TerraMain.Audio.cs` sends only the currently visible surface to the singleton `TerraNarration`. The adapters are additive partials so layout work can proceed independently. When changing a dialogue surface, keep its title/body in this adapter or call `TerraNarration.Show` with the displayed text. No toast, hover or background HUD narration.

The voice defaults on. HUD dialogue popups have Voice on/off and Replay/Retry controls; U opens persistent narration volume and voice toggle alongside existing audio buses. Master mute/volume applies. Background music ducks while speech plays. A user gesture is required for browser audio. Moving to another page or closing the popup stops the previous voice. Muting or leaving the tab pauses playback. Failure leaves the same text and actions available; only Replay retries a failed passage. Rich-text tags are removed and the whole passage is split into bounded segments, without truncating long guides.

## Provider and key

Default: Matilda (`XrExE9yKIg1WjnnlVkGX`), female American narration; Flash v2.5 for interactive latency and credit efficiency. Stability .55, similarity .75, speed .96. Actual account/voice confirmation and live evidence are recorded in `docs/evidence/voice-narration/README.md`.

The dedicated key has Text to Speech and Voices read permissions, with a **10,000-credit limit per refresh period**. It cannot administer the account or generate unrelated media. Do not put the key in Unity, JavaScript, Git, build files, logs or screenshots. Only dialogue text goes to ElevenLabs; the save, ledger object and player identity are not sent by this feature.

The browser calls the same-origin `/api/narration` function. The key belongs in the Vercel `terra-browser` project's sensitive `ELEVENLABS_API_KEY` environment variable for production/preview. Optional server variables: `ELEVENLABS_VOICE_ID`, `ELEVENLABS_VOICE_NAME`, `ELEVENLABS_MODEL`, `TERRA_VOICE_ORIGINS`. A Unity export copies only the function source into `Builds/Web/api/narration.js`. Deploy the whole tested export, preserving the function. Static-only hosts need an equivalent server endpoint.

API safeguards: fixed provider URL/model/voice, 700-character plain-text request limit, allowed origins, three concurrent upstream requests per instance, 30 requests / 10,000 characters per IP per minute per instance, 20-second provider timeout, sanitized errors, duplicate request coalescing, bounded disk audio cache. The per-instance throttle is not a distributed quota or authentication system; **the ElevenLabs key cap is the hard global spending limit**. Exhausting it makes new speech unavailable until refreshed or deliberately raised; text gameplay remains usable. No paid upgrade is required by the implementation.

The browser keeps up to 250 compressed passages in Cache Storage and 30 in memory, keyed by exact text and a voice/model/settings revision. Server disk caching is bounded and ephemeral on Vercel. Replay/reload can reuse cached audio. Private browsing or disabled storage falls back to memory. Changing the voice/settings changes the revision.

## Local / Editor

Create a private repository-root `.env` using `services/narration/.env.example`, then run:

```sh
node --env-file=.env services/narration/server.js
```

The voice service binds only `127.0.0.1:8788`; local browser exports and the Editor use it. It is independent of the existing chapter-writing service on 8787. Node 22+ is recommended (production uses the project's Node 24). Unity Web uses Web Audio through `narration.js`; Editor/native uses a dedicated nonspatial AudioSource and the same gateway. Browser-only voice credentials are never needed.

## Verification

```sh
node --test tests/narration/*.test.cjs
```

Checks exercise gesture/mute/focus, cancellation, cache replay, explicit recovery, segmentation, request validation, origin restrictions, duplicate generation, rate limits and secret-free errors. Follow with the actual Unity Web export in Chrome: open help/guide, switch pages, End Year and read the story, replay, mute, close mid-speech, leave/return, and check the sound panel. Confirm the network sees only the game endpoint and no provider credential.

Official technical references: [authentication](https://elevenlabs.io/docs/api-reference/authentication), [speech conversion](https://elevenlabs.io/docs/api-reference/text-to-speech/convert), [models](https://elevenlabs.io/docs/overview/models).
