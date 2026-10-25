# B / D / C3 integration — October 24, 2026

The three authorized side packages were combined on `codex/parallel-package-integration` and merged into the primary `codex/unity-setup` checkout at **`1c6a1b0b8fe91a89cf7f0da1d95fafad55848c38`**. Integration head: `9304ba88e409e30565f7ec34b7cd443a00a3d8da`; original shared base: `08092a1f19ceb064ab21c11c6f97dd221693d410`. This is repository/Unity package integration. Phase 2A rendering and Phase 3 editing stay with their existing owners; the live Phase 4–6 consequences/mission/ending features are not activated by importing these packages.

**Merge preservation:** all **120** pre-existing modified/untracked files were hashed before and immediately after merging; every byte hash and deletion state was unchanged. The merge added only the 122 reviewed package/integration paths, with no overlap against active dirty files. [Recorded merge and hashes](evidence/parallel-integration/merge-preservation.json). The Phase 3 checkout was read only for compatibility; its Editor and files were not changed.

## Delivered and checked

| Package | Delivered implementation | Verification |
|---|---|---|
| B | Pure `Terra.Simulation` assembly: direct runoff, connected bounded retention, proportional per-unit accounting, structured facts, strict validation and frozen CN-mixture helper | 19 standalone C# suites, including 1,000 numeric combinations; .NET Standard 2.1/C#9 compile; independent frozen example; actual Unity assembly execution and JSON input/output round trip |
| D | 49 authored `ContentEntry` files plus unchanged fixture intro; brief, 5 hints, treatment/rejection copy, disclosures, 2 reasoning questions, 3 takeaway cards, ending templates and availability fallbacks | 12 Python test methods with malformed/mutated fixtures; actual DTO/source/scenario checks; all 50 entries round-trip through Unity `JsonUtility`; compatibility with both original Core and the current Phase 3 worker's Core |
| C3 | Five CC0-derived audio clips, stable GUIDs, exact source/license manifest, deterministic mastering and isolated Chrome audition | 94 source/identity/waveform checks; archive/member hashes and reproducibility review; Chrome WAV decode/playback-state/Stop checks; actual Unity import/load/readback for all five, decoded signal checks for the four short cues |

The combined isolated Unity run passes **141 package checks plus the existing 139 Phase 2 data/state checks** under Unity **6000.6.6f1**. [Actual Unity report](evidence/parallel-integration/unity-checks.json). The separate [combined standalone report](evidence/parallel-integration/combined-checks.json) records all seven commands, their exit codes, and the read-only Phase 3 Core compatibility snapshot. **All seven commands also passed from the primary checkout after merge**, including the audio GUID scan against current art assets and a fresh read-only Phase 3 compatibility check: [primary-checkout report](evidence/parallel-integration/primary-checks.json).

No main scene, renderer, camera, Core, gameplay/HUD, shared DTO, scenario, package manifest, project setting or public deployment is part of this integration. The extra Editor checker is scene-independent and never opens TerraMain. Unity's incidental changes to unrelated settings/metadata in the isolated checkout were excluded.

## Review fixes made before integration

- Phase 3 introduced eight new rejection messages. Exact mappings and six additional copy entries now cover them while preserving the old aliases. `--core-source` validates another checkout without editing it; unknown reasons still fail the compatibility check deliberately.
- Content validation now rejects a runoff template whose guard, result instance or DTO path could turn pending zeroes into an apparent comparison. Baseline/current provenance remains mandatory. The brief says to compare estimates when available, rather than implying that the disabled water goal already exists.
- The public scalar runoff helper rejects unrepresentable derived storage instead of returning a misleading zero for extreme double inputs. The float-DTO path was unaffected; regression tests cover both the public edge and valid small float input.
- The first Unity audit found identity-only audio metadata importing short cues as compressed clips, which prevented decoded-sample verification. `ParallelPackageChecks.ConfigureAudioImports` applied settings through `AudioImporter.SaveAndReimport`, then serialized current importer metadata. All clips preserve mono 22,050 Hz. Short cues are PCM, Decompress On Load and preloaded; ambience is Vorbis, Compressed In Memory and not preloaded. Force To Mono is off, so Unity's downmix-normalization option is inactive. GUIDs and source WAV bytes are unchanged.

TN-06/TN-08/CS-04 guide the learning content; TN-07 guides the restrained audio. The [Terra Nil reference](DESIGN_REFERENCE.md) and [active art contract](art-direction.md) remain authoritative. No visual gate is inferred from package or state checks.

## Reproduce

From this repository:

```sh
python3 scripts/verify-parallel-packages.py
# Optionally check the editing worker's current reason strings, without changing that checkout:
python3 scripts/verify-parallel-packages.py --phase3-core /absolute/path/to/Assets/Terra/Scripts/Core/WorldStateStore.cs
```

The standalone checks use Python 3 and the installed .NET SDK 10. Run Unity checks only in an isolated checkout that is not owned by an active worker:

```sh
unity run /absolute/path/to/isolated/Terra --log-file /absolute/path/to/package-checks.log --no-tail -- -executeMethod ParallelPackageChecks.Run
```

`Run` verifies committed importer settings without changing them. `ConfigureAudioImports` is the explicit setup/repair entry point for this kit alone, followed by the same checks. It must not be pointed at another worker's Editor. The historical Phase 2 evidence file is restored byte-for-byte after the regression check.

## Remaining phase integration boundaries

These are later game features, not unfinished implementations inside the delivered packages:

- **Phase 4:** derive baseline/proposal analysis units from state, resolve the documented sealed-cover parameter, partition unchanged context, use per-component aggregation where required, join hydrology by site ID without erasing production, and bind matched-storm/source facts. Keep current UI runoff unavailable until that adapter is verified. The scalar seam cannot establish geographic non-overlap or real drainage by itself.
- **Phases 5–6:** package/load index-listed content, bind availability predicates and real result values, connect onboarding/objectives/comparison/learning/Continue Building. The Python renderer is a test oracle, not an already installed UI formatter. No content entry grants an unsupported action.
- **Audio wiring/mix:** use the shared mute setting and a real user gesture, prevent duplicate old/new cues, handle background/pause and late loads, and test final Web AAC looping and sound levels in the actual scene. Source-kit and Unity import acceptance do not claim subjective listening, two browser game loops, or final mix approval.

The [model assumptions](model-assumptions.md), [content storyboard and bindings](learning-content.md), and [audio handoff](asset-kit.md) contain the exact integration contracts. The single Meadowbrook Valley location, free-only asset budget, and no-image-generation direction remain unchanged.
