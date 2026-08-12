// Filename: PlayerPowerUpFeedback.cs
// Folder: Assets/Scripts/Player/
// Purpose: Visual tint while Speed Burst / Glow Shield are active (Phase 13).
// Dependencies: PlayerPowerUps, PlayerHealth, SpriteRenderer

using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Soft visual cue for active timed power-ups. Defers to hurt-flash while damaged.
    /// </summary>
    public class PlayerPowerUpFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerPowerUps powerUps;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Tints")]
        [SerializeField] private Color speedTint = new Color(1f, 0.92f, 0.45f, 1f);
        [SerializeField] private Color shieldTint = new Color(0.45f, 0.95f, 1f, 1f);
        [SerializeField] private float shieldPulseSpeed = 6f;

        private Color _defaultColor = Color.white;

        private void Awake()
        {
            if (powerUps == null)
            {
                powerUps = GetComponent<PlayerPowerUps>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                _defaultColor = spriteRenderer.color;
            }
        }

        private void LateUpdate()
        {
            if (spriteRenderer == null || powerUps == null)
            {
                return;
            }

            // Hurt flash owns the sprite while damage i-frames are active.
            if (playerHealth != null && playerHealth.HasHurtInvulnerability)
            {
                return;
            }

            if (powerUps.HasGlowShield)
            {
                var pulse = 0.65f + (0.35f * (0.5f + 0.5f * Mathf.Sin(Time.time * shieldPulseSpeed)));
                spriteRenderer.color = Color.Lerp(_defaultColor, shieldTint, pulse);
                return;
            }

            if (powerUps.HasSpeedBurst)
            {
                spriteRenderer.color = Color.Lerp(_defaultColor, speedTint, 0.55f);
                return;
            }

            spriteRenderer.color = _defaultColor;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            shieldPulseSpeed = Mathf.Max(0.1f, shieldPulseSpeed);
        }
#endif
    }
}
