# Development Roadmap — Bounder Trail

**Status:** Master development plan for this project  
**Rule:** Implement only the phase you are told to start. Do not skip or combine phases unless explicitly requested.

---

## Phase 0 — Project Definition *(current)*

**Goal:** Define the game before building systems.

**Deliverables:**
- Game Design Specification
- Technical Specification
- Project Architecture Plan
- Folder structure
- This Development Roadmap

**Does not include:** gameplay, enemies, final levels, menus, advanced systems.

---

## Phase 1 — Project Foundation *(complete when approved)*

**Goal:** Create a clean C# / Unity project foundation.

**Deliverables:**
- Unity 6 project created (`6000.5.6f1`)
- Phase 1 folder structure + naming conventions
- Scene / prefab / art / audio / data / test organization
- Bootstrap system + logging conventions
- Foundation packages (2D feature, URP, uGUI, Test Framework)

**Does not include:** player gameplay, enemies, combat, power-ups, final UI.

---

## Phase 2 — Core Game Loop *(complete when approved)*

**Goal:** Create the basic game lifecycle.

**Deliverables:**
- Game state management (Boot, Main Menu, Gameplay, Pause, Game Over, Level Complete)
- Scene/state transitions (`Bootstrap` → `MainMenu` → `Gameplay`)
- Pause/resume, restart, return to main menu
- Minimal flow UI (not final artwork)

**Does not include:** final menu art, complex UI, enemies, collectibles.

---

## Phase 3 — Player Foundation *(complete when approved)*

**Goal:** Create the basic playable character.

**Deliverables:**
- Player entity/prefab (`Player_Pip`)
- Controller with move, accel/decel, jump, gravity, max fall speed
- Ground detection, facing, collision
- Spawn point + test platforms in Gameplay

**Does not include:** enemies, combat, power-ups, advanced abilities, final animations.

---

## Phase 4 — Player Physics and Game Feel *(complete when approved)*

**Goal:** Make player movement feel responsive and polished.

**Deliverables:**
- Accel/decel, air control, coyote time, jump buffer, variable jump
- Gravity/fall tuning, improved ground/edge/slope sensing
- Tuned walk/run/jump defaults on `Player_Pip`

**Does not include:** enemies, combat, power-ups, advanced movement abilities.

---

## Phase 5 — Player Animation *(complete when approved)*

**Goal:** Connect the player's visual state to gameplay.

**Deliverables:**
- Idle / Walk / Run / Jump / Fall / Land / Death animations
- Animator driven by movement, velocity, ground, jump, fall, death
- Minimal `PlayerDeath` state for death animation binding

**Does not include:** enemy animations, elaborate VFX.

---

## Phase 6 — Camera System *(complete when approved)*

**Goal:** Create a reliable 2D side-scrolling camera.

**Deliverables:**
- Smooth player tracking (X/Y)
- Dead zone
- Level bounds + camera limits
- Gameplay scene wiring

**Does not include:** cinematic cameras or complicated camera effects.

---

## Phase 7 — Level and World System *(complete when approved)*

**Goal:** Create the foundation for building levels.

**Deliverables:**
- LevelRoot structure + content folders
- Start/end points, bounds, platforms/ground, tilemap foundation
- LevelData + LevelCatalog + LevelLoader
- Content markers for enemies/collectibles/hazards/checkpoints/decor

**Does not include:** final campaign, many levels, or advanced level scripting.

---

## Phase 8 — Environmental Objects *(complete when approved)*

**Goal:** Create reusable objects used to construct levels.

**Deliverables:**
- Solid platforms
- One-way platforms
- Moving platforms
- Bounce pads
- Exit doors
- Prefabs easy to place under `Prefabs/World`

**Skipped (unnecessary for current scope):** breakable platforms, ladders.

---

## Phase 9 — Enemy Foundation *(complete when approved)*

**Goal:** Create the enemy architecture.

**Deliverables:**
- Base enemy brain/health/mover/sensor/contact
- Flexible states (Idle, Patrol, Chase, Attack, Hurt, Dead)
- One example patrol enemy: Crawlbug
- Player `IDamageable` bridge for contact damage

