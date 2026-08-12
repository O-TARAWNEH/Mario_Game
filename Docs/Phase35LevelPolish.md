# Phase 35 — Campaign Level Polish

## Goal

Make the three campaign levels readable, fair, and progressively harder — with hazards anchored to platform geometry and a wider gameplay viewport.

## Deliverables

| Area | Change |
|------|--------|
| **Layouts** | Re-authored L1–L3 via Phase 24 helpers (pits between edges, spikes on shelf tops, fire on platforms) |
| **Difficulty** | L1 tutorial trail → L2 stepped cliffs → L3 sky bridges + movers + finale spike |
| **Visuals** | Per-level decor clusters, ground tile strips under paths, larger parallax backdrops |
| **Viewport** | Orthographic **7.5** (ref 640×480 @ 32 PPU) — ~33% more visible area than 5.5 |
| **Collision** | Platforms use world-size colliders (no transform scale); tilemaps are visual-only (no physics) |

## Setup

`Bounder Trail → Phase 35 → Setup Level Polish`  
(rebuilds layouts, refreshes visuals/backdrops, applies camera)

## Validate

`Bounder Trail → Phase 35 → Validate Level Polish`

## Acceptance

- [ ] L1: one taught gap, spikes only at exit ramp
- [ ] L2: pits align to shelf gaps; fire gate on shelf D
- [ ] L3: void pit under bridges; moving spike before exit
- [ ] Campaign scenes use orthographic size 7.5 + Pixel Perfect 640×480
- [ ] Coin lines guide the route; decor visible on each level

## Explicitly unchanged

- Player feel tuning (Phase 34)
- Main menu scale (Phase 34)
- Enemy/combat balance scripts (Phase 27 values remain unless re-run)
