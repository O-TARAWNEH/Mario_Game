// Filename: MovingPlatform.cs
// Folder: Assets/Scripts/World/
// Purpose: Simple kinematic platform that moves between two points and carries the player (Phase 8).
// Dependencies: PlatformPiece

using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Moves back and forth and carries a standing player via position delta
    /// (avoids parenting Dynamic Rigidbody2D objects).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlatformPiece))]
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Path")]
        [SerializeField] private Vector2 pointA = new Vector2(-2f, 0f);
        [SerializeField] private Vector2 pointB = new Vector2(2f, 0f);
        [SerializeField] private bool pointsAreLocal = true;
        [SerializeField] private float speed = 2.5f;
        [SerializeField] private float arriveThreshold = 0.05f;

        [Header("Carry")]
        [SerializeField] private string playerTag = "Player";

        [Header("Collision")]
        [SerializeField] private bool ignoreStaticGroundCollisions = true;

        private Rigidbody2D _body;
        private Collider2D _collider;
        private Vector3 _worldA;
        private Vector3 _worldB;
        private Vector3 _target;
        private Rigidbody2D _passenger;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.freezeRotation = true;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _body.useFullKinematicContacts = true;

            var piece = GetComponent<PlatformPiece>();
            if (piece != null)
            {
                piece.SetKind(PlatformPiece.PlatformKind.Moving);
            }

            CacheWorldPoints();
            _target = _worldB;
        }

        private void Start()
        {
            if (ignoreStaticGroundCollisions)
            {
                IgnoreStaticGroundCollisions();
            }
        }

        public void ConfigurePath(Vector2 localA, Vector2 localB, float newSpeed)
        {
            pointA = localA;
            pointB = localB;
            pointsAreLocal = true;
            speed = Mathf.Max(0.01f, newSpeed);
            CacheWorldPoints();
            _target = _worldB;
        }

        private void OnValidate()
        {
            speed = Mathf.Max(0.01f, speed);
            arriveThreshold = Mathf.Max(0.01f, arriveThreshold);
        }

        private void FixedUpdate()
        {
            if (_passenger != null && !IsPassengerStillOnTop(_passenger))
            {
                _passenger = null;
            }

            var start = _body.position;
            var next = Vector2.MoveTowards(start, (Vector2)_target, speed * Time.fixedDeltaTime);
            var delta = next - start;
            _body.MovePosition(next);

            if (_passenger != null)
            {
                // Rigidbody2D.MovePosition has Vector2 semantics; keep everything Vector2 to avoid Vector2+Vector3 ambiguity.
                _passenger.MovePosition(_passenger.position + delta);
                _passenger.linearVelocity = new Vector2(delta.x / Time.fixedDeltaTime, _passenger.linearVelocity.y);
            }

            if (Vector2.Distance(next, (Vector2)_target) <= arriveThreshold)
            {
                _target = AlmostEqual(_target, _worldA) ? _worldB : _worldA;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryAssignPassenger(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryAssignPassenger(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.rigidbody != null && collision.rigidbody == _passenger)
            {
                _passenger = null;
            }
        }

        private void TryAssignPassenger(Collision2D collision)
        {
            if (collision.collider == null || !collision.collider.CompareTag(playerTag))
            {
                return;
            }

            if (!IsStandingOnTop(collision) || collision.rigidbody == null)
            {
                return;
            }

            _passenger = collision.rigidbody;
        }

        private bool IsStandingOnTop(Collision2D collision)
        {
            var contacts = collision.contactCount;
            for (var i = 0; i < contacts; i++)
            {
                if (collision.GetContact(i).normal.y >= 0.5f)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPassengerStillOnTop(Rigidbody2D passenger)
        {
            if (_collider == null || passenger == null)
            {
                return false;
            }

            var passengerCollider = passenger.GetComponent<Collider2D>();
            if (passengerCollider == null)
            {
                return false;
            }

            var platformTop = _collider.bounds.max.y;
            return passengerCollider.bounds.min.y >= platformTop - 0.2f
                   && passengerCollider.bounds.center.x >= _collider.bounds.min.x - 0.1f
                   && passengerCollider.bounds.center.x <= _collider.bounds.max.x + 0.1f;
        }

        private void IgnoreStaticGroundCollisions()
        {
            if (_collider == null)
            {
                return;
            }

            var solids = FindObjectsByType<SolidPlatform>(FindObjectsSortMode.None);
            for (var i = 0; i < solids.Length; i++)
            {
                var other = solids[i].GetComponent<Collider2D>();
                if (other == null || other == _collider)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(_collider, other, true);
            }

            var oneWays = FindObjectsByType<OneWayPlatform>(FindObjectsSortMode.None);
            for (var i = 0; i < oneWays.Length; i++)
            {
                var other = oneWays[i].GetComponent<Collider2D>();
                if (other == null || other == _collider)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(_collider, other, true);
            }
        }

        private void CacheWorldPoints()
        {
            if (pointsAreLocal)
            {
                _worldA = transform.TransformPoint(pointA);
                _worldB = transform.TransformPoint(pointB);
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
                a = transform.TransformPoint(pointA);
                b = transform.TransformPoint(pointB);
            }
            else
            {
                a = pointA;
                b = pointB;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(a, 0.12f);
            Gizmos.DrawSphere(b, 0.12f);
            Gizmos.DrawLine(a, b);
        }
    }
}
