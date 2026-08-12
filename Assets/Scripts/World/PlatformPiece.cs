// Filename: PlatformPiece.cs
// Folder: Assets/Scripts/World/
// Purpose: Identifies platform/ground pieces in a level (Phase 7/8).
// Dependencies: None

using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Marks a collider object as level platform/ground geometry.
    /// </summary>
    public class PlatformPiece : MonoBehaviour
    {
        public enum PlatformKind
        {
            Ground = 0,
            Solid = 1,
            OneWay = 2,
            Moving = 3
        }

        [SerializeField] private PlatformKind kind = PlatformKind.Solid;

        public PlatformKind Kind => kind;

        public void SetKind(PlatformKind newKind)
        {
            kind = newKind;
        }
    }
}
