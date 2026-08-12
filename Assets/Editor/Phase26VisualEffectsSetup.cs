// Filename: Phase26VisualEffectsSetup.cs
// Folder: Assets/Editor/
// Purpose: Wires camera shake + player/enemy/world visual juice (Phase 26).
// Menu: Bounder Trail/Phase 26/Setup Visual Effects And Juice
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase26VisualEffectsSetup.SetupVisualEffectsAndJuice

#if UNITY_EDITOR
using BounderTrail.CameraSystem;
using BounderTrail.Core;
using BounderTrail.Enemies;
using BounderTrail.Player;
using BounderTrail.Vfx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase26VisualEffectsSetup
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player_Pip.prefab";
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string DustPath = "Assets/Art/VFX/FX_Dust.png";
        private const string SparklePath = "Assets/Art/VFX/FX_Sparkle.png";
        private const string HitRingPath = "Assets/Art/VFX/FX_HitRing.png";

        private static readonly string[] EnemyPrefabs =
        {
            "Assets/Prefabs/Enemies/Enemy_Crawlbug.prefab",
            "Assets/Prefabs/Enemies/Enemy_Dartling.prefab",
            "Assets/Prefabs/Enemies/Enemy_Hopmite.prefab",
            "Assets/Prefabs/Enemies/Enemy_Skimmer.prefab",
            "Assets/Prefabs/Enemies/Enemy_Spikewatch.prefab",
            "Assets/Prefabs/Enemies/Enemy_Spitter.prefab"
        };

        private static readonly string[] LevelScenes =
        {
            "Assets/Scenes/Gameplay.unity",
            $"Assets/Scenes/{ProjectConstants.Level01SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level02SceneName}.unity",
            $"Assets/Scenes/{ProjectConstants.Level03SceneName}.unity"
        };

        [MenuItem("Bounder Trail/Phase 26/Setup Visual Effects And Juice")]
        public static void SetupVisualEffectsAndJuice()
        {
            var dust = LoadSprite(DustPath);
            var sparkle = LoadSprite(SparklePath);
            var hitRing = LoadSprite(HitRingPath);
            if (dust == null || sparkle == null || hitRing == null)
            {
                Debug.LogError(
                    $"{GameLog.ProjectPrefix}[Setup] Phase 26 aborted — run Phase 25 art polish first (missing VFX sprites).");
                return;
            }

            WirePlayerJuice(dust, sparkle, hitRing);
            WireEnemyJuice(dust, sparkle);
            WireBootstrapJuice(sparkle, hitRing);
            WireCamerasInScenes();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"{GameLog.ProjectPrefix}[Setup] Phase 26 visual juice ready " +
                "(shake + bursts; no ParticleSystems; gameplay logic unchanged).");
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

            Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Missing sprite: {path}");
            return null;
        }

        private static void WirePlayerJuice(Sprite dust, Sprite sparkle, Sprite hitRing)
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var juice = root.GetComponent<PlayerVisualJuice>();
                if (juice == null)
                {
                    juice = root.AddComponent<PlayerVisualJuice>();
                }

                var so = new SerializedObject(juice);
                so.FindProperty("playerController").objectReferenceValue = root.GetComponent<PlayerController>();
                so.FindProperty("playerHealth").objectReferenceValue = root.GetComponent<PlayerHealth>();
                so.FindProperty("playerDeath").objectReferenceValue = root.GetComponent<PlayerDeath>();
                so.FindProperty("playerPowerUps").objectReferenceValue = root.GetComponent<PlayerPowerUps>();
                so.FindProperty("dustSprite").objectReferenceValue = dust;
                so.FindProperty("hitRingSprite").objectReferenceValue = hitRing;
                so.FindProperty("sparkleSprite").objectReferenceValue = sparkle;
                so.FindProperty("enableJumpPuff").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            for (var i = 0; i < LevelScenes.Length; i++)
            {
                PatchPlayerInScene(LevelScenes[i]);
            }
        }

        private static void PatchPlayerInScene(string scenePath)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var player = GameObject.Find("Player_Pip");
            if (player != null && PrefabUtility.IsPartOfPrefabInstance(player))
            {
                PrefabUtility.RevertPrefabInstance(player, InteractionMode.AutomatedAction);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void WireEnemyJuice(Sprite dust, Sprite sparkle)
        {
            for (var i = 0; i < EnemyPrefabs.Length; i++)
            {
                var path = EnemyPrefabs[i];
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var juice = root.GetComponent<EnemyDefeatVisualJuice>();
                    if (juice == null)
                    {
                        juice = root.AddComponent<EnemyDefeatVisualJuice>();
                    }

                    var so = new SerializedObject(juice);
                    so.FindProperty("health").objectReferenceValue = root.GetComponent<EnemyHealth>();
                    so.FindProperty("dustSprite").objectReferenceValue = dust;
                    so.FindProperty("sparkleSprite").objectReferenceValue = sparkle;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void WireBootstrapJuice(Sprite sparkle, Sprite hitRing)
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var bootstrap = GameObject.Find(ProjectConstants.BootstrapObjectName);
            if (bootstrap == null)
            {
                Debug.LogError($"{GameLog.ProjectPrefix}[Setup] Bootstrap missing.");
                return;
            }

            var juice = bootstrap.GetComponent<GameplayVisualJuice>();
            if (juice == null)
            {
                juice = bootstrap.AddComponent<GameplayVisualJuice>();
            }

            var so = new SerializedObject(juice);
            so.FindProperty("sparkleSprite").objectReferenceValue = sparkle;
            so.FindProperty("hitRingSprite").objectReferenceValue = hitRing;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(juice);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void WireCamerasInScenes()
        {
            for (var i = 0; i < LevelScenes.Length; i++)
            {
                WireCameraInScene(LevelScenes[i]);
            }
        }

        private static void WireCameraInScene(string scenePath)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var follow = Object.FindAnyObjectByType<CameraFollow2D>();
            if (follow == null)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    follow = cam.GetComponent<CameraFollow2D>();
                }
            }

            if (follow == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] No CameraFollow2D in {scenePath}");
                return;
            }

            var shake = follow.GetComponent<CameraShake2D>();
            if (shake == null)
            {
                shake = follow.gameObject.AddComponent<CameraShake2D>();
            }

            var so = new SerializedObject(follow);
            so.FindProperty("cameraShake").objectReferenceValue = shake;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(follow);
            EditorUtility.SetDirty(shake);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
        }
    }
}
#endif
