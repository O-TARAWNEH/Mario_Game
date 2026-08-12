// Filename: AudioVolumeSettings.cs
// Folder: Assets/Scripts/Audio/
// Purpose: Master / music / SFX volume values with PlayerPrefs persistence (Phase 18).
// Dependencies: UnityEngine

using UnityEngine;

namespace BounderTrail.Audio
{
    /// <summary>
    /// Simple volume mixer for Bounder Trail. Values are 0..1 and stored in PlayerPrefs.
    /// </summary>
    [System.Serializable]
    public class AudioVolumeSettings
    {
        private const string MasterKey = "BounderTrail.Audio.MasterVolume";
        private const string MusicKey = "BounderTrail.Audio.MusicVolume";
        private const string SfxKey = "BounderTrail.Audio.SfxVolume";

        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.55f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.85f;

        public float MasterVolume
        {
            get => masterVolume;
            set => masterVolume = Mathf.Clamp01(value);
        }

        public float MusicVolume
        {
            get => musicVolume;
            set => musicVolume = Mathf.Clamp01(value);
        }

        public float SfxVolume
        {
            get => sfxVolume;
            set => sfxVolume = Mathf.Clamp01(value);
        }

        public float EffectiveMusicVolume => masterVolume * musicVolume;
        public float EffectiveSfxVolume => masterVolume * sfxVolume;

        public void Load()
        {
            masterVolume = PlayerPrefs.GetFloat(MasterKey, masterVolume);
            musicVolume = PlayerPrefs.GetFloat(MusicKey, musicVolume);
            sfxVolume = PlayerPrefs.GetFloat(SfxKey, sfxVolume);
            ClampAll();
        }

        public void Save()
        {
            ClampAll();
            PlayerPrefs.SetFloat(MasterKey, masterVolume);
            PlayerPrefs.SetFloat(MusicKey, musicVolume);
            PlayerPrefs.SetFloat(SfxKey, sfxVolume);
            PlayerPrefs.Save();
        }

        public void ClampAll()
        {
            masterVolume = Mathf.Clamp01(masterVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);
        }
    }
}
