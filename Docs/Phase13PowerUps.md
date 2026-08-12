# Phase 13 — Power-Ups

## Goal

Implement the design-spec power-up system only (no random or out-of-spec powers).

## Design lock

From Game Design Specification:

| Power-up | Behavior |
|----------|----------|
| **Speed Burst** | Temporary move speed increase |
| **Glow Shield** | Temporary invincibility |
| **Heart Drop** | Restore **1 HP** (clamped to max health; not a life/stock yet) |

Not implemented (not in design): jump boost, attack power-up, size transform.

## Scripts

| File | Role |
|------|------|
| `PowerUpKind.cs` | SpeedBurst / GlowShield / HeartDrop |
| `PowerUpPickup.cs` | Placeable pickup: detection, collect state, effect, sound |
| `PlayerPowerUps.cs` | Activation, duration timers, removal, state events |
| `PlayerPowerUpFeedback.cs` | Speed/shield tint while active |
| `PlayerController.SetSpeedMultiplier` | Speed Burst hook |
| `PlayerHealth.SetGlowShield` | Glow Shield invincibility hook |

## Prefabs

- `Assets/Prefabs/Items/Item_SpeedBurst.prefab`
- `Assets/Prefabs/Items/Item_GlowShield.prefab`
- `Assets/Prefabs/Items/Item_HeartDrop.prefab`

Tag: `PowerUp` · Layer: `Pickup`

## Defaults

| Setting | Value |
|---------|-------|
| Speed Burst duration | 5 s |
| Speed Burst multiplier | 1.45× |
| Glow Shield duration | 5 s |
| Heart Drop heal | +1 HP |

## State rules

- Timed effects refresh duration if picked up again while active.
- Speed Burst and Glow Shield can be active together.
- All timed power-ups clear on player death.
- Heart Drop is instant (no duration).

## Setup

`Bounder Trail → Phase 13 → Setup Power-Ups`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase13PowerUpsSetup.SetupPowerUps`
