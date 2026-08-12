// Filename: Phase21SaveSystemSetup.cs
// Folder: Assets/Editor/
// Purpose: Wires SaveSystem on bootstrap and Reset Save in Settings (Phase 21).
// Menu: Bounder Trail/Phase 21/Setup Save System
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase21SaveSystemSetup.SetupSaveSystem

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Save;
using BounderTrail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase21SaveSystemSetup
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        private static readonly Color ButtonColor = new Color(0.45f, 0.22f, 0.22f, 1f);
        private static readonly Color ButtonHighlight = new Color(0.62f, 0.32f, 0.32f, 1f);

        [MenuItem("Bounder Trail/Phase 21/Setup Save System")]
        public static void SetupSaveSystem()
        {
            WireBootstrap();
            WireMainMenuResetSave();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 21 save system ready.");
        }

        private static void WireBootstrap()
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrap == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] GameBootstrap missing.");
                return;
            }

            if (bootstrap.GetComponent<SaveSystem>() == null)
            {
                bootstrap.AddComponent<SaveSystem>();
            }

            if (bootstrap.GetComponent<GameProgress>() == null)
            {
                bootstrap.AddComponent<GameProgress>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void WireMainMenuResetSave()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            var settingsPanel = GameObject.Find("SettingsPanel");
            if (settingsPanel == null)
            {
                var canvas = GameObject.Find("MainMenuCanvas");
                settingsPanel = canvas != null ? canvas.transform.Find("SettingsPanel")?.gameObject : null;
            }

            if (settingsPanel == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] SettingsPanel missing.");
                return;
            }

            // Nudge volume rows up slightly and place Reset Save above Back.
            var back = settingsPanel.transform.Find("SettingsBackButton");
            if (back != null)
            {
                back.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -230f);
            }

            var existing = settingsPanel.transform.Find("ResetSaveButton");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var resetButton = CreateButton(settingsPanel.transform, "ResetSaveButton", "Reset Save", new Vector2(0f, -160f));
            CreateLabel(settingsPanel.transform, "ResetHint", "Clears campaign progress. Keeps audio settings.", 16, new Vector2(0f, -195f));

            var controller = Object.FindAnyObjectByType<MainMenuController>();
            if (controller != null)
            {
                var so = new SerializedObject(controller);
                so.FindProperty("resetSaveButton").objectReferenceValue = resetButton;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(controller);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
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
            colors.pressedColor = new Color(0.3f, 0.14f, 0.14f, 1f);
            button.colors = colors;

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 48f);
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

        private static void CreateLabel(Transform parent, string name, string message, int fontSize, Vector2 anchoredPosition)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

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
            text.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            text.raycastTarget = false;
            var rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(700f, 28f);
            rect.anchoredPosition = anchoredPosition;
        }
    }
}
#endif
