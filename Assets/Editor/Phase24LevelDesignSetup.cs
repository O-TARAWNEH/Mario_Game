// Filename: Phase24LevelDesignSetup.cs
// Folder: Assets/Editor/
// Purpose: Authors unique Level 01–03 layouts and repairs LevelCatalog (Phase 24).
// Menu: Bounder Trail/Phase 24/Setup Level Design
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase24LevelDesignSetup.SetupLevelDesign

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Data;
using BounderTrail.Levels;
using BounderTrail.Player;
using BounderTrail.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace BounderTrail.EditorTools
{
    public static class Phase24LevelDesignSetup
    {
        private const string DataFolder = "Assets/Data/Levels";
        private const string CatalogPath = DataFolder + "/LevelCatalog.asset";
        private const string PrefabWorld = "Assets/Prefabs/World";
        private const string PrefabEnemies = "Assets/Prefabs/Enemies";
        private const string PrefabItems = "Assets/Prefabs/Items";
        private const string GroundSpritePath = "Assets/Art/World/Ground_Placeholder.png";
        private const string GroundTilePath = "Assets/Art/Tiles/Tile_GroundBasic.asset";

        // Hazard prefab footprint helpers (match prefab local scales).
        private const float SpikeHalfHeight = 0.275f;
        private const float FireHalfHeight = 0.45f;
        private const float EnemyStandOffset = 0.35f;
        private const float PitDepthY = -4.6f;
        private const float PitHeight = 1.2f;

        private static readonly LevelSpec[] Campaign =
        {
            new LevelSpec(
                "level_01",
                "Lumen Meadows",
                ProjectConstants.Level01SceneName,
                $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
                $"{DataFolder}/LevelData_01_LumenMeadows.asset",
                0,
                "Theme: soft meadow path. Tutorial — flat trail, one taught gap, exit spike warning. Decor + ground tiles."),
            new LevelSpec(
                "level_02",
                "Cascade Cliffs",
                ProjectConstants.Level02SceneName,
                $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
                $"{DataFolder}/LevelData_02_CascadeCliffs.asset",
                1,
                "Theme: stepped cliff shelves. Medium — bounce/one-way, fire gate, Spikewatch. Pits align to shelf gaps."),
            new LevelSpec(
                "level_03",
                "Skybridge Spire",
                ProjectConstants.Level03SceneName,
                $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity",
                $"{DataFolder}/LevelData_03_SkybridgeSpire.asset",
                2,
                "Theme: sky bridges and movers. Hard — narrow bridges, mover crossing, stacked enemies, moving spike finale.")
        };

        private readonly struct LevelSpec
        {
            public readonly string LevelId;
            public readonly string DisplayName;
            public readonly string SceneName;
            public readonly string ScenePath;
            public readonly string DataPath;
            public readonly int BuildIndex;
            public readonly string DesignerNotes;

            public LevelSpec(
                string levelId,
                string displayName,
                string sceneName,
                string scenePath,
                string dataPath,
                int buildIndex,
                string designerNotes)
            {
                LevelId = levelId;
                DisplayName = displayName;
                SceneName = sceneName;
                ScenePath = scenePath;
                DataPath = dataPath;
                BuildIndex = buildIndex;
                DesignerNotes = designerNotes;
            }
        }

        private sealed class PrefabKit
        {
            public GameObject Solid;
            public GameObject OneWay;
            public GameObject Moving;
            public GameObject Bounce;
            public GameObject Exit;
            public GameObject Checkpoint;
            public GameObject DeathZone;
            public GameObject Spikes;
            public GameObject Fire;
            public GameObject MovingSpike;
            public GameObject Coin;
            public GameObject SpeedBurst;
            public GameObject GlowShield;
            public GameObject HeartDrop;
            public GameObject Crawlbug;
            public GameObject Dartling;
            public GameObject Hopmite;
            public GameObject Skimmer;
            public GameObject Spikewatch;
            public GameObject Spitter;
        }

        [MenuItem("Bounder Trail/Phase 24/Setup Level Design")]
        public static void SetupLevelDesign()
        {
            var kit = LoadPrefabs();
            if (kit == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 24 aborted — missing approved prefabs.");
                return;
            }

            var levelAssets = new LevelData[Campaign.Length];
            for (var i = 0; i < Campaign.Length; i++)
            {
                BuildLevel(Campaign[i], kit);
                UpdateLevelData(Campaign[i]);
            }

            AssetDatabase.SaveAssets();
            for (var i = 0; i < Campaign.Length; i++)
            {
                AssetDatabase.ImportAsset(Campaign[i].DataPath);
                levelAssets[i] = AssetDatabase.LoadAssetAtPath<LevelData>(Campaign[i].DataPath);
                if (levelAssets[i] == null)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Failed to load LevelData: {Campaign[i].DataPath}");
                }
            }

            UpdateCatalog(levelAssets);
            Phase6CameraSetup.SetupCameraSystem();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"{GameLog.ProjectPrefix}[Setup] Phase 24 complete: authored {Campaign.Length} campaign levels " +
                "(Lumen Meadows → Cascade Cliffs → Skybridge Spire). Secrets not included.");
        }

        private static PrefabKit LoadPrefabs()
        {
            var kit = new PrefabKit
            {
                Solid = LoadPrefab($"{PrefabWorld}/Platform_Solid.prefab"),
                OneWay = LoadPrefab($"{PrefabWorld}/Platform_OneWay.prefab"),
                Moving = LoadPrefab($"{PrefabWorld}/Platform_Moving.prefab"),
                Bounce = LoadPrefab($"{PrefabWorld}/BouncePad.prefab"),
                Exit = LoadPrefab($"{PrefabWorld}/LevelExitDoor.prefab"),
                Checkpoint = LoadPrefab($"{PrefabWorld}/Checkpoint_Flag.prefab"),
                DeathZone = LoadPrefab($"{PrefabWorld}/Hazard_DeathZone.prefab"),
                Spikes = LoadPrefab($"{PrefabWorld}/Hazard_Spikes.prefab"),
                Fire = LoadPrefab($"{PrefabWorld}/Hazard_Fire.prefab"),
                MovingSpike = LoadPrefab($"{PrefabWorld}/Hazard_MovingSpike.prefab"),
                Coin = LoadPrefab($"{PrefabItems}/Item_Coin.prefab"),
                SpeedBurst = LoadPrefab($"{PrefabItems}/Item_SpeedBurst.prefab"),
                GlowShield = LoadPrefab($"{PrefabItems}/Item_GlowShield.prefab"),
                HeartDrop = LoadPrefab($"{PrefabItems}/Item_HeartDrop.prefab"),
                Crawlbug = LoadPrefab($"{PrefabEnemies}/Enemy_Crawlbug.prefab"),
                Dartling = LoadPrefab($"{PrefabEnemies}/Enemy_Dartling.prefab"),
                Hopmite = LoadPrefab($"{PrefabEnemies}/Enemy_Hopmite.prefab"),
                Skimmer = LoadPrefab($"{PrefabEnemies}/Enemy_Skimmer.prefab"),
                Spikewatch = LoadPrefab($"{PrefabEnemies}/Enemy_Spikewatch.prefab"),
                Spitter = LoadPrefab($"{PrefabEnemies}/Enemy_Spitter.prefab")
            };

            if (kit.Solid == null || kit.Exit == null || kit.Checkpoint == null || kit.Coin == null
                || kit.Crawlbug == null || kit.DeathZone == null)
            {
                return null;
            }

            return kit;
        }

        private static GameObject LoadPrefab(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing prefab: {path}");
            }

            return prefab;
        }

        private static void BuildLevel(LevelSpec spec, PrefabKit kit)
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(spec.ScenePath))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing scene: {spec.ScenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(spec.ScenePath, OpenSceneMode.Single);
            var levelRoot = Object.FindAnyObjectByType<LevelRoot>();
            if (levelRoot == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] No LevelRoot in {spec.ScenePath}");
                return;
            }

            WireIdentity(levelRoot, spec);
            var roots = ResolveContentRoots(levelRoot);
            ClearAuthoredContent(levelRoot, roots);
            ClearTilemap(roots.Tilemaps);
            SanitizeTilemapPhysics(roots.Tilemaps);

            switch (spec.BuildIndex)
            {
                case 0:
                    BuildLumenMeadows(levelRoot, roots, kit);
                    break;
                case 1:
                    BuildCascadeCliffs(levelRoot, roots, kit);
                    break;
                default:
                    BuildSkybridgeSpire(levelRoot, roots, kit);
                    break;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, spec.ScenePath);
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Authored {spec.DisplayName}.");
        }

        private sealed class ContentRoots
        {
            public Transform Platforms;
            public Transform Enemies;
            public Transform Collectibles;
            public Transform Hazards;
            public Transform Checkpoints;
            public Transform Decorations;
            public Transform Tilemaps;
            public Transform Spawn;
            public Transform End;
            public Transform Bounds;
        }

        private static ContentRoots ResolveContentRoots(LevelRoot levelRoot)
        {
            var t = levelRoot.transform;
            return new ContentRoots
            {
                Platforms = EnsureChild(t, "Platforms"),
                Enemies = EnsureChild(t, "Enemies"),
                Collectibles = EnsureChild(t, "Collectibles"),
                Hazards = EnsureChild(t, "Hazards"),
                Checkpoints = EnsureChild(t, "Checkpoints"),
                Decorations = EnsureChild(t, "Decorations"),
                Tilemaps = t.Find("Tilemaps"),
                Spawn = t.Find("PlayerSpawn"),
                End = t.Find("LevelEnd"),
                Bounds = t.Find("LevelBounds")
            };
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

        private static void WireIdentity(LevelRoot levelRoot, LevelSpec spec)
        {
            var so = new SerializedObject(levelRoot);
            so.FindProperty("levelId").stringValue = spec.LevelId;
            so.FindProperty("displayName").stringValue = spec.DisplayName;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(levelRoot);
        }

        private static void ClearAuthoredContent(LevelRoot levelRoot, ContentRoots roots)
        {
            ClearChildren(roots.Platforms);
            ClearChildren(roots.Enemies);
            ClearChildren(roots.Collectibles);
            ClearChildren(roots.Hazards);
            ClearChildren(roots.Checkpoints);
            ClearChildren(roots.Decorations);

            // Remove leftover sample exit / markers hanging under LevelRoot.
            for (var i = levelRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = levelRoot.transform.GetChild(i);
                var n = child.name;
                if (n.StartsWith("Sample_")
                    || n.StartsWith("Marker_")
                    || n == "Exit_Goal"
                    || child.GetComponent<LevelContentMarker>() != null
                    || child.GetComponent<LevelExitDoor>() != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void ClearChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (var i = root.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(root.GetChild(i).gameObject);
            }
        }

        private static void ClearTilemap(Transform tilemaps)
        {
            if (tilemaps == null)
            {
                return;
            }

            var maps = tilemaps.GetComponentsInChildren<Tilemap>(true);
            for (var i = 0; i < maps.Length; i++)
            {
                maps[i].ClearAllTiles();
                EditorUtility.SetDirty(maps[i]);
            }
        }

        /// <summary>
        /// Tilemaps are visual-only — prefab platforms own all gameplay collision.
        /// </summary>
        private static void SanitizeTilemapPhysics(Transform tilemaps)
        {
            if (tilemaps == null)
            {
                return;
            }

            var tilemapColliders = tilemaps.GetComponentsInChildren<TilemapCollider2D>(true);
            for (var i = 0; i < tilemapColliders.Length; i++)
            {
                Object.DestroyImmediate(tilemapColliders[i]);
            }

            var compositeColliders = tilemaps.GetComponentsInChildren<CompositeCollider2D>(true);
            for (var i = 0; i < compositeColliders.Length; i++)
            {
                Object.DestroyImmediate(compositeColliders[i]);
            }

            var rigidbodies = tilemaps.GetComponentsInChildren<Rigidbody2D>(true);
            for (var i = 0; i < rigidbodies.Length; i++)
            {
                Object.DestroyImmediate(rigidbodies[i]);
            }

            var maps = tilemaps.GetComponentsInChildren<Tilemap>(true);
            for (var i = 0; i < maps.Length; i++)
            {
                maps[i].gameObject.layer = 0;
                maps[i].gameObject.tag = "Untagged";
                EditorUtility.SetDirty(maps[i].gameObject);
            }
        }

        private static void BuildLumenMeadows(LevelRoot levelRoot, ContentRoots roots, PrefabKit kit)
        {
            // Tutorial — flat meadow path, one taught gap, spikes only at the exit ramp.
            SetBounds(roots.Bounds, new Vector2(48f, 18f), new Vector2(19f, 1f));
            SetTransform(roots.Spawn, new Vector3(0.5f, -1.4f, 0f));

            const float groundH = 0.7f;
            const float groundY = -2.2f;
            var groundTop = PlatTop(groundY, groundH);

            // Continuous path with a single forgiving gap (~3.4u).
            PlaceSolid(roots.Platforms, kit.Solid, "Ground_Start", new Vector3(3.5f, groundY, 0f), new Vector2(10f, groundH));
            PlaceSolid(roots.Platforms, kit.Solid, "Ground_Mid", new Vector3(17f, groundY, 0f), new Vector2(10f, groundH));
            PlaceSolid(roots.Platforms, kit.Solid, "Ground_Rise", new Vector3(28f, -1.5f, 0f), new Vector2(8f, groundH));
            PlaceSolid(roots.Platforms, kit.Solid, "Ground_Exit", new Vector3(37.5f, -0.7f, 0f), new Vector2(7f, groundH));

            var gapLeft = PlatRight(3.5f, 10f);
            var gapRight = PlatLeft(17f, 10f);
            PlacePitBetween(roots.Hazards, kit.DeathZone, "Hazard_Pit_Gap", gapLeft, gapRight);

            var exitTop = PlatTop(-0.7f, groundH);
            PlaceSpikesOnEdge(roots.Hazards, kit.Spikes, "Hazard_Spikes_Exit", PlatRight(37.5f, 7f), exitTop);

            PlaceEnemyOnPlatform(roots.Enemies, kit.Crawlbug, "Enemy_Crawlbug_A", 6.5f, groundTop);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Crawlbug, "Enemy_Crawlbug_B", 20f, groundTop);

            PlaceCoinLine(roots.Collectibles, kit.Coin, 2f, 7.5f, -1.1f, 1.2f);
            Place(roots.Collectibles, kit.Coin, "Coin_Gap_Arc", new Vector3((gapLeft + gapRight) * 0.5f, -0.4f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 14f, 22f, -1.1f, 1.5f);
            Place(roots.Collectibles, kit.Coin, "Coin_High", new Vector3(28f, 0.2f, 0f));
            Place(roots.Collectibles, kit.HeartDrop, "PowerUp_HeartDrop", new Vector3(24f, -0.3f, 0f));

            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_Early", new Vector3(14.5f, EnemyStandY(groundTop), 0f));
            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_Mid", new Vector3(26f, EnemyStandY(PlatTop(-1.5f, groundH)), 0f));

            PlaceDecorCluster(roots.Decorations, "Decor_Start", new Vector3(-1.5f, groundTop, 0f), DecorTheme.Meadow);
            PlaceDecorCluster(roots.Decorations, "Decor_Mid", new Vector3(23f, groundTop, 0f), DecorTheme.Meadow);
            PaintGroundStrip(roots.Tilemaps, -2f, 42f, -5);

            PlaceGoal(levelRoot, roots, kit.Exit, new Vector3(39.5f, exitTop + 1.1f, 0f));
        }

        private static void BuildCascadeCliffs(LevelRoot levelRoot, ContentRoots roots, PrefabKit kit)
        {
            // Medium — stepped cliff shelves; hazards sit on shelf edges and gate the route.
            SetBounds(roots.Bounds, new Vector2(54f, 24f), new Vector2(21f, 3f));
            SetTransform(roots.Spawn, new Vector3(0.8f, -1.5f, 0f));

            const float shelfH = 0.65f;
            var shelfA = new Vector3(2.5f, -2.3f, 0f);
            var shelfAW = 8f;
            var shelfB = new Vector3(12f, -0.6f, 0f);
            var shelfBW = 5f;
            var shelfC = new Vector3(20.5f, 1.1f, 0f);
            var shelfCW = 5f;
            var shelfD = new Vector3(29.5f, 2.6f, 0f);
            var shelfDW = 5.5f;
            var shelfE = new Vector3(39f, 4.0f, 0f);
            var shelfEW = 5f;
            var shelfExit = new Vector3(48f, 5.2f, 0f);
            var shelfExitW = 6f;

            PlaceSolid(roots.Platforms, kit.Solid, "Shelf_Start", shelfA, new Vector2(shelfAW, shelfH));
            PlaceSolid(roots.Platforms, kit.Solid, "Shelf_B", shelfB, new Vector2(shelfBW, shelfH));
            PlaceSolid(roots.Platforms, kit.Solid, "Shelf_C", shelfC, new Vector2(shelfCW, shelfH));
            PlaceSolid(roots.Platforms, kit.Solid, "Shelf_D", shelfD, new Vector2(shelfDW, shelfH));
            PlaceSolid(roots.Platforms, kit.Solid, "Shelf_E", shelfE, new Vector2(shelfEW, shelfH));
            PlaceSolid(roots.Platforms, kit.Solid, "Shelf_Exit", shelfExit, new Vector2(shelfExitW, shelfH));

            Place(roots.Platforms, kit.Bounce, "Bounce_Assist", new Vector3(7.5f, PlatTop(shelfA.y, shelfH) - 0.25f, 0f));
            PlaceTiledPlatform(roots.Platforms, kit.OneWay, "OneWay_High", new Vector3(22f, 3.8f, 0f), new Vector2(3.2f, 0.4f));

            var topA = PlatTop(shelfA.y, shelfH);
            var topB = PlatTop(shelfB.y, shelfH);
            var topC = PlatTop(shelfC.y, shelfH);
            var topD = PlatTop(shelfD.y, shelfH);
            var topE = PlatTop(shelfE.y, shelfH);
            var topExit = PlatTop(shelfExit.y, shelfH);

            PlacePitBetween(roots.Hazards, kit.DeathZone, "Hazard_Pit_A", PlatRight(shelfA.x, shelfAW), PlatLeft(shelfB.x, shelfBW));
            PlacePitBetween(roots.Hazards, kit.DeathZone, "Hazard_Pit_B", PlatRight(shelfC.x, shelfCW), PlatLeft(shelfD.x, shelfDW));
            PlacePitBetween(roots.Hazards, kit.DeathZone, "Hazard_Pit_C", PlatRight(shelfE.x, shelfEW), PlatLeft(shelfExit.x, shelfExitW));

            PlaceSpikesOnEdge(roots.Hazards, kit.Spikes, "Hazard_Spikes_B", PlatRight(shelfB.x, shelfBW), topB);
            PlaceFireOnPlatform(roots.Hazards, kit.Fire, "Hazard_Fire_Gate", shelfD.x, topD);

            PlaceEnemyOnPlatform(roots.Enemies, kit.Crawlbug, "Enemy_Crawlbug_A", shelfA.x - 1f, topA);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Hopmite, "Enemy_Hopmite_A", shelfC.x, topC);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Spikewatch, "Enemy_Spikewatch_A", shelfE.x + 0.5f, topE);

            PlaceCoinLine(roots.Collectibles, kit.Coin, 3f, 6f, topA + 0.5f, 1f);
            PlaceCoinLine(roots.Collectibles, kit.Coin, 11f, 14.5f, topB + 0.6f, 1.2f);
            Place(roots.Collectibles, kit.SpeedBurst, "PowerUp_SpeedBurst", new Vector3(15f, topB + 0.8f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 19f, 23f, topC + 0.5f, 1.3f);
            Place(roots.Collectibles, kit.Coin, "Coin_OneWay", new Vector3(22f, 4.6f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 28f, 33f, topD + 0.5f, 1.4f);
            Place(roots.Collectibles, kit.HeartDrop, "PowerUp_HeartDrop", new Vector3(32f, topD + 0.6f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 38f, 47f, topExit + 0.5f, 1.5f);

            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_A", new Vector3(18f, EnemyStandY(topC), 0f));
            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_B", new Vector3(33f, EnemyStandY(topD), 0f));

            PlaceDecorCluster(roots.Decorations, "Decor_Cliff_A", new Vector3(5f, topA, 0f), DecorTheme.Cliff);
            PlaceDecorCluster(roots.Decorations, "Decor_Cliff_B", new Vector3(26f, topD, 0f), DecorTheme.Cliff);
            PaintGroundStrip(roots.Tilemaps, -1f, 52f, -6);

            PlaceGoal(levelRoot, roots, kit.Exit, new Vector3(49.5f, topExit + 1.2f, 0f));
        }

        private static void BuildSkybridgeSpire(LevelRoot levelRoot, ContentRoots roots, PrefabKit kit)
        {
            // Hard — narrow sky bridges, movers, stacked threats before the exit.
            SetBounds(roots.Bounds, new Vector2(60f, 26f), new Vector2(25f, 4f));
            SetTransform(roots.Spawn, new Vector3(0.6f, 0.2f, 0f));

            const float bridgeH = 0.55f;
            var tower = new Vector3(2.5f, -0.6f, 0f);
            var towerW = 6f;
            var bridgeA = new Vector3(10f, 0.5f, 0f);
            var bridgeAW = 3.5f;
            var bridgeB = new Vector3(16.5f, 1.6f, 0f);
            var bridgeBW = 3.2f;
            var landing = new Vector3(25f, 2.4f, 0f);
            var landingW = 4.5f;
            var bridgeC = new Vector3(33.5f, 3.5f, 0f);
            var bridgeCW = 3.2f;
            var arena = new Vector3(41f, 4.2f, 0f);
            var arenaW = 5.5f;
            var spireExit = new Vector3(51f, 5.4f, 0f);
            var spireExitW = 6f;

            PlaceSolid(roots.Platforms, kit.Solid, "Tower_Start", tower, new Vector2(towerW, bridgeH));
            PlaceSolid(roots.Platforms, kit.Solid, "Bridge_A", bridgeA, new Vector2(bridgeAW, bridgeH));
            PlaceSolid(roots.Platforms, kit.Solid, "Bridge_B", bridgeB, new Vector2(bridgeBW, bridgeH));
            PlaceSolid(roots.Platforms, kit.Solid, "Bridge_C", bridgeC, new Vector2(bridgeCW, bridgeH));
            PlaceSolid(roots.Platforms, kit.Solid, "Arena_Mid", arena, new Vector2(arenaW, bridgeH));
            PlaceSolid(roots.Platforms, kit.Solid, "Spire_Exit", spireExit, new Vector2(spireExitW, bridgeH));

            PlaceTiledPlatform(roots.Platforms, kit.OneWay, "OneWay_Assist", new Vector3(13.5f, 3.0f, 0f), new Vector2(2.6f, 0.35f));
            Place(roots.Platforms, kit.Bounce, "Bounce_Recover", new Vector3(29f, PlatTop(bridgeB.y, bridgeH) - 0.2f, 0f));

            var moverPathHalf = landingW * 0.45f;
            PlaceMovingPlatform(
                roots.Platforms,
                kit.Moving,
                "Platform_Moving_A",
                landing,
                new Vector2(landingW, bridgeH),
                moverPathHalf,
                2.1f);

            var topTower = PlatTop(tower.y, bridgeH);
            var topA = PlatTop(bridgeA.y, bridgeH);
            var topB = PlatTop(bridgeB.y, bridgeH);
            var topLanding = PlatTop(landing.y, bridgeH);
            var topC = PlatTop(bridgeC.y, bridgeH);
            var topArena = PlatTop(arena.y, bridgeH);
            var topExit = PlatTop(spireExit.y, bridgeH);

            PlacePitSpan(roots.Hazards, kit.DeathZone, "Hazard_Pit_Sky", 8f, 54f);
            PlaceFireOnPlatform(roots.Hazards, kit.Fire, "Hazard_Fire_Landing", bridgeC.x, topC);

            var movingSpike = Place(
                roots.Hazards,
                kit.MovingSpike,
                "Hazard_MovingSpike_Finale",
                new Vector3(47.5f, topExit + SpikeHalfHeight + 0.15f, 0f));
            ConfigureMovingHazard(movingSpike, new Vector2(-1.8f, 0f), new Vector2(1.8f, 0f), 2.4f);

            PlaceEnemyOnPlatform(roots.Enemies, kit.Crawlbug, "Enemy_Crawlbug_A", bridgeA.x, topA);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Dartling, "Enemy_Dartling_A", landing.x + 0.5f, topLanding);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Hopmite, "Enemy_Hopmite_A", bridgeC.x, topC);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Skimmer, "Enemy_Skimmer_A", arena.x - 1f, topArena + 1.2f);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Spitter, "Enemy_Spitter_A", arena.x + 1.5f, topArena);

            PlaceCoinLine(roots.Collectibles, kit.Coin, 2f, 5f, topTower + 0.5f, 1f);
            PlaceCoinLine(roots.Collectibles, kit.Coin, 9f, 12f, topA + 0.6f, 1.1f);
            Place(roots.Collectibles, kit.SpeedBurst, "PowerUp_SpeedBurst", new Vector3(16.5f, topB + 0.7f, 0f));
            Place(roots.Collectibles, kit.Coin, "Coin_Mover", new Vector3(landing.x, topLanding + 1.2f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 31f, 36f, topC + 0.6f, 1.2f);
            Place(roots.Collectibles, kit.HeartDrop, "PowerUp_HeartDrop", new Vector3(26f, topLanding + 0.8f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 40f, 46f, topArena + 0.7f, 1.3f);
            Place(roots.Collectibles, kit.GlowShield, "PowerUp_GlowShield", new Vector3(44f, topArena + 0.8f, 0f));
            Place(roots.Collectibles, kit.Coin, "Coin_Exit", new Vector3(52f, topExit + 0.9f, 0f));

            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_A", new Vector3(15.5f, EnemyStandY(topB), 0f));
            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_B", new Vector3(31f, EnemyStandY(topLanding), 0f));
            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_C", new Vector3(43f, EnemyStandY(topArena), 0f));

            PlaceDecorCluster(roots.Decorations, "Decor_Spire_A", new Vector3(8f, topA, 0f), DecorTheme.Spire);
            PlaceDecorCluster(roots.Decorations, "Decor_Spire_B", new Vector3(38f, topArena, 0f), DecorTheme.Spire);

            PlaceGoal(levelRoot, roots, kit.Exit, new Vector3(52.5f, topExit + 1.3f, 0f));
        }

        private static void PlaceGoal(LevelRoot levelRoot, ContentRoots roots, GameObject exitPrefab, Vector3 position)
        {
            Place(levelRoot.transform, exitPrefab, "Exit_Goal", position);
            if (roots.End != null)
            {
                roots.End.position = position;
                // Exit_Goal is the visible completer; LevelEnd stays as LevelRoot marker only (Phase 31).
                var endPoint = roots.End.GetComponent<LevelEndPoint>();
                if (endPoint != null)
                {
                    var endSo = new SerializedObject(endPoint);
                    endSo.FindProperty("completeLevelOnEnter").boolValue = false;
                    endSo.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(endPoint);
                }

                var endCol = roots.End.GetComponent<Collider2D>();
                if (endCol != null)
                {
                    endCol.enabled = false;
                    EditorUtility.SetDirty(endCol);
                }
            }

            var so = new SerializedObject(levelRoot);
            if (roots.End != null)
            {
                so.FindProperty("endPoint").objectReferenceValue = roots.End.GetComponent<LevelEndPoint>();
            }

            if (roots.Spawn != null)
            {
                so.FindProperty("startPoint").objectReferenceValue = roots.Spawn.GetComponent<PlayerSpawnPoint>();
            }

            if (roots.Bounds != null)
            {
                so.FindProperty("levelBounds").objectReferenceValue = roots.Bounds.GetComponent<LevelBounds>();
            }

            so.FindProperty("platformsRoot").objectReferenceValue = roots.Platforms;
            so.FindProperty("enemiesRoot").objectReferenceValue = roots.Enemies;
            so.FindProperty("collectiblesRoot").objectReferenceValue = roots.Collectibles;
            so.FindProperty("hazardsRoot").objectReferenceValue = roots.Hazards;
            so.FindProperty("checkpointsRoot").objectReferenceValue = roots.Checkpoints;
            so.FindProperty("decorationsRoot").objectReferenceValue = roots.Decorations;
            if (roots.Tilemaps != null)
            {
                so.FindProperty("tilemapRoot").objectReferenceValue = roots.Tilemaps;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(levelRoot);
        }

        private static void PlaceSolid(Transform parent, GameObject prefab, string name, Vector3 position, Vector2 worldSize)
        {
            var go = Place(parent, prefab, name, position);
            ApplyTiledPlatformWorldSize(go, worldSize);
        }

        private static void PlaceTiledPlatform(
            Transform parent,
            GameObject prefab,
            string name,
            Vector3 position,
            Vector2 worldSize)
        {
            var go = Place(parent, prefab, name, position);
            ApplyTiledPlatformWorldSize(go, worldSize);
        }

        private static GameObject PlaceMovingPlatform(
            Transform parent,
            GameObject prefab,
            string name,
            Vector3 position,
            Vector2 worldSize,
            float pathHalfWidth,
            float speed)
        {
            var go = Place(parent, prefab, name, position);
            ApplyTiledPlatformWorldSize(go, worldSize);
            ConfigureMovingPlatform(go, new Vector2(-pathHalfWidth, 0f), new Vector2(pathHalfWidth, 0f), speed);
            return go;
        }

        /// <summary>
        /// Sets collider + tiled sprite to exact world size (never use transform scale on platforms).
        /// </summary>
        private static void ApplyTiledPlatformWorldSize(GameObject go, Vector2 worldSize)
        {
            if (go == null)
            {
                return;
            }

            go.transform.localScale = Vector3.one;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.tileMode = SpriteTileMode.Continuous;
                sr.size = worldSize;
                EditorUtility.SetDirty(sr);
            }

            var col = go.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = worldSize;
                col.offset = Vector2.zero;
                col.autoTiling = false;
                EditorUtility.SetDirty(col);
            }

            EditorUtility.SetDirty(go);
        }

        private static GameObject Place(
            Transform parent,
            GameObject prefab,
            string name,
            Vector3 position,
            Vector3? scale = null)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Skip '{name}' — prefab missing.");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            if (scale.HasValue)
            {
                instance.transform.localScale = scale.Value;
            }

            Undo.RegisterCreatedObjectUndo(instance, "Phase 24 Place " + name);
            return instance;
        }

        private static void ConfigureMovingPlatform(GameObject go, Vector2 localA, Vector2 localB, float speed)
        {
            if (go == null)
            {
                return;
            }

            var moving = go.GetComponent<MovingPlatform>();
            if (moving == null)
            {
                return;
            }

            var so = new SerializedObject(moving);
            so.FindProperty("pointA").vector2Value = localA;
            so.FindProperty("pointB").vector2Value = localB;
            so.FindProperty("pointsAreLocal").boolValue = true;
            so.FindProperty("speed").floatValue = speed;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(moving);
        }

        private static void ConfigureMovingHazard(GameObject go, Vector2 localA, Vector2 localB, float speed)
        {
            if (go == null)
            {
                return;
            }

            var moving = go.GetComponent<MovingHazard>();
            if (moving == null)
            {
                return;
            }

            var so = new SerializedObject(moving);
            so.FindProperty("pointA").vector2Value = localA;
            so.FindProperty("pointB").vector2Value = localB;
            so.FindProperty("pointsAreLocal").boolValue = true;
            so.FindProperty("speed").floatValue = speed;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(moving);
        }

        private static void SetBounds(Transform boundsTransform, Vector2 size, Vector2 centerOffset)
        {
            if (boundsTransform == null)
            {
                return;
            }

            boundsTransform.position = Vector3.zero;
            var bounds = boundsTransform.GetComponent<LevelBounds>();
            if (bounds == null)
            {
                return;
            }

            var so = new SerializedObject(bounds);
            so.FindProperty("size").vector2Value = size;
            so.FindProperty("centerOffset").vector2Value = centerOffset;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bounds);
        }

        private static void SetTransform(Transform t, Vector3 position)
        {
            if (t == null)
            {
                return;
            }

            t.position = position;
            EditorUtility.SetDirty(t.gameObject);
        }

        private enum DecorTheme
        {
            Meadow,
            Cliff,
            Spire
        }

        private static float PlatLeft(float centerX, float width) => centerX - width * 0.5f;
        private static float PlatRight(float centerX, float width) => centerX + width * 0.5f;
        private static float PlatTop(float centerY, float height) => centerY + height * 0.5f;
        private static float EnemyStandY(float platformTop) => platformTop + EnemyStandOffset;

        private static void PlacePitBetween(
            Transform parent,
            GameObject prefab,
            string name,
            float gapLeft,
            float gapRight)
        {
            if (gapRight <= gapLeft)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Skip pit '{name}' — invalid gap.");
                return;
            }

            var width = gapRight - gapLeft + 0.4f;
            var centerX = (gapLeft + gapRight) * 0.5f;
            Place(parent, prefab, name, new Vector3(centerX, PitDepthY, 0f), new Vector3(width, PitHeight, 1f));
        }

        private static void PlacePitSpan(Transform parent, GameObject prefab, string name, float leftX, float rightX)
        {
            PlacePitBetween(parent, prefab, name, leftX, rightX);
        }

        private static void PlaceSpikesOnEdge(
            Transform parent,
            GameObject prefab,
            string name,
            float platformRightEdge,
            float platformTop)
        {
            var x = platformRightEdge - 0.55f;
            var y = platformTop + SpikeHalfHeight;
            Place(parent, prefab, name, new Vector3(x, y, 0f));
        }

        private static void PlaceFireOnPlatform(
            Transform parent,
            GameObject prefab,
            string name,
            float platformCenterX,
            float platformTop)
        {
            var y = platformTop + FireHalfHeight;
            Place(parent, prefab, name, new Vector3(platformCenterX, y, 0f));
        }

        private static void PlaceEnemyOnPlatform(
            Transform parent,
            GameObject prefab,
            string name,
            float platformCenterX,
            float platformTop)
        {
            Place(parent, prefab, name, new Vector3(platformCenterX, EnemyStandY(platformTop), 0f));
        }

        private static void PlaceCoinLine(
            Transform parent,
            GameObject prefab,
            float startX,
            float endX,
            float y,
            float spacing)
        {
            if (prefab == null || spacing <= 0f)
            {
                return;
            }

            var index = 1;
            for (var x = startX; x <= endX; x += spacing)
            {
                Place(parent, prefab, $"Coin_Line_{startX:F0}_{index}", new Vector3(x, y, 0f));
                index++;
            }
        }

        private static void PlaceDecorCluster(Transform parent, string rootName, Vector3 anchor, DecorTheme theme)
        {
            if (parent == null)
            {
                return;
            }

            var root = new GameObject(rootName);
            root.transform.SetParent(parent, true);
            root.transform.position = anchor;

            var bushColor = theme == DecorTheme.Meadow
                ? new Color(0.28f, 0.62f, 0.32f, 1f)
                : theme == DecorTheme.Cliff
                    ? new Color(0.45f, 0.38f, 0.28f, 1f)
                    : new Color(0.35f, 0.32f, 0.55f, 1f);
            var rockColor = theme == DecorTheme.Meadow
                ? new Color(0.5f, 0.45f, 0.38f, 1f)
                : new Color(0.42f, 0.4f, 0.46f, 1f);

            CreateDecorSprite(root.transform, "Bush_A", new Vector3(-1.2f, 0.4f, 0f), new Vector3(0.9f, 1.1f, 1f), bushColor, -8);
            CreateDecorSprite(root.transform, "Bush_B", new Vector3(1.4f, 0.35f, 0f), new Vector3(0.7f, 0.9f, 1f), bushColor * 0.9f, -8);
            CreateDecorSprite(root.transform, "Rock_A", new Vector3(-0.3f, 0.15f, 0f), new Vector3(0.6f, 0.45f, 1f), rockColor, -7);
            EditorUtility.SetDirty(root);
        }

        private static void CreateDecorSprite(
            Transform parent,
            string name,
            Vector3 localPos,
            Vector3 scale,
            Color color,
            int sortingOrder)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(GroundSpritePath);
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
        }

        private static void PaintGroundStrip(Transform tilemaps, float startX, float endX, int tileY)
        {
            if (tilemaps == null)
            {
                return;
            }

            var tile = AssetDatabase.LoadAssetAtPath<Tile>(GroundTilePath);
            if (tile == null)
            {
                return;
            }

            var gridTransform = tilemaps.Find("Grid");
            if (gridTransform == null)
            {
                var gridGo = new GameObject("Grid");
                gridGo.transform.SetParent(tilemaps, false);
                gridGo.AddComponent<Grid>().cellSize = new Vector3(1f, 1f, 0f);
                gridTransform = gridGo.transform;
            }

            var mapTransform = gridTransform.Find("Tilemap_Decor");
            Tilemap map;
            if (mapTransform == null)
            {
                var mapGo = new GameObject("Tilemap_Decor");
                mapGo.transform.SetParent(gridTransform, false);
                mapGo.layer = 0;
                mapGo.tag = "Untagged";
                map = mapGo.AddComponent<Tilemap>();
                var renderer = mapGo.AddComponent<TilemapRenderer>();
                renderer.sortingOrder = -4;
            }
            else
            {
                map = mapTransform.GetComponent<Tilemap>();
            }

            if (map == null)
            {
                return;
            }

            var minX = Mathf.FloorToInt(startX);
            var maxX = Mathf.CeilToInt(endX);
            for (var x = minX; x <= maxX; x++)
            {
                map.SetTile(new Vector3Int(x, tileY, 0), tile);
            }

            EditorUtility.SetDirty(map);
        }

        private static LevelData UpdateLevelData(LevelSpec spec)
        {
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(spec.DataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(data, spec.DataPath);
            }

            var so = new SerializedObject(data);
            so.FindProperty("levelId").stringValue = spec.LevelId;
            so.FindProperty("displayName").stringValue = spec.DisplayName;
            so.FindProperty("sceneName").stringValue = spec.SceneName;
            so.FindProperty("buildIndex").intValue = spec.BuildIndex;
            so.FindProperty("designerNotes").stringValue = spec.DesignerNotes;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static void UpdateCatalog(LevelData[] levelAssets)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var so = new SerializedObject(catalog);
            var levels = so.FindProperty("levels");
            levels.arraySize = levelAssets.Length;
            for (var i = 0; i < levelAssets.Length; i++)
            {
                levels.GetArrayElementAtIndex(i).objectReferenceValue = levelAssets[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] LevelCatalog wired ({levelAssets.Length} levels).");
        }
    }
}
#endif
