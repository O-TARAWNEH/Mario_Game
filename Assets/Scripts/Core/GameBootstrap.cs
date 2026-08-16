// Filename: GameBootstrap.cs
// Folder: Assets/Scripts/Core/
// Purpose: Initial runtime bootstrap entry point for foundation, managers, and save hydration.
// Dependencies: GameLog, ProjectConstants, GameStateManager, LevelLoader, CollectibleCounter, AudioManager, SaveSystem, GameProgress.

using BounderTrail.Audio;
using BounderTrail.Items;
using BounderTrail.Levels;
using BounderTrail.Save;
using BounderTrail.UI;
using BounderTrail.Vfx;
using UnityEngine;

namespace BounderTrail.Core
{
    /// <summary>
    /// Runs once when the Bootstrap scene starts.
    /// Applies foundation settings, ensures managers exist, then enters Boot -> Main Menu.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Foundation Settings")]
        [SerializeField] private int targetFrameRate = ProjectConstants.TargetFrameRate;
        [SerializeField] private bool logBootstrapEvents = true;
        [SerializeField] private bool dontDestroyOnLoad = true;

        [Header("Phase 2 Flow")]
        [SerializeField] private bool autoStartGameLoop = true;

        private static GameBootstrap _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                GameLog.Warning("Bootstrap", "Duplicate GameBootstrap detected. Destroying the new instance.");
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            ApplyFoundationSettings();
            EnsureSaveSystem();
            EnsureGameStateManager();
            EnsureLevelLoader();
            EnsureCollectibleCounter();
            EnsureAudioManager();
            EnsureGameProgress();
            EnsureHitStop();
            EnsureScreenFade();
            SaveSystem.Instance?.ApplyLoadedData();

            if (logBootstrapEvents)
            {
                GameLog.Info("Bootstrap", $"{ProjectConstants.GameTitle} bootstrap Awake complete.");
            }
        }

        private void Start()
        {
            SaveSystem.Instance?.ApplyLoadedData();

            if (logBootstrapEvents)
            {
                GameLog.Info(
                    "Bootstrap",
                    $"Foundation ready. Target FPS={targetFrameRate}, Scene={gameObject.scene.name}.");
            }

            if (autoStartGameLoop && GameStateManager.Instance != null)
            {
                GameStateManager.Instance.InitializeAndBoot();
            }
        }

        private void ApplyFoundationSettings()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
        }

        private void EnsureSaveSystem()
        {
            if (GetComponent<SaveSystem>() == null)
            {
                gameObject.AddComponent<SaveSystem>();
                GameLog.Info("Bootstrap", "SaveSystem added to GameBootstrap object.");
            }
        }

        private void EnsureGameStateManager()
        {
            if (GetComponent<GameStateManager>() == null)
            {
                gameObject.AddComponent<GameStateManager>();
                GameLog.Info("Bootstrap", "GameStateManager added to GameBootstrap object.");
            }
        }

        private void EnsureLevelLoader()
        {
            if (GetComponent<LevelLoader>() == null)
            {
                gameObject.AddComponent<LevelLoader>();
                GameLog.Info("Bootstrap", "LevelLoader added to GameBootstrap object.");
            }
        }

        private void EnsureCollectibleCounter()
        {
            if (GetComponent<CollectibleCounter>() == null)
            {
                gameObject.AddComponent<CollectibleCounter>();
                GameLog.Info("Bootstrap", "CollectibleCounter added to GameBootstrap object.");
            }
        }

        private void EnsureAudioManager()
        {
            if (GetComponent<AudioManager>() == null)
            {
                gameObject.AddComponent<AudioManager>();
                GameLog.Info("Bootstrap", "AudioManager added to GameBootstrap object.");
            }
        }

        private void EnsureGameProgress()
        {
            if (GetComponent<GameProgress>() == null)
            {
                gameObject.AddComponent<GameProgress>();
                GameLog.Info("Bootstrap", "GameProgress added to GameBootstrap object.");
            }
        }

        private void EnsureHitStop()
        {
            if (GetComponent<HitStop>() == null)
            {
                gameObject.AddComponent<HitStop>();
                GameLog.Info("Bootstrap", "HitStop added to GameBootstrap object.");
            }
        }

        private void EnsureScreenFade()
        {
            if (GetComponent<ScreenFade>() == null)
            {
                gameObject.AddComponent<ScreenFade>();
                GameLog.Info("Bootstrap", "ScreenFade added to GameBootstrap object.");
            }
        }
    }
}
