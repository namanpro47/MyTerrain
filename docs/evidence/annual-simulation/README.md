# Annual simulation v4 — implementation in progress

Fresh isolated worktree based on `67c01417906b74a8f024ae9975158cb1dda6979d`. This candidate is not integrated, activated or browser verified. Existing 3.x runs remain unchanged.

The first pure C# component consumes the exact published Wisconsin weather pack and daily companion. It checks their original byte hashes before decoding, derives the agreed UTF-8 SHA-256 contiguous schedule, validates saved schedule identity, returns detached data and calculates separate crop-calendar precipitation/heat/dry-spell summaries. No run/branch/action enters the weather selection. It preserves precipitation as liquid equivalent and does not infer intensity, flood depth or a renewable-generation driver.

Initial validation: 118 checks pass, including all six published ASCII/Unicode schedule vectors, changed-byte/schedule rejection, UTF-8 seed size, ten annual crop-calendar summaries and detached return objects. This does not validate the upcoming crop response, persistent site damage, public repair or report-only acceptance implementation.

```sh
DOTNET_ROLL_FORWARD=Major dotnet run --project tests/annual-simulation/Terra.AnnualSimulationChecks.csproj
```

Next work extends `AnnualTown` as the same authoritative queue/ledger/history owner, with optional versioned numeric DTOs and preserved legacy hashes. Every site needs persistent condition/crop state; accepted reports need itemized liabilities, raw deficits and score deltas. Report-only narration is a separately owned additive contract. No scene/renderer/HUD is owned by this pure-model slice.
