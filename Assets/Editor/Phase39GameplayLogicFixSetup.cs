// Filename: Phase39GameplayLogicFixSetup.cs
// Folder: Assets/Editor/
// Purpose: Rebuild level layouts + disable Pixel Perfect warning (Phase 39).
// Menu: Bounder Trail/Phase 39/Setup Gameplay Logic Fix
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase39GameplayLogicFixSetup.SetupGameplayLogicFix

#if UNITY_EDITOR
using BounderTrail.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase39GameplayLogicFixSetup
    {
        private static readonly string[] CampaignScenes =
        {
            $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity",
            "Assets/Scenes/Gameplay.unity"
        };

        [MenuItem("Bounder Trail/Phase 39/Setup Gameplay Logic Fix")]
        public static void SetupGameplayLogicFix()
        {
            // Rebake authored layouts so platforms, coins, and checkpoints match.
            Phase24LevelDesignSetup.SetupLevelDesign();
            Phase33VisualUpgradeSetup.SetupVisualUpgrade();
            Phase6CameraSetup.SetupCameraSystem();
            Phase36HeartsHudSetup.SetupHeartsHud();
            Phase37OverlayUiScaleSetup.SetupOverlayUiScale();

            for (var i = 0; i < CampaignScenes.Length; i++)
            {
                if (System.IO.File.Exists(CampaignScenes[i]))
                {
                    DisablePixelPerfectInScene(CampaignScenes[i]);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"{GameLog.ProjectPrefix}[Setup] Phase 39 ready — level layouts aligned, " +
                "Pixel Perfect warning disabled, pickups stay on platforms.");
        }

        private static void DisablePixelPerfectInScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var changed = false;
            for (var i = 0; i < cameras.Length; i++)
            {
                var camera = cameras[i];
                if (camera == null)
                {
                    continue;
                }

                camera.orthographic = true;
                camera.orthographicSize = ProjectConstants.GameplayOrthographicSize;

                var behaviours = camera.GetComponents<Behaviour>();
                for (var j = 0; j < behaviours.Length; j++)
                {
                    if (behaviours[j] == null || behaviours[j].GetType().Name != "PixelPerfectCamera")
                    {
                        continue;
                    }

                    behaviours[j].enabled = false;
                    EditorUtility.SetDirty(behaviours[j]);
                    changed = true;
                }

                EditorUtility.SetDirty(camera);
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, scenePath);
            }
            else
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, scenePath);
            }
        }
    }
}
#endif
