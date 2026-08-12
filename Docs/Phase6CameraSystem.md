# Phase 6 — Camera System

## Goal

Reliable 2D side-scrolling camera that follows Pip smoothly inside level bounds.

## Scripts

| File | Role |
|------|------|
| `Assets/Scripts/Camera/CameraFollow2D.cs` | Smooth follow, dead zone, clamp |
| `Assets/Scripts/Levels/LevelBounds.cs` | Level rectangle + camera clamp helper |

## Behavior

- Tracks player (tag `Player`, auto-find)
- Smooth horizontal + vertical follow (`SmoothDamp`)
- Dead zone reduces tiny camera jitter
- Clamps view so outside-level areas are not shown
- Snaps to player on Start

## Default tuning

| Property | Value |
|----------|-------|
| Smooth Time X | 0.12 |
| Smooth Time Y | 0.18 |
| Max Speed | 40 |
| Dead Zone | 1.8 x 1.1 |
| Focus Offset | (0, 0.75) |
| Orthographic Size | 5.5 |
| Level Bounds Size | 36 x 16 |

## Setup menu

`Bounder Trail → Phase 6 → Setup Camera System`
