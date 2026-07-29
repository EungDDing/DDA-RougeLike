using UnityEngine;

namespace DDARoguelike
{
    public class RandomMovingShotEnemy : Enemy
    {
        private const string PlayerTag = "Player";
        private const string EnemyTag = "Enemy";

        private static readonly Vector2[] CardinalDirections = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right,
        };

        [SerializeField] private float attackRange = 6.0f;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float directionChangeInterval = 1.5f;
        [SerializeField] private float pauseDuration = 0.3f;
        [SerializeField] private float fireRate = 1.0f;
        [SerializeField] private float shotSpeed = 3.0f;
        [SerializeField] private Transform shotPosition;
        [SerializeField] private GameObject enemyProjectilePrefab;
        [SerializeField] private ProjectilePool projectilePool;

        private Transform playerTransform;
        private Vector2 moveDirection;
        private float stateEndTime;
        private bool isPaused;
        private float nextFireTime;

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
            SetState(AI_State.Roaming);

            if (rigidbody2D == null)
            {
                Debug.LogError($"[{nameof(RandomMovingShotEnemy)}] Rigidbody2D is required on {gameObject.name}.", this);
            }

            if (shotPosition == null)
            {
                shotPosition = transform;
            }

            if (enemyProjectilePrefab == null)
            {
                Debug.LogError($"[{nameof(RandomMovingShotEnemy)}] enemyProjectilePrefab is not assigned on {gameObject.name}.", this);
            }

            if (projectilePool == null)
            {
                projectilePool = FindFirstObjectByType<ProjectilePool>();
            }

            if (projectilePool == null)
            {
                Debug.LogError($"[{nameof(RandomMovingShotEnemy)}] projectilePool is not assigned on {gameObject.name}.", this);
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);

            if (playerObject == null)
            {
                Debug.LogError($"[{nameof(RandomMovingShotEnemy)}] Player with tag '{PlayerTag}' was not found.", this);
            }
            else
            {
                playerTransform = playerObject.transform;
            }

            BeginMoving();
        }

        protected override void OnPreparedFromPool()
        {
            SetState(AI_State.Roaming);
            nextFireTime = 0.0f;

            if (projectilePool == null)
            {
                projectilePool = FindFirstObjectByType<ProjectilePool>();
            }

            if (playerTransform == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);

                if (playerObject != null)
                {
                    playerTransform = playerObject.transform;
                }
            }

            BeginMoving();
        }

        protected override void OnRoaming()
        {
            TryShoot();
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

            if (currentState == AI_State.Die)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
                return;
            }

            if (currentState != AI_State.Roaming)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
                return;
            }

            if (Time.time >= stateEndTime)
            {
                if (isPaused)
                {
                    BeginMoving();
                }
                else
                {
                    BeginPause();
                }
            }

            if (isPaused)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
            }
            else
            {
                rigidbody2D.linearVelocity = moveDirection * moveSpeed;
            }
        }

        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            base.OnCollisionEnter2D(collision);

            if (currentState == AI_State.Die)
            {
                return;
            }

            if (collision.collider.CompareTag(PlayerTag))
            {
                return;
            }

            BeginPause();
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

            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance > attackRange)
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
                attackRange,
                attackPower,
                projectilePool,
                gameObject.name,
                EnemyTag);
        }

        private void BeginMoving()
        {
            int index = Random.Range(0, CardinalDirections.Length);
            moveDirection = CardinalDirections[index];
            isPaused = false;
            stateEndTime = Time.time + directionChangeInterval;
        }

        private void BeginPause()
        {
            isPaused = true;
            stateEndTime = Time.time + pauseDuration;

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
            }
        }
    }
}
