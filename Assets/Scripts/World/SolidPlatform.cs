// Filename: SolidPlatform.cs
// Folder: Assets/Scripts/World/
// Purpose: Reusable solid platform/ground piece (Phase 8).
// Dependencies: PlatformPiece

using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Fully solid platform. Blocks from all sides.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlatformPiece))]
    public class SolidPlatform : MonoBehaviour
    {
        private void Reset()
        {
            var piece = GetComponent<PlatformPiece>();
            if (piece != null)
            {
                piece.SetKind(PlatformPiece.PlatformKind.Solid);
            }

            var col = GetComponent<Collider2D>();
            col.isTrigger = false;
        }

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = false;
            }
        }
    }
}
