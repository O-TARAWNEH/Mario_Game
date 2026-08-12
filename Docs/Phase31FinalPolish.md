# Phase 31 — Final Polish

## Goal

Fix remaining issues and make the game feel complete. **No major new features** — polish and finalize existing systems only.

## Checklist coverage

| Area | Polish applied |
|------|----------------|
| Controls | Unchanged (A/D, Space, Shift, Esc already consistent) |
| Animation | Unchanged (player/enemy animators already wired) |
| Visual consistency | Checkpoint first-reach burst; bounce squash |
| Audio consistency | Coin + per-power-up clips wired; CP/bounce SFX |
| UI | Campaign Complete title + summary on final level |
| Difficulty / pacing | Unchanged (Phase 27 lock) |
| Feedback | Heart refuse, CP/bounce juice, distinct power-up SFX |
| Loading | Unchanged (async LevelLoader) |
| Saving | Unchanged (Phase 21/28) |
| Performance | Unchanged (Phase 29) |

## Fixes from Phase 30

| # | Issue | Polish |
|---|-------|--------|
| 1 | Heart Drop eaten at full HP | `TryHeal` / `TryActivate` returns false; pickup stays |
| 2 | Dual `Exit_Goal` + `LevelEnd` triggers | Campaign `LevelEnd.completeLevelOnEnter = false` (+ PlaceGoal) |
| 3 | Abrupt L3 ending | **CAMPAIGN COMPLETE** title + “Campaign cleared!” summary; Finish still → menu |
| 4 | Unused coin / power-up SFX | `Collect` → `SFX_Coin`; Heart/Shield/Speed ids wired |
| 5 | Silent checkpoint / bounce | First CP: Ui SFX + burst; bounce: Jump SFX + squash |
| 6 | Phase 24 doc drift | Note pointing to Phase 27 live layouts |

## Explicitly not added

- Credits scene / cutscenes
- New levels, bosses, systems
- Balance retunes
- Glow Shield vs InstantKill redesign
- New music themes for Game Over / Complete

## Setup / validation

`Bounder Trail → Phase 31 → Apply Final Polish`  
`Bounder Trail → Phase 31 → Validate Final Polish`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase31FinalPolishSetup.ApplyFinalPolish`  
`-executeMethod BounderTrail.EditorTools.Phase31FinalPolishSetup.ValidateFinalPolish`
