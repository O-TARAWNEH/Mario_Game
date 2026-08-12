// Filename: IDamageable.cs
// Folder: Assets/Scripts/Core/
// Purpose: Shared damage interface for player and enemies (Phase 9).
// Dependencies: None

using UnityEngine;

namespace BounderTrail.Core
{
    /// <summary>
    /// Minimal damage contract used by combat/contact systems.
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(int amount, Vector2 hitFrom);
    }
}
