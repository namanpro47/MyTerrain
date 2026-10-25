# Regional hazard research

This folder is a research handoff, not runtime configuration.

- [Research and numeric-result flow](../../docs/REGIONAL_HAZARDS.md).
- `regional-hazards.research.json`: source-backed candidate hazards and original likelihood measures; engine probabilities remain null pending a suitable threshold/area/time contract.
- `sources.research.json`: inspected source URLs, source scope and downloaded PDF hashes/page locators.
- `wisconsin-weather-proxies.research.json`: observed counts derived from the existing pinned weather pack. These are weather conditions, not natural-disaster damage probabilities.
- `validation.research.json`: consistency checks, including independent reproduction of all four counts from the daily companion, source-ID resolution and disabled/null runtime profiles.

No code loads this folder. Preserve null versus zero; do not enable a profile by coercing a planning risk rating or a multi-year bound into an exact annual site-damage percentage. Shared Wisconsin crop-weather/economic references remain separate from the proposed regional hazard catalogue.

Downloaded PDFs, extracted text and source-table renders stay in ignored `research-cache/`. Source URLs/hashes and exact qualifying years remain reviewable in the committed research files. Research date: October 25, 2026.
