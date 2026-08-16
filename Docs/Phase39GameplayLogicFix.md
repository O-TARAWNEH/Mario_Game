# Phase 39 — Gameplay Logic + Display Warning Fix

## Goal

1. Fix Level 3 platforms / pickups / checkpoints being out of sync.
2. Remove the red Pixel Perfect resolution warning in the Game view.

## Root causes

| Issue | Cause |
|-------|--------|
| Floating checkpoint / hard-to-reach coins | `LevelPhysicsSanitizer` resized/moved L3 bridges at runtime while coins/checkpoints stayed at old scene positions |
| Checkpoint_B mid-air | Phase 24 placed it at landing X/Y instead of on `Bridge_C` |
| Red top text | URP `PixelPerfectCamera` OnGUI when Game view &lt; reference resolution |

## Fixes

- Sanitizer only strips tilemap physics / duplicate solids (no platform rewrite)
- Phase 24: `Checkpoint_B` on `Bridge_C`; bounce pad on `Bridge_C`
- Disable `PixelPerfectCamera`; keep stable orthographic size **7.5**
- Coin / power-up `OnTriggerStay2D` so overlaps still collect
- Squash disabled when wired to the physics root (avoids shrinking colliders)

## Setup

`Bounder Trail → Phase 39 → Setup Gameplay Logic Fix`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase39GameplayLogicFixSetup.SetupGameplayLogicFix`

## Acceptance

- [ ] Level 3 platforms match coin/checkpoint positions
- [ ] No red resolution warning in Game view
- [ ] Walking into coins / power-ups collects them
