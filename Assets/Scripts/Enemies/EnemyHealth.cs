// Filename: EnemyHealth.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Enemy hit points, hurt i-frames, knockback, and death (Phase 9/11).
// Dependencies: BounderTrail.Core.IDamageable, GameLog

using System;
using BounderTrail.Audio;
using BounderTrail.Core;
using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Health/state for enemies. Implements IDamageable for stomps and future attacks.
    /// </summary>
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private float hurtDuration = 0.2f;
        [SerializeField] private float invulnerabilityDuration = 0.2f;
        [SerializeField] private Vector2 knockbackForce = new Vector2(2.5f, 3.5f);
        [SerializeField] private bool applyKnockbackOnHit = true;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float destroyDelay = 0.35f;

        private int _currentHealth;
        private float _hurtTimer;
        private float _invulnerabilityTimer;
        private Collider2D[] _colliders;
        private Rigidbody2D _body;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => _currentHealth;
        public bool IsAlive => _currentHealth > 0 && !IsDead;
        public bool IsDead { get; private set; }
        public bool IsHurt => _hurtTimer > 0f;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;

        public event Action Hurt;
        public event Action Died;

        private void Awake()
        {
            _currentHealth = Mathf.Max(1, maxHealth);
            _colliders = GetComponentsInChildren<Collider2D>(true);
            _body = GetComponent<Rigidbody2D>();
            // Timers idle until a hit — skips needless Update (Phase 29).
            enabled = false;
        }

        private void Update()
        {
            var ticking = false;
            if (_hurtTimer > 0f)
            {
                _hurtTimer -= Time.deltaTime;
                ticking = _hurtTimer > 0f;
            }

            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= Time.deltaTime;
                if (_invulnerabilityTimer > 0f)
                {
                    ticking = true;
                }
            }

            if (!ticking)
            {
                _hurtTimer = 0f;
                _invulnerabilityTimer = 0f;
                enabled = false;
            }
        }

        public void TakeDamage(int amount, Vector2 hitFrom)
        {
            if (!IsAlive || amount <= 0 || IsInvulnerable)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            _hurtTimer = hurtDuration;
            _invulnerabilityTimer = invulnerabilityDuration;
            enabled = true;
            // Raise Hurt first so brain can enter Hurt/stop patrol, then apply knockback.
            Hurt?.Invoke();
            ApplyKnockback(hitFrom);
            GameLog.Info("Enemy", $"{name} took {amount} damage ({_currentHealth}/{maxHealth}).");

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            _currentHealth = 0;
            _hurtTimer = 0f;
            _invulnerabilityTimer = 0f;
            enabled = false;

            if (_colliders != null)
            {
                for (var i = 0; i < _colliders.Length; i++)
                {
                    if (_colliders[i] != null)
                    {
                        _colliders[i].enabled = false;
                    }
                }
            }

            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
                _body.simulated = false;
            }

            GameLog.Info("Enemy", $"{name} died.");
            AudioManager.PlaySfx(SfxId.EnemyDefeat);
            Died?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Soft-death mode used by checkpoint respawn (enemy is disabled instead of destroyed).
        /// </summary>
        public void SetDestroyOnDeath(bool destroy)
        {
            destroyOnDeath = destroy;
        }

        /// <summary>
        /// Restores health/colliders after a soft death for checkpoint respawn.
        /// </summary>
        public void Revive()
        {
            IsDead = false;
            _currentHealth = Mathf.Max(1, maxHealth);
            _hurtTimer = 0f;
            _invulnerabilityTimer = 0f;
            enabled = false;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (_colliders != null)
            {
                for (var i = 0; i < _colliders.Length; i++)
                {
                    if (_colliders[i] != null)
                    {
                        _colliders[i].enabled = true;
                    }
                }
            }

            if (_body != null)
            {
                _body.simulated = true;
                _body.linearVelocity = Vector2.zero;
            }
        }

        private void ApplyKnockback(Vector2 hitFrom)
        {
            if (!applyKnockbackOnHit || _body == null || !_body.simulated)
            {
                return;
            }

            var away = ((Vector2)transform.position - hitFrom).normalized;
            if (away.sqrMagnitude < 0.001f)
            {
                away = Vector2.right;
            }

            var horizontal = Mathf.Sign(away.x);
            if (Mathf.Abs(horizontal) < 0.01f)
            {
                horizontal = 1f;
            }

            _body.linearVelocity = new Vector2(horizontal * knockbackForce.x, knockbackForce.y);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            hurtDuration = Mathf.Max(0f, hurtDuration);
            invulnerabilityDuration = Mathf.Max(0f, invulnerabilityDuration);
            knockbackForce.x = Mathf.Max(0f, knockbackForce.x);
            knockbackForce.y = Mathf.Max(0f, knockbackForce.y);
            destroyDelay = Mathf.Max(0f, destroyDelay);
        }
#endif
    }
}
