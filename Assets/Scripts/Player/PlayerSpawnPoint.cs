// Filename: PlayerSpawnPoint.cs
// Folder: Assets/Scripts/Player/
// Purpose: Marks where the player should appear in a level (Phase 3).
// Dependencies: None.

using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Level spawn marker for Pip. Place in the Gameplay scene.
    /// </summary>
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.9f, 1f, 0.85f);

        public Vector3 SpawnPosition => transform.position;

        /// <summary>
        /// Moves a player transform to this spawn point.
        /// </summary>
        public void PlacePlayer(Transform player)
        {
            if (player == null)
            {
                return;
            }

            player.position = SpawnPosition;

            var body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, 0.35f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.8f);
        }
    }
}
