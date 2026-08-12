# Phase 28 — Bug Fixing

## Goal

Dedicated bug-fixing pass. **No new features** — only incorrect behavior.

## Systems tested

Player movement/jump/collisions · Camera · Enemies/combat · Collectibles · Power-ups ·  
Hazards · Checkpoints · Death/respawn · UI · Audio · Level transitions · Save/load · Menus

## Fixes applied

| # | Severity | Issue | Fix |
|---|----------|-------|-----|
| 1 | Major | Camera dead-zone used shaken pose → follow drift after shake | Dead-zone uses unshaken camera position |
| 2 | Major | Continue after level complete restarted the cleared level | `RegisterLevelCompleted` advances `continueLevelIndex` |
| 3 | Blocker | Failed `LevelLoader` early-outs left GSM `_isTransitioning` stuck | Notify load failed; `TryLoadNextLevel` only returns true when a load starts |
| 4 | Major | `SimpleBurstVfx` `_alive` leaked across scene unloads → juice stopped | Count release on destroy + reset on scene unload |
| 5 | Minor | Checkpoint `faceRightOnRespawn` ignored | Applied via public `PlayerController.SetFacing` on respawn |
| 6 | Minor | Control lock survived death→respawn | `LockControl(0)` clears lock on respawn |
| 7 | Minor | Stomp bounce reapplied every Stay during enemy i-frames | Skip stomp handling while invulnerable |
| 8 | Minor | Level-complete juice could keep a stale completion service | Rebind when `LevelCompletionService.Instance` changes |

## Explicitly not changed

- No new gameplay systems or content
- No balance retunes (Phase 27)
- No art/VFX feature additions

## Setup / validation

`Bounder Trail → Phase 28 → Validate Bug Fixes`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase28BugFixingSetup.ValidateBugFixes`

Confirms critical scripts still exist and LevelCatalog is fully wired.
