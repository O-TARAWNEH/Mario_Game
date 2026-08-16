// Filename: Phase37OverlayUiScaleSetup.cs
// Folder: Assets/Editor/
// Purpose: Scales Level Complete / Game Over / Pause overlays for readability (Phase 37).
// Menu: Bounder Trail/Phase 37/Setup Overlay UI Scale
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase37OverlayUiScaleSetup.SetupOverlayUiScale

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase37OverlayUiScaleSetup
    {
        private static readonly string[] GameplayScenes =
        {
            "Assets/Scenes/Gameplay.unity",
            $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity"
        };

        private const float ButtonWidth = 520f;
        private const float ButtonHeight = 76f;
        private const int ButtonFontSize = 34;
        private const int TitleFontSize = 72;
        private const int SummaryFontSize = 36;

        [MenuItem("Bounder Trail/Phase 37/Setup Overlay UI Scale")]
        public static void SetupOverlayUiScale()
        {
            var touched = 0;
            for (var i = 0; i < GameplayScenes.Length; i++)
            {
                if (!System.IO.File.Exists(GameplayScenes[i]))
                {
                    continue;
                }

                ScaleScene(GameplayScenes[i]);
                touched++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log(
                $"{GameLog.ProjectPrefix}[Setup] Phase 37 overlay UI scaled on {touched} scene(s) — " +
                "Level Complete / Game Over / Pause text and buttons enlarged.");
        }

        private static void ScaleScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < canvases.Length; i++)
            {
                ConfigureCanvasScaler(canvases[i].gameObject);
            }

            ScaleOverlayPanel(FindIncludingInactive("LevelCompletePanel"), isLevelComplete: true);
            ScaleOverlayPanel(FindIncludingInactive("GameOverPanel"), isLevelComplete: false);
            ScaleOverlayPanel(FindIncludingInactive("PausePanel"), isLevelComplete: false);
            ScalePauseSettingsPanel(FindIncludingInactive("PauseSettingsPanel"));

            WireLevelCompleteTitle(Object.FindAnyObjectByType<GameplayFlowController>());

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void ConfigureCanvasScaler(GameObject canvas)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(
                ProjectConstants.ReferenceWidth,
                ProjectConstants.ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            EditorUtility.SetDirty(scaler);
        }

        private static void ScaleOverlayPanel(GameObject panel, bool isLevelComplete)
        {
            if (panel == null)
            {
                return;
            }

            var title = panel.transform.Find("Title");
            if (title != null)
            {
                var titleRect = title.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.5f, 0.5f);
                titleRect.anchorMax = new Vector2(0.5f, 0.5f);
                titleRect.pivot = new Vector2(0.5f, 0.5f);
                titleRect.anchoredPosition = new Vector2(0f, 220f);
                titleRect.sizeDelta = new Vector2(1100f, 100f);

                var titleText = title.GetComponent<Text>();
                if (titleText != null)
                {
                    titleText.fontSize = TitleFontSize;
                    titleText.alignment = TextAnchor.MiddleCenter;
                    titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    titleText.verticalOverflow = VerticalWrapMode.Overflow;
                    titleText.resizeTextForBestFit = false;
                }

                EditorUtility.SetDirty(title.gameObject);
            }

            var summary = panel.transform.Find("RunSummary");
            if (summary != null)
            {
                var summaryRect = summary.GetComponent<RectTransform>();
                summaryRect.anchorMin = new Vector2(0.5f, 0.5f);
                summaryRect.anchorMax = new Vector2(0.5f, 0.5f);
                summaryRect.pivot = new Vector2(0.5f, 0.5f);
                summaryRect.anchoredPosition = new Vector2(0f, 110f);
                summaryRect.sizeDelta = new Vector2(900f, 110f);

                var summaryText = summary.GetComponent<Text>();
                if (summaryText != null)
                {
                    summaryText.fontSize = SummaryFontSize;
                    summaryText.alignment = TextAnchor.MiddleCenter;
                    summaryText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    summaryText.verticalOverflow = VerticalWrapMode.Overflow;
                    summaryText.lineSpacing = 1.15f;
                    summaryText.resizeTextForBestFit = false;
                }

                EditorUtility.SetDirty(summary.gameObject);
            }

            if (isLevelComplete)
            {
                LayoutButton(panel.transform, "LevelCompleteNextButton", new Vector2(0f, -10f));
                LayoutButton(panel.transform, "LevelCompleteRestartButton", new Vector2(0f, -110f));
                LayoutButton(panel.transform, "LevelCompleteMainMenuButton", new Vector2(0f, -210f));
            }
            else if (panel.name == "GameOverPanel")
            {
                LayoutButton(panel.transform, "GameOverRestartButton", new Vector2(0f, -40f));
                LayoutButton(panel.transform, "GameOverMainMenuButton", new Vector2(0f, -140f));
            }
            else if (panel.name == "PausePanel")
            {
                LayoutButton(panel.transform, "ResumeButton", new Vector2(0f, 40f));
                LayoutButton(panel.transform, "PauseRestartButton", new Vector2(0f, -60f));
                LayoutButton(panel.transform, "PauseSettingsButton", new Vector2(0f, -160f));
                LayoutButton(panel.transform, "PauseMainMenuButton", new Vector2(0f, -260f));
            }

            var flowSummary = panel.GetComponent<FlowScreenSummary>();
            if (flowSummary != null)
            {
                var so = new SerializedObject(flowSummary);
                so.FindProperty("format").stringValue = "Coins  {0}\nScore  {1}";
                so.FindProperty("campaignCompleteFormat").stringValue =
                    "Campaign cleared!\nCoins  {0}\nScore  {1}";
                if (summary != null)
                {
                    so.FindProperty("summaryText").objectReferenceValue = summary.GetComponent<Text>();
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(flowSummary);
            }

            EditorUtility.SetDirty(panel);
        }

        private static void ScalePauseSettingsPanel(GameObject panel)
        {
            if (panel == null)
            {
                return;
            }

            var title = panel.transform.Find("Title");
            if (title != null)
            {
                var titleText = title.GetComponent<Text>();
                if (titleText != null)
                {
                    titleText.fontSize = Mathf.Max(titleText.fontSize, 52);
                }

                var titleRect = title.GetComponent<RectTransform>();
                titleRect.sizeDelta = new Vector2(1000f, 90f);
                titleRect.anchoredPosition = new Vector2(0f, 250f);
            }

            LayoutButton(panel.transform, "PauseSettingsBackButton", new Vector2(0f, -280f));
            EditorUtility.SetDirty(panel);
        }

        private static void LayoutButton(Transform panel, string name, Vector2 pos)
        {
            var t = panel.Find(name);
            if (t == null)
            {
                return;
            }

            var rect = t.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            rect.anchoredPosition = pos;

            var text = t.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.fontSize = ButtonFontSize;
                text.alignment = TextAnchor.MiddleCenter;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }

            EditorUtility.SetDirty(t.gameObject);
        }

        private static void WireLevelCompleteTitle(GameplayFlowController controller)
        {
            if (controller == null)
            {
                return;
            }

            var panel = FindIncludingInactive("LevelCompletePanel");
            if (panel == null)
            {
                return;
            }

            var title = panel.transform.Find("Title")?.GetComponent<Text>();
            var so = new SerializedObject(controller);
            so.FindProperty("levelCompleteTitleText").objectReferenceValue = title;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
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
