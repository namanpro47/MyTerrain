# Inactive browser run directory

Prepared from main19e7eae in a fresh isolated worktree. This is an additive storage checkpoint, not active gameplay, a complete adapter, or browser acceptance.

`TerraNumericDirectory.jslib` upgrades the existing numeric database to version2 while retaining its `runs` store. Separate immutable `definitions` and small `directories` stores hold exact rule text and a per-location/schema/model-family active pointer. Create compares the old directory hash, inserts a previously absent run and any absent definition, and updates the pointer in one strict readwrite transaction. Select compares both the old directory and target saved-run hash, then changes only the pointer. Every prior entry is retained. Reads return one consistent directory/definition/save bundle or one retained run; missing/corrupt references are errors. JSON strings containing numeric definitions/saves are preserved verbatim.

The existing per-run bridge also opens version2 and creates all three stores on first use, so either bridge may initialize the database. No legacy PlayerPrefs keys or saved numeric runs are deleted. Old version1 code cannot reopen a version2 database; activation requires the updated bridges together. This remains outside the frozen production export.

Verification:81 modeled IndexedDB assertions using pinned fake-indexeddb6.2.6 cover exactly-one-winner concurrent creation/selection, stale target saves, retained history, abort after each of the three writes, quota failure, lost-ack recovery, corrupt references, immutable definition conflicts, v1 upgrade with existing data, missing references, orphaned-run overwrite, and attempted removal/replacement of retained entries. Symbolic fixture hashes exercise the storage protocol; the pure annual controller supplies full numeric/hash validation. Existing per-run bridge33 fault assertions still pass. These are source tests, not actual Chrome transaction/Unity callback proof.

Run from the repository root:

```sh
npm ci --prefix tests/annual-numeric-directory-browser --ignore-scripts
node tests/annual-numeric-directory-browser/directory-checks.mjs
node tests/annual-numeric-client/storage-checks.mjs
```

The annual owner implements and validates the pure directory controller separately. A Unity adapter, actual v2 Chrome checks and combined integration still follow. No Main/HUD/scenes or public release change is included here.

## Deadline handoff — October 25

The pure directory controller from `8b33cc9` is merged with the exact numeric codec and frame-checkpoint candidate. The additive `AnnualNumericRunDirectoryBrowserStore` now implements its acknowledged store interface with explicit GameObject lifetime, exact definition/save strings, validated directory identity, request correlation, timeout/cancellation recovery, and no automatic gameplay attachment. An optional checkpoint passes through directory content validation into the numeric transaction validator; WebGL integration must supply a main-thread next-frame callback.

Validation after reconciliation with main337dbf2: **149 pure directory assertions**, **81 modeled IndexedDB assertions**, **33 per-run bridge fault assertions**, and the actual installed Unity6000.6.6f1 WebGL API compilation (**0 warnings/errors**) pass. The new checks verify checkpoint propagation and cancellation without writing or changing accepted history.

**Unfinished:** actual v2 browser/IL2CPP callback and reload tests, dense memory-fix player acceptance, production Main/HUD wiring and a combined public run. These changes remain inactive on a pushed integration branch. Do not enable them or upgrade a submission build's database without the linked tests and shipping both v2 bridges together. The public schema-2/v5 game and this schema-3 numeric-v4 module intentionally remain separate.
