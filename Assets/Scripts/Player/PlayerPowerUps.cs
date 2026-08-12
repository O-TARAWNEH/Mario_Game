// Filename: PlayerPowerUps.cs
// Folder: Assets/Scripts/Player/
// Purpose: Power-up activation, duration, removal, and state for Pip (Phase 13).
// Dependencies: PlayerController, PlayerHealth, PlayerDeath, BounderTrail.Items.PowerUpKind, GameLog

using System;
using BounderTrail.Core;
using BounderTrail.Items;
using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Owns active power-up state for the player.
    /// Supports only design-spec power-ups: Speed Burst, Glow Shield, Heart Drop.
    /// </summary>
    public class PlayerPowerUps : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerDeath playerDeath;

        [Header("Speed Burst")]
        [SerializeField] private float speedBurstDuration = 5f;
        [SerializeField] private float speedBurstMultiplier = 1.45f;

        [Header("Glow Shield")]
        [SerializeField] private float glowShieldDuration = 6.5f;

        [Header("Heart Drop")]
        [SerializeField] private int heartHealAmount = 1;

        private float _speedBurstRemaining;
        private float _glowShieldRemaining;
        private bool _speedBurstActive;
        private bool _glowShieldActive;

        public bool CanAcceptPowerUps => playerDeath == null || playerDeath.IsAlive;
        public bool HasSpeedBurst => _speedBurstActive;
        public bool HasGlowShield => _glowShieldActive;
        public float SpeedBurstRemaining => Mathf.Max(0f, _speedBurstRemaining);
        public float GlowShieldRemaining => Mathf.Max(0f, _glowShieldRemaining);

        public event Action<PowerUpKind> Activated;
        public event Action<PowerUpKind> Expired;
        public event Action StateChanged;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }

            if (playerDeath == null)
            {
                playerDeath = GetComponent<PlayerDeath>();
            }
        }

        private void OnEnable()
        {
            if (playerDeath != null)
            {
                playerDeath.Died += OnPlayerDied;
            }
        }

        private void OnDisable()
        {
            if (playerDeath != null)
            {
                playerDeath.Died -= OnPlayerDied;
            }
        }

        private void Update()
        {
            // Early-out when idle (do not disable this component — OnDisable unbinds death cleanup).
            if (!_speedBurstActive && !_glowShieldActive)
            {
                return;
            }

            TickTimedPowerUp(ref _speedBurstActive, ref _speedBurstRemaining, PowerUpKind.SpeedBurst, ClearSpeedBurst);
            TickTimedPowerUp(ref _glowShieldActive, ref _glowShieldRemaining, PowerUpKind.GlowShield, ClearGlowShield);
        }

        /// <summary>
        /// Activates a design-spec power-up. Re-picking a timed effect refreshes its duration.
        /// </summary>
        public bool TryActivate(PowerUpKind kind)
        {
            if (!CanAcceptPowerUps)
            {
                return false;
            }

            switch (kind)
            {
                case PowerUpKind.SpeedBurst:
                    ActivateSpeedBurst();
                    break;
                case PowerUpKind.GlowShield:
                    ActivateGlowShield();
                    break;
                case PowerUpKind.HeartDrop:
                    if (!ActivateHeartDrop())
                    {
                        return false;
                    }

                    break;
                default:
                    GameLog.Warning("Player", $"Unsupported power-up kind: {kind}");
                    return false;
            }

            Activated?.Invoke(kind);
            StateChanged?.Invoke();
            return true;
        }

        public void ClearAllPowerUps()
        {
            var hadSpeed = _speedBurstActive;
            var hadShield = _glowShieldActive;

            ClearSpeedBurst();
            ClearGlowShield();

            if (hadSpeed || hadShield)
            {
                StateChanged?.Invoke();
            }
        }

        private void ActivateSpeedBurst()
        {
            _speedBurstActive = true;
            _speedBurstRemaining = speedBurstDuration;
            if (playerController != null)
            {
                playerController.SetSpeedMultiplier(speedBurstMultiplier);
            }

            GameLog.Info("Player", $"Speed Burst active ({speedBurstDuration:0.##}s, x{speedBurstMultiplier:0.##}).");
        }

        private void ActivateGlowShield()
        {
            _glowShieldActive = true;
            _glowShieldRemaining = glowShieldDuration;
            if (playerHealth != null)
            {
                playerHealth.SetGlowShield(true);
            }

            GameLog.Info("Player", $"Glow Shield active ({glowShieldDuration:0.##}s).");
        }

        private bool ActivateHeartDrop()
        {
            if (playerHealth == null || !playerHealth.TryHeal(heartHealAmount))
            {
                return false;
            }

            GameLog.Info("Player", $"Heart Drop applied (+{heartHealAmount} HP).");
            return true;
        }

        private void ClearSpeedBurst()
        {
            if (!_speedBurstActive && _speedBurstRemaining <= 0f)
            {
                if (playerController != null)
                {
                    playerController.SetSpeedMultiplier(1f);
                }

                return;
            }

            _speedBurstActive = false;
            _speedBurstRemaining = 0f;
            if (playerController != null)
            {
                playerController.SetSpeedMultiplier(1f);
            }
        }

        private void ClearGlowShield()
        {
            _glowShieldActive = false;
            _glowShieldRemaining = 0f;
            if (playerHealth != null)
            {
                playerHealth.SetGlowShield(false);
            }
        }

        private void TickTimedPowerUp(
            ref bool active,
            ref float remaining,
            PowerUpKind kind,
            Action clear)
        {
            if (!active)
            {
                return;
            }

            remaining -= Time.deltaTime;
            if (remaining > 0f)
            {
                return;
            }

            clear();
            Expired?.Invoke(kind);
            StateChanged?.Invoke();
            GameLog.Info("Player", $"{kind} expired.");
        }

        private void OnPlayerDied()
        {
            ClearAllPowerUps();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            speedBurstDuration = Mathf.Max(0.1f, speedBurstDuration);
            speedBurstMultiplier = Mathf.Max(1f, speedBurstMultiplier);
            glowShieldDuration = Mathf.Max(0.1f, glowShieldDuration);
            heartHealAmount = Mathf.Max(1, heartHealAmount);
        }
#endif
    }
}
