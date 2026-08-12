# Phase 5 — Player Animation

## Goal

Connect Pip's visuals to gameplay states.

## Animations

- Idle
- Walk
- Run
- Jump
- Fall
- Land
- Death

## Scripts

| File | Role |
|------|------|
| `PlayerAnimator.cs` | Sets Animator params from movement/ground/death |
| `PlayerDeath.cs` | Minimal death state (not full combat/health) |
| `PlayerController.cs` | Exposes `IsRunning` / velocity helpers |

## Animator parameters

- `Speed` (float)
- `IsGrounded` (bool)
- `IsJumping` (bool)
- `IsFalling` (bool)
- `IsRunning` (bool)
- `IsDead` (bool)
- `Land` (trigger)
- `Die` (trigger)

## Assets

- Controller: `Assets/Animations/Player/Anim_Pip.controller`
- Clips: `Assets/Animations/Player/Anim_Pip_*.anim`
- Placeholder frames: `Assets/Art/Player/Pip_*_#.png`

## Debug

- `K` — trigger death animation (disables movement)
