# Phase 27 — Game Balancing

## Goal

Tune difficulty and pacing so the campaign teaches → challenges → finishes fairly.

## Boss difficulty

**N/A.** Bosses are not in the design (Phase 23 lock). No boss tuning.

## Audit findings → changes

| Area | Finding | Change |
|------|---------|--------|
| Player coyote/buffer | Edge deaths felt harsh | Coyote **0.12**, buffer **0.14** |
| Glow Shield | Expired mid L3 finale | Duration **5 → 6.5s**; pickup moved later |
| Dartling | Too fast on narrow bridges (**4.5**) | Speed **3.6**; L3 starts with Crawlbug on Bridge_A |
| Hopmite | Jump spam (**1.1s**) | Interval **1.45s** |
| Spitter | Wide cover + fast fire | Interval **1.85s**, sensor **5.5** |
| L1 gap / combat | Combat at spawn; late first CP | Easier gap, Crawlbug delayed, **Checkpoint_Early** |
| L2 Spikewatch | Tight after fire | Gate moved farther right |
| L3 finale | ~21u CP→exit with Spitter+spikes | **Checkpoint_C**, slower mover/spike, shield nearer exit |
| Lives / HP / jump height | Already fair | Unchanged (3 / 3 / 15) |

## Locked player baseline (post-tune)

| Stat | Value |
|------|-------|
| Walk / Run | 6.5 / 9.5 |
| Jump force | 15 |
| Gravity / fall mult | 3.2 / 1.55 |
| Coyote / buffer | **0.12 / 0.14** |
| Max HP / i-frames | 3 / 1.25s |
| Speed Burst | 5s × 1.45 |
| Glow Shield | **6.5s** |
| Starting lives | 3 |

## Per-level balance intent

### Lumen Meadows (easy)
- Teach walk → jump gap → stomp → hazards
- 2 Crawlbugs, 2 checkpoints, 1 heart, late spikes
- No movers / flyers / spitters

### Cascade Cliffs (medium)
- Introduce bounce, one-way, Hopmite, fire, Spikewatch
- 2 checkpoints, Speed + Heart, readable climb

### Skybridge Spire (hard but fair)
- Bridges → mover → aerials → Spitter finale
- 3 checkpoints, all three power-ups
- Dartling on wide landing; Crawlbug teaches first bridge

## Look-fors addressed

| Issue | Mitigation |
|-------|------------|
| Frustrating early deaths | Earlier L1 CP, delayed first enemy, coyote |
| Unfair L3 stretch | Checkpoint_C + shield placement |
| Difficulty spike (Dartling) | Slower + better shelf |
| Mechanics never useful | Bounce/one-way/shield remain on critical paths |
| Exploits | No HP/invuln buffs that break combat |
| Boring sections | Kept coin trails and optional height coins |

## Explicitly unchanged

- Core combat rules (1 HP enemies except Spikewatch unstompable)
- Hazard damage amounts
- Coin values
- Boss systems (still rejected)

## Setup

`Bounder Trail → Phase 27 → Setup Game Balancing`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase27GameBalancingSetup.SetupGameBalancing`

This retunes prefabs, rebuilds Phase 24 layouts, then re-applies Phase 25 art backdrops and Phase 26 juice wiring.
