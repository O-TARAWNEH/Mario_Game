# Phase 26 — Visual Effects and Game Juice

## Goal

Make gameplay feel satisfying with short, readable feedback.  
Effects must clarify actions — not decorate the screen.

## Design rules

| Rule | Choice |
|------|--------|
| Style | Sprite bursts + light camera shake (Phase 25 FX art) |
| Particles | **Not used** (TechSpec: avoid unnecessary ParticleSystems) |
| Intensity | Brief, capped (`SimpleBurstVfx` max 12 alive) |
| Gameplay | Event hooks only — no combat/movement/timing changes |

## Effects map

| Moment | Feedback |
|--------|----------|
| Jump | Small dust puff at feet |
| Land | Dust puff |
| Hurt (non-lethal) | Hit ring + short shake |
| Death | Larger ring + stronger shake |
| Collectible | Sparkle at pickup position |
| Enemy defeat | Dust + sparkle |
| Power-up | Sparkle + tinted ring |
| Level complete | Green ring + sparkle + mild shake |

Existing systems kept: hurt flash, collect/power pop, power-up tint, SFX.

## Scripts

| File | Role |
|------|------|
| `CameraShake2D.cs` | Trauma shake offset |
| `CameraFollow2D.cs` | Applies shake after follow (SmoothDamp-safe) |
| `SimpleBurstVfx.cs` | Scale/fade sprite burst |
| `PlayerVisualJuice.cs` | Player event → burst/shake |
| `EnemyDefeatVisualJuice.cs` | Enemy `Died` → burst |
| `GameplayVisualJuice.cs` | Collect + level complete |

## Explicitly not added

- ParticleSystem stacks, trails, bloom, hitstop, slow-mo
- Screen-filling flashes that hide hazards/enemies
- Juice that changes knockback, i-frames, or jump height

## Setup

`Bounder Trail → Phase 26 → Setup Visual Effects And Juice`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase26VisualEffectsSetup.SetupVisualEffectsAndJuice`
