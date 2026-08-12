// Filename: Phase30FullPlaytestSetup.cs
// Folder: Assets/Editor/
// Purpose: Validates Phase 30 full-playtest coverage inventory (record-only phase).
// Menu: Bounder Trail/Phase 30/Validate Full Playtest Coverage
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase30FullPlaytestSetup.ValidateFullPlaytestCoverage

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Data;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase30FullPlaytestSetup
    {
        private static readonly string[] RequiredBuildScenes =
        {
            "Assets/Scenes/Bootstrap.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level_01_LumenMeadows.unity",
            "Assets/Scenes/Level_02_CascadeCliffs.unity",
            "Assets/Scenes/Level_03_SkybridgeSpire.unity"
        };

        private static readonly string[] RequiredDocs =
        {
            "Docs/Phase30FullPlaytest.md",
            "Docs/Phase23BossSystem.md"
        };

        [MenuItem("Bounder Trail/Phase 30/Validate Full Playtest Coverage")]
        public static void ValidateFullPlaytestCoverage()
        {
            var issues = 0;
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 30 — validating full playtest coverage inventory.");

            for (var i = 0; i < RequiredDocs.Length; i++)
            {
                if (!File.Exists(RequiredDocs[i]))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing doc: {RequiredDocs[i]}");
                    issues++;
                }
            }

            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/Data/Levels/LevelCatalog.asset");
            if (catalog == null || catalog.Count < 3)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] LevelCatalog missing or incomplete.");
                issues++;
            }
            else
            {
                for (var i = 0; i < catalog.Count; i++)
                {
                    if (catalog.GetLevel(i) == null)
                    {
                        Debug.LogError($"{GameLog.ProjectPrefix}[Setup] LevelCatalog slot {i} is null.");
                        issues++;
                    }
                }
            }

            var build = EditorBuildSettings.scenes;
            for (var i = 0; i < RequiredBuildScenes.Length; i++)
            {
                var path = RequiredBuildScenes[i];
                if (!File.Exists(path))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing scene: {path}");
                    issues++;
                    continue;
                }

                var enabled = false;
                for (var b = 0; b < build.Length; b++)
                {
                    if (build[b].path == path && build[b].enabled)
                    {
                        enabled = true;
                        break;
                    }
                }

                if (!enabled)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Scene not enabled in Build Settings: {path}");
                    issues++;
                }
            }

            // Campaign content markers (live Phase 27 layouts).
            issues += AssertSceneContains("Assets/Scenes/Level_01_LumenMeadows.unity",
                "Enemy_Crawlbug_A", "Enemy_Crawlbug_B", "Checkpoint_Early", "Checkpoint_Mid",
                "Hazard_Pit_Gap", "Hazard_Spikes_Late", "PowerUp_HeartDrop", "Exit_Goal");

            issues += AssertSceneContains("Assets/Scenes/Level_02_CascadeCliffs.unity",
                "Enemy_Crawlbug_A", "Enemy_Hopmite_A", "Enemy_Spikewatch_A",
                "Checkpoint_A", "Checkpoint_B", "Hazard_Fire_Gate", "Hazard_Pit_A",
                "PowerUp_SpeedBurst", "PowerUp_HeartDrop", "Exit_Goal");

            issues += AssertSceneContains("Assets/Scenes/Level_03_SkybridgeSpire.unity",
                "Enemy_Crawlbug_A", "Enemy_Dartling_A", "Enemy_Hopmite_A", "Enemy_Skimmer_A", "Enemy_Spitter_A",
                "Checkpoint_A", "Checkpoint_B", "Checkpoint_C",
                "Hazard_MovingSpike_Finale", "PowerUp_GlowShield", "PowerUp_SpeedBurst", "Exit_Goal");

            // Flow / save / boss-lock markers.
            issues += AssertContains("Assets/Scripts/Core/GameStateManager.cs", "Campaign finished");
            issues += AssertContains("Assets/Scripts/Save/GameProgress.cs", "RegisterLevelCompleted");
            issues += AssertContains("Assets/Scripts/UI/GameplayFlowController.cs", "Finish");
            issues += AssertContains("Docs/Phase23BossSystem.md", "Bosses are not required");
            issues += AssertContains("Docs/Phase30FullPlaytest.md", "Interactive Play Mode checklist");

            // Gameplay.unity must remain out of the campaign build.
            for (var b = 0; b < build.Length; b++)
            {
                if (build[b].enabled && build[b].path.EndsWith("Gameplay.unity"))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Gameplay.unity is enabled in Build Settings (sandbox should stay out of campaign).");
                    issues++;
                }
            }

            if (issues == 0)
            {
                Debug.Log(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 30 validation passed — " +
                    "catalog, build scenes, and campaign coverage markers present. " +
                    "Complete the Interactive Play Mode checklist in Docs/Phase30FullPlaytest.md.");
            }
            else
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 30 validation failed ({issues} issue(s)).");
            }
        }

        private static int AssertSceneContains(string scenePath, params string[] markers)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing scene for assert: {scenePath}");
                return 1;
            }

            var text = File.ReadAllText(scenePath);
            var issues = 0;
            for (var i = 0; i < markers.Length; i++)
            {
                if (!text.Contains(markers[i]))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Scene {scenePath} missing marker '{markers[i]}'");
                    issues++;
                }
            }

            return issues;
        }

        private static int AssertContains(string path, string marker)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing file for assert: {path}");
                return 1;
            }

            if (File.ReadAllText(path).Contains(marker))
            {
                return 0;
            }

            Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Expected marker '{marker}' in {path}");
            return 1;
        }
    }
}
#endif
