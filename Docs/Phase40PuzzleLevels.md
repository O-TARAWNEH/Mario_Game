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
| 4 | `Level_04_EchoCaverns` | Solid cavern route (always completable) + optional blink secret path, mover assist, Speed Burst fire gate |
| 5 | `Level_05_LanternLockworks` | Switch lockworks — pressure gates, timed latch, Glow Shield fire hall, moving spike finale |

## Setup

`Bounder Trail → Phase 40 → Setup Puzzle Levels`

Rebuild layouts only (no catalog/camera pass):

`Bounder Trail → Phase 40 → Rebuild Puzzle Levels Only`

Rebuild one level:

`Bounder Trail → Phase 40 → Rebuild Echo Caverns Only`

`Bounder Trail → Phase 40 → Rebuild Lantern Lockworks Only`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase40PuzzleLevelsSetup.SetupPuzzleLevels`

`-executeMethod BounderTrail.EditorTools.Phase40PuzzleLevelsSetup.RebuildPuzzleLevelsOnly`

Creates scenes (from Gameplay template), LevelData, catalog entries, puzzle prefabs, build settings, camera/HUD polish.

## Echo Caverns design rules

- Main path uses **solid stepping pads** with walk-jump gaps (~1.8–2.5) and gentle rises
- Blink pads are **optional** (secrets / shortcuts), never the only route
- Combat is light: **one Crawlbug** on a wide shelf
- Spikes / fire / checkpoints / exit sit **on platforms**
- Template leftovers (`Ground_Main`, `Platform_*`, `Marker_*`, sample enemies) are cleared on rebuild

## Lantern Lockworks design rules

- Rooms are linked by **solid bridges** (blink pads optional for coins only)
- Switches use **LatchTimed / LatchPermanent** (HoldWhileStanding is not solo-viable)
- Combat is light: **one Crawlbug** away from the switch
- Single fire patch with Glow Shield — no moving-spike finale gauntlet

## Acceptance

- [ ] Level Select shows 5 levels
- [ ] Clearing L3 unlocks Echo Caverns
- [ ] Echo Caverns has a continuous solid path to the exit
- [ ] Timed platforms blink and carry jumps when solid
- [ ] Pressure switches open gate barriers (Level 5)
- [ ] Both scenes no longer contain Gameplay-template enemy stacks
- [ ] Both new scenes are in Build Settings
