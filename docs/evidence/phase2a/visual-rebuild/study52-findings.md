# Study 52 — continuous grain canopy, rejected

October 25, 2026. Patterns TN-02/09/12. Reviewed the official [Terra Nil restored-bank frame](https://freelives.net/uploads/2024/04/Terra-Nil-Screen-3.webp) in Chrome again: readable surface masses and sparse edge accents remain a stronger target than our repeated fine plant grid. This is a visual observation, not a claim about the reference game's implementation. Phase 2A remains open.

## Construction and controls

On `aec6261` with the transient study51 material experiment still active, a new continuous crop-canopy layer reused 182 terrain mesh chunks, displaced them approximately0.95m above ground, and clipped against field/river/path/track masks. It sampled existing Atapataco artist brushwork and matched the existing wheat material's linear color. Original artist assets were unchanged; no image generation or purchase. The canopy cast no shadow and was not integrated with annual lifecycle.

[Initial canopy close](study52-canopy-close.png) reduced the contrast of individual stalks but retained a regular planted grid. A second control jittered265,147 existing crop transforms by up to0.4m per horizontal axis, changed their yaw within±20degrees, and slightly broadened their geometry. [Close](study52-jitter-close.png), [normal](study52-jitter-normal.png). Placement resampled display height. This trial did not enforce parcel/route boundaries after jitter, and its conservative prior batch bounds were not rebuilt; it must not be promoted as a verified placement algorithm.

Disabling the canopy while retaining the jitter isolates its contribution: [close without shell](study52-jitter-only-close.png), [normal without shell](study52-jitter-only-normal.png). The grid becomes less obvious, but the field still reads as fine noise without the shell. With the shell, track sides and field edges visibly reveal an artificial raised sheet. Neither construction earns adoption. Do not keep tuning this layer's height or color as the main direction.

## Evidence limits and next action

The [construction receipt](study52-canopy.json) reports no shader errors; [placement receipt](study52-break-grid.json) records Year1/Planning/revision0/queued0/assets0. These are actual1280×720 Unity camera renders without overlay HUD, not Chrome or performance evidence. No claim of parity, performance improvement, validated annual behavior or production integration.

Shader and runtime scripts are preserved as text alongside the captures. The shell was disabled after the comparison. The diagnostic Play session was subsequently paused for another chat's foreground Chrome baseline measurements. The original production renderer is unchanged by these experiments.

The next visible gap being tested is the uniform road ribbon: retain mapped centerlines/outer coverage while giving the illustrative surface readable worn tracks, grass shoulders and coherent artist-authored earth paint. Meadow/field construction is still unresolved; road improvement cannot close that broader gate by itself.
