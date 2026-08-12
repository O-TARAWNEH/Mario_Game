// Filename: LevelCompletionService.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Level goal completion authority, transition, and next-level handoff (Phase 16).
// Dependencies: GameStateManager, LevelLoader, PlayerDeath, PlayerController, GameLog

using System.Collections;
using BounderTrail.Audio;
using BounderTrail.Core;
using BounderTrail.Player;
using UnityEngine;

namespace BounderTrail.Levels
{
    /// <summary>
    /// Single authority for finishing a level when the player reaches a goal.
    /// Completion is sticky: leaving the goal trigger does not cancel it.
    /// </summary>
    public class LevelCompletionService : MonoBehaviour
    {
        public static LevelCompletionService Instance { get; private set; }

        [Header("Transition")]
        [SerializeField] private float completionDelay = 0.55f;
        [SerializeField] private bool freezePlayerOnComplete = true;

        [Header("Rules")]
        [SerializeField] private bool requirePlayerAlive = true;

        private bool _completed;
        private bool _transitionStarted;

        public bool IsCompleted => _completed;

        public event System.Action Completed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                GameLog.Warning("Level", "Duplicate LevelCompletionService destroyed.");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Called by goal objects when the player enters the goal.
        /// Safe to call multiple times; only the first valid reach counts.
        /// </summary>
        public void NotifyGoalReached(Component source = null)
        {
            if (_completed)
            {
                return;
            }

            if (GameStateManager.Instance != null
                && GameStateManager.Instance.CurrentState != GameStateId.Gameplay
                && GameStateManager.Instance.CurrentState != GameStateId.Pause)
            {
                return;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            if (requirePlayerAlive)
            {
                var death = player.GetComponent<PlayerDeath>();
                if (death != null && death.IsDead)
                {
                    return;
                }
            }

            _completed = true;
            var sourceName = source != null ? source.name : "Goal";
            GameLog.Info("Level", $"Level goal reached via '{sourceName}'.");

            if (freezePlayerOnComplete)
            {
                FreezePlayer(player);
            }

            AudioManager.PlaySfx(SfxId.LevelComplete);
            Completed?.Invoke();
            StartCoroutine(CompleteTransitionRoutine());
        }

        /// <summary>
        /// Leaving the goal trigger does nothing once completed.
        /// Exposed for clarity / future hooks.
        /// </summary>
        public void NotifyGoalExited(Component source = null)
        {
            // Intentionally empty: completion is sticky for the rest of the level run.
            if (_completed)
            {
                return;
            }
        }

        public void ResetCompletionState()
        {
            _completed = false;
            _transitionStarted = false;
        }

        private IEnumerator CompleteTransitionRoutine()
        {
            if (_transitionStarted)
            {
                yield break;
            }

            _transitionStarted = true;

            if (completionDelay > 0f)
            {
                // Use unscaled wait so a paused timeScale still advances the delay if needed.
                yield return new WaitForSecondsRealtime(completionDelay);
            }

            if (GameStateManager.Instance != null)
            {
                // Ensure gameplay time is restored briefly then Level Complete freezes via TriggerLevelComplete.
                Time.timeScale = 1f;
                GameStateManager.Instance.TriggerLevelComplete();
            }
            else
            {
                GameLog.Warning("Level", "Level completed but GameStateManager is missing.");
            }
        }

        private static void FreezePlayer(GameObject player)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            var body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            completionDelay = Mathf.Max(0f, completionDelay);
        }
#endif
    }
}
