// Filename: LevelBounds.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Defines the playable level rectangle used by the camera (Phase 6).
// Dependencies: None

using UnityEngine;

namespace BounderTrail.Levels
{
    /// <summary>
    /// World-space level boundary used to clamp the camera view.
    /// </summary>
    public class LevelBounds : MonoBehaviour
    {
        [Header("Bounds")]
        [SerializeField] private Vector2 size = new Vector2(40f, 18f);
        [SerializeField] private Vector2 centerOffset = Vector2.zero;
        [SerializeField] private Color gizmoColor = new Color(1f, 0.85f, 0.2f, 0.35f);

        public Vector2 Size => size;
        public Vector2 CenterOffset => centerOffset;

        public Bounds WorldBounds
        {
            get
            {
                var center = (Vector2)transform.position + centerOffset;
                return new Bounds(center, size);
            }
        }

        /// <summary>
        /// Clamps a camera center so the orthographic view stays inside the level bounds.
        /// </summary>
        public Vector3 ClampCameraCenter(Camera camera, Vector3 desiredCenter)
        {
            if (camera == null || !camera.orthographic)
            {
                return desiredCenter;
            }

            var bounds = WorldBounds;
            var halfHeight = camera.orthographicSize;
            var halfWidth = halfHeight * camera.aspect;

            // If the level is smaller than the view, lock to bounds center on that axis.
            float minX;
            float maxX;
            float minY;
            float maxY;

            if (bounds.size.x <= halfWidth * 2f)
            {
                minX = maxX = bounds.center.x;
            }
            else
            {
                minX = bounds.min.x + halfWidth;
                maxX = bounds.max.x - halfWidth;
            }

            if (bounds.size.y <= halfHeight * 2f)
            {
                minY = maxY = bounds.center.y;
            }
            else
            {
                minY = bounds.min.y + halfHeight;
                maxY = bounds.max.y - halfHeight;
            }

            desiredCenter.x = Mathf.Clamp(desiredCenter.x, minX, maxX);
            desiredCenter.y = Mathf.Clamp(desiredCenter.y, minY, maxY);
            desiredCenter.z = transform.position.z; // unused; caller keeps camera z
            return desiredCenter;
        }

        private void OnDrawGizmos()
        {
            var bounds = WorldBounds;
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            var fill = gizmoColor;
            fill.a *= 0.25f;
            Gizmos.color = fill;
            Gizmos.DrawCube(bounds.center, bounds.size);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            size.x = Mathf.Max(1f, size.x);
            size.y = Mathf.Max(1f, size.y);
        }
#endif
    }
}
