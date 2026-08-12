// Filename: Phase31FinalPolishSetup.cs
// Folder: Assets/Editor/
// Purpose: Applies/validates Phase 31 final polish (no major features).
// Menu: Bounder Trail/Phase 31/Apply Final Polish
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase31FinalPolishSetup.ApplyFinalPolish

#if UNITY_EDITOR
using BounderTrail.Audio;
using BounderTrail.Core;
using BounderTrail.Levels;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase31FinalPolishSetup
    {
        private static readonly string[] CampaignScenes =
        {
            "Assets/Scenes/Level_01_LumenMeadows.unity",
            "Assets/Scenes/Level_02_CascadeCliffs.unity",
            "Assets/Scenes/Level_03_SkybridgeSpire.unity"
        };

        [MenuItem("Bounder Trail/Phase 31/Apply Final Polish")]
        public static void ApplyFinalPolish()
        {
            WireBootstrapSfx();
            DisableDuplicateLevelEndTriggers();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var issues = ValidateInternal(logPass: false);
            if (issues == 0)
            {
                Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 31 final polish applied and validated.");
            }
            else
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 31 applied with {issues} validation issue(s).");
            }
        }

        [MenuItem("Bounder Trail/Phase 31/Validate Final Polish")]
        public static void ValidateFinalPolish()
        {
            ValidateInternal(logPass: true);
        }

        private static int ValidateInternal(bool logPass)
        {
            var issues = 0;
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 31 — validating final polish.");

            if (!File.Exists("Docs/Phase31FinalPolish.md"))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing Docs/Phase31FinalPolish.md");
                issues++;
            }

            issues += AssertContains("Assets/Scripts/Player/PlayerHealth.cs", "TryHeal");
            issues += AssertContains("Assets/Scripts/Player/PlayerPowerUps.cs", "ActivateHeartDrop()");
            issues += AssertContains("Assets/Scripts/Items/PowerUpPickup.cs", "Activate first");
            issues += AssertContains("Assets/Scripts/Audio/SfxId.cs", "PowerUpHeart");
            issues += AssertContains("Assets/Scripts/Player/PlayerAudioFeedback.cs", "PowerUpHeart");
            issues += AssertContains("Assets/Scripts/UI/GameplayFlowController.cs", "CAMPAIGN COMPLETE");
            issues += AssertContains("Assets/Scripts/UI/FlowScreenSummary.cs", "campaignCompleteFormat");
            issues += AssertContains("Assets/Scripts/Levels/Checkpoint.cs", "PlayFirstReachFeedback");
            issues += AssertContains("Assets/Scripts/World/BouncePad.cs", "PlaySquash");
            issues += AssertContains("Assets/Editor/Phase24LevelDesignSetup.cs", "completeLevelOnEnter");

            for (var i = 0; i < CampaignScenes.Length; i++)
            {
                issues += AssertContains(CampaignScenes[i], "completeLevelOnEnter: 0");
            }

            issues += AssertContains("Assets/Scenes/Bootstrap.unity", "4e7b33c03e9478d4b879c70e59be6d41"); // Heart SFX
            issues += AssertContains("Assets/Scenes/Bootstrap.unity", "c08f54caf4a064946824b1b89e5c56f1"); // Coin SFX on Collect

            if (issues == 0 && logPass)
            {
                Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 31 validation passed.");
            }
            else if (issues > 0)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 31 validation failed ({issues} issue(s)).");
            }

            return issues;
        }

        private static void WireBootstrapSfx()
        {
            const string bootstrapPath = "Assets/Scenes/Bootstrap.unity";
            var scene = EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Single);
            var sfx = Object.FindFirstObjectByType<SfxSystem>();
            if (sfx == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] No SfxSystem in Bootstrap.");
                return;
            }

            Assign(sfx, SfxId.Collect, "Assets/Audio/SFX/SFX_Coin.wav", 0.85f);
            Assign(sfx, SfxId.PowerUpHeart, "Assets/Audio/SFX/SFX_PowerUp_Heart.wav", 0.9f);
            Assign(sfx, SfxId.PowerUpShield, "Assets/Audio/SFX/SFX_PowerUp_Shield.wav", 0.9f);
            Assign(sfx, SfxId.PowerUpSpeed, "Assets/Audio/SFX/SFX_PowerUp_Speed.wav", 0.9f);
            EditorUtility.SetDirty(sfx);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, bootstrapPath);
        }

        private static void Assign(SfxSystem sfx, SfxId id, string clipPath, float volume)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            if (clip == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Missing clip {clipPath}");
                return;
            }

            sfx.AssignClip(id, clip, volume);
        }

        private static void DisableDuplicateLevelEndTriggers()
        {
            for (var i = 0; i < CampaignScenes.Length; i++)
            {
                var path = CampaignScenes[i];
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var ends = Object.FindObjectsByType<LevelEndPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (var e = 0; e < ends.Length; e++)
                {
                    var end = ends[e];
                    if (end == null || end.name != "LevelEnd")
                    {
                        continue;
                    }

                    var so = new SerializedObject(end);
                    so.FindProperty("completeLevelOnEnter").boolValue = false;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    var col = end.GetComponent<Collider2D>();
                    if (col != null)
                    {
                        col.enabled = false;
                        EditorUtility.SetDirty(col);
                    }

                    EditorUtility.SetDirty(end);
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, path);
            }
        }

        private static int AssertContains(string path, string marker)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing file: {path}");
                return 1;
            }

            if (File.ReadAllText(path).Contains(marker))
            {
                return 0;
            }

            Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Expected '{marker}' in {path}");
            return 1;
        }
    }
}
#endif
