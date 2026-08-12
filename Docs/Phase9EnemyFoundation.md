# Phase 9 — Enemy Foundation

## Goal

Flexible enemy architecture for multiple future types. One example patrol enemy (`Crawlbug`) validates the system.

## Architecture

| Script | Role |
|--------|------|
| `IDamageable` | Shared damage interface |
| `EnemyStateId` | Idle/Patrol/Chase/Attack/Hurt/Dead |
| `EnemyHealth` | HP, hurt, death |
| `EnemyMover` | Move, flip, ledge/wall checks |
| `EnemySensor` | Player detection |
| `EnemyBrain` | State machine with behavior toggles |
| `EnemyContact` | Stomp vs side-hit rules |

## Example enemy

- Prefab: `Assets/Prefabs/Enemies/Enemy_Crawlbug.prefab`
- Default: **Patrol only** (`canChase`/`canAttack` off)
- Enable chase/attack later per type via Inspector toggles on `EnemyBrain`

## Contact rules

- Stomp from above → enemy takes damage (dies at 0 HP), player bounces
- Side hit → player `TakeDamage` (Phase 11: `PlayerHealth` HP, knockback, i-frames; death at 0 HP)

## Setup menu

`Bounder Trail → Phase 9 → Setup Enemy Foundation`
