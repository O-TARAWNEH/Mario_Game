// Filename: Phase35LevelPolishSetup.cs
// Folder: Assets/Editor/
// Purpose: Campaign level polish — layouts, hazard logic, viewport, decor (Phase 35).
// Menu: Bounder Trail/Phase 35/Setup Level Polish
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase35LevelPolishSetup.SetupLevelPolish

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase35LevelPolishSetup
    {
        private static readonly string[] CampaignScenes =
        {
            $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity"
        };

        [MenuItem("Bounder Trail/Phase 35/Setup Level Polish")]
        public static void SetupLevelPolish()
        {
            Phase24LevelDesignSetup.SetupLevelDesign();
            Phase33VisualUpgradeSetup.SetupVisualUpgrade();
            Phase6CameraSetup.SetupCameraSystem();
            Phase36HeartsHudSetup.SetupHeartsHud();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var issues = ValidateInternal(logPass: false);
            if (issues == 0)
            {
                Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 35 level polish complete.");
            }
            else
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 35 finished with {issues} validation issue(s).");
            }
        }

        [MenuItem("Bounder Trail/Phase 35/Validate Level Polish")]
        public static void ValidateLevelPolish()
        {
            ValidateInternal(logPass: true);
        }

        private static int ValidateInternal(bool logPass)
        {
            var issues = 0;
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 35 — validating level polish.");

            if (!File.Exists("Docs/Phase35LevelPolish.md"))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing Docs/Phase35LevelPolish.md");
                issues++;
            }

            issues += AssertContains("Assets/Scripts/Core/ProjectConstants.cs", "GameplayRefResolutionY");
            issues += AssertContains("Assets/Editor/Phase24LevelDesignSetup.cs", "PlacePitBetween");
            issues += AssertContains("Assets/Editor/Phase24LevelDesignSetup.cs", "PlaceSpikesOnEdge");
            issues += AssertContains("Assets/Editor/Phase24LevelDesignSetup.cs", "PlaceDecorCluster");
            issues += AssertContains("Assets/Editor/Phase24LevelDesignSetup.cs", "ApplyTiledPlatformWorldSize");
            issues += AssertContains("Assets/Editor/Phase24LevelDesignSetup.cs", "SanitizeTilemapPhysics");

            for (var i = 0; i < CampaignScenes.Length; i++)
            {
                if (!File.Exists(CampaignScenes[i]))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing scene: {CampaignScenes[i]}");
                    issues++;
                    continue;
                }

                var sceneText = File.ReadAllText(CampaignScenes[i]);
                if (sceneText.Contains("TilemapCollider2D"))
                {
                    Debug.LogError(
                        $"{GameLog.ProjectPrefix}[Setup] {CampaignScenes[i]} has tilemap colliders — run Setup to remove invisible ground.");
                    issues++;
                }

                if (sceneText.Contains("Step_A") || sceneText.Contains("Landing_Mover"))
                {
                    Debug.LogWarning(
                        $"{GameLog.ProjectPrefix}[Setup] {CampaignScenes[i]} may use legacy layout — run Setup Level Polish.");
                }

                var expectedOrtho = ProjectConstants.GameplayOrthographicSize;
                var orthoToken = $"orthographic size: {expectedOrtho}";
                if (!sceneText.Contains(orthoToken))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] {CampaignScenes[i]} camera not at {expectedOrtho} — run Setup.");
                    issues++;
                }
            }

            if (logPass && issues == 0)
            {
                Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 35 validation passed.");
            }

            return issues;
        }

        private static int AssertContains(string path, string needle)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing file: {path}");
                return 1;
            }

            if (!File.ReadAllText(path).Contains(needle))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Expected '{needle}' in {path}");
                return 1;
            }

            return 0;
        }
    }
}
#endif
