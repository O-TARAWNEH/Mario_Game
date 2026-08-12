// Filename: Phase8EnvironmentSetup.cs
// Folder: Assets/Editor/
// Purpose: Creates reusable environmental prefabs and sample placements (Phase 8).
// Menu: Bounder Trail/Phase 8/Setup Environmental Objects
// Batchmode: -executeMethod BounderTrail.EditorTools.Phase8EnvironmentSetup.SetupEnvironmentalObjects

#if UNITY_EDITOR
using BounderTrail.Core;
using BounderTrail.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BounderTrail.EditorTools
{
    public static class Phase8EnvironmentSetup
    {
        private const string PrefabFolder = "Assets/Prefabs/World";
        private const string ArtWorld = "Assets/Art/World";
        private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Bounder Trail/Phase 8/Setup Environmental Objects")]
        public static void SetupEnvironmentalObjects()
        {
            EnsureFolder("Assets/Prefabs", "World");
            EnsureFolder("Assets/Art", "World");

            var solidSprite = CreateColorSprite($"{ArtWorld}/Env_Solid.png", new Color(0.35f, 0.7f, 0.3f), 64, 16);
            var oneWaySprite = CreateColorSprite($"{ArtWorld}/Env_OneWay.png", new Color(0.45f, 0.85f, 0.55f), 64, 12);
            var movingSprite = CreateColorSprite($"{ArtWorld}/Env_Moving.png", new Color(0.3f, 0.65f, 0.85f), 64, 14);
            var bounceSprite = CreateColorSprite($"{ArtWorld}/Env_Bounce.png", new Color(0.95f, 0.55f, 0.2f), 48, 16);
            var exitSprite = CreateColorSprite($"{ArtWorld}/Env_Exit.png", new Color(0.25f, 0.95f, 0.55f), 24, 40);

            var solidPrefab = CreateSolidPrefab($"{PrefabFolder}/Platform_Solid.prefab", solidSprite);
            var oneWay = CreateOneWayPrefab($"{PrefabFolder}/Platform_OneWay.prefab", oneWaySprite);
            var moving = CreateMovingPrefab($"{PrefabFolder}/Platform_Moving.prefab", movingSprite);
            var bounce = CreateBouncePrefab($"{PrefabFolder}/BouncePad.prefab", bounceSprite);
            var exit = CreateExitPrefab($"{PrefabFolder}/LevelExitDoor.prefab", exitSprite);

            // Keep legacy prefab names as solid platforms for older references.
            ReplaceLegacyPrefab($"{PrefabFolder}/Platform_Basic.prefab", solidPrefab);
            ReplaceLegacyPrefab($"{PrefabFolder}/Ground_Basic.prefab", solidPrefab);

            PlaceSamplesInGameplay(solidPrefab, oneWay, moving, bounce, exit);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{GameLog.ProjectPrefix}[Setup] Phase 8 environmental objects ready.");
        }

        private static void ReplaceLegacyPrefab(string path, GameObject sourcePrefab)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }

        private static GameObject CreateSolidPrefab(string path, Sprite sprite)
        {
            var go = CreateBasePlatform("Platform_Solid", sprite, new Vector3(3f, 0.6f, 1f));
            go.AddComponent<PlatformPiece>().SetKind(PlatformPiece.PlatformKind.Solid);
            go.AddComponent<SolidPlatform>();
            return SavePrefab(go, path);
        }

        private static GameObject CreateOneWayPrefab(string path, Sprite sprite)
        {
            var go = CreateBasePlatform("Platform_OneWay", sprite, new Vector3(3f, 0.4f, 1f));
            go.AddComponent<PlatformPiece>().SetKind(PlatformPiece.PlatformKind.OneWay);
            go.AddComponent<PlatformEffector2D>();
            go.AddComponent<OneWayPlatform>();
            return SavePrefab(go, path);
        }

        private static GameObject CreateMovingPrefab(string path, Sprite sprite)
        {
            var go = CreateBasePlatform("Platform_Moving", sprite, new Vector3(2.5f, 0.5f, 1f));
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            go.AddComponent<PlatformPiece>().SetKind(PlatformPiece.PlatformKind.Moving);
            var moving = go.AddComponent<MovingPlatform>();
            var so = new SerializedObject(moving);
            so.FindProperty("pointA").vector2Value = new Vector2(-2f, 0f);
            so.FindProperty("pointB").vector2Value = new Vector2(2f, 0f);
            so.FindProperty("pointsAreLocal").boolValue = true;
            so.FindProperty("speed").floatValue = 2.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return SavePrefab(go, path);
        }

        private static GameObject CreateBouncePrefab(string path, Sprite sprite)
        {
            var go = CreateBasePlatform("BouncePad", sprite, new Vector3(1.4f, 0.45f, 1f));
            go.AddComponent<BouncePad>();
            var sr = go.GetComponent<SpriteRenderer>();
            sr.color = Color.white;
            return SavePrefab(go, path);
        }

        private static GameObject CreateExitPrefab(string path, Sprite sprite)
        {
            var go = new GameObject("LevelExitDoor");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 6;
            go.transform.localScale = new Vector3(1f, 1f, 1f);

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(0.9f, 1.6f);
            go.AddComponent<LevelExitDoor>();
            return SavePrefab(go, path);
        }

        private static GameObject CreateBasePlatform(string name, Sprite sprite, Vector3 scale)
        {
            var go = new GameObject(name);
            go.tag = "Ground";
            go.layer = LayerMask.NameToLayer("Ground");
            go.transform.localScale = scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 1;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;
            return go;
        }

        private static GameObject SavePrefab(GameObject go, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void PlaceSamplesInGameplay(
            GameObject solid,
            GameObject oneWay,
            GameObject moving,
            GameObject bounce,
            GameObject exit)
        {
            var scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            var platforms = GameObject.Find("Platforms");
            if (platforms == null)
            {
                var root = GameObject.Find("LevelRoot");
                if (root != null)
                {
                    platforms = new GameObject("Platforms");
                    platforms.transform.SetParent(root.transform, false);
                }
            }

            if (platforms == null)
            {
                Debug.LogWarning($"{GameLog.ProjectPrefix}[Setup] No Platforms folder found in Gameplay.");
                return;
            }

            PlaceIfMissing(platforms.transform, "Sample_OneWay", oneWay, new Vector3(-0.5f, 2.2f, 0f));
            PlaceIfMissing(platforms.transform, "Sample_Moving", moving, new Vector3(5.5f, -0.8f, 0f));
            PlaceIfMissing(platforms.transform, "Sample_Bounce", bounce, new Vector3(-3.5f, -2.55f, 0f));

            // Prefer reusable exit prefab near existing end area.
            var existingExit = GameObject.Find("Sample_ExitDoor");
            if (existingExit == null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(exit);
                instance.name = "Sample_ExitDoor";
                var parent = GameObject.Find("LevelRoot");
                if (parent != null)
                {
                    instance.transform.SetParent(parent.transform, true);
                }

                instance.transform.position = new Vector3(9.2f, 3.4f, 0f);
            }

            // Ensure at least one clearly solid sample exists.
            PlaceIfMissing(platforms.transform, "Sample_Solid", solid, new Vector3(-6.5f, 0.8f, 0f));

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
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
