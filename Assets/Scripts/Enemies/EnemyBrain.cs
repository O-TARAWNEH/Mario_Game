// Filename: EnemyBrain.cs
// Folder: Assets/Scripts/Enemies/
// Purpose: Flexible base enemy state machine (Phase 9).
// Dependencies: EnemyHealth, EnemyMover, EnemySensor, EnemyStateId, GameLog

using BounderTrail.Core;
using UnityEngine;

namespace BounderTrail.Enemies
{
    /// <summary>
    /// Base enemy brain. Enable only the behaviors a type needs.
    /// Designed so future enemy types can reuse this or subclass it.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyMover))]
    public class EnemyBrain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyHealth health;
        [SerializeField] private EnemyMover mover;
        [SerializeField] private EnemySensor sensor;

        [Header("Behavior Toggles")]
        [SerializeField] private bool canPatrol = true;
        [SerializeField] private bool canChase;
        [SerializeField] private bool canAttack;
        [SerializeField] private bool turnAtLedges = true;
        [SerializeField] private bool turnAtWalls = true;

        [Header("Timing")]
        [SerializeField] private float idleDuration = 0.75f;
        [SerializeField] private float attackDuration = 0.35f;
        [SerializeField] private float attackRange = 1.1f;
        [SerializeField] private float losePlayerGrace = 0.4f;

        [Header("Start")]
        [SerializeField] private EnemyStateId initialState = EnemyStateId.Patrol;
        [SerializeField] private int initialFacing = -1;

        private float _stateTimer;
        private float _lostPlayerTimer;
        private EnemyStateId _state = EnemyStateId.Idle;

        public EnemyStateId CurrentState => _state;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<EnemyHealth>();
            }

            if (mover == null)
            {
                mover = GetComponent<EnemyMover>();
            }

            if (sensor == null)
            {
                sensor = GetComponent<EnemySensor>();
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

        private void Start()
        {
            mover.SetFacing(initialFacing >= 0 ? 1 : -1);
            SetState(canPatrol ? initialState : EnemyStateId.Idle);
        }

        private void FixedUpdate()
        {
            if (health != null && health.IsDead)
            {
                return;
            }

            if (_stateTimer > 0f)
            {
                _stateTimer -= Time.fixedDeltaTime;
            }

            switch (_state)
            {
                case EnemyStateId.Idle:
                    TickIdle();
                    break;
                case EnemyStateId.Patrol:
                    TickPatrol();
                    break;
                case EnemyStateId.Chase:
                    TickChase();
                    break;
                case EnemyStateId.Attack:
                    TickAttack();
                    break;
                case EnemyStateId.Hurt:
                    TickHurt();
                    break;
                case EnemyStateId.Dead:
                    mover.StopHorizontal();
                    break;
            }
        }

        public void SetState(EnemyStateId next)
        {
            if (_state == next)
            {
                return;
            }

            _state = next;
            switch (next)
            {
                case EnemyStateId.Idle:
                    _stateTimer = idleDuration;
                    mover.StopHorizontal();
                    break;
                case EnemyStateId.Patrol:
                    break;
                case EnemyStateId.Chase:
                    _lostPlayerTimer = losePlayerGrace;
                    break;
                case EnemyStateId.Attack:
                    _stateTimer = attackDuration;
                    mover.StopHorizontal();
                    break;
                case EnemyStateId.Hurt:
                    mover.StopHorizontal();
                    break;
                case EnemyStateId.Dead:
                    mover.StopHorizontal();
                    break;
            }

            GameLog.Info("Enemy", $"{name} -> {_state}");
        }

        private void TickIdle()
        {
            if (TryEnterAggro())
            {
                return;
            }

            if (canPatrol && _stateTimer <= 0f)
            {
                SetState(EnemyStateId.Patrol);
            }
        }

        private void TickPatrol()
        {
            if (TryEnterAggro())
            {
                return;
            }

            if (turnAtWalls && mover.IsWallAhead())
            {
                mover.Flip();
            }
            else if (turnAtLedges && !mover.IsGroundAhead())
            {
                mover.Flip();
            }

            mover.MoveFacingDirection();
        }

        private void TickChase()
        {
            if (sensor == null || !canChase)
            {
                SetState(canPatrol ? EnemyStateId.Patrol : EnemyStateId.Idle);
                return;
            }

            if (sensor.PlayerDetected && sensor.DetectedPlayer != null)
            {
                _lostPlayerTimer = losePlayerGrace;
                var playerPos = sensor.DetectedPlayer.position;

                if (canAttack && Vector2.Distance(transform.position, playerPos) <= attackRange)
                {
                    SetState(EnemyStateId.Attack);
                    return;
                }

                mover.MoveToward(playerPos);
                return;
            }

            _lostPlayerTimer -= Time.fixedDeltaTime;
            if (_lostPlayerTimer <= 0f)
            {
                SetState(canPatrol ? EnemyStateId.Patrol : EnemyStateId.Idle);
            }
            else
            {
                mover.MoveFacingDirection();
            }
        }

        private void TickAttack()
        {
            // Attack contact damage is handled by EnemyContact.
            // This state mainly pauses movement briefly for future attack timing/anim.
            if (_stateTimer <= 0f)
            {
                if (canChase && sensor != null && sensor.PlayerDetected)
                {
                    SetState(EnemyStateId.Chase);
                }
                else
                {
                    SetState(canPatrol ? EnemyStateId.Patrol : EnemyStateId.Idle);
                }
            }
        }

        private void TickHurt()
        {
            if (health == null || !health.IsHurt)
            {
                SetState(canPatrol ? EnemyStateId.Patrol : EnemyStateId.Idle);
            }
        }

        private bool TryEnterAggro()
        {
            if (!canChase || sensor == null || !sensor.PlayerDetected)
            {
                return false;
            }

            SetState(EnemyStateId.Chase);
            return true;
        }

        private void OnHurt()
        {
            if (health != null && health.IsAlive)
            {
                SetState(EnemyStateId.Hurt);
            }
        }

        private void OnDied()
        {
            SetState(EnemyStateId.Dead);
            enabled = false;
        }
    }
}
