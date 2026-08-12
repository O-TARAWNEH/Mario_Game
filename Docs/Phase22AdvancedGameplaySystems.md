# Phase 22 — Advanced Gameplay Systems

## Goal

Implement **only** additional mechanics defined in the Game Design Specification.  
Do **not** add systems just because other platformers have them.

## Verdict

All design-approved advanced systems were already delivered in earlier phases.  
Phase 22 is a **validation + lock** pass: confirm readiness, document the approved set, and explicitly reject unapproved ideas from the phase prompt.

## Prompt “possible systems” vs design

| Prompt idea | Design status | Project status |
|-------------|---------------|----------------|
| Moving enemies | Approved | Present (Crawlbug, Hopmite, Spikewatch + Phase 10 variants) |
| Special platforms | Approved | Present (solid, one-way, moving, bounce) |
| Moving objects | Approved | Present (moving platforms + moving spikes) |
| Temporary abilities | Approved | Present (Speed Burst, Glow Shield, Heart Drop) |
| Doors (level exit) | Approved | Present (`LevelExitDoor`) |
| Secret areas | **Not approved** | **Not added** |
| Hidden collectibles | **Not approved** | **Not added** |
| Switches | **Not approved** | **Not added** |
| Switch-gated doors | **Not approved** | **Not added** |
| Water | **Not approved** | **Not added** |
| Special movement (wall-jump/dash/etc.) | **Not approved** | **Not added** |

## Approved systems (locked)

See `ApprovedGameplayCatalog.cs`:

- Player move/run/jump, damage/respawn, three power-ups
- Patrol / jump / fly / stationary / shooter enemies
- Platforms, bounce pads, pits, spikes, fire, moving hazards
- Checkpoints, exit goal, coins

## Deliverables

| Item | Role |
|------|------|
| `ApprovedGameplayCatalog.cs` | Explicit approved / rejected lists |
| `Phase22AdvancedSystemsSetup.cs` | Prefab/script validation |
| This doc | Design lock for content pass |

## Setup

`Bounder Trail → Phase 22 → Validate Advanced Gameplay Systems`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase22AdvancedSystemsSetup.ValidateAdvancedGameplaySystems`

## Next

Phase 24 authors unique Level 1–3 layouts (no secrets).  
Level layout / difficulty curve uses **only** the approved prefab set above.
