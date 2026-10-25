# Package B delivery evidence — October 24, 2026

Base: `08092a1f19ceb064ab21c11c6f97dd221693d410`.
Managed checkout: `/Users/naman/.codex/worktrees/terra-simulation-b/Terra`.

Implemented the pure `Terra.Simulation` assembly, frozen scalar CN mixture helper, connected bounded retention, proportional source accounting, validation and explanation facts. No shared Contracts, Core, gameplay, world data, UI, scene, project settings, packages or phase-status files changed. No Unity Editor, browser, deployment or additional location was opened/created.

- [C# test results](simulation-checks.json): **19 suites pass**, including 1,000 storm/CN/capacity combinations, the frozen JSON numeric example, independent hand/table values, all current recipe CN mappings, actual Meadowbrook cover-input compatibility, mutation/repeatability/permutation checks, malformed inputs, connection/cap limits and float overflow.
- [API compatibility build](netstandard-build.txt): actual production files and frozen DTOs compile with C# 9 / .NET Standard 2.1; **0 warnings, 0 errors**. This is a standalone API compatibility check, not Unity import or an IL2CPP/Web build.
- [Original independent scalar example](frozen-example-check.json): unchanged Python verifier passes.
- [USDA source verification](source-review.json): formula and coefficient tables inspected from the authoritative downloaded report; SHA256 recorded.
- [Model assumptions, facts and adapter requirements](../../model-assumptions.md): scalar-versus-component mixture rule, non-double-counted additional capacity, source/device attribution, finite float tolerances, unsupported metadata and analysis-domain limits.

Reproduce from this checkout with .NET SDK 10.0.402 (10.0.13 runtime) and Python 3:

```sh
dotnet run --project tests/simulation/SimulationChecks.csproj -- .
dotnet build tests/simulation/UnityApiCompatibility.csproj --nologo
python3 scripts/verify-simulation-example.py
```

The test projects require no external NuGet test framework. Build intermediates stay ignored inside `tests/simulation/`. Runtime code is standard C# with only the frozen Contracts assembly referenced by its Unity `.asmdef`.

Integration remains separate: map current cover and capacity to inputs, join hydrology by site ID while preserving production, use identical baseline/proposal storms, bind explanation/source IDs, resolve the missing `sealed` mapping, and define truthful analysis coverage. The current runtime contract does not freeze `sealed`; CN98 is a sourced proposal awaiting the integration owner's deliberate adoption. The evaluator cannot infer treatment IDs or prove non-overlapping geography from the scalar input. Phase 2A/3/4 acceptance is not claimed.
