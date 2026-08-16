// Filename: PlayerController.cs
// Folder: Assets/Scripts/Player/
// Purpose: Responsive player motor with polished jump/air feel (Phase 4).
// Dependencies: PlayerGroundSensor, BounderTrail.Core.GameLog, GameStateManager (pause gate).

using System;
using BounderTrail.Core;
using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Playable character motor for Pip with acceleration, coyote time,
    /// jump buffering, variable jump height, and air control.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlayerGroundSensor))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private PlayerGroundSensor groundSensor;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Horizontal Movement")]
        [SerializeField] private float walkSpeed = 7.2f;
        [SerializeField] private float runSpeed = 10.8f;
        [SerializeField] private float acceleration = 92f;
        [SerializeField] private float deceleration = 98f;
        [SerializeField] private float airAcceleration = 50f;
        [SerializeField] private float airDeceleration = 44f;
        [Range(0f, 1f)]
        [SerializeField] private float airControl = 0.82f;

        [Header("Jumping")]
        [SerializeField] private float jumpForce = 16.2f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.14f;
        [Range(0f, 1f)]
        [SerializeField] private float jumpCutMultiplier = 0.48f;
        [SerializeField] private float jumpCutGravityMultiplier = 2.35f;

        [Header("Gravity")]
        [SerializeField] private float gravity = 3.45f;
        [SerializeField] private float fallGravityMultiplier = 2.35f;
        [SerializeField] private float apexHangGravityMultiplier = 0.72f;
        [SerializeField] private float apexHangVelocityThreshold = 0.85f;
        [SerializeField] private float maximumFallSpeed = 24f;

        [Header("Slope / Edge Assist")]
        [SerializeField] private bool projectMoveOnSlope = true;
        [SerializeField] private float groundedStickForce = 2.5f;

        [Header("Input")]
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private KeyCode[] jumpKeys = { KeyCode.Space, KeyCode.W, KeyCode.UpArrow };
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

        private float _horizontalInput;
        private bool _runHeld;
        private bool _jumpPressed;
        private bool _jumpReleased;
        private bool _jumpHeld;
        private bool _facingRight = true;

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _isJumping;
        private bool _wasGrounded;
        private float _controlLockTimer;
        private float _speedMultiplier = 1f;
        private float _fallSpeedTracker;
        private float _lastLandingSpeed;

        public bool IsGrounded => groundSensor != null && groundSensor.IsGrounded && !IsRisingFromJump;
        public bool FacingRight => _facingRight;
        public float CurrentMoveSpeed => (_runHeld ? runSpeed : walkSpeed) * _speedMultiplier;
        public bool IsRunning => _runHeld && Mathf.Abs(_horizontalInput) > 0.01f;
        public float HorizontalVelocity => rigidBody != null ? rigidBody.linearVelocity.x : 0f;
        public float VerticalVelocity => rigidBody != null ? rigidBody.linearVelocity.y : 0f;
        public bool IsJumpingVisual => !IsGrounded && VerticalVelocity > 0.15f;
        public bool IsFallingVisual => !IsGrounded && VerticalVelocity < -0.15f;
        public bool IsControlLocked => _controlLockTimer > 0f;
        public float SpeedMultiplier => _speedMultiplier;
        /// <summary>Downward speed at the moment of the most recent landing (for juice).</summary>
        public float LastLandingSpeed => _lastLandingSpeed;
        private bool IsRisingFromJump => _isJumping && VerticalVelocity > 0.35f;

        /// <summary>Raised when a jump is successfully consumed.</summary>
        public event Action Jumped;

        /// <summary>Raised on the frame the player becomes grounded after airtime.</summary>
        public event Action Landed;

        /// <summary>
        /// Briefly disables horizontal steering so knockback can play out.
        /// Pass 0 (or less) to clear any active lock.
        /// </summary>
        public void LockControl(float duration)
        {
            if (duration <= 0f)
            {
                _controlLockTimer = 0f;
                return;
            }

            _controlLockTimer = Mathf.Max(_controlLockTimer, duration);
            _horizontalInput = 0f;
            _jumpPressed = false;
            _jumpReleased = false;
            _jumpBufferTimer = 0f;
        }

        /// <summary>Sets facing used by sprite flip / respawn checkpoints.</summary>
        public void SetFacing(bool faceRight)
        {
            _facingRight = faceRight;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !faceRight;
            }
            else
            {
                var scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
                transform.localScale = scale;
            }
        }

        /// <summary>
        /// Multiplier applied to walk/run speed (Speed Burst power-up).
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(0.01f, multiplier);
        }

        private void Awake()
        {
            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody2D>();
            }

            if (groundSensor == null)
            {
                groundSensor = GetComponent<PlayerGroundSensor>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            ConfigureRigidbody();
        }

        private void Update()
        {
            TickTimers();

            if (!CanAcceptGameplayInput() || IsControlLocked)
            {
                _horizontalInput = 0f;
                _runHeld = false;
                _jumpHeld = false;
                _jumpPressed = false;
                _jumpReleased = false;
                return;
            }

            _horizontalInput = Input.GetAxisRaw(horizontalAxis);
            _runHeld = Input.GetKey(runKey);
            _jumpHeld = IsAnyJumpKeyHeld();

            if (WasAnyJumpKeyPressedThisFrame())
            {
                _jumpPressed = true;
                _jumpBufferTimer = jumpBufferTime;
            }

            if (WasAnyJumpKeyReleasedThisFrame())
            {
                _jumpReleased = true;
            }

            UpdateFacing();
        }

        private void FixedUpdate()
        {
            if (rigidBody == null)
            {
                return;
            }

            TrackFallSpeed();
            UpdateCoyoteAndLanding();
            ApplyGravityScaling();

            if (!IsControlLocked)
            {
                ApplyHorizontalMovement();
                TryConsumeJump();
                ApplyJumpCut();
                ApplyGroundStick();
            }

            ClampFallSpeed();
        }

        private void ConfigureRigidbody()
        {
            rigidBody.freezeRotation = true;
            rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            rigidBody.gravityScale = gravity;
        }

        private void TickTimers()
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpBufferTimer -= Time.deltaTime;
            }

            if (_controlLockTimer > 0f)
            {
                _controlLockTimer -= Time.deltaTime;
            }
        }

        private void TrackFallSpeed()
        {
            if (IsGrounded)
            {
                return;
            }

            var downward = -VerticalVelocity;
            if (downward > _fallSpeedTracker)
            {
                _fallSpeedTracker = downward;
            }
        }

        private void UpdateCoyoteAndLanding()
        {
            var grounded = IsGrounded;
            if (grounded)
            {
                _coyoteTimer = coyoteTime;

                if (!_wasGrounded)
                {
                    _lastLandingSpeed = _fallSpeedTracker;
                    _fallSpeedTracker = 0f;
                    _isJumping = false;
                    Landed?.Invoke();
                }
            }
            else if (_coyoteTimer > 0f)
            {
                _coyoteTimer -= Time.fixedDeltaTime;
            }

            _wasGrounded = grounded;
        }

        private void ApplyGravityScaling()
        {
            var velocity = rigidBody.linearVelocity;
            var scale = gravity;

            if (velocity.y < 0f)
            {
                scale = gravity * fallGravityMultiplier;
            }
            else if (_isJumping && velocity.y > 0f && velocity.y <= apexHangVelocityThreshold)
            {
                // Brief apex hang — Mario-like air control window at jump peak (Phase 34).
                scale = gravity * apexHangGravityMultiplier;
            }
            else if (_isJumping && !_jumpHeld)
            {
                // Extra gravity while rising after early jump release.
                scale = gravity * jumpCutGravityMultiplier;
            }

            rigidBody.gravityScale = scale;
        }

        private void ApplyHorizontalMovement()
        {
            var velocity = rigidBody.linearVelocity;
            var targetSpeed = _horizontalInput * CurrentMoveSpeed;

            float accel;
            float decel;

            if (IsGrounded)
            {
                accel = acceleration;
                decel = deceleration;
            }
            else
            {
                accel = airAcceleration * airControl;
                decel = airDeceleration * airControl;
            }

            var rate = Mathf.Abs(targetSpeed) > 0.01f ? accel : decel;
            var newX = Mathf.MoveTowards(velocity.x, targetSpeed, rate * Time.fixedDeltaTime);

            if (IsGrounded && projectMoveOnSlope && groundSensor != null && groundSensor.GroundAngle > 0.5f)
            {
                // Move along the ground tangent so shallow slopes feel stable.
                var tangent = new Vector2(groundSensor.GroundNormal.y, -groundSensor.GroundNormal.x);
                if (Mathf.Sign(tangent.x) != Mathf.Sign(newX) && Mathf.Abs(newX) > 0.01f)
                {
                    tangent = -tangent;
                }

                var alongSlope = tangent.normalized * newX;
                velocity.x = alongSlope.x;
                // Preserve intentional vertical motion (jumps); only nudge gently on slopes while grounded.
                if (!_isJumping)
                {
                    velocity.y = alongSlope.y;
                }
            }
            else
            {
                velocity.x = newX;
            }

            rigidBody.linearVelocity = velocity;
        }

        private void TryConsumeJump()
        {
            var buffered = _jumpBufferTimer > 0f || _jumpPressed;
            _jumpPressed = false;

            if (!buffered)
            {
                return;
            }

            var canCoyoteJump = _coyoteTimer > 0f;
            if (!canCoyoteJump)
            {
                return;
            }

            var velocity = rigidBody.linearVelocity;
            velocity.y = jumpForce;
            rigidBody.linearVelocity = velocity;

            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            _isJumping = true;
            Jumped?.Invoke();
        }

        private void ApplyJumpCut()
        {
            if (!_jumpReleased)
            {
                return;
            }

            _jumpReleased = false;

            var velocity = rigidBody.linearVelocity;
            if (_isJumping && velocity.y > 0f)
            {
                velocity.y *= jumpCutMultiplier;
                rigidBody.linearVelocity = velocity;
            }
        }

        private void ClampFallSpeed()
        {
            var velocity = rigidBody.linearVelocity;
            if (velocity.y < -maximumFallSpeed)
            {
                velocity.y = -maximumFallSpeed;
                rigidBody.linearVelocity = velocity;
            }
        }

        private void ApplyGroundStick()
        {
            if (!IsGrounded || _isJumping || groundedStickForce <= 0f)
            {
                return;
            }

            // Small downward bias helps keep contact on edges/slopes without killing jumps.
            if (Mathf.Abs(_horizontalInput) > 0.01f && rigidBody.linearVelocity.y <= 0f)
            {
                rigidBody.linearVelocity += Vector2.down * groundedStickForce * Time.fixedDeltaTime;
            }
        }

        private void UpdateFacing()
        {
            if (_horizontalInput > 0.01f && !_facingRight)
            {
                SetFacing(true);
            }
            else if (_horizontalInput < -0.01f && _facingRight)
            {
                SetFacing(false);
            }
        }

        private bool WasAnyJumpKeyPressedThisFrame()
        {
            if (jumpKeys != null)
            {
                for (var i = 0; i < jumpKeys.Length; i++)
                {
                    if (Input.GetKeyDown(jumpKeys[i]))
                    {
                        return true;
                    }
                }
            }

            return Input.GetKeyDown(jumpKey);
        }

        private bool WasAnyJumpKeyReleasedThisFrame()
        {
            if (jumpKeys != null)
            {
                for (var i = 0; i < jumpKeys.Length; i++)
                {
                    if (Input.GetKeyUp(jumpKeys[i]))
                    {
                        return true;
                    }
                }
            }

            return Input.GetKeyUp(jumpKey);
        }

        private bool IsAnyJumpKeyHeld()
        {
            if (jumpKeys != null)
            {
                for (var i = 0; i < jumpKeys.Length; i++)
                {
                    if (Input.GetKey(jumpKeys[i]))
                    {
                        return true;
                    }
                }
            }

            return Input.GetKey(jumpKey);
        }

        private static bool CanAcceptGameplayInput()
        {
            if (GameStateManager.Instance == null)
            {
                return true;
            }

            return GameStateManager.Instance.CurrentState == GameStateId.Gameplay;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            walkSpeed = Mathf.Max(0f, walkSpeed);
            runSpeed = Mathf.Max(walkSpeed, runSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            deceleration = Mathf.Max(0f, deceleration);
            airAcceleration = Mathf.Max(0f, airAcceleration);
            airDeceleration = Mathf.Max(0f, airDeceleration);
            airControl = Mathf.Clamp01(airControl);
            jumpForce = Mathf.Max(0f, jumpForce);
            coyoteTime = Mathf.Max(0f, coyoteTime);
            jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
            jumpCutMultiplier = Mathf.Clamp01(jumpCutMultiplier);
            jumpCutGravityMultiplier = Mathf.Max(1f, jumpCutGravityMultiplier);
            gravity = Mathf.Max(0f, gravity);
            fallGravityMultiplier = Mathf.Max(1f, fallGravityMultiplier);
            apexHangGravityMultiplier = Mathf.Clamp(apexHangGravityMultiplier, 0.05f, 1f);
            apexHangVelocityThreshold = Mathf.Max(0.05f, apexHangVelocityThreshold);
            maximumFallSpeed = Mathf.Max(0f, maximumFallSpeed);
            groundedStickForce = Mathf.Max(0f, groundedStickForce);

            if (jumpKeys == null || jumpKeys.Length == 0)
            {
                jumpKeys = new[] { KeyCode.Space, KeyCode.W, KeyCode.UpArrow };
            }
        }
#endif
    }
}
