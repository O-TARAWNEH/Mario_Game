// Filename: CameraFollow2D.cs
// Folder: Assets/Scripts/Camera/
// Purpose: Smooth 2D side-scrolling camera with dead zones and level bounds (Phase 6).
// Dependencies: BounderTrail.Levels.LevelBounds, BounderTrail.Core.GameLog

using BounderTrail.Core;
using BounderTrail.Levels;
using UnityEngine;

namespace BounderTrail.CameraSystem
{
    /// <summary>
    /// Follows the player smoothly within a dead zone and clamps to level bounds.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 focusOffset = new Vector2(0f, 0.5f);
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private bool autoFindTarget = true;

        [Header("Smoothing")]
        [SerializeField] private float smoothTimeX = 0.12f;
        [SerializeField] private float smoothTimeY = 0.18f;
        [SerializeField] private float maxSpeed = 40f;

        [Header("Dead Zone")]
        [SerializeField] private bool useDeadZone = true;
        [SerializeField] private Vector2 deadZoneSize = new Vector2(1.8f, 1.1f);

        [Header("Bounds")]
        [SerializeField] private LevelBounds levelBounds;
        [SerializeField] private bool clampToBounds = true;

        [Header("Juice")]
        [SerializeField] private CameraShake2D cameraShake;

        private Camera _camera;
        private float _velocityX;
        private float _velocityY;
        private bool _loggedMissingTarget;
        private Vector2 _appliedShakeOffset;

        public Transform Target => target;
        public LevelBounds LevelBounds => levelBounds;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera != null)
            {
                _camera.orthographic = true;
            }

            if (cameraShake == null)
            {
                cameraShake = GetComponent<CameraShake2D>();
            }
        }

        private Camera Cam
        {
            get
            {
                if (_camera == null)
                {
                    _camera = GetComponent<Camera>();
                }

                return _camera;
            }
        }

        private void Start()
        {
            TryResolveTarget();
            TryResolveBounds();

            if (target != null)
            {
                SnapToTarget();
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                TryResolveTarget();
                if (target == null)
                {
                    if (!_loggedMissingTarget)
                    {
                        GameLog.Warning("Camera", "CameraFollow2D has no target.");
                        _loggedMissingTarget = true;
                    }

                    return;
                }
            }

            if (levelBounds == null && clampToBounds)
            {
                TryResolveBounds();
            }

            // Smooth from the unshaken pose so juice never drifts the follow target.
            var current = transform.position;
            current.x -= _appliedShakeOffset.x;
            current.y -= _appliedShakeOffset.y;

            var desired = GetDesiredCameraPosition(unshakenCurrent: (Vector2)current);

            var nextX = Mathf.SmoothDamp(current.x, desired.x, ref _velocityX, smoothTimeX, maxSpeed, Time.deltaTime);
            var nextY = Mathf.SmoothDamp(current.y, desired.y, ref _velocityY, smoothTimeY, maxSpeed, Time.deltaTime);
            var next = new Vector3(nextX, nextY, current.z);

            if (clampToBounds && levelBounds != null && Cam != null)
            {
                var clamped = levelBounds.ClampCameraCenter(Cam, next);
                next.x = clamped.x;
                next.y = clamped.y;
            }

            if (cameraShake == null)
            {
                cameraShake = GetComponent<CameraShake2D>();
            }

            _appliedShakeOffset = cameraShake != null ? cameraShake.CurrentOffset : Vector2.zero;
            next.x += _appliedShakeOffset.x;
            next.y += _appliedShakeOffset.y;
            transform.position = next;
        }

        /// <summary>
        /// Instantly places the camera on the target (used on level start).
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            _velocityX = 0f;
            _velocityY = 0f;
            _appliedShakeOffset = Vector2.zero;
            if (cameraShake != null)
            {
                cameraShake.StopShake();
            }

            var desired = GetDesiredCameraPosition(ignoreDeadZone: true, unshakenCurrent: (Vector2)transform.position);
            if (clampToBounds && levelBounds != null && Cam != null)
            {
                var clamped = levelBounds.ClampCameraCenter(Cam, desired);
                desired.x = clamped.x;
                desired.y = clamped.y;
            }

            desired.z = transform.position.z;
            transform.position = desired;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            _loggedMissingTarget = false;
        }

        public void SetLevelBounds(LevelBounds bounds)
        {
            levelBounds = bounds;
        }

        private Vector3 GetDesiredCameraPosition(bool ignoreDeadZone = false, Vector2? unshakenCurrent = null)
        {
            var focus = (Vector2)target.position + focusOffset;
            // Dead-zone must use the unshaken camera pose or shake permanently biases follow.
            var current = unshakenCurrent ?? ((Vector2)transform.position - _appliedShakeOffset);
            var desired = current;

            if (!useDeadZone || ignoreDeadZone)
            {
                desired = focus;
            }
            else
            {
                var delta = focus - current;
                var half = deadZoneSize * 0.5f;

                if (delta.x > half.x)
                {
                    desired.x = focus.x - half.x;
                }
                else if (delta.x < -half.x)
                {
                    desired.x = focus.x + half.x;
                }

                if (delta.y > half.y)
                {
                    desired.y = focus.y - half.y;
                }
                else if (delta.y < -half.y)
                {
                    desired.y = focus.y + half.y;
                }
            }

            return new Vector3(desired.x, desired.y, transform.position.z);
        }

        private void TryResolveTarget()
        {
            if (target != null || !autoFindTarget)
            {
                return;
            }

            var player = GameObject.FindGameObjectWithTag(targetTag);
            if (player != null)
            {
                target = player.transform;
                _loggedMissingTarget = false;
            }
        }

        private void TryResolveBounds()
        {
            if (levelBounds != null)
            {
                return;
            }

            levelBounds = FindFirstObjectByType<LevelBounds>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            smoothTimeX = Mathf.Max(0.01f, smoothTimeX);
            smoothTimeY = Mathf.Max(0.01f, smoothTimeY);
            maxSpeed = Mathf.Max(0.1f, maxSpeed);
            deadZoneSize.x = Mathf.Max(0f, deadZoneSize.x);
            deadZoneSize.y = Mathf.Max(0f, deadZoneSize.y);
        }

        private void OnDrawGizmosSelected()
        {
            if (!useDeadZone)
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireCube(transform.position, deadZoneSize);
        }
#endif
    }
}
