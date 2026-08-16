// Filename: Phase41GameplayRepairSetup.cs
// Folder: Assets/Editor/
// Purpose: Knight player visual, level layout rebuild, physics/UI repairs (Phase 41).
// Menu: Bounder Trail/Phase 41/Setup Gameplay Repair
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase41GameplayRepairSetup.SetupGameplayRepair

#if UNITY_EDITOR
using System.IO;
using BounderTrail.Core;
using BounderTrail.Player;
using BounderTrail.UI;
using BounderTrail.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.EditorTools
{
    public static class Phase41GameplayRepairSetup
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string KnightSourceFileName = "Knight_Image.png";
        private const string KnightAssetPath = "Assets/Art/Player/Knight.png";
        private const string SpikesPrefabPath = "Assets/Prefabs/World/Hazard_Spikes.prefab";
        private const string MovingSpikePrefabPath = "Assets/Prefabs/World/Hazard_MovingSpike.prefab";

        private static readonly string[] AllGameplayScenes =
        {
            "Assets/Scenes/Gameplay.unity",
            $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level04SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level05SceneName}.unity"
        };

        [MenuItem("Bounder Trail/Phase 41/Setup Gameplay Repair")]
        public static void SetupGameplayRepair()
        {
            ImportKnightSprite();
            ApplyKnightToPlayer();
            HardenHazardPrefabs();

            // Rebuild authored layouts so platforms, enemies, coins, and hazards match again.
            Phase24LevelDesignSetup.SetupLevelDesign();
            Phase40PuzzleLevelsSetup.SetupPuzzleLevels();
            Phase33VisualUpgradeSetup.SetupVisualUpgrade();
            Phase6CameraSetup.SetupCameraSystem();
            Phase36HeartsHudSetup.SetupHeartsHud();
            Phase37OverlayUiScaleSetup.SetupOverlayUiScale();

            for (var i = 0; i < AllGameplayScenes.Length; i++)
            {
                if (File.Exists(AllGameplayScenes[i]))
                {
                    WireButtonsInScene(AllGameplayScenes[i]);
                }
            }

            // Re-apply knight after visual upgrade so Pip art generation cannot overwrite it.
            ApplyKnightToPlayer();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"{GameLog.ProjectPrefix}[Setup] Phase 41 ready — knight player, " +
                "level layouts rebuilt, platforms hardened, ResumeButton punch safe.");
        }

        private static void ImportKnightSprite()
        {
            EnsureFolder("Assets/Art", "Player");

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Could not resolve project root for knight art.");
                return;
            }

            var sourcePath = Path.Combine(projectRoot, KnightSourceFileName);
            if (!File.Exists(sourcePath))
            {
                // Fallbacks for common typos / locations.
                var alt = Path.Combine(projectRoot, "Lnight_Image.png");
                if (File.Exists(alt))
                {
                    sourcePath = alt;
                }
                else
                {
                    Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing {KnightSourceFileName} in project root.");
                    return;
                }
            }

            var croppedPng = MakeTransparentAndCrop(File.ReadAllBytes(sourcePath));
            var absoluteDest = Path.Combine(projectRoot, KnightAssetPath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteDest) ?? projectRoot);
            File.WriteAllBytes(absoluteDest, croppedPng);
            AssetDatabase.ImportAsset(KnightAssetPath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(KnightAssetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Knight texture importer missing.");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 180f;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 1024;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMode = (int)SpriteImportMode.Single;
            settings.spritePixelsPerUnit = 180f;
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            settings.spritePivot = new Vector2(0.5f, 0f);
            settings.filterMode = FilterMode.Point;
            settings.mipmapEnabled = false;
            settings.alphaIsTransparency = true;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static byte[] MakeTransparentAndCrop(byte[] pngBytes)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(pngBytes))
            {
                Object.DestroyImmediate(tex);
                return pngBytes;
            }

            var width = tex.width;
            var height = tex.height;
            var src = tex.GetPixels32();

            // Flood-fill light gray studio / checker backdrop from the edges.
            var clear = new bool[src.Length];
            var queue = new System.Collections.Generic.Queue<int>(src.Length / 4);
            void TrySeed(int x, int y)
            {
                if (x < 0 || y < 0 || x >= width || y >= height)
                {
                    return;
                }

                var i = y * width + x;
                if (clear[i] || !IsBackdrop(src[i]))
                {
                    return;
                }

                clear[i] = true;
                queue.Enqueue(i);
            }

            for (var x = 0; x < width; x++)
            {
                TrySeed(x, 0);
                TrySeed(x, height - 1);
            }

            for (var y = 0; y < height; y++)
            {
                TrySeed(0, y);
                TrySeed(width - 1, y);
            }

            while (queue.Count > 0)
            {
                var i = queue.Dequeue();
                var x = i % width;
                var y = i / width;
                TrySeed(x + 1, y);
                TrySeed(x - 1, y);
                TrySeed(x, y + 1);
                TrySeed(x, y - 1);
            }

            for (var i = 0; i < src.Length; i++)
            {
                if (clear[i])
                {
                    src[i] = new Color32(0, 0, 0, 0);
                }
            }

            var minX = width;
            var minY = height;
            var maxX = 0;
            var maxY = 0;
            var found = false;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (src[y * width + x].a < 8)
                    {
                        continue;
                    }

                    found = true;
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (!found)
            {
                Object.DestroyImmediate(tex);
                return pngBytes;
            }

            minX = Mathf.Max(0, minX - 4);
            minY = Mathf.Max(0, minY - 4);
            maxX = Mathf.Min(width - 1, maxX + 4);
            maxY = Mathf.Min(height - 1, maxY + 4);

            var cropW = maxX - minX + 1;
            var cropH = maxY - minY + 1;
            var dst = new Color32[cropW * cropH];
            for (var y = 0; y < cropH; y++)
            {
                for (var x = 0; x < cropW; x++)
                {
                    dst[y * cropW + x] = src[(minY + y) * width + (minX + x)];
                }
            }

            var cropped = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
            cropped.SetPixels32(dst);
            cropped.Apply(false, false);
            var encoded = cropped.EncodeToPNG();
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(cropped);
            return encoded;
        }

        private static bool IsBackdrop(Color32 c)
        {
            var max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            var min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            return max - min <= 18 && min >= 175;
        }

        private static void ApplyKnightToPlayer()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(KnightAssetPath);
            if (sprite == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Knight sprite missing at {KnightAssetPath}");
                return;
            }

            if (!File.Exists(PlayerPrefabPath))
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing {PlayerPrefabPath}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var visual = root.transform.Find("Visual");
                if (visual == null)
                {
                    var visualGo = new GameObject("Visual");
                    visual = visualGo.transform;
                    visual.SetParent(root.transform, false);
                    visual.localPosition = Vector3.zero;
                    visual.localRotation = Quaternion.identity;
                    visual.localScale = Vector3.one;
                }

                var rootRenderer = root.GetComponent<SpriteRenderer>();
                SpriteRenderer visualRenderer;
                if (rootRenderer != null)
                {
                    // Move renderer onto Visual so squash/stretch never scales the Rigidbody.
                    visualRenderer = visual.GetComponent<SpriteRenderer>();
                    if (visualRenderer == null)
                    {
                        visualRenderer = visual.gameObject.AddComponent<SpriteRenderer>();
                    }

                    EditorUtility.CopySerialized(rootRenderer, visualRenderer);
                    Object.DestroyImmediate(rootRenderer);
                }
                else
                {
                    visualRenderer = visual.GetComponent<SpriteRenderer>();
                    if (visualRenderer == null)
                    {
                        visualRenderer = visual.gameObject.AddComponent<SpriteRenderer>();
                    }
                }

                visualRenderer.sprite = sprite;
                visualRenderer.color = Color.white;
                visualRenderer.sortingOrder = 10;
                visualRenderer.drawMode = SpriteDrawMode.Simple;
                visualRenderer.flipX = false;
                visualRenderer.flipY = false;

                // Single knight frame — disable Pip animation overrides.
                var animator = root.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.enabled = false;
                    animator.runtimeAnimatorController = null;
                }

                var playerAnimator = root.GetComponent<PlayerAnimator>();
                if (playerAnimator != null)
                {
                    playerAnimator.enabled = false;
                }

                var body = root.GetComponent<Rigidbody2D>();
                if (body != null)
                {
                    body.bodyType = RigidbodyType2D.Dynamic;
                    body.simulated = true;
                    body.gravityScale = Mathf.Max(3.2f, body.gravityScale);
                    body.freezeRotation = true;
                    body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                    body.interpolation = RigidbodyInterpolation2D.Interpolate;
                }

                var capsule = root.GetComponent<CapsuleCollider2D>();
                if (capsule != null)
                {
                    // Match knight silhouette (taller than Pip blob).
                    capsule.direction = CapsuleDirection2D.Vertical;
                    capsule.size = new Vector2(0.72f, 1.15f);
                    capsule.offset = new Vector2(0f, 0.08f);
                    capsule.isTrigger = false;
                }

                var groundCheck = root.transform.Find("GroundCheck");
                if (groundCheck != null)
                {
                    groundCheck.localPosition = new Vector3(0f, -0.52f, 0f);
                }

                var controller = root.GetComponent<PlayerController>();
                if (controller != null)
                {
                    var so = new SerializedObject(controller);
                    so.FindProperty("spriteRenderer").objectReferenceValue = visualRenderer;
                    so.FindProperty("apexHangGravityMultiplier").floatValue = 0.72f;
                    so.FindProperty("apexHangVelocityThreshold").floatValue = 0.85f;
                    so.FindProperty("gravity").floatValue = 3.45f;
                    so.FindProperty("fallGravityMultiplier").floatValue = 2.35f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                var hurt = root.GetComponent<PlayerHurtFeedback>();
                if (hurt != null)
                {
                    var so = new SerializedObject(hurt);
                    so.FindProperty("spriteRenderer").objectReferenceValue = visualRenderer;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                var powerFx = root.GetComponent<PlayerPowerUpFeedback>();
                if (powerFx != null)
                {
                    var so = new SerializedObject(powerFx);
                    so.FindProperty("spriteRenderer").objectReferenceValue = visualRenderer;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                var squash = root.GetComponent<PlayerSquashStretch>();
                if (squash == null)
                {
                    squash = root.AddComponent<PlayerSquashStretch>();
                }

                {
                    var so = new SerializedObject(squash);
                    so.FindProperty("playerController").objectReferenceValue = controller;
                    so.FindProperty("visualRoot").objectReferenceValue = visual;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    squash.enabled = true;
                }

                EditorUtility.SetDirty(root);
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            // Update scene instances that already reference the prefab.
            for (var i = 0; i < AllGameplayScenes.Length; i++)
            {
                if (!File.Exists(AllGameplayScenes[i]))
                {
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(AllGameplayScenes[i], OpenSceneMode.Single);
                var players = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (var p = 0; p < players.Length; p++)
                {
                    var go = players[p].gameObject;
                    if (PrefabUtility.IsPartOfPrefabInstance(go))
                    {
                        PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, AllGameplayScenes[i]);
            }
        }

        private static void HardenHazardPrefabs()
        {
            HardenSpikePrefab(SpikesPrefabPath);
            HardenSpikePrefab(MovingSpikePrefabPath);
        }

        private static void HardenSpikePrefab(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var hazard = root.GetComponent<EnvironmentalHazard>();
                if (hazard != null)
                {
                    var so = new SerializedObject(hazard);
                    so.FindProperty("response").enumValueIndex = (int)HazardResponse.ContactDamage;
                    so.FindProperty("damage").intValue = 1;
                    so.FindProperty("damageInterval").floatValue = 0.18f;
                    so.FindProperty("useTrigger").boolValue = true;
                    so.FindProperty("playerTag").stringValue = "Player";
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                var col = root.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    // Trigger only — never solid ground for the player/enemies.
                    col.isTrigger = true;
                    col.enabled = true;
                    // Slightly taller hitbox so tips register cleanly.
                    if (col.size.y < 1.05f)
                    {
                        col.size = new Vector2(Mathf.Max(0.9f, col.size.x), 1.1f);
                    }

                    col.offset = new Vector2(0f, 0.05f);
                }

                var layer = LayerMask.NameToLayer("Hazard");
                if (layer >= 0)
                {
                    root.layer = layer;
                }

                root.tag = "Hazard";
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void WireButtonsInScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null || string.IsNullOrEmpty(button.gameObject.scene.name))
                {
                    continue;
                }

                if (button.GetComponent<UiButtonPunch>() == null)
                {
                    button.gameObject.AddComponent<UiButtonPunch>();
                    EditorUtility.SetDirty(button.gameObject);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
