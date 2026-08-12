# Phase 8 — Environmental Objects

## Goal

Reusable level-building objects with clear, predictable collision behavior.

## Included

| Prefab | Behavior |
|--------|----------|
| `Platform_Solid` | Blocks from all sides |
| `Platform_OneWay` | Jump through from below, land on top |
| `Platform_Moving` | Moves between two points, carries player |
| `BouncePad` | Launches player upward |
| `LevelExitDoor` | Completes level on enter |

## Intentionally not included

- Breakable platforms
- Ladders

(Not needed for the current classic platformer scope.)

## Scripts

- `Assets/Scripts/World/SolidPlatform.cs`
- `Assets/Scripts/World/OneWayPlatform.cs`
- `Assets/Scripts/World/MovingPlatform.cs`
- `Assets/Scripts/World/BouncePad.cs`
- `Assets/Scripts/World/LevelExitDoor.cs`
- `Assets/Scripts/World/PlatformPiece.cs` *(updated kinds)*

## Setup menu

`Bounder Trail → Phase 8 → Setup Environmental Objects`
