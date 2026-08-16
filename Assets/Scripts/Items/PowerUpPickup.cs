// Filename: PowerUpPickup.cs
// Folder: Assets/Scripts/Items/
// Purpose: Placeable power-up pickup with detection and collection state (Phase 13).
// Dependencies: PowerUpKind, BounderTrail.Player.PlayerPowerUps, GameLog

using System.Collections;
using BounderTrail.Audio;
using BounderTrail.Core;
using BounderTrail.Player;
using UnityEngine;

namespace BounderTrail.Items
{
    /// <summary>
    /// World pickup for a design-spec power-up. Activates effects through PlayerPowerUps.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PowerUpPickup : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private PowerUpKind kind = PowerUpKind.SpeedBurst;

        [Header("Detection")]
        [SerializeField] private string playerTag = "Player";

        [Header("Feedback")]
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private float collectSoundVolume = 0.9f;
        [SerializeField] private float collectEffectDuration = 0.2f;
        [SerializeField] private float collectPopScale = 1.4f;
        [SerializeField] private bool destroyOnCollect = true;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D triggerCollider;
        [SerializeField] private AudioSource audioSource;

        private bool _collected;
        private Vector3 _baseScale;
        private Color _baseColor = Color.white;

        public PowerUpKind Kind => kind;
        public bool IsCollected => _collected;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (triggerCollider == null)
            {
                triggerCollider = GetComponent<Collider2D>();
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }

            _baseScale = transform.localScale;
            if (spriteRenderer != null)
            {
                _baseColor = spriteRenderer.color;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryCollect(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryCollect(other);
        }

        private void TryCollect(Collider2D other)
        {
            if (_collected || other == null || !other.CompareTag(playerTag))
            {
                return;
            }

            var powerUps = other.GetComponentInParent<PlayerPowerUps>();
            if (powerUps == null || !powerUps.CanAcceptPowerUps)
            {
                return;
            }

            Collect(powerUps);
        }

        public void Collect(PlayerPowerUps powerUps)
        {
            if (_collected || powerUps == null)
            {
                return;
            }

            // Activate first so full-HP Heart Drops stay in the world (Phase 31).
            if (!powerUps.TryActivate(kind))
            {
                return;
            }

            _collected = true;
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            GameLog.Info("Items", $"Power-up '{name}' ({kind}) collected.");
            PlayCollectSound();
            StartCoroutine(PlayCollectEffect());
        }

        private void PlayCollectSound()
        {
            // Central power-up SFX plays via PlayerAudioFeedback on Activated.
            if (AudioManager.Instance != null)
            {
                return;
            }

            // Fallback for isolated prefab tests without bootstrap audio.
            if (collectSound == null)
            {
                return;
            }

            if (audioSource != null)
            {
                audioSource.PlayOneShot(collectSound, collectSoundVolume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, collectSoundVolume);
            }
        }

        private IEnumerator PlayCollectEffect()
        {
            var duration = Mathf.Max(0.05f, collectEffectDuration);
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = _baseScale * Mathf.Lerp(1f, collectPopScale, t);

                if (spriteRenderer != null)
                {
                    var color = _baseColor;
                    color.a = Mathf.Lerp(1f, 0f, t);
                    spriteRenderer.color = color;
                }

                yield return null;
            }

            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            collectSoundVolume = Mathf.Clamp01(collectSoundVolume);
            collectEffectDuration = Mathf.Max(0.05f, collectEffectDuration);
            collectPopScale = Mathf.Max(1f, collectPopScale);

            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
#endif
    }
}
