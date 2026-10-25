# Terra stream candidate

Source: [MatrixRex/Uber-Stylized-Water](https://github.com/MatrixRex/Uber-Stylized-Water/tree/1fdd16c6312611fef7fb248dee86cdd3fd456a1e), pinned `1fdd16c6312611fef7fb248dee86cdd3fd456a1e`. MatrixRex MIT license is included. The CustomLighting include retains Cyanilux's MIT license. No purchase or generated image.

Imported the supplied prebuilt shader, its two HLSL dependencies, supplied textures and Clear/Tropical reference presets. No upstream C# scripts, Editor extension, pipeline assets or demo scenes are imported. The original shader SHA-256 is `63445669c07c50d57b06a2037b233b87236267bdfb2d6cb1d8e5b2cdbb2d8e66`.

The shader's only Terra adaptations are a `_MotionTime` property/uniform in its six passes, twelve input-time assignments connected to that clock, and removal of the optional custom Inspector declaration. This preserves the upstream normal/refraction/caustic implementation while allowing the game's reduced-motion setting to freeze every animated layer. The original remains available at the pinned source.

`Resources/Water/TerraStream.mat` copies current shader properties from the supplied Clear preset. Planar reflection is disabled because no planar reflection component is installed; unused surface-foam, intersection and shoreline texture references are cleared. Normal mapping, scene-color refraction and caustics retain their supplied settings. The main camera requests depth/color textures only when the pack has water features. Shared quality/pipeline assets are unchanged.

The mapped route, analytical terrain, channel presentation mesh, annual accounting and dry swales are unchanged. Visible bed/optical depth is illustrative, not surveyed or a hydrological simulation. The candidate is not accepted until its exact Chrome export, motion control, artifact review and performance pass. See `docs/evidence/full-game/water-study/` for matched Editor diagnostics.
