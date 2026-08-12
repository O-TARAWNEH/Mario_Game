# Phase 2 — Core Game Loop

## Flow

```
BOOT (Bootstrap scene)
  -> MAIN MENU (MainMenu scene)
      -> GAMEPLAY (Gameplay scene)
          -> PAUSE (overlay)
          -> GAME OVER (overlay)
          -> LEVEL COMPLETE (overlay)
```

## Scripts

| File | Role |
|------|------|
| `Assets/Scripts/Core/GameStateId.cs` | State enum |
| `Assets/Scripts/Core/GameStateManager.cs` | State machine + scene loads |
| `Assets/Scripts/Core/GameBootstrap.cs` | Boots into Main Menu |
| `Assets/Scripts/UI/MainMenuController.cs` | Play / Quit |
| `Assets/Scripts/UI/GameplayFlowController.cs` | Pause / Game Over / Level Complete UI |

## Scenes

- `Assets/Scenes/Bootstrap.unity`
- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/Gameplay.unity`

## Debug shortcuts (Gameplay)

- `Esc` Pause / Resume
- `G` Game Over
- `C` Level Complete
