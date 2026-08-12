# Phase 23 — Boss System

## Goal

Create boss encounters **only if** the Game Design Specification requires them.

## Verdict

**Bosses are not required.** Bounder Trail’s design specifies three standard enemy roles
(Crawlbug, Hopmite, Spikewatch) with simple patrol/contact AI. Prior enemy and combat
phases explicitly excluded bosses. Phase 22’s approved catalog does not list bosses.

Phase 23 is a **design lock** pass: confirm the decision, document rejected boss
subsystems from the phase prompt, and ensure no boss architecture is invented.

## Prompt “create” list vs design

| Prompt idea | Design status | Project status |
|-------------|---------------|----------------|
| Boss architecture | **Not approved** | **Not added** |
| Boss health | **Not approved** | **Not added** (standard `EnemyHealth` covers foes) |
| Boss states (Idle / Move / Attack / Hurt / Phase / Dead) | **Not approved** | **Not added** (standard `EnemyBrain` states cover foes) |
| Boss attacks | **Not approved** | **Not added** |
| Boss phases | **Not approved** | **Not added** |
| Boss damage | **Not approved** | **Not added** |
| Boss death | **Not approved** | **Not added** |
| Boss arena | **Not approved** | **Not added** |
| Boss completion | **Not approved** | **Not added** |

## What the game uses instead

Levels end via `LevelExitDoor` / `LevelCompletionService` after platforming, standard
enemies, hazards, coins, and power-ups — not after defeating a boss.

Enemy combat remains:

- `EnemyHealth` + `IDamageable`
- `EnemyBrain` states: Idle / Patrol / Chase / Attack / Hurt / Dead
- Design-spec types and Phase 10 variants only

## Deliverables

| Item | Role |
|------|------|
| `ApprovedGameplayCatalog.cs` | Bosses added to rejected list |
| `Phase23BossSystemSetup.cs` | Confirms design lock; asserts no Boss* assets |
| This doc | Explicit non-requirement for content pass |

## Setup

`Bounder Trail → Phase 23 → Validate Boss System Decision`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase23BossSystemSetup.ValidateBossSystemDecision`

## Next

Phase 24 authored unique Level 1–3 layouts (no secrets / no bosses).  
Balance and polish against those layouts in Phase 25.
