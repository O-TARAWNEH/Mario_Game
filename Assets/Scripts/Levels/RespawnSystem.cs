// Filename: RespawnSystem.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Checkpoint tracking, death → respawn flow, and level reset rules (Phase 15).
// Dependencies: Checkpoint, Player*, EnemyRespawnState, MovingHazard, GameStateManager, GameLog

using System.Collections;
using BounderTrail.CameraSystem;
using BounderTrail.Core;
using BounderTrail.Enemies;
using BounderTrail.Player;
using BounderTrail.World;
using UnityEngine;

namespace BounderTrail.Levels
{
    /// <summary>
    /// Owns mid-level recovery:
    /// death → lose a life → respawn at last checkpoint (or level start) → reset enemies/hazards.
    /// </summary>
    public class RespawnSystem : MonoBehaviour
    {
        public static RespawnSystem Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerSpawnPoint levelStartSpawn;
        [SerializeField] private Transform player;
        [SerializeField] private CameraFollow2D cameraFollow;

        [Header("Lives")]
        [SerializeField] private int startingLives = 3;

        [Header("Timing")]
        [SerializeField] private float respawnDelay = 0.85f;

        [Header("Reset Rules")]
        [SerializeField] private bool resetEnemiesOnRespawn = true;
        [SerializeField] private bool resetMovingHazardsOnRespawn = true;
        [Tooltip("Collected coins/power-ups stay collected. Counters stay.")]
        [SerializeField] private bool keepCollectedPickups = true;

        private Checkpoint _activeCheckpoint;
        private int _lives;
        private bool _isRespawning;
        private PlayerDeath _playerDeath;
        private PlayerHealth _playerHealth;
        private PlayerPowerUps _playerPowerUps;
        private PlayerController _playerController;
        private PlayerAnimator _playerAnimator;
        private Rigidbody2D _playerBody;

        public Checkpoint ActiveCheckpoint => _activeCheckpoint;
        public int LivesRemaining => _lives;
        public int StartingLives => startingLives;
        public bool KeepCollectedPickups => keepCollectedPickups;

        public event System.Action<int> LivesChanged;
        public event System.Action Respawned;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLog.Warning("Level", "Duplicate RespawnSystem destroyed.");
                Destroy(this);
                return;
            }

            Instance = this;
            _lives = Mathf.Max(1, startingLives);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            ResolveReferences();
            BindPlayerDeath();
            ResetLives();

