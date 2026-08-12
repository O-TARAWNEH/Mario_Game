// Filename: EnvironmentalHazard.cs
// Folder: Assets/Scripts/World/
// Purpose: Shared player detection + environmental damage/death for hazards (Phase 14).
// Dependencies: HazardResponse, BounderTrail.Core.IDamageable, BounderTrail.Player.PlayerDeath, GameLog

using BounderTrail.Core;
using BounderTrail.Player;
using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Placeable hazard trigger/collider. Detects the player and applies kill or damage.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnvironmentalHazard : MonoBehaviour
    {
        [Header("Response")]
        [SerializeField] private HazardResponse response = HazardResponse.ContactDamage;
        [SerializeField] private int damage = 1;
        [SerializeField] private float damageInterval = 0.45f;

        [Header("Detection")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool useTrigger = true;

        private Collider2D _collider;
        private float _nextDamageTime;
        private bool _playerInside;

        public HazardResponse Response => response;
        public int Damage => damage;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            if (_collider != null && useTrigger)
            {
                _collider.isTrigger = true;
            }
        }

        private void OnEnable()
        {
            ResetHazardState();
        }

        private void OnDisable()
        {
            _playerInside = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsPlayer(other))
            {
                return;
            }

            _playerInside = true;
            ApplyTo(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!IsPlayer(other))
            {
                return;
            }

            _playerInside = true;
            if (response == HazardResponse.DamageOverTime)
            {
                ApplyTo(other);
            }
            else if (response == HazardResponse.ContactDamage)
            {
                // Re-apply when i-frames end while still overlapping spikes.
                ApplyTo(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsPlayer(other))
            {
                return;
            }

            _playerInside = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (useTrigger || collision.collider == null || !IsPlayer(collision.collider))
            {
                return;
            }

            _playerInside = true;
            ApplyTo(collision.collider);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (useTrigger || collision.collider == null || !IsPlayer(collision.collider))
            {
                return;
            }

            ApplyTo(collision.collider);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (useTrigger || collision.collider == null || !IsPlayer(collision.collider))
            {
                return;
            }

            _playerInside = false;
        }

        /// <summary>
        /// Clears overlap/timers so re-enabled hazards behave cleanly after scene reload / pool reuse.
        /// </summary>
        public void ResetHazardState()
        {
            _playerInside = false;
            _nextDamageTime = 0f;
        }

        private bool IsPlayer(Collider2D other)
        {
            return other != null && other.CompareTag(playerTag);
        }

        private void ApplyTo(Collider2D other)
        {
            switch (response)
            {
                case HazardResponse.InstantKill:
                    ApplyInstantKill(other);
                    break;
                case HazardResponse.ContactDamage:
                case HazardResponse.DamageOverTime:
                    ApplyDamage(other);
                    break;
            }
        }

        private void ApplyInstantKill(Collider2D other)
        {
            // Route through health so hearts empty before death (pits = lose all remaining hearts).
            var health = other.GetComponentInParent<PlayerHealth>();
            if (health != null && health.IsAlive)
            {
                if (!health.IsInvulnerable)
                {
                    GameLog.Info("Hazard", $"{name} lethal hit — draining remaining health.");
                    health.TakeDamage(health.CurrentHealth, transform.position);
                }

                return;
            }

            var death = other.GetComponentInParent<PlayerDeath>();
            if (death == null || !death.IsAlive)
            {
                return;
            }

            GameLog.Info("Hazard", $"{name} killed player (InstantKill fallback).");
            death.Die();
        }

        private void ApplyDamage(Collider2D other)
        {
            if (Time.time < _nextDamageTime)
            {
                return;
            }

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            // Respects PlayerHealth i-frames / Glow Shield.
            if (damageable is PlayerHealth health && health.IsInvulnerable)
            {
                return;
            }

            damageable.TakeDamage(Mathf.Max(1, damage), transform.position);
            _nextDamageTime = Time.time + Mathf.Max(0.05f, damageInterval);
            GameLog.Info("Hazard", $"{name} dealt {damage} environmental damage ({response}).");
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            damage = Mathf.Max(1, damage);
            damageInterval = Mathf.Max(0.05f, damageInterval);

            var col = GetComponent<Collider2D>();
            if (col != null && useTrigger)
            {
                col.isTrigger = true;
            }
        }
#endif
    }
}
