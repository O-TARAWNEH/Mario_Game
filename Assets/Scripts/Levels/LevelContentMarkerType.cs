// Filename: LevelContentMarkerType.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Marker categories supported by the level structure (Phase 7).

namespace BounderTrail.Levels
{
    /// <summary>
    /// Content types a level can host. Markers reserve space for later systems.
    /// </summary>
    public enum LevelContentMarkerType
    {
        Enemy = 0,
        Collectible = 1,
        Hazard = 2,
        Checkpoint = 3,
        Decoration = 4,
        Custom = 5
    }
}
