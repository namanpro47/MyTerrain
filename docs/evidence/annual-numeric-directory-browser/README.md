# Inactive browser run directory

Prepared from main19e7eae in a fresh isolated worktree. This is an additive storage checkpoint, not active gameplay, a complete adapter, or browser acceptance.

`TerraNumericDirectory.jslib` upgrades the existing numeric database to version2 while retaining its `runs` store. Separate immutable `definitions` and small `directories` stores hold exact rule text and a per-location/schema/model-family active pointer. Create compares the old directory hash, inserts a previously absent run and any absent definition, and updates the pointer in one strict readwrite transaction. Select compares both the old directory and target saved-run hash, then changes only the pointer. Every prior entry is retained. Reads return one consistent directory/definition/save bundle or one retained run; missing/corrupt references are errors. JSON strings containing numeric definitions/saves are preserved verbatim.

The existing per-run bridge also opens version2 and creates all three stores on first use, so either bridge may initialize the database. No legacy PlayerPrefs keys or saved numeric runs are deleted. Old version1 code cannot reopen a version2 database; activation requires the updated bridges together. This remains outside the frozen production export.

Verification:66 modeled IndexedDB assertions using pinned fake-indexeddb6.2.6 cover exactly-one-winner concurrent creation/selection, stale target saves, retained history, abort after each of the three writes, quota failure, lost-ack recovery, corrupt references, immutable definition conflicts, and v1 upgrade with existing data. Existing per-run bridge33 fault assertions still pass. These are source tests, not actual Chrome transaction/Unity callback proof.

Run from the repository root:

```sh
npm ci --prefix tests/annual-numeric-directory-browser --ignore-scripts
node tests/annual-numeric-directory-browser/directory-checks.mjs
node tests/annual-numeric-client/storage-checks.mjs
```

The annual owner implements and validates the pure directory controller separately. A Unity adapter, actual v2 Chrome checks and combined integration still follow. No Main/HUD/scenes or public release change is included here.
