# Continue phase commits from Phase 11 on existing phase-history tip (ab22d04).
$ErrorActionPreference = "Continue"
Set-Location "C:\Users\omart\OneDrive\Desktop\MGames\FORTH_GAME"

function Wait-GitReady {
    for ($i = 0; $i -lt 40; $i++) {
        if (-not (Test-Path ".git/index.lock")) { return }
        Start-Sleep -Milliseconds 250
    }
    if (Test-Path ".git/index.lock") { Remove-Item -Force ".git/index.lock" }
}

function Add-Paths([string[]]$paths) {
    foreach ($p in $paths) {
        if ([string]::IsNullOrWhiteSpace($p)) { continue }
        if (Test-Path -LiteralPath $p) {
            Wait-GitReady
            git add -f -- $p | Out-Null
        }
    }
}

function Add-Glob([string]$pattern) {
    $items = @(Get-ChildItem -Path $pattern -Recurse -File -ErrorAction SilentlyContinue)
    if ($items.Count -eq 0) { return }
    Wait-GitReady
    $rels = @($items | ForEach-Object { $_.FullName.Substring((Get-Location).Path.Length + 1) })
    if ($rels.Count -gt 0) {
        git add -f -- @rels | Out-Null
    }
}

function Commit-IfStaged([string]$message) {
    Wait-GitReady
    $staged = @(git diff --cached --name-only)
    if ($staged.Count -eq 0) {
        Write-Host "SKIP: $($message.Split([char]10)[0])"
        return
    }
    $tmp = Join-Path $env:TEMP "forth_commit_msg.txt"
    [System.IO.File]::WriteAllText($tmp, $message.Trim() + "`n")
    Wait-GitReady
    git commit -F $tmp | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Commit failed: $($message.Split([char]10)[0])" }
    Write-Host "OK: $(git log -1 --format='%h %s')"
    Start-Sleep -Milliseconds 200
}

Write-Host "Branch=$(git branch --show-current) HEAD=$(git rev-parse --short HEAD)"

Add-Paths @(
    "Docs/Phase11PlayerEnemyInteractions.md",
    "Assets/Editor/Phase11CombatInteractionsSetup.cs","Assets/Editor/Phase11CombatInteractionsSetup.cs.meta",
    "Assets/Scripts/Player/PlayerHealth.cs","Assets/Scripts/Player/PlayerHealth.cs.meta",
    "Assets/Scripts/Player/PlayerHurtFeedback.cs","Assets/Scripts/Player/PlayerHurtFeedback.cs.meta"
)
Commit-IfStaged "Phase 11: Define player and enemy combat interactions.`n`nWire damage, stomp, knockback, i-frames, and hurt/death feedback between player and enemies."

Add-Paths @(
    "Docs/Phase12Collectibles.md",
    "Assets/Editor/Phase12CollectiblesSetup.cs","Assets/Editor/Phase12CollectiblesSetup.cs.meta",
    "Assets/Scripts/Items.meta","Assets/Scripts/Items/.gitkeep",
    "Assets/Scripts/Items/Collectible.cs","Assets/Scripts/Items/Collectible.cs.meta",
    "Assets/Scripts/Items/CollectibleCounter.cs","Assets/Scripts/Items/CollectibleCounter.cs.meta",
    "Assets/Scripts/Items/CollectibleIdleMotion.cs","Assets/Scripts/Items/CollectibleIdleMotion.cs.meta",
    "Assets/Scripts/Items/CollectibleKind.cs","Assets/Scripts/Items/CollectibleKind.cs.meta",
    "Assets/Scripts/Items/CollectiblePickupInfo.cs","Assets/Scripts/Items/CollectiblePickupInfo.cs.meta",
    "Assets/Scripts/UI/CollectibleCounterUI.cs","Assets/Scripts/UI/CollectibleCounterUI.cs.meta",
    "Assets/Prefabs/Items.meta"
)
Add-Glob "Assets/Prefabs/Items/*"
Commit-IfStaged "Phase 12: Add collectible coins and scoring hooks.`n`nIntroduce pickup detection, counters, and placeable coin prefabs that feed HUD/score systems."

