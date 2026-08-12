# Phase 12 — Collectibles

## Goal

Reusable collectible objects (coins) that are easy to place, detect the player, play feedback, and notify the game counter.

## Scripts

| File | Role |
|------|------|
| `CollectibleKind.cs` | Collectible categories (`Coin`) |
| `CollectiblePickupInfo.cs` | Event payload on pickup |
| `Collectible.cs` | Trigger detection, collection state, effect, sound, notify |
| `CollectibleIdleMotion.cs` | Bob / spin idle motion |
| `CollectibleCounter.cs` | Coin + score totals + `Collected` / `CountsChanged` events |
| `CollectibleCounterUI.cs` | Minimal on-screen counter (full HUD later) |

## Prefab

- `Assets/Prefabs/Items/Item_Coin.prefab`
- Tag: `Coin`
- Layer: `Pickup`
- Trigger `CircleCollider2D`
- Defaults: **1 coin**, **10 score**

## Audio

- `Assets/Audio/SFX/SFX_Coin.wav` — short pickup blip (placeholder)

## Flow

1. Player enters coin trigger.
2. `Collectible` marks collected (cannot re-collect).
3. `CollectibleCounter.RegisterCollection(...)` updates totals and raises events.
4. Collect SFX plays; sprite pops + fades; object destroys.
5. UI refreshes from `CountsChanged`.

## Counter reset

Resets on:
- Level load start (`LevelLoader`)
- Fresh enter into Gameplay from Main Menu / Game Over / Level Complete / Boot

Does **not** reset on Pause → Resume.

## Setup

`Bounder Trail → Phase 12 → Setup Collectibles`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase12CollectiblesSetup.SetupCollectibles`

## Out of scope

- Shops / economy systems
- Power-ups
- Final HUD art (Phase 13+)
