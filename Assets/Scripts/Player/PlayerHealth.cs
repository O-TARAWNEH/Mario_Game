// Filename: PlayerHealth.cs
// Folder: Assets/Scripts/Player/
// Purpose: Player hit points, invulnerability, knockback, and damage routing (Phase 11).
// Dependencies: PlayerController, PlayerDeath, BounderTrail.Core.IDamageable, GameLog

using System;
using BounderTrail.Core;
using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Owns Pip's health. Enemies/projectiles damage through IDamageable.
    /// Surviving hits grant invulnerability frames and knockback; 0 HP triggers PlayerDeath.
    /// </summary>
    [RequireComponent(typeof(PlayerDeath))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerDeath playerDeath;
        [SerializeField] private Rigidbody2D rigidBody;

        [Header("Health")]
        [SerializeField] private int maxHealth = 3;

        [Header("Invulnerability")]
        [SerializeField] private float invulnerabilityDuration = 1.25f;

        [Header("Knockback")]
        [SerializeField] private Vector2 knockbackForce = new Vector2(6.5f, 8f);
        [SerializeField] private float controlLockDuration = 0.2f;

        private int _currentHealth;
        private float _invulnerabilityTimer;
        private bool _glowShieldActive;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => _currentHealth;
        public bool IsAlive => playerDeath == null ? _currentHealth > 0 : playerDeath.IsAlive && _currentHealth > 0;
        public bool IsFullHealth => _currentHealth >= maxHealth;
        public bool HasHurtInvulnerability => _invulnerabilityTimer > 0f;
        public bool HasGlowShield => _glowShieldActive;
        public bool IsInvulnerable => HasHurtInvulnerability || HasGlowShield;
        public float InvulnerabilityRemaining => Mathf.Max(0f, _invulnerabilityTimer);

        public event Action<int, int> Damaged;
        public event Action HealthChanged;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (playerDeath == null)
            {
                playerDeath = GetComponent<PlayerDeath>();
            }

            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody2D>();
            }

            _currentHealth = Mathf.Max(1, maxHealth);
            // No i-frames yet — skip idle Update (Phase 29).
            enabled = false;
        }

        private void Update()
        {
            if (_invulnerabilityTimer <= 0f)
            {
                enabled = false;
                return;
            }

            _invulnerabilityTimer -= Time.deltaTime;
            if (_invulnerabilityTimer <= 0f)
            {
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
            _invulnerabilityTimer = invulnerabilityDuration;
            enabled = true;

            ApplyKnockback(hitFrom);
            Damaged?.Invoke(amount, _currentHealth);
            HealthChanged?.Invoke();
            GameLog.Info("Player", $"Took {amount} damage ({_currentHealth}/{maxHealth}).");

            if (_currentHealth <= 0)
            {
                _invulnerabilityTimer = 0f;
                enabled = false;
                if (playerDeath != null)
                {
                    playerDeath.Die();
                }
            }
        }

        /// <summary>
        /// Bounce used after a successful enemy stomp.
        /// </summary>
        public void ApplyStompBounce(float upwardSpeed)
        {
            if (rigidBody == null || !IsAlive)
            {
                return;
            }

            var velocity = rigidBody.linearVelocity;
            velocity.y = upwardSpeed;
            rigidBody.linearVelocity = velocity;
        }

        public void Heal(int amount)
        {
            TryHeal(amount);
        }

        /// <summary>
        /// Heals when possible. Returns false if dead, invalid amount, or already full (Phase 31).
        /// </summary>
        public bool TryHeal(int amount)
        {
            if (!IsAlive || amount <= 0 || IsFullHealth)
            {
                return false;
            }

            var before = _currentHealth;
            _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
            if (_currentHealth == before)
            {
                return false;
            }

            HealthChanged?.Invoke();
            GameLog.Info("Player", $"Healed {amount} ({_currentHealth}/{maxHealth}).");
            return true;
        }

        public void ResetHealth()
        {
            _currentHealth = Mathf.Max(1, maxHealth);
            _invulnerabilityTimer = 0f;
            _glowShieldActive = false;
            enabled = false;
            HealthChanged?.Invoke();
        }

        /// <summary>
        /// Ensures health is at max when a level begins (hearts full).
        /// </summary>
        public void RefillToMax()
        {
            if (_currentHealth >= maxHealth)
            {
                return;
            }

            _currentHealth = maxHealth;
            HealthChanged?.Invoke();
        }

        /// <summary>
        /// Enables/disables temporary Glow Shield invincibility from power-ups.
        /// </summary>
        public void SetGlowShield(bool active)
        {
            _glowShieldActive = active;
        }

        private void ApplyKnockback(Vector2 hitFrom)
        {
            if (rigidBody == null)
            {
                return;
            }

            var away = ((Vector2)transform.position - hitFrom).normalized;
            if (away.sqrMagnitude < 0.001f)
            {
                away = playerController != null && playerController.FacingRight
                    ? Vector2.left
                    : Vector2.right;
            }

            var horizontal = Mathf.Sign(away.x);
            if (Mathf.Abs(horizontal) < 0.01f)
            {
                horizontal = playerController != null && playerController.FacingRight ? -1f : 1f;
            }

            var knockback = new Vector2(horizontal * knockbackForce.x, knockbackForce.y);
            rigidBody.linearVelocity = knockback;

            if (playerController != null)
            {
                playerController.LockControl(controlLockDuration);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            invulnerabilityDuration = Mathf.Max(0f, invulnerabilityDuration);
            knockbackForce.x = Mathf.Max(0f, knockbackForce.x);
            knockbackForce.y = Mathf.Max(0f, knockbackForce.y);
            controlLockDuration = Mathf.Max(0f, controlLockDuration);
        }
#endif
    }
}
