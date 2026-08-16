// Filename: GateBarrier.cs
// Folder: Assets/Scripts/World/
// Purpose: Blocking wall/platform opened by PressureSwitch (Phase 40).
// Dependencies: None

using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Solid barrier that disappears (collider off + faded sprite) when opened.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GateBarrier : MonoBehaviour
    {
        [SerializeField] private bool startClosed = true;
        [SerializeField] private float openAlpha = 0.15f;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Collider2D _collider;
        private Color _baseColor = Color.white;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                _baseColor = spriteRenderer.color;
            }

            SetOpen(!startClosed);
        }

        public void SetOpen(bool open)
        {
            _isOpen = open;
            if (_collider != null)
            {
                _collider.enabled = !open;
            }

            if (spriteRenderer == null)
            {
                return;
            }

            var color = _baseColor;
            color.a = open ? openAlpha : _baseColor.a;
            spriteRenderer.color = color;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            openAlpha = Mathf.Clamp01(openAlpha);
        }
#endif
    }
}
