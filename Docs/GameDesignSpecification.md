# Game Design Specification — Phase 0

**Document status:** Approved for Phase 0 (definition only)  
**Last updated:** 2026-08-10  
**Working title:** Bounder Trail  
**Protagonist:** Pip (original character — not Nintendo-related)

---

## 1. Game Concept

**Bounder Trail** is an original 2D side-scrolling platformer.

You play as **Pip**, a small explorer racing through the **Lumen Isles** — floating island paths filled with platforms, coins, simple enemies, hazards, and goals.

The feel targets classic arcade platforming: tight left/right movement, responsive jumping, readable hazards, and short levels that end at a clear goal flag/portal.

**Original IP rules (mandatory):**
- Original character names, enemy names, world names, and branding
- Original or placeholder art/audio only
- No Nintendo/Mario characters, names, sprites, music, SFX, levels, or logos

---

## 2. Core Gameplay Loop

1. Start a level from the main menu / level select.
2. Move, run, and jump across platforms.
3. Collect coins for score.
4. Avoid or defeat simple enemies.
5. Avoid hazards (pits, spikes, etc.).
6. Reach checkpoints to save progress within the level.
7. Reach the goal to complete the level.
8. Advance to the next level, or retry / return to menu on failure.

**Session loop:** Menu → Play Level → Complete or Fail → Next Level / Retry / Menu

---

## 3. Target Platform

| Item | Decision |
|------|----------|
| Primary target | PC / Desktop (Windows) |
| Input | Keyboard (optional gamepad later if requested) |
| Distribution | Local Unity build / Play Mode testing |
| Performance target | Comfortable on a normal laptop |

---

## 4. Player Fantasy & Tone

- Tone: bright, friendly, lightly adventurous
- Difficulty: approachable, with gradual challenge increase across levels
- Feedback: clear collisions, simple juice (SFX, brief visual feedback later)

---

## 5. Player Abilities (Planned Scope)

Implemented only in their assigned later phases:

- Move left / right
- Run (hold modifier or faster move speed)
- Jump / fall under gravity
- Collide with ground and platforms
- Collect items
- Take damage / lose a life
- Die and respawn (checkpoint-aware)
- Use simple temporary power-ups

---

## 6. Enemies (Planned Scope)

Several simple enemy types, for example:

| Working name | Behavior idea |
|--------------|---------------|
| Crawlbug | Walks back and forth on a platform |
| Hopmite | Small hop / bounce pattern |
| Spikewatch | Stationary or slow patrol hazard-like foe |

Enemy AI stays simple: patrol, turn at edges/walls, basic player contact rules, stomp/defeat where designed.

**Out of scope:** Boss encounters, multi-phase fights, and boss arenas.

---

## 7. Collectibles & Power-Ups (Planned Scope)

**Collectibles**
- Coins → score (+ optional coin count UI)

**Power-ups (simple)**
- Speed Burst — temporary move speed increase
- Glow Shield — temporary invincibility
- Heart Drop — restore **1 health** (clamped to max HP; lives/stock handled later if needed)

---

## 8. Hazards & Level Elements (Planned Scope)

- Solid ground and platforms
- Gaps / pits (fall death via death zones)
- Spikes or similar static hazards
- Ember / fire zones (damage while standing in them)
- Simple moving spike hazards
- Moving platforms (only if a later phase requires them)
- Checkpoints
- Level goal / exit

---

## 9. Level Structure

Each playable level should include:

- Player start spawn
- Readable platform path
- Coins
- At least one enemy type (after enemy phases)
- Optional hazards
- Optional checkpoint(s)
- A clear goal object

**Difficulty curve:** Level 1 tutorial-friendly → later levels add tighter jumps, more enemies, and denser hazards.

---

## 10. Planned Number of Levels

| Content | Count |
|---------|-------|
| Playable campaign levels | **3** |
| Optional stretch goal | +1–2 extra levels (only if requested later) |

**Working level names:**
1. Lumen Meadows
2. Cascade Cliffs
3. Skybridge Spire

---

## 11. Art Style

| Item | Decision |
|------|----------|
| Style | Clean 2D geometric / simple stylized sprites |
| Palette | Bright, high-contrast, readable silhouettes |
| Camera | Orthographic side view |
| Placeholders | Colored shapes / simple sprites first |
| Replacement | All art paths designed for easy swap later |

Art is support for gameplay clarity, not visual complexity.

---

## 12. UI / Screens (Planned Scope)

- Main Menu
- In-game HUD (score, coins, lives/health)
- Pause Menu
- Level Complete
- Game Over
- Restart / return to menu options

---

## 13. Audio Direction (Design Intent)

- Short, light background music loops per context (menu / gameplay)
- Simple SFX: jump, land, coin, hurt, enemy defeat, UI click, level complete
- Original or free/open-licensed audio only
- Placeholders acceptable until audio phase

---

## 14. Save System Requirements (Design Intent)

Minimum save needs for the finished game:

- Highest unlocked level
- Optional: best score / coins (if implemented in save phase)
- Settings later if requested (volume, etc.)

**Approach locked in Technical Spec:** lightweight local save (PlayerPrefs first).

---

## 15. Overall Game Scope

**In scope for the full project (across all phases):**
- Complete playable PC platformer
- Player movement, combat-lite interactions, collectibles, power-ups
- Multiple levels with progression
- Menus, HUD, pause, game over, level complete
- Basic audio and feedback
- Lightweight performance profile

**Out of scope unless explicitly requested later:**
- Online multiplayer
- Complex RPG systems
- Inventory crafting
- Cutscene cinema system
- Large open world
- Advanced shader/VFX stacks
- Mobile touch controls
- Nintendo asset mimicry

---

## 16. Success Criteria (Whole Project)

The project succeeds when a player can:

1. Launch from a main menu
2. Play through multiple levels
3. Collect coins, fight/avoid enemies, use power-ups
4. Pause, die/respawn, complete levels
5. Reach a game over or campaign completion state
6. Run comfortably on a normal Windows laptop
