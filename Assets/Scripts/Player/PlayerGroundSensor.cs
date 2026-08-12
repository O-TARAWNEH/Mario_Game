// Filename: PlayerGroundSensor.cs
// Folder: Assets/Scripts/Player/
// Purpose: Improved ground, edge, and light slope detection (Phase 4).
// Dependencies: BounderTrail.Core.GameLog

using BounderTrail.Core;
using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Multi-probe ground sensor for reliable grounding near edges and mild slopes.
    /// </summary>
    public class PlayerGroundSensor : MonoBehaviour
    {
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float checkRadius = 0.12f;
        [SerializeField] private float probeDistance = 0.18f;
        [SerializeField] private float edgeProbeOffset = 0.28f;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float maxSlopeAngle = 50f;
        [SerializeField] private bool drawGizmos = true;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[4];

        public bool IsGrounded { get; private set; }
        public Vector2 GroundNormal { get; private set; } = Vector2.up;
        public float GroundAngle { get; private set; }

        private void Awake()
        {
            if (groundCheckPoint == null)
            {
                GameLog.Warning("Player", "PlayerGroundSensor is missing a groundCheckPoint.");
            }
        }

        private void FixedUpdate()
        {
            EvaluateGround();
        }

        private void EvaluateGround()
        {
            if (groundCheckPoint == null)
            {
                IsGrounded = false;
                GroundNormal = Vector2.up;
                GroundAngle = 0f;
                return;
            }

            var origin = groundCheckPoint.position;
            var bestNormal = Vector2.up;
            var found = false;
            var bestDistance = float.MaxValue;

            // Center + left + right probes improve edge reliability.
            TryProbe(origin, ref found, ref bestDistance, ref bestNormal);
            TryProbe(origin + Vector3.left * edgeProbeOffset, ref found, ref bestDistance, ref bestNormal);
            TryProbe(origin + Vector3.right * edgeProbeOffset, ref found, ref bestDistance, ref bestNormal);

            // Overlap fallback catches very short steps / flat contacts.
            if (!found && Physics2D.OverlapCircle(origin, checkRadius, groundLayers) != null)
            {
                found = true;
                bestNormal = Vector2.up;
            }

            if (found)
            {
                var angle = Vector2.Angle(bestNormal, Vector2.up);
                if (angle <= maxSlopeAngle)
                {
                    IsGrounded = true;
                    GroundNormal = bestNormal;
                    GroundAngle = angle;
                    return;
                }
            }

            IsGrounded = false;
            GroundNormal = Vector2.up;
            GroundAngle = 0f;
        }

        private void TryProbe(Vector3 origin, ref bool found, ref float bestDistance, ref Vector2 bestNormal)
        {
            var count = Physics2D.CircleCastNonAlloc(
                origin,
                checkRadius * 0.85f,
                Vector2.down,
                _hits,
                probeDistance,
                groundLayers);

            for (var i = 0; i < count; i++)
            {
                var hit = _hits[i];
                if (hit.collider == null)
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestNormal = hit.normal;
                    found = true;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || groundCheckPoint == null)
            {
                return;
            }

            Gizmos.color = IsGrounded ? Color.green : Color.yellow;
            DrawProbeGizmo(groundCheckPoint.position);
            DrawProbeGizmo(groundCheckPoint.position + Vector3.left * edgeProbeOffset);
            DrawProbeGizmo(groundCheckPoint.position + Vector3.right * edgeProbeOffset);
        }

        private void DrawProbeGizmo(Vector3 origin)
        {
            Gizmos.DrawWireSphere(origin, checkRadius * 0.85f);
            Gizmos.DrawLine(origin, origin + Vector3.down * probeDistance);
        }
    }
}
