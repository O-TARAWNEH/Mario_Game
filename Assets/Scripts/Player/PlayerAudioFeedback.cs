// Filename: PlayerAudioFeedback.cs
// Folder: Assets/Scripts/Player/
// Purpose: Routes player jump/land/damage/death/power-up events to AudioManager (Phase 18).
// Dependencies: PlayerController, PlayerHealth, PlayerDeath, PlayerPowerUps, BounderTrail.Audio

using BounderTrail.Audio;
using BounderTrail.Items;
using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Lightweight player-side audio hooks. Does not own clips — AudioManager does.
    /// </summary>
    public class PlayerAudioFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerDeath playerDeath;
        [SerializeField] private PlayerPowerUps playerPowerUps;

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

            if (playerPowerUps == null)
            {
                playerPowerUps = GetComponent<PlayerPowerUps>();
            }
        }

        private void OnEnable()
        {
            if (playerController != null)
            {
                playerController.Jumped += OnJumped;
                playerController.Landed += OnLanded;
            }

            if (playerHealth != null)
            {
                playerHealth.Damaged += OnDamaged;
            }

            if (playerDeath != null)
            {
                playerDeath.Died += OnDied;
            }

            if (playerPowerUps != null)
            {
                playerPowerUps.Activated += OnPowerUpActivated;
            }
        }

        private void OnDisable()
        {
            if (playerController != null)
            {
                playerController.Jumped -= OnJumped;
                playerController.Landed -= OnLanded;
            }

            if (playerHealth != null)
            {
                playerHealth.Damaged -= OnDamaged;
            }

            if (playerDeath != null)
            {
                playerDeath.Died -= OnDied;
            }

            if (playerPowerUps != null)
            {
                playerPowerUps.Activated -= OnPowerUpActivated;
            }
        }

        private void OnJumped()
        {
            AudioManager.PlaySfx(SfxId.Jump);
        }

        private void OnLanded()
        {
            // Skip soft landings — reduces repetitive thud spam on tiny drops.
            if (playerController != null && playerController.LastLandingSpeed < 3.5f)
            {
                return;
            }

            AudioManager.PlaySfx(SfxId.Land);
        }

        private static void OnDamaged(int amount, int remainingHealth)
        {
            // Lethal hits play Death via PlayerDeath; skip Damage to avoid stacking.
            if (remainingHealth > 0)
            {
                AudioManager.PlaySfx(SfxId.Damage);
            }
        }

        private static void OnDied()
        {
            AudioManager.PlaySfx(SfxId.Death);
        }

        private static void OnPowerUpActivated(PowerUpKind kind)
        {
            var id = kind switch
            {
                PowerUpKind.HeartDrop => SfxId.PowerUpHeart,
                PowerUpKind.GlowShield => SfxId.PowerUpShield,
                PowerUpKind.SpeedBurst => SfxId.PowerUpSpeed,
                _ => SfxId.PowerUp
            };
            AudioManager.PlaySfx(id);
        }
    }
}
