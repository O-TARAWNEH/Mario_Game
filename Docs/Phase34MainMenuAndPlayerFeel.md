# Phase 34 — Main Menu Scale + Player Feel

## Goal

1. Make the main menu **readable at full screen** (was too small).
2. Retune Pip movement toward **Mario-inspired platformer feel** (snappy, not floaty).

## Part A — Main Menu

| Change | Result |
|--------|--------|
| CanvasScaler ref **1280×720**, match **0.5** | UI scales up on 1080p/1440p displays |
| Buttons **520×76**, font **34** | Easier to click/read |
| Title **72**, subtitle **36** | Clear hero text |
| **Settings + Controls → Options** | One button; panel shows audio + control reference |
| Removed root **Controls** button | Less clutter |

Setup: `Bounder Trail → Phase 34 → Setup Main Menu Scale`

## Part B — Player Feel

| Tuning | Before → After (intent) |
|--------|---------------------------|
| Walk / Run | 6.5 / 9.5 → **7.2 / 10.8** (brisker) |
| Ground accel | **92 / 98** (snappier start/stop) |
| Jump force | 15 → **16.2** |
| Fall gravity mult | 1.55 → **2.15** (less floaty) |
| Apex hang | **new** — lighter gravity near jump peak |
| Coyote / buffer | **0.12s** each |

Setup: `Bounder Trail → Phase 34 → Setup Player Feel`

## Explicitly unchanged

- Level layouts, enemies, combat rules
- Camera follow logic (except menu canvas scale)
- Save / progression / audio systems

## Acceptance

- [x] Main menu buttons visibly larger in Play Mode
- [x] Single **Options** entry (no separate Controls on root)
- [x] Options panel shows volumes + controls text
- [x] Pip falls faster, jumps feel snappier, apex hang active
- [x] No gameplay system rewrites beyond feel tuning + menu layout
