// Filename: LevelPhysicsSanitizer.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Strips stray tilemap colliders and hardens platform / player physics at runtime.
// Dependencies: LevelRoot, SolidPlatform, PlatformPiece, MovingPlatform

using BounderTrail.Core;
using BounderTrail.Player;
using BounderTrail.World;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BounderTrail.Levels
{
    /// <summary>
    /// Removes invisible collision left by legacy tilemaps, duplicate landing solids,
    /// and re-asserts solid Ground collision so characters cannot float through platforms.
    /// </summary>
    public static class LevelPhysicsSanitizer
    {
        private const string GroundLayerName = "Ground";
        private const string GroundTag = "Ground";

        public static void Sanitize(LevelRoot root)
        {
            if (root == null)
            {
                return;
            }

            StripTilemapPhysics(root.TilemapRoot);
            RemoveDuplicateLandingSolids(root.PlatformsRoot);
            HardenSolidPlatforms(root.PlatformsRoot);
            HardenPlayerPhysics();
            // Do not rewrite platform positions/sizes at runtime — that desyncs coins,
            // checkpoints, and power-ups from the authored scene layout (Level 3 bug).
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

        private static void HardenSolidPlatforms(Transform platformsRoot)
        {
            if (platformsRoot == null)
            {
                return;
            }

            var groundLayer = LayerMask.NameToLayer(GroundLayerName);
            if (groundLayer < 0)
            {
                return;
            }

            var solids = platformsRoot.GetComponentsInChildren<SolidPlatform>(true);
            var fixedCount = 0;
            for (var i = 0; i < solids.Length; i++)
            {
                var solid = solids[i];
                if (solid == null)
                {
                    continue;
                }

                var go = solid.gameObject;
                if (go.layer != groundLayer)
                {
                    go.layer = groundLayer;
                    fixedCount++;
                }

                if (!go.CompareTag(GroundTag))
                {
                    go.tag = GroundTag;
                }

                var col = go.GetComponent<Collider2D>();
                if (col == null)
                {
                    continue;
                }

                // Timed platforms manage their own enable cycle; only force solid contact.
                if (go.GetComponent<TimedPlatform>() == null)
                {
                    col.enabled = true;
                }

                col.isTrigger = false;
            }

            if (fixedCount > 0)
            {
                GameLog.Info("Level", $"Hardened {fixedCount} solid platform(s) onto Ground layer.");
            }
        }

        private static void HardenPlayerPhysics()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            var body = player.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                return;
            }

            body.bodyType = RigidbodyType2D.Dynamic;
            body.simulated = true;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var controller = player.GetComponent<PlayerController>();
            if (controller == null && body.gravityScale < 0.1f)
            {
                body.gravityScale = 3.2f;
            }
        }
    }
}
