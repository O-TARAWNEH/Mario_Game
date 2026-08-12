// Filename: Phase34MainMenuScaleSetup.cs
// Folder: Assets/Editor/
// Purpose: Scales up Main Menu UI and merges Settings + Controls into Options (Phase 34).
// Menu: Bounder Trail/Phase 34/Setup Main Menu Scale
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase34MainMenuScaleSetup.SetupMainMenuScale

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase34MainMenuScaleSetup
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        private const float ButtonWidth = 520f;
        private const float ButtonHeight = 76f;
        private const int ButtonFontSize = 34;
        private const int TitleFontSize = 72;
        private const int SubtitleFontSize = 36;
        private const int PanelTitleFontSize = 52;
        private const int BodyFontSize = 30;

        [MenuItem("Bounder Trail/Phase 34/Setup Main Menu Scale")]
        public static void SetupMainMenuScale()
        {
            if (!System.IO.File.Exists(MainMenuScenePath))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing {MainMenuScenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("MainMenuCanvas");
            if (canvas == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] MainMenuCanvas missing.");
                return;
            }

            ConfigureCanvasScaler(canvas);
            var root = canvas.transform.Find("RootPanel");
            if (root == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] RootPanel missing.");
                return;
            }

            ScaleTitle(root, "Title", TitleFontSize, new Vector2(0f, 260f), 1100f, 100f);
            ScaleTitle(root, "Subtitle", SubtitleFontSize, new Vector2(0f, 185f), 900f, 70f);

            LayoutRootButton(root, "StartButton", "Start Game", new Vector2(0f, 90f));
            LayoutRootButton(root, "ContinueButton", "Continue", new Vector2(0f, 0f));
            LayoutRootButton(root, "LevelSelectButton", "Level Select", new Vector2(0f, -90f));
            LayoutRootButton(root, "SettingsButton", "Options", new Vector2(0f, -180f));

            var controlsButton = root.Find("ControlsButton");
            if (controlsButton != null)
            {
                Object.DestroyImmediate(controlsButton.gameObject);
            }

            LayoutRootButton(root, "QuitButton", "Quit", new Vector2(0f, -270f));

            MergeOptionsPanel(canvas.transform);
            HideLegacyControlsPanel(canvas.transform);
            WireController(canvas.transform, root);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 34 main menu scaled — larger UI, Options merged.");
        }

        private static void ConfigureCanvasScaler(GameObject canvas)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            EditorUtility.SetDirty(scaler);
        }

        private static void LayoutRootButton(Transform root, string name, string label, Vector2 pos)
        {
            var t = root.Find(name);
            if (t == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Missing button {name}");
                return;
            }

            var rect = t.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            rect.anchoredPosition = pos;

            var text = t.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
                text.fontSize = ButtonFontSize;
            }

            EditorUtility.SetDirty(t.gameObject);
        }

        private static void ScaleTitle(Transform root, string name, int fontSize, Vector2 pos, float width, float height)
        {
            var t = root.Find(name);
            if (t == null)
            {
                return;
            }

            var rect = t.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = pos;
            var text = t.GetComponent<Text>();
            if (text != null)
            {
                text.fontSize = fontSize;
            }

            EditorUtility.SetDirty(t.gameObject);
        }

        private static void MergeOptionsPanel(Transform canvas)
        {
            var settings = canvas.Find("SettingsPanel");
            if (settings == null)
            {
                return;
            }

            var title = settings.Find("Title");
            if (title != null)
            {
                var titleText = title.GetComponent<Text>();
                if (titleText != null)
                {
                    titleText.text = "OPTIONS";
                    titleText.fontSize = PanelTitleFontSize;
                }

                var titleRect = title.GetComponent<RectTransform>();
                titleRect.anchoredPosition = new Vector2(0f, 280f);
                titleRect.sizeDelta = new Vector2(1000f, 90f);
            }

            var audioHeader = settings.Find("AudioHeader");
            if (audioHeader != null)
            {
                var headerText = audioHeader.GetComponent<Text>();
                if (headerText != null)
                {
                    headerText.fontSize = BodyFontSize;
                }

                var headerRect = audioHeader.GetComponent<RectTransform>();
                headerRect.anchoredPosition = new Vector2(0f, 200f);
            }

            ScaleVolumeRow(settings, "Master", new Vector2(0f, 120f));
            ScaleVolumeRow(settings, "Music", new Vector2(0f, 40f));
            ScaleVolumeRow(settings, "Sfx", new Vector2(0f, -40f));

            var controlsBody = settings.Find("ControlsBody");
            if (controlsBody == null)
            {
                controlsBody = CreateControlsBody(settings).transform;
            }

            var bodyText = controlsBody.GetComponent<Text>();
            if (bodyText != null)
            {
                bodyText.text =
                    "Controls\n\nMove: A / D  or  Arrow Keys\nJump: Space\nRun: Left Shift\nPause: Esc";
                bodyText.fontSize = BodyFontSize;
                bodyText.lineSpacing = 1.05f;
            }

            var bodyRect = controlsBody.GetComponent<RectTransform>();
            bodyRect.anchoredPosition = new Vector2(0f, -150f);
            bodyRect.sizeDelta = new Vector2(900f, 220f);

            var back = settings.Find("SettingsBackButton");
            if (back != null)
            {
                var backRect = back.GetComponent<RectTransform>();
                backRect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
                backRect.anchoredPosition = new Vector2(0f, -320f);
                var backText = back.GetComponentInChildren<Text>();
                if (backText != null)
                {
                    backText.fontSize = ButtonFontSize;
                }
            }

            var reset = settings.Find("ResetSaveButton");
            if (reset != null)
            {
                var resetRect = reset.GetComponent<RectTransform>();
                resetRect.sizeDelta = new Vector2(420f, 64f);
                resetRect.anchoredPosition = new Vector2(0f, -240f);
                var resetText = reset.GetComponentInChildren<Text>();
                if (resetText != null)
                {
                    resetText.fontSize = 28;
                }
            }

            EditorUtility.SetDirty(settings.gameObject);
        }

        private static GameObject CreateControlsBody(Transform settings)
        {
            var go = new GameObject("ControlsBody", typeof(RectTransform));
            go.transform.SetParent(settings, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return go;
        }

        private static void ScaleVolumeRow(Transform settings, string rowName, Vector2 pos)
        {
            var row = settings.Find(rowName);
            if (row == null)
            {
                return;
            }

            var rect = row.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(720f, 56f);

            var label = row.Find("Label")?.GetComponent<Text>();
            if (label != null)
            {
                label.fontSize = 28;
            }

            var value = row.Find("Value")?.GetComponent<Text>();
            if (value != null)
            {
                value.fontSize = 26;
            }
        }

        private static void HideLegacyControlsPanel(Transform canvas)
        {
            var controls = canvas.Find("ControlsPanel");
            if (controls != null)
            {
                controls.gameObject.SetActive(false);
            }
        }

        private static void WireController(Transform canvas, Transform root)
        {
            var controller = Object.FindAnyObjectByType<MainMenuController>();
            if (controller == null)
            {
                return;
            }

            var so = new SerializedObject(controller);
            so.FindProperty("rootPanel").objectReferenceValue = root.gameObject;
            so.FindProperty("settingsPanel").objectReferenceValue = canvas.Find("SettingsPanel")?.gameObject;
            so.FindProperty("controlsPanel").objectReferenceValue = null;
            so.FindProperty("levelSelectPanel").objectReferenceValue = canvas.Find("LevelSelectPanel")?.gameObject;
            so.FindProperty("startButton").objectReferenceValue = root.Find("StartButton")?.GetComponent<Button>();
            so.FindProperty("continueButton").objectReferenceValue = root.Find("ContinueButton")?.GetComponent<Button>();
            so.FindProperty("levelSelectButton").objectReferenceValue = root.Find("LevelSelectButton")?.GetComponent<Button>();
            so.FindProperty("settingsButton").objectReferenceValue = root.Find("SettingsButton")?.GetComponent<Button>();
            so.FindProperty("controlsButton").objectReferenceValue = null;
            so.FindProperty("quitButton").objectReferenceValue = root.Find("QuitButton")?.GetComponent<Button>();
            so.FindProperty("settingsBackButton").objectReferenceValue =
                canvas.Find("SettingsPanel/SettingsBackButton")?.GetComponent<Button>();
            so.FindProperty("controlsBackButton").objectReferenceValue = null;
            so.FindProperty("resetSaveButton").objectReferenceValue =
                canvas.Find("SettingsPanel/ResetSaveButton")?.GetComponent<Button>();
            so.FindProperty("audioSettingsView").objectReferenceValue =
                canvas.Find("SettingsPanel")?.GetComponent<AudioSettingsView>();
            so.FindProperty("levelSelectView").objectReferenceValue =
                canvas.Find("LevelSelectPanel")?.GetComponent<LevelSelectView>();
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }
    }
}
#endif