Add-Paths @(
    "Docs/Phase13PowerUps.md",
    "Assets/Editor/Phase13PowerUpsSetup.cs","Assets/Editor/Phase13PowerUpsSetup.cs.meta",
    "Assets/Scripts/Items/PowerUpKind.cs","Assets/Scripts/Items/PowerUpKind.cs.meta",
    "Assets/Scripts/Items/PowerUpPickup.cs","Assets/Scripts/Items/PowerUpPickup.cs.meta",
    "Assets/Scripts/Player/PlayerPowerUps.cs","Assets/Scripts/Player/PlayerPowerUps.cs.meta",
    "Assets/Scripts/Player/PlayerPowerUpFeedback.cs","Assets/Scripts/Player/PlayerPowerUpFeedback.cs.meta"
)
Commit-IfStaged "Phase 13: Add Speed Burst, Glow Shield, and Heart Drop power-ups.`n`nImplement pickup activation, timed player power states, and placeable power-up prefabs."

Add-Paths @(
    "Docs/Phase14Hazards.md",
    "Assets/Editor/Phase14HazardsSetup.cs","Assets/Editor/Phase14HazardsSetup.cs.meta",
    "Assets/Scripts/World/EnvironmentalHazard.cs","Assets/Scripts/World/EnvironmentalHazard.cs.meta",
    "Assets/Scripts/World/HazardResponse.cs","Assets/Scripts/World/HazardResponse.cs.meta",
    "Assets/Scripts/World/MovingHazard.cs","Assets/Scripts/World/MovingHazard.cs.meta",
    "Assets/Prefabs/Hazards.meta"
)
Add-Glob "Assets/Prefabs/Hazards/*"
Commit-IfStaged "Phase 14: Add environmental hazards and damage zones.`n`nShip pits, spikes, fire zones, moving spikes, and shared hazard response/reset behavior."

Add-Paths @(
    "Docs/Phase15CheckpointsAndRespawn.md",
    "Assets/Editor/Phase15CheckpointsSetup.cs","Assets/Editor/Phase15CheckpointsSetup.cs.meta",
    "Assets/Scripts/Levels/Checkpoint.cs","Assets/Scripts/Levels/Checkpoint.cs.meta",
    "Assets/Scripts/Levels/RespawnSystem.cs","Assets/Scripts/Levels/RespawnSystem.cs.meta"
)
Commit-IfStaged "Phase 15: Add checkpoints and reliable respawn recovery.`n`nPersist spawn points, reset player/enemies/hazards on death, and restore run state cleanly."

Add-Paths @(
    "Docs/Phase16LevelCompletion.md",
    "Assets/Editor/Phase16LevelCompletionSetup.cs","Assets/Editor/Phase16LevelCompletionSetup.cs.meta",
    "Assets/Scripts/Levels/LevelCompletionService.cs","Assets/Scripts/Levels/LevelCompletionService.cs.meta"
)
Commit-IfStaged "Phase 16: Add level completion and next-level flow.`n`nDetect goals, freeze the run, enter Level Complete, then continue or return to menu."

Add-Paths @(
    "Docs/Phase17UserInterface.md",
    "Assets/Editor/Phase17HudSetup.cs","Assets/Editor/Phase17HudSetup.cs.meta",
    "Assets/Scripts/UI/GameplayHud.cs","Assets/Scripts/UI/GameplayHud.cs.meta",
    "Assets/Scripts/UI/FlowScreenSummary.cs","Assets/Scripts/UI/FlowScreenSummary.cs.meta",
    "Assets/Scripts/UI/HealthHeartsDisplay.cs","Assets/Scripts/UI/HealthHeartsDisplay.cs.meta",
    "Assets/Editor/Phase36HeartsHudSetup.cs","Assets/Editor/Phase36HeartsHudSetup.cs.meta"
)
Commit-IfStaged "Phase 17: Build gameplay HUD and polished flow screens.`n`nShow lives/health/coins/score/power-up status and clarify Pause, Game Over, and Level Complete panels."

