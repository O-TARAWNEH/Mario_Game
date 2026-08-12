// Filename: Phase17HudSetup.cs
// Folder: Assets/Editor/
// Purpose: Builds gameplay HUD and polishes pause/game-over/level-complete screens (Phase 17).
// Menu: Bounder Trail/Phase 17/Setup Gameplay HUD
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase17HudSetup.SetupGameplayHud

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Player;
using BounderTrail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase17HudSetup
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Bounder Trail/Phase 17/Setup Gameplay HUD")]
        public static void SetupGameplayHud()
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            var canvas = FindIncludingInactive("GameplayFlowCanvas");
            if (canvas == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] GameplayFlowCanvas missing.");
                return;
            }

            // Remove prototype debug HUD pieces.
            DestroyIfExists(FindIncludingInactive("HudHintPanel"));
            DestroyIfExists(FindIncludingInactive("CollectibleCounterHud"));

            var hudRoot = EnsureHudRoot(canvas.transform);
            var hud = WireHud(hudRoot);

            PolishOverlay(FindIncludingInactive("PausePanel"), "PAUSED", includeSummary: false);
            PolishOverlay(FindIncludingInactive("GameOverPanel"), "GAME OVER", includeSummary: true);
            PolishOverlay(FindIncludingInactive("LevelCompletePanel"), "LEVEL COMPLETE", includeSummary: true);

            var controller = Object.FindAnyObjectByType<GameplayFlowController>();
            if (controller != null)
            {
                var so = new SerializedObject(controller);
                var hudProp = so.FindProperty("gameplayHudRoot");
                if (hudProp != null)
                {
                    hudProp.objectReferenceValue = hudRoot;
                }

                // Keep legacy field null if it still exists on older serialized data.
                var hintProp = so.FindProperty("hudHintPanel");
                if (hintProp != null)
                {
                    hintProp.objectReferenceValue = null;
                }

                so.FindProperty("enableDebugShortcuts").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(controller);
            }

            _ = hud;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 17 gameplay HUD ready.");
        }

        private static GameObject EnsureHudRoot(Transform canvas)
        {
            var existing = canvas.Find("GameplayHud");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var hud = new GameObject("GameplayHud", typeof(RectTransform));
            hud.transform.SetParent(canvas, false);
            var rect = hud.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Top bar background for readability.
            var bar = CreatePanel(hud.transform, "HudBar", new Color(0f, 0f, 0f, 0.45f));
            var barRect = bar.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.sizeDelta = new Vector2(0f, 96f);
            barRect.anchoredPosition = Vector2.zero;

            return hud;
        }

        private static GameplayHud WireHud(GameObject hudRoot)
        {
            var player = GameObject.Find("Player_Pip");
            var health = player != null ? player.GetComponent<PlayerHealth>() : null;
            var powerUps = player != null ? player.GetComponent<PlayerPowerUps>() : null;

            var lives = CreateHudLabel(hudRoot.transform, "LivesText", "Retries: 3", new Vector2(24f, -48f), TextAnchor.UpperLeft);
            var hp = CreateHudLabel(hudRoot.transform, "HealthText", "HP: 3/3", new Vector2(24f, -40f), TextAnchor.UpperLeft);
            hp.gameObject.SetActive(false);
            var coins = CreateHudLabel(hudRoot.transform, "CoinsText", "Coins: 0", new Vector2(220f, -12f), TextAnchor.UpperLeft);
            var score = CreateHudLabel(hudRoot.transform, "ScoreText", "Score: 0", new Vector2(220f, -40f), TextAnchor.UpperLeft);
            var level = CreateHudLabel(hudRoot.transform, "LevelText", "Level", new Vector2(-24f, -12f), TextAnchor.UpperRight, right: true);
            var power = CreateHudLabel(hudRoot.transform, "PowerUpText", "Power: —", new Vector2(-24f, -40f), TextAnchor.UpperRight, right: true);
            var pause = CreateHudLabel(hudRoot.transform, "PauseIndicator", "PAUSED", new Vector2(0f, -110f), TextAnchor.UpperCenter, center: true);
            pause.fontSize = 28;
            pause.gameObject.SetActive(false);

            var hud = hudRoot.GetComponent<GameplayHud>();
            if (hud == null)
            {
                hud = hudRoot.AddComponent<GameplayHud>();
            }

            var hearts = hudRoot.GetComponent<HealthHeartsDisplay>();
            if (hearts == null)
            {
                hearts = hudRoot.AddComponent<HealthHeartsDisplay>();
            }

            var heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/PowerUp_HeartDrop.png");
            var heartsSo = new SerializedObject(hearts);
            heartsSo.FindProperty("playerHealth").objectReferenceValue = health;
            heartsSo.FindProperty("heartFullSprite").objectReferenceValue = heartSprite;
            heartsSo.FindProperty("heartEmptySprite").objectReferenceValue = heartSprite;
            heartsSo.ApplyModifiedPropertiesWithoutUndo();

            var so = new SerializedObject(hud);
            so.FindProperty("livesText").objectReferenceValue = lives;
            so.FindProperty("healthText").objectReferenceValue = hp;
            so.FindProperty("coinsText").objectReferenceValue = coins;
            so.FindProperty("scoreText").objectReferenceValue = score;
            so.FindProperty("levelText").objectReferenceValue = level;
            so.FindProperty("powerUpText").objectReferenceValue = power;
            so.FindProperty("pauseIndicatorText").objectReferenceValue = pause;
            so.FindProperty("playerHealth").objectReferenceValue = health;
            so.FindProperty("playerPowerUps").objectReferenceValue = powerUps;
            so.FindProperty("heartsDisplay").objectReferenceValue = hearts;
            so.FindProperty("livesFormat").stringValue = "Retries: {0}";
            so.ApplyModifiedPropertiesWithoutUndo();
            return hud;
        }

        private static void PolishOverlay(GameObject panel, string title, bool includeSummary)
        {
            if (panel == null)
            {
                return;
            }

            Text titleText = null;
            var named = panel.transform.Find("Title");
            if (named != null)
            {
                titleText = named.GetComponent<Text>();
            }

            if (titleText == null)
            {
                var texts = panel.GetComponentsInChildren<Text>(true);
                for (var i = 0; i < texts.Length; i++)
                {
                    if (texts[i].GetComponentInParent<Button>() != null)
                    {
                        continue;
                    }

                    titleText = texts[i];
                    break;
                }
            }

            if (titleText != null)
            {
                titleText.text = title;
                titleText.fontSize = Mathf.Max(titleText.fontSize, 40);
            }

            if (!includeSummary)
            {
                return;
            }

            var existing = panel.transform.Find("RunSummary");
            Text summaryText;
            if (existing == null)
            {
                summaryText = CreateHudLabel(panel.transform, "RunSummary", "Coins: 0   Score: 0", new Vector2(0f, 40f), TextAnchor.MiddleCenter, center: true);
                summaryText.fontSize = 22;
            }
            else
            {
                summaryText = existing.GetComponent<Text>();
            }

            var summary = panel.GetComponent<FlowScreenSummary>();
            if (summary == null)
            {
                summary = panel.AddComponent<FlowScreenSummary>();
            }

            var so = new SerializedObject(summary);
            so.FindProperty("summaryText").objectReferenceValue = summaryText;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return go;
        }

        private static Text CreateHudLabel(
            Transform parent,
            string name,
            string value,
            Vector2 anchoredPos,
            TextAnchor align,
            bool right = false,
            bool center = false)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();

            if (center)
            {
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
            }
            else if (right)
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
            }
            else
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
            }

            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(360f, 28f);

            var text = go.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = align;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
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

        private static void DestroyIfExists(GameObject go)
        {
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
#endif
