---
name: terra-community-import
description: Find representative, reusable satellite or aerial imagery for a named community, align it with real elevation, import it into Terra's community browser, and verify the geographic match. Use for new community terrain imports or fidelity reviews, not invented scenery or annual simulation tuning.
---

# Terra community import

Turn a place name into a reviewable, geographically aligned in-game terrain study. The current adapter downloads public-domain US NAIP aerial orthophotos and USGS 3DEP elevation. Aerial photography is not satellite imagery; label the sensor/product accurately. Outside the continental US, research a suitable provider and extend the adapter deliberately rather than substituting unrelated geography.

## Resolve the place and choose a useful window

Read the target Terra checkout's AGENTS.md, TERRA_PROJECT_SPEC.md, and docs/COMMUNITY_IMPORT.md. Follow its isolated-worktree and shared-tool coordination rules. The maintained importer is `worldgen/community.py`; do not copy a stale version into this skill.

Resolve the user's location through web research, using a municipal or geographic authority where possible. Distinguish town, county, region and similarly named places. Ask only if ambiguity materially changes the place. Record the evidence URL and coordinates. For an unspecified crop, start with a 1.2 km square containing recognizable community features and nearby rural land; inspect it and adjust if the town is cut off or the image is unrepresentative. One photo does not represent an entire county or its people.

Verify download/reuse rights, imagery dates, source resolution and elevation coverage. Prefer free licensed government sources. Do not extract Google/Esri commercial basemaps merely because they display in a browser. No image generation or synthetic replacement for missing geography. For provider details and tested commands read [the adapter guide](references/adapter.md).

## Import and review

1. Run the importer with the resolved name, center, region, location-evidence URL and a fresh versioned ID. It preserves request URLs, raster IDs, source metadata and SHA-256 hashes. Existing IDs are immutable: revise under a new ID.
2. Inspect the downloaded image. Reject clouds, missing strips, wrong place, poor season/contrast or insufficient detail for the requested scale. Record source acquisition separately from retrieval; exported pixels do not increase native resolution.
3. Verify projected meters, equal terrain/image extent, grid orientation and at least three recognizable landmarks (for example an intersection, a field corner and a stream bend). Source rows run north-to-south; game heights run south-to-north. Avoid double flips, stretched textures, invented height exaggeration and estimated tree/building positions presented as surveys.
4. Stage with `python3 scripts/prepare-communities.py`. The existing native atlas browser consumes its catalog; no hosted AI service, API key or network GIS lookup is required to view prepared packs. Build preprocessing stages the same files automatically.
5. Run offline validation, importer failure tests and Unity `CommunityChecks.Run()`. In actual Chrome, open **Your community +**, load the location, compare **Source photo / Overhead / Terrain**, rotate/zoom, change location, close and reopen. Verify original annual destinations and saved runs remain independent. Preserve captures and actual test results in task evidence.

## Fidelity and handoff

The implemented import is a photograph draped on measured terrain at metric scale. This gives close plan-view correspondence, not reconstructed roofs, tree volumes, editable land cover, legal parcels or a calibrated local economy. Keep those limits visible. Do not promote a terrain study to an annual scenario by copying another place's land use, water network, opportunities or numerical assumptions.

If the user requests a playable scenario, separately source/author the required WorldPack layers, validate them with the existing loader and annual contracts, and test interactions. A terrain-study import alone is not that deliverable. Preserve original saves and source provenance.

Deliver the installed skill, imported place IDs, exact data/source dates, in-game entry path, geographic checks and browser evidence. Clearly distinguish observed fidelity, unresolved gaps and any unperformed verification. Update the project vision only for the user's explicit scope change; do not mark the existing visual gate complete because an orthophoto matches itself.