**Does not include:** every enemy type, bosses, complicated AI.

---

## Phase 10 — Enemy Types *(complete when approved)*

**Goal:** Create the different standard enemy types.

**Deliverables:**
- Crawlbug (walker), Dartling (fast), Hopmite (jumper)
- Skimmer (flyer), Spikewatch (stationary), Spitter (projectile)
- Shared enemy animator + contact rule options

**Does not include:** bosses or advanced boss AI.

---

## Phase 11 — Player and Enemy Interactions *(complete when approved)*

**Goal:** Define how the player interacts with enemies.

**Deliverables:**
- Player takes damage (health, knockback, invulnerability frames)
- Enemy takes damage (stomp / hits) + knockback
- Stomp interaction where designed
- Enemy death + player death
- Damage feedback (hurt flash / anim hooks)
- Documented contact rules (touch, stomp, attack, after-hit)

**Does not include:** power-ups, boss battles.

---

## Phase 12 — Collectibles *(complete when approved)*

**Goal:** Create collectible objects.

**Deliverables:**
- Coin (or equivalent) prefab, easy to place and reuse
- Collectible detection + collection state
- Collection effect + sound
- Counter (coins/score) that notifies game systems
- Sample placements in Gameplay

**Does not include:** complex economy, shops, power-ups.

---

## Phase 13 — Power-Ups *(complete when approved)*

**Goal:** Create the game's power-up system.

**Deliverables:**
- Power-up base pickup + player state system
- Design-spec powers only: Speed Burst, Glow Shield, Heart Drop
- Pickup detection, activation, duration, removal, state
- Placeable prefabs + sample placements

**Does not include:** random power-ups, jump/attack/size powers, shops.

---

## Phase 14 — Hazards *(complete when approved)*

**Goal:** Create environmental dangers.

**Deliverables:**
- Pit / death zones (instant kill)
- Spikes (contact damage)
- Fire / ember zones (damage over time)
- Moving spike hazard
- Shared environmental damage detection + clean reset

**Does not include:** unnecessary extra hazard types, bosses.

---

## Phase 15 - Checkpoints and Respawning *(complete when approved)*

**Goal:** Create reliable player recovery.

**Deliverables:**
- Checkpoint system + respawn position
- Death / respawn handling with lives
- Player reset (HP, power-ups, death state)
- Enemy reset + moving hazard reset
- Defined collectible persistence rules

---

## Phase 16 - Level Completion *(complete when approved)*

**Goal:** Create the level-ending system.

**Deliverables:**
- Level goal detection (exit door / end point)
- Level completion state + transition
- Next-level loading (or Finish to menu)
- Defined leave/complete rules

---

## Phase 17 - User Interface *(complete when approved)*

**Goal:** Create the gameplay HUD.

**Deliverables:**
- HUD: lives, health, coins, score, level name, power-up status, pause indicator
- Polished Pause / Game Over / Level Complete screens
- No timer (not in design)

---

## Phase 18 - Audio System

**Goal:** Centralized music + essential sound effects.

**Deliverables:**
- Audio manager with music + SFX systems
- Volume settings (master / music / SFX)
- SFX: jump, land, collect, damage, enemy defeat, power-up, death, level complete, UI
- Menu / gameplay BGM loops

---

## Phase 19 - Game Menus

**Goal:** Complete menu flow.

**Deliverables:**
- Main Menu: Start, Continue, Settings, Controls, Quit
- Audio settings (Master / Music / SFX)
- Controls reference screen
- Pause: Resume, Restart, Settings, Main Menu
- Game Over: Restart, Main Menu
- Lightweight Continue progress (`GameProgress`)

---

## Phase 20 - Level Progression

**Goal:** Connect individual levels into a campaign.

**Deliverables:**
- 3-level catalog (Lumen Meadows → Cascade Cliffs → Skybridge Spire)
- Unlock chain + completion tracking (`GameProgress`)
- Next-level progression after completion
- Level Select menu
- PlayerPrefs save for continue / unlocks / cleared flags

---

## Phase 21 - Save System

**Goal:** Allow the player to retain progress safely.

