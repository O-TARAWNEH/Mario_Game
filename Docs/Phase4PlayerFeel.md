# Phase 4 — Player Physics and Game Feel

## Goal

Make Pip's movement feel responsive and polished.

## Systems added

- Walk / run speeds
- Separate ground & air accel/decel + air control
- Coyote time
- Jump buffering
- Variable jump height (early release cut)
- Fall gravity multiplier + max fall speed
- Multi-probe ground/edge checks
- Mild slope projection + ground stick

## Controls

| Input | Action |
|-------|--------|
| A/D or Arrows | Move |
| Left Shift | Run |
| Space | Jump (hold for higher jump, tap for short hop) |

## Default tuning (`Player_Pip`)

| Property | Value |
|----------|-------|
| Walk Speed | 6.5 |
| Run Speed | 9.5 |
| Acceleration | 75 |
| Deceleration | 85 |
| Air Acceleration | 45 |
| Air Deceleration | 40 |
| Air Control | 0.75 |
| Jump Force | 15 |
| Coyote Time | 0.10s |
| Jump Buffer | 0.12s |
| Jump Cut Multiplier | 0.45 |
| Gravity | 3.2 |
| Fall Gravity Multiplier | 1.55 |
| Max Fall Speed | 22 |

## Files

- `Assets/Scripts/Player/PlayerController.cs` *(modified)*
- `Assets/Scripts/Player/PlayerGroundSensor.cs` *(modified)*
- `Assets/Editor/Phase4PlayerFeelSetup.cs` *(created)*
