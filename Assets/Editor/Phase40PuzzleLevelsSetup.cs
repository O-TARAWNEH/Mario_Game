// Filename: Phase40PuzzleLevelsSetup.cs
// Folder: Assets/Editor/
// Purpose: Adds puzzle props + Level 04/05 campaign content (Phase 40).
// Menu: Bounder Trail/Phase 40/Setup Puzzle Levels
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase40PuzzleLevelsSetup.SetupPuzzleLevels

#if UNITY_EDITOR
using System.IO;
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

        private static string PendingRebuildFlagPath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "Phase40Rebuild.pending"));

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
                "Fair solid stepping cavern; optional blink secrets; one crawlbug; Speed Burst soft fire gate."),
            new LevelSpec(
                "level_05",
                "Lantern Lockworks",
                ProjectConstants.Level05SceneName,
                $"Assets/Scenes/{ProjectConstants.Level05SceneName}.unity",
                $"{DataFolder}/LevelData_05_LanternLockworks.asset",
                4,
                "Switch lockworks on solid bridges; latch/timed gates; one crawlbug; Glow Shield single fire.")
        };

        [InitializeOnLoadMethod]
        private static void ConsumePendingRebuild()
        {
            if (!File.Exists(PendingRebuildFlagPath))
            {
                return;
            }

            try
            {
                File.Delete(PendingRebuildFlagPath);
            }
            catch
            {
                // Retry next domain reload if the flag is locked.
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    return;
                }

                RebuildPuzzleLevelsOnly();
            };
        }

        /// <summary>Creates Temp/Phase40Rebuild.pending so the open Editor rebuilds after compile.</summary>
        public static void RequestPendingRebuild()
        {
            var path = PendingRebuildFlagPath;
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, "rebuild");
        }

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

        [MenuItem("Bounder Trail/Phase 40/Rebuild Echo Caverns Only")]
        public static void RebuildEchoCavernsOnly()
        {
            RebuildSingle(PuzzleCampaign[0], "Echo Caverns rebuilt with a fair solid stepping route.");
        }

        [MenuItem("Bounder Trail/Phase 40/Rebuild Lantern Lockworks Only")]
        public static void RebuildLanternLockworksOnly()
        {
            RebuildSingle(PuzzleCampaign[1], "Lantern Lockworks rebuilt with solid bridges and light combat.");
        }

        [MenuItem("Bounder Trail/Phase 40/Rebuild Puzzle Levels Only")]
        public static void RebuildPuzzleLevelsOnly()
        {
            EnsurePuzzlePrefabs();
            var kit = LoadPrefabs();
            if (kit == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Rebuild puzzle levels aborted — missing prefabs.");
                return;
            }

            for (var i = 0; i < PuzzleCampaign.Length; i++)
            {
                EnsureCampaignScene(PuzzleCampaign[i]);
                BuildLevel(PuzzleCampaign[i], kit);
                CreateOrUpdateLevelData(PuzzleCampaign[i]);
                DisablePixelPerfect(PuzzleCampaign[i].ScenePath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Echo Caverns + Lantern Lockworks layouts rebuilt.");
        }

        private static void RebuildSingle(LevelSpec spec, string successMessage)
        {
            EnsurePuzzlePrefabs();
            var kit = LoadPrefabs();
            if (kit == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Rebuild aborted — missing prefabs.");
                return;
            }

            EnsureCampaignScene(spec);
            BuildLevel(spec, kit);
            CreateOrUpdateLevelData(spec);
            DisablePixelPerfect(spec.ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] {successMessage}");
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
            ClearTemplateLeftovers(levelRoot.transform);

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

            // Visual polish without overwriting Pip.
            PaintEchoGroundStrip(levelRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, spec.ScenePath);
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Authored {spec.DisplayName}.");
        }

        private static void ClearTemplateLeftovers(Transform levelRoot)
        {
            if (levelRoot == null)
            {
                return;
            }

            for (var i = levelRoot.childCount - 1; i >= 0; i--)
            {
                var child = levelRoot.GetChild(i);
                if (IsTemplateLeftoverName(child.name)
                    || (child.name.StartsWith("Hazard_") && child.parent == levelRoot)
                    || (child.name.StartsWith("Enemy_") && child.parent == levelRoot))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            // Wipe leftover template props under content folders (Gameplay clone ships 7 enemies + markers).
            var folders = new[] { "Platforms", "Hazards", "Enemies", "Collectibles", "Checkpoints", "Decorations" };
            for (var f = 0; f < folders.Length; f++)
            {
                var folder = levelRoot.Find(folders[f]);
                if (folder == null)
                {
                    continue;
                }

                for (var i = folder.childCount - 1; i >= 0; i--)
                {
                    var child = folder.GetChild(i);
                    if (IsTemplateLeftoverName(child.name)
                        || child.name.StartsWith("Enemy_")
                        || child.name.StartsWith("Hazard_")
                        || child.name.StartsWith("Coin_")
                        || child.name.StartsWith("PowerUp_"))
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                }
            }
        }

        private static bool IsTemplateLeftoverName(string n)
        {
            return n.StartsWith("Marker_")
                   || n.StartsWith("Sample_")
                   || n.StartsWith("Platform_")
                   || n == "Ground_Main"
                   || n == "Ground"
                   || n == "Platform_Slope";
        }

        private static void PaintEchoGroundStrip(Transform levelRoot)
        {
            // Optional visual strip — Phase 24 helper may not be available; skip safely.
            var tilemaps = levelRoot.Find("Tilemaps");
            if (tilemaps == null)
            {
                return;
            }

            // Leave existing tilemap art alone; gameplay collision is prefab-owned.
        }

        private static void BuildEchoCaverns(LevelRoot levelRoot, ContentRoots roots, PrefabKit kit)
        {
            // Fair stepping cavern: solid pads with walk-jump gaps (~1.8–2.5), gentle rises.
            // Blink pads are optional secrets only. One crawlbug on a wide shelf.
            const float h = 0.65f;
            SetBounds(roots.Bounds, new Vector2(58f, 20f), new Vector2(24f, 1.5f));
            SetTransform(roots.Spawn, new Vector3(1.0f, 0.15f, 0f));

            var start = new Vector3(3.2f, -1.05f, 0f);
            var pad1 = new Vector3(10.0f, -0.55f, 0f);
            var shelfA = new Vector3(16.0f, -0.1f, 0f);
            var pad2 = new Vector3(21.8f, 0.35f, 0f);
            var shelfB = new Vector3(28.0f, 0.8f, 0f);
            var pad3 = new Vector3(34.2f, 1.2f, 0f);
            var shelfC = new Vector3(40.2f, 1.6f, 0f);
            var pad4 = new Vector3(45.8f, 2.0f, 0f);
            var exitPad = new Vector3(52.0f, 2.35f, 0f);

            const float startW = 7.0f;
            const float pad1W = 3.2f;
            const float shelfAW = 5.0f;
            const float pad2W = 3.0f;
            const float shelfBW = 5.8f;
            const float pad3W = 3.2f;
            const float shelfCW = 5.2f;
            const float pad4W = 3.0f;
            const float exitW = 7.5f;

            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Start", start, new Vector2(startW, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Pad1", pad1, new Vector2(pad1W, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_ShelfA", shelfA, new Vector2(shelfAW, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Pad2", pad2, new Vector2(pad2W, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_ShelfB", shelfB, new Vector2(shelfBW, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Pad3", pad3, new Vector2(pad3W, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_ShelfC", shelfC, new Vector2(shelfCW, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Pad4", pad4, new Vector2(pad4W, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Cave_Exit", exitPad, new Vector2(exitW, h));

            var topStart = PlatTop(start.y, h);
            var topA = PlatTop(shelfA.y, h);
            var topB = PlatTop(shelfB.y, h);
            var topC = PlatTop(shelfC.y, h);
            var topExit = PlatTop(exitPad.y, h);

            // Optional blink shortcut / secret coins above Start→Pad1 (main path stays solid).
            PlaceTimed(
                roots.Platforms,
                kit.Timed,
                "Blink_Secret_A",
                new Vector3(7.4f, 0.45f, 0f),
                new Vector2(1.6f, 0.4f),
                1.5f,
                1.0f,
                0f,
                true);
            PlaceTimed(
                roots.Platforms,
                kit.Timed,
                "Blink_Secret_B",
                new Vector3(8.9f, 0.65f, 0f),
                new Vector2(1.6f, 0.4f),
                1.5f,
                1.0f,
                0.7f,
                false);

            // Soft heart ledge above Shelf A (bounce assist, not required).
            Place(roots.Platforms, kit.Bounce, "Bounce_Assist", new Vector3(shelfA.x + 1.4f, topA - 0.12f, 0f));
            PlaceTiled(roots.Platforms, kit.OneWay, "OneWay_HeartLedge", new Vector3(18.6f, 1.55f, 0f), new Vector2(2.4f, 0.35f));

            // Optional mover beside Pad4 — exit is already reachable via Pad4 solid step.
            PlaceMoving(
                roots.Platforms,
                kit.Moving,
                "Mover_Assist",
                new Vector3(48.6f, 2.15f, 0f),
                new Vector2(2.6f, h),
                new Vector2(-1.0f, 0f),
                new Vector2(1.0f, 0f),
                1.6f);

            PlacePitSpan(roots.Hazards, kit.DeathZone, "Hazard_Pit_Cave", PlatRight(start.x, startW), PlatLeft(exitPad.x, exitW));

            // One avoidable spike pack on Shelf A trailing edge.
            PlaceSpikesOnEdge(roots.Hazards, kit.Spikes, "Hazard_Spikes_A", PlatRight(shelfA.x, shelfAW), topA);

            // Single fire near exit entrance — Speed Burst sits on Shelf C with room to grab first.
            Place(roots.Collectibles, kit.SpeedBurst, "PowerUp_SpeedBurst", new Vector3(shelfC.x - 0.8f, topC + 0.7f, 0f));
            PlaceFireOnPlatform(roots.Hazards, kit.Fire, "Hazard_Fire_ExitGate", PlatLeft(exitPad.x, exitW) + 1.4f, topExit);

            // One patrol only — wide Shelf B has walk-around space; no hopmite/spitter stack.
            PlaceEnemyOnPlatform(roots.Enemies, kit.Crawlbug, "Enemy_Crawlbug_A", shelfB.x + 1.2f, topB);

            PlaceCoinLine(roots.Collectibles, kit.Coin, 1.4f, 5.8f, topStart + 0.55f, 1.2f);
            PlaceCoinLine(roots.Collectibles, kit.Coin, 14.2f, 17.6f, topA + 0.55f, 1.15f);
            Place(roots.Collectibles, kit.Coin, "Coin_Secret_Blink", new Vector3(8.1f, 1.35f, 0f));
            Place(roots.Collectibles, kit.HeartDrop, "PowerUp_HeartDrop", new Vector3(18.6f, 2.1f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 26.0f, 30.0f, topB + 0.55f, 1.2f);
            PlaceCoinLine(roots.Collectibles, kit.Coin, 38.2f, 42.0f, topC + 0.55f, 1.2f);
            PlaceCoinLine(roots.Collectibles, kit.Coin, 49.5f, 54.5f, topExit + 0.55f, 1.25f);

            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_A", new Vector3(shelfA.x - 0.6f, EnemyStandY(topA), 0f));
            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_B", new Vector3(shelfC.x - 1.0f, EnemyStandY(topC), 0f));

            PlaceGoal(levelRoot, roots, kit.Exit, new Vector3(PlatRight(exitPad.x, exitW) - 1.5f, topExit + 1.15f, 0f));
        }

        private static void BuildLanternLockworks(LevelRoot levelRoot, ContentRoots roots, PrefabKit kit)
        {
            // Puzzle-first lockworks: solid bridges between rooms (blink optional).
            // Switches use latch modes (single-player safe). One crawlbug total.
            const float h = 0.55f;
            SetBounds(roots.Bounds, new Vector2(64f, 22f), new Vector2(27f, 2.5f));
            SetTransform(roots.Spawn, new Vector3(0.8f, 0.25f, 0f));

            var start = new Vector3(3.2f, -0.4f, 0f);
            var bridge1 = new Vector3(10.4f, 0.05f, 0f);
            var roomA = new Vector3(16.8f, 0.5f, 0f);
            var bridge2 = new Vector3(23.4f, 0.95f, 0f);
            var roomB = new Vector3(30.2f, 1.4f, 0f);
            var bridge3 = new Vector3(37.0f, 1.85f, 0f);
            var hall = new Vector3(44.0f, 2.3f, 0f);
            var bridge4 = new Vector3(50.8f, 2.7f, 0f);
            var exitPad = new Vector3(57.5f, 3.05f, 0f);

            const float startW = 7.5f;
            const float bridge1W = 3.2f;
            const float roomAW = 6.2f;
            const float bridge2W = 3.2f;
            const float roomBW = 6.4f;
            const float bridge3W = 3.2f;
            const float hallW = 7.5f;
            const float bridge4W = 3.4f;
            const float exitW = 7.5f;

            PlaceSolid(roots.Platforms, kit.Solid, "Lock_Start", start, new Vector2(startW, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_Bridge1", bridge1, new Vector2(bridge1W, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_RoomA", roomA, new Vector2(roomAW, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_Bridge2", bridge2, new Vector2(bridge2W, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_RoomB", roomB, new Vector2(roomBW, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_Bridge3", bridge3, new Vector2(bridge3W, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_Hall", hall, new Vector2(hallW, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_Bridge4", bridge4, new Vector2(bridge4W, h));
            PlaceSolid(roots.Platforms, kit.Solid, "Lock_Exit", exitPad, new Vector2(exitW, h));

            var topStart = PlatTop(start.y, h);
            var topA = PlatTop(roomA.y, h);
            var topB = PlatTop(roomB.y, h);
            var topHall = PlatTop(hall.y, h);
            var topExit = PlatTop(exitPad.y, h);

            // Puzzle 1: short timed latch opens path onto Bridge1 (hold mode is not solo-viable).
            var gate1 = PlaceGate(
                roots.Platforms,
                kit.Gate,
                "Gate_Intro",
                new Vector3(PlatRight(start.x, startW) + 0.35f, topStart + 1.0f, 0f),
                new Vector2(0.65f, 2.1f));
            PlaceSwitch(
                roots.Platforms,
                kit.Switch,
                "Switch_Intro",
                new Vector3(5.0f, topStart + 0.05f, 0f),
                PressureSwitch.Mode.LatchTimed,
                5.5f,
                gate1);

            // Puzzle 2: timed latch on Room A → Bridge2. Optional blink coins above the solid bridge.
            PlaceTimed(
                roots.Platforms,
                kit.Timed,
                "Blink_Lock_A",
                new Vector3(20.4f, 1.55f, 0f),
                new Vector2(1.5f, 0.4f),
                1.45f,
                1.0f,
                0f,
                true);
            PlaceTimed(
                roots.Platforms,
                kit.Timed,
                "Blink_Lock_B",
                new Vector3(21.8f, 1.7f, 0f),
                new Vector2(1.5f, 0.4f),
                1.45f,
                1.0f,
                0.65f,
                false);
            var gate2 = PlaceGate(
                roots.Platforms,
                kit.Gate,
                "Gate_Timed",
                new Vector3(PlatRight(roomA.x, roomAW) + 0.35f, topA + 1.0f, 0f),
                new Vector2(0.65f, 2.2f));
            PlaceSwitch(
                roots.Platforms,
                kit.Switch,
                "Switch_Timed",
                new Vector3(roomA.x - 1.4f, topA + 0.05f, 0f),
                PressureSwitch.Mode.LatchTimed,
                5.0f,
                gate2);

            // Puzzle 3: permanent latch opens Bridge3 into the fire hall.
            var gate3 = PlaceGate(
                roots.Platforms,
                kit.Gate,
                "Gate_Latch",
                new Vector3(PlatRight(roomB.x, roomBW) + 0.35f, topB + 1.05f, 0f),
                new Vector2(0.7f, 2.3f));
            PlaceSwitch(
                roots.Platforms,
                kit.Switch,
                "Switch_Latch",
                new Vector3(roomB.x - 1.2f, topB + 0.05f, 0f),
                PressureSwitch.Mode.LatchPermanent,
                0f,
                gate3);

            // Glow Shield before a single fire patch (not a stacked fire gauntlet).
            Place(roots.Collectibles, kit.GlowShield, "PowerUp_GlowShield", new Vector3(hall.x - 2.2f, topHall + 0.75f, 0f));
            PlaceFireOnPlatform(roots.Hazards, kit.Fire, "Hazard_Fire_Hall", hall.x + 0.6f, topHall);

            // Slow optional mover near Bridge4 — exit pad is already a solid hop away.
            PlaceMoving(
                roots.Platforms,
                kit.Moving,
                "Mover_Assist",
                new Vector3(54.0f, 2.85f, 0f),
                new Vector2(2.8f, h),
                new Vector2(-1.0f, 0f),
                new Vector2(1.0f, 0f),
                1.55f);

            PlacePitSpan(roots.Hazards, kit.DeathZone, "Hazard_Pit_Lock", PlatRight(start.x, startW), PlatLeft(exitPad.x, exitW));

            // One crawlbug on Room B far from the switch so the latch puzzle stays readable.
            PlaceEnemyOnPlatform(roots.Enemies, kit.Crawlbug, "Enemy_Crawlbug_A", roomB.x + 1.8f, topB);

            PlaceCoinLine(roots.Collectibles, kit.Coin, 1.4f, 5.6f, topStart + 0.55f, 1.15f);
            PlaceCoinLine(roots.Collectibles, kit.Coin, 14.6f, 18.8f, topA + 0.55f, 1.2f);
            Place(roots.Collectibles, kit.Coin, "Coin_Secret_Blink", new Vector3(21.1f, 2.35f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 28.0f, 32.4f, topB + 0.55f, 1.2f);
            Place(roots.Collectibles, kit.HeartDrop, "PowerUp_HeartDrop", new Vector3(hall.x + 2.4f, topHall + 0.8f, 0f));
            PlaceCoinLine(roots.Collectibles, kit.Coin, 55.0f, 60.0f, topExit + 0.6f, 1.25f);

            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_A", new Vector3(roomA.x, EnemyStandY(topA), 0f));
            Place(roots.Checkpoints, kit.Checkpoint, "Checkpoint_B", new Vector3(hall.x - 1.5f, EnemyStandY(topHall), 0f));
            PlaceGoal(levelRoot, roots, kit.Exit, new Vector3(PlatRight(exitPad.x, exitW) - 1.5f, topExit + 1.2f, 0f));
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

        private static void PlaceSpikesOnEdge(
            Transform parent,
            GameObject prefab,
            string name,
            float platformRightEdge,
            float platformTop)
        {
            if (prefab == null)
            {
                return;
            }

            var x = platformRightEdge - 0.55f;
            var y = platformTop + SpikeHalfHeight;
            Place(parent, prefab, name, new Vector3(x, y, 0f));
        }

        private static void PlaceEnemyOnPlatform(Transform parent, GameObject prefab, string name, float x, float platformTop)
        {
            Place(parent, prefab, name, new Vector3(x, platformTop + EnemyStandOffset, 0f));
        }

        private static float PlatLeft(float centerX, float width) => centerX - width * 0.5f;
        private static float PlatRight(float centerX, float width) => centerX + width * 0.5f;

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
