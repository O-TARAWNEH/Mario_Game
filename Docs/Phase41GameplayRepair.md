# Phase 41 — Gameplay Repair (Knight + Physics + Resume Fix)

## Goal

1. Replace the blue Pip blob with the knight art (`Knight_Image.png`).
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
| Blue blob player | Cropped knight sprite on `Visual` child; Pip animator disabled |
| Spikes feel soft | Hazard prefabs forced to trigger ContactDamage with a reliable hitbox |

## Setup

`Bounder Trail → Phase 41 → Setup Gameplay Repair`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase41GameplayRepairSetup.SetupGameplayRepair`

Requires `Knight_Image.png` in the project root.

## Acceptance

- [ ] Player looks like the knight (not a blue blob)
- [ ] Player and ground enemies rest on platforms (no idle mid-air float)
- [ ] Spikes hurt on contact and are not walkable ground
- [ ] Resume from pause does not throw the inactive coroutine error
