using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    public sealed class FirstStageBoss : BossController
    {
        private const string WallTag = "Wall";
        private const int SummonOptionCount = 3;
        private const int SummonSpawnMaxAttempts = 16;
        private const int SummonFallbackAttempts = 8;
        private const float SummonClearanceRadius = 0.35f;
        private const float SummonFallbackMinDistance = 0.75f;

        [SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private float summonInterval = 5.0f;
        [SerializeField] private float summonSpawnMinDistance = 3.5f;
        [SerializeField] private float summonSpawnMaxDistance = 5.5f;
        [SerializeField] private GameObject flyingChasingPrefab;
        [SerializeField] private GameObject flyingRandomMovingPrefab;
        [SerializeField] private GameObject flyingChasingShotPrefab;
        [SerializeField] private EnemyPool enemyPool;

        private readonly Collider2D[] summonOverlapBuffer = new Collider2D[16];
        private readonly RaycastHit2D[] summonCastBuffer = new RaycastHit2D[16];
        private Vector2 bounceDirection;
        private bool hasStartedCombatLoop;
        private Coroutine summonCoroutine;

        protected override void Awake()
        {
            base.Awake();
            ResolveEnemyPool();
            ValidateSummonPrefabs();
            bounceDirection = new Vector2(1.0f, 1.0f).normalized;
        }

        protected override void OnPreparedFromPool()
        {
            base.OnPreparedFromPool();
            hasStartedCombatLoop = false;
            bounceDirection = new Vector2(1.0f, 1.0f).normalized;
            summonCoroutine = null;
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
            ApplyBounceMovement();
        }

        protected override void OnDie()
        {
            hasStartedCombatLoop = false;

            if (summonCoroutine != null)
            {
                StopCoroutine(summonCoroutine);
                summonCoroutine = null;
            }

            base.OnDie();
        }

        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            base.OnCollisionEnter2D(collision);

            if (currentState == AI_State.Die || collision == null || collision.collider == null)
            {
                return;
            }

            if (!collision.collider.CompareTag(WallTag))
            {
                return;
            }

            if (collision.contactCount <= 0)
            {
                return;
            }

            Vector2 normal = collision.GetContact(0).normal;
            Vector2 reflected = Vector2.Reflect(bounceDirection, normal);

            if (reflected.sqrMagnitude <= 0.0001f)
            {
                reflected = -bounceDirection;
            }

            bounceDirection = reflected.normalized;
            ApplyBounceMovement();
        }

        private void BeginCombatLoop()
        {
            if (hasStartedCombatLoop || currentState == AI_State.Die)
            {
                return;
            }

            hasStartedCombatLoop = true;
            SetState(AI_State.Attack);
            ApplyBounceMovement();

            if (summonCoroutine != null)
            {
                StopCoroutine(summonCoroutine);
            }

            summonCoroutine = StartCoroutine(SummonLoop());
        }

        private void ApplyBounceMovement()
        {
            if (currentState == AI_State.Die)
            {
                StopMovement();
                return;
            }

            float speed = Mathf.Max(0.0f, moveSpeed);
            SetMovement(bounceDirection * speed);
        }

        private IEnumerator SummonLoop()
        {
            float interval = Mathf.Max(0.1f, summonInterval);

            while (currentState != AI_State.Die)
            {
                yield return new WaitForSeconds(interval);

                if (currentState == AI_State.Die)
                {
                    yield break;
                }

                SummonRandomOption();
            }
        }

        private void SummonRandomOption()
        {
            int option = Random.Range(0, SummonOptionCount);

            switch (option)
            {
                case 0:
                    SpawnEnemies(flyingChasingPrefab, 2);
                    break;
                case 1:
                    SpawnEnemies(flyingRandomMovingPrefab, 4);
                    break;
                default:
                    SpawnEnemies(flyingChasingShotPrefab, 1);
                    break;
            }
        }

        private void SpawnEnemies(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
            {
                Debug.LogError($"[{nameof(FirstStageBoss)}] Summon prefab is missing on {gameObject.name}.", this);
                return;
            }

            ResolveEnemyPool();

            if (enemyPool == null)
            {
                Debug.LogError($"[{nameof(FirstStageBoss)}] {nameof(EnemyPool)} was not found for {gameObject.name}.", this);
                return;
            }

            RoomController roomController = GetComponentInParent<RoomController>();
            int spawnedCount = 0;

            for (int i = 0; i < count; i++)
            {
                Enemy enemy = enemyPool.Get(prefab);

                if (enemy == null)
                {
                    continue;
                }

                Transform parent = roomController != null ? roomController.transform : transform;
                Vector3 prefabScale = prefab.transform.localScale;
                Vector3 spawnPosition = ResolveSummonSpawnPosition();

                enemy.transform.SetParent(parent, false);
                enemy.transform.position = spawnPosition;
                enemy.transform.rotation = Quaternion.identity;
                ApplyWorldScale(enemy.transform, prefabScale);
                spawnedCount++;
            }

            if (roomController != null && spawnedCount > 0)
            {
                roomController.RegisterSpawnedEnemies(spawnedCount);
            }
        }

        private Vector3 ResolveSummonSpawnPosition()
        {
            Vector3 origin = transform.position;

            for (int attempt = 0; attempt < SummonSpawnMaxAttempts; attempt++)
            {
                Vector2 offset = GetSummonSpawnOffset();
                Vector3 candidate = origin + new Vector3(offset.x, offset.y, 0.0f);

                if (IsSummonPositionClear(candidate))
                {
                    return candidate;
                }
            }

            return GetFallbackSummonPosition(origin);
        }

        private Vector3 GetFallbackSummonPosition(Vector3 origin)
        {
            float maxDistance = Mathf.Max(summonSpawnMinDistance, summonSpawnMaxDistance);

            for (int attempt = 0; attempt < SummonFallbackAttempts; attempt++)
            {
                Vector2 direction = GetRandomUnitDirection();
                float placeDistance = maxDistance;
                int hitCount = Physics2D.CircleCastNonAlloc(
                    origin,
                    SummonClearanceRadius,
                    direction,
                    summonCastBuffer,
                    maxDistance);

                for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
                {
                    RaycastHit2D hit = summonCastBuffer[hitIndex];

                    if (hit.collider == null || !hit.collider.CompareTag(WallTag))
                    {
                        continue;
                    }

                    placeDistance = Mathf.Min(placeDistance, hit.distance);
                }

                placeDistance = Mathf.Max(SummonFallbackMinDistance, placeDistance - SummonClearanceRadius);
                Vector3 candidate = origin + new Vector3(direction.x, direction.y, 0.0f) * placeDistance;

                if (IsSummonPositionClear(candidate))
                {
                    return candidate;
                }
            }

            return origin;
        }

        private bool IsSummonPositionClear(Vector3 position)
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(
                position,
                SummonClearanceRadius,
                summonOverlapBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = summonOverlapBuffer[i];

                if (hit == null)
                {
                    continue;
                }

                if (hit.CompareTag(WallTag))
                {
                    return false;
                }
            }

            return true;
        }

        private Vector2 GetSummonSpawnOffset()
        {
            float minDistance = Mathf.Max(0.0f, summonSpawnMinDistance);
            float maxDistance = Mathf.Max(minDistance, summonSpawnMaxDistance);
            Vector2 direction = GetRandomUnitDirection();
            float distance = Random.Range(minDistance, maxDistance);
            return direction * distance;
        }

        private static Vector2 GetRandomUnitDirection()
        {
            Vector2 direction = Random.insideUnitCircle;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.right;
            }

            return direction.normalized;
        }

        private void ResolveEnemyPool()
        {
            if (enemyPool != null)
            {
                return;
            }

            enemyPool = FindFirstObjectByType<EnemyPool>();
        }

        private void ValidateSummonPrefabs()
        {
            if (flyingChasingPrefab == null)
            {
                Debug.LogError($"[{nameof(FirstStageBoss)}] flyingChasingPrefab is not assigned on {gameObject.name}.", this);
            }

            if (flyingRandomMovingPrefab == null)
            {
                Debug.LogError($"[{nameof(FirstStageBoss)}] flyingRandomMovingPrefab is not assigned on {gameObject.name}.", this);
            }

            if (flyingChasingShotPrefab == null)
            {
                Debug.LogError($"[{nameof(FirstStageBoss)}] flyingChasingShotPrefab is not assigned on {gameObject.name}.", this);
            }
        }

        private static void ApplyWorldScale(Transform target, Vector3 worldScale)
        {
            if (target == null)
            {
                return;
            }

            Transform parent = target.parent;

            if (parent == null)
            {
                target.localScale = worldScale;
                return;
            }

            Vector3 parentScale = parent.lossyScale;
            float scaleX = Mathf.Approximately(parentScale.x, 0.0f) ? worldScale.x : worldScale.x / parentScale.x;
            float scaleY = Mathf.Approximately(parentScale.y, 0.0f) ? worldScale.y : worldScale.y / parentScale.y;
            float scaleZ = Mathf.Approximately(parentScale.z, 0.0f) ? worldScale.z : worldScale.z / parentScale.z;
            target.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }
    }
}
