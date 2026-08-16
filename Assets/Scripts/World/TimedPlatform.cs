// Filename: TimedPlatform.cs
// Folder: Assets/Scripts/World/
// Purpose: Platform that cycles solid/ghost on a timer for timing puzzles (Phase 40).
// Dependencies: None

using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Enables / disables collision on a cycle so players must time jumps.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TimedPlatform : MonoBehaviour
    {
        [SerializeField] private float onDuration = 1.4f;
        [SerializeField] private float offDuration = 1.1f;
        [SerializeField] private float startDelay;
        [SerializeField] private bool startEnabled = true;
        [SerializeField] private float ghostAlpha = 0.28f;

        private Collider2D _collider;
        private SpriteRenderer _sprite;
        private Color _baseColor = Color.white;
        private float _timer;
        private bool _isOn;

        public bool IsOn => _isOn;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _sprite = GetComponent<SpriteRenderer>();
            if (_sprite == null)
            {
                _sprite = GetComponentInChildren<SpriteRenderer>();
            }

            if (_sprite != null)
            {
                _baseColor = _sprite.color;
            }

            _isOn = startEnabled;
            _timer = startDelay;
            ApplyState(immediate: true);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f)
            {
                return;
            }

            _isOn = !_isOn;
            _timer = _isOn ? Mathf.Max(0.1f, onDuration) : Mathf.Max(0.1f, offDuration);
            ApplyState(immediate: false);
        }

        public void Configure(float onSeconds, float offSeconds, float delay, bool beginsOn)
        {
            onDuration = Mathf.Max(0.1f, onSeconds);
            offDuration = Mathf.Max(0.1f, offSeconds);
            startDelay = Mathf.Max(0f, delay);
            startEnabled = beginsOn;
            _isOn = beginsOn;
            _timer = startDelay;
            ApplyState(immediate: true);
        }

        private void ApplyState(bool immediate)
        {
            if (_collider != null)
            {
                _collider.enabled = _isOn;
            }

            if (_sprite == null)
            {
                return;
            }

            var color = _baseColor;
            color.a = _isOn ? _baseColor.a : ghostAlpha;
            _sprite.color = color;
            if (!immediate)
            {
                // Keep tint readable when off.
                _sprite.color = color;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            onDuration = Mathf.Max(0.1f, onDuration);
            offDuration = Mathf.Max(0.1f, offDuration);
            startDelay = Mathf.Max(0f, startDelay);
            ghostAlpha = Mathf.Clamp01(ghostAlpha);
        }
#endif
    }
}
