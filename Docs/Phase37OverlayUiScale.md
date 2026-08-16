# Phase 37 — Overlay UI Scale (Level Complete / Pause / Game Over)

## Goal

Make the **Level Complete** screen (and matching overlays) large and readable — same scale language as the Phase 34 main menu.

## Problem

Overlay titles were ~44pt, summary text ~22pt, buttons 240×54. On full screen that reads as tiny “prototype UI,” especially when going level → next level.

## Deliverables

| Element | Target |
|---------|--------|
| CanvasScaler | Ref **1280×720**, match **0.5** (all gameplay canvases) |
| Overlay title | Font **72**, box ~1100×100, y ≈ **220** |
| Run summary | Font **36**, box ~900×110, centered below title |
| Overlay buttons | **520×76**, label font **34** |
| Layout | Next / Restart / Main Menu stacked with clear gaps |

Also scales **Game Over** and **Pause** overlays so they match.

## Setup

`Bounder Trail → Phase 37 → Setup Overlay UI Scale`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase37OverlayUiScaleSetup.SetupOverlayUiScale`

## Explicitly unchanged

- Gameplay rules, level layouts, player feel tuning
- Main menu (already Phase 34)

## Acceptance

- [ ] Level Complete title and “Next Level” are clearly readable at 1080p
- [ ] Coins / Score summary is large and centered
- [ ] Pause and Game Over buttons match the same size language
