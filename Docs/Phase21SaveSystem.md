# Phase 21 — Save System

## Goal

Retain player progress safely across sessions.

## Saved data

| Category | Fields |
|----------|--------|
| Levels completed | `completedMask` |
| Unlocked levels | `highestUnlockedLevelIndex` |
| Progression | `continueLevelIndex`, `hasCampaignSave` |
| Collectibles | `bestCoins`, `bestScore` |
| Settings | Master / Music / SFX volumes |

## API

| Action | Behavior |
|--------|----------|
| **Load** | Primary file → backup → legacy PlayerPrefs migrate → defaults |
| **Save** | Temp write → verify → update backup (never wipe backup on failure) |
| **New Game** | Reset campaign unlocks/continue; keep settings + career bests |
| **Reset Save** | Wipe campaign + bests; keep audio settings |

## Fail-safe design

- Files: `bounder_trail_save.json` + `.bak.json` in `persistentDataPath`
- Checksum (FNV-1a) on payload
- Write via `.tmp` then replace
- Corrupt primary restores from backup
- Failed write leaves backup untouched

## Scripts

| File | Role |
|------|------|
| `SaveData.cs` | Versioned payload |
| `SaveSystem.cs` | Load / Save / NewGame / ResetSave |
| `GameProgress.cs` | In-memory campaign state (persisted by SaveSystem) |

## UI

Settings → **Reset Save**

## Setup

`Bounder Trail → Phase 21 → Setup Save System`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase21SaveSystemSetup.SetupSaveSystem`
