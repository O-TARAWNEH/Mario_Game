# Phase 3 — Player Foundation

## Goal

Basic playable character (Pip) with move, jump, ground detection, and spawn.

## Scripts

| File | Role |
|------|------|
| `Assets/Scripts/Player/PlayerController.cs` | Move, stop, jump, facing, gravity/fall clamp |
| `Assets/Scripts/Player/PlayerGroundSensor.cs` | Ground overlap check |
| `Assets/Scripts/Player/PlayerSpawnPoint.cs` | Spawn marker |

## Prefab

- `Assets/Prefabs/Player/Player_Pip.prefab`

## Default tuning

| Variable | Default |
|----------|---------|
| Move Speed | 7 |
| Acceleration | 60 |
| Deceleration | 70 |
| Jump Force | 14 |
| Gravity | 3.5 (Rigidbody2D gravityScale) |
| Maximum Fall Speed | 20 |

## Controls

- A / D or Left / Right — move
- Space (Jump) — jump when grounded

## Scene content

Gameplay scene includes:
- `PlayerSpawn`
- `Player_Pip`
- Ground + platforms for testing
