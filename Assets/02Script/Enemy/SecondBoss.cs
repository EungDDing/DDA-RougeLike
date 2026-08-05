using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    public sealed class SecondBoss : BossController
    {
        private const string EnemyIgnoreTag = "Enemy";
        private const int PatternCount = 3;
        private const int RingShotCount = 8;

        [SerializeField] private float chaseRange = 9.0f;
        [SerializeField] private float shotRange = 6.0f;
        [SerializeField] private float shotRangeExitPadding = 1.25f;
        [SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private float obstacleCastRadius = 0.4f;
        [SerializeField] private float obstacleLookAhead = 0.8f;
        [SerializeField] private float avoidanceStickBias = 0.35f;
        [SerializeField] private float turnRadiansPerSecond = 8.0f;
        [SerializeField] private float stopDeceleration = 12.0f;
        [SerializeField] private float patternInterval = 3.0f;
        [SerializeField] private float initialAttackDelay = 1.5f;
        [SerializeField] private float volleyInterval = 0.75f;
        [SerializeField] private float volleySpreadAngle = 18.0f;
        [SerializeField] private float projectileSpeed = 4.0f;
        [SerializeField] private float projectileRange = 12.0f;
        [SerializeField] private Transform shotPosition;
        [SerializeField] private GameObject homingProjectilePrefab;
        [SerializeField] private GameObject normalProjectilePrefab;
        [SerializeField] private ProjectilePool projectilePool;

        private Vector2 smoothedMoveDirection;
        private bool isHoldingForShot;
        private bool hasStartedCombatLoop;
        private Coroutine patternLoopCoroutine;
        private int previousPatternIndex = -1;
        private float facingSign = 1.0f;

        protected override void Awake()
        {
            base.Awake();
            ResolveReferences();
            CacheFacingSign();
        }

        protected override void OnPreparedFromPool()
        {
            base.OnPreparedFromPool();
            hasStartedCombatLoop = false;
            isHoldingForShot = false;
            smoothedMoveDirection = Vector2.zero;
            previousPatternIndex = -1;
            patternLoopCoroutine = null;
            ResolveReferences();
            CacheFacingSign();
        }

        protected override void OnIdle()
        {
            if (Data == null)
            {
                return;
            }

            if (Data.ActivationRange <= 0.0f)
            {
                BeginCombatLoop();
                return;
            }

            base.OnIdle();
        }

        protected override void OnAttack()
        {
            BeginCombatLoop();
            UpdateFacing();
            UpdateChaseMovement();
        }

        protected override void OnDie()
        {
            hasStartedCombatLoop = false;

            if (patternLoopCoroutine != null)
            {
                StopCoroutine(patternLoopCoroutine);
                patternLoopCoroutine = null;
            }

            StopMovement();
            base.OnDie();
        }

        private void BeginCombatLoop()
        {
            if (hasStartedCombatLoop || currentState == AI_State.Die)
            {
                return;
            }

            hasStartedCombatLoop = true;
            SetState(AI_State.Attack);

            if (patternLoopCoroutine != null)
            {
                StopCoroutine(patternLoopCoroutine);
            }

            patternLoopCoroutine = StartCoroutine(PatternLoop());
        }

        private void ResolveReferences()
        {
            EnsurePlayerReference();

            if (shotPosition == null)
            {
                shotPosition = transform;
            }

            if (projectilePool == null)
            {
                projectilePool = FindFirstObjectByType<ProjectilePool>();
            }

            if (homingProjectilePrefab == null)
            {
                Debug.LogError($"[{nameof(SecondBoss)}] homingProjectilePrefab is not assigned on {gameObject.name}.", this);
            }

            if (normalProjectilePrefab == null)
            {
                Debug.LogError($"[{nameof(SecondBoss)}] normalProjectilePrefab is not assigned on {gameObject.name}.", this);
            }

            if (projectilePool == null)
            {
                Debug.LogError($"[{nameof(SecondBoss)}] ProjectilePool was not found for {gameObject.name}.", this);
            }
        }

        private void CacheFacingSign()
        {
            float scaleX = transform.localScale.x;

            if (Mathf.Abs(scaleX) > 0.0001f)
            {
                facingSign = Mathf.Sign(scaleX);
            }
            else
            {
                facingSign = 1.0f;
            }
        }

        private void UpdateFacing()
        {
            if (playerTransform == null || currentState == AI_State.Die)
            {
                return;
            }

            float deltaX = playerTransform.position.x - transform.position.x;

            if (Mathf.Abs(deltaX) <= 0.05f)
            {
                return;
            }

            float targetSign = deltaX > 0.0f ? facingSign : -facingSign;
            Vector3 scale = transform.localScale;
            float absX = Mathf.Abs(scale.x);

            if (absX <= 0.0001f)
            {
                absX = 1.0f;
            }

            float nextX = absX * targetSign;

            if (Mathf.Abs(scale.x - nextX) <= 0.0001f)
            {
                return;
            }

            scale.x = nextX;
            transform.localScale = scale;
        }

        private void UpdateChaseMovement()
        {
            if (currentState == AI_State.Die || playerTransform == null)
            {
                StopMovement();
                return;
            }

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            float attackExitRange = shotRange + Mathf.Max(0.0f, shotRangeExitPadding);

            if (isHoldingForShot)
            {
                if (distance > chaseRange)
                {
                    isHoldingForShot = false;
                }
                else if (distance > attackExitRange)
                {
                    isHoldingForShot = false;
                }
            }
            else if (distance <= shotRange)
            {
                isHoldingForShot = true;
            }

            if (isHoldingForShot || distance > chaseRange)
            {
                smoothedMoveDirection = Vector2.zero;
                StopMovement();
                return;
            }

            Vector2 direction = (Vector2)playerTransform.position - (Vector2)transform.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                smoothedMoveDirection = Vector2.zero;
                StopMovement();
                return;
            }

            direction.Normalize();
            Vector2 steered = ObstacleAvoidanceSteering.Resolve(
                transform.position,
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
                Time.deltaTime);
            SetMovement(smoothedMoveDirection * moveSpeed);
        }

        private void ApplyStopMovement()
        {
            float deceleration = Mathf.Max(0.0f, stopDeceleration) * Time.deltaTime;
            Vector2 current = smoothedMoveDirection * moveSpeed;
            Vector2 next = Vector2.MoveTowards(current, Vector2.zero, deceleration);

            if (next.sqrMagnitude <= 0.0001f)
            {
                smoothedMoveDirection = Vector2.zero;
                StopMovement();
                return;
            }

            smoothedMoveDirection = next.normalized;
            SetMovement(next);
        }

        private IEnumerator PatternLoop()
        {
            float interval = Mathf.Max(0.1f, patternInterval);
            float firstDelay = Mathf.Max(0.0f, initialAttackDelay);

            if (firstDelay > 0.0f)
            {
                yield return new WaitForSeconds(firstDelay);

                if (currentState == AI_State.Die)
                {
                    yield break;
                }
            }

            while (currentState != AI_State.Die)
            {
                int patternIndex = SelectPatternIndex();
                previousPatternIndex = patternIndex;

                switch (patternIndex)
                {
                    case 0:
                        yield return FireHomingShot();
                        break;
                    case 1:
                        yield return FireVolleyPattern();
                        break;
                    default:
                        FireRingPattern();
                        break;
                }

                if (currentState == AI_State.Die)
                {
                    yield break;
                }

                yield return new WaitForSeconds(interval);
            }
        }

        private int SelectPatternIndex()
        {
            int patternIndex = Random.Range(0, PatternCount);

            if (PatternCount > 1 && patternIndex == previousPatternIndex)
            {
                patternIndex = (patternIndex + 1 + Random.Range(0, PatternCount - 1)) % PatternCount;
            }

            return patternIndex;
        }

        private IEnumerator FireHomingShot()
        {
            FireProjectile(homingProjectilePrefab, GetDirectionToPlayer());
            yield break;
        }

        private IEnumerator FireVolleyPattern()
        {
            FireBurstTowardPlayer(3);
            yield return new WaitForSeconds(Mathf.Max(0.05f, volleyInterval));

            if (currentState == AI_State.Die)
            {
                yield break;
            }

            FireBurstTowardPlayer(4);
        }

        private void FireRingPattern()
        {
            float angleStep = 360.0f / RingShotCount;

            for (int i = 0; i < RingShotCount; i++)
            {
                float radians = (angleStep * i) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                FireProjectile(normalProjectilePrefab, direction);
            }
        }

        private void FireBurstTowardPlayer(int count)
        {
            if (count <= 0)
            {
                return;
            }

            Vector2 baseDirection = GetDirectionToPlayer();
            float totalSpread = Mathf.Max(0.0f, volleySpreadAngle) * Mathf.Max(0, count - 1);
            float startAngle = -totalSpread * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float angleOffset = startAngle + (volleySpreadAngle * i);
                Vector2 direction = RotateDirection(baseDirection, angleOffset);
                FireProjectile(normalProjectilePrefab, direction);
            }
        }

        private static Vector2 RotateDirection(Vector2 direction, float angleDegrees)
        {
            if (Mathf.Abs(angleDegrees) <= 0.0001f)
            {
                return direction;
            }

            float radians = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos).normalized;
        }

        private Vector2 GetDirectionToPlayer()
        {
            if (playerTransform == null)
            {
                return Vector2.right;
            }

            Vector2 direction = (Vector2)playerTransform.position - (Vector2)shotPosition.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.right;
            }

            return direction.normalized;
        }

        private void FireProjectile(GameObject prefab, Vector2 direction)
        {
            if (prefab == null || projectilePool == null || currentState == AI_State.Die)
            {
                return;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.right;
            }
            else
            {
                direction.Normalize();
            }

            Projectile projectile = projectilePool.Get(prefab);

            if (projectile == null)
            {
                return;
            }

            projectile.transform.position = shotPosition.position;
            projectile.Launch(
                direction,
                projectileSpeed,
                projectileRange,
                attackPower,
                projectilePool,
                gameObject.name,
                EnemyIgnoreTag);
        }
    }
}
