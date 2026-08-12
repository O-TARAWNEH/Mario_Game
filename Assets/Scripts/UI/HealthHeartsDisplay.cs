// Filename: HealthHeartsDisplay.cs
// Folder: Assets/Scripts/UI/
// Purpose: Visual heart row tied to PlayerHealth (one heart per HP point).
// Dependencies: PlayerHealth, UnityEngine.UI

using System.Collections.Generic;
using BounderTrail.Player;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.UI
{
    /// <summary>
    /// Shows filled/empty hearts that decrease one-by-one when the player takes damage.
    /// </summary>
    public class HealthHeartsDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private RectTransform heartRow;

        [Header("Sprites")]
        [SerializeField] private Sprite heartFullSprite;
        [SerializeField] private Sprite heartEmptySprite;

        [Header("Layout")]
        [SerializeField] private Vector2 heartSize = new Vector2(36f, 36f);
        [SerializeField] private float spacing = 6f;
        [SerializeField] private Vector2 rowOffset = new Vector2(24f, -8f);

        [Header("Colors")]
        [SerializeField] private Color heartFullColor = new Color(1f, 0.42f, 0.55f, 1f);
        [SerializeField] private Color heartEmptyColor = new Color(0.28f, 0.28f, 0.34f, 0.75f);

        private readonly List<Image> _slots = new List<Image>();

        private void OnEnable()
        {
            EnsureDefaultSprites();
            ResolvePlayer();
            EnsureHeartRow();
            if (_slots.Count == 0 || (playerHealth != null && _slots.Count != playerHealth.MaxHealth))
            {
                RebuildSlots();
            }

            Bind();
            Refresh();
        }

        private void EnsureDefaultSprites()
        {
            if (heartFullSprite != null && heartEmptySprite != null)
            {
                return;
            }

            var heart = Resources.Load<Sprite>("UI/HeartSprite");
            if (heart == null)
            {
                return;
            }

            if (heartFullSprite == null)
            {
                heartFullSprite = heart;
            }

            if (heartEmptySprite == null)
            {
                heartEmptySprite = heart;
            }
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void Refresh()
        {
            if (playerHealth == null)
            {
                return;
            }

            var max = playerHealth.MaxHealth;
            var current = playerHealth.CurrentHealth;

            if (_slots.Count != max)
            {
                RebuildSlots();
            }

            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot == null)
                {
                    continue;
                }

                var filled = i < current;
                slot.gameObject.SetActive(i < max);
                slot.sprite = filled ? heartFullSprite : heartEmptySprite;
                slot.color = filled ? heartFullColor : heartEmptyColor;
            }
        }

        private void ResolvePlayer()
        {
            if (playerHealth != null)
            {
                return;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        private void EnsureHeartRow()
        {
            if (heartRow != null)
            {
                return;
            }

            var existing = transform.Find("HeartRow");
            if (existing != null)
            {
                heartRow = existing as RectTransform;
                return;
            }

            var rowGo = new GameObject("HeartRow", typeof(RectTransform));
            rowGo.transform.SetParent(transform, false);
            heartRow = rowGo.GetComponent<RectTransform>();
            heartRow.anchorMin = new Vector2(0f, 1f);
            heartRow.anchorMax = new Vector2(0f, 1f);
            heartRow.pivot = new Vector2(0f, 1f);
            heartRow.anchoredPosition = rowOffset;
        }

        private void RebuildSlots()
        {
            _slots.Clear();

            if (heartRow == null)
            {
                return;
            }

            for (var i = heartRow.childCount - 1; i >= 0; i--)
            {
                Destroy(heartRow.GetChild(i).gameObject);
            }

            var count = playerHealth != null ? playerHealth.MaxHealth : 3;
            for (var i = 0; i < count; i++)
            {
                var imgGo = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(Image));
                imgGo.transform.SetParent(heartRow, false);
                var rect = imgGo.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = heartSize;
                rect.anchoredPosition = new Vector2(i * (heartSize.x + spacing), 0f);

                var img = imgGo.GetComponent<Image>();
                img.raycastTarget = false;
                img.preserveAspect = true;
                img.sprite = heartFullSprite;
                _slots.Add(img);
            }
        }

        private void Bind()
        {
            Unbind();
            if (playerHealth != null)
            {
                playerHealth.HealthChanged += OnHealthChanged;
                playerHealth.Damaged += OnDamaged;
            }
        }

        private void Unbind()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= OnHealthChanged;
                playerHealth.Damaged -= OnDamaged;
            }
        }

        private void OnHealthChanged()
        {
            Refresh();
        }

        private void OnDamaged(int _, int __)
        {
            Refresh();
        }
    }
}