Add-Paths @(
    "Docs/Phase18Audio.md",
    "Assets/Editor/Phase18AudioSetup.cs","Assets/Editor/Phase18AudioSetup.cs.meta",
    "Assets/Scripts/Audio.meta",
    "Assets/Scripts/Audio/AudioManager.cs","Assets/Scripts/Audio/AudioManager.cs.meta",
    "Assets/Scripts/Audio/MusicSystem.cs","Assets/Scripts/Audio/MusicSystem.cs.meta",
    "Assets/Scripts/Audio/MusicId.cs","Assets/Scripts/Audio/MusicId.cs.meta",
    "Assets/Scripts/Audio/SfxSystem.cs","Assets/Scripts/Audio/SfxSystem.cs.meta",
    "Assets/Scripts/Audio/SfxId.cs","Assets/Scripts/Audio/SfxId.cs.meta",
    "Assets/Scripts/Player/PlayerAudioFeedback.cs","Assets/Scripts/Player/PlayerAudioFeedback.cs.meta",
    "Assets/Audio.meta","Assets/Audio/Music.meta","Assets/Audio/SFX.meta"
)
Add-Glob "Assets/Audio/Music/*"
Add-Glob "Assets/Audio/SFX/*"
Commit-IfStaged "Phase 18: Add centralized music and essential SFX.`n`nWire AudioManager volume buses plus menu/gameplay BGM and core gameplay sound cues."

Add-Paths @(
    "Docs/Phase19GameMenus.md",
    "Assets/Editor/Phase19MenusSetup.cs","Assets/Editor/Phase19MenusSetup.cs.meta",
    "Assets/Scripts/UI/AudioSettingsView.cs","Assets/Scripts/UI/AudioSettingsView.cs.meta",
    "Assets/Scripts/Save.meta","Assets/Scripts/Save/.gitkeep",
    "Assets/Scripts/Save/GameProgress.cs","Assets/Scripts/Save/GameProgress.cs.meta"
)
Commit-IfStaged "Phase 19: Complete main menu and pause settings flow.`n`nAdd Start/Continue/Settings/Controls/Quit plus in-pause audio options and lightweight continue progress."

Add-Paths @(
    "Docs/Phase20LevelProgression.md",
    "Assets/Editor/Phase20LevelProgressionSetup.cs","Assets/Editor/Phase20LevelProgressionSetup.cs.meta",
    "Assets/Scripts/UI/LevelSelectView.cs","Assets/Scripts/UI/LevelSelectView.cs.meta"
)
Commit-IfStaged "Phase 20: Connect campaign levels into unlock progression.`n`nAdd 3-level catalog flow, level select, and continue/unlock tracking across clears."

Add-Paths @(
    "Docs/Phase21SaveSystem.md",
    "Assets/Editor/Phase21SaveSystemSetup.cs","Assets/Editor/Phase21SaveSystemSetup.cs.meta",
    "Assets/Scripts/Save/SaveSystem.cs","Assets/Scripts/Save/SaveSystem.cs.meta",
    "Assets/Scripts/Save/SaveData.cs","Assets/Scripts/Save/SaveData.cs.meta"
)
Commit-IfStaged "Phase 21: Persist progress with a durable save system.`n`nSave unlocks, continue point, bests, and audio settings with backup/checksum safety and reset support."

Add-Paths @(
    "Docs/Phase22AdvancedGameplaySystems.md",
    "Assets/Editor/Phase22AdvancedSystemsSetup.cs","Assets/Editor/Phase22AdvancedSystemsSetup.cs.meta",
    "Assets/Scripts/Data/ApprovedGameplayCatalog.cs","Assets/Scripts/Data/ApprovedGameplayCatalog.cs.meta"
)
Commit-IfStaged "Phase 22: Lock approved advanced gameplay systems.`n`nAudit design-approved mechanics and reject out-of-scope systems so later polish stays focused."

