// Filename: LevelPhysicsSanitizer.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Strips stray tilemap colliders and corrects known bad level geometry at runtime.
// Dependencies: LevelRoot, SolidPlatform, MovingPlatform

using BounderTrail.Core;
using BounderTrail.World;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BounderTrail.Levels
{
    /// <summary>
    /// Removes invisible collision left by legacy tilemaps and duplicate landing solids.
    /// </summary>
    public static class LevelPhysicsSanitizer
    {
        public static void Sanitize(LevelRoot root)
        {
            if (root == null)
            {
                return;
            }

            StripTilemapPhysics(root.TilemapRoot);
            RemoveDuplicateLandingSolids(root.PlatformsRoot);
            FixSkybridgePlatforms(root);
            ClampOversizedBridgeColliders(root.PlatformsRoot);
        }

        private static void ClampOversizedBridgeColliders(Transform platformsRoot)
        {
            if (platformsRoot == null)
            {
                return;
            }

            for (var i = 0; i < platformsRoot.childCount; i++)
            {
                var child = platformsRoot.GetChild(i);
                if (child == null || !child.name.StartsWith("Bridge_"))
                {
                    continue;
                }

                var sprite = child.GetComponent<SpriteRenderer>();
                var box = child.GetComponent<BoxCollider2D>();
                if (sprite == null || box == null)
                {
                    continue;
                }

                if (box.size.x > 5f)
                {
                    var height = Mathf.Max(box.size.y, 0.55f);
                    var width = child.name == "Bridge_A" ? 3.5f
                        : child.name == "Bridge_B" ? 3.2f
                        : 3.2f;
                    ApplyPlatformSize(child.gameObject, new Vector2(width, height));
                }
            }
        }

        private static void StripTilemapPhysics(Transform tilemapRoot)
        {
            if (tilemapRoot == null)
            {
                return;
            }

            var tilemapColliders = tilemapRoot.GetComponentsInChildren<TilemapCollider2D>(true);
            for (var i = 0; i < tilemapColliders.Length; i++)
            {
                Object.Destroy(tilemapColliders[i]);
            }

            var compositeColliders = tilemapRoot.GetComponentsInChildren<CompositeCollider2D>(true);
            for (var i = 0; i < compositeColliders.Length; i++)
            {
                Object.Destroy(compositeColliders[i]);
            }

            var rigidbodies = tilemapRoot.GetComponentsInChildren<Rigidbody2D>(true);
            for (var i = 0; i < rigidbodies.Length; i++)
            {
                Object.Destroy(rigidbodies[i]);
            }

            if (tilemapColliders.Length > 0 || compositeColliders.Length > 0)
            {
                GameLog.Info("Level", "Removed legacy tilemap physics colliders.");
            }
        }

        private static void RemoveDuplicateLandingSolids(Transform platformsRoot)
        {
            if (platformsRoot == null)
            {
                return;
            }

            for (var i = platformsRoot.childCount - 1; i >= 0; i--)
            {
                var child = platformsRoot.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name != "Landing_Mover")
                {
                    continue;
                }

                if (child.GetComponent<MovingPlatform>() != null)
                {
                    continue;
                }

                Object.Destroy(child.gameObject);
                GameLog.Info("Level", "Removed invisible Landing_Mover solid under moving platform.");
            }
        }

        private static void FixSkybridgePlatforms(LevelRoot root)
        {
            if (root.LevelId != "level_03")
            {
                return;
            }

            ResizeNamedPlatform(root.PlatformsRoot, "Bridge_A", new Vector2(3.5f, 0.55f), new Vector3(10f, 0.5f, 0f));
            ResizeNamedPlatform(root.PlatformsRoot, "Bridge_B", new Vector2(3.2f, 0.55f), new Vector3(16.5f, 1.6f, 0f));
            ResizeNamedPlatform(root.PlatformsRoot, "Bridge_C", new Vector2(3.2f, 0.55f), new Vector3(33.5f, 3.5f, 0f));

            var mover = FindNamedTransform(root.PlatformsRoot, "Platform_Moving_A");
            if (mover != null)
            {
                mover.localPosition = new Vector3(25f, 2.4f, 0f);
                ApplyPlatformSize(mover.gameObject, new Vector2(4.5f, 0.55f));

                var moving = mover.GetComponent<MovingPlatform>();
                if (moving != null)
                {
                    moving.ConfigurePath(new Vector2(-2.025f, 0f), new Vector2(2.025f, 0f), 2.1f);
                }
            }
        }

        private static Transform FindNamedTransform(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == objectName)
                {
                    return child;
                }
            }

            return null;
        }

        private static void ResizeNamedPlatform(Transform platformsRoot, string objectName, Vector2 size, Vector3 position)
        {
            var platform = FindNamedTransform(platformsRoot, objectName);
            if (platform == null)
            {
                return;
            }

            platform.localPosition = position;
            ApplyPlatformSize(platform.gameObject, size);
        }

        private static void ApplyPlatformSize(GameObject go, Vector2 worldSize)
        {
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
    }
}
