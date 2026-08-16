// Filename: PlayerSquashStretch.cs
// Folder: Assets/Scripts/Player/
// Purpose: Brief squash/stretch on jump and land (Phase 38).
// Dependencies: PlayerController

using System.Collections;
using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Visual-only scale punch. Does not change physics or collision size.
    /// </summary>
    public class PlayerSquashStretch : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float jumpStretchY = 1.18f;
        [SerializeField] private float jumpStretchX = 0.88f;
        [SerializeField] private float landSquashY = 0.78f;
        [SerializeField] private float landSquashX = 1.16f;
        [SerializeField] private float recoverDuration = 0.12f;

        private Vector3 _baseScale = Vector3.one;
        private Coroutine _routine;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (visualRoot == null)
            {
                var sr = GetComponentInChildren<SpriteRenderer>();
                visualRoot = sr != null ? sr.transform : transform;
            }

            // Never scale the physics body — that shrinks colliders and breaks pickups.
            if (visualRoot == transform || visualRoot.GetComponent<Rigidbody2D>() != null)
            {
                enabled = false;
                return;
            }

            _baseScale = visualRoot.localScale;
        }

        private void OnEnable()
        {
            if (playerController != null)
            {
                playerController.Jumped += OnJumped;
                playerController.Landed += OnLanded;
            }
        }

        private void OnDisable()
        {
            if (playerController != null)
            {
                playerController.Jumped -= OnJumped;
                playerController.Landed -= OnLanded;
            }

            if (visualRoot != null)
            {
                visualRoot.localScale = _baseScale;
            }
        }

        private void OnJumped()
        {
            Play(new Vector3(_baseScale.x * jumpStretchX, _baseScale.y * jumpStretchY, _baseScale.z));
        }

        private void OnLanded()
        {
            var impact = playerController != null ? playerController.LastLandingSpeed : 6f;
            if (impact < 3.5f)
            {
                // Tiny hops don't need squash.
                return;
            }

            var t = Mathf.InverseLerp(3.5f, 16f, impact);
            var squashY = Mathf.Lerp(0.9f, landSquashY, t);
            var squashX = Mathf.Lerp(1.05f, landSquashX, t);
            Play(new Vector3(_baseScale.x * squashX, _baseScale.y * squashY, _baseScale.z));
        }

        private void Play(Vector3 punched)
        {
            if (visualRoot == null)
            {
                return;
            }

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(RecoverRoutine(punched));
        }

        private IEnumerator RecoverRoutine(Vector3 punched)
        {
            visualRoot.localScale = punched;
            var duration = Mathf.Max(0.05f, recoverDuration);
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var u = Mathf.Clamp01(elapsed / duration);
                var ease = 1f - (1f - u) * (1f - u);
                visualRoot.localScale = Vector3.Lerp(punched, _baseScale, ease);
                yield return null;
            }

            visualRoot.localScale = _baseScale;
            _routine = null;
        }
    }
}
