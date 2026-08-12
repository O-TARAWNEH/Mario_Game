# Phase 30 — Full Playtest

## Goal

Play the campaign end-to-end and **record** bugs / problems. No new features. No balance retunes.

## Method

1. **Content inventory** — Level_01/02/03 scenes + `Phase24LevelDesignSetup` / Phase 27 layouts + `LevelCatalog`.
2. **Flow audit** — New Game → levels → menus → save/load → Game Over → Finish (scripts + build settings).
3. **Coverage matrix** — every enemy, power-up, hazard, checkpoint, menu path.
4. **Interactive Play Mode** — confirm in Unity Editor (checklist below). This report flags issues found in inventory/flow review; mark Play Mode status when you run it.

**Bosses:** **N/A** (Phase 23 design lock). No boss encounters to playtest.

---

## Campaign coverage matrix

### Levels (build order)

| # | Scene | In build | Cleared path |
|---|--------|----------|--------------|
| Boot | `Bootstrap` | Yes | → Main Menu |
| Menu | `MainMenu` | Yes | New / Continue / Level Select / Settings / Quit |
| 0 | `Level_01_LumenMeadows` | Yes | Exit → L2 |
| 1 | `Level_02_CascadeCliffs` | Yes | Exit → L3 |
| 2 | `Level_03_SkybridgeSpire` | Yes | Exit → **Finish** → Main Menu |
| — | `Gameplay.unity` | **No** | Sandbox only (not part of campaign) |

### Enemies exercised

| Enemy | Stomp | Contact | Special | Levels |
|-------|-------|---------|---------|--------|
| Crawlbug | Yes | Yes | Patrol | L1×2, L2×1, L3×1 |
| Hopmite | Yes | Yes | Jump interval | L2, L3 |
| Spikewatch | No (gate) | Yes | Unstompable | L2 |
| Dartling | Yes | Yes | Fast patrol | L3 |
| Skimmer | Yes | Yes | Flyer | L3 |
| Spitter | Yes | Yes | Projectile | L3 |
| Boss | — | — | — | **N/A** |

### Power-ups

| Kind | L1 | L2 | L3 |
|------|----|----|-----|
| Heart Drop | ✓ | ✓ | ✓ |
| Speed Burst | — | ✓ | ✓ |
| Glow Shield | — | — | ✓ |

### Hazards

| Hazard | L1 | L2 | L3 |
|--------|----|----|-----|
| DeathZone (pit) | ✓ | ✓✓ | ✓ (wide) |
| Spikes | ✓ | ✓ | — |
| Fire | — | ✓ | ✓ |
| Moving spike | — | — | ✓ |

### Checkpoints

| Level | Checkpoints |
|-------|-------------|
| L1 | Early, Mid |
| L2 | A, B |
| L3 | A, B, C |

---

## Flow results

| Test | Expected | Result |
|------|----------|--------|
| New Game | Clears/resets save → L1 | **Pass** (wired) |
| Every level load | Catalog → async scene | **Pass** (3/3 in build) |
| Every enemy type | Present in campaign | **Pass** (6/6 + projectile) |
| Every power-up | Collectible + effect | **Pass** (placements + `PlayerPowerUps`) |
| Every hazard | Damage / kill as designed | **Pass** (placements + `HazardResponse`) |
| Every checkpoint | Activate → respawn there | **Pass** (wired; Play Mode confirm facing) |
| Boss | None | **N/A / Pass** |
| Pause / Settings | Esc, volume, resume | **Pass** (wired) |
| Level Select | Lock/unlock/cleared labels | **Pass** (wired) |
| Saving | On progress / complete | **Pass** (`SaveSystem` + `GameProgress`) |
| Loading / Continue | Resume `continueLevelIndex` | **Pass** (Phase 28 fix) |
| Restart | Reload current level | **Pass** (wired) |
| Game Over | 0 lives → overlay → Restart/Menu | **Pass** (wired) |
| Completion (L3) | Finish → Main Menu + save | **Pass** (no credits — see notes) |

---

## Recorded findings

