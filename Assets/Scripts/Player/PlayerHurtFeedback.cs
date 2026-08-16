// Filename: PlayerHurtFeedback.cs
// Folder: Assets/Scripts/Player/
// Purpose: Visual feedback while the player is invulnerable after damage (Phase 11).
// Dependencies: PlayerHealth, SpriteRenderer

using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Flashes the player sprite during invulnerability frames.
    /// </summary>
    public class PlayerHurtFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Flash")]
        [SerializeField] private float flashInterval = 0.08f;
        [SerializeField] private Color hurtTint = new Color(1f, 0.55f, 0.55f, 1f);

        private Color _defaultColor = Color.white;
        private float _flashTimer;
        private bool _flashVisible = true;
        private bool _wasFlashing;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                _defaultColor = spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged += OnDamaged;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged -= OnDamaged;
            }

            RestoreVisual();
        }

        private void Update()
        {
            if (spriteRenderer == null || playerHealth == null)
            {
                return;
            }

            if (!playerHealth.HasHurtInvulnerability || !playerHealth.IsAlive)
            {
                if (_wasFlashing)
                {
                    RestoreVisual();
                    _wasFlashing = false;
                }

                return;
            }

            _wasFlashing = true;
            _flashTimer -= Time.deltaTime;
            if (_flashTimer > 0f)
            {
                return;
            }

            _flashTimer = flashInterval;
            _flashVisible = !_flashVisible;
            spriteRenderer.enabled = _flashVisible;
            spriteRenderer.color = _flashVisible ? hurtTint : _defaultColor;
        }

        private void OnDamaged(int amount, int remainingHealth)
        {
            _wasFlashing = true;
            _flashTimer = 0f;
            _flashVisible = true;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = hurtTint;
            }
        }

        private void RestoreVisual()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.enabled = true;
            spriteRenderer.color = _defaultColor;
            _flashVisible = true;
            _flashTimer = 0f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            flashInterval = Mathf.Max(0.02f, flashInterval);
        }
#endif
    }
}
