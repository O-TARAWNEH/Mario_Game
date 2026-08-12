// Filename: GameplayFlowController.cs
// Folder: Assets/Scripts/UI/
// Purpose: Gameplay HUD + Pause / Settings / Game Over / Level Complete menus (Phase 2/16/17/19).
// Dependencies: BounderTrail.Core.GameStateManager, GameStateId, LevelLoader, AudioManager, GameLog

using BounderTrail.Audio;
using BounderTrail.Core;
using BounderTrail.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.UI
{
    /// <summary>
    /// Shows HUD during play/pause and routes overlay menu buttons.
    /// </summary>
    public class GameplayFlowController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject pauseSettingsPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private GameObject gameplayHudRoot;

        [Header("Pause Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseSettingsButton;
        [SerializeField] private Button pauseMainMenuButton;
        [SerializeField] private Button pauseSettingsBackButton;

        [Header("Game Over Buttons")]
        [SerializeField] private Button gameOverRestartButton;
        [SerializeField] private Button gameOverMainMenuButton;

        [Header("Level Complete Buttons")]
        [SerializeField] private Button levelCompleteNextButton;
        [SerializeField] private Button levelCompleteRestartButton;
        [SerializeField] private Button levelCompleteMainMenuButton;
        [SerializeField] private Text levelCompleteNextButtonLabel;
        [SerializeField] private Text levelCompleteTitleText;
        [SerializeField] private string levelCompleteTitle = "LEVEL COMPLETE";
        [SerializeField] private string campaignCompleteTitle = "CAMPAIGN COMPLETE";

        [Header("Audio")]
        [SerializeField] private AudioSettingsView pauseAudioSettingsView;

        [Header("Input")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        [Header("Debug")]
        [SerializeField] private bool enableDebugShortcuts;

        private bool _pauseSettingsOpen;

        private void OnEnable()
        {
            BindButtons();

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged += OnStateChanged;
                RefreshPanels(GameStateManager.Instance.CurrentState);
            }
            else
            {
                GameLog.Warning("UI", "GameplayFlowController enabled without GameStateManager.");
                RefreshPanels(GameStateId.Gameplay);
            }
        }

        private void OnDisable()
        {
            UnbindButtons();

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged -= OnStateChanged;
            }
        }

        private void Update()
        {
            if (GameStateManager.Instance == null)
            {
                return;
            }

            if (Input.GetKeyDown(pauseKey))
            {
                HandlePauseKey();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!enableDebugShortcuts || GameStateManager.Instance.CurrentState != GameStateId.Gameplay)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                GameStateManager.Instance.TriggerGameOver();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                if (LevelCompletionService.Instance != null)
                {
                    LevelCompletionService.Instance.NotifyGoalReached(this);
                }
                else
                {
                    GameStateManager.Instance.TriggerLevelComplete();
                }
            }
#endif
        }

        private void HandlePauseKey()
        {
            var state = GameStateManager.Instance.CurrentState;
            if (state == GameStateId.Pause && _pauseSettingsOpen)
            {
                ClosePauseSettings();
                return;
            }

            GameStateManager.Instance.TogglePause();
        }

        private void OnStateChanged(GameStateId previous, GameStateId next)
        {
            if (next != GameStateId.Pause)
            {
                _pauseSettingsOpen = false;
            }

            RefreshPanels(next);
        }

        private void RefreshPanels(GameStateId state)
        {
            var showPause = state == GameStateId.Pause;
            SetActive(pausePanel, showPause && !_pauseSettingsOpen);
            SetActive(pauseSettingsPanel, showPause && _pauseSettingsOpen);
            SetActive(gameOverPanel, state == GameStateId.GameOver);
            SetActive(levelCompletePanel, state == GameStateId.LevelComplete);

            var showHud = state == GameStateId.Gameplay || state == GameStateId.Pause;
            SetActive(gameplayHudRoot, showHud);

            if (showHud)
            {
                var hud = gameplayHudRoot != null ? gameplayHudRoot.GetComponent<GameplayHud>() : null;
                hud?.RefreshAll();
            }

            if (state == GameStateId.LevelComplete)
            {
                var campaignDone = LevelLoader.Instance == null || !LevelLoader.Instance.HasNextLevel;
                RefreshLevelCompleteTitle(campaignDone);
                RefreshNextLevelButton();
                RefreshSummaries(levelCompletePanel, campaignDone);
            }
            else if (state == GameStateId.GameOver)
            {
                RefreshSummaries(gameOverPanel, false);
            }
        }

        private void OpenPauseSettings()
        {
            _pauseSettingsOpen = true;
            pauseAudioSettingsView?.RefreshFromAudioManager();
            RefreshPanels(GameStateId.Pause);
        }

        private void ClosePauseSettings()
        {
            _pauseSettingsOpen = false;
            RefreshPanels(GameStateId.Pause);
        }

        private void RefreshLevelCompleteTitle(bool campaignDone)
        {
            ResolveLevelCompleteTitle();
            if (levelCompleteTitleText == null)
            {
                return;
            }

            levelCompleteTitleText.text = campaignDone ? campaignCompleteTitle : levelCompleteTitle;
        }

        private void ResolveLevelCompleteTitle()
        {
            if (levelCompleteTitleText != null || levelCompletePanel == null)
            {
                return;
            }

            var texts = levelCompletePanel.GetComponentsInChildren<Text>(true);
            for (var i = 0; i < texts.Length; i++)
            {
                var value = texts[i].text;
                if (value == levelCompleteTitle
                    || value == campaignCompleteTitle
                    || value.IndexOf("LEVEL COMPLETE", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || value.IndexOf("CAMPAIGN COMPLETE", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    levelCompleteTitleText = texts[i];
                    return;
                }
            }
        }

        private void RefreshNextLevelButton()
        {
            var hasNext = LevelLoader.Instance != null && LevelLoader.Instance.HasNextLevel;
            var label = hasNext ? "Next Level" : "Finish";

            if (levelCompleteNextButtonLabel != null)
            {
                levelCompleteNextButtonLabel.text = label;
            }
            else if (levelCompleteNextButton != null)
            {
                var text = levelCompleteNextButton.GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.text = label;
                }
            }
        }

        private static void RefreshSummaries(GameObject panel, bool campaignComplete)
        {
            if (panel == null)
            {
                return;
            }

            var summaries = panel.GetComponentsInChildren<FlowScreenSummary>(true);
            for (var i = 0; i < summaries.Length; i++)
            {
                summaries[i].Refresh(campaignComplete);
            }
        }

        private static void SetActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        private void BindButtons()
        {
            Add(resumeButton, () =>
            {
                _pauseSettingsOpen = false;
                GameStateManager.Instance?.ResumeGame();
            });
            Add(pauseRestartButton, () => GameStateManager.Instance?.RestartGameplay());
            Add(pauseSettingsButton, OpenPauseSettings);
            Add(pauseSettingsBackButton, ClosePauseSettings);
            Add(pauseMainMenuButton, () => GameStateManager.Instance?.GoToMainMenu());

            Add(gameOverRestartButton, () => GameStateManager.Instance?.RestartGameplay());
            Add(gameOverMainMenuButton, () => GameStateManager.Instance?.GoToMainMenu());

            Add(levelCompleteNextButton, () => GameStateManager.Instance?.ProceedToNextLevel());
            Add(levelCompleteRestartButton, () => GameStateManager.Instance?.RestartGameplay());
            Add(levelCompleteMainMenuButton, () => GameStateManager.Instance?.GoToMainMenu());
        }

        private void UnbindButtons()
        {
            Remove(resumeButton);
            Remove(pauseRestartButton);
            Remove(pauseSettingsButton);
            Remove(pauseSettingsBackButton);
            Remove(pauseMainMenuButton);
            Remove(gameOverRestartButton);
            Remove(gameOverMainMenuButton);
            Remove(levelCompleteNextButton);
            Remove(levelCompleteRestartButton);
            Remove(levelCompleteMainMenuButton);
        }

        private static void Add(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    AudioManager.PlaySfx(SfxId.Ui);
                    action?.Invoke();
                });
            }
        }

        private static void Remove(Button button)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}
