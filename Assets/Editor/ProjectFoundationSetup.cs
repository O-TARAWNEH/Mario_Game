// Filename: ProjectFoundationSetup.cs
// Folder: Assets/Editor/
// Purpose: Phase 1 editor utility — Bootstrap scene, player settings, foundation packages.
// Dependencies: BounderTrail.Core.GameBootstrap, ProjectConstants, GameLog.
//
// Menu: Bounder Trail/Phase 1/Create Bootstrap Scene
// Menu: Bounder Trail/Phase 1/Apply Foundation Player Settings
// Menu: Bounder Trail/Phase 1/Run Full Foundation Setup
// Batchmode: -executeMethod BounderTrail.EditorTools.ProjectFoundationSetup.RunFullFoundationSetup

#if UNITY_EDITOR
using BounderTrail.Core;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BounderTrail.EditorTools
{
    public static class ProjectFoundationSetup
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";

        private static readonly string[] FoundationPackages =
        {
            "com.unity.ugui",
            "com.unity.feature.2d",
            "com.unity.render-pipelines.universal",
            "com.unity.test-framework",
            "com.unity.ide.visualstudio"
        };

        [MenuItem("Bounder Trail/Phase 1/Run Full Foundation Setup")]
        public static void RunFullFoundationSetup()
        {
            EnsureFoundationPackages();
            ApplyFoundationPlayerSettings();
            EnsureUrp2DPipeline();
            CreateBootstrapScene();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 1 full foundation setup finished (packages may still resolve).");
        }

        [MenuItem("Bounder Trail/Phase 1/Ensure URP 2D Pipeline")]
        public static void EnsureUrp2DPipeline()
        {
            const string configFolder = "Assets/Data/Configs";
            const string rendererPath = configFolder + "/URP_Renderer2D.asset";
            const string pipelinePath = configFolder + "/URP_Pipeline.asset";

            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                AssetDatabase.CreateFolder("Assets", "Data");
            }

            if (!AssetDatabase.IsValidFolder(configFolder))
            {
                AssetDatabase.CreateFolder("Assets/Data", "Configs");
            }

            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            AssetDatabase.SaveAssets();

            Debug.Log($"{GameLog.ProjectPrefix}[Setup] URP 2D pipeline assigned ({pipelinePath}).");
        }

        [MenuItem("Bounder Trail/Phase 1/Create Bootstrap Scene")]
        public static void CreateBootstrapScene()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var existing = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var bootstrapObject = new GameObject(ProjectConstants.BootstrapObjectName);
            bootstrapObject.AddComponent<GameBootstrap>();

            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
            AddSceneToBuildSettings(BootstrapScenePath);

            // Make Bootstrap the first enabled scene.
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length > 0 && scenes[0].path != BootstrapScenePath)
            {
                var bootstrapIndex = System.Array.FindIndex(scenes, s => s.path == BootstrapScenePath);
                if (bootstrapIndex > 0)
                {
                    var bootstrap = scenes[bootstrapIndex];
                    for (var i = bootstrapIndex; i > 0; i--)
                    {
                        scenes[i] = scenes[i - 1];
                    }

                    scenes[0] = bootstrap;
                    EditorBuildSettings.scenes = scenes;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Bootstrap scene created at {BootstrapScenePath}");
        }

        [MenuItem("Bounder Trail/Phase 1/Apply Foundation Player Settings")]
        public static void ApplyFoundationPlayerSettings()
        {
            PlayerSettings.companyName = ProjectConstants.CompanyName;
            PlayerSettings.productName = ProjectConstants.GameTitle;
            PlayerSettings.defaultScreenWidth = ProjectConstants.ReferenceWidth;
            PlayerSettings.defaultScreenHeight = ProjectConstants.ReferenceHeight;

            // activeInputHandler remains Input Manager (Old) via ProjectSettings (Phase 0).

            AssetDatabase.SaveAssets();

            Debug.Log(
                $"{GameLog.ProjectPrefix}[Setup] Player settings applied: " +
                $"{PlayerSettings.productName}, {ProjectConstants.ReferenceWidth}x{ProjectConstants.ReferenceHeight}.");
        }

        [MenuItem("Bounder Trail/Phase 1/Ensure Foundation Packages")]
        public static void EnsureFoundationPackages()
        {
            foreach (var packageId in FoundationPackages)
            {
                var request = Client.Add(packageId);
                WaitForRequest(request);
                if (request.Status == StatusCode.Success)
                {
                    Debug.Log($"{GameLog.ProjectPrefix}[Setup] Package ready: {packageId}");
                }
                else if (request.Status == StatusCode.Failure)
                {
                    Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Package add issue for {packageId}: {request.Error?.message}");
                }
            }

            AssetDatabase.Refresh();
        }

        private static void WaitForRequest(Request request)
        {
            // Editor batchmode-safe polling for package requests.
            while (!request.IsCompleted)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var entry in scenes)
            {
                if (entry.path == scenePath)
                {
                    return;
                }
            }

            var list = new EditorBuildSettingsScene[scenes.Length + 1];
            for (var i = 0; i < scenes.Length; i++)
            {
                list[i] = scenes[i];
            }

            list[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = list;
        }
    }
}
#endif
