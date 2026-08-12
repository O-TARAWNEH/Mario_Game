// Filename: CollectibleCounter.cs
// Folder: Assets/Scripts/Items/
// Purpose: Run-wide coin/score counter and collection event hub (Phase 12).
// Dependencies: CollectiblePickupInfo, BounderTrail.Core.GameLog, GameStateManager, LevelLoader

using System;
using BounderTrail.Core;
using BounderTrail.Data;
using BounderTrail.Levels;
using UnityEngine;

namespace BounderTrail.Items
{
    /// <summary>
    /// Tracks collected coins/score and notifies listeners (HUD, audio, etc.).
    /// Lives on the persistent bootstrap object.
    /// </summary>
    public class CollectibleCounter : MonoBehaviour
    {
        public static CollectibleCounter Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool logCollections = true;
        [SerializeField] private bool autoResetOnNewRun = true;

        public int CoinCount { get; private set; }
        public int Score { get; private set; }

        /// <summary>Raised after counts update for a successful pickup.</summary>
        public event Action<CollectiblePickupInfo> Collected;

        /// <summary>Raised whenever coin/score totals change (including reset).</summary>
        public event Action CountsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLog.Warning("Items", "Duplicate CollectibleCounter destroyed.");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged += OnStateChanged;
            }

            if (LevelLoader.Instance != null)
            {
                LevelLoader.Instance.LevelLoadStarted += OnLevelLoadStarted;
            }
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged -= OnStateChanged;
            }

            if (LevelLoader.Instance != null)
            {
                LevelLoader.Instance.LevelLoadStarted -= OnLevelLoadStarted;
            }
        }

        private void Start()
        {
            // Late subscribe if managers awaken in the same frame.
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged -= OnStateChanged;
                GameStateManager.Instance.StateChanged += OnStateChanged;
            }

            if (LevelLoader.Instance != null)
            {
                LevelLoader.Instance.LevelLoadStarted -= OnLevelLoadStarted;
                LevelLoader.Instance.LevelLoadStarted += OnLevelLoadStarted;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterCollection(CollectiblePickupInfo info)
        {
            if (info.CoinValue > 0)
            {
                CoinCount += info.CoinValue;
            }

            if (info.ScoreValue > 0)
            {
                Score += info.ScoreValue;
            }

            if (logCollections)
            {
                GameLog.Info(
                    "Items",
                    $"Collected {info.Kind} (+{info.CoinValue} coin, +{info.ScoreValue} score) from '{info.SourceName}'. Totals: {CoinCount} coins, {Score} score.");
            }

            Collected?.Invoke(info);
            CountsChanged?.Invoke();
        }

        public void ResetCounts()
        {
            CoinCount = 0;
            Score = 0;
            CountsChanged?.Invoke();
            GameLog.Info("Items", "Collectible counters reset.");
        }

        private void OnLevelLoadStarted(LevelData _)
        {
            if (autoResetOnNewRun)
            {
                ResetCounts();
            }
        }

        private void OnStateChanged(GameStateId previous, GameStateId next)
        {
            if (!autoResetOnNewRun || next != GameStateId.Gameplay)
            {
                return;
            }

            // Fresh run into gameplay (not unpausing).
            if (previous == GameStateId.MainMenu
                || previous == GameStateId.GameOver
                || previous == GameStateId.LevelComplete
                || previous == GameStateId.Boot)
            {
                ResetCounts();
            }
        }
    }
}
