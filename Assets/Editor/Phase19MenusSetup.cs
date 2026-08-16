// Filename: Phase19MenusSetup.cs
// Folder: Assets/Editor/
// Purpose: Builds complete Main Menu + pause settings flow (Phase 19).
// Menu: Bounder Trail/Phase 19/Setup Game Menus
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase19MenusSetup.SetupGameMenus

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Save;
using BounderTrail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase19MenusSetup
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        private static readonly Color ButtonColor = new Color(0.18f, 0.48f, 0.34f, 1f);
        private static readonly Color ButtonHighlight = new Color(0.28f, 0.62f, 0.44f, 1f);
        private static readonly Color PanelColor = new Color(0.05f, 0.08f, 0.1f, 0.88f);
        private static readonly Color SliderBg = new Color(0.12f, 0.14f, 0.16f, 1f);
        private static readonly Color SliderFill = new Color(0.35f, 0.75f, 0.55f, 1f);

        [MenuItem("Bounder Trail/Phase 19/Setup Game Menus")]
        public static void SetupGameMenus()
        {
            EnsureBootstrapProgress();
            BuildMainMenuScene();
            PolishGameplayMenus();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 19 game menus ready.");
        }

        private static void EnsureBootstrapProgress()
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrap == null)
            {
                bootstrap = new GameObject(ProjectConstants.BootstrapObjectName);
                bootstrap.AddComponent<GameBootstrap>();
            }

            if (bootstrap.GetComponent<GameProgress>() == null)
            {
                bootstrap.AddComponent<GameProgress>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EnsureEventSystem();

            var canvas = CreateCanvas("MainMenuCanvas");
            var root = CreateFullPanel(canvas.transform, "RootPanel", new Color(0.04f, 0.07f, 0.09f, 0.92f));
            CreateLabel(root.transform, "Title", ProjectConstants.GameTitle, 56, new Vector2(0f, 210f));
            CreateLabel(root.transform, "Subtitle", "Main Menu", 24, new Vector2(0f, 150f));

            var start = CreateButton(root.transform, "StartButton", "Start Game", new Vector2(0f, 70f));
            var cont = CreateButton(root.transform, "ContinueButton", "Continue", new Vector2(0f, 0f));
            var settings = CreateButton(root.transform, "SettingsButton", "Settings", new Vector2(0f, -70f));
            var controls = CreateButton(root.transform, "ControlsButton", "Controls", new Vector2(0f, -140f));
            var quit = CreateButton(root.transform, "QuitButton", "Quit", new Vector2(0f, -210f));

            var settingsPanel = CreateFullPanel(canvas.transform, "SettingsPanel", PanelColor);
            CreateLabel(settingsPanel.transform, "Title", "SETTINGS", 44, new Vector2(0f, 210f));
            CreateLabel(settingsPanel.transform, "AudioHeader", "Audio", 28, new Vector2(0f, 140f));
            var master = CreateVolumeRow(settingsPanel.transform, "Master", "Master", new Vector2(0f, 70f));
            var music = CreateVolumeRow(settingsPanel.transform, "Music", "Music", new Vector2(0f, 0f));
            var sfx = CreateVolumeRow(settingsPanel.transform, "Sfx", "SFX", new Vector2(0f, -70f));
            var settingsBack = CreateButton(settingsPanel.transform, "SettingsBackButton", "Back", new Vector2(0f, -180f));
            settingsPanel.SetActive(false);

            var audioView = settingsPanel.AddComponent<AudioSettingsView>();
            var audioSo = new SerializedObject(audioView);
            audioSo.FindProperty("masterSlider").objectReferenceValue = master.slider;
            audioSo.FindProperty("musicSlider").objectReferenceValue = music.slider;
            audioSo.FindProperty("sfxSlider").objectReferenceValue = sfx.slider;
            audioSo.FindProperty("masterValueLabel").objectReferenceValue = master.valueLabel;
            audioSo.FindProperty("musicValueLabel").objectReferenceValue = music.valueLabel;
            audioSo.FindProperty("sfxValueLabel").objectReferenceValue = sfx.valueLabel;
            audioSo.ApplyModifiedPropertiesWithoutUndo();

            var controlsPanel = CreateFullPanel(canvas.transform, "ControlsPanel", PanelColor);
            CreateLabel(controlsPanel.transform, "Title", "CONTROLS", 44, new Vector2(0f, 180f));
            CreateLabel(
                controlsPanel.transform,
                "ControlsBody",
                "Move: A / D\nJump: W\nRun: Left Shift\nPause: Esc",
                26,
                new Vector2(0f, 20f),
                height: 180f);
            var controlsBack = CreateButton(controlsPanel.transform, "ControlsBackButton", "Back", new Vector2(0f, -160f));
            controlsPanel.SetActive(false);

            var controllerObject = new GameObject("MainMenuController");
            var controller = controllerObject.AddComponent<MainMenuController>();
            var so = new SerializedObject(controller);
            so.FindProperty("rootPanel").objectReferenceValue = root;
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            so.FindProperty("controlsPanel").objectReferenceValue = controlsPanel;
            so.FindProperty("startButton").objectReferenceValue = start;
            so.FindProperty("continueButton").objectReferenceValue = cont;
            so.FindProperty("settingsButton").objectReferenceValue = settings;
            so.FindProperty("controlsButton").objectReferenceValue = controls;
            so.FindProperty("quitButton").objectReferenceValue = quit;
            so.FindProperty("settingsBackButton").objectReferenceValue = settingsBack;
            so.FindProperty("controlsBackButton").objectReferenceValue = controlsBack;
            so.FindProperty("audioSettingsView").objectReferenceValue = audioView;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void PolishGameplayMenus()
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("GameplayFlowCanvas");
            if (canvas == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] GameplayFlowCanvas missing.");
                return;
            }

            var pausePanel = FindIncludingInactive("PausePanel");
            var gameOverPanel = FindIncludingInactive("GameOverPanel");
            PolishOverlayTitle(pausePanel, "PAUSED");
            PolishOverlayTitle(gameOverPanel, "GAME OVER");
            EnsureButtonLabel(pausePanel, "ResumeButton", "Resume");
            EnsureButtonLabel(pausePanel, "PauseRestartButton", "Restart");
            EnsureButtonLabel(pausePanel, "PauseMainMenuButton", "Main Menu");
            EnsureButtonLabel(gameOverPanel, "GameOverRestartButton", "Restart");
            EnsureButtonLabel(gameOverPanel, "GameOverMainMenuButton", "Main Menu");

            // Rebuild pause button stack with Settings.
            Button pauseSettingsButton = null;
            if (pausePanel != null)
            {
                RelayoutPauseButtons(pausePanel, out pauseSettingsButton);
            }

            var pauseSettings = EnsurePauseSettingsPanel(canvas.transform);
            var controller = Object.FindAnyObjectByType<GameplayFlowController>();
            if (controller != null)
            {
                var audioView = pauseSettings.GetComponent<AudioSettingsView>();
                var so = new SerializedObject(controller);
                so.FindProperty("pauseSettingsPanel").objectReferenceValue = pauseSettings;
                so.FindProperty("pauseSettingsButton").objectReferenceValue = pauseSettingsButton;
                so.FindProperty("pauseSettingsBackButton").objectReferenceValue =
                    pauseSettings.transform.Find("SettingsBackButton")?.GetComponent<Button>();
                so.FindProperty("pauseAudioSettingsView").objectReferenceValue = audioView;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(controller);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void RelayoutPauseButtons(GameObject pausePanel, out Button settingsButton)
        {
            settingsButton = null;
            var resume = pausePanel.transform.Find("ResumeButton")?.GetComponent<Button>();
            var restart = pausePanel.transform.Find("PauseRestartButton")?.GetComponent<Button>();
            var mainMenu = pausePanel.transform.Find("PauseMainMenuButton")?.GetComponent<Button>();

            var existingSettings = pausePanel.transform.Find("PauseSettingsButton");
            if (existingSettings != null)
            {
                Object.DestroyImmediate(existingSettings.gameObject);
            }

            settingsButton = CreateButton(pausePanel.transform, "PauseSettingsButton", "Settings", new Vector2(0f, -90f));

            SetButtonPos(resume, new Vector2(0f, -20f));
            SetButtonPos(restart, new Vector2(0f, -90f));
            SetButtonPos(settingsButton, new Vector2(0f, -160f));
            SetButtonPos(mainMenu, new Vector2(0f, -230f));
        }

        private static GameObject EnsurePauseSettingsPanel(Transform canvas)
        {
            var existing = canvas.Find("PauseSettingsPanel");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var panel = CreateFullPanel(canvas, "PauseSettingsPanel", PanelColor);
            CreateLabel(panel.transform, "Title", "SETTINGS", 44, new Vector2(0f, 210f));
            CreateLabel(panel.transform, "AudioHeader", "Audio", 28, new Vector2(0f, 140f));
            var master = CreateVolumeRow(panel.transform, "Master", "Master", new Vector2(0f, 70f));
            var music = CreateVolumeRow(panel.transform, "Music", "Music", new Vector2(0f, 0f));
            var sfx = CreateVolumeRow(panel.transform, "Sfx", "SFX", new Vector2(0f, -70f));
            CreateButton(panel.transform, "SettingsBackButton", "Back", new Vector2(0f, -180f));

            var audioView = panel.AddComponent<AudioSettingsView>();
            var audioSo = new SerializedObject(audioView);
            audioSo.FindProperty("masterSlider").objectReferenceValue = master.slider;
            audioSo.FindProperty("musicSlider").objectReferenceValue = music.slider;
            audioSo.FindProperty("sfxSlider").objectReferenceValue = sfx.slider;
            audioSo.FindProperty("masterValueLabel").objectReferenceValue = master.valueLabel;
            audioSo.FindProperty("musicValueLabel").objectReferenceValue = music.valueLabel;
            audioSo.FindProperty("sfxValueLabel").objectReferenceValue = sfx.valueLabel;
            audioSo.ApplyModifiedPropertiesWithoutUndo();

            panel.SetActive(false);
            return panel;
        }

        private static void PolishOverlayTitle(GameObject panel, string title)
        {
            if (panel == null)
            {
                return;
            }

            var titleTransform = panel.transform.Find("Title");
            var text = titleTransform != null ? titleTransform.GetComponent<Text>() : null;
            if (text != null)
            {
                text.text = title;
                text.fontSize = Mathf.Max(text.fontSize, 44);
            }
        }

        private static void EnsureButtonLabel(GameObject panel, string buttonName, string label)
        {
            if (panel == null)
            {
                return;
            }

            var button = panel.transform.Find(buttonName);
            if (button == null)
            {
                return;
            }

            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
            }
        }

        private static void SetButtonPos(Button button, Vector2 pos)
        {
            if (button == null)
            {
                return;
            }

            var rect = button.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
        }

        private static (Slider slider, Text valueLabel) CreateVolumeRow(
            Transform parent,
            string key,
            string label,
            Vector2 anchoredPos)
        {
            var row = new GameObject($"{key}Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(520f, 40f);
            rowRect.anchoredPosition = anchoredPos;

            var nameLabel = CreateLabel(row.transform, "Label", label, 22, new Vector2(-180f, 0f), width: 120f, height: 36f);
            nameLabel.alignment = TextAnchor.MiddleLeft;

            var valueLabel = CreateLabel(row.transform, "Value", "100%", 20, new Vector2(220f, 0f), width: 70f, height: 36f);
            valueLabel.alignment = TextAnchor.MiddleRight;

            var slider = CreateSlider(row.transform, $"{key}Slider", Vector2.zero);
            var sliderRect = slider.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(280f, 24f);
            sliderRect.anchoredPosition = new Vector2(40f, 0f);
            return (slider, valueLabel);
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPos)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(280f, 24f);
            rootRect.anchoredPosition = anchoredPos;

            var background = new GameObject("Background", typeof(RectTransform));
            background.transform.SetParent(root.transform, false);
            var bgImage = background.AddComponent<Image>();
            bgImage.color = SliderBg;
            Stretch(background.GetComponent<RectTransform>());

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            Stretch(fillAreaRect);
            fillAreaRect.offsetMin = new Vector2(4f, 4f);
            fillAreaRect.offsetMax = new Vector2(-4f, -4f);

            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = SliderFill;
            Stretch(fill.GetComponent<RectTransform>());

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(root.transform, false);
            Stretch(handleArea.GetComponent<RectTransform>());

            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(18f, 24f);

            var slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.value = 1f;
            return slider;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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

        private static Text CreateLabel(
            Transform parent,
            string name,
            string message,
            int fontSize,
            Vector2 anchoredPosition,
            float width = 900f,
            float height = 80f)
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
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            var rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
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
            rect.sizeDelta = new Vector2(260f, 54f);
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

            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            Stretch(text.GetComponent<RectTransform>());
            return button;
        }

        private static GameObject FindIncludingInactive(string name)
        {
            var transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (var i = 0; i < transforms.Length; i++)
            {
                var t = transforms[i];
                if (t == null || t.name != name || string.IsNullOrEmpty(t.gameObject.scene.name))
                {
                    continue;
                }

                return t.gameObject;
            }

            return null;
        }
    }
}
#endif
