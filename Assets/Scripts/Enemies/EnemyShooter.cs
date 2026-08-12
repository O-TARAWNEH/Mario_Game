// Filename: EnemyShooter.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Fires projectiles when the player is detected (Phase 10).
// Dependencies: EnemySensor, EnemyProjectile, EnemyBrain

using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Projectile enemy attack module.
    /// </summary>
    public class EnemyShooter : MonoBehaviour
    {
        [SerializeField] private EnemySensor sensor;
        [SerializeField] private EnemyBrain brain;
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireInterval = 1.85f;
        [SerializeField] private float projectileSpeed = 7f;
        [SerializeField] private bool requirePlayerDetected = true;
        [SerializeField] private bool enterAttackStateOnFire = true;

        private float _timer;

        private void Awake()
        {
            if (sensor == null)
            {
                sensor = GetComponent<EnemySensor>();
            }

            if (brain == null)
            {
                brain = GetComponent<EnemyBrain>();
            }

            if (firePoint == null)
            {
                firePoint = transform;
            }

            _timer = fireInterval * 0.5f;
        }

        private void Update()
        {
            if (projectilePrefab == null)
            {
                return;
            }

            if (brain != null && brain.CurrentState == EnemyStateId.Dead)
            {
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer > 0f)
            {
                return;
            }

            if (requirePlayerDetected && (sensor == null || !sensor.PlayerDetected || sensor.DetectedPlayer == null))
            {
                return;
            }

            Fire();
            _timer = fireInterval;
        }

        private void Fire()
        {
            var direction = Vector2.left;
            if (sensor != null && sensor.DetectedPlayer != null)
            {
                direction = ((Vector2)sensor.DetectedPlayer.position - (Vector2)firePoint.position).normalized;
            }
            else if (brain != null)
            {
                // Face-based fallback if mover facing is available through scale/sprite — use player-less left/right from local.
                direction = transform.localScale.x < 0f ? Vector2.left : Vector2.right;
            }

            if (enterAttackStateOnFire && brain != null && brain.CurrentState != EnemyStateId.Dead)
            {
                brain.SetState(EnemyStateId.Attack);
            }

            var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            projectile.Launch(direction, projectileSpeed);
        }
    }
}
