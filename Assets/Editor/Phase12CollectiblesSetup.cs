// Filename: Phase12CollectiblesSetup.cs
// Folder: Assets/Editor/
// Purpose: Creates coin prefab, SFX, counter wiring, and sample placements (Phase 12).
// Menu: Bounder Trail/Phase 12/Setup Collectibles
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase12CollectiblesSetup.SetupCollectibles

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Core;
using BounderTrail.Items;
using BounderTrail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase12CollectiblesSetup
    {
        private const string PrefabPath = "Assets/Prefabs/Items/Item_Coin.prefab";
        private const string SpritePath = "Assets/Art/Items/Coin_Placeholder.png";
        private const string SfxPath = "Assets/Audio/SFX/SFX_Coin.wav";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";

        [MenuItem("Bounder Trail/Phase 12/Setup Collectibles")]
        public static void SetupCollectibles()
        {
            EnsureFolder("Assets/Prefabs", "Items");
            EnsureFolder("Assets/Art", "Items");
            EnsureFolder("Assets/Audio", "SFX");
            EnsureFolder("Assets/Scripts", "Items");

            EnsureTag("Coin");
            EnsureLayer("Pickup");

            var sprite = CreateCoinSprite(SpritePath);
            var clip = CreateCoinSfx(SfxPath);
            var prefab = CreateCoinPrefab(sprite, clip);

            WireBootstrapCounter();
            PlaceSamplesAndHud(prefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 12 collectibles ready.");
        }

        private static GameObject CreateCoinPrefab(Sprite sprite, AudioClip clip)
        {
            var go = new GameObject("Item_Coin");
            go.tag = "Coin";
            var pickupLayer = LayerMask.NameToLayer("Pickup");
            if (pickupLayer >= 0)
            {
                go.layer = pickupLayer;
            }

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 12;

            var circle = go.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.35f;

            var audio = go.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 0f;

            var collectible = go.AddComponent<Collectible>();
            var idle = go.AddComponent<CollectibleIdleMotion>();

            var collectibleSo = new SerializedObject(collectible);
            collectibleSo.FindProperty("kind").enumValueIndex = (int)CollectibleKind.Coin;
            collectibleSo.FindProperty("coinValue").intValue = 1;
            collectibleSo.FindProperty("scoreValue").intValue = 10;
            collectibleSo.FindProperty("collectSound").objectReferenceValue = clip;
            collectibleSo.FindProperty("spriteRenderer").objectReferenceValue = sr;
            collectibleSo.FindProperty("triggerCollider").objectReferenceValue = circle;
            collectibleSo.FindProperty("audioSource").objectReferenceValue = audio;
            collectibleSo.ApplyModifiedPropertiesWithoutUndo();

            var idleSo = new SerializedObject(idle);
            idleSo.FindProperty("collectible").objectReferenceValue = collectible;
            idleSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void WireBootstrapCounter()
        {
            if (!File.Exists(BootstrapScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrap == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] GameBootstrap missing; counter will be added at runtime.");
                return;
            }

            if (bootstrap.GetComponent<CollectibleCounter>() == null)
            {
                bootstrap.AddComponent<CollectibleCounter>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void PlaceSamplesAndHud(GameObject prefab)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            var collectiblesRoot = GameObject.Find("Collectibles");
            if (collectiblesRoot == null)
            {
                var levelRoot = GameObject.Find("LevelRoot");
                collectiblesRoot = new GameObject("Collectibles");
                if (levelRoot != null)
                {
                    collectiblesRoot.transform.SetParent(levelRoot.transform, false);
                }
            }

            PlaceIfMissing(collectiblesRoot.transform, "Coin_A", prefab, new Vector3(-2.5f, -1.4f, 0f));
            PlaceIfMissing(collectiblesRoot.transform, "Coin_B", prefab, new Vector3(0.5f, -0.6f, 0f));
            PlaceIfMissing(collectiblesRoot.transform, "Coin_C", prefab, new Vector3(3.2f, -1.4f, 0f));
            PlaceIfMissing(collectiblesRoot.transform, "Coin_D", prefab, new Vector3(5.8f, 0.4f, 0f));
            PlaceIfMissing(collectiblesRoot.transform, "Coin_E", prefab, new Vector3(8.2f, 2.2f, 0f));

            EnsureCounterHud();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void EnsureCounterHud()
        {
            var canvas = GameObject.Find("GameplayFlowCanvas");
            if (canvas == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] GameplayFlowCanvas not found; skipping counter UI.");
                return;
            }

            var existing = canvas.transform.Find("CollectibleCounterHud");
            if (existing != null)
            {
                return;
            }

            var hud = new GameObject("CollectibleCounterHud", typeof(RectTransform));
            hud.transform.SetParent(canvas.transform, false);
            var rect = hud.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -20f);
            rect.sizeDelta = new Vector2(280f, 70f);

            var coinLabel = CreateHudLabel(hud.transform, "CoinText", "Coins: 0", new Vector2(0f, -4f));
            var scoreLabel = CreateHudLabel(hud.transform, "ScoreText", "Score: 0", new Vector2(0f, -34f));

            var ui = hud.AddComponent<CollectibleCounterUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("coinText").objectReferenceValue = coinLabel;
            so.FindProperty("scoreText").objectReferenceValue = scoreLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Text CreateHudLabel(Transform parent, string name, string value, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(280f, 28f);

            var text = go.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.raycastTarget = false;
            return text;
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

        private static Sprite CreateCoinSprite(string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > 14.5f)
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                    else if (dist > 12.5f)
                    {
                        pixels[y * size + x] = new Color(0.75f, 0.45f, 0.05f, 1f);
                    }
                    else
                    {
                        var highlight = dist < 6f ? 1.1f : 1f;
                        pixels[y * size + x] = new Color(1f * highlight, 0.84f * highlight, 0.2f, 1f);
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static AudioClip CreateCoinSfx(string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int sampleRate = 22050;
            const float duration = 0.12f;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var env = 1f - (t / duration);
                var tone = Mathf.Sin(2f * Mathf.PI * 980f * t) * 0.55f
                           + Mathf.Sin(2f * Mathf.PI * 1470f * t) * 0.25f;
                samples[i] = tone * env * env;
            }

            WriteWavMono16(assetPath, samples, sampleRate);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if (importer != null)
            {
                var settings = new AudioImporterSampleSettings
                {
                    loadType = AudioClipLoadType.DecompressOnLoad,
                    compressionFormat = AudioCompressionFormat.PCM,
                    sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate,
                    quality = 1f
                };
                importer.defaultSampleSettings = settings;
                importer.forceToMono = true;
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
