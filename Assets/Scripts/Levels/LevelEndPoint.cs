// Filename: LevelEndPoint.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Marks the level end/goal and notifies LevelCompletionService (Phase 7/16).
// Dependencies: LevelCompletionService, BounderTrail.Core.GameStateManager

using BounderTrail.Core;
using UnityEngine;

namespace BounderTrail.Levels
{
    /// <summary>
    /// End-of-level goal marker. Completes the level on player contact.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelEndPoint : MonoBehaviour
    {
        [SerializeField] private bool completeLevelOnEnter = true;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private Color gizmoColor = new Color(0.3f, 1f, 0.45f, 0.9f);

        public Vector3 EndPosition => transform.position;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!completeLevelOnEnter || other == null || !other.CompareTag(playerTag))
            {
                return;
            }

            if (LevelCompletionService.Instance != null)
            {
                LevelCompletionService.Instance.NotifyGoalReached(this);
                return;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.TriggerLevelComplete();
            }
            else
            {
                GameLog.Warning("Level", "LevelEndPoint reached but no completion service/GameStateManager.");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!completeLevelOnEnter || other == null || !other.CompareTag(playerTag))
            {
                return;
            }

            LevelCompletionService.Instance?.NotifyGoalExited(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, new Vector3(0.8f, 1.2f, 0.1f));
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.4f);
        }
    }
}
