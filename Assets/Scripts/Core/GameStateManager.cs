// Filename: GameStateManager.cs
// Folder: Assets/Scripts/Core/
// Purpose: Core game lifecycle / state machine and scene transitions (Phase 2).
// Dependencies: GameLog, ProjectConstants, GameStateId.
//
// Attach to the persistent GameBootstrap object (or any DontDestroyOnLoad systems object).

using System;
using System.Collections;
using BounderTrail.Items;
using BounderTrail.Levels;
using BounderTrail.Save;
using BounderTrail.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BounderTrail.Core
{
    /// <summary>
    /// Owns the game flow:
    /// Boot -> Main Menu -> Gameplay -> Pause / Game Over / Level Complete.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = ProjectConstants.MainMenuSceneName;
        [SerializeField] private string gameplaySceneName = ProjectConstants.GameplaySceneName;

        [Header("Debug")]
        [SerializeField] private bool logStateChanges = true;

        private bool _isTransitioning;

        public GameStateId CurrentState { get; private set; } = GameStateId.Boot;

        /// <summary>Raised after a state change (previous, next).</summary>
        public event Action<GameStateId, GameStateId> StateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLog.Warning("GameState", "Duplicate GameStateManager destroyed.");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Called by GameBootstrap after foundation init.
        /// </summary>
        public void InitializeAndBoot()
        {
            SetState(GameStateId.Boot);
            GoToMainMenu();
        }

        public void GoToMainMenu()
        {
            if (_isTransitioning)
            {
                return;
            }

            StartCoroutine(LoadSceneAndSetState(mainMenuSceneName, GameStateId.MainMenu, resetTimeScale: true));
        }

        public void StartGameplay()
        {
            if (_isTransitioning)
            {
                return;
            }

            if (LevelLoader.Instance != null)
            {
                _isTransitioning = true;
                LevelLoader.Instance.LoadCurrentLevel();
                return;
            }

            StartCoroutine(LoadSceneAndSetState(gameplaySceneName, GameStateId.Gameplay, resetTimeScale: true));
        }

        /// <summary>
        /// Starts a new campaign from the first catalog level.
        /// </summary>
        public void StartNewGame()
        {
            if (_isTransitioning)
            {
                return;
            }

            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.StartNewGame();
            }

            CollectibleCounter.Instance?.ResetCounts();

            if (LevelLoader.Instance != null)
            {
                _isTransitioning = true;
                LevelLoader.Instance.LoadLevel(0);
                return;
            }

            StartGameplay();
        }

        /// <summary>
        /// Continues from the saved campaign level index.
        /// </summary>
        public void ContinueGame()
        {
            if (_isTransitioning)
            {
                return;
            }

            var index = GameProgress.Instance != null ? GameProgress.Instance.ContinueLevelIndex : 0;
            StartLevel(index);
        }

        /// <summary>
        /// Starts a specific catalog level if unlocked.
        /// </summary>
        public void StartLevel(int levelIndex)
        {
            if (_isTransitioning)
            {
                return;
            }

            if (GameProgress.Instance != null && !GameProgress.Instance.IsLevelUnlocked(levelIndex))
            {
                GameLog.Warning("GameState", $"Level {levelIndex} is locked.");
                return;
            }

            if (LevelLoader.Instance == null)
            {
                StartGameplay();
                return;
            }

            if (LevelLoader.Instance.Catalog == null || !LevelLoader.Instance.Catalog.IsValidIndex(levelIndex))
            {
                GameLog.Error("GameState", $"Invalid campaign level index {levelIndex}.");
                return;
            }

            _isTransitioning = true;
            LevelLoader.Instance.LoadLevel(levelIndex);
        }

        public void RestartGameplay()
        {
            if (_isTransitioning)
            {
                return;
            }

            // Fresh attempt — clear run coins so restarts stay fair and frustration-free.
            CollectibleCounter.Instance?.ResetCounts();

            if (LevelLoader.Instance != null)
            {
                _isTransitioning = true;
                LevelLoader.Instance.ReloadCurrentLevel();
                return;
            }

            StartCoroutine(LoadSceneAndSetState(gameplaySceneName, GameStateId.Gameplay, resetTimeScale: true));
        }

        public void PauseGame()
        {
            if (CurrentState != GameStateId.Gameplay || _isTransitioning)
            {
                return;
            }

            Time.timeScale = 0f;
            SetState(GameStateId.Pause);
        }

        public void ResumeGame()
        {
            if (CurrentState != GameStateId.Pause || _isTransitioning)
            {
                return;
            }

            Time.timeScale = 1f;
            SetState(GameStateId.Gameplay);
        }

        public void TogglePause()
        {
            if (CurrentState == GameStateId.Gameplay)
            {
                PauseGame();
            }
            else if (CurrentState == GameStateId.Pause)
            {
                ResumeGame();
            }
        }

        public void TriggerGameOver()
        {
            if (CurrentState != GameStateId.Gameplay && CurrentState != GameStateId.Pause)
            {
                return;
            }

            Time.timeScale = 0f;
            SetState(GameStateId.GameOver);
        }

        public void TriggerLevelComplete()
        {
            if (CurrentState != GameStateId.Gameplay && CurrentState != GameStateId.Pause)
            {
                return;
            }

            if (LevelLoader.Instance != null && GameProgress.Instance != null)
            {
                GameProgress.Instance.RegisterLevelCompleted(LevelLoader.Instance.CurrentLevelIndex);
            }

            Time.timeScale = 0f;
            SetState(GameStateId.LevelComplete);
        }

        /// <summary>
        /// Continues to the next catalog level, or returns to Main Menu if none remain.
        /// </summary>
        public void ProceedToNextLevel()
        {
            if (CurrentState != GameStateId.LevelComplete && CurrentState != GameStateId.Gameplay)
            {
                return;
            }

            if (_isTransitioning)
            {
                return;
            }

            Time.timeScale = 1f;

            if (LevelLoader.Instance != null && LevelLoader.Instance.HasNextLevel)
            {
                _isTransitioning = true;
                if (!LevelLoader.Instance.TryLoadNextLevel())
                {
                    _isTransitioning = false;
                    GoToMainMenu();
                }

                return;
            }

            GameLog.Info("GameState", "Campaign finished (no next level). Returning to Main Menu.");
            GoToMainMenu();
        }

        /// <summary>
        /// Called by LevelLoader after a level scene finishes loading.
        /// </summary>
        public void NotifyEnteredGameplayFromLevelLoad()
        {
            Time.timeScale = 1f;
            SetState(GameStateId.Gameplay);
            _isTransitioning = false;

            if (LevelLoader.Instance != null && GameProgress.Instance != null)
            {
                GameProgress.Instance.SetContinueLevel(LevelLoader.Instance.CurrentLevelIndex);
            }

            if (LevelCompletionService.Instance != null)
            {
                LevelCompletionService.Instance.ResetCompletionState();
            }
        }

        /// <summary>
        /// Called by LevelLoader if a level scene fails to load.
        /// </summary>
        public void NotifyLevelLoadFailed()
        {
            _isTransitioning = false;
        }

        private IEnumerator LoadSceneAndSetState(string sceneName, GameStateId state, bool resetTimeScale)
        {
            _isTransitioning = true;

            if (resetTimeScale)
            {
                Time.timeScale = 1f;
            }

            if (logStateChanges)
            {
                GameLog.Info("GameState", $"Loading scene '{sceneName}' for state {state}...");
            }

            yield return ScreenFade.FadeOut(0.18f);

            var operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                GameLog.Error("GameState", $"Failed to load scene '{sceneName}'. Is it in Build Settings?");
                yield return ScreenFade.FadeIn(0.12f);
                _isTransitioning = false;
                yield break;
            }

            while (!operation.isDone)
            {
                yield return null;
            }

            SetState(state);
            yield return ScreenFade.FadeIn(0.22f);
            _isTransitioning = false;
        }

        private void SetState(GameStateId nextState)
        {
            var previous = CurrentState;
            CurrentState = nextState;

            if (logStateChanges)
            {
                GameLog.Info("GameState", $"State: {previous} -> {nextState}");
            }

            StateChanged?.Invoke(previous, nextState);
        }
    }
}
