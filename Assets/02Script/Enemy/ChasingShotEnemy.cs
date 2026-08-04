using UnityEngine;

namespace DDARoguelike
{
    public class ChasingShotEnemy : Enemy
    {
        private const string EnemyTag = "Enemy";

        [SerializeField] private float chaseRange = 10.0f;
        [SerializeField] private float shotRange = 6.0f;
        [SerializeField] private float shotRangeExitPadding = 1.25f;
        [SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private float fireRate = 1.0f;
        [SerializeField] private float shotSpeed = 3.0f;
        [SerializeField] private float obstacleCastRadius = 0.35f;
        [SerializeField] private float obstacleLookAhead = 0.8f;
        [SerializeField] private float avoidanceStickBias = 0.35f;
        [SerializeField] private float turnRadiansPerSecond = 8.0f;
        [SerializeField] private float stopDeceleration = 12.0f;
        [SerializeField] private Transform shotPosition;
        [SerializeField] private GameObject enemyProjectilePrefab;
        [SerializeField] private ProjectilePool projectilePool;

        private Transform playerTransform;
        private float nextFireTime;
        private Vector2 smoothedMoveDirection;

        protected override void Awake()
        {
            if (maxHp <= 0.0f)
            {
                maxHp = 10.0f;
            }

            if (attackPower <= 0)
            {
                attackPower = 1;
            }

            base.Awake();
            SetState(AI_State.Chase);

            if (rigidbody2D == null)
            {
                Debug.LogError($"[{nameof(ChasingShotEnemy)}] Rigidbody2D is required on {gameObject.name}.", this);
            }

            if (shotPosition == null)
            {
                shotPosition = transform;
            }

            if (enemyProjectilePrefab == null)
            {
                Debug.LogError($"[{nameof(ChasingShotEnemy)}] enemyProjectilePrefab is not assigned on {gameObject.name}.", this);
            }

            if (projectilePool == null)
            {
                projectilePool = FindFirstObjectByType<ProjectilePool>();
            }

            if (projectilePool == null)
            {
                Debug.LogError($"[{nameof(ChasingShotEnemy)}] projectilePool is not assigned on {gameObject.name}.", this);
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject == null)
            {
                Debug.LogError($"[{nameof(ChasingShotEnemy)}] Player with tag 'Player' was not found.", this);
            }
            else
            {
                playerTransform = playerObject.transform;
            }
        }

        protected override void OnPreparedFromPool()
        {
            SetState(AI_State.Chase);
            nextFireTime = 0.0f;
            smoothedMoveDirection = Vector2.zero;

            if (projectilePool == null)
            {
                projectilePool = FindFirstObjectByType<ProjectilePool>();
            }

            if (playerTransform == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                {
                    playerTransform = playerObject.transform;
                }
            }
        }

        protected override void OnIdle()
        {
            UpdateCombatState();
        }

        protected override void OnChase()
        {
            UpdateCombatState();
        }

        protected override void OnAttack()
        {
            UpdateCombatState();

            if (currentState != AI_State.Attack)
            {
                return;
            }

            TryShoot();
        }

        private void UpdateCombatState()
        {
            if (playerTransform == null || currentState == AI_State.Die)
            {
                return;
            }

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            float attackExitRange = shotRange + Mathf.Max(0.0f, shotRangeExitPadding);

            if (currentState == AI_State.Attack)
            {
                if (distance > chaseRange)
                {
                    SetState(AI_State.Idle);
                }
                else if (distance > attackExitRange)
                {
                    SetState(AI_State.Chase);
                }

                return;
            }

            if (currentState == AI_State.Chase)
            {
                if (distance <= shotRange)
                {
                    SetState(AI_State.Attack);
                }
                else if (distance > chaseRange)
                {
                    SetState(AI_State.Idle);
                }

                return;
            }

            if (distance <= shotRange)
            {
                SetState(AI_State.Attack);
            }
            else if (distance <= chaseRange)
            {
                SetState(AI_State.Chase);
            }
            else
            {
                SetState(AI_State.Idle);
            }
        }

        private void TryShoot()
        {
            if (enemyProjectilePrefab == null || projectilePool == null || playerTransform == null)
            {
                return;
            }

            if (fireRate <= 0.0f)
            {
                return;
            }

            if (Time.time < nextFireTime)
            {
                return;
            }

            nextFireTime = Time.time + 1.0f / fireRate;

            Vector2 direction = (Vector2)playerTransform.position - (Vector2)shotPosition.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            direction.Normalize();

            Projectile projectile = projectilePool.Get(enemyProjectilePrefab);

            if (projectile == null)
            {
                return;
            }

            projectile.transform.position = shotPosition.position;
            projectile.Launch(
                direction,
                shotSpeed,
                shotRange,
                attackPower,
                projectilePool,
                gameObject.name,
                EnemyTag);
        }

        private void FixedUpdate()
        {
            if (TryApplyKnockbackMovement())
            {
                return;
            }

            if (rigidbody2D == null)
            {
                return;
            }

            if (playerTransform == null || currentState == AI_State.Die || currentState == AI_State.Idle)
            {
                ApplyStopMovement();
                return;
            }

            if (currentState == AI_State.Attack)
            {
                ApplyStopMovement();
                return;
            }

            if (currentState != AI_State.Chase)
            {
                ApplyStopMovement();
                return;
            }

            Vector2 direction = (Vector2)playerTransform.position - rigidbody2D.position;

            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
                Vector2 steered = ObstacleAvoidanceSteering.Resolve(
                    rigidbody2D.position,
                    direction,
                    playerTransform.position,
                    obstacleCastRadius,
                    obstacleLookAhead,
                    smoothedMoveDirection,
                    avoidanceStickBias);
                smoothedMoveDirection = ObstacleAvoidanceSteering.SmoothDirection(
                    smoothedMoveDirection,
                    steered,
                    turnRadiansPerSecond,
                    Time.fixedDeltaTime);
                rigidbody2D.linearVelocity = smoothedMoveDirection * moveSpeed;
            }
            else
            {
                ApplyStopMovement();
            }
        }

        private void ApplyStopMovement()
        {
            float deceleration = Mathf.Max(0.0f, stopDeceleration) * Time.fixedDeltaTime;
            Vector2 currentVelocity = rigidbody2D.linearVelocity;
            rigidbody2D.linearVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration);

            if (rigidbody2D.linearVelocity.sqrMagnitude <= 0.0001f)
            {
                smoothedMoveDirection = Vector2.zero;
            }
            else
            {
                smoothedMoveDirection = rigidbody2D.linearVelocity.normalized;
            }
        }
    }
}
