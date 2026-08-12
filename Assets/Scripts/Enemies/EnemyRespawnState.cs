// Filename: EnemyRespawnState.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Caches spawn pose and restores enemies on checkpoint respawn (Phase 15).
// Dependencies: EnemyHealth, EnemyBrain, EnemyMover

using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Soft-death + restore support so defeated enemies can return after the player respawns.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyRespawnState : MonoBehaviour
    {
        [SerializeField] private EnemyHealth health;
        [SerializeField] private EnemyBrain brain;
        [SerializeField] private EnemyMover mover;
        [SerializeField] private bool softDeath = true;

        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private int _spawnFacing = -1;
        private bool _cached;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }

            if (brain == null)
            {
                brain = GetComponent<EnemyBrain>();
            }

            if (mover == null)
            {
                mover = GetComponent<EnemyMover>();
            }

            CacheSpawn();

            if (softDeath && health != null)
            {
                health.SetDestroyOnDeath(false);
            }
        }

        private void CacheSpawn()
        {
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            if (mover != null)
            {
                _spawnFacing = mover.Facing;
            }

            _cached = true;
        }

        /// <summary>
        /// Restores this enemy to its original spawn state if it was defeated.
        /// </summary>
        public void ResetEnemy()
        {
            if (!_cached)
            {
                CacheSpawn();
            }

            transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (health != null)
            {
                health.Revive();
            }

            if (mover != null)
            {
                mover.SetFacing(_spawnFacing);
                mover.StopHorizontal();
            }

            if (brain != null)
            {
                brain.enabled = true;
                brain.SetState(EnemyStateId.Patrol);
            }
        }
    }
}
