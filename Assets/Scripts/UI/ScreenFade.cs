// Filename: ScreenFade.cs
// Folder: Assets/Scripts/UI/
// Purpose: Full-screen fade for scene / level transitions (Phase 38).
// Dependencies: None

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.UI
{
    /// <summary>
    /// Persistent black overlay used during level loads and menu transitions.
    /// </summary>
    public class ScreenFade : MonoBehaviour
    {
        public static ScreenFade Instance { get; private set; }

        [SerializeField] private Image fadeImage;
        [SerializeField] private float defaultDuration = 0.22f;
        [SerializeField] private Color fadeColor = Color.black;

        private Coroutine _fadeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EnsureOverlay();
            SetAlpha(0f);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static IEnumerator FadeOut(float duration = -1f)
        {
            if (Instance == null)
            {
                yield break;
            }

            yield return Instance.FadeTo(1f, duration);
        }

        public static IEnumerator FadeIn(float duration = -1f)
        {
            if (Instance == null)
            {
                yield break;
            }

            yield return Instance.FadeTo(0f, duration);
        }

        public IEnumerator FadeTo(float targetAlpha, float duration = -1f)
        {
            EnsureOverlay();
            if (fadeImage == null)
            {
                yield break;
            }

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            var seconds = duration < 0f ? defaultDuration : Mathf.Max(0.01f, duration);
            var start = fadeImage.color.a;
            var elapsed = 0f;
            fadeImage.raycastTarget = targetAlpha > 0.01f || start > 0.01f;

            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                var u = Mathf.Clamp01(elapsed / seconds);
                SetAlpha(Mathf.Lerp(start, targetAlpha, u));
                yield return null;
            }

            SetAlpha(targetAlpha);
            fadeImage.raycastTarget = targetAlpha > 0.01f;
            _fadeRoutine = null;
        }

        private void EnsureOverlay()
        {
            if (fadeImage != null)
            {
                return;
            }

            var canvasGo = new GameObject("ScreenFadeCanvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var imageGo = new GameObject("Fade", typeof(RectTransform));
            imageGo.transform.SetParent(canvasGo.transform, false);
            fadeImage = imageGo.AddComponent<Image>();
            fadeImage.color = fadeColor;
            fadeImage.raycastTarget = false;

            var rect = fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void SetAlpha(float alpha)
        {
            if (fadeImage == null)
            {
                return;
            }

            var c = fadeColor;
            c.a = Mathf.Clamp01(alpha);
            fadeImage.color = c;
        }
    }
}
