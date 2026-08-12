# Phase 14 — Hazards

## Goal

Environmental dangers that detect the player, apply damage/death, and reset cleanly.

## Hazard set (kept minimal)

| Prefab | Response | Notes |
|--------|----------|-------|
| `Hazard_DeathZone` | Instant kill | Pit / void under gaps |
| `Hazard_Spikes` | Contact damage (1) | Static spikes |
| `Hazard_Fire` | Damage over time (1 / 0.45s) | Ember / flame equivalent |
| `Hazard_MovingSpike` | Contact damage + path move | Simple moving hazard |

No extra hazard types beyond this set.

## Scripts

| File | Role |
|------|------|
| `HazardResponse.cs` | InstantKill / ContactDamage / DamageOverTime |
| `EnvironmentalHazard.cs` | Player detection + consequences |
| `MovingHazard.cs` | A↔B kinematic motion + enable reset |

## Rules

- **Pits:** `PlayerDeath.Die()` immediately (voids bypass Glow Shield / i-frames).
- **Spikes / Fire / Moving:** `IDamageable.TakeDamage` — respects hurt i-frames and Glow Shield.
- **Fire:** damages on an interval while the player remains inside.
- **Reset:** `OnEnable` clears overlap/timers; moving hazards return to start pose.

## Setup

`Bounder Trail → Phase 14 → Setup Hazards`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase14HazardsSetup.SetupHazards`
