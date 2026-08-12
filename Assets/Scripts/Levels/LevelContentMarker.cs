// Filename: LevelContentMarker.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Placeholder marker for enemies/collectibles/hazards/checkpoints/decor (Phase 7).
// Dependencies: LevelContentMarkerType

using UnityEngine;

namespace BounderTrail.Levels
{
    /// <summary>
    /// Marks a level slot for future content without implementing that content yet.
    /// </summary>
    public class LevelContentMarker : MonoBehaviour
    {
        [SerializeField] private LevelContentMarkerType markerType = LevelContentMarkerType.Custom;
        [SerializeField] private string contentId = "";
        [SerializeField] private Color gizmoColor = Color.magenta;

        public LevelContentMarkerType MarkerType => markerType;
        public string ContentId => contentId;

        private void OnDrawGizmos()
        {
            Gizmos.color = GetColor();
            Gizmos.DrawWireSphere(transform.position, 0.25f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.5f);
        }

        private Color GetColor()
        {
            switch (markerType)
            {
                case LevelContentMarkerType.Enemy: return new Color(1f, 0.35f, 0.35f, 0.9f);
                case LevelContentMarkerType.Collectible: return new Color(1f, 0.85f, 0.2f, 0.9f);
                case LevelContentMarkerType.Hazard: return new Color(1f, 0.45f, 0.1f, 0.9f);
                case LevelContentMarkerType.Checkpoint: return new Color(0.3f, 1f, 0.55f, 0.9f);
                case LevelContentMarkerType.Decoration: return new Color(0.6f, 0.8f, 1f, 0.9f);
                default: return gizmoColor;
            }
        }
    }
}
