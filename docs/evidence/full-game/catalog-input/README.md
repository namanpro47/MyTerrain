# Catalog keyboard routing — October 25, 2026

The scenery candidate's Chrome check found that Undo was swallowed after catalog keyboard navigation. The nonmodal catalog retained its keyboard mode after pointer use and consumed Z/Y as unrecognized catalog keys.

The runtime now forwards Z, Y and Shift-Z to the existing gameplay undo/redo route and exits catalog navigation. A pointer press also exits that mode; hover alone preserves it. Modal input isolation and catalog arrows/Tab/Escape retain their behavior. No ledger or refund semantics change.

## Verification

[Runtime fixture](check.cs) executes against the actual TerraMain HUD in Play mode, using a temporary Input System keyboard. [Recorded result](editor-check.json): **18 assertions pass**, frame108204; annual state byte-identical (Year1 Planning, revision0, $10M, zero proposals/assets). The fixture exercises the real main pointer-down call, catalog navigation, undo/redo forwarding, modal containment and recovery. It verifies routing, not the downstream transaction itself.

For this batch Editor fixture, input is temporarily routed to the Game View with manual updates and IgnoreFocus; the keyboard is created after that configuration and sampled inside onAfterUpdate. All settings are restored and the device removed in finally. Initial attempts outside the update/focus context failed before reaching gameplay assertions. The preserved [empty-scene setup failure](empty-scene-setup.json) predates opening TerraMain and is not a product failure. The Editor was stopped and closed gracefully after the successful run; session configuration and generated metadata noise were restored.

Next combined Chrome export must confirm B → pointer proposal → Z → redo, direct B → Z/Y, and modal isolation. Browser verification is pending; this fixture does not certify Phase2A or whole-game acceptance.
