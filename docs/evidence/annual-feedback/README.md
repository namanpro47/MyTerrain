# Annual feedback — October 25, 2026

Work began from freshly fetched main `f4bd323` in the isolated `annual-feedback` worktree. Incoming frozen release `19e7eae` was reviewed and merged before implementation. It preserves toast priority, rounded score consistency and dock clearance. No changes to the original checkout's dirty Unity configuration.

The user's nine requirements are the acceptance checklist: full-plot skip to Year 10; correct current-year affordability; pre-play decision horizon with all ten years simulated; paid delayed land-use conversion; remove Living Margin from choices; remove saved-year-story alerts; remove abstract production capacity metrics; decade narrative; project details before placement.

## Decisions and limits

- Affordability reserves **upfront costs + this year's program/upkeep + existing arrears**, using current cash. Future appropriations and tax receipts do not fund current placement. Later obligations remain disclosed. This intentionally replaces the former requirement to hold all future upkeep in cash. Later income/obligations can still cause shortfalls; all unpaid amounts persist in the ledger.
- Changing use charges the new project plus site preparation/removal at **10% of the old upfront contribution, minimum $25K**, and settles remaining contracted program installments. These are authored game assumptions. Historic spending is never refunded. Old benefits stop; the replacement has at least one construction/transition year, followed by its configured establishment period. Previous accepted reports remain unchanged.
- 1, 3, 5 or 10 decision years are offered before play. The same ten annual transitions, conditions, payments, reports and save boundaries run afterward. The engine refuses new proposals outside the chosen decision horizon.
- Living Margin data remains interpretable for historical saves; it is removed from the playable catalog and shortcut. No definition hash change or historical reinterpretation is needed.

## Current verification

- New model/regression harness: 283 assertions, all three dense-parcel landscapes. Includes $5M apartments with $5.5M cash, one-cent-short rejection, conversion preview/undo/redo/pending reload/exactly-once payment, establishment, immutable history, rejection of operating effects during conversion, all four decision horizons, full-land skip equivalence and Year 10 reload. Full-land stress uses an explicit $100M fixture to isolate the completion path from strategy financing.
- Full-game regression: 2,810 assertions including 51 strategy/order stress routes and interrupted/reloaded future progression. Assertions now check exact carried debt rather than the obsolete full-term-reserve/no-debt assumption.
- Chapter service: 50 checks pass, including old Unity serialized pending requests and hashes; no real provider called.

UI integration is implemented on presentation handoff `cb78b65`: pre-play duration selection, conversion review, current-year budget with future obligations, full-landscape shortcut, persistent selected-tool facts and whole-decade introduction. The shared scrollable reader and footer layout are preserved. Unity/browser tests and final main delivery remain pending. Do not treat model checks as visual acceptance. Patterns TN-05/06/08, CS-01/02/04 and AX-01 apply. This is bounded gameplay/UI work, not scenery/Phase 2A acceptance.


## Current-main reconciliation

Merged refreshed `1330dc9`, preserving v5 cancellation/refunds, negative public cash, seeded regional events and save-first optional narration. Direct conversion remains a distinct paid transition; cancelled plots can be rebuilt without being charged a conversion again. Planning guidance retains incoming cancellation/debt guards. Future upkeep no longer prevents an otherwise affordable current project in either legacy or v5 rules.

Combined model checks: **472** across all three dense landscapes and both rule versions; v5 regression **81**; save-first session adapter **24** (stub transport/Unity, no live provider). The replay storage follow-up uses lossless compression for full histories/chapters and preserves the current run if backup fails. Legacy JSON remains readable. Native and Chrome verification of this combined source remain pending. The obsolete d329158 export was intentionally stopped after its native1106+16 checks passed; it is not a final build.

## Browser save blocker and bounded fix

Frozen `e74496b` exported successfully but was rejected: actual Chrome failed at the first compressed save (`RuntimeError: unreachable`). Failed captures and logs are preserved in `chrome-e74496b-failed`; its export/native/served manifests are historical, not acceptance.

WebGL now uses synchronous MIT fflate 0.8.2 GZIP, retaining `terra-gzip-v1:` and native/old plain JSON compatibility. Decode validates size, CRC, UTF-8 and the gzip trailer; native/editor retain GZipStream. The browser bridge avoids the native compression callback used by the failing WebGL/LTO path. Twenty-four focused Node interoperability/corruption checks pass, including the actual dense Unity-written fixture.

Annual/chapter save and receipt reads now reuse the existing pinned exact Json.NET codec. Internal receipt copies no longer roundtrip through JsonUtility. All committed request/report hashes and money remain strictly checked. Thirty-six session checks pass, including the supplied fractional v5 history, exact bit pattern, and rejection of changed ledger/outcome, unknown members and null input. The two compressed restore fixtures are regression evidence only; the regional maps they describe are not included as playable content. Native fixture checks and actual browser rerun are pending.

## Timeboxed delivery

User requested a ten-minute stopping point at 20:59:28 UTC. Source fixes landed on main via `b9c6686` (later main changes are owned by the consolidation chat). Corrected source `2aeca9a` passed native **1,322 + 16** checks and exported successfully. `native-save-fix.json` and `export-save-fix.json` identify that exact candidate; MIT fflate notice is included in the frozen export under `licenses/`.

Actual Chrome now reaches Begin and the selected-tool screen with revision 1 saved, passing the former startup crash point. The automated placement/year/replay sequence is not accepted: the harness remained in placement selection and needs its pointer moved off the action UI before keyboard targeting. No full browser or production deployment pass is claimed here. The consolidation chat owns final latest-main integration, remaining browser verification, and publication. This candidate does not include late `8a275b8`/`857530d` runtime changes that landed after its freeze.
