# Initial local iteration — October 24, 2026

The first real-landscape export built successfully through live Editor job `2ada015d211d4a90a3938313c5095961` (125.138 seconds for the script/build operation, 14,656,480 reported build bytes). It has not been published.

Local Chrome at 1280×720 loaded `meadowbrook-v1` without Console errors. Source ground, fields, streams and roads rendered; Tab/F showed the West field close-up, Enter placed an open plot (6→17 illustrative production, 60→36 credits), and Z restored the baseline.

The weakest initial issues were opening composition (the southern map edge sat behind the tool dock), image softness under the Web 0.8 render scale, and a 50 m shadow distance intended for the tiny fixture. The next source iteration lowers the opening angle and reframes the whole landscape, uses native render scale, extends the actual Web quality profile shadow range, strengthens site outlines slightly, and corrects the HTML accessibility/loading copy.

`initial-console.json` includes the initial local 30-second camera/placement path: R, F9, right-arrow 8 s, Tab/F, Enter/Z, left-arrow 8 s, zoom out 6 s, zoom in 5 s, R and right-arrow 4 s. 3,590 frames over 30.000 s: average 119.67 fps, p95 9 ms, p99 10 ms, maximum 120 ms; one frame >33.34 ms, none >200 ms. Three preview events (maximum CPU 0.5 ms), one placement (CPU 4.4 ms), 3,579 camera updates and zero unfocused frames. These are local observations of the initial export, not the final public performance gate. CPU handler timing does not measure browser dispatch or display latency; the next build adds DOM-input-to-next-rAF instrumentation.

Initial screenshots document the iteration, not final acceptance. Final 1280/1440 review, public deployment, actual public performance, error recovery and handoff remain pending.
