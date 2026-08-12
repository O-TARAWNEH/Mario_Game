// Filename: GameStateId.cs
// Folder: Assets/Scripts/Core/
// Purpose: Identifiers for the core game lifecycle states (Phase 2).
// Dependencies: None.

namespace BounderTrail.Core
{
    /// <summary>
    /// High-level game lifecycle states.
    /// </summary>
    public enum GameStateId
    {
        Boot = 0,
        MainMenu = 1,
        Gameplay = 2,
        Pause = 3,
        GameOver = 4,
        LevelComplete = 5
    }
}
