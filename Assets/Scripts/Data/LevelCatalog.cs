// Filename: LevelCatalog.cs
// Folder: Assets/Scripts/Data/
// Purpose: Ordered list of levels for loading/progression foundation (Phase 7).
// Dependencies: LevelData

using UnityEngine;

namespace BounderTrail.Data
{
    /// <summary>
    /// Catalog of LevelData assets. Used by LevelLoader.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Bounder Trail/Level Catalog", order = 1)]
    public class LevelCatalog : ScriptableObject
    {
        [SerializeField] private LevelData[] levels;

        public int Count => levels != null ? levels.Length : 0;

        public bool IsValidIndex(int index)
        {
            return levels != null && index >= 0 && index < levels.Length && levels[index] != null;
        }

        public LevelData GetLevel(int index)
        {
            if (!IsValidIndex(index))
            {
                return null;
            }

            return levels[index];
        }

        public LevelData GetLevelById(string levelId)
        {
            if (levels == null || string.IsNullOrWhiteSpace(levelId))
            {
                return null;
            }

            for (var i = 0; i < levels.Length; i++)
            {
                if (levels[i] != null && levels[i].LevelId == levelId)
                {
                    return levels[i];
                }
            }

            return null;
        }

        public int IndexOf(LevelData data)
        {
            if (levels == null || data == null)
            {
                return -1;
            }

            for (var i = 0; i < levels.Length; i++)
            {
                if (levels[i] == data)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
