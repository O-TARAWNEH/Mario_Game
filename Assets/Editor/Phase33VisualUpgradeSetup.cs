// Filename: Phase33VisualUpgradeSetup.cs
// Folder: Assets/Editor/
// Purpose: Visual-only upgrade — richer art, wire sprites, tiled platforms, pixel-perfect (Phase 33).
// Menu: Bounder Trail/Phase 33/Setup Visual Upgrade
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase33VisualUpgradeSetup.SetupVisualUpgrade
// Does NOT change gameplay systems, layouts, or collider world sizes.

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Core;
using BounderTrail.Levels;
using BounderTrail.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase33VisualUpgradeSetup
    {
        private const float Ppu = 32f;

        private static readonly Color Ink = new Color(0.1f, 0.12f, 0.16f, 1f);
        private static readonly Color PipBody = new Color(0.2f, 0.78f, 0.95f, 1f);
        private static readonly Color PipFace = new Color(0.92f, 0.97f, 1f, 1f);
        private static readonly Color PipShade = new Color(0.1f, 0.48f, 0.68f, 1f);
        private static readonly Color PipEye = new Color(0.08f, 0.12f, 0.2f, 1f);
        private static readonly Color PipShoe = new Color(0.15f, 0.2f, 0.35f, 1f);
        private static readonly Color PipTuft = new Color(0.05f, 0.55f, 0.75f, 1f);

        private static readonly string[] CampaignScenes =
        {
            $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity"
        };

        private static readonly (string prefab, string sprite)[] SimplePrefabSprites =
        {
            // Player art is Pip (Phase 33). Knight swap was reverted.
            ("Assets/Prefabs/Player/Player_Pip.prefab", "Assets/Art/Player/Pip_Idle_0.png"),
            ("Assets/Prefabs/Enemies/Enemy_Crawlbug.prefab", "Assets/Art/Enemies/Enemy_Crawlbug_0.png"),
            ("Assets/Prefabs/Enemies/Enemy_Dartling.prefab", "Assets/Art/Enemies/Enemy_Dartling_0.png"),
            ("Assets/Prefabs/Enemies/Enemy_Hopmite.prefab", "Assets/Art/Enemies/Enemy_Hopmite_0.png"),
            ("Assets/Prefabs/Enemies/Enemy_Skimmer.prefab", "Assets/Art/Enemies/Enemy_Skimmer_0.png"),
            ("Assets/Prefabs/Enemies/Enemy_Spikewatch.prefab", "Assets/Art/Enemies/Enemy_Spikewatch_0.png"),
            ("Assets/Prefabs/Enemies/Enemy_Spitter.prefab", "Assets/Art/Enemies/Enemy_Spitter_0.png"),
            ("Assets/Prefabs/Enemies/Enemy_Projectile.prefab", "Assets/Art/Enemies/Enemy_Projectile_0.png"),
            ("Assets/Prefabs/Items/Item_Coin.prefab", "Assets/Art/Items/Coin_Placeholder.png"),
            ("Assets/Prefabs/Items/Item_SpeedBurst.prefab", "Assets/Art/Items/PowerUp_SpeedBurst.png"),
            ("Assets/Prefabs/Items/Item_GlowShield.prefab", "Assets/Art/Items/PowerUp_GlowShield.png"),
            ("Assets/Prefabs/Items/Item_HeartDrop.prefab", "Assets/Art/Items/PowerUp_HeartDrop.png"),
            ("Assets/Prefabs/World/Hazard_DeathZone.prefab", "Assets/Art/World/Hazard_DeathZone.png"),
            ("Assets/Prefabs/World/Hazard_Spikes.prefab", "Assets/Art/World/Hazard_Spikes.png"),
            ("Assets/Prefabs/World/Hazard_MovingSpike.prefab", "Assets/Art/World/Hazard_MovingSpike.png"),
            ("Assets/Prefabs/World/Hazard_Fire.prefab", "Assets/Art/World/Hazard_Fire.png"),
            ("Assets/Prefabs/World/Checkpoint_Flag.prefab", "Assets/Art/World/Checkpoint_Flag.png"),
            ("Assets/Prefabs/World/LevelExitDoor.prefab", "Assets/Art/World/Env_Exit.png"),
            ("Assets/Prefabs/World/BouncePad.prefab", "Assets/Art/World/Env_Bounce.png")
        };

        private static readonly (string prefab, string sprite)[] TiledPlatformPrefabs =
        {
            ("Assets/Prefabs/World/Platform_Solid.prefab", "Assets/Art/World/Env_Solid.png"),
            ("Assets/Prefabs/World/Platform_OneWay.prefab", "Assets/Art/World/Env_OneWay.png"),
            ("Assets/Prefabs/World/Platform_Moving.prefab", "Assets/Art/World/Env_Moving.png")
        };

        [MenuItem("Bounder Trail/Phase 33/Setup Visual Upgrade")]
        public static void SetupVisualUpgrade()
        {
            EnsureFolders();
            BuildImprovedArt();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireSimplePrefabs();
            WireTiledPlatformPrefabs();
            WireSceneInstances();
            UpgradeBackdropsAndCameras();
            RebindPipClips();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var issues = ValidateInternal(logPass: false);
            if (issues == 0)
            {
                Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 33 visual upgrade complete — sprites wired, platforms tiled, pixel-perfect on.");
            }
            else
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 33 finished with {issues} issue(s).");
            }
        }

        [MenuItem("Bounder Trail/Phase 33/Validate Visual Upgrade")]
        public static void ValidateVisualUpgrade()
        {
            ValidateInternal(logPass: true);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "Player");
            EnsureFolder("Assets/Art", "Enemies");
            EnsureFolder("Assets/Art", "World");
            EnsureFolder("Assets/Art", "Items");
            EnsureFolder("Assets/Art", "UI");
            EnsureFolder("Assets/Art", "Tiles");
            EnsureFolder("Assets/Art", "Backgrounds");
            EnsureFolder("Assets/Art", "VFX");
        }

        private static void BuildImprovedArt()
        {
            // Pip poses (keep filenames Phase 25 used so anim clips stay valid).
            WritePipFrames("Pip_Idle", PipPose.Idle, 2);
            WritePipFrames("Pip_Walk", PipPose.Walk, 4);
            WritePipFrames("Pip_Run", PipPose.Run, 4);
            WritePipFrames("Pip_Jump", PipPose.Jump, 1);
            WritePipFrames("Pip_Fall", PipPose.Fall, 1);
            WritePipFrames("Pip_Land", PipPose.Land, 2);
            WritePipFrames("Pip_Death", PipPose.Death, 3);
            WriteSprite("Assets/Art/Player/Pip_Placeholder.png", DrawPip(PipPose.Idle, 0, 2), 32, 32);

            WriteSprite("Assets/Art/Enemies/Enemy_Crawlbug_0.png", DrawCrawlbug(), 32, 24);
            WriteSprite("Assets/Art/Enemies/Crawlbug_Placeholder.png", DrawCrawlbug(), 32, 24);
            WriteSprite("Assets/Art/Enemies/Enemy_Dartling_0.png", DrawDartling(), 32, 24);
            WriteSprite("Assets/Art/Enemies/Enemy_Hopmite_0.png", DrawHopmite(), 28, 28);
            WriteSprite("Assets/Art/Enemies/Enemy_Skimmer_0.png", DrawSkimmer(), 32, 20);
            WriteSprite("Assets/Art/Enemies/Enemy_Spikewatch_0.png", DrawSpikewatch(), 28, 28);
            WriteSprite("Assets/Art/Enemies/Enemy_Spitter_0.png", DrawSpitter(), 28, 28);
            WriteSprite("Assets/Art/Enemies/Enemy_Projectile_0.png", DrawProjectile(), 12, 12);

            WriteSprite("Assets/Art/World/Env_Solid.png", DrawGrassBlock(), 32, 32, tileable: true);
            WriteSprite("Assets/Art/World/Env_OneWay.png", DrawOneWayLedge(), 32, 16, tileable: true);
            WriteSprite("Assets/Art/World/Env_Moving.png", DrawMovingBlock(), 32, 32, tileable: true);
            WriteSprite("Assets/Art/World/Env_Bounce.png", DrawBounce(), 48, 20);
            WriteSprite("Assets/Art/World/Env_Exit.png", DrawExit(), 24, 48);
            WriteSprite("Assets/Art/World/Ground_Placeholder.png", DrawGrassBlock(), 32, 32, tileable: true);
            WriteSprite("Assets/Art/World/Hazard_DeathZone.png", DrawDeathZone(), 96, 24);
            WriteSprite("Assets/Art/World/Hazard_Spikes.png", DrawSpikes(), 48, 20);
            WriteSprite("Assets/Art/World/Hazard_MovingSpike.png", DrawSpikes(), 40, 20);
            WriteSprite("Assets/Art/World/Hazard_Fire.png", DrawFire(), 40, 32);
            WriteSprite("Assets/Art/World/Checkpoint_Flag.png", DrawFlag(), 24, 48);
            WriteSprite("Assets/Art/Tiles/Tile_Ground.png", DrawGrassBlock(), 32, 32, tileable: true);

            WriteSprite("Assets/Art/Items/Coin_Placeholder.png", DrawCoin(), 24, 24);
            WriteSprite("Assets/Art/Items/PowerUp_SpeedBurst.png", DrawSpeedBolt(), 24, 24);
            WriteSprite("Assets/Art/Items/PowerUp_GlowShield.png", DrawShield(), 24, 24);
            WriteSprite("Assets/Art/Items/PowerUp_HeartDrop.png", DrawHeart(), 24, 24);

            WriteSprite("Assets/Art/Backgrounds/BG_Sky_Meadow.png", DrawSky(new Color(0.45f, 0.78f, 1f), new Color(0.85f, 0.94f, 1f), true), 384, 192);
            WriteSprite("Assets/Art/Backgrounds/BG_Hills_Meadow.png", DrawHills(new Color(0.32f, 0.72f, 0.38f), new Color(0.2f, 0.5f, 0.28f)), 384, 96);
            WriteSprite("Assets/Art/Backgrounds/BG_Clouds_Meadow.png", DrawClouds(new Color(1f, 1f, 1f, 0.92f)), 384, 96);
            WriteSprite("Assets/Art/Backgrounds/BG_Sky_Cliffs.png", DrawSky(new Color(0.35f, 0.55f, 0.85f), new Color(0.75f, 0.85f, 0.95f), false), 384, 192);
            WriteSprite("Assets/Art/Backgrounds/BG_Hills_Cliffs.png", DrawHills(new Color(0.58f, 0.45f, 0.32f), new Color(0.38f, 0.28f, 0.2f)), 384, 96);
            WriteSprite("Assets/Art/Backgrounds/BG_Clouds_Cliffs.png", DrawClouds(new Color(0.9f, 0.92f, 0.95f, 0.85f)), 384, 96);
            WriteSprite("Assets/Art/Backgrounds/BG_Sky_Spire.png", DrawSky(new Color(0.2f, 0.22f, 0.45f), new Color(0.55f, 0.4f, 0.7f), false), 384, 192);
            WriteSprite("Assets/Art/Backgrounds/BG_Hills_Spire.png", DrawHills(new Color(0.32f, 0.28f, 0.5f), new Color(0.18f, 0.16f, 0.32f)), 384, 96);
            WriteSprite("Assets/Art/Backgrounds/BG_Clouds_Spire.png", DrawClouds(new Color(0.75f, 0.7f, 0.9f, 0.75f)), 384, 96);

            WriteSprite("Assets/Art/VFX/FX_Sparkle.png", DrawSparkle(), 16, 16);
            WriteSprite("Assets/Art/VFX/FX_Dust.png", DrawDust(), 16, 16);
            WriteSprite("Assets/Art/VFX/FX_HitRing.png", DrawHitRing(), 24, 24);
            WriteSprite("Assets/Art/UI/UI_Panel.png", DrawUiPanel(), 64, 64);
            WriteSprite("Assets/Art/UI/UI_Button.png", DrawUiButton(), 64, 32);
            WriteSprite("Assets/Art/UI/UI_HudBar.png", DrawUiHudBar(), 128, 32);
        }

        private static void WireSimplePrefabs()
        {
            for (var i = 0; i < SimplePrefabSprites.Length; i++)
            {
                var (prefabPath, spritePath) = SimplePrefabSprites[i];
                var sprite = LoadSprite(spritePath);
                if (sprite == null || !File.Exists(prefabPath))
                {
                    Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] Skip wire: {prefabPath}");
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    var sr = root.GetComponent<SpriteRenderer>();
                    if (sr == null)
                    {
                        sr = root.GetComponentInChildren<SpriteRenderer>();
                    }

                    if (sr != null)
                    {
                        sr.sprite = sprite;
                        sr.color = Color.white;
                        sr.drawMode = SpriteDrawMode.Simple;
                        EditorUtility.SetDirty(sr);
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void WireTiledPlatformPrefabs()
        {
            for (var i = 0; i < TiledPlatformPrefabs.Length; i++)
            {
                var (prefabPath, spritePath) = TiledPlatformPrefabs[i];
                var sprite = LoadSprite(spritePath);
                if (sprite == null || !File.Exists(prefabPath))
                {
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    ApplyTiledPlatformVisual(root, sprite);
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void WireSceneInstances()
        {
            var scenes = new[]
            {
                "Assets/Scenes/Gameplay.unity",
                CampaignScenes[0],
                CampaignScenes[1],
                CampaignScenes[2]
            };

            for (var s = 0; s < scenes.Length; s++)
            {
                if (!File.Exists(scenes[s]))
                {
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenes[s], OpenSceneMode.Single);
                var renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (var i = 0; i < renderers.Length; i++)
                {
                    var sr = renderers[i];
                    if (sr == null)
                    {
                        continue;
                    }

                    var go = sr.gameObject;
                    var name = go.name;

                    if (IsAuthoredPlatform(go))
                    {
                        ApplyTiledPlatformVisual(go, LoadSprite("Assets/Art/World/Env_Solid.png"));
                    }
                    else if (go.GetComponent<OneWayPlatform>() != null || name.StartsWith("Platform_OneWay") || name.StartsWith("OneWay_"))
                    {
                        ApplyTiledPlatformVisual(go, LoadSprite("Assets/Art/World/Env_OneWay.png"));
                    }
                    else if (name.StartsWith("Platform_Moving") || go.GetComponent<MovingPlatform>() != null)
                    {
                        ApplyTiledPlatformVisual(go, LoadSprite("Assets/Art/World/Env_Moving.png"));
                    }
                    else if (name.StartsWith("Bounce") || go.GetComponent<BouncePad>() != null)
                    {
                        AssignSimple(sr, "Assets/Art/World/Env_Bounce.png");
                    }
                    else if (name.Contains("Checkpoint") || name.Contains("Flag"))
                    {
                        AssignSimple(sr, "Assets/Art/World/Checkpoint_Flag.png");
                    }
                    else if (name.Contains("Exit") || name.Contains("Goal") || name.Contains("Door"))
                    {
                        AssignSimple(sr, "Assets/Art/World/Env_Exit.png");
                    }
                    else if (name.Contains("Spike") && name.Contains("Moving"))
                    {
                        AssignSimple(sr, "Assets/Art/World/Hazard_MovingSpike.png");
                    }
                    else if (name.Contains("Spike"))
                    {
                        AssignSimple(sr, "Assets/Art/World/Hazard_Spikes.png");
                    }
                    else if (name.Contains("Fire"))
                    {
                        AssignSimple(sr, "Assets/Art/World/Hazard_Fire.png");
                    }
                    else if (name.Contains("Death") || name.Contains("Pit"))
                    {
                        AssignSimple(sr, "Assets/Art/World/Hazard_DeathZone.png");
                    }
                    else if (name.Contains("Coin"))
                    {
                        AssignSimple(sr, "Assets/Art/Items/Coin_Placeholder.png");
                    }
                    else if (name.Contains("Heart"))
                    {
                        AssignSimple(sr, "Assets/Art/Items/PowerUp_HeartDrop.png");
                    }
                    else if (name.Contains("Shield"))
                    {
                        AssignSimple(sr, "Assets/Art/Items/PowerUp_GlowShield.png");
                    }
                    else if (name.Contains("Speed"))
                    {
                        AssignSimple(sr, "Assets/Art/Items/PowerUp_SpeedBurst.png");
                    }
                    else if (name.Contains("Crawlbug"))
                    {
                        AssignSimple(sr, "Assets/Art/Enemies/Enemy_Crawlbug_0.png");
                    }
                    else if (name.Contains("Dartling"))
                    {
                        AssignSimple(sr, "Assets/Art/Enemies/Enemy_Dartling_0.png");
                    }
                    else if (name.Contains("Hopmite"))
                    {
                        AssignSimple(sr, "Assets/Art/Enemies/Enemy_Hopmite_0.png");
                    }
                    else if (name.Contains("Skimmer"))
                    {
                        AssignSimple(sr, "Assets/Art/Enemies/Enemy_Skimmer_0.png");
                    }
                    else if (name.Contains("Spikewatch"))
                    {
                        AssignSimple(sr, "Assets/Art/Enemies/Enemy_Spikewatch_0.png");
                    }
                    else if (name.Contains("Spitter"))
                    {
                        AssignSimple(sr, "Assets/Art/Enemies/Enemy_Spitter_0.png");
                    }
                    else if (name.Contains("Projectile"))
                    {
                        AssignSimple(sr, "Assets/Art/Enemies/Enemy_Projectile_0.png");
                    }
                    else if (name.Contains("Pip") || name.Contains("Player"))
                    {
                        AssignSimple(sr, "Assets/Art/Player/Pip_Idle_0.png");
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, scenes[s]);
            }
        }

        private static void AssignSimple(SpriteRenderer sr, string spritePath)
        {
            var sprite = LoadSprite(spritePath);
            if (sprite == null || sr == null)
            {
                return;
            }

            sr.sprite = sprite;
            sr.color = Color.white;
            sr.drawMode = SpriteDrawMode.Simple;
            EditorUtility.SetDirty(sr);
        }

        private static bool IsAuthoredPlatform(GameObject go)
        {
            if (go.GetComponent<SolidPlatform>() != null)
            {
                return true;
            }

            var name = go.name;
            return name.StartsWith("Ground_")
                || name.StartsWith("Step_")
                || name.StartsWith("Shelf_")
                || name.StartsWith("Bridge_")
                || name.StartsWith("Tower_")
                || name.StartsWith("Arena_")
                || name.StartsWith("Spire_")
                || name.StartsWith("Pad_")
                || name.StartsWith("Platform_Solid");
        }

        /// <summary>
        /// Converts scaled Simple sprites into Tiled draw mode while preserving world collider size.
        /// </summary>
        private static void ApplyTiledPlatformVisual(GameObject go, Sprite sprite)
        {
            if (go == null || sprite == null)
            {
                return;
            }

            var sr = go.GetComponent<SpriteRenderer>();
            var col = go.GetComponent<BoxCollider2D>();
            var t = go.transform;
            if (sr == null || col == null)
            {
                return;
            }

            var scale = t.localScale;
            var worldSize = new Vector2(
                Mathf.Abs(col.size.x * scale.x),
                Mathf.Abs(col.size.y * scale.y));
            var worldOffset = new Vector2(col.offset.x * scale.x, col.offset.y * scale.y);

            // Avoid zero-size platforms.
            if (worldSize.x < 0.05f)
            {
                worldSize.x = sprite.bounds.size.x;
            }

            if (worldSize.y < 0.05f)
            {
                worldSize.y = sprite.bounds.size.y;
            }

            t.localScale = Vector3.one;
            sr.sprite = sprite;
            sr.color = Color.white;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = worldSize;
            sr.tileMode = SpriteTileMode.Continuous;

            col.size = worldSize;
            col.offset = worldOffset;
            col.autoTiling = false;

            EditorUtility.SetDirty(sr);
            EditorUtility.SetDirty(col);
            EditorUtility.SetDirty(go);
        }

        private static void UpgradeBackdropsAndCameras()
        {
            PlaceBackdrop(
                CampaignScenes[0],
                "Assets/Art/Backgrounds/BG_Sky_Meadow.png",
                "Assets/Art/Backgrounds/BG_Hills_Meadow.png",
                "Assets/Art/Backgrounds/BG_Clouds_Meadow.png",
                new Color(0.45f, 0.75f, 0.95f));
            PlaceBackdrop(
                CampaignScenes[1],
                "Assets/Art/Backgrounds/BG_Sky_Cliffs.png",
                "Assets/Art/Backgrounds/BG_Hills_Cliffs.png",
                "Assets/Art/Backgrounds/BG_Clouds_Cliffs.png",
                new Color(0.35f, 0.5f, 0.75f));
            PlaceBackdrop(
                CampaignScenes[2],
                "Assets/Art/Backgrounds/BG_Sky_Spire.png",
                "Assets/Art/Backgrounds/BG_Hills_Spire.png",
                "Assets/Art/Backgrounds/BG_Clouds_Spire.png",
                new Color(0.22f, 0.24f, 0.42f));
        }

        private static void PlaceBackdrop(
            string scenePath,
            string skyPath,
            string hillsPath,
            string cloudsPath,
            Color cameraClear)
        {
            if (!File.Exists(scenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var levelRoot = Object.FindAnyObjectByType<LevelRoot>();
            if (levelRoot == null)
            {
                return;
            }

            var decorations = levelRoot.transform.Find("Decorations");
            if (decorations == null)
            {
                var go = new GameObject("Decorations");
                go.transform.SetParent(levelRoot.transform, false);
                decorations = go.transform;
            }

            var existing = decorations.Find("LevelBackdrop");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var root = new GameObject("LevelBackdrop");
            root.transform.SetParent(decorations, false);
            var backdrop = root.AddComponent<LevelBackdrop>();

            var far = CreateBackdropLayer(root.transform, "Far_Sky", LoadSprite(skyPath), new Vector3(0f, 2f, 10f), new Vector3(28f, 15f, 1f), -30);
            var mid = CreateBackdropLayer(root.transform, "Mid_Hills", LoadSprite(hillsPath), new Vector3(0f, -1.8f, 8f), new Vector3(26f, 6.5f, 1f), -20);
            var near = CreateBackdropLayer(root.transform, "Near_Clouds", LoadSprite(cloudsPath), new Vector3(0f, 1.5f, 6f), new Vector3(24f, 5f, 1f), -10);

            var so = new SerializedObject(backdrop);
            so.FindProperty("farLayer").objectReferenceValue = far.transform;
            so.FindProperty("midLayer").objectReferenceValue = mid.transform;
            so.FindProperty("nearLayer").objectReferenceValue = near.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = cameraClear;
                camera.orthographic = true;
                camera.orthographicSize = ProjectConstants.GameplayOrthographicSize;
                DisablePixelPerfect(camera.gameObject);
                EditorUtility.SetDirty(camera);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void DisablePixelPerfect(GameObject camGo)
        {
            // URP Pixel Perfect draws a red Game-view warning when the display is
            // smaller than its reference resolution. Prefer a stable ortho size instead.
            var behaviours = camGo.GetComponents<Behaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                var typeName = behaviour.GetType().Name;
                if (typeName == "PixelPerfectCamera")
                {
                    behaviour.enabled = false;
                    EditorUtility.SetDirty(behaviour);
                }
            }
        }

        private static GameObject CreateBackdropLayer(
            Transform parent,
            string name,
            Sprite sprite,
            Vector3 localPos,
            Vector3 scale,
            int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            sr.color = Color.white;
            return go;
        }

        private static void RebindPipClips()
        {
            BindPipClip("Anim_Pip_Idle", LoadPipFrames("Pip_Idle", 2), 4f, true);
            BindPipClip("Anim_Pip_Walk", LoadPipFrames("Pip_Walk", 4), 10f, true);
            BindPipClip("Anim_Pip_Run", LoadPipFrames("Pip_Run", 4), 14f, true);
            BindPipClip("Anim_Pip_Jump", LoadPipFrames("Pip_Jump", 1), 8f, true);
            BindPipClip("Anim_Pip_Fall", LoadPipFrames("Pip_Fall", 1), 8f, true);
            BindPipClip("Anim_Pip_Land", LoadPipFrames("Pip_Land", 2), 12f, false);
            BindPipClip("Anim_Pip_Death", LoadPipFrames("Pip_Death", 3), 8f, false);
        }

        private static Sprite[] LoadPipFrames(string baseName, int count)
        {
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++)
            {
                frames[i] = LoadSprite($"Assets/Art/Player/{baseName}_{i}.png");
            }

            return frames;
        }

        private static void BindPipClip(string clipName, Sprite[] frames, float frameRate, bool loop)
        {
            if (frames == null || frames.Length == 0 || frames[0] == null)
            {
                return;
            }

            var path = $"Assets/Animations/Player/{clipName}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.frameRate = frameRate;
            var binding = new EditorCurveBinding
            {
                path = "",
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            };

            var keys = new ObjectReferenceKeyframe[frames.Length];
            for (var i = 0; i < frames.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = i / frameRate, value = frames[i] };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }

        private static int ValidateInternal(bool logPass)
        {
            var issues = 0;
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 33 — validating visual upgrade.");

            if (!File.Exists("Docs/Phase33VisualUpgrade.md"))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing Docs/Phase33VisualUpgrade.md");
                issues++;
            }

            for (var i = 0; i < SimplePrefabSprites.Length; i++)
            {
                issues += AssertPrefabHasSprite(SimplePrefabSprites[i].prefab);
            }

            for (var i = 0; i < TiledPlatformPrefabs.Length; i++)
            {
                issues += AssertPrefabHasSprite(TiledPlatformPrefabs[i].prefab);
                issues += AssertPrefabTiled(TiledPlatformPrefabs[i].prefab);
            }

            issues += AssertFile("Assets/Art/Backgrounds/BG_Clouds_Meadow.png");
            issues += AssertContains("Assets/Scripts/World/LevelBackdrop.cs", "nearLayer");

            if (issues == 0 && logPass)
            {
                Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 33 validation passed.");
            }
            else if (issues > 0)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Phase 33 validation failed ({issues} issue(s)).");
            }

            return issues;
        }

        private static int AssertPrefabHasSprite(string prefabPath)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var sr = root.GetComponent<SpriteRenderer>() ?? root.GetComponentInChildren<SpriteRenderer>();
                if (sr == null || sr.sprite == null)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing sprite on {prefabPath}");
                    return 1;
                }

                return 0;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static int AssertPrefabTiled(string prefabPath)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var sr = root.GetComponent<SpriteRenderer>();
                if (sr == null || sr.drawMode != SpriteDrawMode.Tiled)
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Expected Tiled drawMode on {prefabPath}");
                    return 1;
                }

                return 0;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static int AssertFile(string path)
        {
            if (File.Exists(path))
            {
                return 0;
            }

            Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing file: {path}");
            return 1;
        }

        private static int AssertContains(string path, string marker)
        {
            if (!File.Exists(path) || !File.ReadAllText(path).Contains(marker))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Expected '{marker}' in {path}");
                return 1;
            }

            return 0;
        }

        // --- Art generation ---

        private enum PipPose
        {
            Idle,
            Walk,
            Run,
            Jump,
            Fall,
            Land,
            Death
        }

        private static void WritePipFrames(string baseName, PipPose pose, int frames)
        {
            for (var i = 0; i < frames; i++)
            {
                WriteSprite($"Assets/Art/Player/{baseName}_{i}.png", DrawPip(pose, i, frames), 32, 32);
            }
        }

        private static Color[] DrawPip(PipPose pose, int frame, int frames)
        {
            var c = Clear(32, 32);
            var bob = (pose == PipPose.Idle || pose == PipPose.Walk || pose == PipPose.Run) && frame % 2 == 1 ? 1 : 0;
            var lean = (pose == PipPose.Walk || pose == PipPose.Run) && frame % 2 == 1 ? 1 : 0;
            var bodyY = 9 + bob;
            var bodyH = 14;
            var bodyW = 14;
            var bodyX = 9 + lean;

            switch (pose)
            {
                case PipPose.Jump:
                    bodyY = 12;
                    bodyH = 15;
                    bodyW = 12;
                    bodyX = 10;
                    break;
                case PipPose.Fall:
                    bodyY = 8;
                    bodyH = 16;
                    bodyW = 13;
                    bodyX = 9;
                    break;
                case PipPose.Land:
                    bodyY = 8;
                    bodyH = 12;
                    bodyW = 16;
                    bodyX = 8;
                    break;
                case PipPose.Death:
                    bodyY = 6 + frame;
                    bodyW = 14;
                    bodyH = 12;
                    break;
            }

            // Shoes
            FillRect(c, 32, 32, bodyX + 1, bodyY - 2, 5, 3, PipShoe);
            FillRect(c, 32, 32, bodyX + bodyW - 6, bodyY - 2, 5, 3, PipShoe);

            // Body + shade
            FillRound(c, 32, 32, bodyX, bodyY, bodyW, bodyH, PipBody);
            FillRect(c, 32, 32, bodyX, bodyY, 3, bodyH, PipShade);

            // Face plate
            FillRound(c, 32, 32, bodyX + 3, bodyY + 5, 9, 7, PipFace);

            // Eyes
            var eyeOpen = pose != PipPose.Death || frame < 2;
            if (eyeOpen)
            {
                FillRect(c, 32, 32, bodyX + 5, bodyY + 8, 2, 3, PipEye);
                FillRect(c, 32, 32, bodyX + 9, bodyY + 8, 2, 3, PipEye);
                Plot(c, 32, 32, bodyX + 5, bodyY + 10, Color.white);
                Plot(c, 32, 32, bodyX + 9, bodyY + 10, Color.white);
            }
            else
            {
                FillRect(c, 32, 32, bodyX + 5, bodyY + 9, 2, 1, PipEye);
                FillRect(c, 32, 32, bodyX + 9, bodyY + 9, 2, 1, PipEye);
            }

            // Hair tuft (Bounder identity — not a Mario hat)
            FillRect(c, 32, 32, bodyX + 4, bodyY + bodyH - 1, 6, 3, PipTuft);
            FillRect(c, 32, 32, bodyX + 6, bodyY + bodyH + 1, 3, 2, PipTuft);

            OutlineRound(c, 32, 32, bodyX, bodyY, bodyW, bodyH, Ink);
            return c;
        }

        private static Color[] DrawGrassBlock()
        {
            var c = Clear(32, 32);
            // Dirt body
            FillRect(c, 32, 32, 0, 0, 32, 26, new Color(0.55f, 0.35f, 0.18f, 1f));
            FillRect(c, 32, 32, 0, 0, 32, 8, new Color(0.42f, 0.26f, 0.12f, 1f));
            // Speckles
            Plot(c, 32, 32, 6, 10, new Color(0.35f, 0.2f, 0.1f, 1f));
            Plot(c, 32, 32, 18, 14, new Color(0.35f, 0.2f, 0.1f, 1f));
            Plot(c, 32, 32, 25, 8, new Color(0.7f, 0.5f, 0.3f, 1f));
            Plot(c, 32, 32, 10, 18, new Color(0.7f, 0.5f, 0.3f, 1f));
            // Grass top
            FillRect(c, 32, 32, 0, 24, 32, 8, new Color(0.35f, 0.78f, 0.32f, 1f));
            FillRect(c, 32, 32, 0, 28, 32, 4, new Color(0.55f, 0.92f, 0.4f, 1f));
            for (var x = 0; x < 32; x += 4)
            {
                FillRect(c, 32, 32, x, 26, 2, 3, new Color(0.25f, 0.65f, 0.25f, 1f));
            }

            OutlineRect(c, 32, 32, 0, 0, 32, 32, Ink);
            return c;
        }

        private static Color[] DrawOneWayLedge()
        {
            var c = Clear(32, 16);
            FillRect(c, 32, 16, 0, 6, 32, 10, new Color(0.45f, 0.85f, 0.55f, 1f));
            FillRect(c, 32, 16, 0, 12, 32, 4, new Color(0.7f, 0.95f, 0.7f, 1f));
            FillRect(c, 32, 16, 0, 6, 32, 2, new Color(0.3f, 0.6f, 0.35f, 1f));
            OutlineRect(c, 32, 16, 0, 6, 32, 10, Ink);
            return c;
        }

        private static Color[] DrawMovingBlock()
        {
            var c = Clear(32, 32);
            FillRect(c, 32, 32, 0, 0, 32, 32, new Color(0.25f, 0.62f, 0.82f, 1f));
            FillRect(c, 32, 32, 0, 24, 32, 8, new Color(0.45f, 0.82f, 0.95f, 1f));
            FillRect(c, 32, 32, 4, 4, 10, 10, new Color(0.15f, 0.4f, 0.55f, 1f));
            FillRect(c, 32, 32, 18, 4, 10, 10, new Color(0.15f, 0.4f, 0.55f, 1f));
            FillRect(c, 32, 32, 4, 18, 10, 8, new Color(0.15f, 0.4f, 0.55f, 1f));
            FillRect(c, 32, 32, 18, 18, 10, 8, new Color(0.15f, 0.4f, 0.55f, 1f));
            OutlineRect(c, 32, 32, 0, 0, 32, 32, Ink);
            return c;
        }

        private static Color[] DrawBounce()
        {
            var c = Clear(48, 20);
            FillRect(c, 48, 20, 2, 0, 44, 8, new Color(0.35f, 0.35f, 0.4f, 1f));
            FillRect(c, 48, 20, 0, 6, 48, 14, new Color(1f, 0.55f, 0.15f, 1f));
            FillRect(c, 48, 20, 0, 14, 48, 6, new Color(1f, 0.75f, 0.35f, 1f));
            OutlineRect(c, 48, 20, 0, 6, 48, 14, Ink);
            return c;
        }

        private static Color[] DrawExit()
        {
            var c = Clear(24, 48);
            FillRect(c, 24, 48, 10, 0, 4, 40, new Color(0.75f, 0.75f, 0.8f, 1f));
            FillRect(c, 24, 48, 2, 32, 20, 14, new Color(0.95f, 0.25f, 0.35f, 1f));
            FillRect(c, 24, 48, 2, 32, 20, 4, new Color(1f, 1f, 1f, 1f));
            FillRect(c, 24, 48, 2, 40, 20, 2, new Color(1f, 1f, 1f, 1f));
            OutlineRect(c, 24, 48, 2, 32, 20, 14, Ink);
            OutlineRect(c, 24, 48, 10, 0, 4, 40, Ink);
            return c;
        }

        private static Color[] DrawDeathZone()
        {
            var c = Clear(96, 24);
            for (var y = 0; y < 24; y++)
            {
                var a = 0.35f + y / 24f * 0.5f;
                for (var x = 0; x < 96; x++)
                {
                    c[y * 96 + x] = new Color(0.05f, 0.02f, 0.08f, a);
                }
            }

            return c;
        }

        private static Color[] DrawSpikes()
        {
            var c = Clear(48, 20);
            for (var i = 0; i < 6; i++)
            {
                var x = i * 8;
                FillTriangle(c, 48, 20, x + 4, 18, 7, 16, new Color(0.7f, 0.72f, 0.78f, 1f));
                OutlineTriangle(c, 48, 20, x + 4, 18, 7, 16, Ink);
            }

            FillRect(c, 48, 20, 0, 0, 48, 4, new Color(0.35f, 0.35f, 0.4f, 1f));
            return c;
        }

        private static Color[] DrawFire()
        {
            var c = Clear(40, 32);
            FillEllipse(c, 40, 32, 20, 10, 14, 10, new Color(1f, 0.45f, 0.1f, 1f));
            FillEllipse(c, 40, 32, 20, 14, 10, 12, new Color(1f, 0.75f, 0.2f, 1f));
            FillEllipse(c, 40, 32, 20, 18, 5, 8, new Color(1f, 0.95f, 0.6f, 1f));
            OutlineEllipse(c, 40, 32, 20, 12, 14, 14, Ink);
            return c;
        }

        private static Color[] DrawFlag()
        {
            var c = Clear(24, 48);
            FillRect(c, 24, 48, 3, 0, 3, 48, new Color(0.85f, 0.85f, 0.9f, 1f));
            FillRect(c, 24, 48, 6, 28, 16, 16, new Color(0.15f, 0.75f, 0.95f, 1f));
            FillRect(c, 24, 48, 6, 36, 16, 4, Color.white);
            OutlineRect(c, 24, 48, 6, 28, 16, 16, Ink);
            return c;
        }

        private static Color[] DrawCrawlbug()
        {
            var c = Clear(32, 24);
            FillEllipse(c, 32, 24, 16, 10, 12, 8, new Color(0.85f, 0.2f, 0.22f, 1f));
            FillEllipse(c, 32, 24, 16, 12, 8, 5, new Color(1f, 0.4f, 0.35f, 1f));
            FillRect(c, 32, 24, 10, 12, 2, 3, PipEye);
            FillRect(c, 32, 24, 18, 12, 2, 3, PipEye);
            FillRect(c, 32, 24, 6, 2, 3, 4, PipShoe);
            FillRect(c, 32, 24, 23, 2, 3, 4, PipShoe);
            OutlineEllipse(c, 32, 24, 16, 10, 12, 8, Ink);
            return c;
        }

        private static Color[] DrawDartling()
        {
            var c = Clear(32, 24);
            FillEllipse(c, 32, 24, 16, 12, 14, 7, new Color(1f, 0.55f, 0.15f, 1f));
            FillTriangle(c, 32, 24, 28, 12, 6, 8, new Color(0.9f, 0.35f, 0.1f, 1f));
            FillRect(c, 32, 24, 10, 13, 2, 2, PipEye);
            OutlineEllipse(c, 32, 24, 16, 12, 14, 7, Ink);
            return c;
        }

        private static Color[] DrawHopmite()
        {
            var c = Clear(28, 28);
            FillEllipse(c, 28, 28, 14, 14, 10, 10, new Color(0.45f, 0.9f, 0.3f, 1f));
            FillEllipse(c, 28, 28, 14, 16, 6, 5, new Color(0.7f, 1f, 0.5f, 1f));
            FillRect(c, 28, 28, 10, 16, 2, 3, PipEye);
            FillRect(c, 28, 28, 16, 16, 2, 3, PipEye);
            FillRect(c, 28, 28, 6, 4, 4, 5, PipShoe);
            FillRect(c, 28, 28, 18, 4, 4, 5, PipShoe);
            OutlineEllipse(c, 28, 28, 14, 14, 10, 10, Ink);
            return c;
        }

        private static Color[] DrawSkimmer()
        {
            var c = Clear(32, 20);
            FillEllipse(c, 32, 20, 16, 10, 14, 6, new Color(0.35f, 0.75f, 1f, 1f));
            FillEllipse(c, 32, 20, 16, 12, 8, 3, new Color(0.7f, 0.9f, 1f, 1f));
            FillRect(c, 32, 20, 12, 11, 2, 2, PipEye);
            FillRect(c, 32, 20, 18, 11, 2, 2, PipEye);
            OutlineEllipse(c, 32, 20, 16, 10, 14, 6, Ink);
            return c;
        }

        private static Color[] DrawSpikewatch()
        {
            var c = Clear(28, 28);
            FillEllipse(c, 28, 28, 14, 14, 10, 10, new Color(0.65f, 0.65f, 0.7f, 1f));
            for (var i = 0; i < 8; i++)
            {
                var ang = i / 8f * Mathf.PI * 2f;
                var x = 14 + Mathf.RoundToInt(Mathf.Cos(ang) * 11);
                var y = 14 + Mathf.RoundToInt(Mathf.Sin(ang) * 11);
                FillRect(c, 28, 28, x - 1, y - 1, 3, 3, new Color(0.85f, 0.85f, 0.9f, 1f));
            }

            FillRect(c, 28, 28, 11, 14, 2, 3, PipEye);
            FillRect(c, 28, 28, 15, 14, 2, 3, PipEye);
            OutlineEllipse(c, 28, 28, 14, 14, 10, 10, Ink);
            return c;
        }

        private static Color[] DrawSpitter()
        {
            var c = Clear(28, 28);
            FillRect(c, 28, 28, 6, 4, 16, 18, new Color(0.7f, 0.3f, 0.85f, 1f));
            FillRect(c, 28, 28, 8, 14, 12, 8, new Color(0.9f, 0.55f, 1f, 1f));
            FillRect(c, 28, 28, 10, 18, 3, 3, PipEye);
            FillRect(c, 28, 28, 16, 18, 3, 3, PipEye);
            FillRect(c, 28, 28, 11, 8, 6, 4, new Color(0.4f, 0.1f, 0.5f, 1f));
            OutlineRect(c, 28, 28, 6, 4, 16, 18, Ink);
            return c;
        }

        private static Color[] DrawProjectile()
        {
            var c = Clear(12, 12);
            FillEllipse(c, 12, 12, 6, 6, 5, 5, new Color(1f, 0.9f, 0.2f, 1f));
            OutlineEllipse(c, 12, 12, 6, 6, 5, 5, Ink);
            return c;
        }

        private static Color[] DrawCoin()
        {
            var c = Clear(24, 24);
            FillEllipse(c, 24, 24, 12, 12, 9, 9, new Color(1f, 0.82f, 0.15f, 1f));
            FillEllipse(c, 24, 24, 12, 12, 6, 6, new Color(1f, 0.92f, 0.4f, 1f));
            FillRect(c, 24, 24, 11, 8, 2, 8, new Color(0.85f, 0.6f, 0.1f, 1f));
            OutlineEllipse(c, 24, 24, 12, 12, 9, 9, Ink);
            return c;
        }

        private static Color[] DrawSpeedBolt()
        {
            var c = Clear(24, 24);
            FillTriangle(c, 24, 24, 14, 20, 10, 18, new Color(1f, 0.85f, 0.2f, 1f));
            FillTriangle(c, 24, 24, 10, 4, 8, 14, new Color(1f, 0.7f, 0.1f, 1f));
            OutlineTriangle(c, 24, 24, 14, 20, 10, 18, Ink);
            return c;
        }

        private static Color[] DrawShield()
        {
            var c = Clear(24, 24);
            OutlineEllipse(c, 24, 24, 12, 12, 9, 9, new Color(0.3f, 0.9f, 1f, 1f));
            OutlineEllipse(c, 24, 24, 12, 12, 7, 7, new Color(0.7f, 0.95f, 1f, 1f));
            return c;
        }

        private static Color[] DrawHeart()
        {
            var c = Clear(24, 24);
            FillEllipse(c, 24, 24, 8, 14, 5, 5, new Color(1f, 0.35f, 0.45f, 1f));
            FillEllipse(c, 24, 24, 16, 14, 5, 5, new Color(1f, 0.35f, 0.45f, 1f));
            FillTriangle(c, 24, 24, 12, 4, 10, 12, new Color(1f, 0.35f, 0.45f, 1f));
            OutlineEllipse(c, 24, 24, 8, 14, 5, 5, Ink);
            OutlineEllipse(c, 24, 24, 16, 14, 5, 5, Ink);
            return c;
        }

        private static Color[] DrawSky(Color bottom, Color top, bool sun)
        {
            var c = Clear(384, 192);
            for (var y = 0; y < 192; y++)
            {
                var t = y / 191f;
                var col = Color.Lerp(bottom, top, t);
                for (var x = 0; x < 384; x++)
                {
                    c[y * 384 + x] = col;
                }
            }

            if (sun)
            {
                FillEllipse(c, 384, 192, 320, 150, 22, 22, new Color(1f, 0.95f, 0.55f, 1f));
                FillEllipse(c, 384, 192, 320, 150, 14, 14, new Color(1f, 1f, 0.75f, 1f));
            }
            else
            {
                // Stars / distant orbs
                Plot(c, 384, 192, 40, 160, Color.white);
                Plot(c, 384, 192, 90, 140, Color.white);
                Plot(c, 384, 192, 200, 170, Color.white);
                Plot(c, 384, 192, 300, 155, Color.white);
            }

            return c;
        }

        private static Color[] DrawHills(Color near, Color far)
        {
            var c = Clear(384, 96);
            for (var x = 0; x < 384; x++)
            {
                var h1 = 30 + Mathf.RoundToInt(18f * Mathf.Sin(x * 0.03f) + 10f * Mathf.Sin(x * 0.01f));
                var h2 = 18 + Mathf.RoundToInt(12f * Mathf.Sin(x * 0.02f + 1f));
                for (var y = 0; y < h2; y++)
                {
                    Plot(c, 384, 96, x, y, far);
                }

                for (var y = 0; y < h1; y++)
                {
                    Plot(c, 384, 96, x, y, near);
                }
            }

            return c;
        }

        private static Color[] DrawClouds(Color col)
        {
            var c = Clear(384, 96);
            StampCloud(c, 50, 50, 28, col);
            StampCloud(c, 140, 65, 34, col);
            StampCloud(c, 240, 45, 26, col);
            StampCloud(c, 330, 70, 30, col);
            return c;
        }

        private static void StampCloud(Color[] c, int cx, int cy, int r, Color col)
        {
            FillEllipse(c, 384, 96, cx, cy, r, r / 2, col);
            FillEllipse(c, 384, 96, cx - r / 2, cy - 4, r / 2, r / 3, col);
            FillEllipse(c, 384, 96, cx + r / 2, cy - 2, r / 2 + 2, r / 3, col);
        }

        private static Color[] DrawSparkle()
        {
            var c = Clear(16, 16);
            FillRect(c, 16, 16, 7, 2, 2, 12, new Color(1f, 1f, 0.7f, 1f));
            FillRect(c, 16, 16, 2, 7, 12, 2, new Color(1f, 1f, 0.7f, 1f));
            return c;
        }

        private static Color[] DrawDust()
        {
            var c = Clear(16, 16);
            FillEllipse(c, 16, 16, 8, 8, 6, 4, new Color(0.75f, 0.7f, 0.55f, 0.7f));
            return c;
        }

        private static Color[] DrawHitRing()
        {
            var c = Clear(24, 24);
            OutlineEllipse(c, 24, 24, 12, 12, 9, 9, new Color(1f, 0.55f, 0.45f, 1f));
            OutlineEllipse(c, 24, 24, 12, 12, 7, 7, new Color(1f, 0.8f, 0.7f, 0.8f));
            return c;
        }

        private static Color[] DrawUiPanel()
        {
            var c = Clear(64, 64);
            FillRect(c, 64, 64, 0, 0, 64, 64, new Color(0.06f, 0.1f, 0.14f, 0.92f));
            OutlineRect(c, 64, 64, 0, 0, 64, 64, new Color(0.25f, 0.55f, 0.4f, 1f));
            OutlineRect(c, 64, 64, 2, 2, 60, 60, new Color(0.18f, 0.35f, 0.28f, 1f));
            return c;
        }

        private static Color[] DrawUiButton()
        {
            var c = Clear(64, 32);
            FillRect(c, 64, 32, 0, 0, 64, 32, new Color(0.18f, 0.48f, 0.34f, 1f));
            FillRect(c, 64, 32, 0, 22, 64, 10, new Color(0.28f, 0.62f, 0.44f, 1f));
            OutlineRect(c, 64, 32, 0, 0, 64, 32, Ink);
            return c;
        }

        private static Color[] DrawUiHudBar()
        {
            var c = Clear(128, 32);
            for (var y = 0; y < 32; y++)
            {
                var a = 0.55f - y / 32f * 0.25f;
                for (var x = 0; x < 128; x++)
                {
                    c[y * 128 + x] = new Color(0.02f, 0.05f, 0.08f, a);
                }
            }

            FillRect(c, 128, 32, 0, 0, 128, 2, new Color(0.25f, 0.55f, 0.4f, 0.8f));
            return c;
        }

        // --- Pixel helpers ---

        private static Sprite WriteSprite(string assetPath, Color[] pixels, int w, int h, bool tileable = false)
        {
            var dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = tileable ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(assetPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = Ppu;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.wrapMode = tileable ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                importer.spriteImportMode = SpriteImportMode.Single;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteExtrude = 0;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }

            return LoadSprite(assetPath);
        }

        private static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                return sprite;
            }

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite s)
                {
                    return s;
                }
            }

            return null;
        }

        private static void EnsureFolder(string parent, string name)
        {
            var path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static Color[] Clear(int w, int h)
        {
            var c = new Color[w * h];
            for (var i = 0; i < c.Length; i++)
            {
                c[i] = Color.clear;
            }

            return c;
        }

        private static void Plot(Color[] c, int w, int h, int x, int y, Color col)
        {
            if (x < 0 || y < 0 || x >= w || y >= h)
            {
                return;
            }

            c[y * w + x] = AlphaBlend(c[y * w + x], col);
        }

        private static Color AlphaBlend(Color dst, Color src)
        {
            if (src.a >= 0.999f)
            {
                return src;
            }

            if (src.a <= 0.001f)
            {
                return dst;
            }

            var a = src.a + dst.a * (1f - src.a);
            return new Color(
                (src.r * src.a + dst.r * dst.a * (1f - src.a)) / a,
                (src.g * src.a + dst.g * dst.a * (1f - src.a)) / a,
                (src.b * src.a + dst.b * dst.a * (1f - src.a)) / a,
                a);
        }

        private static void FillRect(Color[] c, int w, int h, int x, int y, int rw, int rh, Color col)
        {
            for (var yy = y; yy < y + rh; yy++)
            {
                for (var xx = x; xx < x + rw; xx++)
                {
                    Plot(c, w, h, xx, yy, col);
                }
            }
        }

        private static void OutlineRect(Color[] c, int w, int h, int x, int y, int rw, int rh, Color col)
        {
            for (var xx = x; xx < x + rw; xx++)
            {
                Plot(c, w, h, xx, y, col);
                Plot(c, w, h, xx, y + rh - 1, col);
            }

            for (var yy = y; yy < y + rh; yy++)
            {
                Plot(c, w, h, x, yy, col);
                Plot(c, w, h, x + rw - 1, yy, col);
            }
        }

        private static void FillRound(Color[] c, int w, int h, int x, int y, int rw, int rh, Color col)
        {
            FillRect(c, w, h, x + 1, y, rw - 2, rh, col);
            FillRect(c, w, h, x, y + 1, rw, rh - 2, col);
        }

        private static void OutlineRound(Color[] c, int w, int h, int x, int y, int rw, int rh, Color col)
        {
            OutlineRect(c, w, h, x, y, rw, rh, col);
            Plot(c, w, h, x, y, Color.clear);
            Plot(c, w, h, x + rw - 1, y, Color.clear);
            Plot(c, w, h, x, y + rh - 1, Color.clear);
            Plot(c, w, h, x + rw - 1, y + rh - 1, Color.clear);
        }

        private static void FillEllipse(Color[] c, int w, int h, int cx, int cy, int rx, int ry, Color col)
        {
            for (var y = -ry; y <= ry; y++)
            {
                for (var x = -rx; x <= rx; x++)
                {
                    if (rx == 0 || ry == 0)
                    {
                        continue;
                    }

                    if ((x * x) / (float)(rx * rx) + (y * y) / (float)(ry * ry) <= 1f)
                    {
                        Plot(c, w, h, cx + x, cy + y, col);
                    }
                }
            }
        }

        private static void OutlineEllipse(Color[] c, int w, int h, int cx, int cy, int rx, int ry, Color col)
        {
            for (var y = -ry; y <= ry; y++)
            {
                for (var x = -rx; x <= rx; x++)
                {
                    if (rx == 0 || ry == 0)
                    {
                        continue;
                    }

                    var v = (x * x) / (float)(rx * rx) + (y * y) / (float)(ry * ry);
                    if (v <= 1f && v >= 0.7f)
                    {
                        Plot(c, w, h, cx + x, cy + y, col);
                    }
                }
            }
        }

        private static void FillTriangle(Color[] c, int w, int h, int tipX, int tipY, int halfBase, int height, Color col)
        {
            for (var y = 0; y < height; y++)
            {
                var t = y / (float)Mathf.Max(1, height - 1);
                var half = Mathf.RoundToInt(halfBase * t);
                for (var x = -half; x <= half; x++)
                {
                    Plot(c, w, h, tipX + x, tipY - y, col);
                }
            }
        }

        private static void OutlineTriangle(Color[] c, int w, int h, int tipX, int tipY, int halfBase, int height, Color col)
        {
            for (var y = 0; y < height; y++)
            {
                var t = y / (float)Mathf.Max(1, height - 1);
                var half = Mathf.RoundToInt(halfBase * t);
                Plot(c, w, h, tipX - half, tipY - y, col);
                Plot(c, w, h, tipX + half, tipY - y, col);
            }

            for (var x = -halfBase; x <= halfBase; x++)
            {
                Plot(c, w, h, tipX + x, tipY - height + 1, col);
            }
        }
    }
}
#endif
