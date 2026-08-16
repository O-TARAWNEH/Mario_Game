// Filename: LevelSelectView.cs
// Folder: Assets/Scripts/UI/
// Purpose: Campaign level list with unlock / completion states (Phase 20).
// Dependencies: LevelLoader, LevelCatalog, GameProgress, GameStateManager, AudioManager

using BounderTrail.Audio;
using BounderTrail.Core;
using BounderTrail.Data;
using BounderTrail.Levels;
using BounderTrail.Save;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.UI
{
    /// <summary>
    /// Builds a vertical level list from LevelCatalog and starts unlocked levels.
    /// </summary>
    public class LevelSelectView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform buttonRoot;
        [SerializeField] private Button levelButtonTemplate;
        [SerializeField] private Button backButton;

        public event System.Action BackRequested;

        private void OnEnable()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            Rebuild();
        }

        private void OnDisable()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
            }
        }

        public void Rebuild()
        {
            if (buttonRoot == null || levelButtonTemplate == null)
            {
                return;
            }

            ClearGeneratedButtons();

            var catalog = LevelLoader.Instance != null ? LevelLoader.Instance.Catalog : null;
            if (catalog == null || catalog.Count == 0)
            {
                GameLog.Warning("UI", "Level select has no LevelCatalog.");
                return;
            }

            levelButtonTemplate.gameObject.SetActive(false);

            for (var i = 0; i < catalog.Count; i++)
            {
                var data = catalog.GetLevel(i);
                if (data == null)
                {
                    continue;
                }

                CreateLevelButton(i, data);
            }
        }

        private void CreateLevelButton(int index, LevelData data)
        {
            var unlocked = GameProgress.Instance == null || GameProgress.Instance.IsLevelUnlocked(index);
            var completed = GameProgress.Instance != null && GameProgress.Instance.IsLevelCompleted(index);

            var instance = Instantiate(levelButtonTemplate, buttonRoot);
            instance.name = $"LevelButton_{index}";
            instance.gameObject.SetActive(true);
            instance.interactable = unlocked;

            var label = instance.GetComponentInChildren<Text>();
            if (label != null)
            {
                var status = !unlocked ? " (Locked)" : completed ? " (Cleared)" : string.Empty;
                label.text = $"{index + 1}. {data.DisplayName}{status}";
            }

            var capturedIndex = index;
            instance.onClick.RemoveAllListeners();
            instance.onClick.AddListener(() => OnLevelClicked(capturedIndex));

            var rect = instance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(0f, 140f - (index * 58f));
                rect.sizeDelta = new Vector2(Mathf.Max(rect.sizeDelta.x, 520f), Mathf.Max(rect.sizeDelta.y, 52f));
            }

            if (label != null)
            {
                label.fontSize = Mathf.Max(label.fontSize, 28);
            }
        }

        private void OnLevelClicked(int index)
        {
            AudioManager.PlaySfx(SfxId.Ui);

            if (GameProgress.Instance != null && !GameProgress.Instance.IsLevelUnlocked(index))
            {
                return;
            }

            if (GameStateManager.Instance == null)
            {
                GameLog.Error("UI", "GameStateManager missing.");
                return;
            }

            GameStateManager.Instance.StartLevel(index);
        }

        private void OnBackClicked()
        {
            AudioManager.PlaySfx(SfxId.Ui);
            BackRequested?.Invoke();
        }

        private void ClearGeneratedButtons()
        {
            for (var i = buttonRoot.childCount - 1; i >= 0; i--)
            {
                var child = buttonRoot.GetChild(i);
                if (child == null || child.gameObject == levelButtonTemplate.gameObject)
                {
                    continue;
                }

                Destroy(child.gameObject);
            }
        }
    }
}
