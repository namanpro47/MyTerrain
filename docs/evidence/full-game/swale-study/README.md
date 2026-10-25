# Dry-swale boundary study — rejected, October 24, 2026

Source: `f4f148d`. Patterns TN-02/09/10/12. The exposed dry bed reads as a broad muddy strip, with weak planted shoulders. This bounded study compares alternatives using the existing free Nature grass family and ground textures. No production source change was retained.

The official [Terra Nil frame](https://freelives.net/uploads/2024/04/Terra-Nil-Screen-3.webp) was inspected in Chrome. Its connected vegetation masses and clear bank boundaries informed the study; its stylized cliffs were not transferred to the Wisconsin terrain. The reference is comparison material, not a game asset.

All four captures are the same 1600×900 offscreen Editor camera: perspective FOV 42°, pitch 42°, yaw −32°, distance 175, focused on Meadow edge. The isolated annual fixture completes one connected swale through Year 10 ($19.723M cash). Play was deliberately paused at frame 4218, with the main update disabled. The capture script explicitly submits InstancedNature batches before camera.Render. These are paused art diagnostics, not browser play or performance evidence. Labels/HUD are hidden for the comparison.

- `baseline.png`: production swale, rounded 1.15 m illustrative depression and full existing earth mask.
- `narrow-bed.png`: cube the surface blue channel to narrow exposed soil, then add 336 low and 40 tall plants along inner shoulders. Small boundary improvement; the muddy ribbon remains dominant.
- `taller-shoulders.png`: same narrower mask, 187 larger low plants and 19 tall plants. The edge is more visible, but scattered bright blades do not form a convincing continuous bank. Rejected; no further tuning of this approach.
- `vegetated-bed.png`: remove the earth mask and plant the basin as well as its shoulders (338 low/38 tall). This materially different fully vegetated approach merges into the surrounding meadow and loses the swale's legibility. Rejected.

All trials retain the original rendered depression, analytical terrain, storage rules and public finances. Temporary objects and treatment texture edits lived only in Play Mode. Play was stopped afterward, discarding the fixture and modifications. No browser session, save, asset or scene file was modified. The scripts are preserved solely to reproduce the rejected trials; remove `/tmp/terra-swale-original-pixels.bin` before a fresh fixture to avoid reusing a stale temporary buffer.

The next direction needs a coherent low bank/groundcover treatment with useful volume and restrained values, rather than extra scattered blades or a thin soil stripe. No Phase 2A score or acceptance claim follows from this study.
