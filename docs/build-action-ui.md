# Categorized build actions

Peter requested the 17 PRD actions and their models in the playable UI, grouped into building/land-use categories, and asked why water management is restricted. This implementation starts from refreshed `origin/main` at `1bcc254` in an isolated worktree. It is a separately authorized catalog/placement integration, not Phase 2A acceptance or a calibrated simulation of the wider PRD.

## Player flow

Five tabs replace the original three illustrated tools:

- **Farming (4):** traditional farmland, cover crop, conservation rotation, living margin.
- **Residential (2):** low-density homes, high-density apartments.
- **Community (2):** public park, village center.
- **Utilities (7):** vegetated swale, agricultural storage, data center, gas power, solar, wind, wastewater treatment.
- **Nature (2):** ground-cover restoration, conservation/stewardship landscape treatment.

Every action remains selectable, including a model that currently fits no parcel. Cards show the matching prefab-rendered icon and the number of physically/scenario-eligible fields, before budget. Selecting an action shows its metric size; inspecting a field shows the actual site price and rejection reason. Valid previews use the same model and occupied-footprint outline as placement. A new action replaces the current proposal for that parcel, with the existing replacement refund and undo behavior.

Click tabs/cards, or press **B** to navigate the catalog with Tab/arrows and Enter. Enter on an action returns keyboard focus to the map; Escape leaves catalog navigation. Existing 1/2/3/M shortcuts still select the original treatments. Map Tab/Enter/F/Z retain inspect/place/focus/undo behavior. Modal input remains isolated.

## Actual land constraints

The current source does not define three uniform terrain types with automatic capabilities. It has three useful editing classes:

1. Ten **authored editable field polygons**, ranging from 4,000 to 6,750 m². All previously supported open cultivation, living margins and meadow restoration.
2. Two **protected scenario zones**: Wooded slope (19,500 m²) and Stream corridor (7,000 m²). Their protection is an authored gameplay rule, not a legal designation; all interventions remain blocked.
3. The remaining **background landscape**, which is real terrain/cover scenery but has no placement-site ID. It cannot receive these parcel-level commands.

Before this change, `worlds/meadowbrook-v1/world.json` explicitly included `water-swale` only in Meadow edge (`south-07`) and Brye-side field (`east-09`). This whitelist and each site's isolated `site-only-…` connection ID are authored scenario assumptions. Eligibility was not calculated from surface water, soil, slope or drainage. There are two eligible sites in current main, even if only one was apparent in a particular camera/build. The catalog now exposes both the count and the named restriction. No runoff simulator is connected.

The wider PRD catalog is enabled for the editable fields where the asset fits, with additional physical checks below. The swale whitelist and both protected zones remain intact. Wastewater treatment is a separate illustrative facility, not a swale, drinking-water source or verified service connection.

## Metric placement

`BuildActionRules` gets widths/depths directly from the art catalog. It considers two orthogonal orientations and a small deterministic set of centers inside a field; it never rescales a structure. Every footprint corner must fit the WorldPack's validated convex polygon, which also contains the complete rectangle edges and interior. It samples the actual triangulated terrain including grid and diagonal intersections.

Rigid assets remain upright and level. A terrain-following foundation skirt fills the bounded gap below their pad. The prototype allows at most **3 m of elevation range across the occupied footprint**; larger slopes are rejected with their measured range and a suggestion to choose a flatter field. This is an authored art/gameplay limit, not an engineering rule or permission to grade real land. Candidate search is deliberately small; rejection means no candidate in this prototype's search meets the rule, not proof that no engineering arrangement is possible.

Landscape samples are conformed to the terrain at their original metre scale and surrounded by the site's treatment ground surface. They illustrate the full parcel treatment; they do not claim each decorative plant is mapped or every part of the land is represented at that detail. Swale geometry remains an illustrative surface treatment, not terrain excavation or a drainage computation.

The 150 × 150 m utility-wind site does not fit any of the current fields. It is visible and selectable with an explicit rejection; the turbine is not shrunk to fit. A larger eligible parcel would be a separate authored map decision. This does not turn the protected hillside into a wind site.

## Accounting and scope

The four existing treatments retain their original per-site offers. New actions use disclosed prototype costs: Farming 26 credits, Residential 30, Community 22, Utilities 36, Nature 14. These are game assumptions, not construction quotes, taxes or public-finance estimates. New farming practices retain the site's baseline farm output; other new actions remove its illustrative farm output. The existing one-proposal-per-site accounting treats a project as the parcel's selected use; the model's smaller physical footprint is not a partial-parcel agricultural simulation. No jobs, housing occupancy, electricity, treatment capacity, stewardship outcomes or annual score bonuses are inferred from the art.

The runtime expands **copies** of the canonical WorldPack, so the original geographic files, legacy fixtures and separate annual Editor playtest keep their contracts. `WorldStateStore` accepts an optional placement rejection callback; preview and commit both enforce it, so UI feedback cannot bypass the rules. Future annual integration must supply explicit supported recipes rather than implicitly using these prototype prices.

The main scene references `BuildActionCatalog.asset`; `Phase1Setup.CreateScene` also sets that reference. Existing renderer cleanup now destroys only its owned generated meshes, preserving prefab shared meshes during repeated ghost previews and undo.

## Verification

Run `BuildActionIntegrationChecks.PrepareAndCheck` to bind the catalog, validate the augmented pack, check all 17 actions against all 12 sites, test protected/water/size/slope/budget restrictions, and verify placement/replacement/undo and original-pack immutability. It also runs the existing Phase 1 and Phase 2 checks on the unchanged canonical data. `CheckAndBuild` then exports the actual Web player. Launch Unity with `-batchmode -buildTarget WebGL -executeMethod BuildActionIntegrationChecks.CheckAndBuild -quit`; selecting the Web target up front is required for the platform-specific keyboard-capture assembly. Results and fresh browser captures belong in `docs/evidence/build-action-ui/`.

Design patterns: **TN-04** (matching menu/world assets), **TN-05** (visible occupied footprint and previews), **CS-02** (specific spatial restrictions), **AX-01** (readable backed text and keyboard controls). Reference quality and browser performance must be reported from the actual build; the earlier contact sheet is not playable-game evidence.
