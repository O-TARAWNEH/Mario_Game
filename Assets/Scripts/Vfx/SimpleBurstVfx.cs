// Filename: SimpleBurstVfx.cs
// Folder: Assets/Scripts/Vfx/
// Purpose: Short-lived sprite burst (scale + fade). No ParticleSystem (Phase 26).
// Dependencies: None

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BounderTrail.Vfx
{
    /// <summary>
    /// Spawns a one-shot decorative sprite that scales and fades out.
    /// Hard-capped and pooled (Phase 29) to avoid Instantiate/Destroy GC spikes.
    /// </summary>
    public class SimpleBurstVfx : MonoBehaviour
    {
        private const int MaxAlive = 12;
        private const int MaxPoolSize = 12;

        private static int _alive;
        private static Transform _bucket;
        private static bool _sceneHooked;
        private static readonly List<SimpleBurstVfx> Pool = new List<SimpleBurstVfx>(MaxPoolSize);

        private bool _counted;
        private Coroutine _playRoutine;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float lifetime = 0.28f;
        [SerializeField] private Vector3 startScale = new Vector3(0.35f, 0.35f, 1f);
        [SerializeField] private Vector3 endScale = new Vector3(1.15f, 1.15f, 1f);

        public static void Spawn(
            Sprite sprite,
            Vector3 worldPosition,
            Color color,
            float lifetime = 0.28f,
            float startSize = 0.35f,
            float endSize = 1.1f,
            int sortingOrder = 25)
        {
            EnsureSceneHook();
            if (sprite == null || _alive >= MaxAlive)
            {
                return;
            }

            EnsureBucket();
            var burst = Rent();
            burst.transform.SetParent(_bucket, false);
            burst.transform.position = worldPosition;
            burst.spriteRenderer.sprite = sprite;
            burst.spriteRenderer.color = color;
            burst.spriteRenderer.sortingOrder = sortingOrder;
            burst.lifetime = Mathf.Max(0.05f, lifetime);
            burst.startScale = new Vector3(startSize, startSize, 1f);
            burst.endScale = new Vector3(endSize, endSize, 1f);
            burst.gameObject.SetActive(true);
            burst._playRoutine = burst.StartCoroutine(burst.Play());
        }

        private static SimpleBurstVfx Rent()
        {
            for (var i = Pool.Count - 1; i >= 0; i--)
            {
                var candidate = Pool[i];
                Pool.RemoveAt(i);
                if (candidate != null)
                {
                    return candidate;
                }
            }

            var go = new GameObject("BurstVfx");
            var sr = go.AddComponent<SpriteRenderer>();
            var burst = go.AddComponent<SimpleBurstVfx>();
            burst.spriteRenderer = sr;
            return burst;
        }

        private static void Return(SimpleBurstVfx burst)
        {
            if (burst == null)
            {
                return;
            }

            if (burst._playRoutine != null)
            {
                burst.StopCoroutine(burst._playRoutine);
                burst._playRoutine = null;
            }

            burst.ReleaseCount();
            burst.gameObject.SetActive(false);

            if (Pool.Count < MaxPoolSize)
            {
                if (_bucket != null)
                {
                    burst.transform.SetParent(_bucket, false);
                }

                Pool.Add(burst);
            }
            else
            {
                Object.Destroy(burst.gameObject);
            }
        }

        private static void EnsureSceneHook()
        {
            if (_sceneHooked)
            {
                return;
            }

            SceneManager.sceneUnloaded += _ => ResetAliveCount();
            _sceneHooked = true;
        }

        private static void ResetAliveCount()
        {
            _alive = 0;
            _bucket = null;
            for (var i = 0; i < Pool.Count; i++)
            {
                if (Pool[i] != null)
                {
                    Object.Destroy(Pool[i].gameObject);
                }
            }

            Pool.Clear();
        }

        private static void EnsureBucket()
        {
            if (_bucket != null)
            {
                return;
            }

            var existing = GameObject.Find("VfxBucket");
            if (existing == null)
            {
                existing = new GameObject("VfxBucket");
            }

            _bucket = existing.transform;
        }

        private IEnumerator Play()
        {
            _alive++;
            _counted = true;
            var t = 0f;
            transform.localScale = startScale;
            var baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

            while (t < lifetime)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.Clamp01(t / lifetime);
                var ease = 1f - (1f - u) * (1f - u);
                transform.localScale = Vector3.LerpUnclamped(startScale, endScale, ease);
                if (spriteRenderer != null)
                {
                    var c = baseColor;
                    c.a = baseColor.a * (1f - u);
                    spriteRenderer.color = c;
                }

                yield return null;
            }

            _playRoutine = null;
            Return(this);
        }

        private void OnDestroy()
        {
            ReleaseCount();
        }

        private void ReleaseCount()
        {
            if (!_counted)
            {
                return;
            }

            _counted = false;
            _alive = Mathf.Max(0, _alive - 1);
        }
    }
}
