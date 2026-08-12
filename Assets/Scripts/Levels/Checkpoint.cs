// Filename: Checkpoint.cs
// Folder: Assets/Scripts/Levels/
// Purpose: Placeable checkpoint trigger that stores a respawn pose (Phase 15/31).
// Dependencies: RespawnSystem, BounderTrail.Core.GameLog, AudioManager, SimpleBurstVfx

using BounderTrail.Audio;
using BounderTrail.Core;
using BounderTrail.Vfx;
using UnityEngine;

namespace BounderTrail.Levels
{
    /// <summary>
    /// When the player enters, this becomes the active respawn point.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Respawn")]
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private bool faceRightOnRespawn = true;

        [Header("Detection")]
        [SerializeField] private string playerTag = "Player";

        [Header("Visual")]
        [SerializeField] private SpriteRenderer flagRenderer;
        [SerializeField] private Color inactiveColor = new Color(0.7f, 0.75f, 0.85f, 1f);
        [SerializeField] private Color activeColor = new Color(0.35f, 1f, 0.55f, 1f);
        [SerializeField] private Sprite activateBurstSprite;

        private Collider2D _collider;
        private bool _reached;

        public bool IsReached => _reached;
        public bool FaceRightOnRespawn => faceRightOnRespawn;
        public Vector3 RespawnPosition => respawnPoint != null ? respawnPoint.position : transform.position;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }

            if (respawnPoint == null)
            {
                respawnPoint = transform;
            }

            if (flagRenderer == null)
            {
                flagRenderer = GetComponent<SpriteRenderer>();
            }

            ApplyVisual(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || !other.CompareTag(playerTag))
            {
                return;
            }

            Activate();
        }

        public void Activate()
        {
            var firstReach = !_reached;

            if (RespawnSystem.Instance != null)
            {
                RespawnSystem.Instance.RegisterCheckpoint(this);
            }
            else
            {
                SetReached(true);
            }

            if (firstReach)
            {
                PlayFirstReachFeedback();
            }

            GameLog.Info("Level", $"Checkpoint '{name}' activated.");
        }

        public void SetReached(bool reached)
        {
            _reached = reached;
            ApplyVisual(reached);
        }

        private void PlayFirstReachFeedback()
        {
            AudioManager.PlaySfx(SfxId.Ui);

            var sprite = activateBurstSprite;
            if (sprite == null && flagRenderer != null)
            {
                sprite = flagRenderer.sprite;
            }

            if (sprite != null)
            {
                SimpleBurstVfx.Spawn(
                    sprite,
                    transform.position + Vector3.up * 0.35f,
                    new Color(0.45f, 1f, 0.65f, 0.9f),
                    0.22f,
                    0.3f,
                    0.95f,
                    26);
            }
        }

        private void ApplyVisual(bool active)
        {
            if (flagRenderer != null)
            {
                flagRenderer.color = active ? activeColor : inactiveColor;
            }
        }

        private void OnDrawGizmos()
        {
            var pos = respawnPoint != null ? respawnPoint.position : transform.position;
            Gizmos.color = new Color(0.3f, 1f, 0.5f, 0.85f);
            Gizmos.DrawWireSphere(pos, 0.3f);
            Gizmos.DrawLine(pos, pos + Vector3.up * 0.7f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
#endif
    }
}
