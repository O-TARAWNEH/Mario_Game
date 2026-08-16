# Phase 42 — Complete Polish Pass

## Goal

Upgrade Bounder Trail’s existing systems for snappier movement, meaningful collectibles, clearer feedback, and smoother campaign flow — without rebuilding the game.

## What improved

| Area | Change |
|------|--------|
| **Player movement** | Synced Mario-feel defaults; Space / W / Up jump; longer jump buffer; ignore false landings while rising |
| **Game feel** | Impact-scaled land dust / squash; soft landings stay quiet; SFX pitch variation |
| **Enemies** | Contact-normal stomp detection; quieter AI state logging |
| **Collectibles** | Coins persist across levels in a run; **25 coins = 1UP**; HUD shows coins until next life |
| **Levels** | Optional secret coins on all 5 campaign levels |
| **UI / flow** | **R** quick-restart; campaign summary shows bests + bonus lives; Options lists new controls |
| **Audio** | Victory BGM on campaign clear; BonusLife SFX |
| **Bugs** | Fixed coin wipe on every level load (broke progression totals) |

## Setup

`Bounder Trail → Phase 42 → Setup Complete Polish`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase42CompletePolishSetup.SetupCompletePolish`

This also re-runs Phase 41 repair so knight visuals and layouts stay consistent with secret coin placements.

## Acceptance

- [ ] Jump works with Space, W, and Up Arrow
- [ ] Coins carry from Level 1 → Level 2 (not wiped on load)
- [ ] Collecting 25 coins grants a retry + sparkle / SFX
- [ ] Soft landings do not spam land SFX
- [ ] Stomps feel reliable on Crawlbugs
- [ ] Campaign clear plays victory music and shows career bests
- [ ] Esc pauses; R restarts from play / pause / game over / complete
- [ ] Secret coins exist above optional paths (not required to finish)

## Explicitly unchanged

- Core state machine / save / level catalog architecture
- Enemy type roster (still Crawlbug / Hopmite / flyer / shooter set)
- Damage / i-frame rules
