# Phase 25 — Art and Visual Polish

## Goal

Replace development placeholders with approved geometric stylized art while
**not** changing gameplay systems.

## Art direction (locked)

| Rule | Choice |
|------|--------|
| Style | Clean 2D geometric / simple stylized (GDS §11) |
| Palette | Bright, high-contrast, readable silhouettes |
| Scale | PPU 32, Point filter, consistent body sizes |
| Gameplay | Colliders, prefab scales, and scripts unchanged |

## Palette

| Role | Color intent |
|------|----------------|
| Pip | Cyan body, bright face plate, dark eyes |
| Ground / solid | Mid green face + lighter top lip |
| One-way | Soft mint ledge |
| Moving | Teal block |
| Bounce | Orange pad |
| Coin | Gold disc |
| Speed / Shield / Heart | Amber bolt / cyan ring / pink heart |
| Crawlbug / Dartling / Hopmite | Red beetle / orange dart / lime hopper |
| Skimmer / Spikewatch / Spitter | Sky oval / gray spike diamond / purple turret |
| Hazards | Dark pit, gray spikes, orange flame |
| UI | Dark panel + meadow-green buttons |

## Deliverables

| Area | Result |
|------|--------|
| Player | Geometric Pip frames for Idle/Walk/Run/Jump/Fall/Land/Death |
| Enemies | Distinct silhouettes (readable at gameplay scale) |
| Environment | Platform, bounce, exit, checkpoint, ground tile art |
| Items | Coin + power-up icons |
| Backgrounds | Per-level sky + hill layers via `LevelBackdrop` |
| Effects | Soft spark / dust sprites (collect/hurt systems already juice) |
| UI | Panel + button chrome sprites applied to menus/HUD |
| Animations | Pip clips re-bound to polished frames |

## Explicitly unchanged

- Movement, combat, hazards, progression, save, audio logic
- Prefab collider sizes / transform scales
- Level layouts from Phase 24

## Setup

`Bounder Trail → Phase 25 → Setup Art And Visual Polish`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase25ArtPolishSetup.SetupArtAndVisualPolish`

## Checks

- [x] Consistent geometric style
- [x] Consistent PPU / proportions
- [x] Readable player / enemies / hazards / collectibles
- [x] Theme-tinted skies per level
- [x] No gameplay system rewrites

## Next

Phase 26 adds short sprite bursts + camera shake for feedback (no ParticleSystems).
