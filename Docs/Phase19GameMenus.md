# Phase 19 — Game Menus

## Goal

Complete menu flow for Bounder Trail.

## Main Menu

| Action | Behavior |
|--------|----------|
| **Start Game** | New campaign from level 0; creates Continue save |
| **Continue** | Loads saved continue level (disabled until a save exists) |
| **Settings** | Audio volume sliders (Master / Music / SFX) |
| **Controls** | Read-only control reference |
| **Quit** | Exit play mode / application |

## Pause Menu

| Action | Behavior |
|--------|----------|
| Resume | Return to gameplay |
| Restart | Reload current level |
| Settings | Audio sliders (Esc/Back returns to pause) |
| Main Menu | Return to main menu |

## Game Over

| Action | Behavior |
|--------|----------|
| Restart | Reload current level |
| Main Menu | Return to main menu |

Level Complete (from Phase 16/17) remains: Next/Finish, Restart, Main Menu.

## Scripts

| File | Role |
|------|------|
| `MainMenuController.cs` | Root / Settings / Controls navigation |
| `AudioSettingsView.cs` | Volume slider bindings |
| `GameplayFlowController.cs` | Pause settings overlay + existing flow |
| `GameProgress.cs` | Continue progress (PlayerPrefs) |
| `GameStateManager.StartNewGame` / `ContinueGame` | Campaign entry points |

## Setup

`Bounder Trail → Phase 19 → Setup Game Menus`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase19MenusSetup.SetupGameMenus`
