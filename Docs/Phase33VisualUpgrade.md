# Phase 33 — Visual Upgrade

## Goal

Make Bounder Trail **look like a real playable platformer** (Mario-inspired readability)
while **upgrading Bounder Trail** — not copying Mario IP, not changing gameplay systems.

## Why this phase

Phase 25 generated geometric PNGs but **never wired them onto gameplay prefabs**.
Most platforms / enemies / items rendered with **null sprites**, so the shipping build looked empty/broken.

## Scope (this phase only)

| Do | Do not |
|----|--------|
| Richer Mario-inspired stylized sprites (original Bounder look) | Copy Mario art, music, or levels |
| Wire sprites onto all gameplay prefabs + scene instances | Change movement / combat / layouts |
| Tile platforms to match collider world size | Feel/physics retune (later phase) |
| Cloud near-layer + richer skies | Redesign level courses |
| Pixel Perfect Camera on campaign cams | New systems / enemies / menus |

## Art direction

- Style: chunky readable 2D blocks (grass-top dirt, brick accents) — Bounder cyan Pip
- PPU 32, Point filter
- Platforms use **Tiled** draw mode so long ledges look like repeating blocks

## Setup

`Bounder Trail → Phase 33 → Setup Visual Upgrade`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase33VisualUpgradeSetup.SetupVisualUpgrade`

Validate:

`Bounder Trail → Phase 33 → Validate Visual Upgrade`

## Acceptance

- [x] Prefabs no longer have null gameplay sprites
- [x] Platforms visible as tiled blocks matching colliders
- [x] Enemies / items / hazards / checkpoint / exit visible
- [x] Pip idle sprite assigned; anim clips still drive poses
- [x] Campaign levels have sky + hills + clouds backdrop
- [x] Pixel Perfect Camera on L1–L3 cameras
- [x] No gameplay script rewrites beyond `LevelBackdrop` near-layer fields
