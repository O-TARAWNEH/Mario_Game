// Filename: Phase40PuzzleLevelsSetup.cs
// Folder: Assets/Editor/
// Purpose: Adds puzzle props + Level 04/05 campaign content (Phase 40).
// Menu: Bounder Trail/Phase 40/Setup Puzzle Levels
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase40PuzzleLevelsSetup.SetupPuzzleLevels

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.Data;
using BounderTrail.Levels;
using BounderTrail.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase40PuzzleLevelsSetup
    {
        private const string DataFolder = "Assets/Data/Levels";
        private const string CatalogPath = DataFolder + "/LevelCatalog.asset";
        private const string PrefabWorld = "Assets/Prefabs/World";
        private const string PrefabEnemies = "Assets/Prefabs/Enemies";
        private const string PrefabItems = "Assets/Prefabs/Items";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";

        private const float SpikeHalfHeight = 0.275f;
        private const float FireHalfHeight = 0.45f;
        private const float EnemyStandOffset = 0.35f;
        private const float PitDepthY = -4.6f;
        private const float PitHeight = 1.2f;

        private static readonly LevelSpec[] PuzzleCampaign =
        {
            new LevelSpec(
                "level_04",
                "Echo Caverns",
                ProjectConstants.Level04SceneName,
                $"Assets/Scenes/{ProjectConstants.Level04SceneName}.unity",
                $"{DataFolder}/LevelData_04_EchoCaverns.asset",
                3,
                "Puzzle timing cavern — blink platforms, bounce sync, mover gap, Speed Burst fire dash."),
            new LevelSpec(
                "level_05",
                "Lantern Lockworks",
                ProjectConstants.Level05SceneName,
                $"Assets/Scenes/{ProjectConstants.Level05SceneName}.unity",
                $"{DataFolder}/LevelData_05_LanternLockworks.asset",
                4,
                "Switch lockworks — pressure gates, timed latch, Glow Shield fire hall, moving spike finale.")
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
            public GameObject Hopmite;
            public GameObject Spitter;
            public GameObject Spikewatch;
            public GameObject Timed;
            public GameObject Switch;
            public GameObject Gate;
        }

        private sealed class ContentRoots
        {
            public Transform Platforms;
            public Transform Enemies;
            public Transform Collectibles;
            public Transform Hazards;
            public Transform Checkpoints;
            public Transform Decorations;
            public Transform Spawn;
            public Transform End;
            public Transform Bounds;
        }

        [MenuItem("Bounder Trail/Phase 40/Setup Puzzle Levels")]
        public static void SetupPuzzleLevels()
        {
            EnsurePuzzlePrefabs();
            var kit = LoadPrefabs();
            if (kit == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 40 aborted — missing prefabs.");
                return;
            }

            var newAssets = new LevelData[PuzzleCampaign.Length];
            for (var i = 0; i < PuzzleCampaign.Length; i++)
            {
                EnsureCampaignScene(PuzzleCampaign[i]);
                BuildLevel(PuzzleCampaign[i], kit);
                newAssets[i] = CreateOrUpdateLevelData(PuzzleCampaign[i]);
            }

            MergeCatalog(newAssets);
            ConfigureBuildSettings();
            WireBootstrapCatalog();
            EnlargeCoinTriggers();

            Phase6CameraSetup.SetupCameraSystem();
            Phase36HeartsHudSetup.SetupHeartsHud();
            Phase37OverlayUiScaleSetup.SetupOverlayUiScale();

            // Camera polish on new scenes (Pixel Perfect off).
            for (var i = 0; i < PuzzleCampaign.Length; i++)
            {
                DisablePixelPerfect(PuzzleCampaign[i].ScenePath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"{GameLog.ProjectPrefix}[Setup] Phase 40 ready — Echo Caverns + Lantern Lockworks " +
                "with timed platforms, switches, and gates.");
        }

        private static void EnsurePuzzlePrefabs()
        {
            var solid = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabWorld}/Platform_Solid.prefab");
            if (solid == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing Platform_Solid prefab.");
                return;
            }

            EnsureVariantPrefab($"{PrefabWorld}/Platform_Timed.prefab", solid, go =>
            {
                if (go.GetComponent<TimedPlatform>() == null)
                {
                    go.AddComponent<TimedPlatform>();
                }

                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(0.45f, 0.75f, 1f, 1f);
                }
            });

            EnsureVariantPrefab($"{PrefabWorld}/PressureSwitch.prefab", solid, go =>
            {
                if (go.GetComponent<PressureSwitch>() == null)
                {
                    go.AddComponent<PressureSwitch>();
                }

                ApplyWorldSize(go, new Vector2(1.4f, 0.35f));
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(0.85f, 0.55f, 0.2f, 1f);
                }
            });

            EnsureVariantPrefab($"{PrefabWorld}/GateBarrier.prefab", solid, go =>
            {
                if (go.GetComponent<GateBarrier>() == null)
                {
                    go.AddComponent<GateBarrier>();
                }

                ApplyWorldSize(go, new Vector2(0.7f, 2.4f));
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(0.75f, 0.35f, 0.85f, 1f);
                }
            });
        }

        private static void EnsureVariantPrefab(string path, GameObject source, System.Action<GameObject> configure)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    configure(root);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }

                return;
            }

            var instance = Object.Instantiate(source);
            instance.name = System.IO.Path.GetFileNameWithoutExtension(path);
            configure(instance);
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }

        private static PrefabKit LoadPrefabs()
        {
            var kit = new PrefabKit
            {
                Solid = Load($"{PrefabWorld}/Platform_Solid.prefab"),
                OneWay = Load($"{PrefabWorld}/Platform_OneWay.prefab"),
                Moving = Load($"{PrefabWorld}/Platform_Moving.prefab"),
                Bounce = Load($"{PrefabWorld}/BouncePad.prefab"),
                Exit = Load($"{PrefabWorld}/LevelExitDoor.prefab"),
                Checkpoint = Load($"{PrefabWorld}/Checkpoint_Flag.prefab"),
                DeathZone = Load($"{PrefabWorld}/Hazard_DeathZone.prefab"),
                Spikes = Load($"{PrefabWorld}/Hazard_Spikes.prefab"),
                Fire = Load($"{PrefabWorld}/Hazard_Fire.prefab"),
                MovingSpike = Load($"{PrefabWorld}/Hazard_MovingSpike.prefab"),
                Coin = Load($"{PrefabItems}/Item_Coin.prefab"),
                SpeedBurst = Load($"{PrefabItems}/Item_SpeedBurst.prefab"),
                GlowShield = Load($"{PrefabItems}/Item_GlowShield.prefab"),
                HeartDrop = Load($"{PrefabItems}/Item_HeartDrop.prefab"),
                Crawlbug = Load($"{PrefabEnemies}/Enemy_Crawlbug.prefab"),
                Hopmite = Load($"{PrefabEnemies}/Enemy_Hopmite.prefab"),
                Spitter = Load($"{PrefabEnemies}/Enemy_Spitter.prefab"),
                Spikewatch = Load($"{PrefabEnemies}/Enemy_Spikewatch.prefab"),
                Timed = Load($"{PrefabWorld}/Platform_Timed.prefab"),
                Switch = Load($"{PrefabWorld}/PressureSwitch.prefab"),
                Gate = Load($"{PrefabWorld}/GateBarrier.prefab")
            };

            return kit.Solid != null && kit.Exit != null && kit.Timed != null && kit.Switch != null && kit.Gate != null
                ? kit
                : null;
        }

        private static GameObject Load(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing prefab: {path}");
            }

            return prefab;
        }

        private static void EnsureCampaignScene(LevelSpec spec)
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(spec.ScenePath))
            {
                if (!AssetDatabase.CopyAsset(GameplayScenePath, spec.ScenePath))
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Failed to copy Gameplay → {spec.ScenePath}");
                    return;
                }

                AssetDatabase.ImportAsset(spec.ScenePath);
            }

            var scene = EditorSceneManager.OpenScene(spec.ScenePath, OpenSceneMode.Single);
            var levelRoot = Object.FindAnyObjectByType<LevelRoot>();
            if (levelRoot != null)
            {
                var so = new SerializedObject(levelRoot);
                so.FindProperty("levelId").stringValue = spec.LevelId;
                so.FindProperty("displayName").stringValue = spec.DisplayName;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(levelRoot);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, spec.ScenePath);
        }

        private static void BuildLevel(LevelSpec spec, PrefabKit kit)
        {
            var scene = EditorSceneManager.OpenScene(spec.ScenePath, OpenSceneMode.Single);
            var levelRoot = Object.FindAnyObjectByType<LevelRoot>();
            if (levelRoot == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] No LevelRoot in {spec.ScenePath}");
                return;
            }

            var so = new SerializedObject(levelRoot);
            so.FindProperty("levelId").stringValue = spec.LevelId;
            so.FindProperty("displayName").stringValue = spec.DisplayName;
            so.ApplyModifiedPropertiesWithoutUndo();

            var roots = ResolveRoots(levelRoot);
            ClearChildren(roots.Platforms);
            ClearChildren(roots.Enemies);
            ClearChildren(roots.Collectibles);
            ClearChildren(roots.Hazards);
            ClearChildren(roots.Checkpoints);
            ClearChildren(roots.Decorations);

            for (var i = levelRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = levelRoot.transform.GetChild(i);
                if (child.name == "Exit_Goal" || child.GetComponent<LevelExitDoor>() != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            if (spec.BuildIndex == 3)
            {
                BuildEchoCaverns(levelRoot, roots, kit);
            }
            else
            {
                BuildLanternLockworks(levelRoot, roots, kit);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, spec.ScenePath);
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Authored {spec.DisplayName}.");
        }

        private static void BuildEchoCaverns(LevelRoot levelRoot, ContentRoots roots, PrefabKit kit)
        {
            // Timing puzzle: blink pads over a pit, then bounce / mover / speed-fire finale.
            const float h = 0.55f;
            SetBounds(roots.Bounds, new Vector2(58f, 24f), new Vector2(24f, 3f));
            SetTransform(roots.Spawn, new Vector3(0.8f, 0.35f, 0f));

            var start = new Vector3(3f, -0.4f, 0f);
            var ledge = new Vector3(12f, 0.8f, 0f);
            var mid = new Vector3(22f, 1.6f, 0f);
            var rise = new Vector3(31f, 2.6f, 0f);
            var exitPad = new Vector3(42f, 3.4f, 0f);

            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Start", start, new Vector2(7f, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Ledge", ledge, new Vector2(4.5f, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Mid", mid, new Vector2(5f, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Rise", rise, new Vector2(5f, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Exit", exitPad, new Vector2(7f, h));

            var topStart = PlatTop(start.y, h);
            var topLedge = PlatTop(ledge.y, h);
            var topMid = PlatTop(mid.y, h);
            var topRise = PlatTop(rise.y, h);
            var topExit = PlatTop(exitPad.y, h);

            // Blink stepping stones between start and ledge.
            PlaceTimed(roots.Platforms, kit.Timed, "Blink_A", new Vector3(7.6f, 0.1f, 0f), new Vector2(1.6f, 0.45f), 1.25f, 1.0f, 0f, true);
            PlaceTimed(roots.Platforms, kit.Timed, "Blink_B", new Vector3(9.4f, 0.35f, 0f), new Vector2(1.6f, 0.45f), 1.25f, 1.0f, 0.6f, false);
            PlaceTimed(roots.Platforms, kit.Timed, "Blink_C", new Vector3(11.0f, 0.55f, 0f), new Vector2(1.5f, 0.45f), 1.25f, 1.0f, 1.2f, true);

            Place(roots.Platforms, kit.Bounce, "Bounce_Up", new Vector3(14.2f, topLedge - 0.15f, 0f));
            PlaceTiled(roots.Platforms, kit.OneWay, "OneWay_Return", new Vector3(16.5f, 2.8f, 0f), new Vector2(2.4f, 0.35f));

            PlaceMoving(
                roots.Platforms,
                kit.Moving,
                "Mover_Gap",
                new Vector3(26.5f, 2.0f, 0f),
                new Vector2(3.2f, h),
                new Vector2(-1.6f, 0f),
                new Vector2(1.6f, 0f),
                2.0f);

            PlaceTimed(roots.Platforms, kit.Timed, "Blink_Finale_A", new Vector3(35.2f, 3.0f, 0f), new Vector2(1.7f, 0.45f), 1.1f, 0.9f, 0f, true);
            PlaceTimed(roots.Platforms, kit.Timed, "Blink_Finale_B", new Vector3(37.2f, 3.15f, 0f), new Vector2(1.7f, 0.45f), 1.1f, 0.9f, 0.55f, false);

            PlacePitSpan(roots.Hazards, kit.DeathZone, "Hazard_Pit_Cave", 6.5f, 40.5f);
            PlaceFireOnPlatform(roots.Hazards, kit.Fire, "Hazard_Fire_Dash", 39.2f, topExit);
            Place(roots.Collectibles, kit.SpeedBurst, "PowerUp_SpeedBurst", new Vector3(33.2f, topRise + 0.7f, 0f));

            PlaceEnemyOnPlatform(roots.Enemies, kit.Crawlbug, "Enemy_Crawlbug_A", mid.x, topMid);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Hopmite, "Enemy_Hopmite_A", rise.x - 0.8f, topRise);

            PlaceCoinLine(roots.Collectibles, kit.Coin, 1.5f, 5f, topStart + 0.55f, 1.1f);
            PlaceCoinLine(roots.Collectibles, kit.Coin, 12f, 14.5f, topLedge + 0.6f, 1.1f);
            PlaceCoinLine(roots.Collectibles, kit.Coin, 21f, 24.5f, topMid + 0.6f, 1.2f);
            Place(roots.Collectibles, kit.HeartDrop, "PowerUp_HeartDrop", new Vector3(28.5f, 3.1f, 0f));
            Place(roots.Collectibles, kit.Coin, "Coin_Secret_BlinkRoof", new Vector3(23f, topMid + 2.1f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 40f, 44f, topExit + 0.65f, 1.2f);

            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_A", new Vector3(12f, EnemyStandY(topLedge), 0f));
            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_B", new Vector3(31f, EnemyStandY(topRise), 0f));
            PlaceGoal(levelRoot, roots, kit.Exit, new Vector3(44.5f, topExit + 1.25f, 0f));
        }

        private static void BuildLanternLockworks(LevelRoot levelRoot, ContentRoots roots, PrefabKit kit)
        {
            // Switch puzzles: hold gate, timed latch, then shield fire hall.
            const float h = 0.55f;
            SetBounds(roots.Bounds, new Vector2(62f, 26f), new Vector2(26f, 4f));
            SetTransform(roots.Spawn, new Vector3(0.7f, 0.3f, 0f));

            var start = new Vector3(3f, -0.35f, 0f);
            var roomA = new Vector3(12f, 0.6f, 0f);
            var roomB = new Vector3(22f, 1.5f, 0f);
            var hall = new Vector3(33f, 2.4f, 0f);
            var exitPad = new Vector3(46f, 3.5f, 0f);

            PlaceSolid(roots.Platforms, kit.Solid, "Lock_Start", start, new Vector2(7f, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_RoomA", roomA, new Vector2(6f, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_RoomB", roomB, new Vector2(6.5f, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_Hall", hall, new Vector2(8f, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_Exit", exitPad, new Vector2(8f, h));

            var topStart = PlatTop(start.y, h);
            var topA = PlatTop(roomA.y, h);
            var topB = PlatTop(roomB.y, h);
            var topHall = PlatTop(hall.y, h);
            var topExit = PlatTop(exitPad.y, h);

            // Puzzle 1: hold switch opens gate to Room A.
            var gate1 = PlaceGate(roots.Platforms, kit.Gate, "Gate_Hold", new Vector3(8.4f, topStart + 1.0f, 0f), new Vector2(0.7f, 2.2f));
            PlaceSwitch(
                roots.Platforms,
                kit.Switch,
                "Switch_Hold",
                new Vector3(5.2f, topStart + 0.05f, 0f),
                PressureSwitch.Mode.HoldWhileStanding,
                0f,
                gate1);

            // Puzzle 2: timed latch switch + blink path into Room B.
            PlaceTimed(roots.Platforms, kit.Timed, "Blink_Lock_A", new Vector3(16.4f, 1.0f, 0f), new Vector2(1.6f, 0.45f), 1.35f, 1.05f, 0f, true);
            PlaceTimed(roots.Platforms, kit.Timed, "Blink_Lock_B", new Vector3(18.2f, 1.2f, 0f), new Vector2(1.6f, 0.45f), 1.35f, 1.05f, 0.65f, false);
            var gate2 = PlaceGate(roots.Platforms, kit.Gate, "Gate_Timed", new Vector3(19.8f, topA + 1.05f, 0f), new Vector2(0.7f, 2.3f));
            PlaceSwitch(
                roots.Platforms,
                kit.Switch,
                "Switch_Timed",
                new Vector3(13.5f, topA + 0.05f, 0f),
                PressureSwitch.Mode.LatchTimed,
                4.2f,
                gate2);

            // Puzzle 3: permanent latch opens hall gate; fire hall needs Glow Shield.
            var gate3 = PlaceGate(roots.Platforms, kit.Gate, "Gate_Latch", new Vector3(27.6f, topB + 1.1f, 0f), new Vector2(0.75f, 2.5f));
            PlaceSwitch(
                roots.Platforms,
                kit.Switch,
                "Switch_Latch",
                new Vector3(24.2f, topB + 0.05f, 0f),
                PressureSwitch.Mode.LatchPermanent,
                0f,
                gate3);

            Place(roots.Collectibles, kit.GlowShield, "PowerUp_GlowShield", new Vector3(29.5f, topHall + 0.75f, 0f));
            PlaceFireOnPlatform(roots.Hazards, kit.Fire, "Hazard_Fire_Hall_A", 31.5f, topHall);
            PlaceFireOnPlatform(roots.Hazards, kit.Fire, "Hazard_Fire_Hall_B", 34.5f, topHall);

            PlaceMoving(
                roots.Platforms,
                kit.Moving,
                "Mover_Finale",
                new Vector3(40.5f, 3.0f, 0f),
                new Vector2(3.4f, h),
                new Vector2(-1.5f, 0f),
                new Vector2(1.5f, 0f),
                2.2f);

            var spike = Place(
                roots.Hazards,
                kit.MovingSpike,
                "Hazard_MovingSpike_Finale",
                new Vector3(43.5f, topExit + SpikeHalfHeight + 0.15f, 0f));
            ConfigureMovingHazard(spike, new Vector2(-1.6f, 0f), new Vector2(1.6f, 0f), 2.3f);

            PlacePitSpan(roots.Hazards, kit.DeathZone, "Hazard_Pit_Lock", 7f, 44f);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Spikewatch, "Enemy_Spikewatch_A", roomA.x + 1.5f, topA);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Spitter, "Enemy_Spitter_A", roomB.x + 1.2f, topB);
            PlaceEnemyOnPlatform(roots.Enemies, kit.Crawlbug, "Enemy_Crawlbug_A", hall.x - 1.5f, topHall);

            PlaceCoinLine(roots.Collectibles, kit.Coin, 1.5f, 5f, topStart + 0.55f, 1.1f);
            PlaceCoinLine(roots.Collectibles, kit.Coin, 11f, 14.5f, topA + 0.6f, 1.15f);
            PlaceCoinLine(roots.Collectibles, kit.Coin, 21f, 25f, topB + 0.6f, 1.2f);
            Place(roots.Collectibles, kit.HeartDrop, "PowerUp_HeartDrop", new Vector3(36.5f, topHall + 0.8f, 0f));
            Place(roots.Collectibles, kit.Coin, "Coin_Secret_SwitchAlcove", new Vector3(24.5f, topB + 1.9f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 44f, 49f, topExit + 0.65f, 1.2f);

            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_A", new Vector3(12f, EnemyStandY(topA), 0f));
            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_B", new Vector3(33f, EnemyStandY(topHall), 0f));
            PlaceGoal(levelRoot, roots, kit.Exit, new Vector3(48.5f, topExit + 1.3f, 0f));
        }

        private static ContentRoots ResolveRoots(LevelRoot levelRoot)
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

        private static GameObject Place(Transform parent, GameObject prefab, string name, Vector3 position)
        {
            if (prefab == null)
            {
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            return instance;
        }

        private static void PlaceSolid(Transform parent, GameObject prefab, string name, Vector3 position, Vector2 size)
        {
            var go = Place(parent, prefab, name, position);
            ApplyWorldSize(go, size);
        }

        private static void PlaceTiled(Transform parent, GameObject prefab, string name, Vector3 position, Vector2 size)
        {
            var go = Place(parent, prefab, name, position);
            ApplyWorldSize(go, size);
        }

        private static void PlaceTimed(
            Transform parent,
            GameObject prefab,
            string name,
            Vector3 position,
            Vector2 size,
            float onSeconds,
            float offSeconds,
            float delay,
            bool beginsOn)
        {
            var go = Place(parent, prefab, name, position);
            ApplyWorldSize(go, size);
            var timed = go != null ? go.GetComponent<TimedPlatform>() : null;
            timed?.Configure(onSeconds, offSeconds, delay, beginsOn);
        }

        private static GameObject PlaceGate(Transform parent, GameObject prefab, string name, Vector3 position, Vector2 size)
        {
            var go = Place(parent, prefab, name, position);
            ApplyWorldSize(go, size);
            return go;
        }

        private static void PlaceSwitch(
            Transform parent,
            GameObject prefab,
            string name,
            Vector3 position,
            PressureSwitch.Mode mode,
            float latchSeconds,
            GameObject gateObject)
        {
            var go = Place(parent, prefab, name, position);
            if (go == null)
            {
                return;
            }

            ApplyWorldSize(go, new Vector2(1.4f, 0.35f));
            var gate = gateObject != null ? gateObject.GetComponent<GateBarrier>() : null;
            var pressure = go.GetComponent<PressureSwitch>();
            if (pressure != null)
            {
                var so = new SerializedObject(pressure);
                so.FindProperty("mode").enumValueIndex = (int)mode;
                so.FindProperty("latchDuration").floatValue = Mathf.Max(0.1f, latchSeconds);
                var gatesProp = so.FindProperty("linkedGates");
                if (gate != null)
                {
                    gatesProp.arraySize = 1;
                    gatesProp.GetArrayElementAtIndex(0).objectReferenceValue = gate;
                }
                else
                {
                    gatesProp.arraySize = 0;
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(pressure);
            }
        }

        private static void PlaceMoving(
            Transform parent,
            GameObject prefab,
            string name,
            Vector3 position,
            Vector2 size,
            Vector2 localA,
            Vector2 localB,
            float speed)
        {
            var go = Place(parent, prefab, name, position);
            ApplyWorldSize(go, size);
            var moving = go != null ? go.GetComponent<MovingPlatform>() : null;
            moving?.ConfigurePath(localA, localB, speed);
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
        }

        private static void ApplyWorldSize(GameObject go, Vector2 worldSize)
        {
            if (go == null)
            {
                return;
            }

            go.transform.localScale = Vector3.one;
            var sprite = go.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.drawMode = SpriteDrawMode.Tiled;
                sprite.size = worldSize;
            }

            var box = go.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = worldSize;
            }
        }

        private static void PlacePitSpan(Transform parent, GameObject prefab, string name, float leftX, float rightX)
        {
            var width = rightX - leftX + 0.4f;
            var centerX = (leftX + rightX) * 0.5f;
            var go = Place(parent, prefab, name, new Vector3(centerX, PitDepthY, 0f));
            if (go != null)
            {
                go.transform.localScale = new Vector3(width, PitHeight, 1f);
            }
        }

        private static void PlaceFireOnPlatform(Transform parent, GameObject prefab, string name, float x, float platformTop)
        {
            Place(parent, prefab, name, new Vector3(x, platformTop + FireHalfHeight, 0f));
        }

        private static void PlaceEnemyOnPlatform(Transform parent, GameObject prefab, string name, float x, float platformTop)
        {
            Place(parent, prefab, name, new Vector3(x, platformTop + EnemyStandOffset, 0f));
        }

        private static void PlaceCoinLine(Transform parent, GameObject prefab, float startX, float endX, float y, float spacing)
        {
            var x = startX;
            var i = 0;
            while (x <= endX + 0.01f)
            {
                Place(parent, prefab, $"Coin_{i}", new Vector3(x, y, 0f));
                x += spacing;
                i++;
            }
        }

        private static void PlaceGoal(LevelRoot levelRoot, ContentRoots roots, GameObject exitPrefab, Vector3 position)
        {
            Place(levelRoot.transform, exitPrefab, "Exit_Goal", position);
            if (roots.End != null)
            {
                roots.End.position = position;
                var endPoint = roots.End.GetComponent<LevelEndPoint>();
                if (endPoint != null)
                {
                    var endSo = new SerializedObject(endPoint);
                    endSo.FindProperty("completeLevelOnEnter").boolValue = false;
                    endSo.ApplyModifiedPropertiesWithoutUndo();
                }
            }
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
        }

        private static void SetTransform(Transform t, Vector3 position)
        {
            if (t != null)
            {
                t.position = position;
            }
        }

        private static float PlatTop(float centerY, float height) => centerY + height * 0.5f;
        private static float EnemyStandY(float platformTop) => platformTop + EnemyStandOffset;

        private static LevelData CreateOrUpdateLevelData(LevelSpec spec)
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

        private static void MergeCatalog(LevelData[] newLevels)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var existing = new System.Collections.Generic.List<LevelData>();
            var so = new SerializedObject(catalog);
            var levelsProp = so.FindProperty("levels");
            for (var i = 0; i < levelsProp.arraySize; i++)
            {
                var level = levelsProp.GetArrayElementAtIndex(i).objectReferenceValue as LevelData;
                if (level != null && level.BuildIndex < 3)
                {
                    existing.Add(level);
                }
            }

            // Keep L1-L3 by asset path if list was empty/corrupt.
            if (existing.Count == 0)
            {
                TryAdd(existing, $"{DataFolder}/LevelData_01_LumenMeadows.asset");
                TryAdd(existing, $"{DataFolder}/LevelData_02_CascadeCliffs.asset");
                TryAdd(existing, $"{DataFolder}/LevelData_03_SkybridgeSpire.asset");
            }

            for (var i = 0; i < newLevels.Length; i++)
            {
                if (newLevels[i] != null)
                {
                    existing.Add(newLevels[i]);
                }
            }

            levelsProp.arraySize = existing.Count;
            for (var i = 0; i < existing.Count; i++)
            {
                levelsProp.GetArrayElementAtIndex(i).objectReferenceValue = existing[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }

        private static void TryAdd(System.Collections.Generic.List<LevelData> list, string path)
        {
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (data != null)
            {
                list.Add(data);
            }
        }

        private static void ConfigureBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Bootstrap.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene($"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity", true),
                new EditorBuildSettingsScene($"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity", true),
                new EditorBuildSettingsScene($"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity", true),
                new EditorBuildSettingsScene($"Assets/Scenes/{ProjectConstants.Level04SceneName}.unity", true),
                new EditorBuildSettingsScene($"Assets/Scenes/{ProjectConstants.Level05SceneName}.unity", true)
            };
            EditorBuildSettings.scenes = scenes;
        }

        private static void WireBootstrapCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null || !System.IO.File.Exists(BootstrapScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrap == null)
            {
                return;
            }

            var loader = bootstrap.GetComponent<LevelLoader>();
            if (loader == null)
            {
                return;
            }

            var so = new SerializedObject(loader);
            so.FindProperty("catalog").objectReferenceValue = catalog;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(loader);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void EnlargeCoinTriggers()
        {
            var coinPath = $"{PrefabItems}/Item_Coin.prefab";
            var root = PrefabUtility.LoadPrefabContents(coinPath);
            try
            {
                var circle = root.GetComponent<CircleCollider2D>();
                if (circle != null)
                {
                    circle.radius = Mathf.Max(circle.radius, 0.5f);
                    PrefabUtility.SaveAsPrefabAsset(root, coinPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void DisablePixelPerfect(string scenePath)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < cameras.Length; i++)
            {
                var camera = cameras[i];
                camera.orthographic = true;
                camera.orthographicSize = ProjectConstants.GameplayOrthographicSize;
                var behaviours = camera.GetComponents<Behaviour>();
                for (var j = 0; j < behaviours.Length; j++)
                {
                    if (behaviours[j] != null && behaviours[j].GetType().Name == "PixelPerfectCamera")
                    {
                        behaviours[j].enabled = false;
                    }
                }

                EditorUtility.SetDirty(camera);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }
    }
}
#endif
