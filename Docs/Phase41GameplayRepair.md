# Phase 41 — Gameplay Repair (Pip Restored + Physics + Resume Fix)

## Goal

1. Keep Pip as the playable character (knight art was too large / wrong scale — reverted).
2. Stop mid-air floating by rebuilding level layouts and hardening platform/player physics.
3. Make spikes feel like hazards (trigger damage, never solid ground).
4. Fix `ResumeButton` coroutine error when the pause panel closes.

## Fixes

| Issue | Fix |
|-------|-----|
| ResumeButton inactive coroutine | `UiButtonPunch` skips `StartCoroutine` when the button is already deactivated |
| Floating / desynced props | Re-run Phase 24 + 40 level builds so platforms, enemies, coins, hazards match |
| Soft platforms / wrong layer | `LevelPhysicsSanitizer` forces solid platforms onto Ground + non-trigger colliders |
| Floaty jumps | Stronger fall gravity, less apex hang |
| Oversized knight player | Restored Pip sprite + animator; capsule `0.7×0.95` |
| Spikes feel soft | Hazard prefabs forced to trigger ContactDamage with a reliable hitbox |

## Setup

`Bounder Trail → Phase 41 → Setup Gameplay Repair`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase41GameplayRepairSetup.SetupGameplayRepair`

## Acceptance

- [ ] Player looks like Pip (blue blob), not the giant knight
- [ ] Player and ground enemies rest on platforms (no idle mid-air float)
- [ ] Spikes hurt on contact and are not walkable ground
- [ ] Resume from pause does not throw the inactive coroutine error
