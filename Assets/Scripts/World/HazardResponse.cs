// Filename: HazardResponse.cs
// Folder: Assets/Scripts/World/
// Purpose: How an environmental hazard affects the player (Phase 14).
// Dependencies: None

namespace BounderTrail.World
{
    /// <summary>
    /// Consequence applied when the player contacts a hazard.
    /// </summary>
    public enum HazardResponse
    {
        /// <summary>Pit / void — kills immediately (bypasses temporary invincibility).</summary>
        InstantKill = 0,

        /// <summary>Spikes — one damage hit on contact (respects i-frames / Glow Shield).</summary>
        ContactDamage = 1,

        /// <summary>Fire — damage while overlapping, on an interval.</summary>
        DamageOverTime = 2
    }
}
