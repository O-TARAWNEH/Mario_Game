// Filename: EnemySensor.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Simple player detection for enemies (Phase 9).
// Dependencies: None

using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Detects the player inside an overlap box/circle. Keep ranges small and cheap.
    /// </summary>
    public class EnemySensor : MonoBehaviour
    {
        public enum Shape
        {
            Circle = 0,
            Box = 1
        }

        [Header("Detection")]
        [SerializeField] private Shape shape = Shape.Circle;
        [SerializeField] private float radius = 4f;
        [SerializeField] private Vector2 boxSize = new Vector2(6f, 2.5f);
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private bool requireLineOfSight;

        private readonly Collider2D[] _hits = new Collider2D[8];

        public bool PlayerDetected { get; private set; }
        public Transform DetectedPlayer { get; private set; }

        private void FixedUpdate()
        {
            Evaluate();
        }

        public void Evaluate()
        {
            PlayerDetected = false;
            DetectedPlayer = null;

            var origin = (Vector2)transform.position + offset;
            var count = shape == Shape.Circle
                ? Physics2D.OverlapCircleNonAlloc(origin, radius, _hits, targetLayers)
                : Physics2D.OverlapBoxNonAlloc(origin, boxSize, 0f, _hits, targetLayers);

            for (var i = 0; i < count; i++)
            {
                var hit = _hits[i];
                if (hit == null || !hit.CompareTag(targetTag))
                {
                    continue;
                }

                if (requireLineOfSight && !HasLineOfSight(origin, hit.transform.position))
                {
                    continue;
                }

                PlayerDetected = true;
                DetectedPlayer = hit.transform;
                return;
            }
        }

        private bool HasLineOfSight(Vector2 from, Vector2 to)
        {
            var hit = Physics2D.Linecast(from, to, ~targetLayers);
            return hit.collider == null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = PlayerDetected ? Color.red : new Color(1f, 0.5f, 0.2f, 0.7f);
            var origin = (Vector2)transform.position + offset;
            if (shape == Shape.Circle)
            {
                Gizmos.DrawWireSphere(origin, radius);
            }
            else
            {
                Gizmos.DrawWireCube(origin, boxSize);
            }
        }
    }
}
