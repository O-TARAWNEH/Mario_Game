// Filename: Phase7LevelSystemSetup.cs
// Folder: Assets/Editor/
// Purpose: Creates level data, prefabs, tilemap foundation, and restructures Gameplay (Phase 7).
// Menu: Bounder Trail/Phase 7/Setup Level And World System
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase7LevelSystemSetup.SetupLevelAndWorldSystem

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Data;
using BounderTrail.Levels;
using BounderTrail.Player;
using BounderTrail.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BounderTrail.EditorTools
{
    public static class Phase7LevelSystemSetup
    {
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string DataFolder = "Assets/Data/Levels";
        private const string LevelDataPath = DataFolder + "/LevelData_GameplayPrototype.asset";
        private const string CatalogPath = DataFolder + "/LevelCatalog.asset";
        private const string PlatformPrefabPath = "Assets/Prefabs/World/Platform_Basic.prefab";
        private const string GroundPrefabPath = "Assets/Prefabs/World/Ground_Basic.prefab";
        private const string TilePath = "Assets/Art/Tiles/Tile_GroundBasic.asset";
        private const string GroundSpritePath = "Assets/Art/World/Ground_Placeholder.png";

        [MenuItem("Bounder Trail/Phase 7/Setup Level And World System")]
        public static void SetupLevelAndWorldSystem()
        {
            EnsureFolder("Assets/Data", "Levels");
            EnsureFolder("Assets/Prefabs", "World");
            EnsureFolder("Assets/Art", "Tiles");

            var levelData = CreateLevelData();
            var catalog = CreateCatalog(levelData);
            CreateWorldPrefabs();
            var groundTile = CreateGroundTile();

            SetupGameplayScene(groundTile);
            SetupBootstrap(catalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 7 level and world system ready.");
        }

        private static LevelData CreateLevelData()
        {
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(LevelDataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(data, LevelDataPath);
            }

            var so = new SerializedObject(data);
            so.FindProperty("levelId").stringValue = "gameplay_prototype";
            so.FindProperty("displayName").stringValue = "Gameplay Prototype";
            so.FindProperty("sceneName").stringValue = ProjectConstants.GameplaySceneName;
            so.FindProperty("buildIndex").intValue = 0;
            so.FindProperty("designerNotes").stringValue = "Prototype level used while building systems. Replace with campaign levels later.";
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static LevelCatalog CreateCatalog(LevelData levelData)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var so = new SerializedObject(catalog);
            var levels = so.FindProperty("levels");
            levels.arraySize = 1;
            levels.GetArrayElementAtIndex(0).objectReferenceValue = levelData;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void CreateWorldPrefabs()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GroundSpritePath);
            CreatePlatformPrefab(PlatformPrefabPath, "Platform_Basic", sprite, PlatformPiece.PlatformKind.Solid, new Vector3(3f, 0.6f, 1f));
            CreatePlatformPrefab(GroundPrefabPath, "Ground_Basic", sprite, PlatformPiece.PlatformKind.Ground, new Vector3(4f, 1f, 1f));
        }

        private static void CreatePlatformPrefab(
            string path,
            string objectName,
            Sprite sprite,
            PlatformPiece.PlatformKind kind,
            Vector3 scale)
        {
            var go = new GameObject(objectName);
            go.tag = "Ground";
            go.layer = LayerMask.NameToLayer("Ground");
            go.transform.localScale = scale;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 0;

            go.AddComponent<BoxCollider2D>().size = Vector2.one;

            var piece = go.AddComponent<PlatformPiece>();
            var pieceSo = new SerializedObject(piece);
            pieceSo.FindProperty("kind").enumValueIndex = (int)kind;
            pieceSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static Tile CreateGroundTile()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GroundSpritePath);
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(TilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, TilePath);
            }

            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Sprite;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static void SetupGameplayScene(Tile groundTile)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            var oldLevel = GameObject.Find("_Level");
            var player = GameObject.Find("Player_Pip");
            var spawn = GameObject.Find("PlayerSpawn");
            var bounds = GameObject.Find("LevelBounds");
            var world = GameObject.Find("_World");

            // Create / reuse LevelRoot container.
            var rootObject = GameObject.Find("LevelRoot");
            if (rootObject == null)
            {
                rootObject = new GameObject("LevelRoot");
            }

            var levelRoot = rootObject.GetComponent<LevelRoot>();
            if (levelRoot == null)
            {
                levelRoot = rootObject.AddComponent<LevelRoot>();
            }

            // Ensure hierarchy folders.
            var platforms = EnsureChild(rootObject.transform, "Platforms");
            var enemies = EnsureChild(rootObject.transform, "Enemies");
            var collectibles = EnsureChild(rootObject.transform, "Collectibles");
            var hazards = EnsureChild(rootObject.transform, "Hazards");
            var checkpoints = EnsureChild(rootObject.transform, "Checkpoints");
            var decorations = EnsureChild(rootObject.transform, "Decorations");
            var tilemaps = EnsureChild(rootObject.transform, "Tilemaps");

            // Move existing world pieces under Platforms.
            if (world != null)
            {
                while (world.transform.childCount > 0)
                {
                    world.transform.GetChild(0).SetParent(platforms, true);
                }

                Object.DestroyImmediate(world);
            }

            // Tag existing platforms.
            foreach (Transform child in platforms)
            {
                if (child.GetComponent<PlatformPiece>() == null)
                {
                    var piece = child.gameObject.AddComponent<PlatformPiece>();
                    var kind = child.name.StartsWith("Ground")
                        ? PlatformPiece.PlatformKind.Ground
                        : PlatformPiece.PlatformKind.Solid;
                    var so = new SerializedObject(piece);
                    so.FindProperty("kind").enumValueIndex = (int)kind;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            if (bounds != null)
            {
                bounds.transform.SetParent(rootObject.transform, true);
            }

            if (spawn != null)
            {
                spawn.transform.SetParent(rootObject.transform, true);
            }

            // End point.
            var endObject = GameObject.Find("LevelEnd");
            if (endObject == null)
            {
                endObject = new GameObject("LevelEnd");
                endObject.transform.SetParent(rootObject.transform, false);
                endObject.transform.position = new Vector3(8.5f, 3.2f, 0f);
                var box = endObject.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                box.size = new Vector2(1f, 2f);
                endObject.AddComponent<LevelEndPoint>();

                var flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(flag.GetComponent<Collider>());
                flag.name = "EndFlagVisual";
                flag.transform.SetParent(endObject.transform, false);
                flag.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                flag.transform.localScale = new Vector3(0.4f, 1.2f, 0.2f);
                // 3D primitive may not suit 2D well; replace with sprite quad via SpriteRenderer.
                Object.DestroyImmediate(flag);
                var visual = new GameObject("EndFlagVisual");
                visual.transform.SetParent(endObject.transform, false);
                visual.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                visual.transform.localScale = new Vector3(0.6f, 1.2f, 1f);
                var sr = visual.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GroundSpritePath);
                sr.color = new Color(0.3f, 1f, 0.45f, 1f);
                sr.sortingOrder = 5;
            }

            // Sample content markers (not functional gameplay yet).
            CreateMarker(enemies, "Marker_Enemy_A", LevelContentMarkerType.Enemy, new Vector3(1f, -1.8f, 0f));
            CreateMarker(collectibles, "Marker_Collectible_A", LevelContentMarkerType.Collectible, new Vector3(-1f, 0.3f, 0f));
            CreateMarker(hazards, "Marker_Hazard_A", LevelContentMarkerType.Hazard, new Vector3(4.5f, -2.2f, 0f));
            CreateMarker(checkpoints, "Marker_Checkpoint_A", LevelContentMarkerType.Checkpoint, new Vector3(3f, 1.6f, 0f));
            CreateMarker(decorations, "Marker_Decor_A", LevelContentMarkerType.Decoration, new Vector3(-4f, -1.5f, 0f));

            // Tilemap foundation.
            var gridObject = tilemaps.Find("Grid");
            Grid grid;
            if (gridObject == null)
            {
                var gridGo = new GameObject("Grid");
                gridGo.transform.SetParent(tilemaps, false);
                grid = gridGo.AddComponent<Grid>();
                grid.cellSize = new Vector3(1f, 1f, 0f);
                gridObject = gridGo.transform;
            }
            else
            {
                grid = gridObject.GetComponent<Grid>();
            }

            var groundMapTransform = gridObject.Find("Tilemap_Ground");
            Tilemap groundMap;
            if (groundMapTransform == null)
            {
                var mapGo = new GameObject("Tilemap_Ground");
                mapGo.transform.SetParent(gridObject, false);
                mapGo.layer = LayerMask.NameToLayer("Ground");
                mapGo.tag = "Ground";
                groundMap = mapGo.AddComponent<Tilemap>();
                var renderer = mapGo.AddComponent<TilemapRenderer>();
                renderer.sortingOrder = -2;
                var collider = mapGo.AddComponent<TilemapCollider2D>();
                collider.compositeOperation = Collider2D.CompositeOperation.Merge;
                var rb = mapGo.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                mapGo.AddComponent<CompositeCollider2D>();
            }
            else
            {
                groundMap = groundMapTransform.GetComponent<Tilemap>();
            }

            // Paint a short tile strip as a foundation example (does not replace existing platforms).
            if (groundTile != null && groundMap != null)
            {
                for (var x = -2; x <= 2; x++)
                {
                    groundMap.SetTile(new Vector3Int(x, -5, 0), groundTile);
                }
            }

            if (player != null)
            {
                player.transform.SetParent(rootObject.transform, true);
            }

            if (oldLevel != null && oldLevel != rootObject)
            {
                // Move leftover children then remove old organizer.
                while (oldLevel.transform.childCount > 0)
                {
                    oldLevel.transform.GetChild(0).SetParent(rootObject.transform, true);
                }

                Object.DestroyImmediate(oldLevel);
            }

            // Wire LevelRoot references.
            var rootSo = new SerializedObject(levelRoot);
            rootSo.FindProperty("levelId").stringValue = "gameplay_prototype";
            rootSo.FindProperty("displayName").stringValue = "Gameplay Prototype";
            rootSo.FindProperty("levelBounds").objectReferenceValue = bounds != null ? bounds.GetComponent<LevelBounds>() : null;
            rootSo.FindProperty("startPoint").objectReferenceValue = spawn != null ? spawn.GetComponent<PlayerSpawnPoint>() : null;
            rootSo.FindProperty("endPoint").objectReferenceValue = endObject.GetComponent<LevelEndPoint>();
            rootSo.FindProperty("platformsRoot").objectReferenceValue = platforms;
            rootSo.FindProperty("enemiesRoot").objectReferenceValue = enemies;
            rootSo.FindProperty("collectiblesRoot").objectReferenceValue = collectibles;
            rootSo.FindProperty("hazardsRoot").objectReferenceValue = hazards;
            rootSo.FindProperty("checkpointsRoot").objectReferenceValue = checkpoints;
            rootSo.FindProperty("decorationsRoot").objectReferenceValue = decorations;
            rootSo.FindProperty("tilemapRoot").objectReferenceValue = tilemaps;
            rootSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void SetupBootstrap(LevelCatalog catalog)
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrap == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Bootstrap object missing.");
                return;
            }

            if (bootstrap.GetComponent<GameBootstrap>() == null)
            {
                bootstrap.AddComponent<GameBootstrap>();
            }

            if (bootstrap.GetComponent<GameStateManager>() == null)
            {
                bootstrap.AddComponent<GameStateManager>();
            }

            var loader = bootstrap.GetComponent<LevelLoader>();
            if (loader == null)
            {
                loader = bootstrap.AddComponent<LevelLoader>();
            }

            var so = new SerializedObject(loader);
            var catalogProp = so.FindProperty("catalog");
            if (catalogProp == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] LevelLoader.catalog property not found.");
            }
            else
            {
                catalogProp.objectReferenceValue = catalog;
            }

            so.FindProperty("startingLevelIndex").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(loader);
            EditorUtility.SetDirty(bootstrap);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);

            // Verify assignment after save.
            var verify = new SerializedObject(loader);
            var assigned = verify.FindProperty("catalog")?.objectReferenceValue;
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] LevelLoader catalog assigned: {(assigned != null ? assigned.name : "NULL")}");
        }

        private static void CreateMarker(Transform parent, string name, LevelContentMarkerType type, Vector3 position)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                return;
            }

            var marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.position = position;
            var component = marker.AddComponent<LevelContentMarker>();
            var so = new SerializedObject(component);
            so.FindProperty("markerType").enumValueIndex = (int)type;
            so.FindProperty("contentId").stringValue = name;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform EnsureChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
