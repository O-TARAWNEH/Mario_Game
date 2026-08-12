// Filename: CameraShake2D.cs
// Folder: Assets/Scripts/Camera/
// Purpose: Short camera shake impulses for hit/death/complete feedback (Phase 26).
// Dependencies: None

using UnityEngine;

namespace BounderTrail.CameraSystem
{
    /// <summary>
    /// Provides a decaying 2D offset. CameraFollow2D applies it after follow math
    /// so SmoothDamp is not contaminated by shake.
    /// </summary>
    public class CameraShake2D : MonoBehaviour
    {
        [Header("Tuning")]
        [SerializeField] private float maxAmplitude = 0.45f;
        [SerializeField] private float defaultTraumaDecay = 1.6f;

        private float _trauma;
        private float _amplitude;
        private float _decay;
        private float _seed;

        public Vector2 CurrentOffset { get; private set; }

        private void Awake()
        {
            _seed = Random.value * 100f;
            // Idle until Shake() — avoids needless Update while trauma is zero (Phase 29).
            enabled = false;
        }

        private void Update()
        {
            if (_trauma <= 0f)
            {
                CurrentOffset = Vector2.zero;
                enabled = false;
                return;
            }

            _trauma = Mathf.Max(0f, _trauma - _decay * Time.unscaledDeltaTime);
            var strength = _amplitude * (_trauma * _trauma);
            var t = Time.unscaledTime * 28f + _seed;
            CurrentOffset = new Vector2(
                (Mathf.PerlinNoise(t, _seed) * 2f - 1f) * strength,
                (Mathf.PerlinNoise(_seed, t) * 2f - 1f) * strength);

            if (_trauma <= 0f)
            {
                CurrentOffset = Vector2.zero;
                enabled = false;
            }
        }

        /// <summary>Queues a shake. Trauma is clamped 0–1; larger impulses replace weaker ones.</summary>
        public void Shake(float amplitude, float durationSeconds, float trauma = 1f)
        {
            amplitude = Mathf.Clamp(amplitude, 0f, maxAmplitude);
            durationSeconds = Mathf.Max(0.01f, durationSeconds);
            trauma = Mathf.Clamp01(trauma);

            if (trauma < _trauma && amplitude <= _amplitude)
            {
                return;
            }

            _amplitude = amplitude;
            _trauma = trauma;
            _decay = trauma / durationSeconds;
            if (_decay < 0.01f)
            {
                _decay = defaultTraumaDecay;
            }

            enabled = true;
        }

        public void StopShake()
        {
            _trauma = 0f;
            CurrentOffset = Vector2.zero;
            enabled = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxAmplitude = Mathf.Max(0.01f, maxAmplitude);
            defaultTraumaDecay = Mathf.Max(0.01f, defaultTraumaDecay);
        }
#endif
    }
}
