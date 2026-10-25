# Contextual annual planning guidance

Candidate on refreshed main `ba59445`. **Model verification passes; Unity and Chrome verification are pending. This is not a published or accepted release.**

The existing catalog summary now describes the next annual step when no project is selected: inspect, queue without paying, review/commit, wait for the pending result, understand the report, or adapt. Selecting a project retains its own upkeep and available-field summary; cancelling restores contextual guidance. No extra panel, questionnaire, ledger, save format or rule change is introduced.

After the first accepted year, the prompt can identify an unmet community need and suggest a real affordable offer that can open before Year 10. It accounts for already committed capacity, including projects still under construction. Drinking-water scarcity is not presented as something wastewater treatment or waiting fixes. Unaffordable projects, occupied sites and late construction lead to an explicit Wait option with remaining obligations. Suggestions are inspection prompts, not net-score predictions or optimal strategies; the player still reads the preview and chooses.

Reference patterns: **TN-06** (brief contextual instruction and deeper information on demand), **CS-04** (consequences and actionable feedback), **CS-01** (preserve the catalog hierarchy), **AX-01** (actual-size legibility). The existing [connected HUD captures](../hud-connected/README.md) are the layout comparison; they do not establish Phase 2A scenery acceptance.

## Model evidence

Both current connected and retained legacy Wisconsin layouts pass **2,460 assertions**, including 61 new checks. The tests use the actual authoritative annual model and verify unchanged snapshots, full commitment affordability, vacant sites, pending/accepted boundaries, already funded housing/power, drinking-water honesty, no available sites and Year 10 construction limits. Existing 51-family/order routes and failure/reload recovery at every year still pass.

```sh
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/full-game/Terra.FullGameChecks.csproj -- --connected --stress --evidence Builds/AnnualGuidance/model-connected
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/full-game/Terra.FullGameChecks.csproj -- --stress --evidence Builds/AnnualGuidance/model-legacy
```

Receipts: [connected](model-connected.json), [legacy](model-legacy.json), [runtime/test source hashes](source-hashes.json). The underlying strategy/stress evidence and its limits remain in the [annual audit](../annual-completion/README.md).

## Remaining verification

The prepared [disposable HUD check](hud-guidance-check.cs) will test actual summary binding, selected-project/cancel behavior, all authored fallback strings and catalog titles at 1024×576, 1280×720, 1440×900 and 1920×1080 with both text settings. It must leave the live annual engine unchanged. A fresh own-worktree Unity compile/export and actual Chrome interaction/capture are required before merging this candidate to main.

Human playtesting is unavailable by the user's instruction. No novice comprehension, play duration, audio listening or final visual acceptance is claimed.
