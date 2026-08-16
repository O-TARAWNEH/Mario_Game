// Filename: LevelLoader.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Loads levels from LevelCatalog / LevelData (Phase 7).
// Dependencies: BounderTrail.Data.LevelCatalog, LevelData, BounderTrail.Core.GameLog, GameStateId

using System;
using System.Collections;
using BounderTrail.Core;
using BounderTrail.Data;
using BounderTrail.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BounderTrail.Levels
{
    /// <summary>
    /// Handles level selection and scene loading for the campaign foundation.
    /// </summary>
    public class LevelLoader : MonoBehaviour
    {
        public static LevelLoader Instance { get; private set; }

        [Header("Catalog")]
        [SerializeField] private LevelCatalog catalog;
        [SerializeField] private int startingLevelIndex;

        [Header("Debug")]
        [SerializeField] private bool logLoads = true;

        private bool _isLoading;

        public LevelCatalog Catalog => catalog;
        public int CurrentLevelIndex { get; private set; } = -1;
        public LevelData CurrentLevel { get; private set; }
        public int LevelCount => catalog != null ? catalog.Count : 0;

        public event Action<LevelData> LevelLoadStarted;
        public event Action<LevelData> LevelLoadCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLog.Warning("Level", "Duplicate LevelLoader destroyed.");
                Destroy(this);
                return;
            }

            Instance = this;
            CurrentLevelIndex = Mathf.Max(0, startingLevelIndex);
            if (catalog != null)
            {
                CurrentLevel = catalog.GetLevel(CurrentLevelIndex);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetCatalog(LevelCatalog newCatalog)
        {
            catalog = newCatalog;
        }

        public bool TryGetCurrentSceneName(out string sceneName)
        {
            sceneName = null;
            if (CurrentLevel == null && catalog != null)
            {
                CurrentLevel = catalog.GetLevel(CurrentLevelIndex);
            }

            if (CurrentLevel == null)
            {
                return false;
            }

            sceneName = CurrentLevel.SceneName;
            return !string.IsNullOrWhiteSpace(sceneName);
        }

        public void LoadCurrentLevel()
        {
            LoadLevel(CurrentLevelIndex);
        }

        public void ReloadCurrentLevel()
        {
            if (CurrentLevelIndex < 0)
            {
                CurrentLevelIndex = Mathf.Max(0, startingLevelIndex);
            }

            LoadLevel(CurrentLevelIndex);
        }

        /// <summary>
        /// True when the catalog has another level after the current index.
        /// </summary>
        public bool HasNextLevel
        {
            get
            {
                if (catalog == null)
                {
                    return false;
                }

                var next = CurrentLevelIndex < 0 ? startingLevelIndex + 1 : CurrentLevelIndex + 1;
                return catalog.GetLevel(next) != null;
            }
        }

        /// <summary>
        /// Loads the next catalog level. Returns false if none exists.
        /// </summary>
        public bool TryLoadNextLevel()
        {
            if (!HasNextLevel)
            {
                GameLog.Info("Level", "No next level in catalog.");
                return false;
            }

            if (_isLoading)
            {
                return false;
            }

            var nextIndex = CurrentLevelIndex < 0 ? startingLevelIndex + 1 : CurrentLevelIndex + 1;
            var data = catalog != null ? catalog.GetLevel(nextIndex) : null;
            if (data == null)
            {
                GameLog.Error("Level", $"No LevelData at catalog index {nextIndex}.");
                return false;
            }

            LoadLevel(data, nextIndex);
            return true;
        }

        public void LoadLevel(int index)
        {
            if (catalog == null)
            {
                GameLog.Error("Level", "LevelLoader has no LevelCatalog assigned.");
                NotifyLoadFailed();
                return;
            }

            var data = catalog.GetLevel(index);
            if (data == null)
            {
                GameLog.Error("Level", $"No LevelData at catalog index {index}.");
                NotifyLoadFailed();
                return;
            }

            LoadLevel(data, index);
        }

        public void LoadLevel(LevelData data)
        {
            if (data == null)
            {
                GameLog.Error("Level", "Cannot load null LevelData.");
                NotifyLoadFailed();
                return;
            }

            var index = catalog != null ? catalog.IndexOf(data) : -1;
            LoadLevel(data, index >= 0 ? index : CurrentLevelIndex);
        }

        private void LoadLevel(LevelData data, int index)
        {
            if (_isLoading)
            {
                return;
            }

            StartCoroutine(LoadLevelRoutine(data, index));
        }

        private static void NotifyLoadFailed()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.NotifyLevelLoadFailed();
            }
        }

        private IEnumerator LoadLevelRoutine(LevelData data, int index)
        {
            _isLoading = true;
            CurrentLevel = data;
            CurrentLevelIndex = index;

            if (logLoads)
            {
                GameLog.Info("Level", $"Loading '{data.DisplayName}' scene '{data.SceneName}'...");
            }

            LevelLoadStarted?.Invoke(data);

            Time.timeScale = 1f;
            yield return ScreenFade.FadeOut(0.18f);

            var operation = SceneManager.LoadSceneAsync(data.SceneName);
            if (operation == null)
            {
                GameLog.Error("Level", $"Failed to load scene '{data.SceneName}'. Check Build Settings.");
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.NotifyLevelLoadFailed();
                }

                yield return ScreenFade.FadeIn(0.12f);
                _isLoading = false;
                yield break;
            }

            while (!operation.isDone)
            {
                yield return null;
            }

            if (GameStateManager.Instance != null)
            {
                // Keep lifecycle in sync when levels are loaded through LevelLoader.
                GameStateManager.Instance.NotifyEnteredGameplayFromLevelLoad();
            }

            LevelLoadCompleted?.Invoke(data);
            yield return ScreenFade.FadeIn(0.22f);
            _isLoading = false;

            if (logLoads)
            {
                GameLog.Info("Level", $"Loaded '{data.DisplayName}'.");
            }
        }
    }
}
