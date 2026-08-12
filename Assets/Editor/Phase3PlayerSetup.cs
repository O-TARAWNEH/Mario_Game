// Filename: Phase3PlayerSetup.cs
// Folder: Assets/Editor/
// Purpose: Creates player prefab, tags/layers, spawn, and test platforms in Gameplay (Phase 3).
// Dependencies: BounderTrail.Player.*, BounderTrail.Core.ProjectConstants
//
// Menu: Bounder Trail/Phase 3/Setup Player Foundation
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase3PlayerSetup.SetupPlayerFoundation

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase3PlayerSetup
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string ArtPlayerFolder = "Assets/Art/Player";
        private const string ArtWorldFolder = "Assets/Art/World";
        private const string PlayerSpritePath = ArtPlayerFolder + "/Pip_Placeholder.png";
        private const string GroundSpritePath = ArtWorldFolder + "/Ground_Placeholder.png";

        private const string PlayerTag = "Player";
        private const string GroundTag = "Ground";
        private const string PlayerLayerName = "Player";
        private const string GroundLayerName = "Ground";

        [MenuItem("Bounder Trail/Phase 3/Setup Player Foundation")]
        public static void SetupPlayerFoundation()
        {
            EnsureTag(PlayerTag);
            EnsureTag(GroundTag);
            EnsureLayer(PlayerLayerName);
            EnsureLayer(GroundLayerName);
            AssetDatabase.SaveAssets();

            EnsureFolder("Assets/Art", "Player");
            EnsureFolder("Assets/Art", "World");
            EnsureFolder("Assets/Prefabs", "Player");

            var playerSprite = CreateOrLoadColorSprite(PlayerSpritePath, new Color(0.25f, 0.85f, 1f, 1f), 32, 32, 32f);
            var groundSprite = CreateOrLoadColorSprite(GroundSpritePath, new Color(0.35f, 0.7f, 0.3f, 1f), 64, 16, 32f);

            var playerPrefab = CreatePlayerPrefab(playerSprite);
            SetupGameplayScene(playerPrefab, groundSprite);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 3 player foundation ready.");
        }

        private static GameObject CreatePlayerPrefab(Sprite playerSprite)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(PlayerPrefabPath);
            }

            var player = new GameObject("Player_Pip");
            player.tag = PlayerTag;
            player.layer = LayerMask.NameToLayer(PlayerLayerName);

            var renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = playerSprite;
            renderer.sortingOrder = 10;

            var body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.gravityScale = 3.5f;

            var collider = player.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.7f, 0.95f);
            collider.offset = new Vector2(0f, 0f);

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(player.transform, false);
            groundCheck.transform.localPosition = new Vector3(0f, -0.48f, 0f);

            var sensor = player.AddComponent<PlayerGroundSensor>();
            var controller = player.AddComponent<PlayerController>();

            var sensorSo = new SerializedObject(sensor);
            sensorSo.FindProperty("groundCheckPoint").objectReferenceValue = groundCheck.transform;
            sensorSo.FindProperty("checkRadius").floatValue = 0.12f;
            sensorSo.FindProperty("probeDistance").floatValue = 0.18f;
            sensorSo.FindProperty("edgeProbeOffset").floatValue = 0.28f;
            sensorSo.FindProperty("maxSlopeAngle").floatValue = 50f;
            sensorSo.FindProperty("groundLayers").intValue = LayerMask.GetMask(GroundLayerName);
            sensorSo.ApplyModifiedPropertiesWithoutUndo();

            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("rigidBody").objectReferenceValue = body;
            controllerSo.FindProperty("groundSensor").objectReferenceValue = sensor;
            controllerSo.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            controllerSo.FindProperty("walkSpeed").floatValue = 6.5f;
            controllerSo.FindProperty("runSpeed").floatValue = 9.5f;
            controllerSo.FindProperty("acceleration").floatValue = 75f;
            controllerSo.FindProperty("deceleration").floatValue = 85f;
            controllerSo.FindProperty("airAcceleration").floatValue = 45f;
            controllerSo.FindProperty("airDeceleration").floatValue = 40f;
            controllerSo.FindProperty("airControl").floatValue = 0.75f;
            controllerSo.FindProperty("jumpForce").floatValue = 15f;
            controllerSo.FindProperty("coyoteTime").floatValue = 0.1f;
            controllerSo.FindProperty("jumpBufferTime").floatValue = 0.12f;
            controllerSo.FindProperty("jumpCutMultiplier").floatValue = 0.45f;
            controllerSo.FindProperty("jumpCutGravityMultiplier").floatValue = 2.2f;
            controllerSo.FindProperty("gravity").floatValue = 3.2f;
            controllerSo.FindProperty("fallGravityMultiplier").floatValue = 1.55f;
            controllerSo.FindProperty("maximumFallSpeed").floatValue = 22f;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            Object.DestroyImmediate(player);
            return prefab;
        }

        private static void SetupGameplayScene(GameObject playerPrefab, Sprite groundSprite)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            // Remove previous Phase 3 world content if re-running setup.
            DestroyIfExists("_Level");
            DestroyIfExists("Player_Pip");
            DestroyIfExists("PlayerSpawn");

            var levelRoot = new GameObject("_Level");
            var worldRoot = new GameObject("_World");
            worldRoot.transform.SetParent(levelRoot.transform, false);

            var spawnObject = new GameObject("PlayerSpawn");
            spawnObject.transform.SetParent(levelRoot.transform, false);
            spawnObject.transform.position = new Vector3(-5.5f, -1.5f, 0f);
            spawnObject.AddComponent<PlayerSpawnPoint>();

            CreatePlatform(worldRoot.transform, "Ground_Main", groundSprite, new Vector3(0f, -3f, 0f), new Vector3(18f, 1f, 1f));
            CreatePlatform(worldRoot.transform, "Platform_A", groundSprite, new Vector3(-2f, -0.5f, 0f), new Vector3(3.5f, 0.6f, 1f));
            CreatePlatform(worldRoot.transform, "Platform_B", groundSprite, new Vector3(3.5f, 1f, 0f), new Vector3(3f, 0.6f, 1f));
            CreatePlatform(worldRoot.transform, "Platform_C", groundSprite, new Vector3(7.5f, 2.5f, 0f), new Vector3(2.5f, 0.6f, 1f));

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.name = "Player_Pip";
            player.transform.SetParent(levelRoot.transform, true);
            var spawn = spawnObject.GetComponent<PlayerSpawnPoint>();
            spawn.PlacePlayer(player.transform);

            // Keep camera framed on the test arena (no follow system yet).
            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.orthographic = true;
                camera.orthographicSize = 6f;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void CreatePlatform(Transform parent, string name, Sprite sprite, Vector3 position, Vector3 scale)
        {
            var platform = new GameObject(name);
            platform.transform.SetParent(parent, false);
            platform.transform.position = position;
            platform.transform.localScale = scale;
            platform.tag = GroundTag;
            platform.layer = LayerMask.NameToLayer(GroundLayerName);

            var renderer = platform.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.sortingOrder = 0;
            renderer.color = Color.white;

            var box = platform.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;
        }

        private static Sprite CreateOrLoadColorSprite(string assetPath, Color color, int width, int height, float pixelsPerUnit)
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

            var png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
            System.IO.File.WriteAllBytes(assetPath, png);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void DestroyIfExists(string objectName)
        {
            var found = GameObject.Find(objectName);
            if (found != null)
            {
                Object.DestroyImmediate(found);
            }
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void EnsureTag(string tag)
        {
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(asset[0]);
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
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(asset[0]);
            var layers = so.FindProperty("layers");
            for (var i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    return;
                }
            }

            // User layers start at index 8.
            for (var i = 8; i < layers.arraySize; i++)
            {
                var layerProperty = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layerProperty.stringValue))
                {
                    layerProperty.stringValue = layerName;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    return;
                }
            }

            Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] No free user layer slot for '{layerName}'.");
        }
    }
}
#endif
