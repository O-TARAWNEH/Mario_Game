// Filename: PlayerAnimator.cs
// Folder: Assets/Scripts/Player/
// Purpose: Drives the player Animator from gameplay state (Phase 5).
// Dependencies: PlayerController, PlayerGroundSensor, PlayerDeath, Animator

using UnityEngine;

namespace BounderTrail.Player
{
    /// <summary>
    /// Bridges movement/ground/jump/fall/death state into Animator parameters.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
        private static readonly int IsFallingHash = Animator.StringToHash("IsFalling");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        private static readonly int LandHash = Animator.StringToHash("Land");
        private static readonly int DieHash = Animator.StringToHash("Die");

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerGroundSensor groundSensor;
        [SerializeField] private PlayerDeath playerDeath;
        [SerializeField] private Rigidbody2D rigidBody;

        [Header("Thresholds")]
        [SerializeField] private float moveSpeedThreshold = 0.15f;
        [SerializeField] private float verticalThreshold = 0.15f;
        [SerializeField] private float landAnimationLock = 0.12f;

        private bool _wasGrounded = true;
        private float _landLockTimer;
        private bool _deathTriggerSent;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (groundSensor == null)
            {
                groundSensor = GetComponent<PlayerGroundSensor>();
            }

            if (playerDeath == null)
            {
                playerDeath = GetComponent<PlayerDeath>();
            }

            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody2D>();
            }
        }

        private void OnEnable()
        {
            if (playerDeath != null)
            {
                playerDeath.Died += OnPlayerDied;
            }
        }

        private void OnDisable()
        {
            if (playerDeath != null)
            {
                playerDeath.Died -= OnPlayerDied;
            }
        }

        private void Update()
        {
            if (animator == null)
            {
                return;
            }

            if (playerDeath != null && playerDeath.IsDead)
            {
                animator.SetBool(IsDeadHash, true);
                animator.SetFloat(SpeedHash, 0f);
                animator.SetBool(IsRunningHash, false);
                animator.SetBool(IsJumpingHash, false);
                animator.SetBool(IsFallingHash, false);
                return;
            }

            var grounded = groundSensor != null && groundSensor.IsGrounded;
            var velocity = rigidBody != null ? rigidBody.linearVelocity : Vector2.zero;
            var speed = Mathf.Abs(velocity.x);
            var isJumping = !grounded && velocity.y > verticalThreshold;
            var isFalling = !grounded && velocity.y < -verticalThreshold;
            var isRunning = false;

            if (playerController != null)
            {
                isRunning = playerController.IsRunning;
            }
            else
            {
                isRunning = grounded && speed > moveSpeedThreshold * 2f;
            }

            if (_landLockTimer > 0f)
            {
                _landLockTimer -= Time.deltaTime;
            }

            if (!_wasGrounded && grounded && _landLockTimer <= 0f)
            {
                animator.SetTrigger(LandHash);
                _landLockTimer = landAnimationLock;
            }

            animator.SetBool(IsDeadHash, false);
            animator.SetFloat(SpeedHash, speed);
            animator.SetBool(IsGroundedHash, grounded);
            animator.SetBool(IsJumpingHash, isJumping);
            animator.SetBool(IsFallingHash, isFalling);
            animator.SetBool(IsRunningHash, isRunning);

            _wasGrounded = grounded;
        }

        private void OnPlayerDied()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsDeadHash, true);
            if (!_deathTriggerSent)
            {
                animator.SetTrigger(DieHash);
                _deathTriggerSent = true;
            }
        }

        /// <summary>
        /// Clears death animation flags after a checkpoint respawn.
        /// </summary>
        public void ResetAfterRespawn()
        {
            _deathTriggerSent = false;
            _wasGrounded = true;
            _landLockTimer = 0f;

            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsDeadHash, false);
            animator.ResetTrigger(DieHash);
            animator.Play("Idle", 0, 0f);
        }
    }
}