**Deliverables:**
- Central `SaveSystem` (Save / Load / New Game / Reset Save)
- Persist: completed levels, unlocks, continue point, best coins/score, audio settings
- Fail-safe primary + backup JSON with checksum
- Legacy PlayerPrefs migration
- Settings → Reset Save

---

## Phase 22 - Advanced Gameplay Systems

**Goal:** Implement only mechanics defined in the original game design.

**Deliverables:**
- Audit: all approved advanced systems already present from prior phases
- `ApprovedGameplayCatalog` lock (approved vs rejected)
- Validation setup for required prefabs/scripts
- Explicitly **not** added: secrets, switches, water, special movement, hidden collectibles

---

## Phase 23 - Boss System *(complete when approved)*

**Goal:** Create boss encounters only if the Game Design Specification requires them.

**Deliverables:**
- Design audit: bosses are **not** required (GDS enemies are Crawlbug / Hopmite / Spikewatch only)
- Explicitly **not** added: boss architecture, health, states, attacks, phases, arena, boss-gated completion
- `ApprovedGameplayCatalog` rejection entries for boss systems
- `Phase23BossSystemSetup` validation (asserts no Boss* scripts/prefabs)

**Does not include:** inventing a boss fight for polish or “platformer completeness.”

**Next content work:** Phase 24 authors unique Level 1–3 layouts using Phase 22 approved systems only.

---

## Phase 24 - Level Design *(complete when approved)*

**Goal:** Create the three campaign levels with a teaching difficulty curve.

**Deliverables:**
- Unique layouts for Lumen Meadows / Cascade Cliffs / Skybridge Spire
- Per-level theme, start, main path, platforms, enemies, collectibles, hazards, checkpoints, difficulty, goal
- `Phase24LevelDesignSetup` rebuilds scenes from approved prefabs + repairs `LevelCatalog`
- Explicitly **not** included: secrets / hidden areas (Phase 22 lock)

**Difficulty:** Level 1 tutorial → Level 2 introduces bounce/one-way/fire/Hopmite/Spikewatch → Level 3 movers, flyers, shooters, tighter gaps.

---

## Phase 25 - Art and Visual Polish *(complete when approved)*

**Goal:** Replace development placeholders with approved geometric stylized assets.

**Deliverables:**
- Player / enemy / environment / item / UI / VFX / background sprites (PPU 32, Point filter)
- Pip animation frames re-bound; distinct enemy silhouettes
- Per-level `LevelBackdrop` sky + hills (visual only)
- HUD / menu chrome sprites
- Ground tile sprite wired
- Explicitly **does not** change gameplay systems, colliders, or Phase 24 layouts

---

## Phase 26 - Visual Effects and Game Juice *(complete when approved)*

**Goal:** Satisfying feedback without confusing gameplay.

**Deliverables:**
- Camera shake (`CameraShake2D`) integrated with follow
- Sprite bursts for jump/land/hurt/death/collect/defeat/power-up/complete
- Player / enemy / bootstrap juice components wired to existing events
- Explicitly **not** added: ParticleSystems, hitstop, excessive screen flash

---

## Phase 27 - Game Balancing *(complete when approved)*

**Goal:** Tune difficulty and pacing across player feel, enemies, and level placements.

**Deliverables:**
- Player coyote/buffer + Glow Shield duration tweaks
- Dartling / Hopmite / Spitter difficulty retune
- Fairer L1/L3 checkpoint spacing and enemy/hazard placement
- Boss difficulty: **N/A** (Phase 23 — not in design)
- Setup rebuilds layouts then restores Phase 25–26 visuals/juice

---

## Phase 28 - Bug Fixing *(complete when approved)*

**Goal:** Dedicated bug-fixing pass — no new features.

**Deliverables:**
- Camera shake / dead-zone follow drift fix
- Continue index advances on level complete
- LevelLoader failure clears GSM transition lock
- VFX burst counter leak fix across scene loads
- Respawn facing + control-lock clear
- Stomp Stay i-frame bounce fix
- Level-complete juice rebind fix

---

## Phase 29 - Performance Optimization *(complete when approved)*

**Goal:** Improve performance without changing gameplay; optimize only where necessary.

