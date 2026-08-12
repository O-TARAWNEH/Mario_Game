// Filename: MovingHazard.cs
// Folder: Assets/Scripts/World/
// Purpose: Kinematic back-and-forth mover for damaging hazards (Phase 14).
// Dependencies: EnvironmentalHazard

using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Moves a hazard between two points. Resets to the start pose on enable.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnvironmentalHazard))]
    public class MovingHazard : MonoBehaviour
    {
        [Header("Path")]
        [SerializeField] private Vector2 pointA = new Vector2(-1.5f, 0f);
        [SerializeField] private Vector2 pointB = new Vector2(1.5f, 0f);
        [SerializeField] private bool pointsAreLocal = true;
        [SerializeField] private float speed = 2.5f;
        [SerializeField] private float arriveThreshold = 0.05f;
        [SerializeField] private bool startAtPointA = true;

        private Rigidbody2D _body;
        private EnvironmentalHazard _hazard;
        private Vector3 _worldA;
        private Vector3 _worldB;
        private Vector3 _target;
        private Vector3 _spawnPosition;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _hazard = GetComponent<EnvironmentalHazard>();
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.freezeRotation = true;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _spawnPosition = transform.position;
            CacheWorldPoints();
        }

        private void OnEnable()
        {
            ResetMovingHazard();
        }

        private void FixedUpdate()
        {
            if (_body == null)
            {
                return;
            }

            var start = _body.position;
            var next = Vector2.MoveTowards(start, (Vector2)_target, speed * Time.fixedDeltaTime);
            _body.MovePosition(next);

            if (Vector2.Distance(next, (Vector2)_target) <= arriveThreshold)
            {
                _target = AlmostEqual(_target, _worldA) ? _worldB : _worldA;
            }
        }

        /// <summary>
        /// Returns the hazard to its spawn pose and clears contact timers.
        /// </summary>
        public void ResetMovingHazard()
        {
            CacheWorldPoints();
            var start = startAtPointA ? _worldA : _worldB;
            if (_body != null)
            {
                _body.position = start;
            }
            else
            {
                transform.position = start;
            }

            _target = startAtPointA ? _worldB : _worldA;

            if (_hazard != null)
            {
                _hazard.ResetHazardState();
            }
        }

        private void CacheWorldPoints()
        {
            // Rebuild from current transform so local paths stay consistent after scene load.
            if (pointsAreLocal)
            {
                var origin = _spawnPosition;
                _worldA = origin + (Vector3)pointA;
                _worldB = origin + (Vector3)pointB;
            }
            else
            {
                _worldA = pointA;
                _worldB = pointB;
            }
        }

        private static bool AlmostEqual(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) <= 0.01f;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 a;
            Vector3 b;
            if (pointsAreLocal)
            {
                a = transform.position + (Vector3)pointA;
                b = transform.position + (Vector3)pointB;
            }
            else
            {
                a = pointA;
                b = pointB;
            }

            Gizmos.color = new Color(1f, 0.4f, 0.15f, 0.9f);
            Gizmos.DrawSphere(a, 0.1f);
            Gizmos.DrawSphere(b, 0.1f);
            Gizmos.DrawLine(a, b);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            speed = Mathf.Max(0.01f, speed);
            arriveThreshold = Mathf.Max(0.01f, arriveThreshold);
        }
#endif
    }
}
