// Filename: HitStop.cs
// Folder: Assets/Scripts/Vfx/
// Purpose: Brief timeScale freeze for combat/impact weight (Phase 38).
// Dependencies: BounderTrail.Core.GameStateManager, GameStateId

using System.Collections;
using BounderTrail.Core;
using UnityEngine;

namespace BounderTrail.Vfx
{
    /// <summary>
    /// Tiny freeze-frame pulses. Uses realtime waits so it can restore timeScale cleanly.
    /// Skips when the game is already paused / overlay-frozen.
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        public static HitStop Instance { get; private set; }

        [SerializeField] private float defaultScale = 0.05f;
        [SerializeField] private float maxDuration = 0.12f;

        private Coroutine _pulseRoutine;
        private float _restoreScale = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static void Pulse(float durationSeconds, float timeScale = -1f)
        {
            if (Instance == null)
            {
                return;
            }

            Instance.Play(durationSeconds, timeScale);
        }

        public void Play(float durationSeconds, float timeScale = -1f)
        {
            if (GameStateManager.Instance != null
                && GameStateManager.Instance.CurrentState != GameStateId.Gameplay)
            {
                return;
            }

            if (Time.timeScale <= 0.001f)
            {
                return;
            }

            var duration = Mathf.Clamp(durationSeconds, 0.01f, maxDuration);
            var scale = timeScale < 0f ? defaultScale : Mathf.Clamp(timeScale, 0.01f, 1f);

            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
                Time.timeScale = _restoreScale;
            }

            _pulseRoutine = StartCoroutine(PulseRoutine(duration, scale));
        }

        private IEnumerator PulseRoutine(float duration, float scale)
        {
            _restoreScale = Time.timeScale > 0.001f ? Time.timeScale : 1f;
            Time.timeScale = scale;
            yield return new WaitForSecondsRealtime(duration);
            RestoreAfterPulse();
            _pulseRoutine = null;
        }

        private void RestoreAfterPulse()
        {
            if (GameStateManager.Instance == null)
            {
                Time.timeScale = _restoreScale;
                return;
            }

            switch (GameStateManager.Instance.CurrentState)
            {
                case GameStateId.Pause:
                case GameStateId.GameOver:
                case GameStateId.LevelComplete:
                    Time.timeScale = 0f;
                    break;
                case GameStateId.Gameplay:
                    Time.timeScale = _restoreScale > 0.001f ? _restoreScale : 1f;
                    break;
                default:
                    Time.timeScale = 1f;
                    break;
            }
        }
    }
}
