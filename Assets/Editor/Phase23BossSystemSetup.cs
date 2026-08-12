// Filename: Phase23BossSystemSetup.cs
// Folder: Assets/Editor/
// Purpose: Confirms bosses are not design-required; asserts no Boss* assets were added (Phase 23).
// Menu: Bounder Trail/Phase 23/Validate Boss System Decision
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase23BossSystemSetup.ValidateBossSystemDecision

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Data;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase23BossSystemSetup
    {
        private static readonly string[] ForbiddenBossPrefabFolders =
        {
            "Assets/Prefabs/Bosses",
            "Assets/Prefabs/Enemies/Bosses"
        };

        private static readonly string[] ForbiddenBossPrefabNamePrefixes =
        {
            "Boss_",
            "Enemy_Boss"
        };

        [MenuItem("Bounder Trail/Phase 23/Validate Boss System Decision")]
        public static void ValidateBossSystemDecision()
        {
            var issues = 0;

            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 23 — bosses are NOT required by the Game Design Specification.");
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Verdict: do not create boss architecture, health, states, attacks, phases, arena, or boss completion.");
            LogList("Design-approved enemy systems (no bosses)", ApprovedGameplayCatalog.EnemySystems);
            LogRejectedBossEntries();

            issues += AssertNoForbiddenScripts();
            issues += AssertNoForbiddenPrefabFolders();
            issues += AssertNoForbiddenPrefabsByName();

            if (issues == 0)
            {
                Debug.Log(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 23 complete: boss system correctly omitted. " +
                    "Level content should use only Phase 22 approved systems.");
            }
            else
            {
                Debug.LogError(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 23 found {issues} unauthorized boss asset(s). " +
                    "Remove them or update the Game Design Specification before approving bosses.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void LogRejectedBossEntries()
        {
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Rejected boss-related systems:");
            for (var i = 0; i < ApprovedGameplayCatalog.RejectedSystems.Length; i++)
            {
                var entry = ApprovedGameplayCatalog.RejectedSystems[i];
                if (entry.IndexOf("Boss", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Debug.Log($"  - {entry}");
                }
            }
        }

        private static int AssertNoForbiddenScripts()
        {
            var issues = 0;
            var guids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Scripts" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName.IndexOf("Boss", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Unauthorized boss script: {path}");
                issues++;
            }

            return issues;
        }

        private static int AssertNoForbiddenPrefabFolders()
        {
            var issues = 0;
            for (var i = 0; i < ForbiddenBossPrefabFolders.Length; i++)
            {
                var folder = ForbiddenBossPrefabFolders[i];
                if (AssetDatabase.IsValidFolder(folder))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Unauthorized boss prefab folder: {folder}");
                    issues++;
                }
            }

            return issues;
        }

        private static int AssertNoForbiddenPrefabsByName()
        {
            var issues = 0;
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var fileName = Path.GetFileNameWithoutExtension(path);
                for (var p = 0; p < ForbiddenBossPrefabNamePrefixes.Length; p++)
                {
                    if (!fileName.StartsWith(ForbiddenBossPrefabNamePrefixes[p], System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Unauthorized boss prefab: {path}");
                    issues++;
                    break;
                }
            }

            return issues;
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
