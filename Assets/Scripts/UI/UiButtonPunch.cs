// Filename: UiButtonPunch.cs
// Folder: Assets/Scripts/UI/
// Purpose: Short scale punch when a UI button is clicked (Phase 38).
// Dependencies: None

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BounderTrail.UI
{
    /// <summary>
    /// Adds a readable press response without changing navigation or layout permanently.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UiButtonPunch : MonoBehaviour
    {
        [SerializeField] private float punchScale = 1.08f;
        [SerializeField] private float duration = 0.12f;

        private Button _button;
        private Vector3 _baseScale = Vector3.one;
        private Coroutine _routine;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnClicked);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClicked);
            }

            transform.localScale = _baseScale;
        }

        private void OnClicked()
        {
            // Resume / panel swaps deactivate this button in the same click.
            // Never StartCoroutine on an inactive object (Unity throws).
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                return;
            }

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine()
        {
            var punched = _baseScale * punchScale;
            transform.localScale = punched;
            var elapsed = 0f;
            var seconds = Mathf.Max(0.05f, duration);
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                var u = Mathf.Clamp01(elapsed / seconds);
                transform.localScale = Vector3.Lerp(punched, _baseScale, u * u);
                yield return null;
            }

            transform.localScale = _baseScale;
            _routine = null;
        }
    }
}
