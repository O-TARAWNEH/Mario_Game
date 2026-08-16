// Filename: ApprovedGameplayCatalog.cs
// Folder: Assets/Scripts/Data/
// Purpose: Explicit list of design-approved gameplay systems (Phases 22–23).
// Dependencies: None

namespace BounderTrail.Data
{
    /// <summary>
    /// Catalog of mechanics locked by the Game Design Specification.
    /// Phase 22–23 use this to validate readiness — not to invent new features.
    /// </summary>
    public static class ApprovedGameplayCatalog
    {
        /// <summary>Design-spec player abilities and temporary power-ups.</summary>
        public static readonly string[] PlayerSystems =
        {
            "Move / Run / Jump",
            "Damage / Lives / Checkpoint respawn",
            "Speed Burst (temporary)",
            "Glow Shield (temporary)",
            "Heart Drop (heal 1 HP)"
        };

        /// <summary>Design-spec enemy roles (Crawlbug / Hopmite / Spikewatch + Phase 10 variants).</summary>
        public static readonly string[] EnemySystems =
        {
            "Patrol walkers (Crawlbug / Dartling)",
            "Jumping patrol (Hopmite)",
            "Flying patrol (Skimmer)",
            "Stationary hazard foe (Spikewatch)",
            "Stationary shooter (Spitter)"
        };

        /// <summary>Design-spec world / hazard / platform elements.</summary>
        public static readonly string[] WorldSystems =
        {
            "Solid platforms",
            "One-way platforms",
            "Moving platforms",
            "Bounce pads",
            "Death zones (pits)",
            "Static spikes",
            "Fire / ember zones",
            "Moving spike hazards",
            "Timed platforms (blink on/off)",
            "Pressure switches",
            "Gate barriers (switch-opened)",
            "Checkpoints",
            "Level exit door / goal"
        };

        /// <summary>Design-spec collectibles.</summary>
        public static readonly string[] CollectibleSystems =
        {
            "Coins (score)"
        };

        /// <summary>
        /// Explicitly rejected — present in phase prompts' "possible" lists
        /// but not approved in the Game Design Specification.
        /// </summary>
        public static readonly string[] RejectedSystems =
        {
            "Secret areas",
            "Hidden collectibles (beyond coins)",
            "Water / swimming",
            "Special movement (wall-jump, dash, etc.)",
            "Extra temporary abilities beyond Speed Burst / Glow Shield / Heart Drop",
            // Phase 23 — boss encounters not required by design
            "Boss architecture / encounters",
            "Boss health / phases / arena",
            "Boss-gated level completion"
        };
    }
}
