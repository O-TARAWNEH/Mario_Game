// Filename: MusicSystem.cs
// Folder: Assets/Scripts/Audio/
// Purpose: Looping background music playback for menu and gameplay (Phase 18).
// Dependencies: MusicId, UnityEngine

using UnityEngine;

namespace BounderTrail.Audio
{
    /// <summary>
    /// Owns a dedicated looping AudioSource for BGM.
    /// </summary>
    public class MusicSystem : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private AudioSource musicSource;

        [Header("Clips")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;

        private MusicId _current = MusicId.None;
        private float _volume = 1f;

        public MusicId CurrentMusic => _current;

        private void Awake()
        {
            EnsureSource();
        }

        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = _volume;
            }
        }

        public void Play(MusicId id)
        {
            EnsureSource();

            if (id == MusicId.None)
            {
                Stop();
                return;
            }

            var clip = ResolveClip(id);
            if (clip == null)
            {
                return;
            }

            if (_current == id && musicSource.isPlaying && musicSource.clip == clip)
            {
                musicSource.volume = _volume;
                return;
            }

            _current = id;
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = _volume;
            musicSource.Play();
        }

        public void Stop()
        {
            _current = MusicId.None;
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        public void AssignClips(AudioClip menu, AudioClip gameplay)
        {
            menuMusic = menu;
            gameplayMusic = gameplay;
        }

        private AudioClip ResolveClip(MusicId id)
        {
            return id switch
            {
                MusicId.Menu => menuMusic,
                MusicId.Gameplay => gameplayMusic,
                _ => null
            };
        }

        private void EnsureSource()
        {
            if (musicSource != null)
            {
                return;
            }

            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }

            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.priority = 64;
        }
    }
}
