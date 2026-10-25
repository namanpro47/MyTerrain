# Actual Chrome recorder preflight — October 25, 2026

This verifies the existing opt-in F8 canvas recorder on frozen candidate **2f8543dd2734172942f92c1e3395b695174395e9**, served at `http://127.0.0.1:61259/?capture=1&place=meadowbrook-v1`. It is **provisional engineering footage, not the final release fallback or a completed rehearsal**. No runtime code changed for this check.

The tab used Chrome 155.0.8059.85 at 1280×720. F8 started the recording; actual controls moved the camera, opened and paged the catalog, selected Westfield, queued Restore meadow, undid/redid it and opened the ten-year confirmation. The recording finished while that confirmation remained open. It does not show Year10, a completed annual chapter or the HTML atlas. No audio stream is present.

The visible download link appeared and was clicked. The browser download-event wait timed out and reset the tool session, but Chrome had saved the completed 10,122,957-byte WebM to Downloads. The file was copied without modification to `Builds/Recordings/terra-recorded-2026-10-25-2f8543d-provisional.webm`; [identity](identity.json) records its SHA-256. The original download is preserved. The recording console history was unavailable after reconnection, so no completion-console claim is made.

A complete FFmpeg 7.1 decode exited 0: **1,058 decoded VP9 frames, 1280×720, last decoded time 59.90s**, no audio. The MediaRecorder WebM does not carry a container duration field; the decoded timestamps establish its approximately 60-second span. This variable-rate capture is not a 30fps performance result. Seven extracted frames were visually inspected: landscape/catalog at 0/10/20s, then the confirmation at 30/40/50/59s. Long modal dwell makes this unsuitable as the final demo fallback.

[Full decoder receipt](decode.txt), [download link visible](download-visible.jpg), [sample contact sheet](samples.jpg). The large video remains in ignored local Builds; metadata and sample evidence are committed. The temporary server was stopped, viewport override reset and test tab closed.

The final accepted export still needs an intentionally paced recorded gameplay route with date/source labeling, content and duration verification, plus the two timed rehearsals and exact public verification. Recording overhead remains separate from performance measurements.
