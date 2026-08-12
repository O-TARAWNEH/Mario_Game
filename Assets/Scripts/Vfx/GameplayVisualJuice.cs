// Filename: GameplayVisualJuice.cs
// Folder: Assets/Scripts/Vfx/
// Purpose: World juice for collectibles and level completion (Phase 26).
// Dependencies: CollectibleCounter, LevelCompletionService, CameraShake2D, SimpleBurstVfx

using BounderTrail.CameraSystem;
using BounderTrail.Items;
using BounderTrail.Levels;
using UnityEngine;

namespace BounderTrail.Vfx
{
    /// <summary>
    /// Bootstrap-side listeners for non-player juice events.
    /// Phase 29: rebinds on level load events instead of polling every frame.
    /// </summary>
    public class GameplayVisualJuice : MonoBehaviour
    {
        [SerializeField] private Sprite sparkleSprite;
        [SerializeField] private Sprite hitRingSprite;
        [SerializeField] private float completeShakeAmplitude = 0.14f;

        private CollectibleCounter _counter;
        private LevelCompletionService _completion;
        private CameraShake2D _cameraShake;

        private void OnEnable()
        {
            SubscribeLevelLoader();
            TryBind();
        }

        private void Start()
        {
            // Script order can mean LevelLoader.Instance is not ready in OnEnable.
            SubscribeLevelLoader();
            TryBind();
        }

        private void OnDisable()
        {
            if (LevelLoader.Instance != null)
            {
                LevelLoader.Instance.LevelLoadCompleted -= OnLevelLoaded;
            }

            Unbind();
        }

        private void Update()
        {
            // Fallback only while unbound (avoids Phase 28 stale-service gap without perpetual work).
            if (_counter == null || _completion == null)
            {
                SubscribeLevelLoader();
                TryBind();
            }
        }

        private void SubscribeLevelLoader()
        {
            if (LevelLoader.Instance == null)
            {
                return;
            }

            LevelLoader.Instance.LevelLoadCompleted -= OnLevelLoaded;
            LevelLoader.Instance.LevelLoadCompleted += OnLevelLoaded;
        }

        private void OnLevelLoaded(Data.LevelData _)
        {
            // LevelCompletionService is per-level; rebind after each load.
            if (_completion != null)
            {
                _completion.Completed -= OnLevelCompleted;
                _completion = null;
            }

            _cameraShake = null;
            TryBind();
        }

        private void TryBind()
        {
            if (_counter == null && CollectibleCounter.Instance != null)
            {
                _counter = CollectibleCounter.Instance;
                _counter.Collected += OnCollected;
            }

            if (_completion == null && LevelCompletionService.Instance != null)
            {
                _completion = LevelCompletionService.Instance;
                _completion.Completed += OnLevelCompleted;
            }
        }

        private void Unbind()
        {
            if (_counter != null)
            {
                _counter.Collected -= OnCollected;
                _counter = null;
            }

            if (_completion != null)
            {
                _completion.Completed -= OnLevelCompleted;
                _completion = null;
            }
        }

        private void OnCollected(CollectiblePickupInfo info)
        {
            var pos = (Vector3)info.WorldPosition;
            SimpleBurstVfx.Spawn(sparkleSprite, pos, new Color(1f, 0.9f, 0.35f, 0.95f), 0.25f, 0.35f, 1.1f, 27);
        }

        private void OnLevelCompleted()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var pos = player != null ? player.transform.position : Vector3.zero;
            SimpleBurstVfx.Spawn(hitRingSprite, pos, new Color(0.35f, 1f, 0.65f, 0.9f), 0.4f, 0.55f, 1.6f, 30);
            SimpleBurstVfx.Spawn(sparkleSprite, pos + Vector3.up * 0.4f, new Color(1f, 1f, 0.7f, 0.95f), 0.35f, 0.4f, 1.3f, 31);

            if (_cameraShake == null)
            {
                _cameraShake = FindFirstObjectByType<CameraShake2D>();
            }

            if (_cameraShake != null)
            {
                _cameraShake.Shake(completeShakeAmplitude, 0.35f, 0.75f);
            }
        }
    }
}
