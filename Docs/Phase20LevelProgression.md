# Phase 20 — Level Progression

## Goal

Connect campaign levels into an unlock chain with save-backed progress.

## Campaign structure

```
Level 1 — Lumen Meadows
    |
Level 2 — Cascade Cliffs
    |
Level 3 — Skybridge Spire
```

Completing a level unlocks the next. Level 1 is always unlocked.

## Data / scenes

| Asset | Role |
|-------|------|
| `LevelCatalog.asset` | Ordered campaign list |
| `LevelData_01_LumenMeadows.asset` | Level 1 |
| `LevelData_02_CascadeCliffs.asset` | Level 2 |
| `LevelData_03_SkybridgeSpire.asset` | Level 3 |
| `Level_01_LumenMeadows.unity` | Level 1 scene |
| `Level_02_CascadeCliffs.unity` | Level 2 scene |
| `Level_03_SkybridgeSpire.unity` | Level 3 scene |

## Progress (`GameProgress`)

| Field | Meaning |
|-------|---------|
| Highest unlocked index | Furthest playable level |
| Completed mask | Which levels are cleared |
| Continue index | Last entered level |

## Flow

| Action | Result |
|--------|--------|
| Start Game | Reset progress, load Level 1 |
| Continue | Load saved continue level |
| Level Select | Pick any unlocked level |
| Level complete | Mark cleared + unlock next |
| Next Level | Load next catalog entry (or Finish → menu) |

## Scripts

| File | Role |
|------|------|
| `GameProgress.cs` | Unlock / completion / continue |
| `LevelSelectView.cs` | Level list UI |
| `GameStateManager.StartLevel` | Locked-aware level start |
| `LevelCatalog.IsValidIndex` | Catalog bounds helper |

## Setup

`Bounder Trail → Phase 20 → Setup Level Progression`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase20LevelProgressionSetup.SetupLevelProgression`
