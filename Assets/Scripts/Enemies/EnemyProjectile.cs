// Filename: EnemyProjectile.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Simple enemy projectile that damages the player (Phase 10).
// Dependencies: BounderTrail.Core.IDamageable

using BounderTrail.Core;
using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Moves in a direction and damages IDamageable targets on trigger enter.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 7f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private int damage = 1;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private LayerMask destroyOnLayers;

        private Vector2 _direction = Vector2.left;
        private float _life;

        public void Launch(Vector2 direction, float overrideSpeed = -1f)
        {
            _direction = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.left;
            if (overrideSpeed > 0f)
            {
                speed = overrideSpeed;
            }

            _life = lifetime;
        }

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);
            _life -= Time.deltaTime;
            if (_life <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null)
            {
                return;
            }

            if (((1 << other.gameObject.layer) & destroyOnLayers) != 0 && !other.CompareTag(playerTag))
            {
                Destroy(gameObject);
                return;
            }

            if (!other.CompareTag(playerTag))
            {
                return;
            }

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(damage, transform.position);
            }

            Destroy(gameObject);
        }
    }
}
