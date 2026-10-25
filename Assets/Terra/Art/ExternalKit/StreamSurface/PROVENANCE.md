# Stream surface normal

Source: MatrixRex/Uber-Stylized-Water, MIT, pinned commit `1fdd16c6312611fef7fb248dee86cdd3fd456a1e`. Original `Assets/Shaders/Uber Stylized Water/Textures/Normal 1.png`, unmodified source pixels. Source URL: https://github.com/MatrixRex/Uber-Stylized-Water/tree/1fdd16c6312611fef7fb248dee86cdd3fd456a1e . Full license is included here and in the exported atlas/vendor/WATER-LICENSE.txt.

Terra samples the normal texture in world space in its existing one-pass stream shader. It does not use the upstream shader, planar-reflection script, scene-color/refraction copy, or camera-depth pass. The generated channel mesh has no tangent attribute. Texture movement follows the existing reduced-motion clock. The surface is illustrative; no measured depth or flow claim.

Source SHA-256: `49b823e8072e4848e9af63440cfb663c40d04a4fc265214792db00365dd94112`.
