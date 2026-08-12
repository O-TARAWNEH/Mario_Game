// Filename: Phase15CheckpointsSetup.cs
// Folder: Assets/Editor/
// Purpose: Checkpoint prefab, RespawnSystem wiring, enemy soft-death, samples (Phase 15).
// Menu: Bounder Trail/Phase 15/Setup Checkpoints And Respawn
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase15CheckpointsSetup.SetupCheckpointsAndRespawn

#if UNITY_EDITOR
using System.IO;
using BounderTrail.CameraSystem;
using BounderTrail.Core;
using BounderTrail.Enemies;
using BounderTrail.Levels;
using BounderTrail.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase15CheckpointsSetup
    {
        private const string PrefabPath = "Assets/Prefabs/World/Checkpoint_Flag.prefab";
        private const string ArtPath = "Assets/Art/World/Checkpoint_Flag.png";
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Bounder Trail/Phase 15/Setup Checkpoints And Respawn")]
        public static void SetupCheckpointsAndRespawn()
        {
            EnsureFolder("Assets/Prefabs", "World");
            EnsureFolder("Assets/Art", "World");
            EnsureTag("Checkpoint");
            EnsureLayer("Checkpoint");

            var prefab = CreateCheckpointPrefab();
            WireEnemyPrefabsForRespawn();
            WirePlayerPrefab();
            WireGameplayScene(prefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 15 checkpoints and respawn ready.");
        }

        private static GameObject CreateCheckpointPrefab()
        {
            var sprite = CreateFlagSprite(ArtPath);
            var go = new GameObject("Checkpoint_Flag");
            go.tag = "Checkpoint";
            var layer = LayerMask.NameToLayer("Checkpoint");
            if (layer >= 0)
            {
                go.layer = layer;
            }

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 7;

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(0.9f, 1.6f);

            var respawn = new GameObject("RespawnPoint");
            respawn.transform.SetParent(go.transform, false);
            respawn.transform.localPosition = new Vector3(0.35f, 0f, 0f);

            var checkpoint = go.AddComponent<Checkpoint>();
            var so = new SerializedObject(checkpoint);
            so.FindProperty("respawnPoint").objectReferenceValue = respawn.transform;
            so.FindProperty("flagRenderer").objectReferenceValue = sr;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void WireEnemyPrefabsForRespawn()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Enemies" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Projectile"))
                {
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (root.GetComponent<EnemyHealth>() == null)
                    {
                        continue;
                    }

                    var state = root.GetComponent<EnemyRespawnState>();
                    if (state == null)
                    {
                        state = root.AddComponent<EnemyRespawnState>();
                    }

                    var health = root.GetComponent<EnemyHealth>();
                    var brain = root.GetComponent<EnemyBrain>();
                    var mover = root.GetComponent<EnemyMover>();

                    var so = new SerializedObject(state);
                    so.FindProperty("health").objectReferenceValue = health;
                    so.FindProperty("brain").objectReferenceValue = brain;
                    so.FindProperty("mover").objectReferenceValue = mover;
                    so.FindProperty("softDeath").boolValue = true;
                    so.ApplyModifiedPropertiesWithoutUndo();

                    if (health != null)
                    {
                        var healthSo = new SerializedObject(health);
                        healthSo.FindProperty("destroyOnDeath").boolValue = false;
                        healthSo.ApplyModifiedPropertiesWithoutUndo();
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void WirePlayerPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var death = root.GetComponent<PlayerDeath>();
                if (death != null)
                {
                    var so = new SerializedObject(death);
                    so.FindProperty("triggerGameOverOnDeath").boolValue = false;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void WireGameplayScene(GameObject checkpointPrefab)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            var levelRoot = GameObject.Find("LevelRoot");
            var checkpointsRoot = GameObject.Find("Checkpoints");
            if (checkpointsRoot == null)
            {
                checkpointsRoot = new GameObject("Checkpoints");
                if (levelRoot != null)
                {
                    checkpointsRoot.transform.SetParent(levelRoot.transform, false);
                }
            }

            var respawnSystem = Object.FindAnyObjectByType<RespawnSystem>();
            if (respawnSystem == null)
            {
                var host = levelRoot != null ? levelRoot : new GameObject("RespawnSystem");
                if (levelRoot == null)
                {
                    host.name = "RespawnSystem";
                }

                respawnSystem = host.GetComponent<RespawnSystem>();
                if (respawnSystem == null)
                {
                    respawnSystem = host.AddComponent<RespawnSystem>();
                }
            }

            var start = Object.FindAnyObjectByType<PlayerSpawnPoint>();
            var player = GameObject.Find("Player_Pip");
            var camera = Object.FindAnyObjectByType<CameraFollow2D>();

            var so = new SerializedObject(respawnSystem);
            so.FindProperty("levelStartSpawn").objectReferenceValue = start;
            so.FindProperty("player").objectReferenceValue = player != null ? player.transform : null;
            so.FindProperty("cameraFollow").objectReferenceValue = camera;
            so.FindProperty("startingLives").intValue = 3;
            so.FindProperty("respawnDelay").floatValue = 0.85f;
            so.FindProperty("resetEnemiesOnRespawn").boolValue = true;
            so.FindProperty("resetMovingHazardsOnRespawn").boolValue = true;
            so.FindProperty("keepCollectedPickups").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (player != null)
            {
                var death = player.GetComponent<PlayerDeath>();
                if (death != null)
                {
                    var deathSo = new SerializedObject(death);
                    deathSo.FindProperty("triggerGameOverOnDeath").boolValue = false;
                    deathSo.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // Ensure scene enemy instances also have respawn state.
            var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var health in enemies)
            {
                if (health == null || health.GetComponent<EnemyProjectile>() != null)
                {
                    continue;
                }

                var state = health.GetComponent<EnemyRespawnState>();
                if (state == null)
                {
                    state = health.gameObject.AddComponent<EnemyRespawnState>();
                }

                var stateSo = new SerializedObject(state);
                stateSo.FindProperty("health").objectReferenceValue = health;
                stateSo.FindProperty("brain").objectReferenceValue = health.GetComponent<EnemyBrain>();
                stateSo.FindProperty("mover").objectReferenceValue = health.GetComponent<EnemyMover>();
                stateSo.FindProperty("softDeath").boolValue = true;
                stateSo.ApplyModifiedPropertiesWithoutUndo();

                var healthSo = new SerializedObject(health);
                healthSo.FindProperty("destroyOnDeath").boolValue = false;
                healthSo.ApplyModifiedPropertiesWithoutUndo();
            }

            PlaceIfMissing(checkpointsRoot.transform, "Checkpoint_A", checkpointPrefab, new Vector3(2.8f, -1.9f, 0f));
            PlaceIfMissing(checkpointsRoot.transform, "Checkpoint_B", checkpointPrefab, new Vector3(7.4f, 1.4f, 0f));

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

        private static Sprite CreateFlagSprite(string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            const int width = 24;
            const int height = 40;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            var pole = new Color(0.55f, 0.4f, 0.25f, 1f);
            var flag = new Color(0.35f, 0.95f, 0.55f, 1f);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    Color c = Color.clear;
                    if (x >= 3 && x <= 5)
                    {
                        c = pole;
                    }

                    if (y >= height - 18 && y <= height - 4 && x >= 6 && x <= 20)
                    {
                        c = flag;
                    }

                    pixels[y * width + x] = c;
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
