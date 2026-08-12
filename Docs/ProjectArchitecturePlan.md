# Project Architecture Plan — Phase 0

**Project:** Bounder Trail  
**Status:** Definition only (no runtime systems yet)

---

## 1. Architecture Goals

- Keep systems modular and easy to debug
- Prefer simple scripts over deep inheritance trees
- Build incrementally by phase without rewriting working systems
- Make prefabs the main reusable gameplay units
- Keep managers thin and explicit

---

## 2. High-Level Architecture

```
Unity Scenes
 ├─ MainMenu
 ├─ Level_01_LumenMeadows
 ├─ Level_02_CascadeCliffs
 └─ Level_03_SkybridgeSpire

Runtime gameplay objects
 ├─ Player (input → movement → collisions → feedback)
 ├─ Enemies (simple behaviors → player interaction)
 ├─ Items / Power-ups / Hazards
 ├─ Checkpoints / Goal
 └─ Level bounds / Tilemap environment

Coordination
 ├─ GameManager (lives, game flow, game over)
 ├─ LevelManager (load/complete/restart level)
 ├─ Score/Coins UI bridge
 ├─ Audio hooks
 └─ Save/Progress (later phase)
```

Exact manager names may be finalized when those phases begin, but responsibilities stay as above.

---

## 3. Script Folder Responsibilities

| Folder | Responsibility |
|--------|----------------|
| `Assets/Scripts/Core` | Bootstrap, logging, shared constants/utilities |
| `Assets/Scripts/Player` | Movement, jump, player health/lives, animation drivers |
| `Assets/Scripts/Enemies` | Enemy movement/AI, defeat, player contact rules |
| `Assets/Scripts/World` | Hazards, platforms, world prop logic |
| `Assets/Scripts/Levels` | Checkpoints, goals, spawn points, level helpers |
| `Assets/Scripts/Items` | Coins, power-ups, collectible logic |
| `Assets/Scripts/UI` | Menus, HUD, pause, game over, level complete |
| `Assets/Scripts/Audio` | Audio helpers/managers |
| `Assets/Scripts/Camera` | Camera follow / feedback helpers |
| `Assets/Scripts/Save` | Save/load progress |
| `Assets/Scripts/Data` | Data definitions / SO wrappers |
| `Assets/Scripts/Tests` | Optional runtime test helpers |

---

## 4. Prefab Strategy

| Prefab group | Examples |
|--------------|----------|
| Player | Pip prefab with Rigidbody2D, colliders, scripts, animator |
| Enemies | Crawlbug, Hopmite, Spikewatch |
| Items | Coin, Speed Burst, Glow Shield, Heart Drop |
| Environment | Spike hazard, checkpoint flag, goal portal, platform props |
| UI | HUD canvas pieces, pause panel, menu buttons (as needed) |

Prefabs are created in the phases that introduce those features — not in Phase 0 beyond folders.

---

## 5. Scene Strategy

- **Main Menu scene** for title flow and level start
- **One scene per campaign level** for clarity and easy testing
- Shared prefabs placed into levels as needed
- Avoid a monolithic “everything in one scene forever” approach

Persistent DontDestroyOnLoad managers are optional and should only appear if a later phase proves they are necessary.

---

## 6. Physics Layer Plan (Create When Needed)

Planned layers:

- Default
- Ground
- Player
- Enemy
- Hazard
- Pickup
- Checkpoint
- Goal

Collision matrix will be configured when layers are first introduced so only meaningful pairs collide.

---

## 7. Tag Plan (Create When Needed)

Planned tags:

- Player
- Ground
- Enemy
- Hazard
- Coin
- PowerUp
- Checkpoint
- Goal

Tags support readable collision filtering and debugging.

---

## 8. Data Strategy

- Tunable values exposed in the Inspector (`speed`, `jumpForce`, `enemySpeed`, etc.)
- ScriptableObjects only for shared reusable data definitions when duplication becomes real pain
- PlayerPrefs for lightweight save/progress in the save phase

---

## 9. Dependency Rules

- Lower-level gameplay components should not depend on UI implementation details
- Managers may read/write gameplay state and notify UI
- Avoid circular script dependencies
- Do not create duplicate classes for the same responsibility

---

## 10. Extension Points (For Later Phases Only)

These are intentional seams — not current work:

- Swap placeholder sprites without rewriting movement
- Add enemy types by composing simple behavior scripts
- Add levels by duplicating a level scene pattern
- Replace placeholder audio clips in existing AudioSources

---

## 11. What Phase 0 Does Not Create

- No gameplay scripts
- No enemies
- No final levels
- No menus
- No advanced systems
- No Unity package installation in this phase

Phase 0 establishes documents + folder scaffolding only.