**Deliverables:**
- HUD power-up/pause refresh GC reduction
- Pooled `SimpleBurstVfx` (cap 12)
- Event-driven juice rebind (idle Update only while unbound)
- Idle-disable for camera shake + health i-frame timers
- Release builds skip debug-kill Update
- Art texture import max-size clamps
- Explicitly **no** gameplay/physics/system rewrites

---

## Phase 30 - Full Playtest *(complete when approved)*

**Goal:** Play the campaign end-to-end and record issues — no new features.

**Deliverables:**
- Coverage matrix (levels / enemies / power-ups / hazards / checkpoints / menus)
- Flow results (New Game, save/load, restart, Game Over, L3 Finish)
- Boss playtest: **N/A** (Phase 23)
- Recorded bugs / UX / audio / visual / performance / confusion notes
- Interactive Play Mode checklist for Editor confirmation
- Explicitly **no** fixes in this phase (record only)

---

## Phase 31 - Final Polish *(complete when approved)*

**Goal:** Fix remaining issues and finalize feel — no major new features.

**Deliverables:**
- Heart Drop refused at full HP
- Campaign `LevelEnd` no longer double-triggers with `Exit_Goal`
- Campaign Complete UI copy on last level
- Coin + per-power-up SFX wiring; checkpoint/bounce feedback
- Phase 24 doc live-layout note
- Explicitly **no** credits scene, new levels, or balance rewrites

---

## Phase 32 - Final Build *(complete when approved)*

**Goal:** Prepare and ship the final playable Windows build.

**Deliverables:**
- Shipping logs/debug gated for Release
- Unused legacy prefabs / orphan Collect SFX removed
- Build Settings verified (5 campaign scenes; Gameplay sandbox out)
- Windows x64 Release build via Phase 32 menu / batchmode
- Final-build smoke-test checklist (exe ≠ Editor)

---

## Phase 33 - Visual Upgrade *(complete when approved)*

**Goal:** Make Bounder Trail look like a real platformer (Mario-inspired readability) without changing gameplay.

**Deliverables:**
- Wire sprites onto all gameplay prefabs (fix null-sprite invisibility)
- Richer grass-block / enemy / Pip / backdrop art (original Bounder look)
- Tiled platforms matching collider world size
- Cloud near-layer parallax + Pixel Perfect Camera on campaign levels
- Explicitly **no** feel retune, level redesign, or Mario IP

---

## Phase 34 - Main Menu Scale + Player Feel *(complete when approved)*

**Goal:** Bigger readable main menu + Mario-inspired movement retune.

**Deliverables:**
- CanvasScaler + larger buttons/fonts; Settings+Controls merged into **Options**
- Snappier walk/run/accel; higher fall gravity; apex hang at jump peak
- Explicitly **no** level redesign or new systems

---

## Phase 35 - Campaign Level Polish

**Goal:** Readable campaign levels with fair hazard placement, progressive difficulty, and a wider gameplay viewport.

**Deliverables:**
- Re-authored L1–L3 layouts (pit/spike/fire helpers anchored to platforms)
- Decor clusters + ground tile strips; larger parallax backdrops
- Gameplay orthographic **7.5** (640×480 ref @ 32 PPU)
- `Bounder Trail → Phase 35 → Setup Level Polish`

**Explicitly unchanged:** player feel (Phase 34), main menu (Phase 34)

---

## Roadmap Notes

1. Phase titles/details may be refined when a phase starts, but order remains sequential.
2. If your separately pasted master roadmap differs, **your pasted roadmap wins** — update this file in that phase’s approval cycle.
3. Future ideas may be listed during phase reports; they are not automatically scheduled.
4. No phase starts automatically after completion.

---

## Phase 0 Completion Checklist

- [x] Game concept defined
- [x] Core loop defined
- [x] Platform / resolution / frame rate defined
- [x] Engine / C# / input / physics / rendering defined
- [x] Asset / audio / save strategy defined
- [x] Level structure and level count defined
- [x] Art style and overall scope defined
- [x] Spec docs created
- [x] Architecture plan created
- [x] Folder structure created
- [x] Roadmap created