Add-Paths @(
    "Docs/Phase23BossSystem.md",
    "Assets/Editor/Phase23BossSystemSetup.cs","Assets/Editor/Phase23BossSystemSetup.cs.meta"
)
Commit-IfStaged "Phase 23: Confirm bosses are out of scope.`n`nValidate design rejection of boss fights and keep campaign content on approved enemy types only."

Add-Paths @(
    "Docs/Phase24LevelDesign.md",
    "Assets/Editor/Phase24LevelDesignSetup.cs","Assets/Editor/Phase24LevelDesignSetup.cs.meta"
)
Add-Glob "Assets/Scenes/*"
Commit-IfStaged "Phase 24: Author the three campaign level layouts.`n`nBuild Lumen Meadows, Cascade Cliffs, and Skybridge Spire with progressive difficulty and scene wiring."

Add-Paths @(
    "Docs/Phase25ArtAndVisualPolish.md",
    "Assets/Editor/Phase25ArtPolishSetup.cs","Assets/Editor/Phase25ArtPolishSetup.cs.meta",
    "Assets/Scripts/World/LevelBackdrop.cs","Assets/Scripts/World/LevelBackdrop.cs.meta",
    "Assets/Art.meta","Assets/Data/Configs.meta","Assets/Data/ScriptableObjects.meta",
    "Assets/DefaultVolumeProfile.asset","Assets/DefaultVolumeProfile.asset.meta",
    "Assets/UniversalRenderPipelineGlobalSettings.asset","Assets/UniversalRenderPipelineGlobalSettings.asset.meta"
)
Add-Glob "Assets/Art/*"
Add-Glob "Assets/Data/Configs/*"
Add-Glob "Assets/Data/ScriptableObjects/*"
Commit-IfStaged "Phase 25: Replace placeholders with stylized art polish.`n`nBind player/enemy/world/UI sprites, ground tiles, and per-level backdrop visuals without changing gameplay systems."

Add-Paths @(
    "Docs/Phase26VisualEffects.md",
    "Assets/Editor/Phase26VisualEffectsSetup.cs","Assets/Editor/Phase26VisualEffectsSetup.cs.meta",
    "Assets/Scripts/Camera/CameraShake2D.cs","Assets/Scripts/Camera/CameraShake2D.cs.meta",
    "Assets/Scripts/Vfx.meta",
    "Assets/Scripts/Vfx/GameplayVisualJuice.cs","Assets/Scripts/Vfx/GameplayVisualJuice.cs.meta",
    "Assets/Scripts/Vfx/SimpleBurstVfx.cs","Assets/Scripts/Vfx/SimpleBurstVfx.cs.meta",
    "Assets/Scripts/Player/PlayerVisualJuice.cs","Assets/Scripts/Player/PlayerVisualJuice.cs.meta",
    "Assets/Scripts/Enemies/EnemyDefeatVisualJuice.cs","Assets/Scripts/Enemies/EnemyDefeatVisualJuice.cs.meta"
)
Commit-IfStaged "Phase 26: Add visual juice and feedback effects.`n`nIntroduce camera shake and sprite bursts for jump, hurt, collect, defeat, and level-complete moments."

Add-Paths @(
    "Docs/Phase27GameBalancing.md",
    "Assets/Editor/Phase27GameBalancingSetup.cs","Assets/Editor/Phase27GameBalancingSetup.cs.meta"
)
Commit-IfStaged "Phase 27: Retune difficulty and pacing across the campaign.`n`nBalance player feel, enemy pressure, and checkpoint spacing for a fairer learning curve."

Add-Paths @(
    "Docs/Phase28BugFixing.md",
    "Assets/Editor/Phase28BugFixingSetup.cs","Assets/Editor/Phase28BugFixingSetup.cs.meta"
)
Commit-IfStaged "Phase 28: Fix critical gameplay and flow bugs.`n`nResolve camera drift, continue-index, respawn, stomp, and level-complete juice issues without adding features."

