# Phase 38 — Feel Polish (Hitstop, Squash, Fades, Juice Hierarchy)

## Goal

Make impacts and transitions feel weightier without changing gameplay rules.

## Deliverables

| System | Behavior |
|--------|----------|
| **HitStop** | Short realtime freeze on hurt / death / enemy defeat / level complete |
| **PlayerSquashStretch** | Jump stretch + land squash on Pip |
| **ScreenFade** | Soft black fade on menu ↔ gameplay and level → level loads |
| **Juice hierarchy** | Quieter coin sparkles; louder stomp / death / complete bursts + shake |
| **UiButtonPunch** | Scale punch on overlay / menu buttons |

## Scripts

- `Assets/Scripts/Vfx/HitStop.cs`
- `Assets/Scripts/Player/PlayerSquashStretch.cs`
- `Assets/Scripts/UI/ScreenFade.cs`
- `Assets/Scripts/UI/UiButtonPunch.cs`

Bootstrap auto-adds `HitStop` + `ScreenFade`. LevelLoader / GameStateManager drive fades.

## Setup

`Bounder Trail → Phase 38 → Setup Feel Polish`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase38FeelPolishSetup.SetupFeelPolish`

## Explicitly unchanged

- Movement numbers (Phase 34)
- Combat damage / i-frames / enemy AI
- Overlay font sizes (Phase 37)

## Acceptance

- [ ] Stomp / death has a tiny freeze
- [ ] Pip squash/stretch on jump & land
- [ ] Level loads fade instead of hard cuts
- [ ] Menu / overlay buttons punch on click
