// Filename: SaveSystem.cs
// Folder: Assets/Scripts/Save/
// Purpose: Fail-safe save/load with primary+backup files, New Game, and Reset (Phase 21).
// Dependencies: SaveData, GameProgress, AudioManager, GameLog

using System;
using System.IO;
using System.Text;
using BounderTrail.Audio;
using BounderTrail.Core;
using BounderTrail.Items;
using UnityEngine;

namespace BounderTrail.Save
{
    /// <summary>
    /// Central save authority. Writes primary + backup JSON under persistentDataPath.
    /// Load prefers primary, falls back to backup, never wipes a good backup on failed write.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private const string PrimaryFileName = "bounder_trail_save.json";
        private const string BackupFileName = "bounder_trail_save.bak.json";

        // Legacy PlayerPrefs keys (Phase 19/20) — migrated once if files are missing.
        private const string LegacyHasSaveKey = "BounderTrail.Progress.HasSave";
        private const string LegacyContinueKey = "BounderTrail.Progress.ContinueLevelIndex";
        private const string LegacyUnlockedKey = "BounderTrail.Progress.HighestUnlockedLevelIndex";
        private const string LegacyCompletedMaskKey = "BounderTrail.Progress.CompletedMask";
        private const string LegacyMasterKey = "BounderTrail.Audio.MasterVolume";
        private const string LegacyMusicKey = "BounderTrail.Audio.MusicVolume";
        private const string LegacySfxKey = "BounderTrail.Audio.SfxVolume";
        private const string MigrationFlagKey = "BounderTrail.Save.MigratedToFileV1";

        [Header("Debug")]
        [SerializeField] private bool logSaveEvents = true;

        private SaveData _data = SaveData.CreateDefaults();
        private bool _loaded;

        public SaveData Data => _data;
        public bool HasCampaignSave => _data != null && _data.hasCampaignSave;
        public bool IsLoaded => _loaded;

