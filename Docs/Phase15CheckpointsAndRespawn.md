# Phase 15 — Checkpoints and Respawning

## Goal

Reliable player recovery after death using checkpoints.

## Where the player respawns

1. **Last activated checkpoint** respawn point, if any were reached  
2. Otherwise the **level start** `PlayerSpawnPoint`

## Death handling

1. Player enters death state (anim, controls off).
2. Lose **1 life**.
3. Wait `respawnDelay` (default 0.85s).
4. If lives remain → respawn. If **0 lives** → Game Over.

Starting lives: **3** (reset when the level loads).

## What gets reset after death / respawn

| System | Reset? |
|--------|--------|
| Player position | Yes → checkpoint / start |
| Player HP | Yes → full |
| Death / anim state | Yes |
| Active power-ups (Speed/Shield) | Yes → cleared |
| Velocity | Yes → zero |
| Enemies (defeated) | Yes → restored at spawn |
| Moving hazards | Yes → start of path |
| Camera | Snaps to player |

## What remains persistent (within the level run)

| System | Persistent? |
|--------|-------------|
| Collected coins | Yes (do not reappear) |
| Collected power-up pickups | Yes (do not reappear) |
| Coin count / score | Yes |
| Activated checkpoints | Yes |
| Lives remaining | Yes (only decreases until level reload) |

## Scripts

| File | Role |
|------|------|
| `Checkpoint.cs` | Trigger + respawn pose + active visual |
| `RespawnSystem.cs` | Lives, death flow, respawn, world resets |
| `EnemyRespawnState.cs` | Soft-death + restore enemies |
| `EnemyHealth.Revive` | Restore HP/colliders |
| `PlayerAnimator.ResetAfterRespawn` | Clear death anim |

## Prefab

- `Assets/Prefabs/World/Checkpoint_Flag.prefab`

## Setup

`Bounder Trail → Phase 15 → Setup Checkpoints And Respawn`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase15CheckpointsSetup.SetupCheckpointsAndRespawn`
