// Filename: MusicId.cs
// Folder: Assets/Scripts/Audio/
// Purpose: Identifiers for looping background music contexts (Phase 18/42).
// Dependencies: None.

namespace BounderTrail.Audio
{
    /// <summary>
    /// Music contexts switched by <see cref="MusicSystem"/>.
    /// </summary>
    public enum MusicId
    {
        None = 0,
        Menu = 1,
        Gameplay = 2,
        Victory = 3
    }
}