        public string PrimaryPath => Path.Combine(Application.persistentDataPath, PrimaryFileName);
        public string BackupPath => Path.Combine(Application.persistentDataPath, BackupFileName);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLog.Warning("Save", "Duplicate SaveSystem destroyed.");
                Destroy(this);
                return;
            }

            Instance = this;
            Load();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        /// <summary>
        /// Applies loaded data into GameProgress and AudioManager.
        /// Call after those components exist.
        /// </summary>
        public void ApplyLoadedData()
        {
            if (!_loaded)
            {
                Load();
            }

            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.ApplyFromSave(_data);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ApplyVolumesFromSave(
                    _data.masterVolume,
                    _data.musicVolume,
                    _data.sfxVolume);
            }
        }

        public bool Load()
        {
            try
            {
                if (TryReadValidated(PrimaryPath, out var primary))
                {
                    _data = primary;
                    _loaded = true;
                    Log($"Loaded primary save (unlocked={_data.highestUnlockedLevelIndex}).");
                    return true;
                }

                if (TryReadValidated(BackupPath, out var backup))
                {
                    _data = backup;
                    _loaded = true;
                    Log("Primary save missing/corrupt; restored from backup.");
                    // Re-write primary from good backup without touching backup.
                    TryWriteFile(PrimaryPath, SerializeForDisk(_data));
                    return true;
                }

                if (TryMigrateLegacyPrefs(out var migrated))
                {
                    _data = migrated;
                    _loaded = true;
                    Log("Migrated legacy PlayerPrefs into file save.");
                    Save();
                    return true;
                }

                _data = SaveData.CreateDefaults();
                _loaded = true;
                Log("No save found; using defaults.");
                return true;
            }
            catch (Exception ex)
            {
                GameLog.Error("Save", $"Load failed safely; using defaults. ({ex.Message})");
                _data = SaveData.CreateDefaults();
                _loaded = true;
                return false;
            }
        }

        /// <summary>
        /// Collects runtime state and writes primary + backup. Fail-safe: never deletes backup on failure.
        /// </summary>
        public bool Save()
        {
            try
            {
                GatherFromRuntime();
                _data.Clamp();
                var json = SerializeForDisk(_data);

                // Refresh backup from last-known-good primary first (if present and valid).
                if (File.Exists(PrimaryPath) && TryReadValidated(PrimaryPath, out _))
                {
                    try
                    {
                        File.Copy(PrimaryPath, BackupPath, overwrite: true);
                    }
                    catch (Exception copyEx)
                    {
                        GameLog.Warning("Save", $"Could not refresh backup before write ({copyEx.Message}). Continuing.");
                    }
                }

                if (!TryWriteFile(PrimaryPath, json))
                {
                    GameLog.Error("Save", "Primary write failed; backup left untouched.");
                    return false;
                }

                // Verify primary round-trip before updating backup to the new payload.
                if (!TryReadValidated(PrimaryPath, out var verified))
                {
                    GameLog.Error("Save", "Primary write verification failed; attempting restore from backup.");
                    if (File.Exists(BackupPath))
                    {
                        try
                        {
                            File.Copy(BackupPath, PrimaryPath, overwrite: true);
                        }
                        catch (Exception restoreEx)
                        {
                            GameLog.Error("Save", $"Restore from backup failed ({restoreEx.Message}).");
                        }
                    }

                    return false;
                }

                _data = verified;
                TryWriteFile(BackupPath, json);
                Log("Save written (primary + backup).");
                return true;
            }
            catch (Exception ex)
            {
                GameLog.Error("Save", $"Save failed safely ({ex.Message}). Existing backup preserved.");
                return false;
            }
        }

        /// <summary>
        /// Resets campaign progression for a new run; keeps settings and career bests.
        /// </summary>
        public void NewGame()
        {
            GatherFromRuntime();
            _data.hasCampaignSave = true;
            _data.continueLevelIndex = 0;
            _data.highestUnlockedLevelIndex = 0;
            _data.completedMask = 0;
            // Keep bestScore / bestCoins / volumes.
            ApplyLoadedData();
            Save();
            Log("New game progress created.");
        }

        /// <summary>
        /// Wipes campaign progression and career collectible totals; keeps audio settings.
        /// </summary>
        public void ResetSave()
        {
            var master = _data.masterVolume;
            var music = _data.musicVolume;
            var sfx = _data.sfxVolume;

            _data = SaveData.CreateDefaults();
            _data.masterVolume = master;
            _data.musicVolume = music;
            _data.sfxVolume = sfx;
            _data.hasCampaignSave = false;

            ApplyLoadedData();
            Save();
            ClearLegacyProgressPrefs();
            Log("Save reset (settings preserved).");
        }

        public void UpdateSettings(float master, float music, float sfx)
        {
            GatherFromRuntime();
            _data.masterVolume = master;
            _data.musicVolume = music;
            _data.sfxVolume = sfx;
            _data.Clamp();

            var json = SerializeForDisk(_data);
            if (!TryWriteFile(PrimaryPath, json))
            {
                GameLog.Error("Save", "Settings save failed; backup left untouched.");
                return;
            }

            if (!TryReadValidated(PrimaryPath, out var verified))
            {
                GameLog.Error("Save", "Settings save verification failed.");
                return;
            }

            _data = verified;
            TryWriteFile(BackupPath, json);
        }

        public void NotifyRunProgress(int coins, int score)
        {
            var changed = false;
            if (score > _data.bestScore)
            {
                _data.bestScore = score;
                changed = true;
            }

            if (coins > _data.bestCoins)
            {
                _data.bestCoins = coins;
                changed = true;
            }

            if (changed)
            {
                Save();
            }
        }

        private void GatherFromRuntime()
        {
            if (_data == null)
            {
                _data = SaveData.CreateDefaults();
            }

            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.CopyToSave(_data);
            }

            if (AudioManager.Instance != null && AudioManager.Instance.Volumes != null)
            {
                var volumes = AudioManager.Instance.Volumes;
                _data.masterVolume = volumes.MasterVolume;
                _data.musicVolume = volumes.MusicVolume;
                _data.sfxVolume = volumes.SfxVolume;
            }

            if (CollectibleCounter.Instance != null)
            {
                if (CollectibleCounter.Instance.Score > _data.bestScore)
                {
                    _data.bestScore = CollectibleCounter.Instance.Score;
                }

                if (CollectibleCounter.Instance.CoinCount > _data.bestCoins)
                {
                    _data.bestCoins = CollectibleCounter.Instance.CoinCount;
                }
            }
        }

        private static bool TryReadValidated(string path, out SaveData data)
        {
            data = null;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }

            string json;
            try
            {
                json = File.ReadAllText(path, Encoding.UTF8);
            }
            catch
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            SaveData parsed;
            try
            {
                parsed = JsonUtility.FromJson<SaveData>(json);
            }
            catch
            {
                return false;
            }

            if (parsed == null || parsed.version < 1)
            {
                return false;
            }

            var expected = parsed.checksum;
            parsed.checksum = string.Empty;
            var actual = ComputeChecksum(JsonUtility.ToJson(parsed));
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                return false;
            }

            parsed.checksum = expected;
            parsed.Clamp();
            data = parsed;
            return true;
        }

        private static bool TryWriteFile(string path, string json)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var tempPath = path + ".tmp";
                File.WriteAllText(tempPath, json, Encoding.UTF8);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.Move(tempPath, path);
                return true;
            }
            catch (Exception ex)
            {
                GameLog.Warning("Save", $"File write failed ({path}): {ex.Message}");
                try
                {
                    var tempPath = path + ".tmp";
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch
                {
                    // Ignore temp cleanup failures.
                }

                return false;
            }
        }

        private static string SerializeForDisk(SaveData data)
        {
            data.checksum = string.Empty;
            data.Clamp();
            var payload = JsonUtility.ToJson(data);
            data.checksum = ComputeChecksum(payload);
            return JsonUtility.ToJson(data);
        }

        private static string ComputeChecksum(string payload)
        {
            // FNV-1a 32-bit — detects corruption; not a security hash.
            unchecked
            {
                var hash = 2166136261u;
                for (var i = 0; i < payload.Length; i++)
                {
                    hash ^= payload[i];
                    hash *= 16777619u;
                }

                return hash.ToString("X8");
            }
        }

        private bool TryMigrateLegacyPrefs(out SaveData data)
        {
            data = null;
            if (PlayerPrefs.GetInt(MigrationFlagKey, 0) == 1)
            {
                return false;
            }

            var hasLegacy = PlayerPrefs.HasKey(LegacyHasSaveKey)
                            || PlayerPrefs.HasKey(LegacyUnlockedKey)
                            || PlayerPrefs.HasKey(LegacyMasterKey);
            if (!hasLegacy)
            {
                return false;
            }

            data = SaveData.CreateDefaults();
            data.hasCampaignSave = PlayerPrefs.GetInt(LegacyHasSaveKey, 0) == 1;
            data.continueLevelIndex = Mathf.Max(0, PlayerPrefs.GetInt(LegacyContinueKey, 0));
            data.highestUnlockedLevelIndex = Mathf.Max(0, PlayerPrefs.GetInt(LegacyUnlockedKey, 0));
            data.completedMask = Mathf.Max(0, PlayerPrefs.GetInt(LegacyCompletedMaskKey, 0));
            data.masterVolume = PlayerPrefs.GetFloat(LegacyMasterKey, data.masterVolume);
            data.musicVolume = PlayerPrefs.GetFloat(LegacyMusicKey, data.musicVolume);
            data.sfxVolume = PlayerPrefs.GetFloat(LegacySfxKey, data.sfxVolume);
            data.Clamp();

            PlayerPrefs.SetInt(MigrationFlagKey, 1);
            PlayerPrefs.Save();
            return true;
        }

        private static void ClearLegacyProgressPrefs()
        {
            PlayerPrefs.DeleteKey(LegacyHasSaveKey);
            PlayerPrefs.DeleteKey(LegacyContinueKey);
            PlayerPrefs.DeleteKey(LegacyUnlockedKey);
            PlayerPrefs.DeleteKey(LegacyCompletedMaskKey);
            PlayerPrefs.Save();
        }

        private void Log(string message)
        {
            if (logSaveEvents)
            {
                GameLog.Info("Save", message);
            }
        }
    }
}
