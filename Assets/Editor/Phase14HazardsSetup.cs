// Filename: Phase14HazardsSetup.cs
// Folder: Assets/Editor/
// Purpose: Creates pit, spike, fire, and moving hazard prefabs + samples (Phase 14).
// Menu: Bounder Trail/Phase 14/Setup Hazards
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase14HazardsSetup.SetupHazards

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Core;
using BounderTrail.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase14HazardsSetup
    {
        private const string PrefabFolder = "Assets/Prefabs/World";
        private const string ArtFolder = "Assets/Art/World";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Bounder Trail/Phase 14/Setup Hazards")]
        public static void SetupHazards()
        {
            EnsureFolder("Assets/Prefabs", "World");
            EnsureFolder("Assets/Art", "World");

            EnsureTag("Hazard");
            EnsureLayer("Hazard");

            var pit = CreateHazardPrefab(
                $"{PrefabFolder}/Hazard_DeathZone.prefab",
                "Hazard_DeathZone",
                HazardResponse.InstantKill,
                CreateColorSprite($"{ArtFolder}/Hazard_DeathZone.png", new Color(0.15f, 0.05f, 0.2f, 0.85f), 96, 24),
                new Vector2(4f, 1f),
                moving: false);

            var spikes = CreateHazardPrefab(
                $"{PrefabFolder}/Hazard_Spikes.prefab",
                "Hazard_Spikes",
                HazardResponse.ContactDamage,
                CreateSpikeSprite($"{ArtFolder}/Hazard_Spikes.png"),
                new Vector2(1.6f, 0.55f),
                moving: false,
                damage: 1,
                interval: 0.2f);

            var fire = CreateHazardPrefab(
                $"{PrefabFolder}/Hazard_Fire.prefab",
                "Hazard_Fire",
                HazardResponse.DamageOverTime,
                CreateColorSprite($"{ArtFolder}/Hazard_Fire.png", new Color(1f, 0.45f, 0.1f, 1f), 40, 28),
                new Vector2(1.4f, 0.9f),
                moving: false,
                damage: 1,
                interval: 0.45f);

            var moving = CreateHazardPrefab(
                $"{PrefabFolder}/Hazard_MovingSpike.prefab",
                "Hazard_MovingSpike",
                HazardResponse.ContactDamage,
                CreateSpikeSprite($"{ArtFolder}/Hazard_MovingSpike.png"),
                new Vector2(1.1f, 0.5f),
                moving: true,
                damage: 1,
                interval: 0.2f);

            PlaceSamples(pit, spikes, fire, moving);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 14 hazards ready.");
        }

        private static GameObject CreateHazardPrefab(
            string path,
            string objectName,
            HazardResponse response,
            Sprite sprite,
            Vector2 size,
            bool moving,
            int damage = 1,
            float interval = 0.45f)
        {
            var go = new GameObject(objectName);
            go.tag = "Hazard";
            var layer = LayerMask.NameToLayer("Hazard");
            if (layer >= 0)
            {
                go.layer = layer;
            }

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = Vector2.one;

            var hazard = go.AddComponent<EnvironmentalHazard>();
            var hazardSo = new SerializedObject(hazard);
            hazardSo.FindProperty("response").enumValueIndex = (int)response;
            hazardSo.FindProperty("damage").intValue = damage;
            hazardSo.FindProperty("damageInterval").floatValue = interval;
            hazardSo.FindProperty("useTrigger").boolValue = true;
            hazardSo.ApplyModifiedPropertiesWithoutUndo();

            if (moving)
            {
                var body = go.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;
                body.freezeRotation = true;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;

                var mover = go.AddComponent<MovingHazard>();
                var moverSo = new SerializedObject(mover);
                moverSo.FindProperty("pointA").vector2Value = new Vector2(-1.5f, 0f);
                moverSo.FindProperty("pointB").vector2Value = new Vector2(1.5f, 0f);
                moverSo.FindProperty("pointsAreLocal").boolValue = true;
                moverSo.FindProperty("speed").floatValue = 2.5f;
                moverSo.ApplyModifiedPropertiesWithoutUndo();
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void PlaceSamples(GameObject pit, GameObject spikes, GameObject fire, GameObject moving)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var hazardsRoot = GameObject.Find("Hazards");
            if (hazardsRoot == null)
            {
                var levelRoot = GameObject.Find("LevelRoot");
                hazardsRoot = new GameObject("Hazards");
                if (levelRoot != null)
                {
                    hazardsRoot.transform.SetParent(levelRoot.transform, false);
                }
            }

            // Wide pit under a gap / below playfield.
            PlaceIfMissing(hazardsRoot.transform, "Hazard_DeathZone_A", pit, new Vector3(1.5f, -5.2f, 0f));
            PlaceIfMissing(hazardsRoot.transform, "Hazard_Spikes_A", spikes, new Vector3(-5.2f, -2.55f, 0f));
            PlaceIfMissing(hazardsRoot.transform, "Hazard_Fire_A", fire, new Vector3(4.4f, -2.35f, 0f));
            PlaceIfMissing(hazardsRoot.transform, "Hazard_MovingSpike_A", moving, new Vector3(8.6f, 0.9f, 0f));

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
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            return ImportSprite(assetPath);
        }

        private static Sprite CreateSpikeSprite(string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int width = 48;
            const int height = 24;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            var spike = new Color(0.75f, 0.78f, 0.85f, 1f);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    // Three triangular spikes along the strip.
                    var localX = (x % 16) / 15f;
                    var tip = 1f - (Mathf.Abs(localX - 0.5f) * 2f);
                    var threshold = y / (float)(height - 1);
                    pixels[y * width + x] = threshold <= tip * 0.95f ? spike : Color.clear;
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
