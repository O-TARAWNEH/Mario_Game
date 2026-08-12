// Filename: EnemyFlyer.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Flying enemy movement helper (Phase 10).
// Dependencies: EnemyMover, Rigidbody2D

using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Keeps the enemy airborne and optionally bobs vertically while patrol/chase move horizontally.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyMover))]
    public class EnemyFlyer : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private EnemyMover mover;
        [SerializeField] private float hoverGravityScale = 0f;
        [SerializeField] private bool enableBob = true;
        [SerializeField] private float bobAmplitude = 0.35f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private bool autoFlip = true;
        [SerializeField] private float flipInterval = 2.5f;

        private float _originY;
        private float _bobTimer;
        private float _flipTimer;

        private void Awake()
        {
            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody2D>();
            }

            if (mover == null)
            {
                mover = GetComponent<EnemyMover>();
            }

            rigidBody.gravityScale = hoverGravityScale;
            rigidBody.freezeRotation = true;
            _originY = transform.position.y;
            _flipTimer = flipInterval;
        }

        private void FixedUpdate()
        {
            if (rigidBody == null)
            {
                return;
            }

            rigidBody.gravityScale = hoverGravityScale;

            if (autoFlip && mover != null)
            {
                _flipTimer -= Time.fixedDeltaTime;
                if (_flipTimer <= 0f)
                {
                    mover.Flip();
                    _flipTimer = flipInterval;
                }
            }

            if (!enableBob)
            {
                return;
            }

            _bobTimer += Time.fixedDeltaTime * bobSpeed;
            var targetY = _originY + Mathf.Sin(_bobTimer) * bobAmplitude;
            var velocity = rigidBody.linearVelocity;
            velocity.y = (targetY - rigidBody.position.y) / Time.fixedDeltaTime;
            rigidBody.linearVelocity = velocity;
        }
    }
}
