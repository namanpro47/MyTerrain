# Annual feedback — October 25, 2026

Work began from freshly fetched main `f4bd323` in the isolated `annual-feedback` worktree. Incoming frozen release `19e7eae` was reviewed and merged before implementation. It preserves toast priority, rounded score consistency and dock clearance. No changes to the original checkout's dirty Unity configuration.

The user's nine requirements are the acceptance checklist: full-plot skip to Year 10; correct current-year affordability; pre-play decision horizon with all ten years simulated; paid delayed land-use conversion; remove Living Margin from choices; remove saved-year-story alerts; remove abstract production capacity metrics; decade narrative; project details before placement.

## Decisions and limits

- Affordability reserves **upfront costs + this year's program/upkeep + existing arrears**, using current cash. Future appropriations and tax receipts do not fund current placement. Later obligations remain disclosed. This intentionally replaces the former requirement to hold all future upkeep in cash. Later income/obligations can still cause shortfalls; all unpaid amounts persist in the ledger.
- Changing use charges the new project plus site preparation/removal at **10% of the old upfront contribution, minimum $25K**, and settles remaining contracted program installments. These are authored game assumptions. Historic spending is never refunded. Old benefits stop; the replacement has at least one construction/transition year, followed by its configured establishment period. Previous accepted reports remain unchanged.
- 1, 3, 5 or 10 decision years are offered before play. The same ten annual transitions, conditions, payments, reports and save boundaries run afterward. The engine refuses new proposals outside the chosen decision horizon.
- Living Margin data remains interpretable for historical saves; it is removed from the playable catalog and shortcut. No definition hash change or historical reinterpretation is needed.

## Current verification

- New model/regression harness: 277 assertions, all three dense-parcel landscapes. Includes $5M apartments with $5.5M cash, one-cent-short rejection, conversion preview/undo/redo/pending reload/exactly-once payment, establishment, immutable history, all four decision horizons, full-land skip equivalence and Year 10 reload. Full-land stress uses an explicit $100M fixture to isolate the completion path from strategy financing.
- Full-game regression: 2,810 assertions including 51 strategy/order stress routes and interrupted/reloaded future progression. Assertions now check exact carried debt rather than the obsolete full-term-reserve/no-debt assumption.
- Chapter service: 50 checks pass, including old Unity serialized pending requests and hashes; no real provider called.

UI integration, Unity/browser tests and final main delivery remain pending. Do not treat model checks as visual acceptance. Patterns TN-05/06/08, CS-01/02/04 and AX-01 apply. This is bounded gameplay/UI work, not scenery/Phase 2A acceptance.
