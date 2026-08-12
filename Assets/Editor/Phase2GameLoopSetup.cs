// Filename: Phase2GameLoopSetup.cs
// Folder: Assets/Editor/
// Purpose: Creates MainMenu + Gameplay scenes with minimal flow UI and build settings (Phase 2).
// Dependencies: BounderTrail.Core, BounderTrail.UI
//
// Menu: Bounder Trail/Phase 2/Setup Core Game Loop
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase2GameLoopSetup.SetupCoreGameLoop

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase2GameLoopSetup
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Bounder Trail/Phase 2/Setup Core Game Loop")]
        public static void SetupCoreGameLoop()
        {
            EnsureBootstrapHasStateManager();
            CreateMainMenuScene();
            CreateGameplayScene();
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 2 core game loop scenes ready.");
        }

        private static void EnsureBootstrapHasStateManager()
        {
            var bootstrapScene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrapObject = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrapObject == null)
            {
                bootstrapObject = new GameObject(ProjectConstants.BootstrapObjectName);
                bootstrapObject.AddComponent<GameBootstrap>();
            }

            if (bootstrapObject.GetComponent<GameBootstrap>() == null)
            {
                bootstrapObject.AddComponent<GameBootstrap>();
            }

            if (bootstrapObject.GetComponent<GameStateManager>() == null)
            {
                bootstrapObject.AddComponent<GameStateManager>();
            }

            EditorSceneManager.SaveScene(bootstrapScene, BootstrapScenePath);
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EnsureEventSystem();

            var canvas = CreateCanvas("MainMenuCanvas");
            var title = CreateLabel(canvas.transform, "Title", ProjectConstants.GameTitle, 48, new Vector2(0f, 120f));
            var subtitle = CreateLabel(canvas.transform, "Subtitle", "Main Menu (Phase 2)", 24, new Vector2(0f, 60f));

            var playButton = CreateButton(canvas.transform, "PlayButton", "Play", new Vector2(0f, -20f));
            var quitButton = CreateButton(canvas.transform, "QuitButton", "Quit", new Vector2(0f, -90f));

            var controllerObject = new GameObject("MainMenuController");
            var controller = controllerObject.AddComponent<MainMenuController>();
            AssignObjectReference(controller, "playButton", playButton);
            AssignObjectReference(controller, "quitButton", quitButton);

            // Keep unused warning free in editor generation.
            _ = title;
            _ = subtitle;

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void CreateGameplayScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EnsureEventSystem();

            var canvas = CreateCanvas("GameplayFlowCanvas");

            var hudHint = CreatePanel(canvas.transform, "HudHintPanel", new Color(0f, 0f, 0f, 0.35f), fullScreen: false);
            var hudRect = hudHint.GetComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0f, 1f);
            hudRect.anchorMax = new Vector2(1f, 1f);
            hudRect.pivot = new Vector2(0.5f, 1f);
            hudRect.sizeDelta = new Vector2(0f, 90f);
            hudRect.anchoredPosition = Vector2.zero;
            CreateLabel(
                hudHint.transform,
                "HudHintText",
                "GAMEPLAY | Esc=Pause | G=Game Over | C=Level Complete",
                22,
                Vector2.zero);

            var pausePanel = CreateOverlayPanel(canvas.transform, "PausePanel", "PAUSED");
            var resumeButton = CreateButton(pausePanel.transform, "ResumeButton", "Resume", new Vector2(0f, -20f));
            var pauseRestartButton = CreateButton(pausePanel.transform, "PauseRestartButton", "Restart", new Vector2(0f, -90f));
            var pauseMainMenuButton = CreateButton(pausePanel.transform, "PauseMainMenuButton", "Main Menu", new Vector2(0f, -160f));

            var gameOverPanel = CreateOverlayPanel(canvas.transform, "GameOverPanel", "GAME OVER");
            var gameOverRestartButton = CreateButton(gameOverPanel.transform, "GameOverRestartButton", "Restart", new Vector2(0f, -40f));
            var gameOverMainMenuButton = CreateButton(gameOverPanel.transform, "GameOverMainMenuButton", "Main Menu", new Vector2(0f, -110f));

            var levelCompletePanel = CreateOverlayPanel(canvas.transform, "LevelCompletePanel", "LEVEL COMPLETE");
            var levelCompleteRestartButton = CreateButton(levelCompletePanel.transform, "LevelCompleteRestartButton", "Restart", new Vector2(0f, -40f));
            var levelCompleteMainMenuButton = CreateButton(levelCompletePanel.transform, "LevelCompleteMainMenuButton", "Main Menu", new Vector2(0f, -110f));

            pausePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            levelCompletePanel.SetActive(false);

            var controllerObject = new GameObject("GameplayFlowController");
            var controller = controllerObject.AddComponent<GameplayFlowController>();
            AssignObjectReference(controller, "pausePanel", pausePanel);
            AssignObjectReference(controller, "gameOverPanel", gameOverPanel);
            AssignObjectReference(controller, "levelCompletePanel", levelCompletePanel);
            AssignObjectReference(controller, "hudHintPanel", hudHint);
            AssignObjectReference(controller, "resumeButton", resumeButton);
            AssignObjectReference(controller, "pauseRestartButton", pauseRestartButton);
            AssignObjectReference(controller, "pauseMainMenuButton", pauseMainMenuButton);
            AssignObjectReference(controller, "gameOverRestartButton", gameOverRestartButton);
            AssignObjectReference(controller, "gameOverMainMenuButton", gameOverMainMenuButton);
            AssignObjectReference(controller, "levelCompleteRestartButton", levelCompleteRestartButton);
            AssignObjectReference(controller, "levelCompleteMainMenuButton", levelCompleteMainMenuButton);

            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void ConfigureBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapScenePath, true),
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true)
            };
            EditorBuildSettings.scenes = scenes;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ProjectConstants.ReferenceWidth, ProjectConstants.ReferenceHeight);
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateOverlayPanel(Transform parent, string name, string title)
        {
            var panel = CreatePanel(parent, name, new Color(0f, 0f, 0f, 0.75f), fullScreen: true);
            CreateLabel(panel.transform, "Title", title, 44, new Vector2(0f, 120f));
            return panel;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color, bool fullScreen)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = color;
            var rect = panel.GetComponent<RectTransform>();
            if (fullScreen)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                rect.sizeDelta = new Vector2(800f, 100f);
                rect.anchoredPosition = Vector2.zero;
            }

            return panel;
        }

        private static Text CreateLabel(Transform parent, string name, string message, int fontSize, Vector2 anchoredPosition)
        {
            var labelObject = new GameObject(name);
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
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900f, 80f);
            rect.anchoredPosition = anchoredPosition;
            return text;
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

        private static void AssignObjectReference(Object target, string fieldName, Object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Missing field '{fieldName}' on {target.name}");
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
