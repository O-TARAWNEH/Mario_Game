# Phase 29 — Performance Optimization

## Goal

Improve performance **without changing gameplay**. Optimize only where necessary.

## Audit checklist

| Area | Finding | Action |
|------|---------|--------|
| Frame rate | Target 60 already set in `GameBootstrap` | Unchanged |
| CPU — idle Updates | Health / shake / juice / HUD did needless per-frame work | Disable or gate when idle |
| CPU — debug poll | `PlayerDeath` debug kill key polled in shipping builds | Editor/Dev only |
| Memory — textures | Tiny sprites imported with **2048** max | Clamp import max to asset size |
| Memory / GC — HUD | Power-up + pause refreshed / allocated every frame | Dirty-flag + 0.1s text cadence |
| GC — VFX | `SimpleBurstVfx` Instantiate/Destroy per burst | Small fixed pool (cap 12) |
| Object counts | Small campaign levels; no runaway spawners | No change |
| Physics | Already NonAlloc sensors; simple Rigidbody2D | No change |
| Rendering | Point-filtered small sprites; no ParticleSystems | No change |
| Asset sizes | Art PNGs are tiny (≤256px) | Import caps only |
| Scene loading | Already `LoadSceneAsync` | No change |
| Garbage collection | Burst spawn + HUD strings were the hotspots | Pool + HUD gate |

## Changes applied

1. **`GameplayHud`** — pause indicator is event-driven; power-up text updates only while timed power-ups are active and only when the displayed 0.1s value changes.
2. **`SimpleBurstVfx`** — reuse up to 12 pooled burst objects instead of create/destroy.
3. **`GameplayVisualJuice`** — rebinds on `LevelLoader.LevelLoadCompleted`; Update runs only while unbound (Phase 28 safety kept).
4. **`CameraShake2D`** — component disabled while trauma is zero.
5. **`PlayerHealth` / `EnemyHealth`** — Update disabled while i-frame/hurt timers are idle.
6. **`PlayerDeath`** — debug kill `Update` compiled out of non-dev builds.
7. **Art texture imports** — max size clamped to category (backgrounds ≤256, most sprites ≤128, VFX ≤64).

## Explicitly not changed

- Player / enemy movement, combat, physics tuning
- Level layouts, spawn counts, AI behavior
- No new pooling for projectiles (rare Spitter fire; not worth the risk)
- No physics layer matrix rewrite
- No system rewrites for theoretical gains

## Setup / validation

`Bounder Trail → Phase 29 → Apply Performance Optimizations`  
`Bounder Trail → Phase 29 → Validate Performance Optimizations`

Batchmode:

`-executeMethod BounderTrail.EditorTools.Phase29PerformanceSetup.ApplyPerformanceOptimizations`  
`-executeMethod BounderTrail.EditorTools.Phase29PerformanceSetup.ValidatePerformanceOptimizations`
