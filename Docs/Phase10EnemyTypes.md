# Phase 10 — Enemy Types

## Goal

Standard enemy types built on the Phase 9 architecture.

## Types

| Prefab | Role |
|--------|------|
| `Enemy_Crawlbug` | Basic walker (patrol) |
| `Enemy_Dartling` | Fast walker |
| `Enemy_Hopmite` | Jumping patrol |
| `Enemy_Skimmer` | Flying bob + horizontal patrol |
| `Enemy_Spikewatch` | Stationary, cannot be stomped |
| `Enemy_Spitter` | Stationary shooter + projectile |
| `Enemy_Projectile` | Spitter ammo |

## Shared modules added

- `EnemyFlyer`
- `EnemyJumper`
- `EnemyShooter`
- `EnemyProjectile`
- `EnemyAnimator`
- `EnemyContact` now supports `canBeStomped` / `dealContactDamage`

## Animation

- Controller: `Assets/Animations/Enemies/Anim_Enemy.controller`
- Driven by `EnemyAnimator` (`State`, `Speed`, `IsDead`, `Hurt`, `Attack`)

## Setup

`Bounder Trail → Phase 10 → Setup Enemy Types`
