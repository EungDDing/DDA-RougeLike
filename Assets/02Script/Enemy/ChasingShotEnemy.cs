using UnityEngine;

namespace DDARoguelike
{
    public class ChasingShotEnemy : Enemy
    {
        private const string EnemyTag = "Enemy";

        [SerializeField] private float chaseRange = 10.0f;
        [SerializeField] private float shotRange = 6.0f;
        [SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private float fireRate = 1.0f;
        [SerializeField] private float shotSpeed = 3.0f;
        [SerializeField] private Transform shotPosition;
        [SerializeField] private GameObject enemyProjectilePrefab;
        [SerializeField] private ProjectilePool projectilePool;

        private Rigidbody2D rigidbody2D;
        private Transform playerTransform;
        private float nextFireTime;

        protected override void Awake()
        {
            maxHp = 10.0f;
            attackPower = 1;
            base.Awake();
            SetState(AI_State.Chase);

            rigidbody2D = GetComponent<Rigidbody2D>();

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

            if (currentState != AI_State.Chase || playerTransform == null)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 direction = (Vector2)playerTransform.position - rigidbody2D.position;

            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
                rigidbody2D.linearVelocity = direction * moveSpeed;
            }
            else
            {
                rigidbody2D.linearVelocity = Vector2.zero;
            }
        }
    }
}
