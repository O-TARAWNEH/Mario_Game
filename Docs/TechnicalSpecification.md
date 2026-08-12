# Technical Specification — Phase 0

**Document status:** Phase 0 definition  
**Project:** Bounder Trail  
**Last updated:** 2026-08-10

---

## 1. Engine & Language

| Item | Decision |
|------|----------|
| Engine | **Unity 6** |
| Language | **C#** |
| C# language level | Whatever Unity 6’s scripting runtime enables (treat as **C# 9+** features only when needed; prefer simple C# for clarity) |
| Scripting backend (build) | Mono or IL2CPP acceptable; default Unity Windows build settings unless a later phase requires otherwise |
| API compatibility | Unity’s default for the created Unity 6 project |
| External frameworks | **None** unless explicitly approved |

---

## 2. Target Platform & Performance

| Item | Decision |
|------|----------|
| Platform | Windows PC / Desktop |
| Target frame rate | **60 FPS** |
| VSync | Follow Unity project default initially; tune in optimization phase if needed |
| Quality goal | Stable gameplay on a normal laptop; avoid heavy VFX/physics |

---

## 3. Resolution & Display

| Item | Decision |
|------|----------|
| Reference resolution | **1920 × 1080** |
| Aspect ratio focus | 16:9 |
| Fullscreen / windowed | Support both via Unity player settings later |
| UI scaling | Canvas Scaler reference resolution **1920 × 1080**, Scale With Screen Size |
| Pixel style note | If pixel art is used later, PPU and filter mode will be configured then; gameplay must not depend on one art pipeline |

---

## 4. Rendering Approach

| Item | Decision |
|------|----------|
| Pipeline | **Universal Render Pipeline (URP) — 2D Renderer** (Unity 6 standard 2D setup) |
| Camera | Orthographic `Camera`, side-scrolling follow |
| Sprites | `SpriteRenderer` |
| Tilemaps | Unity Tilemap for ground/platforms where useful |
| Lighting | Keep simple (2D defaults); no complex global illumination requirement |
| Post-processing | Off by default; add only if a later phase requests it |

If Unity project creation defaults differ slightly, match URP 2D and document any deviation in that phase.

---

## 5. Physics Approach

| Item | Decision |
|------|----------|
| System | **Unity 2D Physics** |
| Player body | `Rigidbody2D` (Dynamic), freeze Z rotation |
| Colliders | `Collider2D` variants (`BoxCollider2D`, `CapsuleCollider2D`, etc. as needed) |
| Platforms / ground | Static colliders and/or Tilemap colliders |
| Movement model | Velocity-based horizontal move + impulse/force jump; gravity via Rigidbody2D |
| Layers | Dedicated physics layers planned (Player, Ground, Enemy, Hazard, Pickup, Checkpoint, Goal) — created when first needed |
| Queries | Prefer collision callbacks / simple checks; avoid expensive per-frame physics spam |

---

## 6. Input System

| Item | Decision |
|------|----------|
| Primary system | **Unity Built-in Input Manager** (`Input.GetAxis`, `Input.GetButton`, etc.) |
| Why | No extra package required; simple; enough for keyboard platformer controls |
| Default actions (planned) | Horizontal move, Jump, Run/modifier, Pause |
| Gamepad | Not required in Phase 0; may be added later only if requested |

**Not selected for now:** New Input System package (can be reconsidered later without rewriting design intent).

---

## 7. Animation Approach

| Item | Decision |
|------|----------|
| System | Unity **Animator** + Animation Clips |
| Player states (planned) | Idle, Run, Jump, Fall, Hurt/Death (exact set locked in animation phase) |
| Complexity | Minimal state machine; no complex blend trees unless needed |

---

## 8. UI Approach

| Item | Decision |
|------|----------|
| System | Unity **uGUI** (Canvas / Text / Buttons / Images) |
| HUD | Screen-space overlay |
| Menus | Separate scenes or menu roots as decided in UI phases |
| Fonts | Simple readable fonts; replaceable |

---

## 9. Prefabs & Data

| Item | Decision |
|------|----------|
| Prefabs | Player, enemies, items, hazards, UI panels, level props |
| ScriptableObjects | **Only when genuinely useful** (e.g., shared enemy stats, level catalog). Do not force SO architecture everywhere |
| Scenes | Main Menu + one scene per level (minimum). Bootstrap/persistent managers only if a later phase needs them |

---

## 10. Asset Strategy

| Item | Decision |
|------|----------|
| Early assets | Placeholder geometric sprites / simple shapes |
| Final assets | Original or free/open-licensed only |
| Audio assets | Placeholder-safe; replaceable file slots |
| Naming | Clear, original project names (`Pip`, `Crawlbug`, etc.) |
| Copyright | No Nintendo/Mario copyrighted materials |

Gameplay code must reference components/tags/layers, not hard-depend on final art dimensions beyond reasonable collider setup.

---

## 11. Audio Strategy

| Item | Decision |
|------|----------|
| Playback | Unity `AudioSource` |
| Music | Looping BGM clips (menu / level) via `MusicSystem` |
| SFX | One-shots via `SfxSystem` (`SfxId` catalog) |
| Mixing | Master / Music / SFX volumes (`AudioVolumeSettings`, PlayerPrefs) |
| Manager | `AudioManager` on bootstrap (Phase 18) |

---

## 12. Save System Requirements

| Item | Decision |
|------|----------|
| Storage | JSON files in `persistentDataPath` (primary + backup); PlayerPrefs legacy migrated |
| Saved data | Unlocks, completions, continue level, best coins/score, audio volumes |
| Safety | Checksum validation; atomic temp write; backup restore on corrupt primary |
| Operations | Save, Load, New Game, Reset Save |
| Not required | Cloud saves, encryption, multi-profile |

---

## 13. Level Structure (Technical)

| Item | Decision |
|------|----------|
| Level count | **3** campaign levels |
| Representation | Unity scenes + Tilemaps/prefab layout |
| Spawn | Marked start transform |
| Checkpoint | Trigger volumes storing respawn point |
| Goal | Trigger completing the level via level/game manager |
| Progression | Unlock next level on success |

---

## 14. Coding Standards (Project-Wide)

- Clean, readable, beginner-friendly C#
- Modular scripts by responsibility (Player, Enemy, UI, Managers)
- Serialized Inspector fields for tunable gameplay values
- Avoid duplicate classes/scripts
- No placeholder stubs when a phase requires a real implementation
- Comment only where useful
- Prefer simple patterns over heavy frameworks

---

## 15. Performance Constraints

Avoid:
- Oversized textures
- Unnecessary particle systems
- Complex AI
- Huge scenes
- Extra packages
- Needless `Update()` work

Prefer:
- Simple physics
- Reused prefabs
- Small tile sets
- Efficient collision layers

---

## 16. Testing Requirements (Every Phase)

For each phase after implementation:
- Verify phase completion criteria in Unity Play Mode when applicable
- Do not claim untested results
- Fix regressions caused by the current phase before approval

---

## 17. Phase 0 Deliverable Note

This document defines decisions only.  
**No gameplay systems are implemented in Phase 0.**

Unity project creation and package/template confirmation happen in the next approved phase.
