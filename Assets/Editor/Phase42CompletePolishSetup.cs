// Filename: Phase42CompletePolishSetup.cs
// Folder: Assets/Editor/
// Purpose: Complete polish pass — player input, audio, secrets, menu copy (Phase 42).
// Menu: Bounder Trail/Phase 42/Setup Complete Polish
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase42CompletePolishSetup.SetupCompletePolish

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Audio;
using BounderTrail.Core;
using BounderTrail.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase42CompletePolishSetup
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string SfxFolder = "Assets/Audio/SFX";
        private const string MusicFolder = "Assets/Audio/Music";

        [MenuItem("Bounder Trail/Phase 42/Setup Complete Polish")]
        public static void SetupCompletePolish()
        {
            TunePlayerPrefab();
            WireVictoryAudio();
            UpdateMainMenuControlsCopy();

            // Rebuild layouts via Phase 41 (Pip restored — knight no longer applied).
            Phase41GameplayRepairSetup.SetupGameplayRepair();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"{GameLog.ProjectPrefix}[Setup] Phase 42 complete polish ready — " +
                "multi-key jump, coin persistence/1UPs, victory music, secret coins, R-restart (Pip visual).");
        }

        private static void TunePlayerPrefab()
        {
            if (!File.Exists(PlayerPrefabPath))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing {PlayerPrefabPath}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var controller = root.GetComponent<PlayerController>();
                if (controller == null)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] PlayerController missing on prefab.");
                    return;
                }

                var so = new SerializedObject(controller);
                so.FindProperty("walkSpeed").floatValue = 7.2f;
                so.FindProperty("runSpeed").floatValue = 10.8f;
                so.FindProperty("acceleration").floatValue = 92f;
                so.FindProperty("deceleration").floatValue = 98f;
                so.FindProperty("airAcceleration").floatValue = 50f;
                so.FindProperty("airDeceleration").floatValue = 44f;
                so.FindProperty("airControl").floatValue = 0.82f;
                so.FindProperty("jumpForce").floatValue = 16.2f;
                so.FindProperty("coyoteTime").floatValue = 0.12f;
                so.FindProperty("jumpBufferTime").floatValue = 0.14f;
                so.FindProperty("jumpCutMultiplier").floatValue = 0.48f;
                so.FindProperty("jumpCutGravityMultiplier").floatValue = 2.35f;
                so.FindProperty("gravity").floatValue = 3.45f;
                so.FindProperty("fallGravityMultiplier").floatValue = 2.35f;
                so.FindProperty("jumpKey").intValue = (int)KeyCode.Space;

                var jumpKeys = so.FindProperty("jumpKeys");
                if (jumpKeys != null)
                {
                    jumpKeys.arraySize = 3;
                    jumpKeys.GetArrayElementAtIndex(0).intValue = (int)KeyCode.Space;
                    jumpKeys.GetArrayElementAtIndex(1).intValue = (int)KeyCode.W;
                    jumpKeys.GetArrayElementAtIndex(2).intValue = (int)KeyCode.UpArrow;
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void WireVictoryAudio()
        {
            EnsureFolder("Assets/Audio", "SFX");
            EnsureFolder("Assets/Audio", "Music");

            var bonusLife = CreateArpeggio(
                $"{SfxFolder}/SFX_BonusLife.wav",
                new[] { 659f, 880f, 1175f });
            var victory = CreateLoopPad($"{MusicFolder}/BGM_Victory.wav", 4.0f, 262f, 392f);

            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrap == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Bootstrap missing.");
                return;
            }

            var music = bootstrap.GetComponentInChildren<MusicSystem>(true);
            var sfx = bootstrap.GetComponentInChildren<SfxSystem>(true);
            if (music == null || sfx == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Audio systems missing on Bootstrap.");
                return;
            }

            var musicSo = new SerializedObject(music);
            musicSo.FindProperty("victoryMusic").objectReferenceValue = victory;
            musicSo.ApplyModifiedPropertiesWithoutUndo();

            AssignSfx(sfx, SfxId.BonusLife, bonusLife, 0.9f);

            EditorUtility.SetDirty(music);
            EditorUtility.SetDirty(sfx);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void UpdateMainMenuControlsCopy()
        {
            if (!File.Exists(MainMenuScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("MainMenuCanvas");
            if (canvas == null)
            {
                return;
            }

            var controlsBody = canvas.transform.Find("SettingsPanel/ControlsBody");
            if (controlsBody == null)
            {
                controlsBody = canvas.transform.Find("RootPanel/SettingsPanel/ControlsBody");
            }

            if (controlsBody == null)
            {
                var texts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (var i = 0; i < texts.Length; i++)
                {
                    if (texts[i] != null && texts[i].text != null
                        && texts[i].text.IndexOf("Jump", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        texts[i].text =
                            "Controls\n\nMove: A / D\nJump: Space / W / Up\nRun: Left Shift\nPause: Esc\nRestart: R";
                        EditorUtility.SetDirty(texts[i]);
                        break;
                    }
                }
            }
            else
            {
                var text = controlsBody.GetComponent<Text>();
                if (text != null)
                {
                    text.text =
                        "Controls\n\nMove: A / D\nJump: Space / W / Up\nRun: Left Shift\nPause: Esc\nRestart: R";
                    EditorUtility.SetDirty(text);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
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

        private static AudioClip CreateArpeggio(string assetPath, float[] notes)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int sampleRate = 22050;
            const float noteDuration = 0.1f;
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

            return WriteWav(assetPath, samples, sampleRate);
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
                var n = t / duration;
                var fade = Mathf.Sin(n * Mathf.PI);
                var a = Mathf.Sin(2f * Mathf.PI * freqA * t) * 0.18f;
                var b = Mathf.Sin(2f * Mathf.PI * freqB * t) * 0.14f;
                var c = Mathf.Sin(2f * Mathf.PI * (freqA * 1.5f) * t) * 0.08f;
                samples[i] = (a + b + c) * fade;
            }

            return WriteWav(assetPath, samples, sampleRate);
        }

        private static AudioClip WriteWav(string assetPath, float[] samples, int sampleRate)
        {
            var absolute = Path.Combine(
                Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                assetPath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(absolute) ?? Application.dataPath);

            using (var stream = new FileStream(absolute, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
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
                    writer.Write((short)(clamped * short.MaxValue));
                }
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
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
