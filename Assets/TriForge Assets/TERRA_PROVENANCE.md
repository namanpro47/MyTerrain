# TriForge forest resources used by Terra

Source: TriForge Assets, Fantasy Worlds: Forest FREE, asset 282610, version 1.0 (April 17, 2024).
Official listing: https://assetstore.unity.com/packages/3d/environments/fantasy/fantasy-worlds-forest-free-stylized-forest-environment-open-worl-282610
License: Standard Unity Asset Store EULA, https://unity.com/legal/as-terms. Not CC0.
The user explicitly authorized accepting these terms for the free acquisition on October 24, 2026. No payment.

The original archive and full imported gallery remain local research files. This private project tracks only the dependencies used by the candidate tree/sapling wrappers. Do not redistribute the source library as a standalone asset pack.

Derived assets in `Assets/Terra/Resources/Forest` retain creator geometry, UVs and texture artwork. Terra splits submeshes into consistently ordered LOD parts and bakes clustered leaf normals with `scripts/art/bake-cluster-forest.cs.txt`. Native bark shader is retained; leaf rendering uses Terra's cutout/alpha-to-coverage shader. Original texture pixels are unchanged. Import maxima: leaf atlas 2048, bark/normal/mask textures 1024. Native tree root origin is the soil plane; buried root geometry is intentional.

This is an unaccepted visual candidate, not a production-quality certification. Re-baking requires the original free package import; the derived runtime prefabs work from the tracked subset alone. Study34 evidence records remaining gaps and browser verification status.
