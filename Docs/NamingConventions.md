# Naming Conventions — Bounder Trail

**Phase:** 1 — Project Foundation  
**Status:** Active

---

## 1. C# Types

| Kind | Convention | Example |
|------|------------|---------|
| Namespaces | `BounderTrail.<Area>` | `BounderTrail.Core`, `BounderTrail.Player` |
| Classes / Structs | PascalCase | `GameBootstrap`, `PlayerMotor` |
| Interfaces | `I` + PascalCase | `IDamageable` (later phases) |
| MonoBehaviours | PascalCase, one main type per file | `GameLog.cs` contains `GameLog` |
| Constants | PascalCase | `TargetFrameRate` |
| Private fields | camelCase, often with `[SerializeField]` | `targetFrameRate` |
| Properties | PascalCase | `CurrentScore` |
| Methods | PascalCase | `ApplyFoundationSettings()` |
| Events / Actions | PascalCase, verb phrase | `OnPlayerDied` |

**File rule:** Filename matches the primary public type (`GameBootstrap.cs` → `GameBootstrap`).

---

## 2. Unity Assets

| Asset | Convention | Example |
|-------|------------|---------|
| Scenes | PascalCase or `Level_##_Name` | `Bootstrap`, `MainMenu`, `Level_01_LumenMeadows` |
| Prefabs | PascalCase, role-first | `Player_Pip`, `Enemy_Crawlbug`, `Item_Coin` |
| Sprites / Art | PascalCase or snake prefixes by folder | `Pip_Idle`, `Tile_Grass` |
| Audio | `SFX_` / `BGM_` prefix | `SFX_Jump`, `BGM_Level01` |
| ScriptableObjects | `SO_` or descriptive PascalCase | `SO_EnemyCrawlbugStats` |
| Materials | `Mat_` prefix | `Mat_SpikeHazard` |
| Animators | `Anim_` / controller name matching entity | `Anim_Pip` |

---

## 3. GameObjects in Scenes

| Role | Convention | Example |
|------|------------|---------|
| Systems | Clear system name | `GameBootstrap`, `GameManager` |
| World roots | Grouping empties | `_Level`, `_Systems`, `_UI` |
| Spawn markers | Suffix `Spawn` / `Point` | `PlayerSpawn` |
| Triggers | Descriptive + type | `Checkpoint_A`, `Goal_Exit` |

Leading underscore on empty organizer objects is optional but recommended.

---

## 4. Layers & Tags (create when needed)

Planned tags: `Player`, `Ground`, `Enemy`, `Hazard`, `Coin`, `PowerUp`, `Checkpoint`, `Goal`  
Planned layers: `Ground`, `Player`, `Enemy`, `Hazard`, `Pickup`, `Checkpoint`, `Goal`

Use Unity Tag Manager / Layer Manager — do not invent ad-hoc string tags in code without registering them.

---

## 5. Logging Convention

Always use `GameLog`:

```csharp
GameLog.Info("Bootstrap", "Ready");
GameLog.Warning("Player", "Missing Rigidbody2D");
GameLog.Error("Save", "Failed to write progress");
```

Format produced: `[BounderTrail][Category] message`

Categories should be short area names: `Bootstrap`, `Player`, `Enemy`, `UI`, `Save`, `Audio`, etc.

---

## 6. Script Organization by Folder

| Folder | Put here |
|--------|----------|
| `Scripts/Core` | Bootstrap, logging, shared constants, core utilities |
| `Scripts/Player` | Player movement, player state (later) |
| `Scripts/Enemies` | Enemy behaviours |
| `Scripts/World` | Hazards, platforms, world props logic |
| `Scripts/Levels` | Checkpoints, goals, level flow helpers |
| `Scripts/Items` | Coins, power-ups |
| `Scripts/UI` | HUD and menus |
| `Scripts/Audio` | Audio helpers/managers |
| `Scripts/Camera` | Camera follow/shake helpers |
| `Scripts/Save` | Save/load progress |
| `Scripts/Data` | Data definitions / SO wrappers |
| `Scripts/Tests` | Small runtime test helpers (non-EditMode) |

Edit Mode / Play Mode Unity Test Framework tests live under `Assets/Tests/`.
