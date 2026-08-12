// Filename: Phase28BugFixingSetup.cs
// Folder: Assets/Editor/
// Purpose: Validates Phase 28 bug-fix presence (no new features).
// Menu: Bounder Trail/Phase 28/Validate Bug Fixes
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase28BugFixingSetup.ValidateBugFixes

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Data;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase28BugFixingSetup
    {
        private static readonly string[] RequiredScripts =
        {
            "Assets/Scripts/Camera/CameraFollow2D.cs",
            "Assets/Scripts/Camera/CameraShake2D.cs",
            "Assets/Scripts/Levels/LevelLoader.cs",
            "Assets/Scripts/Levels/RespawnSystem.cs",
            "Assets/Scripts/Save/GameProgress.cs",
            "Assets/Scripts/Player/PlayerController.cs",
            "Assets/Scripts/Enemies/EnemyContact.cs",
            "Assets/Scripts/Vfx/SimpleBurstVfx.cs",
            "Assets/Scripts/Vfx/GameplayVisualJuice.cs",
            "Assets/Data/Levels/LevelCatalog.asset"
        };

        [MenuItem("Bounder Trail/Phase 28/Validate Bug Fixes")]
        public static void ValidateBugFixes()
        {
            var issues = 0;

            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 28 — validating bug-fix deliverables (no new features).");

            for (var i = 0; i < RequiredScripts.Length; i++)
            {
                var path = RequiredScripts[i];
                if (!File.Exists(path) && AssetDatabase.LoadMainAssetAtPath(path) == null)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing: {path}");
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

            // Smoke-check key fix markers in source.
            issues += AssertContains("Assets/Scripts/Camera/CameraFollow2D.cs", "unshakenCurrent");
            issues += AssertContains("Assets/Scripts/Save/GameProgress.cs", "_continueLevelIndex");
            issues += AssertContains("Assets/Scripts/Levels/LevelLoader.cs", "NotifyLoadFailed");
            issues += AssertContains("Assets/Scripts/Vfx/SimpleBurstVfx.cs", "ResetAliveCount");
            issues += AssertContains("Assets/Scripts/Levels/RespawnSystem.cs", "FaceRightOnRespawn");
            issues += AssertContains("Assets/Scripts/Enemies/EnemyContact.cs", "IsInvulnerable");

            if (issues == 0)
            {
                Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 28 validation passed — bug fixes present.");
            }
            else
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 28 validation found {issues} issue(s).");
            }
        }

        private static int AssertContains(string path, string token)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing file for marker check: {path}");
                return 1;
            }

            var text = File.ReadAllText(path);
            if (text.IndexOf(token, System.StringComparison.Ordinal) < 0)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Expected fix marker '{token}' in {path}");
                return 1;
            }

            return 0;
        }
    }
}
#endif
