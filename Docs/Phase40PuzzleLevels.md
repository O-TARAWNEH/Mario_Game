# Phase 40 — Puzzle Levels (Echo Caverns + Lantern Lockworks)

## Goal

Add two campaign levels focused on **puzzles to solve**, plus lightweight puzzle props.

## New mechanics

| Script | Puzzle role |
|--------|-------------|
| `TimedPlatform` | Solid blinks on/off — time your jumps |
| `PressureSwitch` | Stand to open linked gates (hold / latch / timed latch) |
| `GateBarrier` | Blocks the path until a switch opens it |

## New levels

| # | Scene | Idea |
|---|--------|------|
| 4 | `Level_04_EchoCaverns` | Timing cavern — blink platforms, bounce sync, mover gap, Speed Burst fire dash |
| 5 | `Level_05_LanternLockworks` | Switch lockworks — pressure gates, timed latch, Glow Shield fire hall, moving spike finale |

## Setup

`Bounder Trail → Phase 40 → Setup Puzzle Levels`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase40PuzzleLevelsSetup.SetupPuzzleLevels`

Creates scenes (from Gameplay template), LevelData, catalog entries, puzzle prefabs, build settings, camera/HUD polish.

## Acceptance

- [ ] Level Select shows 5 levels
- [ ] Clearing L3 unlocks Echo Caverns
- [ ] Timed platforms blink and carry jumps when solid
- [ ] Pressure switches open gate barriers
- [ ] Both new scenes are in Build Settings
