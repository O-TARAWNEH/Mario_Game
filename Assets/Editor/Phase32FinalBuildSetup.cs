// Filename: Phase32FinalBuildSetup.cs
// Folder: Assets/Editor/
// Purpose: Prepares, validates, and builds the shipping Windows player (Phase 32).
// Menu: Bounder Trail/Phase 32/...
// Batchmode:
//   -executeMethod BounderTrail.EditorTools.Phase32FinalBuildSetup.PrepareFinalBuild
//   -executeMethod BounderTrail.EditorTools.Phase32FinalBuildSetup.ValidateFinalBuild
//   -executeMethod BounderTrail.EditorTools.Phase32FinalBuildSetup.BuildWindowsPlayer

#if UNITY_EDITOR
using BounderTrail.Core;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase32FinalBuildSetup
    {
        public const string WindowsBuildFolder = "Builds/Windows";
        public const string WindowsExeName = "BounderTrail.exe";
        public static string WindowsBuildPath => Path.Combine(WindowsBuildFolder, WindowsExeName);

        private static readonly string[] RequiredBuildScenes =
        {
            "Assets/Scenes/Bootstrap.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level_01_LumenMeadows.unity",
            "Assets/Scenes/Level_02_CascadeCliffs.unity",
            "Assets/Scenes/Level_03_SkybridgeSpire.unity",
            "Assets/Scenes/Level_04_EchoCaverns.unity",
            "Assets/Scenes/Level_05_LanternLockworks.unity"
        };

        [MenuItem("Bounder Trail/Phase 32/Prepare Final Build")]
        public static void PrepareFinalBuild()
        {
            DisableBootstrapVerboseLogs();
            EnsureBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var issues = ValidateInternal(logPass: false);
            if (issues == 0)
            {
                Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 32 prepare complete — ready to build.");
            }
            else
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 32 prepare finished with {issues} issue(s).");
            }
        }

        [MenuItem("Bounder Trail/Phase 32/Validate Final Build")]
        public static void ValidateFinalBuild()
        {
            ValidateInternal(logPass: true);
        }

        [MenuItem("Bounder Trail/Phase 32/Build Windows Player")]
        public static void BuildWindowsPlayer()
        {
            PrepareFinalBuild();

            Directory.CreateDirectory(WindowsBuildFolder);
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length < RequiredBuildScenes.Length)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Build aborted — Build Settings scenes incomplete.");
                EditorApplication.Exit(1);
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = WindowsBuildPath.Replace('\\', '/'),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CompressWithLz4HC
            };

            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Building Windows x64 Release → {WindowsBuildPath}");
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 32 Windows build SUCCEEDED " +
                    $"({summary.totalSize} bytes, {summary.totalTime}). Exe: {Path.GetFullPath(WindowsBuildPath)}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            else
            {
                Debug.LogError(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 32 Windows build FAILED: {summary.result} " +
                    $"({summary.totalErrors} errors). See Editor.log.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static void DisableBootstrapVerboseLogs()
        {
            const string path = "Assets/Scenes/Bootstrap.unity";
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            if (bootstrap != null)
            {
                var so = new SerializedObject(bootstrap);
                var log = so.FindProperty("logBootstrapEvents");
                if (log != null)
                {
                    log.boolValue = false;
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bootstrap);
            }

            SetBoolOnComponentInScene("logLoads", false);
            SetBoolOnComponentInScene("logCollections", false);
            SetBoolOnComponentInScene("logSaveEvents", false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void SetBoolOnComponentInScene(string propertyName, bool value)
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (var r = 0; r < roots.Length; r++)
            {
                var behaviours = roots[r].GetComponentsInChildren<MonoBehaviour>(true);
                for (var b = 0; b < behaviours.Length; b++)
                {
                    if (behaviours[b] == null)
                    {
                        continue;
                    }

                    var so = new SerializedObject(behaviours[b]);
                    var prop = so.FindProperty(propertyName);
                    if (prop == null || prop.propertyType != SerializedPropertyType.Boolean)
                    {
                        continue;
                    }

                    prop.boolValue = value;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(behaviours[b]);
                }
            }
        }

        private static void EnsureBuildScenes()
        {
            var scenes = new EditorBuildSettingsScene[RequiredBuildScenes.Length];
            for (var i = 0; i < RequiredBuildScenes.Length; i++)
            {
                scenes[i] = new EditorBuildSettingsScene(RequiredBuildScenes[i], true);
            }

            EditorBuildSettings.scenes = scenes;
        }

        private static int ValidateInternal(bool logPass)
        {
            var issues = 0;
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 32 — validating final build readiness.");

            if (!File.Exists("Docs/Phase32FinalBuild.md"))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing Docs/Phase32FinalBuild.md");
                issues++;
            }

            issues += AssertContains("Assets/Scripts/Core/GameLog.cs", "ConfigureForPlayer");
            issues += AssertContains("Assets/Scripts/UI/GameplayFlowController.cs", "DEVELOPMENT_BUILD");
            issues += AssertContains("Assets/Scripts/Player/PlayerDeath.cs", "DEVELOPMENT_BUILD");
            issues += AssertContains("Assets/Scenes/Bootstrap.unity", "logBootstrapEvents: 0");
            issues += AssertContains("Assets/Scenes/Bootstrap.unity", "logLoads: 0");
            issues += AssertContains("Assets/Scenes/Bootstrap.unity", "logCollections: 0");
            issues += AssertContains("Assets/Scenes/Bootstrap.unity", "logSaveEvents: 0");
            issues += AssertContains("Assets/Prefabs/Player/Player_Pip.prefab", "enableDebugKillKey: 0");

            // Removed orphan shipping clutter.
            if (File.Exists("Assets/Prefabs/World/Ground_Basic.prefab"))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Legacy Ground_Basic.prefab should be removed.");
                issues++;
            }

            if (File.Exists("Assets/Audio/SFX/SFX_Collect.wav"))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Orphan SFX_Collect.wav should be removed (Collect uses SFX_Coin).");
                issues++;
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

                var enabled = build.Any(s => s.path == path && s.enabled);
                if (!enabled)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Scene not enabled in Build Settings: {path}");
                    issues++;
                }
            }

            for (var b = 0; b < build.Length; b++)
            {
                if (build[b].enabled && build[b].path.EndsWith("Gameplay.unity"))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Gameplay.unity must stay OUT of the shipping build.");
                    issues++;
                }
            }

            // Player settings sanity.
            if (PlayerSettings.companyName != ProjectConstants.CompanyName)
            {
                Debug.LogWarning(
                    $"{GameLog.ProjectPrefix}[Setup] companyName is '{PlayerSettings.companyName}' " +
                    $"(expected {ProjectConstants.CompanyName}).");
            }

            if (PlayerSettings.productName != ProjectConstants.GameTitle)
            {
                Debug.LogWarning(
                    $"{GameLog.ProjectPrefix}[Setup] productName is '{PlayerSettings.productName}' " +
                    $"(expected {ProjectConstants.GameTitle}).");
            }

            if (issues == 0 && logPass)
            {
                Debug.Log(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 32 validation passed — " +
                    "shipping logs off, debug gated, build scenes OK. Run Build Windows Player next.");
            }
            else if (issues > 0)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 32 validation failed ({issues} issue(s)).");
            }

            return issues;
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