Add-Paths @(
    "Docs/Phase29PerformanceOptimization.md",
    "Assets/Editor/Phase29PerformanceSetup.cs","Assets/Editor/Phase29PerformanceSetup.cs.meta"
)
Commit-IfStaged "Phase 29: Optimize performance hotspots safely.`n`nPool VFX bursts, reduce HUD GC, idle-disable timers, and clamp texture imports without changing gameplay."

Add-Paths @(
    "Docs/Phase30FullPlaytest.md",
    "Assets/Editor/Phase30FullPlaytestSetup.cs","Assets/Editor/Phase30FullPlaytestSetup.cs.meta",
    "Assets/Tests.meta","Assets/Tests/EditMode.meta","Assets/Tests/PlayMode.meta",
    "Assets/Scripts/Tests.meta","Assets/Scripts/Tests/.gitkeep"
)
Add-Glob "Assets/Tests/*"
Commit-IfStaged "Phase 30: Record full-campaign playtest coverage.`n`nCapture end-to-end checklist results for levels, systems, menus, and known issues without shipping new features."

Add-Paths @(
    "Docs/Phase31FinalPolish.md",
    "Assets/Editor/Phase31FinalPolishSetup.cs","Assets/Editor/Phase31FinalPolishSetup.cs.meta"
)
Commit-IfStaged "Phase 31: Apply final polish and campaign-complete UX.`n`nFix heart-drop edge cases, duplicate end triggers, campaign-complete copy, and remaining feedback polish."

Add-Paths @(
    "Docs/Phase32FinalBuild.md",
    "Assets/Editor/Phase32FinalBuildSetup.cs","Assets/Editor/Phase32FinalBuildSetup.cs.meta"
)
Commit-IfStaged "Phase 32: Prepare the final Windows shipping build.`n`nGate debug paths for Release, verify build settings, and ready the playable x64 package checklist."

Add-Paths @(
    "Docs/Phase33VisualUpgrade.md",
    "Assets/Editor/Phase33VisualUpgradeSetup.cs","Assets/Editor/Phase33VisualUpgradeSetup.cs.meta"
)
Commit-IfStaged "Phase 33: Upgrade visuals for platformer readability.`n`nWire richer sprites, tiled platforms, parallax clouds, and Pixel Perfect Camera without changing gameplay rules."

Add-Paths @(
    "Docs/Phase34MainMenuAndPlayerFeel.md",
    "Assets/Editor/Phase34MainMenuScaleSetup.cs","Assets/Editor/Phase34MainMenuScaleSetup.cs.meta",
    "Assets/Editor/Phase34PlayerFeelSetup.cs","Assets/Editor/Phase34PlayerFeelSetup.cs.meta"
)
Commit-IfStaged "Phase 34: Scale the main menu and retune player feel.`n`nEnlarge menu UI for readability and sharpen Mario-inspired walk/run/jump responsiveness."

Add-Paths @(
    "Docs/Phase35LevelPolish.md",
    "Assets/Editor/Phase35LevelPolishSetup.cs","Assets/Editor/Phase35LevelPolishSetup.cs.meta"
)
Wait-GitReady
git add -A | Out-Null
git reset HEAD -- "_rebuild_phase_commits.ps1" "_continue_phase_commits.ps1" 2>$null | Out-Null
Commit-IfStaged "Phase 35: Polish campaign levels and widen the gameplay view.`n`nRe-author fair hazard layouts, decor, and a larger orthographic viewport for clearer platforming."

Wait-GitReady
git add -A | Out-Null
git reset HEAD -- "_rebuild_phase_commits.ps1" "_continue_phase_commits.ps1" 2>$null | Out-Null
Commit-IfStaged "Polish Level Complete UI scaling and Pixel Perfect warnings.`n`nMake post-level screens readable and remove the red Game View resolution warning."

Wait-GitReady
git branch -f main HEAD
git checkout main
git push -u origin main --force
if ($LASTEXITCODE -ne 0) { throw "Force push failed" }

Write-Host ""
Write-Host "Done. Commit count: $((git rev-list --count HEAD))"
git log --oneline
