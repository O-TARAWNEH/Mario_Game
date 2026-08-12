// Filename: EnemyContact.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Configurable player-enemy collision rules (Phase 9/11).
// Dependencies: EnemyHealth, BounderTrail.Core.IDamageable, BounderTrail.Player.PlayerHealth

using BounderTrail.Core;
using BounderTrail.Player;
using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Handles contact rules between player and enemy:
    /// stomp from above damages the enemy; side/body hits damage the player.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyContact : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyHealth health;
        [SerializeField] private string playerTag = "Player";

        [Header("Rules")]
        [SerializeField] private bool canBeStomped = true;
        [SerializeField] private bool dealContactDamage = true;
        [SerializeField] private int contactDamageToPlayer = 1;
        [SerializeField] private int stompDamageToEnemy = 1;
        [SerializeField] private float stompBounceForce = 10f;
        [SerializeField] private float stompHeightThreshold = 0.12f;

        public bool CanBeStomped => canBeStomped;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleContact(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            HandleContact(collision);
        }

        private void HandleContact(Collision2D collision)
        {
            if (health == null || !health.IsAlive)
            {
                return;
            }

            if (collision.collider == null || !collision.collider.CompareTag(playerTag))
            {
                return;
            }

            var player = collision.transform;
            var playerBody = collision.rigidbody;
            var playerHealth = collision.collider.GetComponentInParent<PlayerHealth>();

            var isStomp = canBeStomped
                          && player.position.y >= transform.position.y + stompHeightThreshold
                          && playerBody != null
                          && playerBody.linearVelocity.y <= 0.1f;

            if (isStomp)
            {
                // Skip bounce while invulnerable so Stay contact cannot rocket the player.
                if (health.IsInvulnerable)
                {
                    return;
                }

                health.TakeDamage(stompDamageToEnemy, player.position);

                if (playerHealth != null)
                {
                    playerHealth.ApplyStompBounce(stompBounceForce);
                }
                else if (playerBody != null)
                {
                    var velocity = playerBody.linearVelocity;
                    velocity.y = stompBounceForce;
                    playerBody.linearVelocity = velocity;
                }

                return;
            }

            if (!dealContactDamage)
            {
                return;
            }

            var damageable = collision.collider.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(contactDamageToPlayer, transform.position);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            contactDamageToPlayer = Mathf.Max(0, contactDamageToPlayer);
            stompDamageToEnemy = Mathf.Max(1, stompDamageToEnemy);
            stompBounceForce = Mathf.Max(0f, stompBounceForce);
            stompHeightThreshold = Mathf.Max(0f, stompHeightThreshold);
        }
#endif
    }
}
