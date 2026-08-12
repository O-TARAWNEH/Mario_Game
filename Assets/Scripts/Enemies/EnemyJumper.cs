// Filename: EnemyJumper.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Periodic jump behavior for hopping enemies (Phase 10).
// Dependencies: EnemyBrain, EnemyMover, Rigidbody2D

using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Makes an enemy hop at intervals while idle/patrol/chase.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyBrain))]
    public class EnemyJumper : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private EnemyBrain brain;
        [SerializeField] private float jumpForce = 9f;
        [SerializeField] private float jumpInterval = 1.45f;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.12f;

        private float _timer;

        private void Awake()
        {
            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody2D>();
            }

            if (brain == null)
            {
                brain = GetComponent<EnemyBrain>();
            }

            _timer = jumpInterval * 0.5f;
        }

        private void FixedUpdate()
        {
            if (brain != null && brain.CurrentState == EnemyStateId.Dead)
            {
                return;
            }

            _timer -= Time.fixedDeltaTime;
            if (_timer > 0f || !IsGrounded())
            {
                return;
            }

            var state = brain != null ? brain.CurrentState : EnemyStateId.Patrol;
            if (state == EnemyStateId.Hurt || state == EnemyStateId.Attack || state == EnemyStateId.Dead)
            {
                return;
            }

            var velocity = rigidBody.linearVelocity;
            velocity.y = jumpForce;
            rigidBody.linearVelocity = velocity;
            _timer = jumpInterval;
        }

        private bool IsGrounded()
        {
            var origin = groundCheck != null ? groundCheck.position : transform.position + Vector3.down * 0.4f;
            return Physics2D.OverlapCircle(origin, groundCheckRadius, groundLayers) != null;
        }
    }
}
