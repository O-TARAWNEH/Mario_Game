// Filename: AudioSettingsView.cs
// Folder: Assets/Scripts/UI/
// Purpose: Reusable Master / Music / SFX volume sliders bound to AudioManager (Phase 19).
// Dependencies: BounderTrail.Audio.AudioManager

using BounderTrail.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.UI
{
    /// <summary>
    /// Binds UI sliders to AudioManager volume setters.
    /// </summary>
    public class AudioSettingsView : MonoBehaviour
    {
        [Header("Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Optional Labels")]
        [SerializeField] private Text masterValueLabel;
        [SerializeField] private Text musicValueLabel;
        [SerializeField] private Text sfxValueLabel;

        private bool _suppressCallbacks;

        private void OnEnable()
        {
            RefreshFromAudioManager();
            Bind(masterSlider, OnMasterChanged);
            Bind(musicSlider, OnMusicChanged);
            Bind(sfxSlider, OnSfxChanged);
        }

        private void OnDisable()
        {
            Unbind(masterSlider, OnMasterChanged);
            Unbind(musicSlider, OnMusicChanged);
            Unbind(sfxSlider, OnSfxChanged);
        }

        public void RefreshFromAudioManager()
        {
            if (AudioManager.Instance == null || AudioManager.Instance.Volumes == null)
            {
                return;
            }

            var volumes = AudioManager.Instance.Volumes;
            _suppressCallbacks = true;
            SetSlider(masterSlider, volumes.MasterVolume);
            SetSlider(musicSlider, volumes.MusicVolume);
            SetSlider(sfxSlider, volumes.SfxVolume);
            _suppressCallbacks = false;

            UpdateLabels(volumes.MasterVolume, volumes.MusicVolume, volumes.SfxVolume);
        }

        private void OnMasterChanged(float value)
        {
            if (_suppressCallbacks)
            {
                return;
            }

            AudioManager.Instance?.SetMasterVolume(value);
            UpdateLabel(masterValueLabel, value);
        }

        private void OnMusicChanged(float value)
        {
            if (_suppressCallbacks)
            {
                return;
            }

            AudioManager.Instance?.SetMusicVolume(value);
            UpdateLabel(musicValueLabel, value);
        }

        private void OnSfxChanged(float value)
        {
            if (_suppressCallbacks)
            {
                return;
            }

            AudioManager.Instance?.SetSfxVolume(value);
            UpdateLabel(sfxValueLabel, value);
        }

        private void UpdateLabels(float master, float music, float sfx)
        {
            UpdateLabel(masterValueLabel, master);
            UpdateLabel(musicValueLabel, music);
            UpdateLabel(sfxValueLabel, sfx);
        }

        private static void UpdateLabel(Text label, float value)
        {
            if (label != null)
            {
                label.text = $"{Mathf.RoundToInt(value * 100f)}%";
            }
        }

        private static void SetSlider(Slider slider, float value)
        {
            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.wholeNumbers = false;
                slider.SetValueWithoutNotify(Mathf.Clamp01(value));
            }
        }

        private static void Bind(Slider slider, UnityEngine.Events.UnityAction<float> action)
        {
            if (slider != null)
            {
                slider.onValueChanged.AddListener(action);
            }
        }

        private static void Unbind(Slider slider, UnityEngine.Events.UnityAction<float> action)
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(action);
            }
        }
    }
}
