# Phase 7 — Level and World System

## Goal

Foundation for building levels: structure, bounds, spawn/end, platforms, markers, data, and loading.

## Scripts

| File | Role |
|------|------|
| `Scripts/Data/LevelData.cs` | Level ScriptableObject |
| `Scripts/Data/LevelCatalog.cs` | Ordered level list |
| `Scripts/Levels/LevelRoot.cs` | Scene hierarchy + init |
| `Scripts/Levels/LevelLoader.cs` | Loads catalog levels |
| `Scripts/Levels/LevelEndPoint.cs` | End/goal marker |
| `Scripts/Levels/LevelContentMarker.cs` | Enemy/collectible/hazard/checkpoint/decor slots |
| `Scripts/World/PlatformPiece.cs` | Platform/ground identity |
| `Scripts/Levels/LevelBounds.cs` | Existing bounds (Phase 6) |
| `Scripts/Player/PlayerSpawnPoint.cs` | Existing start spawn (Phase 3) |

## Hierarchy (Gameplay)

```
LevelRoot
├── LevelBounds
├── PlayerSpawn
├── LevelEnd
├── Player_Pip
├── Platforms/
├── Enemies/          (markers)
├── Collectibles/     (markers)
├── Hazards/          (markers)
├── Checkpoints/      (markers)
├── Decorations/      (markers)
└── Tilemaps/Grid/Tilemap_Ground
```

## Data assets

- `Assets/Data/Levels/LevelData_GameplayPrototype.asset`
- `Assets/Data/Levels/LevelCatalog.asset`

## Prefabs

- `Assets/Prefabs/World/Platform_Basic.prefab`
- `Assets/Prefabs/World/Ground_Basic.prefab`
- `Assets/Art/Tiles/Tile_GroundBasic.asset`

## Loading flow

Main Menu Play → `GameStateManager.StartGameplay()` → `LevelLoader.LoadCurrentLevel()` → Gameplay scene → `LevelRoot.InitializeLevel()`
