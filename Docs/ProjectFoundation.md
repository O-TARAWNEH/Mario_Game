# Project Foundation Guide — Phase 1

**Project:** Bounder Trail  
**Unity:** 6000.5.6f1 (Unity 6)  
**Goal:** Clean C# / Unity project foundation with no gameplay systems yet.

---

## 1. Folder Structure (Assets)

```
Assets/
├── Art/                  # Sprites, tiles, visual placeholders
│   ├── Player/
│   ├── Enemies/
│   ├── Items/
│   ├── World/
│   ├── UI/
│   └── Tiles/
├── Audio/
│   ├── Music/
│   └── SFX/
├── Data/                 # Config assets & ScriptableObjects
│   ├── Configs/
│   └── ScriptableObjects/
├── Editor/               # Editor-only utilities
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Items/
│   ├── World/
│   ├── UI/
│   └── Systems/
├── Scenes/               # Bootstrap, menus, levels
├── Scripts/
│   ├── Core/
│   ├── Player/
│   ├── Enemies/
│   ├── World/
│   ├── Levels/
│   ├── Items/
│   ├── UI/
│   ├── Audio/
│   ├── Camera/
│   ├── Save/
│   ├── Data/
│   └── Tests/
└── Tests/
    ├── EditMode/
    └── PlayMode/
```

Documentation remains outside Assets in `/Docs`.

---

## 2. Scene Organization

| Scene | Purpose | Phase |
|-------|---------|-------|
| `Bootstrap` | Runtime entry, foundation init | Phase 1 |
| `MainMenu` | Title / start flow | Later |
| `Level_01_LumenMeadows` | Campaign level 1 | Later |
| `Level_02_CascadeCliffs` | Campaign level 2 | Later |
| `Level_03_SkybridgeSpire` | Campaign level 3 | Later |

**Rule:** One gameplay level per scene. Systems that must persist can use `DontDestroyOnLoad` only when needed.

---

## 3. Prefab / Entity Organization

| Prefab folder | Contents |
|---------------|----------|
| `Prefabs/Player` | Pip and player variants |
| `Prefabs/Enemies` | Enemy prefabs |
| `Prefabs/Items` | Coins, power-ups |
| `Prefabs/World` | Platforms props, hazards, checkpoints, goals |
| `Prefabs/UI` | Reusable UI panels/widgets |
| `Prefabs/Systems` | Bootstrap/manager prefabs if extracted |

---

## 4. Asset Organization

- Visuals → `Art/`
- Sounds → `Audio/Music` or `Audio/SFX`
- Tunable data assets → `Data/`
- Never drop loose assets in `Assets/` root

---

## 5. Configuration / Data Organization

- Inspector fields on components for local tuning
- Shared data assets under `Assets/Data/`
- ScriptableObjects only when sharing/reuse is genuinely useful
- Project identity constants in `ProjectConstants` (not gameplay balance)

---

## 6. Testing Organization

| Location | Use |
|----------|-----|
| `Assets/Tests/EditMode` | Editor/unit-style tests (when Test Framework is added later) |
| `Assets/Tests/PlayMode` | Runtime play-mode tests (later) |
| `Assets/Scripts/Tests` | Tiny helper scripts used while developing (optional) |

Phase 1 establishes folders and conventions only. No gameplay test suites yet.

---

## 7. Bootstrap System

- Script: `Assets/Scripts/Core/GameBootstrap.cs`
- Logging: `Assets/Scripts/Core/GameLog.cs`
- Constants: `Assets/Scripts/Core/ProjectConstants.cs`
- Scene: `Assets/Scenes/Bootstrap.unity`
- Editor helper: `Assets/Editor/ProjectFoundationSetup.cs`

Bootstrap responsibilities in Phase 1:
- Singleton guard
- Optional `DontDestroyOnLoad`
- Apply target frame rate (60)
- Log foundation readiness

Bootstrap does **not**:
- Load gameplay
- Spawn player/enemies
- Show final UI
- Run combat/power-ups

---

## 8. Logging / Debugging Conventions

Use `BounderTrail.Core.GameLog` for all project logs.

See `Docs/NamingConventions.md` for category rules.
