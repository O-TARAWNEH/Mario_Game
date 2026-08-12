# Phase 17 — User Interface (Gameplay HUD)

## Goal

Design-spec gameplay HUD plus polished Pause / Game Over / Level Complete screens.

## HUD displays

| Element | Source |
|---------|--------|
| Lives | `RespawnSystem` |
| Health | `PlayerHealth` |
| Coins | `CollectibleCounter` |
| Score | `CollectibleCounter` |
| Level name/number | `LevelLoader` / `LevelRoot` |
| Power-up status | `PlayerPowerUps` (Speed / Shield timers) |
| Pause indicator | Shown while `GameStateId.Pause` |

**Not included:** level timer (not in design).

## Screens

| Screen | Contents |
|--------|----------|
| **Pause** | Resume, Restart, Main Menu + PAUSED |
| **Game Over** | Run summary (coins/score), Restart, Main Menu |
| **Level Complete** | Run summary, Next/Finish, Restart, Main Menu |

## Scripts

| File | Role |
|------|------|
| `GameplayHud.cs` | Live HUD bindings |
| `FlowScreenSummary.cs` | Coin/score on end screens |
| `GameplayFlowController.cs` | HUD visibility + overlay routing |

## Setup

`Bounder Trail → Phase 17 → Setup Gameplay HUD`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase17HudSetup.SetupGameplayHud`
