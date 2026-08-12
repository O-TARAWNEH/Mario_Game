# Phase 11 — Player and Enemy Interactions

## Goal

Define how the player and enemies hurt each other: damage, stomp, knockback, invulnerability, death, and feedback.

## Scripts

| File | Role |
|------|------|
| `PlayerHealth.cs` | HP, `IDamageable`, knockback, i-frames |
| `PlayerHurtFeedback.cs` | Sprite flash during invulnerability |
| `PlayerDeath.cs` | Death state + optional Game Over (no longer owns damage) |
| `PlayerController.cs` | `LockControl` so knockback is not overwritten |
| `EnemyContact.cs` | Stomp vs side-hit rules |
| `EnemyHealth.cs` | Enemy HP, hurt i-frames, knockback, death |

## Interaction rules

### Player touches enemy (side / body)

1. Enemy deals `contactDamageToPlayer` (default 1) if `dealContactDamage` is on.
2. Player loses HP, receives knockback away from the enemy, and gains invulnerability frames.
3. Horizontal control is locked briefly so knockback can play out.
4. Sprite flashes while invulnerable.
5. Further contact/projectile hits are ignored until i-frames end.

### Player lands on enemy (stomp)

1. Requires `canBeStomped`, player above the enemy by `stompHeightThreshold`, and falling / not rising.
2. Enemy takes `stompDamageToEnemy` (default 1).
3. Player gets an upward bounce (`stompBounceForce`); player is **not** damaged.
4. Enemies with `canBeStomped = false` (e.g. Spikewatch) treat top contact like a side hit.

### Enemy attacks player

1. Contact damage during chase/attack still goes through `EnemyContact`.
2. Spitter projectiles call `IDamageable.TakeDamage` on the player (`PlayerHealth`).
3. Same knockback / i-frames / death path as contact damage.

### After player takes damage

| HP left | Result |
|---------|--------|
| > 0 | Knockback + control lock + i-frames + flash |
| 0 | `PlayerDeath.Die()` → movement off, death anim, Game Over after delay |

### Enemy death

- HP reaches 0 → colliders off, rigidbody stopped, `Dead` state / anim, destroy after delay.

## Defaults

| Setting | Value |
|---------|-------|
| Player max health | 3 |
| Player i-frames | 1.25 s |
| Player knockback | (6.5, 8) |
| Player control lock | 0.2 s |
| Game Over delay | 0.85 s |
| Enemy hurt / i-frames | 0.2 s |

## Setup

`Bounder Trail → Phase 11 → Setup Combat Interactions`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase11CombatInteractionsSetup.SetupCombatInteractions`

## Out of scope

- Power-ups (Glow Shield, Heart Drop, etc.)
- Boss battles
- Lives / HUD display (later phases)
- Checkpoint respawn
