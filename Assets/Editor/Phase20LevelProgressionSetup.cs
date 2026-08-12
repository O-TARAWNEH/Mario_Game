// Filename: Phase20LevelProgressionSetup.cs
// Folder: Assets/Editor/
// Purpose: Creates 3 campaign levels, catalog progression, and level select UI (Phase 20).
// Menu: Bounder Trail/Phase 20/Setup Level Progression
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase20LevelProgressionSetup.SetupLevelProgression

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Data;
using BounderTrail.Levels;
using BounderTrail.Save;
using BounderTrail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase20LevelProgressionSetup
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        private const string DataFolder = "Assets/Data/Levels";
        private const string CatalogPath = DataFolder + "/LevelCatalog.asset";
        private const string ScenesFolder = "Assets/Scenes";

        private static readonly LevelSpec[] Campaign =
        {
            new LevelSpec(
                "level_01",
                "Lumen Meadows",
                ProjectConstants.Level01SceneName,
                $"{ScenesFolder}/{ProjectConstants.Level01SceneName}.unity",
                $"{DataFolder}/LevelData_01_LumenMeadows.asset",
                0),
            new LevelSpec(
                "level_02",
                "Cascade Cliffs",
                ProjectConstants.Level02SceneName,
                $"{ScenesFolder}/{ProjectConstants.Level02SceneName}.unity",
                $"{DataFolder}/LevelData_02_CascadeCliffs.asset",
                1),
            new LevelSpec(
                "level_03",
                "Skybridge Spire",
                ProjectConstants.Level03SceneName,
                $"{ScenesFolder}/{ProjectConstants.Level03SceneName}.unity",
                $"{DataFolder}/LevelData_03_SkybridgeSpire.asset",
                2)
        };

        private static readonly Color ButtonColor = new Color(0.18f, 0.48f, 0.34f, 1f);
        private static readonly Color ButtonHighlight = new Color(0.28f, 0.62f, 0.44f, 1f);
        private static readonly Color PanelColor = new Color(0.05f, 0.08f, 0.1f, 0.88f);

        private readonly struct LevelSpec
        {
            public readonly string LevelId;
            public readonly string DisplayName;
            public readonly string SceneName;
            public readonly string ScenePath;
            public readonly string DataPath;
            public readonly int BuildIndex;

            public LevelSpec(string levelId, string displayName, string sceneName, string scenePath, string dataPath, int buildIndex)
            {
                LevelId = levelId;
                DisplayName = displayName;
                SceneName = sceneName;
                ScenePath = scenePath;
                DataPath = dataPath;
                BuildIndex = buildIndex;
            }
        }

        [MenuItem("Bounder Trail/Phase 20/Setup Level Progression")]
        public static void SetupLevelProgression()
        {
            EnsureFolder("Assets/Data", "Levels");
            EnsureBootstrapProgress();

            var levelAssets = new LevelData[Campaign.Length];
            for (var i = 0; i < Campaign.Length; i++)
            {
                EnsureCampaignScene(Campaign[i]);
                levelAssets[i] = CreateLevelData(Campaign[i]);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(Campaign[i].DataPath);
                levelAssets[i] = AssetDatabase.LoadAssetAtPath<LevelData>(Campaign[i].DataPath);
            }

            AssetDatabase.SaveAssets();
            var catalog = UpdateCatalog(levelAssets);
            WireBootstrapCatalog(catalog);
            ConfigureBuildSettings();
            WireMainMenuLevelSelect();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 20 level progression ready ({Campaign.Length} campaign levels).");
        }

        private static void EnsureBootstrapProgress()
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrap == null)
            {
                return;
            }

            if (bootstrap.GetComponent<GameProgress>() == null)
            {
                bootstrap.AddComponent<GameProgress>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void EnsureCampaignScene(LevelSpec spec)
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(spec.ScenePath))
            {
                if (!AssetDatabase.CopyAsset(GameplayScenePath, spec.ScenePath))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Failed to copy Gameplay to {spec.ScenePath}");
                    return;
                }

                AssetDatabase.ImportAsset(spec.ScenePath);
            }

            var scene = EditorSceneManager.OpenScene(spec.ScenePath, OpenSceneMode.Single);
            var levelRoot = Object.FindAnyObjectByType<LevelRoot>();
            if (levelRoot != null)
            {
                var so = new SerializedObject(levelRoot);
                so.FindProperty("levelId").stringValue = spec.LevelId;
                so.FindProperty("displayName").stringValue = spec.DisplayName;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(levelRoot);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, spec.ScenePath);
        }

        private static LevelData CreateLevelData(LevelSpec spec)
        {
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(spec.DataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(data, spec.DataPath);
            }

            var so = new SerializedObject(data);
            so.FindProperty("levelId").stringValue = spec.LevelId;
            so.FindProperty("displayName").stringValue = spec.DisplayName;
            so.FindProperty("sceneName").stringValue = spec.SceneName;
            so.FindProperty("buildIndex").intValue = spec.BuildIndex;
            so.FindProperty("designerNotes").stringValue =
                $"Campaign level {spec.BuildIndex + 1}. Content tuned in later level-pass phases.";
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static LevelCatalog UpdateCatalog(LevelData[] levelAssets)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var so = new SerializedObject(catalog);
            var levels = so.FindProperty("levels");
            levels.arraySize = levelAssets.Length;
            for (var i = 0; i < levelAssets.Length; i++)
            {
                levels.GetArrayElementAtIndex(i).objectReferenceValue = levelAssets[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void WireBootstrapCatalog(LevelCatalog catalog)
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrap == null)
            {
                return;
            }

            var loader = bootstrap.GetComponent<LevelLoader>();
            if (loader == null)
            {
                loader = bootstrap.AddComponent<LevelLoader>();
            }

            var so = new SerializedObject(loader);
            so.FindProperty("catalog").objectReferenceValue = catalog;
            so.FindProperty("startingLevelIndex").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(loader);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void ConfigureBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[2 + Campaign.Length];
            scenes[0] = new EditorBuildSettingsScene(BootstrapScenePath, true);
            scenes[1] = new EditorBuildSettingsScene(MainMenuScenePath, true);
            for (var i = 0; i < Campaign.Length; i++)
            {
                scenes[2 + i] = new EditorBuildSettingsScene(Campaign[i].ScenePath, true);
            }

            EditorBuildSettings.scenes = scenes;
        }

        private static void WireMainMenuLevelSelect()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("MainMenuCanvas");
            if (canvas == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] MainMenuCanvas missing.");
                return;
            }

            EnsureEventSystem();

            var root = canvas.transform.Find("RootPanel");
            if (root == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] RootPanel missing.");
                return;
            }

            RelayoutRootButtons(root);

            var levelSelect = EnsureLevelSelectPanel(canvas.transform);
            var controller = Object.FindAnyObjectByType<MainMenuController>();
            if (controller == null)
            {
                var go = new GameObject("MainMenuController");
                controller = go.AddComponent<MainMenuController>();
            }

            var levelSelectView = levelSelect.GetComponent<LevelSelectView>();
            var levelSelectButton = root.Find("LevelSelectButton")?.GetComponent<Button>();

            var so = new SerializedObject(controller);
            so.FindProperty("rootPanel").objectReferenceValue = root.gameObject;
            so.FindProperty("levelSelectPanel").objectReferenceValue = levelSelect;
            so.FindProperty("levelSelectButton").objectReferenceValue = levelSelectButton;
            so.FindProperty("levelSelectView").objectReferenceValue = levelSelectView;

            // Keep existing refs if already assigned.
            AssignIfExists(so, "settingsPanel", canvas.transform.Find("SettingsPanel")?.gameObject);
            AssignIfExists(so, "controlsPanel", canvas.transform.Find("ControlsPanel")?.gameObject);
            AssignIfExists(so, "startButton", root.Find("StartButton")?.GetComponent<Button>());
            AssignIfExists(so, "continueButton", root.Find("ContinueButton")?.GetComponent<Button>());
            AssignIfExists(so, "settingsButton", root.Find("SettingsButton")?.GetComponent<Button>());
            AssignIfExists(so, "controlsButton", root.Find("ControlsButton")?.GetComponent<Button>());
            AssignIfExists(so, "quitButton", root.Find("QuitButton")?.GetComponent<Button>());
            AssignIfExists(so, "settingsBackButton", canvas.transform.Find("SettingsPanel/SettingsBackButton")?.GetComponent<Button>());
            AssignIfExists(so, "controlsBackButton", canvas.transform.Find("ControlsPanel/ControlsBackButton")?.GetComponent<Button>());
            var audioView = canvas.transform.Find("SettingsPanel")?.GetComponent<AudioSettingsView>();
            AssignIfExists(so, "audioSettingsView", audioView);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void RelayoutRootButtons(Transform root)
        {
            EnsureRootButton(root, "StartButton", "Start Game", new Vector2(0f, 100f));
            EnsureRootButton(root, "ContinueButton", "Continue", new Vector2(0f, 30f));
            EnsureRootButton(root, "LevelSelectButton", "Level Select", new Vector2(0f, -40f));
            EnsureRootButton(root, "SettingsButton", "Settings", new Vector2(0f, -110f));
            EnsureRootButton(root, "ControlsButton", "Controls", new Vector2(0f, -180f));
            EnsureRootButton(root, "QuitButton", "Quit", new Vector2(0f, -250f));
        }

        private static void EnsureRootButton(Transform root, string name, string label, Vector2 pos)
        {
            var existing = root.Find(name);
            Button button;
            if (existing == null)
            {
                button = CreateButton(root, name, label, pos);
            }
            else
            {
                button = existing.GetComponent<Button>();
                var rect = existing.GetComponent<RectTransform>();
                rect.anchoredPosition = pos;
                var text = existing.GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.text = label;
                }
            }

            _ = button;
        }

        private static GameObject EnsureLevelSelectPanel(Transform canvas)
        {
            var existing = canvas.Find("LevelSelectPanel");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var panel = CreateFullPanel(canvas, "LevelSelectPanel", PanelColor);
            CreateLabel(panel.transform, "Title", "LEVEL SELECT", 44, new Vector2(0f, 210f));

            var listRoot = new GameObject("ButtonRoot", typeof(RectTransform));
            listRoot.transform.SetParent(panel.transform, false);
            var listRect = listRoot.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.5f, 0.5f);
            listRect.anchorMax = new Vector2(0.5f, 0.5f);
            listRect.sizeDelta = new Vector2(420f, 280f);
            listRect.anchoredPosition = new Vector2(0f, 20f);

            var template = CreateButton(listRoot.transform, "LevelButtonTemplate", "1. Level", new Vector2(0f, 80f));
            template.gameObject.SetActive(false);

            var back = CreateButton(panel.transform, "BackButton", "Back", new Vector2(0f, -200f));

            var view = panel.AddComponent<LevelSelectView>();
            var so = new SerializedObject(view);
            so.FindProperty("buttonRoot").objectReferenceValue = listRoot.transform;
            so.FindProperty("levelButtonTemplate").objectReferenceValue = template;
            so.FindProperty("backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();

            panel.SetActive(false);
            return panel;
        }

        private static void AssignIfExists(SerializedObject so, string field, Object value)
        {
            if (value == null)
            {
                return;
            }

            var prop = so.FindProperty(field);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateFullPanel(Transform parent, string name, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = color;
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panel;
        }

        private static Text CreateLabel(Transform parent, string name, string message, int fontSize, Vector2 anchoredPosition)
        {
            var labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);
            var text = labelObject.AddComponent<Text>();
            text.text = message;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            var rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900f, 80f);
            rect.anchoredPosition = anchoredPosition;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.AddComponent<Image>();
            image.color = ButtonColor;

            var button = buttonObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = ButtonHighlight;
            colors.pressedColor = new Color(0.12f, 0.34f, 0.24f, 1f);
            colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.65f);
            button.colors = colors;

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300f, 54f);
            rect.anchoredPosition = anchoredPosition;

            var textObject = new GameObject("Label", typeof(RectTransform));
            textObject.transform.SetParent(buttonObject.transform, false);
            var text = textObject.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
