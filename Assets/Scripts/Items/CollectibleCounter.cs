// Filename: CollectibleCounter.cs
// Folder: Assets/Scripts/Items/
// Purpose: Run-wide coin/score counter and collection event hub (Phase 12/42).
// Dependencies: CollectiblePickupInfo, BounderTrail.Core.GameLog, GameStateManager, RespawnSystem

using System;
using BounderTrail.Core;
using BounderTrail.Levels;
using UnityEngine;

namespace BounderTrail.Items
{
    /// <summary>
    /// Tracks collected coins/score and notifies listeners (HUD, audio, etc.).
    /// Lives on the persistent bootstrap object.
    /// Coins persist across levels within a run; reset on new game / restart / game over / menu.
    /// </summary>
    public class CollectibleCounter : MonoBehaviour
    {
        public static CollectibleCounter Instance { get; private set; }

        [Header("Rewards")]
        [SerializeField] private int coinsPerBonusLife = 25;
        [SerializeField] private bool grantBonusLives = true;

        [Header("Debug")]
        [SerializeField] private bool logCollections = true;
        [SerializeField] private bool autoResetOnNewRun = true;

        private int _coinsTowardBonusLife;

        public int CoinCount { get; private set; }
        public int Score { get; private set; }
        public int BonusLivesEarnedThisRun { get; private set; }
        public int CoinsPerBonusLife => Mathf.Max(1, coinsPerBonusLife);
        public int CoinsUntilBonusLife =>
            grantBonusLives ? Mathf.Max(0, CoinsPerBonusLife - _coinsTowardBonusLife) : 0;

        /// <summary>Raised after counts update for a successful pickup.</summary>
        public event Action<CollectiblePickupInfo> Collected;

        /// <summary>Raised whenever coin/score totals change (including reset).</summary>
        public event Action CountsChanged;

        /// <summary>Raised when a coin milestone grants an extra life.</summary>
        public event Action BonusLifeEarned;

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
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged -= OnStateChanged;
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
                TryAwardBonusLives(info.CoinValue);
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
            BonusLivesEarnedThisRun = 0;
            _coinsTowardBonusLife = 0;
            CountsChanged?.Invoke();
            GameLog.Info("Items", "Collectible counters reset.");
        }

        private void TryAwardBonusLives(int coinsAdded)
        {
            if (!grantBonusLives || coinsAdded <= 0 || coinsPerBonusLife <= 0)
            {
                return;
            }

            _coinsTowardBonusLife += coinsAdded;
            while (_coinsTowardBonusLife >= coinsPerBonusLife)
            {
                _coinsTowardBonusLife -= coinsPerBonusLife;
                BonusLivesEarnedThisRun++;

                if (RespawnSystem.Instance != null)
                {
                    RespawnSystem.Instance.GrantBonusLife();
                }

                BonusLifeEarned?.Invoke();
                GameLog.Info("Items", $"Bonus life earned ({CoinCount} coins).");
            }
        }

        private void OnStateChanged(GameStateId previous, GameStateId next)
        {
            if (!autoResetOnNewRun)
            {
                return;
            }

            // Wipe run totals when returning to menu / boot.
            // Keep totals on Game Over / Level Complete so summary screens stay accurate.
            // RestartGameplay / StartNewGame call ResetCounts explicitly.
            if (next == GameStateId.MainMenu || next == GameStateId.Boot)
            {
                ResetCounts();
            }
        }
    }
}
