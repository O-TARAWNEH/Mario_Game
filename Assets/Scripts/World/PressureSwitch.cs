// Filename: PressureSwitch.cs
// Folder: Assets/Scripts/World/
// Purpose: Player-activated floor switch for gate puzzles (Phase 40).
// Dependencies: GateBarrier

using System;
using UnityEngine;

namespace BounderTrail.World
{
    /// <summary>
    /// Activates when the player stands on top. Can hold, latch permanently, or latch timed.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PressureSwitch : MonoBehaviour
    {
        public enum Mode
        {
            HoldWhileStanding = 0,
            LatchPermanent = 1,
            LatchTimed = 2
        }

        [SerializeField] private Mode mode = Mode.HoldWhileStanding;
        [SerializeField] private float latchDuration = 3.5f;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private GateBarrier[] linkedGates;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color idleColor = new Color(0.85f, 0.55f, 0.2f, 1f);
        [SerializeField] private Color activeColor = new Color(0.35f, 0.95f, 0.55f, 1f);

        private bool _pressed;
        private float _latchTimer;
        private int _standCount;

        public bool IsActive => _pressed;
        public event Action<bool> ActiveChanged;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            ApplyVisual();
            PushToGates(false);
        }

        private void Update()
        {
            if (mode != Mode.LatchTimed || !_pressed)
            {
                return;
            }

            _latchTimer -= Time.deltaTime;
            if (_latchTimer > 0f)
            {
                return;
            }

            SetActive(false);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryPress(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryPress(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider == null || !collision.collider.CompareTag(playerTag))
            {
                return;
            }

            _standCount = Mathf.Max(0, _standCount - 1);
            if (mode == Mode.HoldWhileStanding && _standCount <= 0)
            {
                SetActive(false);
            }
        }

        private void TryPress(Collision2D collision)
        {
            if (collision.collider == null || !collision.collider.CompareTag(playerTag))
            {
                return;
            }

            if (!IsStandingOnTop(collision))
            {
                return;
            }

            if (_standCount <= 0)
            {
                _standCount = 1;
            }

            if (mode == Mode.LatchTimed)
            {
                _latchTimer = Mathf.Max(0.1f, latchDuration);
            }

            SetActive(true);
        }

        public void Configure(Mode newMode, float timedSeconds, GateBarrier[] gates)
        {
            mode = newMode;
            latchDuration = Mathf.Max(0.1f, timedSeconds);
            linkedGates = gates;
            PushToGates(_pressed);
        }

        private void SetActive(bool active)
        {
            if (_pressed == active)
            {
                if (active && mode == Mode.LatchTimed)
                {
                    _latchTimer = Mathf.Max(0.1f, latchDuration);
                }

                return;
            }

            _pressed = active;
            ApplyVisual();
            PushToGates(active);
            ActiveChanged?.Invoke(active);
        }

        private void PushToGates(bool open)
        {
            if (linkedGates == null)
            {
                return;
            }

            for (var i = 0; i < linkedGates.Length; i++)
            {
                if (linkedGates[i] != null)
                {
                    linkedGates[i].SetOpen(open);
                }
            }
        }

        private void ApplyVisual()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = _pressed ? activeColor : idleColor;
            }
        }

        private static bool IsStandingOnTop(Collision2D collision)
        {
            var contacts = collision.contactCount;
            for (var i = 0; i < contacts; i++)
            {
                if (collision.GetContact(i).normal.y >= 0.45f)
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            latchDuration = Mathf.Max(0.1f, latchDuration);
        }
#endif
    }
}
