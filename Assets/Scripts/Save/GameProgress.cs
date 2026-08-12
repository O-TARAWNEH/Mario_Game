// Filename: GameProgress.cs
// Folder: Assets/Scripts/Save/
// Purpose: In-memory campaign unlock/completion state persisted by SaveSystem (Phase 19–21).
// Dependencies: SaveSystem, BounderTrail.Core.GameLog, BounderTrail.Items.CollectibleCounter

using BounderTrail.Core;
using BounderTrail.Items;
using BounderTrail.Levels;
using UnityEngine;

namespace BounderTrail.Save
{
    /// <summary>
    /// Runtime campaign progress. Persistence is owned by <see cref="SaveSystem"/>.
    /// </summary>
    public class GameProgress : MonoBehaviour
    {
        public static GameProgress Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool logProgress = true;

        private bool _hasCampaignSave;
        private int _continueLevelIndex;
        private int _highestUnlockedLevelIndex;
        private int _completedMask;
        private int _bestScore;
        private int _bestCoins;

        public bool CanContinue => _hasCampaignSave;
        public int ContinueLevelIndex => _continueLevelIndex;
        public int HighestUnlockedLevelIndex => _highestUnlockedLevelIndex;
        public int BestScore => _bestScore;
        public int BestCoins => _bestCoins;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLog.Warning("Save", "Duplicate GameProgress destroyed.");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ApplyFromSave(SaveData data)
        {
            if (data == null)
            {
                data = SaveData.CreateDefaults();
            }

            _hasCampaignSave = data.hasCampaignSave;
            _continueLevelIndex = Mathf.Max(0, data.continueLevelIndex);
            _highestUnlockedLevelIndex = Mathf.Max(0, data.highestUnlockedLevelIndex);
            _completedMask = Mathf.Max(0, data.completedMask);
            _bestScore = Mathf.Max(0, data.bestScore);
            _bestCoins = Mathf.Max(0, data.bestCoins);
        }

        public void CopyToSave(SaveData data)
        {
            if (data == null)
            {
                return;
            }

            data.hasCampaignSave = _hasCampaignSave;
            data.continueLevelIndex = _continueLevelIndex;
            data.highestUnlockedLevelIndex = _highestUnlockedLevelIndex;
            data.completedMask = _completedMask;
            data.bestScore = _bestScore;
            data.bestCoins = _bestCoins;
        }

        /// <summary>
        /// Starts a fresh campaign from level 0 (settings/career bests kept via SaveSystem).
        /// </summary>
        public void StartNewGame()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.NewGame();
                return;
            }

            _hasCampaignSave = true;
            _continueLevelIndex = 0;
            _highestUnlockedLevelIndex = 0;
            _completedMask = 0;
            Log("New game progress created (level 0 unlocked).");
        }

        public bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex < 0)
            {
                return false;
            }

            if (levelIndex == 0)
            {
                return true;
            }

            return levelIndex <= _highestUnlockedLevelIndex;
        }

        public bool IsLevelCompleted(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex > 30)
            {
                return false;
            }

            return (_completedMask & (1 << levelIndex)) != 0;
        }

        public void SetContinueLevel(int levelIndex)
        {
            levelIndex = Mathf.Max(0, levelIndex);
            _continueLevelIndex = levelIndex;
            if (levelIndex > _highestUnlockedLevelIndex)
            {
                _highestUnlockedLevelIndex = levelIndex;
            }

            _hasCampaignSave = true;
            Persist();
            Log($"Continue level set to {levelIndex}.");
        }

        public void UnlockLevel(int levelIndex)
        {
            levelIndex = Mathf.Max(0, levelIndex);
            if (levelIndex <= _highestUnlockedLevelIndex)
            {
                return;
            }

            _highestUnlockedLevelIndex = levelIndex;
            _hasCampaignSave = true;
            Persist();
            Log($"Unlocked level index {levelIndex}.");
        }

        public void RegisterLevelCompleted(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex > 30)
            {
                return;
            }

            _completedMask |= 1 << levelIndex;
            _hasCampaignSave = true;

            // Continue resumes at the next level; if this was the last, stay on it.
            if (LevelLoader.Instance != null
                && LevelLoader.Instance.CurrentLevelIndex == levelIndex
                && !LevelLoader.Instance.HasNextLevel)
            {
                _continueLevelIndex = levelIndex;
            }
            else
            {
                _continueLevelIndex = levelIndex + 1;
            }

            if (levelIndex + 1 > _highestUnlockedLevelIndex)
            {
                _highestUnlockedLevelIndex = levelIndex + 1;
            }

            if (CollectibleCounter.Instance != null)
            {
                if (CollectibleCounter.Instance.Score > _bestScore)
                {
                    _bestScore = CollectibleCounter.Instance.Score;
                }

                if (CollectibleCounter.Instance.CoinCount > _bestCoins)
                {
                    _bestCoins = CollectibleCounter.Instance.CoinCount;
                }
            }

            Persist();
            Log($"Level {levelIndex} marked complete; unlocked {levelIndex + 1}.");
        }

        private void Persist()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Save();
            }
        }

        private void Log(string message)
        {
            if (logProgress)
            {
                GameLog.Info("Save", message);
            }
        }
    }
}
