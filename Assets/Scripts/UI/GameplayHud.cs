// Filename: GameplayHud.cs
// Folder: Assets/Scripts/UI/
// Purpose: In-game HUD for lives, health, coins, score, level, power-ups, pause (Phase 17).
// Dependencies: PlayerHealth, PlayerPowerUps, RespawnSystem, CollectibleCounter, LevelLoader, GameStateManager

using BounderTrail.Core;
using BounderTrail.Items;
using BounderTrail.Levels;
using BounderTrail.Player;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.UI
{
    /// <summary>
    /// Design-spec gameplay HUD. No timer (not in design).
    /// Phase 29: power-up/pause refresh avoids needless per-frame string allocations.
    /// </summary>
    public class GameplayHud : MonoBehaviour
    {
        [Header("Readouts")]
        [SerializeField] private Text livesText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text powerUpText;
        [SerializeField] private Text pauseIndicatorText;

        [Header("Formats")]
        [SerializeField] private string livesFormat = "Retries: {0}";
        [SerializeField] private string healthFormat = "HP: {0}/{1}";
        [SerializeField] private string coinsFormat = "Coins: {0}";
        [SerializeField] private string scoreFormat = "Score: {0}";
        [SerializeField] private string levelFormat = "{0}";
        [SerializeField] private string powerUpNone = "Power: —";
        [SerializeField] private string pauseLabel = "PAUSED";

        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerPowerUps playerPowerUps;
        [SerializeField] private HealthHeartsDisplay heartsDisplay;

        private bool _powerUpTickActive;
        private bool _cachedHasShield;
        private bool _cachedHasSpeed;
        private int _cachedShieldTenths = int.MinValue;
        private int _cachedSpeedTenths = int.MinValue;
        private bool _pauseShown;

        private void OnEnable()
        {
            ResolvePlayer();
            Bind();
            RefreshAll();
        }

        private void Start()
        {
            ResolvePlayer();
            Bind();
            RefreshAll();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Update()
        {
            // Timers change without events; only tick while a timed power-up is active.
            if (_powerUpTickActive)
            {
                RefreshPowerUps();
            }
        }

        public void RefreshAll()
        {
            RefreshLives();
            RefreshHealth();
            RefreshCollectibles();
            RefreshLevel();
            RefreshPowerUps();
            RefreshPauseIndicator();
        }

        private void Bind()
        {
            Unbind();

            if (CollectibleCounter.Instance != null)
            {
                CollectibleCounter.Instance.CountsChanged += RefreshCollectibles;
            }

            if (RespawnSystem.Instance != null)
            {
                RespawnSystem.Instance.LivesChanged += OnLivesChanged;
                RespawnSystem.Instance.Respawned += RefreshAll;
            }

            if (playerHealth != null)
            {
                playerHealth.HealthChanged += RefreshHealth;
                playerHealth.Damaged += OnDamaged;
            }

            if (playerPowerUps != null)
            {
                playerPowerUps.StateChanged += RefreshPowerUps;
                playerPowerUps.Activated += OnPowerUpChanged;
                playerPowerUps.Expired += OnPowerUpChanged;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged += OnStateChanged;
            }

            if (LevelLoader.Instance != null)
            {
                LevelLoader.Instance.LevelLoadCompleted += OnLevelLoaded;
            }
        }

        private void Unbind()
        {
            if (CollectibleCounter.Instance != null)
            {
                CollectibleCounter.Instance.CountsChanged -= RefreshCollectibles;
            }

            if (RespawnSystem.Instance != null)
            {
                RespawnSystem.Instance.LivesChanged -= OnLivesChanged;
                RespawnSystem.Instance.Respawned -= RefreshAll;
            }

            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= RefreshHealth;
                playerHealth.Damaged -= OnDamaged;
            }

            if (playerPowerUps != null)
            {
                playerPowerUps.StateChanged -= RefreshPowerUps;
                playerPowerUps.Activated -= OnPowerUpChanged;
                playerPowerUps.Expired -= OnPowerUpChanged;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged -= OnStateChanged;
            }

            if (LevelLoader.Instance != null)
            {
                LevelLoader.Instance.LevelLoadCompleted -= OnLevelLoaded;
            }
        }

        private void ResolvePlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                if (playerHealth == null)
                {
                    playerHealth = player.GetComponent<PlayerHealth>();
                }

                if (playerPowerUps == null)
                {
                    playerPowerUps = player.GetComponent<PlayerPowerUps>();
                }
            }

            EnsureHeartsDisplay();
        }

        private void EnsureHeartsDisplay()
        {
            if (heartsDisplay == null)
            {
                heartsDisplay = GetComponent<HealthHeartsDisplay>();
            }

            if (heartsDisplay == null)
            {
                heartsDisplay = gameObject.AddComponent<HealthHeartsDisplay>();
            }
        }

        private void RefreshLives()
        {
            if (livesText == null)
            {
                return;
            }

            var lives = RespawnSystem.Instance != null ? RespawnSystem.Instance.LivesRemaining : 0;
            livesText.text = string.Format(livesFormat, lives);
        }

        private void RefreshHealth()
        {
            if (heartsDisplay != null)
            {
                heartsDisplay.Refresh();
            }

            if (healthText == null)
            {
                return;
            }

            // Hearts are the primary health readout when the display is present.
            if (heartsDisplay != null)
            {
                healthText.gameObject.SetActive(false);
                return;
            }

            healthText.gameObject.SetActive(true);
            if (playerHealth == null)
            {
                healthText.text = string.Format(healthFormat, 0, 0);
                return;
            }

            healthText.text = string.Format(healthFormat, playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void RefreshCollectibles()
        {
            var coins = CollectibleCounter.Instance != null ? CollectibleCounter.Instance.CoinCount : 0;
            var score = CollectibleCounter.Instance != null ? CollectibleCounter.Instance.Score : 0;

            if (coinsText != null)
            {
                coinsText.text = string.Format(coinsFormat, coins);
            }

            if (scoreText != null)
            {
                scoreText.text = string.Format(scoreFormat, score);
            }
        }

        private void RefreshLevel()
        {
            if (levelText == null)
            {
                return;
            }

            if (LevelLoader.Instance != null && LevelLoader.Instance.CurrentLevel != null)
            {
                var data = LevelLoader.Instance.CurrentLevel;
                var number = LevelLoader.Instance.CurrentLevelIndex + 1;
                levelText.text = string.Format(levelFormat, $"Level {number}: {data.DisplayName}");
                return;
            }

            var root = FindAnyObjectByType<LevelRoot>();
            if (root != null)
            {
                levelText.text = string.Format(levelFormat, root.DisplayName);
                return;
            }

            levelText.text = string.Format(levelFormat, "Level");
        }

        private void RefreshPowerUps()
        {
            if (powerUpText == null)
            {
                return;
            }

            if (playerPowerUps == null)
            {
                _powerUpTickActive = false;
                if (_cachedHasShield || _cachedHasSpeed || _cachedShieldTenths != -1)
                {
                    powerUpText.text = powerUpNone;
                    _cachedHasShield = false;
                    _cachedHasSpeed = false;
                    _cachedShieldTenths = -1;
                    _cachedSpeedTenths = -1;
                }

                return;
            }

            var hasShield = playerPowerUps.HasGlowShield;
            var hasSpeed = playerPowerUps.HasSpeedBurst;
            _powerUpTickActive = hasShield || hasSpeed;

            if (!hasShield && !hasSpeed)
            {
                if (_cachedHasShield || _cachedHasSpeed)
                {
                    powerUpText.text = powerUpNone;
                    _cachedHasShield = false;
                    _cachedHasSpeed = false;
                    _cachedShieldTenths = -1;
                    _cachedSpeedTenths = -1;
                }

                return;
            }

            // Update at 0.1s resolution so we don't rebuild the string every frame.
            var shieldTenths = hasShield ? Mathf.CeilToInt(playerPowerUps.GlowShieldRemaining * 10f) : -1;
            var speedTenths = hasSpeed ? Mathf.CeilToInt(playerPowerUps.SpeedBurstRemaining * 10f) : -1;
            if (hasShield == _cachedHasShield
                && hasSpeed == _cachedHasSpeed
                && shieldTenths == _cachedShieldTenths
                && speedTenths == _cachedSpeedTenths)
            {
                return;
            }

            _cachedHasShield = hasShield;
            _cachedHasSpeed = hasSpeed;
            _cachedShieldTenths = shieldTenths;
            _cachedSpeedTenths = speedTenths;

            if (hasShield && hasSpeed)
            {
                powerUpText.text =
                    $"Power: Shield {playerPowerUps.GlowShieldRemaining:0.0}s | Speed {playerPowerUps.SpeedBurstRemaining:0.0}s";
            }
            else if (hasShield)
            {
                powerUpText.text = $"Power: Shield {playerPowerUps.GlowShieldRemaining:0.0}s";
            }
            else
            {
                powerUpText.text = $"Power: Speed {playerPowerUps.SpeedBurstRemaining:0.0}s";
            }
        }

        private void RefreshPauseIndicator()
        {
            if (pauseIndicatorText == null)
            {
                return;
            }

            var paused = GameStateManager.Instance != null
                         && GameStateManager.Instance.CurrentState == GameStateId.Pause;
            if (paused == _pauseShown && (!paused || pauseIndicatorText.text == pauseLabel))
            {
                return;
            }

            _pauseShown = paused;
            pauseIndicatorText.gameObject.SetActive(paused);
            if (paused)
            {
                pauseIndicatorText.text = pauseLabel;
            }
        }

        private void OnLivesChanged(int _)
        {
            RefreshLives();
        }

        private void OnDamaged(int _, int __)
        {
            RefreshHealth();
        }

        private void OnPowerUpChanged(PowerUpKind _)
        {
            RefreshPowerUps();
        }

        private void OnStateChanged(GameStateId _, GameStateId __)
        {
            RefreshPauseIndicator();
        }

        private void OnLevelLoaded(Data.LevelData _)
        {
            ResolvePlayer();
            Bind();
            RefreshAll();
        }
    }
}
