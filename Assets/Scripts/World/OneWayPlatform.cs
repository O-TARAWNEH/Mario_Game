// Filename: OneWayPlatform.cs
// Folder: Assets/Scripts/World/
// Purpose: Platform you can jump up through, but land on from above (Phase 8).
// Dependencies: PlatformPiece

using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// One-way platform using PlatformEffector2D.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlatformEffector2D))]
    [RequireComponent(typeof(PlatformPiece))]
    public class OneWayPlatform : MonoBehaviour
    {
        [SerializeField] private float surfaceArc = 170f;
        [SerializeField] private bool useOneWay = true;

        private void Reset()
        {
            Configure();
        }

        private void Awake()
        {
            Configure();
        }

        private void Configure()
        {
            var piece = GetComponent<PlatformPiece>();
            if (piece != null)
            {
                piece.SetKind(PlatformPiece.PlatformKind.OneWay);
            }

            var col = GetComponent<Collider2D>();
            col.usedByEffector = true;
            col.isTrigger = false;

            var effector = GetComponent<PlatformEffector2D>();
            effector.useOneWay = useOneWay;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = surfaceArc;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
        }
    }
}
