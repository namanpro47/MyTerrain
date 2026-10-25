# Study66 — free painted nature family acquisition

October 25, 2026. Research/import checkpoint only. Phase 2A remains open.

## Purpose and evidence boundary

Evaluate a coherent existing painted tree/bush family against the repeated crowns and weak woodland transitions in study65. Terra Nil patterns TN-01/02/09–12 remain the comparison criteria. First inspect the supplied URP demo/materials; then compare the same Wisconsin play and close views. An asset download is not evidence of visual improvement.

The user approved accepting the Terms/EULA for BigRookGames' [Stylized Hand Painted Nature Pack – Free](https://assetstore.unity.com/packages/3d/vegetation/trees/stylized-hand-painted-nature-pack-free-177775). Account ownership was confirmed in Chrome during study65. Unity's signed-in Asset Store download service retrieved product177775 without payment. No purchase or generated image was used.

## Downloaded contents

- Version1.1 archive:53,124,420 bytes; SHA-256 `233fc839ec098e654ec1672887acb3b426f760afbb186ab9ea5e61acf8250cab`.
- [Actual archive inventory](study66-archive-inventory.json): tree and bush models/prefabs, painted textures, materials, built-in/URP/HDRP demo scenes and two nested conversion packages. This replaces the earlier advertised-only inventory.
- [Nested URP inventory](study66-urp-inventory.json): materials only. It does not contain project settings or replacement scripts. The inspected leaf material references a cutout shader, leaf albedo and normal textures; its supplied color/cutoff should be evaluated before adaptation.
- License recorded in the [asset register](assets.json): Unity Asset Store EULA. Source assets remain external licensed resources; no redistribution claim beyond the applicable terms.

## Import interruption

A filtered evaluation archive excluded the HDRP scene and embedded conversion archives. `AssetDatabase.ImportPackage` through the study Editor timed out. Read-only process sampling identified `ActiveTrustWindowProxy.ShouldProceedAfterTrustCheck` waiting in a modal window. No `Assets/BigRookGames` folder appeared. The subsequent command also timed out; no successful import or render is claimed.

The study Editor was closed with the CLI after the modal prevented graceful shutdown. All work was saved; no Play scene was active. The downloaded original and inspection files remain recoverable under local `Builds/ArtResearch/Study66`. Other Editors were not operated. The research browser tab was also closed for the coordinated performance window.

Next: use the original acquired package through Unity's package workflow, resolve its visible import prompt, apply the creator's URP materials, and inspect its demo before any production integration. Keep the assets out of the shipped scene until actual matched captures demonstrate a stronger result.

## Shared-state refresh

Fetched `origin/main` at `64384a04e956f623a38c07b7bfa611c941fe231b` and merged its test/evidence additions as `c0ebb13`, retaining both sides of the phase-status conflict. These include the annual-completion checks and failed Web64 timing. No candidate crop runtime was promoted. Study65 remains the latest Editor art candidate; study64 remains the latest browser-tested art on this branch.
