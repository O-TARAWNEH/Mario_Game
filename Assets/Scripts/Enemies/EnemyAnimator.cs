// Filename: EnemyAnimator.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Drives enemy Animator from EnemyBrain/EnemyHealth state (Phase 10).
// Dependencies: EnemyBrain, EnemyHealth, Animator

using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Bridges enemy gameplay state into Animator parameters.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        private static readonly int StateHash = Animator.StringToHash("State");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        [SerializeField] private Animator animator;
        [SerializeField] private EnemyBrain brain;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private Rigidbody2D rigidBody;

        private EnemyStateId _lastState;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (brain == null)
            {
                brain = GetComponent<EnemyBrain>();
            }

            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }

            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody2D>();
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Hurt += OnHurt;
                health.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Hurt -= OnHurt;
                health.Died -= OnDied;
            }
        }

        private void Update()
        {
            if (animator == null)
            {
                return;
            }

            var state = brain != null ? brain.CurrentState : EnemyStateId.Idle;
            var speed = rigidBody != null ? Mathf.Abs(rigidBody.linearVelocity.x) : 0f;
            var dead = health != null && health.IsDead;

            animator.SetInteger(StateHash, (int)state);
            animator.SetFloat(SpeedHash, speed);
            animator.SetBool(IsDeadHash, dead);

            if (state == EnemyStateId.Attack && _lastState != EnemyStateId.Attack)
            {
                animator.SetTrigger(AttackHash);
            }

            _lastState = state;
        }

        private void OnHurt()
        {
            if (animator != null)
            {
                animator.SetTrigger(HurtHash);
            }
        }

        private void OnDied()
        {
            if (animator != null)
            {
                animator.SetBool(IsDeadHash, true);
                animator.SetInteger(StateHash, (int)EnemyStateId.Dead);
            }
        }
    }
}
