// Filename: BouncePad.cs
// Folder: Assets/Scripts/World/
// Purpose: Spring/bounce pad that launches the player upward (Phase 8/31).
// Dependencies: BounderTrail.Audio

using BounderTrail.Audio;
using System.Collections;
using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Bounces the player when landed on from above.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BouncePad : MonoBehaviour
    {
        [SerializeField] private float bounceForce = 18f;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float cooldown = 0.05f;
        [SerializeField] private float squashScaleY = 0.72f;
        [SerializeField] private float squashDuration = 0.12f;

        private float _nextBounceTime;
        private Vector3 _baseScale;
        private Coroutine _squashRoutine;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryBounce(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Helps if the first contact frame was ambiguous.
            TryBounce(collision);
        }

        private void TryBounce(Collision2D collision)
        {
            if (Time.time < _nextBounceTime)
            {
                return;
            }

            if (collision.collider == null || !collision.collider.CompareTag(playerTag))
            {
                return;
            }

            if (!IsHitFromAbove(collision))
            {
                return;
            }

            var body = collision.rigidbody;
            if (body == null)
            {
                return;
            }

            var velocity = body.linearVelocity;
            velocity.y = bounceForce;
            body.linearVelocity = velocity;
            _nextBounceTime = Time.time + cooldown;

            AudioManager.PlaySfx(SfxId.Jump);
            PlaySquash();
        }

        private void PlaySquash()
        {
            if (_squashRoutine != null)
            {
                StopCoroutine(_squashRoutine);
            }

            _squashRoutine = StartCoroutine(SquashRoutine());
        }

        private IEnumerator SquashRoutine()
        {
            var duration = Mathf.Max(0.05f, squashDuration);
            var elapsed = 0f;
            var squashed = new Vector3(_baseScale.x * 1.08f, _baseScale.y * squashScaleY, _baseScale.z);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var u = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(squashed, _baseScale, u * u);
                yield return null;
            }

            transform.localScale = _baseScale;
            _squashRoutine = null;
        }

        private bool IsHitFromAbove(Collision2D collision)
        {
            return collision.transform.position.y >= transform.position.y + 0.05f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            bounceForce = Mathf.Max(0f, bounceForce);
            cooldown = Mathf.Max(0f, cooldown);
            squashScaleY = Mathf.Clamp(squashScaleY, 0.4f, 1f);
            squashDuration = Mathf.Max(0.05f, squashDuration);
        }
#endif
    }
}
