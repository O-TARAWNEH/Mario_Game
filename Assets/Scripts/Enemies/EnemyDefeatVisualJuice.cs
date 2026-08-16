// Filename: EnemyDefeatVisualJuice.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Brief defeat burst when an enemy dies (Phase 26).
// Dependencies: EnemyHealth, SimpleBurstVfx

using BounderTrail.CameraSystem;
using BounderTrail.Vfx;
using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Visual-only death puff. Does not change damage, rewards, or respawn.
    /// </summary>
    public class EnemyDefeatVisualJuice : MonoBehaviour
    {
        [SerializeField] private EnemyHealth health;
        [SerializeField] private Sprite dustSprite;
        [SerializeField] private Sprite sparkleSprite;
        [SerializeField] private Color burstColor = new Color(1f, 0.85f, 0.4f, 0.9f);
        [SerializeField] private float defeatHitStop = 0.055f;
        [SerializeField] private float defeatShakeAmplitude = 0.12f;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        private void OnDied()
        {
            var pos = transform.position;
            SimpleBurstVfx.Spawn(dustSprite, pos, new Color(1f, 1f, 1f, 0.8f), 0.28f, 0.45f, 1.3f, 24);
            SimpleBurstVfx.Spawn(sparkleSprite, pos, burstColor, 0.24f, 0.4f, 1.2f, 26);
            HitStop.Pulse(defeatHitStop);

            var shake = FindFirstObjectByType<CameraShake2D>();
            if (shake != null)
            {
                shake.Shake(defeatShakeAmplitude, 0.14f, 0.7f);
            }
        }
    }
}
