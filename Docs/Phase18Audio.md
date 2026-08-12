# Phase 18 — Audio System

## Goal

Centralized music and sound effects for Bounder Trail.

## Scripts

| File | Role |
|------|------|
| `AudioManager.cs` | Persistent facade, volume apply, state→music routing |
| `MusicSystem.cs` | Looping BGM (menu / gameplay) |
| `SfxSystem.cs` | One-shot SFX catalog |
| `AudioVolumeSettings.cs` | Master / Music / SFX volumes (PlayerPrefs) |
| `SfxId.cs` / `MusicId.cs` | Catalog keys |
| `PlayerAudioFeedback.cs` | Jump, land, damage, death, power-up hooks |

## Sounds

| Event | `SfxId` | Trigger |
|-------|---------|---------|
| Jump | `Jump` | `PlayerController.Jumped` |
| Landing | `Land` | `PlayerController.Landed` |
| Collecting | `Collect` | `Collectible` → AudioManager |
| Taking damage | `Damage` | `PlayerHealth.Damaged` (non-lethal) |
| Enemy defeat | `EnemyDefeat` | `EnemyHealth.Die` |
| Power-up | `PowerUp` | `PlayerPowerUps.Activated` |
| Death | `Death` | `PlayerDeath.Died` |
| Level completion | `LevelComplete` | `LevelCompletionService` |
| UI interaction | `Ui` | Main menu + flow buttons |

Music: `BGM_Menu` on main menu, `BGM_Gameplay` during play / pause / game over / level complete.

## Volume

`AudioManager` exposes Master / Music / SFX (0–1). Values persist via PlayerPrefs.

## Assets

- `Assets/Audio/SFX/SFX_*.wav` — placeholder one-shots
- `Assets/Audio/Music/BGM_Menu.wav`, `BGM_Gameplay.wav` — placeholder loops

## Setup

`Bounder Trail → Phase 18 → Setup Audio System`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase18AudioSetup.SetupAudioSystem`
