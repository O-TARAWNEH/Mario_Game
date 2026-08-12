// Filename: SfxSystem.cs
// Folder: Assets/Scripts/Audio/
// Purpose: One-shot sound effect playback with a central clip catalog (Phase 18).
// Dependencies: SfxId, UnityEngine

using System;
using UnityEngine;

namespace BounderTrail.Audio
{
    /// <summary>
    /// Plays non-spatial one-shot SFX through a shared AudioSource.
    /// </summary>
    public class SfxSystem : MonoBehaviour
    {
        [Serializable]
        private struct SfxEntry
        {
            public SfxId id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volumeScale;
        }

        [Header("Source")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Catalog")]
        [SerializeField] private SfxEntry[] entries = Array.Empty<SfxEntry>();

        private float _volume = 1f;

        private void Awake()
        {
            EnsureSource();
        }

        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
        }

        public void Play(SfxId id)
        {
            EnsureSource();

            if (!TryGetEntry(id, out var entry) || entry.clip == null)
            {
                return;
            }

            var scale = entry.volumeScale <= 0f ? 1f : entry.volumeScale;
            sfxSource.PlayOneShot(entry.clip, _volume * scale);
        }

        public void AssignClip(SfxId id, AudioClip clip, float volumeScale = 1f)
        {
            volumeScale = Mathf.Clamp01(volumeScale);

            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].id != id)
                {
                    continue;
                }

                entries[i].clip = clip;
                entries[i].volumeScale = volumeScale;
                return;
            }

            Array.Resize(ref entries, entries.Length + 1);
            entries[entries.Length - 1] = new SfxEntry
            {
                id = id,
                clip = clip,
                volumeScale = volumeScale
            };
        }

        private bool TryGetEntry(SfxId id, out SfxEntry entry)
        {
            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].id == id)
                {
                    entry = entries[i];
                    return true;
                }
            }

            entry = default;
            return false;
        }

        private void EnsureSource()
        {
            if (sfxSource != null)
            {
                return;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.priority = 128;
        }
    }
}
