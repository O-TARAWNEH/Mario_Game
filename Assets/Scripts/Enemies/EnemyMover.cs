// Filename: EnemyMover.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Simple horizontal enemy movement helpers (Phase 9).
// Dependencies: None

using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Lightweight mover used by patrol/chase states.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMover : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform wallCheck;
        [SerializeField] private float checkDistance = 0.15f;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private bool flipSprite = true;

        private int _facing = 1;

        public float MoveSpeed => moveSpeed;
        public int Facing => _facing;
        public Vector2 Velocity => rigidBody != null ? rigidBody.linearVelocity : Vector2.zero;

        private void Awake()
        {
            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody2D>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            rigidBody.freezeRotation = true;
            rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void SetFacing(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            _facing = direction > 0 ? 1 : -1;
            if (flipSprite && spriteRenderer != null)
            {
                spriteRenderer.flipX = _facing < 0;
            }
        }

        public void StopHorizontal()
        {
            if (rigidBody == null)
            {
                return;
            }

            var velocity = rigidBody.linearVelocity;
            velocity.x = 0f;
            rigidBody.linearVelocity = velocity;
        }

        public void MoveFacingDirection()
        {
            MoveInDirection(_facing);
        }

        public void MoveToward(Vector2 worldPosition)
        {
            var dir = worldPosition.x >= transform.position.x ? 1 : -1;
            SetFacing(dir);
            MoveInDirection(dir);
        }

        public void MoveInDirection(int direction)
        {
            if (rigidBody == null || direction == 0)
            {
                return;
            }

            SetFacing(direction);
            var velocity = rigidBody.linearVelocity;
            velocity.x = _facing * moveSpeed;
            rigidBody.linearVelocity = velocity;
        }

        public bool IsGroundAhead()
        {
            var origin = GetFacingLocalPoint(groundCheck, new Vector2(0.35f, -0.4f));
            var hit = Physics2D.Raycast(origin, Vector2.down, checkDistance, groundLayers);
            return hit.collider != null;
        }

        public bool IsWallAhead()
        {
            var origin = GetFacingLocalPoint(wallCheck, new Vector2(0.45f, 0f));
            var hit = Physics2D.Raycast(origin, Vector2.right * _facing, checkDistance, groundLayers);
            return hit.collider != null;
        }

        private Vector2 GetFacingLocalPoint(Transform point, Vector2 fallbackLocal)
        {
            var local = point != null ? (Vector2)point.localPosition : fallbackLocal;
            local.x = Mathf.Abs(local.x) * _facing;
            return (Vector2)transform.position + local;
        }

        public void Flip()
        {
            SetFacing(-_facing);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var groundOrigin = GetFacingLocalPoint(groundCheck, new Vector2(0.35f, -0.4f));
            var wallOrigin = GetFacingLocalPoint(wallCheck, new Vector2(0.45f, 0f));
            Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * checkDistance);
            Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * _facing * checkDistance);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);
            checkDistance = Mathf.Max(0.01f, checkDistance);
        }
#endif
    }
}
