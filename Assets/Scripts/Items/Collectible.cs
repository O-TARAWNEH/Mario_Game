// Filename: Collectible.cs
// Folder: Assets/Scripts/Items/
// Purpose: Reusable placeable collectible with detection, state, effect, and notify (Phase 12).
// Dependencies: CollectibleCounter, CollectiblePickupInfo, CollectibleKind, GameLog

using System.Collections;
using BounderTrail.Audio;
using BounderTrail.Core;
using UnityEngine;

namespace BounderTrail.Items
{
    /// <summary>
    /// Trigger pickup. Easy to place as a prefab; notifies CollectibleCounter on collect.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private CollectibleKind kind = CollectibleKind.Coin;
        [SerializeField] private int coinValue = 1;
        [SerializeField] private int scoreValue = 10;

        [Header("Detection")]
        [SerializeField] private string playerTag = "Player";

        [Header("Feedback")]
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private float collectSoundVolume = 0.85f;
        [SerializeField] private float collectEffectDuration = 0.22f;
        [SerializeField] private float collectPopScale = 1.45f;
        [SerializeField] private bool destroyOnCollect = true;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D triggerCollider;
        [SerializeField] private AudioSource audioSource;

        private bool _collected;
        private Vector3 _baseScale;
        private Color _baseColor = Color.white;

        public CollectibleKind Kind => kind;
        public int CoinValue => coinValue;
        public int ScoreValue => scoreValue;
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
            if (_collected || other == null)
            {
                return;
            }

            if (!other.CompareTag(playerTag))
            {
                return;
            }

            Collect();
        }

        /// <summary>
        /// Collects this item if still available. Safe to call externally.
        /// </summary>
        public void Collect()
        {
            if (_collected)
            {
                return;
            }

            _collected = true;

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            var info = new CollectiblePickupInfo(
                kind,
                coinValue,
                scoreValue,
                transform.position,
                name);

            if (CollectibleCounter.Instance != null)
            {
                CollectibleCounter.Instance.RegisterCollection(info);
            }
            else
            {
                GameLog.Warning("Items", $"Collected '{name}' but no CollectibleCounter is present.");
            }

            PlayCollectSound();
            StartCoroutine(PlayCollectEffect());
        }

        private void PlayCollectSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.PlaySfx(SfxId.Collect);
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
                var scale = Mathf.Lerp(1f, collectPopScale, t);
                transform.localScale = _baseScale * scale;

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
            coinValue = Mathf.Max(0, coinValue);
            scoreValue = Mathf.Max(0, scoreValue);
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
