// Filename: PlayerDeath.cs
// Folder: Assets/Scripts/Player/
// Purpose: Player death state, control disable, and optional Game Over transition (Phase 5/11).
// Dependencies: PlayerController, BounderTrail.Core.GameLog, GameStateManager

using System.Collections;
using BounderTrail.Core;
using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Tracks whether Pip is dead and disables movement when death occurs.
    /// Damage is handled by PlayerHealth; this component owns the death outcome.
    /// </summary>
    public class PlayerDeath : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Rigidbody2D rigidBody;

        [Header("Game Over")]
        [Tooltip("When false, RespawnSystem handles post-death (respawn / Game Over).")]
        [SerializeField] private bool triggerGameOverOnDeath = false;
        [SerializeField] private float gameOverDelay = 0.85f;

        [Header("Debug")]
        [SerializeField] private bool enableDebugKillKey = true;
        [SerializeField] private KeyCode debugKillKey = KeyCode.K;

        public bool IsDead { get; private set; }
        public bool IsAlive => !IsDead;

        public event System.Action Died;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody2D>();
            }

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            enableDebugKillKey = false;
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            if (!enableDebugKillKey || IsDead)
            {
                return;
            }

            if (Input.GetKeyDown(debugKillKey))
            {
                Die();
            }
        }
#endif

        /// <summary>
        /// Enters death state. Safe to call multiple times.
        /// </summary>
        public void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;

            if (playerController != null)
            {
                playerController.enabled = false;
            }

            if (rigidBody != null)
            {
                rigidBody.linearVelocity = Vector2.zero;
            }

            GameLog.Info("Player", "Death state entered.");
            Died?.Invoke();

            if (triggerGameOverOnDeath)
            {
                StartCoroutine(TriggerGameOverAfterDelay());
            }
        }

        /// <summary>
        /// When a RespawnSystem is present it should disable Game Over-on-death.
        /// </summary>
        public void SetGameOverOnDeath(bool enabled)
        {
            triggerGameOverOnDeath = enabled;
        }

        /// <summary>
        /// Clears death state (used by later respawn systems).
        /// </summary>
        public void ClearDeathState()
        {
            IsDead = false;

            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }

        private IEnumerator TriggerGameOverAfterDelay()
        {
            if (gameOverDelay > 0f)
            {
                yield return new WaitForSeconds(gameOverDelay);
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.TriggerGameOver();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            gameOverDelay = Mathf.Max(0f, gameOverDelay);
        }
#endif
    }
}