            if (levelStartSpawn == null)
            {
                levelStartSpawn = FindFirstObjectByType<PlayerSpawnPoint>();
            }
        }

        private void OnDisable()
        {
            if (_playerDeath != null)
            {
                _playerDeath.Died -= OnPlayerDied;
            }
        }

        public void RegisterCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == null)
            {
                return;
            }

            if (_activeCheckpoint != null && _activeCheckpoint != checkpoint)
            {
                _activeCheckpoint.SetReached(true); // stay visually reached
            }

            _activeCheckpoint = checkpoint;
            checkpoint.SetReached(true);
        }

        public Vector3 GetRespawnPosition()
        {
            if (_activeCheckpoint != null)
            {
                return _activeCheckpoint.RespawnPosition;
            }

            if (levelStartSpawn != null)
            {
                return levelStartSpawn.SpawnPosition;
            }

            return player != null ? player.position : Vector3.zero;
        }

        public void ResetLives()
        {
            _lives = Mathf.Max(1, startingLives);
            LivesChanged?.Invoke(_lives);
        }

        /// <summary>
        /// Grants one extra retry (coin milestone / reward). Caps at a sane upper bound.
        /// </summary>
        public void GrantBonusLife()
        {
            const int maxLives = 99;
            if (_lives >= maxLives)
            {
                return;
            }

            _lives++;
            LivesChanged?.Invoke(_lives);
            GameLog.Info("Level", $"Bonus life granted. Lives remaining: {_lives}.");
        }

        private void OnPlayerDied()
        {
            if (_isRespawning)
            {
                return;
            }

            StartCoroutine(HandleDeathRoutine());
        }

        private IEnumerator HandleDeathRoutine()
        {
            _isRespawning = true;
            _lives = Mathf.Max(0, _lives - 1);
            LivesChanged?.Invoke(_lives);
            GameLog.Info("Level", $"Player died. Lives remaining: {_lives}.");

            if (respawnDelay > 0f)
            {
                yield return new WaitForSeconds(respawnDelay);
            }

            if (_lives <= 0)
            {
                GameLog.Info("Level", "No lives left — Game Over.");
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.TriggerGameOver();
                }

                _isRespawning = false;
                yield break;
            }

            PerformRespawn();
            _isRespawning = false;
        }

        private void PerformRespawn()
        {
            ResolveReferences();

            // --- Player reset ---
            if (player != null)
            {
                player.position = GetRespawnPosition();
            }

            if (_playerBody != null)
            {
                _playerBody.linearVelocity = Vector2.zero;
                _playerBody.angularVelocity = 0f;
            }

            if (_playerDeath != null)
            {
                _playerDeath.ClearDeathState();
            }

            if (_playerHealth != null)
            {
                _playerHealth.RefillToMax();
            }

            if (_playerPowerUps != null)
            {
                _playerPowerUps.ClearAllPowerUps();
            }

            if (_playerAnimator != null)
            {
                _playerAnimator.ResetAfterRespawn();
            }

            if (_playerController != null)
            {
                _playerController.enabled = true;
                _playerController.LockControl(0f);
                if (ActiveCheckpoint != null)
                {
                    _playerController.SetFacing(ActiveCheckpoint.FaceRightOnRespawn);
                }
            }

            // --- World reset ---
            if (resetEnemiesOnRespawn)
            {
                ResetEnemies();
            }

            if (resetMovingHazardsOnRespawn)
            {
                ResetMovingHazards();
            }

            // Collectibles / score / activated checkpoints intentionally NOT reset.

            if (cameraFollow != null)
            {
                cameraFollow.SnapToTarget();
            }

            Respawned?.Invoke();
            GameLog.Info("Level", $"Respawned at {GetRespawnPosition()}.");
        }

        private void ResetEnemies()
        {
            var states = FindObjectsByType<EnemyRespawnState>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < states.Length; i++)
            {
                if (states[i] != null)
                {
                    states[i].ResetEnemy();
                }
            }
        }

        private void ResetMovingHazards()
        {
            var movers = FindObjectsByType<MovingHazard>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < movers.Length; i++)
            {
                if (movers[i] != null)
                {
                    movers[i].ResetMovingHazard();
                }
            }
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject.transform;
                }
            }

            if (player == null)
            {
                return;
            }

            if (_playerDeath == null)
            {
                _playerDeath = player.GetComponent<PlayerDeath>();
            }

            if (_playerHealth == null)
            {
                _playerHealth = player.GetComponent<PlayerHealth>();
            }

            if (_playerPowerUps == null)
            {
                _playerPowerUps = player.GetComponent<PlayerPowerUps>();
            }

            if (_playerController == null)
            {
                _playerController = player.GetComponent<PlayerController>();
            }

            if (_playerAnimator == null)
            {
                _playerAnimator = player.GetComponent<PlayerAnimator>();
            }

            if (_playerBody == null)
            {
                _playerBody = player.GetComponent<Rigidbody2D>();
            }

            if (cameraFollow == null)
            {
                cameraFollow = FindFirstObjectByType<CameraFollow2D>();
            }
        }

        private void BindPlayerDeath()
        {
            if (_playerDeath == null && player != null)
            {
                _playerDeath = player.GetComponent<PlayerDeath>();
            }

            if (_playerDeath == null)
            {
                return;
            }

            _playerDeath.Died -= OnPlayerDied;
            _playerDeath.Died += OnPlayerDied;

            // RespawnSystem owns post-death outcome (respawn or Game Over).
            var soDeath = _playerDeath;
            soDeath.SetGameOverOnDeath(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            startingLives = Mathf.Max(1, startingLives);
            respawnDelay = Mathf.Max(0f, respawnDelay);
        }
#endif
    }
}
