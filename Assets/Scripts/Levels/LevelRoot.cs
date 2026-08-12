// Filename: LevelRoot.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Scene-side level structure root (Phase 7).
// Dependencies: LevelBounds, LevelEndPoint, PlayerSpawnPoint, CameraFollow2D, GameLog

using BounderTrail.CameraSystem;
using BounderTrail.Core;
using BounderTrail.Player;
using UnityEngine;

namespace BounderTrail.Levels
{
    /// <summary>
    /// Organizes a level scene: bounds, start/end, and content folders.
    /// Runs basic level initialization when the scene loads.
    /// </summary>
    public class LevelRoot : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string levelId = "gameplay_prototype";
        [SerializeField] private string displayName = "Gameplay Prototype";

        [Header("Core References")]
        [SerializeField] private LevelBounds levelBounds;
        [SerializeField] private PlayerSpawnPoint startPoint;
        [SerializeField] private LevelEndPoint endPoint;

        [Header("Content Roots")]
        [SerializeField] private Transform platformsRoot;
        [SerializeField] private Transform enemiesRoot;
        [SerializeField] private Transform collectiblesRoot;
        [SerializeField] private Transform hazardsRoot;
        [SerializeField] private Transform checkpointsRoot;
        [SerializeField] private Transform decorationsRoot;
        [SerializeField] private Transform tilemapRoot;

        [Header("Startup")]
        [SerializeField] private bool placePlayerAtStart = true;
        [SerializeField] private bool snapCameraOnStart = true;

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public LevelBounds Bounds => levelBounds;
        public PlayerSpawnPoint StartPoint => startPoint;
        public LevelEndPoint EndPoint => endPoint;

        public Transform PlatformsRoot => platformsRoot;
        public Transform EnemiesRoot => enemiesRoot;
        public Transform CollectiblesRoot => collectiblesRoot;
        public Transform HazardsRoot => hazardsRoot;
        public Transform CheckpointsRoot => checkpointsRoot;
        public Transform DecorationsRoot => decorationsRoot;
        public Transform TilemapRoot => tilemapRoot;

        private void Start()
        {
            InitializeLevel();
        }

        public void InitializeLevel()
        {
            LevelPhysicsSanitizer.Sanitize(this);

            if (placePlayerAtStart)
            {
                PlacePlayerAtSpawn();
            }

            if (snapCameraOnStart)
            {
                SnapCamera();
            }

            GameLog.Info("Level", $"Initialized '{displayName}' ({levelId}).");
        }

        public void PlacePlayerAtSpawn()
        {
            if (startPoint == null)
            {
                startPoint = GetComponentInChildren<PlayerSpawnPoint>(true);
            }

            if (startPoint == null)
            {
                GameLog.Warning("Level", "No PlayerSpawnPoint found.");
                return;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                GameLog.Warning("Level", "No Player tagged object found to place.");
                return;
            }

            startPoint.PlacePlayer(player.transform);
        }

        private void SnapCamera()
        {
            var follow = FindFirstObjectByType<CameraFollow2D>();
            if (follow == null)
            {
                return;
            }

            if (levelBounds != null)
            {
                follow.SetLevelBounds(levelBounds);
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                follow.SetTarget(player.transform);
            }

            follow.SnapToTarget();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (levelBounds == null)
            {
                levelBounds = GetComponentInChildren<LevelBounds>(true);
            }

            if (startPoint == null)
            {
                startPoint = GetComponentInChildren<PlayerSpawnPoint>(true);
            }

            if (endPoint == null)
            {
                endPoint = GetComponentInChildren<LevelEndPoint>(true);
            }
        }
#endif
    }
}
