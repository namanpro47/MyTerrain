# Study 51 — meadow and crop material checkpoint

October 25, 2026. **Experimental, not adopted; Phase 2A remains open.** Captured on visual branch source `50fdb7c`, retaining the selective hillside relief from study50. Latest fetched main is `fff6eaec866281356c6f1270e797a0a6b5d1a747`; its newer swale optimization is not included in this checkpoint. See the [artifact manifest](study51-checkpoint.json).

Keep [DESIGN_REFERENCE](../../../DESIGN_REFERENCE.md) TN-01/02/09/10/12 and the [visual goal](../../../PHASE_2A_GOAL.md) visible during follow-up. The target remains coherent production-quality scenery; quieter textures alone do not meet it.

## What was tested

The original free Atapataco Grass and Grass_desatured images from [study50 provenance](study50-atapataco-source.json) were loaded into transient runtime materials. No new generated images or paid assets were used. Original texture files remain in the local research archive, not redistributed in this commit.

Disabling broad meadow scatter exposed an empty, mottled ground surface: [open meadow](study51-painted-open-normal.png). A subsequent runtime filter retained woodland-edge plants and isolated broad patches, reducing this root's instances from 48,636 to 13,880: [receipt](study51-groups.json), [normal](study51-painted-groups-normal.png), [close](study51-painted-groups-close.png). Other vegetation roots were unchanged. This is a diagnostic distribution experiment, not a demonstrated performance gain.

The final ground shader reduces brush contrast and uses a 16 m illustrative texture scale. A crop shader test reduces root darkening when plants occupy few screen pixels. At distance the field substrate blends toward the crop material color and wrapped lighting, making it read more as a planted mass. Using the actual material color property fixes an earlier linear-color mismatch. [Corrected normal view](study51-field-b-normal.png) and [crop close view](study51-field-close.png) preserve the result.

## Findings and unresolved gaps

The corrected distant field is calmer, but close crops remain sparse, repetitive stems and rows. The meadow still has visible broad brush repetition, while reducing clumps can leave it empty. These observations do not establish Terra Nil-level whole-scene quality. The temporary ground shader also leaves its field-lighting blend active after later path/bank overrides; resolve that interaction before considering production adoption.

`study51-field-normal.png` preserves the failed color calibration and oversized diagnostic outlines. Earlier meadow/group views also predate the fixed display-scale reset; do not compare their outline widths as UI evidence. The final normal/close camera helpers explicitly set display scale for the 1280×720 render target. All captures are Unity camera output with the main input controller disabled; overlay HUD is omitted. They are not Chrome, motion, transformed-scene or performance acceptance evidence.

## Recovery and scope

The exact final [ground shader](study51-ground.shader.txt), [crop shader](study51-crop.shader.txt), camera/runtime script snapshots, receipts and captures are committed as research evidence. Scripts retain their original local asset paths and are archived as text to avoid automatic compilation. To replay, restore the shader snapshots to `Assets/Study51Ground.shader` and `Assets/Study51Crop.shader`, provide the original licensed texture archive at the recorded research path, and use the paint, groups, field and field-b scripts in that order in a fresh Play session. The paint script begins at scale32; the field script changes it to16. Historical captures represent intermediate variants, not an exact replay of the final shader snapshot.

Runtime material/instance edits are temporary and are not wired into production scenes. Temporary shader files remain locally available for continued experiments; their exact contents are backed up here. Editor-generated settings/font churn and unused imported packs are excluded from this checkpoint. No new Web build, deployment, main merge or acceptance claim is included.
