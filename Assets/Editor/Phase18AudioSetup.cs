// Filename: Phase18AudioSetup.cs
// Folder: Assets/Editor/
// Purpose: Creates placeholder BGM/SFX, wires AudioManager on bootstrap, player audio hooks (Phase 18).
// Menu: Bounder Trail/Phase 18/Setup Audio System
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase18AudioSetup.SetupAudioSystem

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Audio;
using BounderTrail.Core;
using BounderTrail.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase18AudioSetup
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string SfxFolder = "Assets/Audio/SFX";
        private const string MusicFolder = "Assets/Audio/Music";

        [MenuItem("Bounder Trail/Phase 18/Setup Audio System")]
        public static void SetupAudioSystem()
        {
            EnsureFolder("Assets/Audio", "SFX");
            EnsureFolder("Assets/Audio", "Music");

            var jump = CreateToneSfx($"{SfxFolder}/SFX_Jump.wav", 0.09f, 520f, 780f, rising: true);
            var land = CreateNoiseThud($"{SfxFolder}/SFX_Land.wav", 0.07f, 140f);
            var collect = CreateToneSfx($"{SfxFolder}/SFX_Collect.wav", 0.11f, 980f, 1470f, rising: true);
            var damage = CreateToneSfx($"{SfxFolder}/SFX_Damage.wav", 0.12f, 320f, 180f, rising: false);
            var enemyDefeat = CreateToneSfx($"{SfxFolder}/SFX_EnemyDefeat.wav", 0.14f, 440f, 220f, rising: false);
            var powerUp = CreateToneSfx($"{SfxFolder}/SFX_PowerUp.wav", 0.16f, 560f, 920f, rising: true);
            var death = CreateToneSfx($"{SfxFolder}/SFX_Death.wav", 0.28f, 360f, 90f, rising: false);
            var levelComplete = CreateArpeggio($"{SfxFolder}/SFX_LevelComplete.wav", new[] { 523f, 659f, 784f, 1046f });
            var ui = CreateToneSfx($"{SfxFolder}/SFX_Ui.wav", 0.05f, 700f, 900f, rising: true);

            var menuMusic = CreateLoopPad($"{MusicFolder}/BGM_Menu.wav", 3.2f, 196f, 294f);
            var gameplayMusic = CreateLoopPad($"{MusicFolder}/BGM_Gameplay.wav", 3.6f, 220f, 330f);

            WireBootstrapAudio(
                jump, land, collect, damage, enemyDefeat, powerUp, death, levelComplete, ui,
                menuMusic, gameplayMusic);
            WirePlayerAudioFeedback();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 18 audio system ready.");
        }

        private static void WireBootstrapAudio(
            AudioClip jump,
            AudioClip land,
            AudioClip collect,
            AudioClip damage,
            AudioClip enemyDefeat,
            AudioClip powerUp,
            AudioClip death,
            AudioClip levelComplete,
            AudioClip ui,
            AudioClip menuMusic,
            AudioClip gameplayMusic)
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrap == null)
            {
                bootstrap = new GameObject(ProjectConstants.BootstrapObjectName);
                bootstrap.AddComponent<GameBootstrap>();
            }

            var audioManager = bootstrap.GetComponent<AudioManager>();
            if (audioManager == null)
            {
                audioManager = bootstrap.AddComponent<AudioManager>();
            }

            var music = bootstrap.GetComponentInChildren<MusicSystem>(true);
            if (music == null)
            {
                var musicObject = new GameObject("MusicSystem");
                musicObject.transform.SetParent(bootstrap.transform, false);
                music = musicObject.AddComponent<MusicSystem>();
            }

            var sfx = bootstrap.GetComponentInChildren<SfxSystem>(true);
            if (sfx == null)
            {
                var sfxObject = new GameObject("SfxSystem");
                sfxObject.transform.SetParent(bootstrap.transform, false);
                sfx = sfxObject.AddComponent<SfxSystem>();
            }

            var musicSo = new SerializedObject(music);
            musicSo.FindProperty("menuMusic").objectReferenceValue = menuMusic;
            musicSo.FindProperty("gameplayMusic").objectReferenceValue = gameplayMusic;
            musicSo.ApplyModifiedPropertiesWithoutUndo();

            AssignSfx(sfx, SfxId.Jump, jump, 0.85f);
            AssignSfx(sfx, SfxId.Land, land, 0.7f);
            AssignSfx(sfx, SfxId.Collect, collect, 0.85f);
            AssignSfx(sfx, SfxId.Damage, damage, 0.9f);
            AssignSfx(sfx, SfxId.EnemyDefeat, enemyDefeat, 0.85f);
            AssignSfx(sfx, SfxId.PowerUp, powerUp, 0.9f);
            AssignSfx(sfx, SfxId.Death, death, 0.95f);
            AssignSfx(sfx, SfxId.LevelComplete, levelComplete, 0.95f);
            AssignSfx(sfx, SfxId.Ui, ui, 0.65f);

            var managerSo = new SerializedObject(audioManager);
            managerSo.FindProperty("musicSystem").objectReferenceValue = music;
            managerSo.FindProperty("sfxSystem").objectReferenceValue = sfx;
            managerSo.FindProperty("loadVolumesFromPrefs").boolValue = true;
            managerSo.FindProperty("autoSwitchMusicWithGameState").boolValue = true;
            var volumes = managerSo.FindProperty("volumes");
            if (volumes != null)
            {
                volumes.FindPropertyRelative("masterVolume").floatValue = 1f;
                volumes.FindPropertyRelative("musicVolume").floatValue = 0.55f;
                volumes.FindPropertyRelative("sfxVolume").floatValue = 0.85f;
            }

            managerSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(audioManager);
            EditorUtility.SetDirty(music);
            EditorUtility.SetDirty(sfx);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void AssignSfx(SfxSystem sfx, SfxId id, AudioClip clip, float volumeScale)
        {
            var so = new SerializedObject(sfx);
            var entries = so.FindProperty("entries");
            var index = -1;
            for (var i = 0; i < entries.arraySize; i++)
            {
                if ((SfxId)entries.GetArrayElementAtIndex(i).FindPropertyRelative("id").enumValueIndex == id)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                index = entries.arraySize;
                entries.InsertArrayElementAtIndex(index);
            }

            var element = entries.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("id").enumValueIndex = (int)id;
            element.FindPropertyRelative("clip").objectReferenceValue = clip;
            element.FindPropertyRelative("volumeScale").floatValue = volumeScale;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WirePlayerAudioFeedback()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Player prefab missing; skip PlayerAudioFeedback.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                if (root.GetComponent<PlayerAudioFeedback>() == null)
                {
                    root.AddComponent<PlayerAudioFeedback>();
                }

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            // Keep scene instance in sync if present.
            var gameplay = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Single);
            var scenePlayer = GameObject.Find("Player_Pip");
            if (scenePlayer != null && scenePlayer.GetComponent<PlayerAudioFeedback>() == null)
            {
                scenePlayer.AddComponent<PlayerAudioFeedback>();
                EditorSceneManager.MarkSceneDirty(gameplay);
                EditorSceneManager.SaveScene(gameplay, "Assets/Scenes/Gameplay.unity");
            }
        }

        private static AudioClip CreateToneSfx(
            string assetPath,
            float duration,
            float freqA,
            float freqB,
            bool rising)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int sampleRate = 22050;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var n = t / duration;
                var freq = rising
                    ? Mathf.Lerp(freqA, freqB, n)
                    : Mathf.Lerp(freqA, freqB, n);
                var env = rising
                    ? (1f - n) * (1f - n)
                    : Mathf.Pow(1f - n, 1.5f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.55f * env;
            }

            return WriteAndImportSfx(assetPath, samples, sampleRate);
        }

        private static AudioClip CreateNoiseThud(string assetPath, float duration, float toneHz)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int sampleRate = 22050;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var n = t / duration;
                var env = Mathf.Pow(1f - n, 2.5f);
                var noise = (Random.value * 2f - 1f) * 0.25f;
                var tone = Mathf.Sin(2f * Mathf.PI * toneHz * t) * 0.4f;
                samples[i] = (noise + tone) * env;
            }

            return WriteAndImportSfx(assetPath, samples, sampleRate);
        }

        private static AudioClip CreateArpeggio(string assetPath, float[] notes)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int sampleRate = 22050;
            const float noteDuration = 0.09f;
            var sampleCount = Mathf.CeilToInt(sampleRate * noteDuration * notes.Length);
            var samples = new float[sampleCount];

            for (var n = 0; n < notes.Length; n++)
            {
                var start = Mathf.FloorToInt(n * noteDuration * sampleRate);
                var length = Mathf.CeilToInt(noteDuration * sampleRate);
                for (var i = 0; i < length; i++)
                {
                    var index = start + i;
                    if (index >= samples.Length)
                    {
                        break;
                    }

                    var t = i / (float)sampleRate;
                    var env = 1f - (i / (float)length);
                    samples[index] += Mathf.Sin(2f * Mathf.PI * notes[n] * t) * 0.45f * env * env;
                }
            }

            return WriteAndImportSfx(assetPath, samples, sampleRate);
        }

        private static AudioClip CreateLoopPad(string assetPath, float duration, float freqA, float freqB)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int sampleRate = 22050;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var fade = Mathf.Sin(Mathf.PI * (i / (float)(sampleCount - 1)));
                var a = Mathf.Sin(2f * Mathf.PI * freqA * t) * 0.18f;
                var b = Mathf.Sin(2f * Mathf.PI * freqB * t) * 0.12f;
                var c = Mathf.Sin(2f * Mathf.PI * (freqA * 0.5f) * t) * 0.08f;
                samples[i] = (a + b + c) * fade;
            }

            return WriteAndImportMusic(assetPath, samples, sampleRate);
        }

        private static AudioClip WriteAndImportSfx(string assetPath, float[] samples, int sampleRate)
        {
            WriteWavMono16(assetPath, samples, sampleRate);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            ConfigureImporter(assetPath, looping: false);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }

        private static AudioClip WriteAndImportMusic(string assetPath, float[] samples, int sampleRate)
        {
            WriteWavMono16(assetPath, samples, sampleRate);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            ConfigureImporter(assetPath, looping: true);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }

        private static void ConfigureImporter(string assetPath, bool looping)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if (importer == null)
            {
                return;
            }

            importer.forceToMono = true;
            importer.loadInBackground = false;
            importer.defaultSampleSettings = new AudioImporterSampleSettings
            {
                loadType = looping ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad,
                compressionFormat = AudioCompressionFormat.PCM,
                sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate,
                quality = 1f
            };
            importer.SaveAndReimport();
        }

        private static void WriteWavMono16(string path, float[] samples, int sampleRate)
        {
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(stream);

            const short channels = 1;
            const short bitsPerSample = 16;
            var byteRate = sampleRate * channels * bitsPerSample / 8;
            var blockAlign = (short)(channels * bitsPerSample / 8);
            var dataSize = samples.Length * blockAlign;

            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            for (var i = 0; i < samples.Length; i++)
            {
                var clamped = Mathf.Clamp(samples[i], -1f, 1f);
                writer.Write((short)Mathf.RoundToInt(clamped * short.MaxValue));
            }
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
