# Phase 16 — Level Completion

## Goal

End-of-level flow: detect the goal, enter Level Complete, then continue or leave.

## How the player completes a level

1. Reach a **goal** (`LevelExitDoor` or `LevelEndPoint`) while alive.
2. `LevelCompletionService` locks completion (one-shot).
3. Player movement freezes.
4. After a short delay → `GameStateId.LevelComplete` (overlay UI).

Leaving the goal trigger afterward does **not** cancel completion.

## What happens after completion

Level Complete panel offers:

| Button | Action |
|--------|--------|
| **Next Level** | Loads next `LevelCatalog` entry |
| **Finish** | Shown when no next level → Main Menu |
| **Restart** | Reloads current level |
| **Main Menu** | Returns to menu |

## What happens if the player leaves

| Situation | Result |
|-----------|--------|
| Walks out of goal trigger after touching it | Still completed (sticky) |
| Quits to Main Menu | Run discarded; no mid-level save here |
| No next catalog level | Finish → Main Menu |
| Scene unload / restart | Completion state resets on new load |

## Scripts

| File | Role |
|------|------|
| `LevelCompletionService.cs` | Goal authority, freeze, delay, Level Complete state |
| `LevelExitDoor.cs` / `LevelEndPoint.cs` | Goal detection → service |
| `LevelLoader.HasNextLevel` / `TryLoadNextLevel` | Next-level loading |
| `GameStateManager.ProceedToNextLevel` | Continue / finish campaign |
| `GameplayFlowController` | Next Level / Finish button |

## Setup

`Bounder Trail → Phase 16 → Setup Level Completion`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase16LevelCompletionSetup.SetupLevelCompletion`
