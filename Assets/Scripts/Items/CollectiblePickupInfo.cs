// Filename: CollectiblePickupInfo.cs
// Folder: Assets/Scripts/Items/
// Purpose: Immutable payload broadcast when a collectible is picked up (Phase 12).
// Dependencies: CollectibleKind

using UnityEngine;

namespace BounderTrail.Items
{
    /// <summary>
    /// Data sent to the counter / listeners when a collectible is collected.
    /// </summary>
    public readonly struct CollectiblePickupInfo
    {
        public CollectibleKind Kind { get; }
        public int CoinValue { get; }
        public int ScoreValue { get; }
        public Vector2 WorldPosition { get; }
        public string SourceName { get; }

        public CollectiblePickupInfo(
            CollectibleKind kind,
            int coinValue,
            int scoreValue,
            Vector2 worldPosition,
            string sourceName)
        {
            Kind = kind;
            CoinValue = coinValue;
            ScoreValue = scoreValue;
            WorldPosition = worldPosition;
            SourceName = sourceName;
        }
    }
}
