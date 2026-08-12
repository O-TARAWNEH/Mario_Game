// Filename: AudioManager.cs
// Folder: Assets/Scripts/Audio/
// Purpose: Centralized audio facade for music, SFX, and volume settings (Phase 18).
// Dependencies: MusicSystem, SfxSystem, AudioVolumeSettings, GameStateManager, GameLog

using BounderTrail.Core;
using BounderTrail.Save;
using UnityEngine;

namespace BounderTrail.Audio
{
    /// <summary>
    /// Persistent audio authority. Lives on the bootstrap object.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Systems")]
        [SerializeField] private MusicSystem musicSystem;
        [SerializeField] private SfxSystem sfxSystem;

        [Header("Volume")]
        [SerializeField] private AudioVolumeSettings volumes = new AudioVolumeSettings();
        [SerializeField] private bool loadVolumesFromPrefs = true;
        [SerializeField] private bool autoSwitchMusicWithGameState = true;

        public MusicSystem Music => musicSystem;
        public SfxSystem Sfx => sfxSystem;
        public AudioVolumeSettings Volumes => volumes;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLog.Warning("Audio", "Duplicate AudioManager destroyed.");
                Destroy(this);
                return;
            }

            Instance = this;
            EnsureSystems();

            if (SaveSystem.Instance != null && SaveSystem.Instance.IsLoaded)
            {
                var data = SaveSystem.Instance.Data;
                ApplyVolumesFromSave(data.masterVolume, data.musicVolume, data.sfxVolume);
            }
            else if (loadVolumesFromPrefs)
            {
                volumes.Load();
                ApplyVolumes();
            }
            else
            {
                volumes.ClampAll();
                ApplyVolumes();
            }
        }

        private void OnEnable()
        {
            if (autoSwitchMusicWithGameState && GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged += OnStateChanged;
                ApplyMusicForState(GameStateManager.Instance.CurrentState);
            }
        }

        private void Start()
        {
            // Late subscribe if GameStateManager awakens in the same frame.
            if (!autoSwitchMusicWithGameState)
            {
                return;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged -= OnStateChanged;
                GameStateManager.Instance.StateChanged += OnStateChanged;
                ApplyMusicForState(GameStateManager.Instance.CurrentState);
            }
        }

        private void OnDisable()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StateChanged -= OnStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static void PlaySfx(SfxId id)
        {
            if (Instance != null && Instance.sfxSystem != null)
            {
                Instance.sfxSystem.Play(id);
            }
        }

        public static void PlayMusic(MusicId id)
        {
            if (Instance != null && Instance.musicSystem != null)
            {
                Instance.musicSystem.Play(id);
            }
        }

        public void SetMasterVolume(float value)
        {
            volumes.MasterVolume = value;
            ApplyVolumes();
            PersistVolumes();
        }

        public void SetMusicVolume(float value)
        {
            volumes.MusicVolume = value;
            ApplyVolumes();
            PersistVolumes();
        }

        public void SetSfxVolume(float value)
        {
            volumes.SfxVolume = value;
            ApplyVolumes();
            PersistVolumes();
        }

        /// <summary>
        /// Applies volumes from SaveSystem without writing back.
        /// </summary>
        public void ApplyVolumesFromSave(float master, float music, float sfx)
        {
            volumes.MasterVolume = master;
            volumes.MusicVolume = music;
            volumes.SfxVolume = sfx;
            ApplyVolumes();
        }

        public void ApplyVolumes()
        {
            volumes.ClampAll();
            if (musicSystem != null)
            {
                musicSystem.SetVolume(volumes.EffectiveMusicVolume);
            }

            if (sfxSystem != null)
            {
                sfxSystem.SetVolume(volumes.EffectiveSfxVolume);
            }
        }

        private void PersistVolumes()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.UpdateSettings(
                    volumes.MasterVolume,
                    volumes.MusicVolume,
                    volumes.SfxVolume);
                return;
            }

            volumes.Save();
        }

        private void OnStateChanged(GameStateId previous, GameStateId next)
        {
            ApplyMusicForState(next);
        }

        private void ApplyMusicForState(GameStateId state)
        {
            if (musicSystem == null)
            {
                return;
            }

            switch (state)
            {
                case GameStateId.MainMenu:
                case GameStateId.Boot:
                    musicSystem.Play(MusicId.Menu);
                    break;
                case GameStateId.Gameplay:
                case GameStateId.Pause:
                case GameStateId.GameOver:
                case GameStateId.LevelComplete:
                    musicSystem.Play(MusicId.Gameplay);
                    break;
            }
        }

        private void EnsureSystems()
        {
            if (musicSystem == null)
            {
                musicSystem = GetComponentInChildren<MusicSystem>(true);
                if (musicSystem == null)
                {
                    var musicObject = new GameObject("MusicSystem");
                    musicObject.transform.SetParent(transform, false);
                    musicSystem = musicObject.AddComponent<MusicSystem>();
                }
            }

            if (sfxSystem == null)
            {
                sfxSystem = GetComponentInChildren<SfxSystem>(true);
                if (sfxSystem == null)
                {
                    var sfxObject = new GameObject("SfxSystem");
                    sfxObject.transform.SetParent(transform, false);
                    sfxSystem = sfxObject.AddComponent<SfxSystem>();
                }
            }
        }
    }
}
