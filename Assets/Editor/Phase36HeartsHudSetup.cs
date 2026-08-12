// Filename: Phase36HeartsHudSetup.cs
// Folder: Assets/Editor/
// Purpose: Wires heart health HUD on all gameplay scenes (Phase 36).
// Menu: Bounder Trail/Phase 36/Setup Hearts HUD

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
    public static class Phase36HeartsHudSetup
    {
        private static readonly string[] GameplayScenes =
        {
            "Assets/Scenes/Gameplay.unity",
            $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity"
        };

        private const string HeartSpritePath = "Assets/Art/Items/PowerUp_HeartDrop.png";

        [MenuItem("Bounder Trail/Phase 36/Setup Hearts HUD")]
        public static void SetupHeartsHud()
        {
            var heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HeartSpritePath);
            if (heartSprite == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing heart sprite: {HeartSpritePath}");
                return;
            }

            for (var i = 0; i < GameplayScenes.Length; i++)
            {
                if (!System.IO.File.Exists(GameplayScenes[i]))
                {
                    continue;
                }

                WireScene(GameplayScenes[i], heartSprite);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 36 hearts HUD wired on gameplay scenes.");
        }

        private static void WireScene(string scenePath, Sprite heartSprite)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var hud = Object.FindAnyObjectByType<GameplayHud>();
            if (hud == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] No GameplayHud in {scenePath}");
                return;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            var health = player != null ? player.GetComponent<PlayerHealth>() : null;

            var hearts = hud.GetComponent<HealthHeartsDisplay>();
            if (hearts == null)
            {
                hearts = hud.gameObject.AddComponent<HealthHeartsDisplay>();
            }

            var heartsSo = new SerializedObject(hearts);
            heartsSo.FindProperty("playerHealth").objectReferenceValue = health;
            heartsSo.FindProperty("heartFullSprite").objectReferenceValue = heartSprite;
            heartsSo.FindProperty("heartEmptySprite").objectReferenceValue = heartSprite;
            heartsSo.ApplyModifiedPropertiesWithoutUndo();

            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("heartsDisplay").objectReferenceValue = hearts;
            hudSo.FindProperty("playerHealth").objectReferenceValue = health;
            hudSo.FindProperty("livesFormat").stringValue = "Retries: {0}";

            var healthText = hudSo.FindProperty("healthText").objectReferenceValue as Text;
            if (healthText != null)
            {
                healthText.gameObject.SetActive(false);
            }

            var livesText = hudSo.FindProperty("livesText").objectReferenceValue as Text;
            if (livesText != null)
            {
                var livesRect = livesText.GetComponent<RectTransform>();
                livesRect.anchoredPosition = new Vector2(24f, -48f);
            }

            hudSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(hearts);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }
    }
}
#endif
