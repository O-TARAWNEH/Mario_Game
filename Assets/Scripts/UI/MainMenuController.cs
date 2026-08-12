// Filename: MainMenuController.cs
// Folder: Assets/Scripts/UI/
// Purpose: Main-menu flow with Start, Continue, Level Select, Settings, Controls (Phase 19/20).
// Dependencies: GameStateManager, GameProgress, AudioManager, LevelSelectView, GameLog

using BounderTrail.Audio;
using BounderTrail.Core;
using BounderTrail.Save;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.UI
{
    /// <summary>
    /// Owns Main Menu root navigation and Settings / Controls / Level Select panels.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private GameObject levelSelectPanel;

        [Header("Root Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button quitButton;

        [Header("Sub-panel Buttons")]
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Button controlsBackButton;
        [SerializeField] private Button resetSaveButton;

        [Header("Views")]
        [SerializeField] private AudioSettingsView audioSettingsView;
        [SerializeField] private LevelSelectView levelSelectView;

        private void OnEnable()
        {
            Bind();
            ShowRoot();
            RefreshContinueButton();
            audioSettingsView?.RefreshFromAudioManager();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Bind()
        {
            Add(startButton, OnStartClicked);
            Add(continueButton, OnContinueClicked);
            Add(levelSelectButton, OnLevelSelectClicked);
            Add(settingsButton, OnSettingsClicked);
            Add(controlsButton, OnControlsClicked);
            Add(quitButton, OnQuitClicked);
            Add(settingsBackButton, ShowRoot);
            Add(controlsBackButton, ShowRoot);
            Add(resetSaveButton, OnResetSaveClicked);

            if (levelSelectView != null)
            {
                levelSelectView.BackRequested += ShowRoot;
            }
        }

        private void Unbind()
        {
            Remove(startButton);
            Remove(continueButton);
            Remove(levelSelectButton);
            Remove(settingsButton);
            Remove(controlsButton);
            Remove(quitButton);
            Remove(settingsBackButton);
            Remove(controlsBackButton);
            Remove(resetSaveButton);

            if (levelSelectView != null)
            {
                levelSelectView.BackRequested -= ShowRoot;
            }
        }

        private void OnStartClicked()
        {
            if (GameStateManager.Instance == null)
            {
                GameLog.Error("UI", "GameStateManager missing. Start from Bootstrap scene.");
                return;
            }

            GameStateManager.Instance.StartNewGame();
        }

        private void OnContinueClicked()
        {
            var canContinue = SaveSystem.Instance != null
                ? SaveSystem.Instance.HasCampaignSave
                : GameProgress.Instance != null && GameProgress.Instance.CanContinue;
            if (!canContinue)
            {
                return;
            }

            if (GameStateManager.Instance == null)
            {
                GameLog.Error("UI", "GameStateManager missing. Start from Bootstrap scene.");
                return;
            }

            GameStateManager.Instance.ContinueGame();
        }

        private void OnLevelSelectClicked()
        {
            SetPanel(levelSelectPanel);
            levelSelectView?.Rebuild();
        }

        private void OnSettingsClicked()
        {
            // Phase 34 — single Options panel (audio + controls copy).
            SetPanel(settingsPanel);
            audioSettingsView?.RefreshFromAudioManager();
        }

        private void OnControlsClicked()
        {
            OnSettingsClicked();
        }

        private void OnResetSaveClicked()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.ResetSave();
            }
            else if (GameProgress.Instance != null)
            {
                GameProgress.Instance.StartNewGame();
            }

            RefreshContinueButton();
            levelSelectView?.Rebuild();
            audioSettingsView?.RefreshFromAudioManager();
            GameLog.Info("UI", "Save reset requested from Settings.");
        }

        private void OnQuitClicked()
        {
            // Flush latest runtime state before quitting.
            SaveSystem.Instance?.Save();
            GameLog.Info("UI", "Quit requested from Main Menu.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowRoot()
        {
            SetPanel(rootPanel);
            RefreshContinueButton();
        }

        private void RefreshContinueButton()
        {
            if (continueButton == null)
            {
                return;
            }

            var canContinue = SaveSystem.Instance != null
                ? SaveSystem.Instance.HasCampaignSave
                : GameProgress.Instance != null && GameProgress.Instance.CanContinue;
            continueButton.interactable = canContinue;
            continueButton.gameObject.SetActive(true);
        }

        private void SetPanel(GameObject active)
        {
            SetActive(rootPanel, active == rootPanel);
            SetActive(settingsPanel, active == settingsPanel);
            SetActive(controlsPanel, active == controlsPanel);
            SetActive(levelSelectPanel, active == levelSelectPanel);
        }

        private static void SetActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        private static void Add(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.AddListener(() =>
            {
                AudioManager.PlaySfx(SfxId.Ui);
                action?.Invoke();
            });
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
