# Dense numeric load allocation diagnostic

The numeric client owner observed an actual linked WebGL out-of-memory abort loading the340-site, ten-year Suusamyr fixture. Sparse initialization/reload passed separately. Numeric mode remains inactive; no check is bypassed to make it load.

This independent read-only probe measures cumulative managed allocations for that exact fixture, with Core/Contracts/Simulation from main19e7eae plus additive run-directory8b33cc9. It changes no game source. The client owner retains the codec, adapter, WebGL harness and integration diagnosis. Shared AnnualTown/TownCopies are concurrently owned by the annual-feedback task.

| Operation | .NET allocated bytes | Installed Unity Mono allocated bytes |
|---|---:|---:|
| Copy full envelope | 14,450,552 | 19,382,696 |
| Hash full envelope | 165,662,712 | 155,829,568 |
| Copy definition | 5,574,392 | 7,182,216 |
| Hash definition | 44,095,344 | 48,144,416 |
| Create engine | 65,668,952 | 74,535,912 |
| Restore ten years | 854,391,400 | 837,935,960 |
| Open existing numeric transactions | 2,543,295,848 | 2,668,344,832 |
| ReadEngine after open | 853,982,344 | 837,891,992 |

Each operation is measured separately after collecting prior garbage. These figures are total allocation volume, not peak live memory or WebAssembly heap measurements. Desktop runtimes can collect during these calls; WebGL's end-of-frame collection behavior makes a long synchronous sequence materially different. Timing is diagnostic only, with other work active on this machine. The two runtimes also use different number-formatting implementations; the Mono repeat avoids attributing .NET's compatibility formatting allocations to Unity.

Source review finds `InitializeNumericAsync` calls ReadEngine, then NumericCommitted immediately calls it again. Existing-save Open also validates ten narration projections after replay. These are concrete investigation targets; this receipt does not establish a fix, justify skipping canonical replay/hash validation, or certify numeric runtime activation. A final linked-browser retest must succeed with exact retained hashes and meaningful heap measurements.

Inputs from the numeric client's `Builds/AnnualNumericClientVectors`:

- dense-definition.json SHA256 `e25b9441a82696d93bcbe81f4b8fb2faa8c530d5fe8b99d145b664cbab1c02bb`.
- dense-save.json SHA256 `3e72b76d277e68bf7caa2a643ae8021f830514468b740aa388efab2ebe6135eb`.

`program.cs.txt` uses the host.NET runtime with net8.0 sources. `mono-program.cs.txt` uses Unity6000.6.6f1's ARM64 Mono, the C#9/.NETStandard2.1 portable assembly, and installed Newtonsoft3.2.2. Its counter is `GC.GetAllocatedBytesForCurrentThread`; the fake store completes synchronously, so all measured work remains on that thread. The programs fail if a retained save fails normal validation. `binaries.txt` pins the portable and Newtonsoft assemblies. JSONL files preserve exact measurements. No Unity Editor, export, foreground browser or production store was touched by these probes.
