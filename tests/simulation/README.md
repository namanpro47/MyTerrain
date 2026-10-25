# Standalone simulation checks

Run from the repository root:

```sh
dotnet run --project tests/simulation/SimulationChecks.csproj -- .
dotnet build tests/simulation/UnityApiCompatibility.csproj --nologo
```

.NET SDK 10 runs the dependency-free console harness. It compiles the production Simulation files and the frozen contract DTOs directly; no copied model implementation or Unity stubs are tested. Expected numbers come from the pre-existing JSON acceptance fixture, hand-reduced rational cases and a rounded published USDA table value. Property/invariance checks complement those reference values. Nonzero exit means failure.

The second project compiles production source against .NET Standard 2.1 / C# 9, matching the intended Unity API/language subset without touching the live Editor. Actual Unity/IL2CPP/Web integration remains a later check. See `docs/model-assumptions.md` for scientific conventions and limitations.
