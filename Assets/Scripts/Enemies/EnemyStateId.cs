// Filename: EnemyStateId.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Shared enemy state identifiers (Phase 9).

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Flexible enemy states. Not every enemy must use every state.
    /// </summary>
    public enum EnemyStateId
    {
        Idle = 0,
        Patrol = 1,
        Chase = 2,
        Attack = 3,
        Hurt = 4,
        Dead = 5
    }
}
