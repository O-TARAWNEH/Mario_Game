// Filename: Phase13PowerUpsSetup.cs
// Folder: Assets/Editor/
// Purpose: Creates design-spec power-up prefabs, SFX, player wiring, samples (Phase 13).
// Menu: Bounder Trail/Phase 13/Setup Power-Ups
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase13PowerUpsSetup.SetupPowerUps

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Core;
using BounderTrail.Items;
using BounderTrail.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase13PowerUpsSetup
    {
        private const string PrefabFolder = "Assets/Prefabs/Items";
        private const string ArtFolder = "Assets/Art/Items";
        private const string SfxFolder = "Assets/Audio/SFX";
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Bounder Trail/Phase 13/Setup Power-Ups")]
        public static void SetupPowerUps()
        {
            EnsureFolder("Assets/Prefabs", "Items");
            EnsureFolder("Assets/Art", "Items");
            EnsureFolder("Assets/Audio", "SFX");

            EnsureTag("PowerUp");
            EnsureLayer("Pickup");

            var speedClip = CreateBlipSfx($"{SfxFolder}/SFX_PowerUp_Speed.wav", 720f, 1080f);
            var shieldClip = CreateBlipSfx($"{SfxFolder}/SFX_PowerUp_Shield.wav", 520f, 880f);
            var heartClip = CreateBlipSfx($"{SfxFolder}/SFX_PowerUp_Heart.wav", 660f, 990f);

            var speedPrefab = CreatePowerUpPrefab(
                $"{PrefabFolder}/Item_SpeedBurst.prefab",
                "Item_SpeedBurst",
                PowerUpKind.SpeedBurst,
                CreateColorSprite($"{ArtFolder}/PowerUp_SpeedBurst.png", new Color(1f, 0.85f, 0.2f), 28, 28),
                speedClip);

            var shieldPrefab = CreatePowerUpPrefab(
                $"{PrefabFolder}/Item_GlowShield.prefab",
                "Item_GlowShield",
                PowerUpKind.GlowShield,
                CreateColorSprite($"{ArtFolder}/PowerUp_GlowShield.png", new Color(0.35f, 0.9f, 1f), 28, 28),
                shieldClip);

            var heartPrefab = CreatePowerUpPrefab(
                $"{PrefabFolder}/Item_HeartDrop.prefab",
                "Item_HeartDrop",
                PowerUpKind.HeartDrop,
                CreateHeartSprite($"{ArtFolder}/PowerUp_HeartDrop.png"),
                heartClip);

            WirePlayerPrefab();
            WireGameplayScenePlayer();
            PlaceSamples(speedPrefab, shieldPrefab, heartPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 13 power-ups ready.");
        }

        private static GameObject CreatePowerUpPrefab(
            string path,
            string objectName,
            PowerUpKind kind,
            Sprite sprite,
            AudioClip clip)
        {
            var go = new GameObject(objectName);
            go.tag = "PowerUp";
            var pickupLayer = LayerMask.NameToLayer("Pickup");
            if (pickupLayer >= 0)
            {
                go.layer = pickupLayer;
            }

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 13;

            var circle = go.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.4f;

            var audio = go.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 0f;

            var pickup = go.AddComponent<PowerUpPickup>();
            var idle = go.AddComponent<CollectibleIdleMotion>();

            var pickupSo = new SerializedObject(pickup);
            pickupSo.FindProperty("kind").enumValueIndex = (int)kind;
            pickupSo.FindProperty("collectSound").objectReferenceValue = clip;
            pickupSo.FindProperty("spriteRenderer").objectReferenceValue = sr;
            pickupSo.FindProperty("triggerCollider").objectReferenceValue = circle;
            pickupSo.FindProperty("audioSource").objectReferenceValue = audio;
            pickupSo.ApplyModifiedPropertiesWithoutUndo();

            var idleSo = new SerializedObject(idle);
            idleSo.FindProperty("powerUpPickup").objectReferenceValue = pickup;
            idleSo.FindProperty("spinDegreesPerSecond").floatValue = 70f;
            idleSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void WirePlayerPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                WirePlayer(root);
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void WireGameplayScenePlayer()
        {
            if (!File.Exists(GameplayScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var player = GameObject.Find("Player_Pip");
            if (player != null)
            {
                WirePlayer(player);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, GameplayScenePath);
            }
        }

        private static void WirePlayer(GameObject player)
        {
            var controller = player.GetComponent<PlayerController>();
            var health = player.GetComponent<PlayerHealth>();
            var death = player.GetComponent<PlayerDeath>();
            var sprite = player.GetComponent<SpriteRenderer>();

            var powerUps = player.GetComponent<PlayerPowerUps>();
            if (powerUps == null)
            {
                powerUps = player.AddComponent<PlayerPowerUps>();
            }

            var feedback = player.GetComponent<PlayerPowerUpFeedback>();
            if (feedback == null)
            {
                feedback = player.AddComponent<PlayerPowerUpFeedback>();
            }

            var powerSo = new SerializedObject(powerUps);
            powerSo.FindProperty("playerController").objectReferenceValue = controller;
            powerSo.FindProperty("playerHealth").objectReferenceValue = health;
            powerSo.FindProperty("playerDeath").objectReferenceValue = death;
            powerSo.FindProperty("speedBurstDuration").floatValue = 5f;
            powerSo.FindProperty("speedBurstMultiplier").floatValue = 1.45f;
            powerSo.FindProperty("glowShieldDuration").floatValue = 5f;
            powerSo.FindProperty("heartHealAmount").intValue = 1;
            powerSo.ApplyModifiedPropertiesWithoutUndo();

            var feedbackSo = new SerializedObject(feedback);
            feedbackSo.FindProperty("powerUps").objectReferenceValue = powerUps;
            feedbackSo.FindProperty("playerHealth").objectReferenceValue = health;
            feedbackSo.FindProperty("spriteRenderer").objectReferenceValue = sprite;
            feedbackSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void PlaceSamples(GameObject speed, GameObject shield, GameObject heart)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var root = GameObject.Find("Collectibles");
            if (root == null)
            {
                var levelRoot = GameObject.Find("LevelRoot");
                root = new GameObject("Collectibles");
                if (levelRoot != null)
                {
                    root.transform.SetParent(levelRoot.transform, false);
                }
            }

            PlaceIfMissing(root.transform, "PowerUp_SpeedBurst_A", speed, new Vector3(-4.2f, 1.1f, 0f));
            PlaceIfMissing(root.transform, "PowerUp_GlowShield_A", shield, new Vector3(2.4f, 1.6f, 0f));
            PlaceIfMissing(root.transform, "PowerUp_HeartDrop_A", heart, new Vector3(7.0f, -0.8f, 0f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void PlaceIfMissing(Transform parent, string name, GameObject prefab, Vector3 position)
        {
            if (parent.Find(name) != null || GameObject.Find(name) != null)
            {
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
        }

        private static Sprite CreateColorSprite(string assetPath, Color color, int width, int height)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            var cx = (width - 1) * 0.5f;
            var cy = (height - 1) * 0.5f;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var dx = x - cx;
                    var dy = y - cy;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > width * 0.48f)
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                    else if (dist > width * 0.38f)
                    {
                        pixels[y * width + x] = color * 0.7f;
                    }
                    else
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            return ImportSprite(assetPath);
        }

        private static Sprite CreateHeartSprite(string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int size = 28;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            var heart = new Color(1f, 0.35f, 0.45f, 1f);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    // Normalized heart-ish blob using two lobes + point.
                    var nx = (x / (float)(size - 1)) * 2f - 1f;
                    var ny = (y / (float)(size - 1)) * 2f - 1f;
                    var a = nx * nx + (ny - Mathf.Abs(nx) * 0.45f) * (ny - 0.1f);
                    pixels[y * size + x] = a < 0.55f ? heart : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            return ImportSprite(assetPath);
        }

        private static Sprite ImportSprite(string assetPath)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static AudioClip CreateBlipSfx(string assetPath, float freqA, float freqB)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int sampleRate = 22050;
            const float duration = 0.14f;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var env = 1f - (t / duration);
                samples[i] = (Mathf.Sin(2f * Mathf.PI * freqA * t) * 0.5f
                              + Mathf.Sin(2f * Mathf.PI * freqB * t) * 0.3f) * env * env;
            }

            WriteWavMono16(assetPath, samples, sampleRate);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if (importer != null)
            {
                importer.forceToMono = true;
                importer.defaultSampleSettings = new AudioImporterSampleSettings
                {
                    loadType = AudioClipLoadType.DecompressOnLoad,
                    compressionFormat = AudioCompressionFormat.PCM,
                    sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate,
                    quality = 1f
                };
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
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

        private static void EnsureTag(string tag)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(assets[0]);
            var tags = so.FindProperty("tags");
            for (var i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureLayer(string layerName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(assets[0]);
            var layers = so.FindProperty("layers");
            for (var i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    return;
                }
            }

            for (var i = 8; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = layerName;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    return;
                }
            }
        }
    }
}
#endif
