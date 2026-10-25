# Opening composition correction

Scope: P4 / TN-01, TN-02, TN-09. This is a candidate analysis, not accepted output or a restriction on camera movement.

The seven initial-building site centers average **(437.14, -282.86)**. The terrain ends at x=590. ResetView uses this mean, pitch48, yaw−32, zoom190 and a36° field of view. At16:9, the current perspective frustum reaches approximately326m to the right of its target on the target-height plane. The mean leaves only153m of land, explaining the blank wedge in `game-browse-1728.png`.

Preserve zoom, pitch, yaw and settlement context. For the opening/reset view only, compute the four view-ray intersections with the target-height plane; move the target inward just far enough to keep that projected footprint within the terrain bounds, allowing a small terrain-relief margin. If a viewport is wider than the terrain, center that axis rather than changing zoom or restricting normal camera movement. Do not apply this to pan, focus, close-up or overview.

Analytical16:9 target without margin: x263.6, z−282.9. At1440×900: x285.7. A24m margin would move x to239.6 or261.7 respectively. These figures are provisional plane intersections, not measured rendered ground bounds; terrain elevation and perspective need actual Chrome confirmation. The settlement remains visible to the right of the decision area, while the creek and interior open fields occupy the center/left.

Acceptance: matched opening, focused park and overview at1280×720 and normal desktop; town, at least two actionable fields, creek/bridge and woodland context remain recognizable. No empty wedge dominating the normal view; full pan/zoom remains available. Verify that all preview footprints and local capsules remain readable. If the analytical adjustment obscures landmarks or places them behind HUD, revise from actual output rather than declaring the geometry sufficient.

The current Main owner has been asked for a ResetView-only handoff. No Main/camera source has been changed by this analysis.
