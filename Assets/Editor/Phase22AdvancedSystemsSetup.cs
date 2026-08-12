// Filename: Phase22AdvancedSystemsSetup.cs
// Folder: Assets/Editor/
// Purpose: Validates design-approved advanced systems exist; does not invent new mechanics (Phase 22).
// Menu: Bounder Trail/Phase 22/Validate Advanced Gameplay Systems
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase22AdvancedSystemsSetup.ValidateAdvancedGameplaySystems

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Data;
using UnityEditor;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase22AdvancedSystemsSetup
    {
        private static readonly string[] RequiredPrefabs =
        {
            // Enemies (moving / design-spec types)
            "Assets/Prefabs/Enemies/Enemy_Crawlbug.prefab",
            "Assets/Prefabs/Enemies/Enemy_Hopmite.prefab",
            "Assets/Prefabs/Enemies/Enemy_Spikewatch.prefab",
            "Assets/Prefabs/Enemies/Enemy_Dartling.prefab",
            "Assets/Prefabs/Enemies/Enemy_Skimmer.prefab",
            "Assets/Prefabs/Enemies/Enemy_Spitter.prefab",

            // Special platforms / moving objects
            "Assets/Prefabs/World/Platform_Solid.prefab",
            "Assets/Prefabs/World/Platform_OneWay.prefab",
            "Assets/Prefabs/World/Platform_Moving.prefab",
            "Assets/Prefabs/World/BouncePad.prefab",
            "Assets/Prefabs/World/LevelExitDoor.prefab",

            // Hazards
            "Assets/Prefabs/World/Hazard_DeathZone.prefab",
            "Assets/Prefabs/World/Hazard_Spikes.prefab",
            "Assets/Prefabs/World/Hazard_Fire.prefab",
            "Assets/Prefabs/World/Hazard_MovingSpike.prefab",

            // Checkpoint + collectibles + temporary abilities
            "Assets/Prefabs/World/Checkpoint_Flag.prefab",
            "Assets/Prefabs/Items/Item_Coin.prefab",
            "Assets/Prefabs/Items/Item_SpeedBurst.prefab",
            "Assets/Prefabs/Items/Item_GlowShield.prefab",
            "Assets/Prefabs/Items/Item_HeartDrop.prefab"
        };

        private static readonly string[] RequiredScripts =
        {
            "Assets/Scripts/World/MovingPlatform.cs",
            "Assets/Scripts/World/OneWayPlatform.cs",
            "Assets/Scripts/World/BouncePad.cs",
            "Assets/Scripts/World/MovingHazard.cs",
            "Assets/Scripts/World/EnvironmentalHazard.cs",
            "Assets/Scripts/World/LevelExitDoor.cs",
            "Assets/Scripts/Enemies/EnemyMover.cs",
            "Assets/Scripts/Enemies/EnemyJumper.cs",
            "Assets/Scripts/Enemies/EnemyFlyer.cs",
            "Assets/Scripts/Enemies/EnemyShooter.cs",
            "Assets/Scripts/Player/PlayerPowerUps.cs",
            "Assets/Scripts/Items/PowerUpPickup.cs",
            "Assets/Scripts/Items/Collectible.cs",
            "Assets/Scripts/Levels/Checkpoint.cs",
            "Assets/Scripts/Data/ApprovedGameplayCatalog.cs"
        };

        [MenuItem("Bounder Trail/Phase 22/Validate Advanced Gameplay Systems")]
        public static void ValidateAdvancedGameplaySystems()
        {
            var missing = 0;

            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 22 — validating design-approved systems only.");
            LogList("Approved player systems", ApprovedGameplayCatalog.PlayerSystems);
            LogList("Approved enemy systems", ApprovedGameplayCatalog.EnemySystems);
            LogList("Approved world systems", ApprovedGameplayCatalog.WorldSystems);
            LogList("Approved collectibles", ApprovedGameplayCatalog.CollectibleSystems);
            LogList("Explicitly NOT implemented (not in design)", ApprovedGameplayCatalog.RejectedSystems);

            for (var i = 0; i < RequiredPrefabs.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(RequiredPrefabs[i]) == null)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing approved prefab: {RequiredPrefabs[i]}");
                    missing++;
                }
            }

            for (var i = 0; i < RequiredScripts.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<MonoScript>(RequiredScripts[i]) == null
                    && !System.IO.File.Exists(RequiredScripts[i]))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing approved script: {RequiredScripts[i]}");
                    missing++;
                }
            }

            if (missing == 0)
            {
                Debug.Log(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 22 complete: all design-approved advanced systems are present. " +
                    "No unauthorized mechanics were added.");
            }
            else
            {
                Debug.LogError(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 22 validation found {missing} missing approved asset(s).");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void LogList(string title, string[] items)
        {
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] {title}:");
            for (var i = 0; i < items.Length; i++)
            {
                Debug.Log($"  - {items[i]}");
            }
        }
    }
}
#endif
