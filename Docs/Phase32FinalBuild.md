# Phase 32 — Final Build

## Goal

Prepare and ship the final playable Windows build. Editor Play Mode ≠ final player — always smoke-test the exe.

## Prep applied

| Task | Result |
|------|--------|
| Unused assets | Removed legacy `Ground_Basic` / `Platform_Basic`, orphan `SFX_Collect.wav` |
| Debug | Kill key + G/C shortcuts compile out of Release; prefab kill flag off |
| Verbose logs | Bootstrap `log*` flags **0**; `GameLog.EnableInfo` off in shipping players |
| Scenes | Build: Bootstrap → MainMenu → L1 → L2 → L3 (`Gameplay.unity` **out**) |
| Settings | Company `MGames`, Product `Bounder Trail`, 1920×1080, Full Screen Window |
| Save | `%USERPROFILE%\AppData\LocalLow\MGames\Bounder Trail\` JSON + backup |
| Controls | A/D or ←/→ move, Space jump, Left Shift run, Esc pause |

## Build output

`Builds/Windows/BounderTrail.exe` (+ `BounderTrail_Data/`)

Release Windows x64 — **not** a Development Build.

## Setup / build commands

Editor menus:

- `Bounder Trail → Phase 32 → Prepare Final Build`
- `Bounder Trail → Phase 32 → Validate Final Build`
- `Bounder Trail → Phase 32 → Build Windows Player`

Batchmode:

```text
Unity.exe -batchmode -nographics -quit -projectPath <PROJECT> ^
  -executeMethod BounderTrail.EditorTools.Phase32FinalBuildSetup.BuildWindowsPlayer ^
  -logFile Builds/Windows/build.log
```

## Final build smoke test

Run the **exe** (not the Editor):

1. Boots to Main Menu (Bootstrap → Menu)
2. New Game → Lumen Meadows loads
3. A/D move, Space jump, Shift run, Esc pause
4. K does **nothing** (Release)
5. Quit → relaunch → Continue / Level Select still work
6. Settings volumes persist across relaunch
7. No Info log flood in `Player.log`

Editor saves do **not** carry into the player (`persistentDataPath` differs).

### Smoke result (this machine)

| Check | Result |
|-------|--------|
| Build | **SUCCEEDED** — `Builds/Windows/BounderTrail.exe` (~95.5 MB total) |
| Launch | Process started; D3D12 + PhysX init OK |
| Boot flow | Bootstrap loaded then unloaded unused assets (menu transition) |
| Info spam | No `[BounderTrail]` Info lines in `Player.log` (shipping quiet) |
| Fatal errors | None observed in smoke window |

Human still should run New Game → L1 controls / pause / save Continue in the exe.

## Kept on purpose

| Asset | Why |
|-------|-----|
| `Gameplay.unity` | Editor sandbox; not in Build Settings |
| Placeholder art | Editor setup scripts may regenerate |
| `Assets/Editor`, `Docs`, `Tests` | Not packed into player |
