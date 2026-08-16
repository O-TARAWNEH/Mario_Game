// Filename: SfxId.cs
// Folder: Assets/Scripts/Audio/
// Purpose: Identifiers for one-shot gameplay and UI sound effects (Phase 18/31).
// Dependencies: None.

namespace BounderTrail.Audio
{
    /// <summary>
    /// Central SFX catalog keys used by <see cref="SfxSystem"/>.
    /// </summary>
    public enum SfxId
    {
        Jump = 0,
        Land = 1,
        Collect = 2,
        Damage = 3,
        EnemyDefeat = 4,
        PowerUp = 5,
        Death = 6,
        LevelComplete = 7,
        Ui = 8,
        // Phase 31 — distinct power-up feedback (existing clips on disk).
        PowerUpHeart = 9,
        PowerUpShield = 10,
        PowerUpSpeed = 11,
        // Phase 42 — milestone / victory feedback
        BonusLife = 12
    }
}
