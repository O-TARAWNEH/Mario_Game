// Filename: Phase16LevelCompletionSetup.cs
// Folder: Assets/Editor/
// Purpose: Wires LevelCompletionService, Next Level UI, and goal samples (Phase 16).
// Menu: Bounder Trail/Phase 16/Setup Level Completion
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase16LevelCompletionSetup.SetupLevelCompletion

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Levels;
using BounderTrail.UI;
using BounderTrail.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase16LevelCompletionSetup
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        private const string ExitPrefabPath = "Assets/Prefabs/World/LevelExitDoor.prefab";

        [MenuItem("Bounder Trail/Phase 16/Setup Level Completion")]
        public static void SetupLevelCompletion()
        {
            EnsureTag("Goal");
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            WireCompletionService();
            EnsureGoalObject();
            WireLevelCompleteUi();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 16 level completion ready.");
        }

        private static void WireCompletionService()
        {
            var levelRoot = GameObject.Find("LevelRoot");
            var host = levelRoot != null ? levelRoot : GameObject.Find("RespawnSystem");
            if (host == null)
            {
                host = new GameObject("LevelCompletionService");
            }

            var service = host.GetComponent<LevelCompletionService>();
            if (service == null)
            {
                service = host.AddComponent<LevelCompletionService>();
            }

            var so = new SerializedObject(service);
            so.FindProperty("completionDelay").floatValue = 0.55f;
            so.FindProperty("freezePlayerOnComplete").boolValue = true;
            so.FindProperty("requirePlayerAlive").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureGoalObject()
        {
            if (GameObject.Find("Sample_ExitDoor") != null || Object.FindAnyObjectByType<LevelExitDoor>() != null)
            {
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExitPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] LevelExitDoor prefab missing.");
                return;
            }

            var parent = GameObject.Find("LevelRoot");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Sample_ExitDoor";
            if (parent != null)
            {
                instance.transform.SetParent(parent.transform, true);
            }

            instance.transform.position = new Vector3(9.2f, 3.4f, 0f);
            instance.tag = "Goal";
        }

        private static void WireLevelCompleteUi()
        {
            var controller = Object.FindAnyObjectByType<GameplayFlowController>();
            var panel = FindIncludingInactive("LevelCompletePanel");
            if (controller == null || panel == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Level complete UI not found. controller={controller != null} panel={panel != null}");
                return;
            }

            var nextButtonTransform = panel.transform.Find("LevelCompleteNextButton");
            Button nextButton;
            if (nextButtonTransform == null)
            {
                nextButton = CreateButton(panel.transform, "LevelCompleteNextButton", "Next Level", new Vector2(0f, -20f));
            }
            else
            {
                nextButton = nextButtonTransform.GetComponent<Button>();
            }

            // Reposition existing buttons for room.
            var restart = panel.transform.Find("LevelCompleteRestartButton")?.GetComponent<RectTransform>();
            var menu = panel.transform.Find("LevelCompleteMainMenuButton")?.GetComponent<RectTransform>();
            if (restart != null)
            {
                restart.anchoredPosition = new Vector2(0f, -90f);
            }

            if (menu != null)
            {
                menu.anchoredPosition = new Vector2(0f, -160f);
            }

            var nextRect = nextButton.GetComponent<RectTransform>();
            nextRect.anchoredPosition = new Vector2(0f, -20f);

            var so = new SerializedObject(controller);
            so.FindProperty("levelCompleteNextButton").objectReferenceValue = nextButton;
            var label = nextButton.GetComponentInChildren<Text>();
            so.FindProperty("levelCompleteNextButtonLabel").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static GameObject FindIncludingInactive(string name)
        {
            var transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (var i = 0; i < transforms.Length; i++)
            {
                var t = transforms[i];
                if (t == null || t.name != name)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(t.gameObject.scene.name))
                {
                    return t.gameObject;
                }
            }

            return null;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.2f, 0.55f, 0.35f, 1f);

            var button = buttonObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.7f, 0.45f, 1f);
            colors.pressedColor = new Color(0.15f, 0.4f, 0.25f, 1f);
            button.colors = colors;

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 54f);
            rect.anchoredPosition = anchoredPosition;

            var textObject = new GameObject("Label");
            textObject.transform.SetParent(buttonObject.transform, false);
            var text = textObject.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private static void EnsureTag(string tag)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(assets[0]);
            var tags = so.FindProperty("tags");
            for (var i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
