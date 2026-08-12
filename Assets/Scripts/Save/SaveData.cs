// Filename: SaveData.cs
// Folder: Assets/Scripts/Save/
// Purpose: Serializable campaign + settings payload for the save system (Phase 21).
// Dependencies: None

using System;

namespace BounderTrail.Save
{
    /// <summary>
    /// Versioned save payload. Checksum is computed over the other fields.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string checksum = string.Empty;

        // Campaign / progression
        public bool hasCampaignSave;
        public int continueLevelIndex;
        public int highestUnlockedLevelIndex;
        public int completedMask;

        // Collectible career progress
        public int bestScore;
        public int bestCoins;

        // Settings
        public float masterVolume = 1f;
        public float musicVolume = 0.55f;
        public float sfxVolume = 0.85f;

        public static SaveData CreateDefaults()
        {
            return new SaveData
            {
                version = CurrentVersion,
                hasCampaignSave = false,
                continueLevelIndex = 0,
                highestUnlockedLevelIndex = 0,
                completedMask = 0,
                bestScore = 0,
                bestCoins = 0,
                masterVolume = 1f,
                musicVolume = 0.55f,
                sfxVolume = 0.85f,
                checksum = string.Empty
            };
        }

        public void Clamp()
        {
            version = Math.Max(1, version);
            continueLevelIndex = Math.Max(0, continueLevelIndex);
            highestUnlockedLevelIndex = Math.Max(0, highestUnlockedLevelIndex);
            completedMask = Math.Max(0, completedMask);
            bestScore = Math.Max(0, bestScore);
            bestCoins = Math.Max(0, bestCoins);
            masterVolume = Clamp01(masterVolume);
            musicVolume = Clamp01(musicVolume);
            sfxVolume = Clamp01(sfxVolume);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }
    }
}
