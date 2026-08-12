// Filename: LevelExitDoor.cs
// Folder: Assets/Scripts/World/
// Purpose: Reusable exit/door goal that notifies LevelCompletionService (Phase 8/16).
// Dependencies: BounderTrail.Levels.LevelCompletionService, BounderTrail.Core.GameStateManager

using BounderTrail.Core;
using BounderTrail.Levels;
using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Placeable exit door/goal. Prefer this prefab for level exits.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelExitDoor : MonoBehaviour
    {
        [SerializeField] private bool completeLevelOnEnter = true;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private Color gizmoColor = new Color(0.25f, 0.95f, 0.55f, 0.95f);

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
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

            // Fallback if Phase 16 service is missing.
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.TriggerLevelComplete();
            }
            else
            {
                GameLog.Warning("World", "LevelExitDoor entered but no completion service/GameStateManager.");
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
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.6f, new Vector3(1f, 1.4f, 0.1f));
        }
    }
}
