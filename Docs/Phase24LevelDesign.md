# Phase 24 — Level Design

> **Live layout note (Phase 27 / 31):** Checkpoint counts, enemy placements, and hazard spacing
> were retuned in Phase 27. Prefer `Phase24LevelDesignSetup.cs` / Phase 27 docs / scene
> contents over any stale counts below. Campaign goals use visible `Exit_Goal`; `LevelEnd`
> is a marker only (`completeLevelOnEnter` off — Phase 31).

## Goal

Create the three campaign levels with readable themes, teaching difficulty, and
approved gameplay systems only.

## Secrets

**Not included.** Secret areas / hidden collectibles were rejected in Phase 22 and
remain out of scope. Coin routes stay on or just off the main path (optional height
bonus), never gated behind hidden rooms.

## Campaign overview

| # | Level | Theme | Difficulty | Teaches before testing |
|---|-------|-------|------------|------------------------|
| 1 | Lumen Meadows | Soft meadow ground, wide ledges | Easy / tutorial | Move, jump, stomp Crawlbug, coins, one pit, mild spikes |
| 2 | Cascade Cliffs | Stepped cliff ledges | Medium | One-way / bounce / fire / Hopmite / Spikewatch after safe intros |
| 3 | Skybridge Spire | High bridges & moving spans | Hard | Moving platforms, Skimmer, Spitter, moving spikes, tighter gaps |

## Shared rules

- Start simple → introduce one idea → reuse it under pressure later in the same level.
- Avoid unfair spikes: hazards telegraph on solid ground; pits sit under readable gaps.
- Final goal: `LevelExitDoor` (+ `LevelEnd` aligned) on the right.
- Prefabs: Phase 22 approved set only.
- Hierarchy under `LevelRoot`: Platforms / Enemies / Collectibles / Hazards / Checkpoints.

---

## Level 1 — Lumen Meadows

| Element | Design |
|---------|--------|
| **Theme** | Open meadow path; low verticality |
| **Start** | Left meadow pad (`PlayerSpawn` on `Ground_Start`) |
| **Main path** | Left → right across four short ground segments with one taught gap |
| **Platforms** | Wide solids + two step-ups; no moving / one-way yet |
| **Enemies** | 2× Crawlbug (first open ground, second after gap) |
| **Collectibles** | Coin trail on main path; one elevated optional coin |
| **Hazards** | Death zone under the taught gap; spikes late (safe approach) |
| **Checkpoints** | 1 mid-route after the gap |
| **Secrets** | None |
| **Difficulty** | Generous jump windows; one enemy at a time |
| **Final goal** | Exit door on right high pad |
| **Power-ups** | Heart Drop near checkpoint (heal intro, optional) |

**Flow:** walk → jump gap → stomp → checkpoint → mild spikes → exit.

---

## Level 2 — Cascade Cliffs

| Element | Design |
|---------|--------|
| **Theme** | Ascending cliff shelves |
| **Start** | Lower left shelf |
| **Main path** | Stair-step climb with a mid bounce assist and a one-way shortcut ledge |
| **Platforms** | Solids, one bounce pad, one one-way platform |
| **Enemies** | Crawlbug (intro) → Hopmite → Spikewatch (stationary gate) |
| **Collectibles** | Coins on ledges; optional high coin on one-way |
| **Hazards** | Pit under mid gap; spikes on a shelf; fire zone before Spikewatch |
| **Checkpoints** | 2 (after first climb; before fire shelf) |
| **Secrets** | None |
| **Difficulty** | Medium spacing; two enemy types after each is shown alone |
| **Final goal** | Exit on upper right cliff |
| **Power-ups** | Speed Burst mid; Heart Drop before fire |

**Flow:** climb → bounce intro → Hopmite → CP → fire + Spikewatch → exit.

---

## Level 3 — Skybridge Spire

| Element | Design |
|---------|--------|
| **Theme** | Narrow bridges and moving spans in the sky |
| **Start** | Left tower pad |
| **Main path** | Bridge hops → moving platform → aerial Skimmer lane → Spitter approach |
| **Platforms** | Solids, one-way, moving platform, bounce recovery |
| **Enemies** | Dartling → Hopmite → Skimmer → Spitter (finale pressure) |
| **Collectibles** | Coins across bridges; risk coins near moving span |
| **Hazards** | Wide pit under bridges; fire on landing; moving spikes before exit |
| **Checkpoints** | 2 (after first bridges; after moving platform) |
| **Secrets** | None |
| **Difficulty** | Tight but fair; Glow Shield offered before Spitter / moving spikes |
| **Final goal** | Exit on right spire pad |
| **Power-ups** | Glow Shield before finale; Heart Drop mid; Speed Burst optional |

**Flow:** bridges → CP → mover → aerial foe → CP → shield → Spitter + moving spikes → exit.

---

## Deliverables

| Item | Role |
|------|------|
| `Phase24LevelDesignSetup.cs` | Rebuilds Level 01–03 layouts + fixes `LevelCatalog` |
| This doc | Per-level design lock |
| Updated LevelData notes | Theme + difficulty summary |
| Roadmap Phase 24 | Level Design |

## Setup

`Bounder Trail → Phase 24 → Setup Level Design`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase24LevelDesignSetup.SetupLevelDesign`

Re-running replaces authored content under each level’s content folders (idempotent rebuild).

Phase 27 re-applies these builders with fairness tweaks (extra checkpoints, enemy pacing).

## Next

Phase 27 owns difficulty tuning; Phase 25–26 visuals/juice re-applied after layout rebuilds.
