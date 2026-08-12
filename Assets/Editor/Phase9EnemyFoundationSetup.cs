// Filename: Phase9EnemyFoundationSetup.cs
// Folder: Assets/Editor/
// Purpose: Creates Enemy tag/layer, Crawlbug example prefab, and places samples (Phase 9).
// Menu: Bounder Trail/Phase 9/Setup Enemy Foundation
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase9EnemyFoundationSetup.SetupEnemyFoundation

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Enemies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase9EnemyFoundationSetup
    {
        private const string PrefabPath = "Assets/Prefabs/Enemies/Enemy_Crawlbug.prefab";
        private const string SpritePath = "Assets/Art/Enemies/Crawlbug_Placeholder.png";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Bounder Trail/Phase 9/Setup Enemy Foundation")]
        public static void SetupEnemyFoundation()
        {
            EnsureTag("Enemy");
            EnsureLayer("Enemy");
            EnsureFolder("Assets/Prefabs", "Enemies");
            EnsureFolder("Assets/Art", "Enemies");

            var sprite = CreateColorSprite(SpritePath, new Color(0.85f, 0.25f, 0.25f), 28, 22);
            var prefab = CreateCrawlbugPrefab(sprite);
            PlaceSamples(prefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 9 enemy foundation ready.");
        }

        private static GameObject CreateCrawlbugPrefab(Sprite sprite)
        {
            var go = new GameObject("Enemy_Crawlbug");
            go.tag = "Enemy";
            go.layer = LayerMask.NameToLayer("Enemy");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 8;

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.gravityScale = 3f;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.85f, 0.7f);

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(go.transform, false);
            groundCheck.transform.localPosition = new Vector3(0.35f, -0.4f, 0f);

            var wallCheck = new GameObject("WallCheck");
            wallCheck.transform.SetParent(go.transform, false);
            wallCheck.transform.localPosition = new Vector3(0.45f, 0f, 0f);

            var health = go.AddComponent<EnemyHealth>();
            var mover = go.AddComponent<EnemyMover>();
            var sensor = go.AddComponent<EnemySensor>();
            var brain = go.AddComponent<EnemyBrain>();
            go.AddComponent<EnemyContact>();

            var healthSo = new SerializedObject(health);
            healthSo.FindProperty("maxHealth").intValue = 1;
            healthSo.ApplyModifiedPropertiesWithoutUndo();

            var moverSo = new SerializedObject(mover);
            moverSo.FindProperty("rigidBody").objectReferenceValue = body;
            moverSo.FindProperty("spriteRenderer").objectReferenceValue = sr;
            moverSo.FindProperty("moveSpeed").floatValue = 2f;
            moverSo.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
            moverSo.FindProperty("wallCheck").objectReferenceValue = wallCheck.transform;
            moverSo.FindProperty("groundLayers").intValue = LayerMask.GetMask("Ground");
            moverSo.ApplyModifiedPropertiesWithoutUndo();

            var sensorSo = new SerializedObject(sensor);
            sensorSo.FindProperty("radius").floatValue = 4f;
            sensorSo.FindProperty("targetLayers").intValue = LayerMask.GetMask("Player");
            sensorSo.ApplyModifiedPropertiesWithoutUndo();

            var brainSo = new SerializedObject(brain);
            brainSo.FindProperty("health").objectReferenceValue = health;
            brainSo.FindProperty("mover").objectReferenceValue = mover;
            brainSo.FindProperty("sensor").objectReferenceValue = sensor;
            brainSo.FindProperty("canPatrol").boolValue = true;
            brainSo.FindProperty("canChase").boolValue = false; // foundation default: patrol only
            brainSo.FindProperty("canAttack").boolValue = false;
            brainSo.FindProperty("initialState").enumValueIndex = (int)EnemyStateId.Patrol;
            brainSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void PlaceSamples(GameObject prefab)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var enemiesRoot = GameObject.Find("Enemies");
            if (enemiesRoot == null)
            {
                var levelRoot = GameObject.Find("LevelRoot");
                if (levelRoot != null)
                {
                    enemiesRoot = new GameObject("Enemies");
                    enemiesRoot.transform.SetParent(levelRoot.transform, false);
                }
            }

            if (enemiesRoot == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] No Enemies folder found.");
                return;
            }

            // Hide/remove old empty markers that occupy sample spots if desired — leave markers, add real enemies.
            PlaceIfMissing(enemiesRoot.transform, "Enemy_Crawlbug_A", prefab, new Vector3(1.2f, -1.9f, 0f));
            PlaceIfMissing(enemiesRoot.transform, "Enemy_Crawlbug_B", prefab, new Vector3(6.2f, -1.9f, 0f));

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
            System.IO.File.WriteAllBytes(assetPath, texture.EncodeToPNG());
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
