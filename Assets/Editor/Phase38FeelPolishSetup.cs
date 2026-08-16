// Filename: Phase38FeelPolishSetup.cs
// Folder: Assets/Editor/
// Purpose: Wires hitstop, squash/stretch, fades, button punch (Phase 38).
// Menu: Bounder Trail/Phase 38/Setup Feel Polish
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase38FeelPolishSetup.SetupFeelPolish

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Player;
using BounderTrail.UI;
using BounderTrail.Vfx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase38FeelPolishSetup
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        private static readonly string[] GameplayScenes =
        {
            "Assets/Scenes/Gameplay.unity",
            $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity"
        };

        [MenuItem("Bounder Trail/Phase 38/Setup Feel Polish")]
        public static void SetupFeelPolish()
        {
            WirePlayerSquash();
            WireBootstrapSystems();
            WireButtonsInScene(MainMenuScenePath);
            for (var i = 0; i < GameplayScenes.Length; i++)
            {
                if (System.IO.File.Exists(GameplayScenes[i]))
                {
                    WireButtonsInScene(GameplayScenes[i]);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"{GameLog.ProjectPrefix}[Setup] Phase 38 feel polish ready — " +
                "hitstop, squash/stretch, screen fade, UI button punch.");
        }

        private static void WirePlayerSquash()
        {
            if (!System.IO.File.Exists(PlayerPrefabPath))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing {PlayerPrefabPath}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var squash = root.GetComponent<PlayerSquashStretch>();
                if (squash == null)
                {
                    squash = root.AddComponent<PlayerSquashStretch>();
                }

                var so = new SerializedObject(squash);
                so.FindProperty("playerController").objectReferenceValue = root.GetComponent<PlayerController>();
                so.FindProperty("visualRoot").objectReferenceValue = root.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void WireBootstrapSystems()
        {
            if (!System.IO.File.Exists(BootstrapScenePath))
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Missing Bootstrap scene.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] GameBootstrap missing.");
                return;
            }

            if (bootstrap.GetComponent<HitStop>() == null)
            {
                bootstrap.gameObject.AddComponent<HitStop>();
            }

            if (bootstrap.GetComponent<ScreenFade>() == null)
            {
                bootstrap.gameObject.AddComponent<ScreenFade>();
            }

            EditorUtility.SetDirty(bootstrap.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void WireButtonsInScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null || string.IsNullOrEmpty(button.gameObject.scene.name))
                {
                    continue;
                }

                if (button.GetComponent<UiButtonPunch>() == null)
                {
                    button.gameObject.AddComponent<UiButtonPunch>();
                    EditorUtility.SetDirty(button.gameObject);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }
    }
}
#endif
