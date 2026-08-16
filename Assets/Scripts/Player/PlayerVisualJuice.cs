// Filename: PlayerVisualJuice.cs
// Folder: Assets/Scripts/Player/
// Purpose: Jump/land/hit/death/power-up visual bursts + camera shake + hitstop (Phase 26/38).
// Dependencies: PlayerController, PlayerHealth, PlayerDeath, PlayerPowerUps, CameraShake2D, SimpleBurstVfx, HitStop

using BounderTrail.CameraSystem;
using BounderTrail.Items;
using BounderTrail.Vfx;
using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Event-driven juice only. Does not alter movement, damage, or timers.
    /// </summary>
    public class PlayerVisualJuice : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerDeath playerDeath;
        [SerializeField] private PlayerPowerUps playerPowerUps;

        [Header("Sprites")]
        [SerializeField] private Sprite dustSprite;
        [SerializeField] private Sprite hitRingSprite;
        [SerializeField] private Sprite sparkleSprite;

        [Header("Strength")]
        [SerializeField] private float landDustSize = 0.9f;
        [SerializeField] private float jumpDustSize = 0.55f;
        [SerializeField] private float hurtShakeAmplitude = 0.18f;
        [SerializeField] private float deathShakeAmplitude = 0.38f;
        [SerializeField] private float hurtHitStop = 0.045f;
        [SerializeField] private float deathHitStop = 0.08f;
        [SerializeField] private bool enableJumpPuff = true;

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
            if (!enableJumpPuff)
            {
                return;
            }

            var pos = transform.position + new Vector3(0f, -0.35f, 0f);
            SimpleBurstVfx.Spawn(dustSprite, pos, new Color(1f, 1f, 1f, 0.65f), 0.18f, 0.25f, jumpDustSize, 18);
        }

        private void OnLanded()
        {
            var impact = playerController != null ? playerController.LastLandingSpeed : 6f;
            // Soft landings stay quiet; hard landings get a bigger dust puff.
            if (impact < 3.5f)
            {
                return;
            }

            var size = Mathf.Lerp(landDustSize * 0.55f, landDustSize * 1.35f, Mathf.InverseLerp(3.5f, 16f, impact));
            var particles = impact > 10f ? 28 : 18;
            var pos = transform.position + new Vector3(0f, -0.4f, 0f);
            SimpleBurstVfx.Spawn(dustSprite, pos, new Color(1f, 1f, 1f, 0.8f), 0.22f, 0.35f, size, particles);
        }

        private void OnDamaged(int amount, int remainingHealth)
        {
            if (remainingHealth <= 0)
            {
                return;
            }

            SimpleBurstVfx.Spawn(
                hitRingSprite,
                transform.position,
                new Color(1f, 0.55f, 0.45f, 0.9f),
                0.2f,
                0.4f,
                1.2f,
                26);
            Shake(hurtShakeAmplitude, 0.16f, 0.85f);
            HitStop.Pulse(hurtHitStop);
        }

        private void OnDied()
        {
            SimpleBurstVfx.Spawn(
                hitRingSprite,
                transform.position,
                new Color(0.7f, 0.75f, 0.9f, 0.85f),
                0.35f,
                0.55f,
                1.65f,
                28);
            Shake(deathShakeAmplitude, 0.32f, 1f);
            HitStop.Pulse(deathHitStop, 0.03f);
        }

        private void OnPowerUpActivated(PowerUpKind kind)
        {
            var color = kind switch
            {
                PowerUpKind.SpeedBurst => new Color(1f, 0.85f, 0.25f, 0.95f),
                PowerUpKind.GlowShield => new Color(0.4f, 0.9f, 1f, 0.95f),
                PowerUpKind.HeartDrop => new Color(1f, 0.45f, 0.6f, 0.95f),
                _ => Color.white
            };

            SimpleBurstVfx.Spawn(sparkleSprite, transform.position + Vector3.up * 0.2f, color, 0.3f, 0.4f, 1.25f, 28);
            SimpleBurstVfx.Spawn(hitRingSprite, transform.position, color, 0.22f, 0.45f, 1.15f, 24);
        }

        private static void Shake(float amplitude, float duration, float trauma)
        {
            var shake = FindFirstObjectByType<CameraShake2D>();
            if (shake != null)
            {
                shake.Shake(amplitude, duration, trauma);
            }
        }
    }
}