### Blockers

*None.* Phase 28 cleared stuck LevelLoader transition, Continue index, VFX leak, and camera dead-zone drift.

### Bugs

| # | Sev | Area | Finding |
|---|-----|------|---------|
| 1 | Minor | Power-ups | Heart Drop at full HP still consumes the pickup (`Heal` no-ops; `TryActivate` still succeeds). |
| 2 | Minor | Levels | Dual goal objects (`Exit_Goal` + `LevelEnd`) both notify completion — sticky one-shot prevents double-fire, but redundant. |
| 3 | Minor | Docs | `Docs/Phase24LevelDesign.md` drifts from live Phase 27 layouts (CP/enemy counts). Use scenes/setup scripts as truth. |

### Gameplay problems

| # | Sev | Finding |
|---|-----|---------|
| 4 | Major (UX) | Beating L3 ends on **Finish → Main Menu** only. No credits / victory beat. Save marks complete; Continue returns to L3. |
| 5 | Note | InstantKill pits ignore Glow Shield (documented design — may feel unfair if shield is misunderstood). |
| 6 | Note | Spikewatch cannot be stomped — intentional gate; first-time players may spam jump expecting a kill. |

### Visual problems

| # | Sev | Finding |
|---|-----|---------|
| 7 | Note | Bounce pads / checkpoints / hazards have no dedicated juice (Phase 26 scope omitted them). |
| 8 | Note | Geometric placeholder art is intentional (Phase 25); not a defect for this phase. |

### Audio problems

| # | Sev | Finding |
|---|-----|---------|
| 9 | Minor | Disk has unused clips: `SFX_Coin.wav`, `SFX_PowerUp_Heart/Shield/Speed.wav` — runtime uses generic `Collect` / `PowerUp` only. |
| 10 | Note | No checkpoint, bounce, hazard, or spit SFX hooks. |
| 11 | Note | Music stays on Gameplay theme through Game Over / Level Complete (no dedicated stingers beyond SFX). |

### Performance problems

| # | Sev | Finding |
|---|-----|---------|
| 12 | Note | Phase 29 addressed HUD GC, VFX pool, idle Updates, texture caps. No new hotspots found in this pass. |
| 13 | Note | Campaign object counts are small; physics NonAlloc sensors remain appropriate. |

### Confusing mechanics

| # | Sev | Finding |
|---|-----|---------|
| 14 | Note | Shield vs pits: shield does not save from InstantKill death zones. |
| 15 | Note | Per-level coin/score HUD resets on level load; career bests live in save only. |
| 16 | Note | `Gameplay.unity` exists as a full sandbox but is **out of build** — easy to confuse with campaign. |

---

## Interactive Play Mode checklist

Run from **Bootstrap** in the Editor and tick:

- [ ] New Game → clear L1 (pit, spikes, 2 Crawlbugs, Heart, 2 CPs, exit)
- [ ] L2 (bounce, one-way, Hopmite, Spikewatch avoid, fire, Speed+Heart, 2 CPs)
- [ ] L3 (mover, Dartling, Skimmer, Spitter shots, Glow Shield, MovingSpike, 3 CPs)
- [ ] Mid-level death → respawn at CP (coins kept, enemies reset, facing OK)
- [ ] Burn 3 lives → Game Over → Restart and → Main Menu
- [ ] Pause → Settings volumes → Resume; pause Restart / Main Menu
- [ ] Level Select locks / unlocks / Cleared labels
- [ ] Quit-to-menu, relaunch Continue → expected level
- [ ] Clear L3 → Finish → Main Menu; save shows L3 cleared
- [ ] Reset Save (if exposed) clears Continue

---

## Explicitly not done this phase

- No bug fixes (record only — fix in a later pass if approved)
- No new content, credits scene, or audio wiring
- No balance changes

## Setup / validation

`Bounder Trail → Phase 30 → Validate Full Playtest Coverage`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase30FullPlaytestSetup.ValidateFullPlaytestCoverage`

Confirms catalog, build scenes, and campaign content markers for the coverage matrix above.
