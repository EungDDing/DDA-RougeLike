using UnityEngine;

namespace DDARoguelike
{
    public enum FixedShotDirection
    {
        Left,
        Right,
    }

    public class FixedShotEnemy : Enemy
    {
        private const string EnemyTag = "Enemy";

        [SerializeField] private float attackRange = 6.0f;
        [SerializeField] private FixedShotDirection shotDirection = FixedShotDirection.Left;
        [SerializeField] private float fireRate = 1.0f;
        [SerializeField] private float shotSpeed = 3.0f;
        [SerializeField] private Transform shotPosition;
        [SerializeField] private GameObject enemyProjectilePrefab;
        [SerializeField] private ProjectilePool projectilePool;

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
            SetState(AI_State.Attack);

            if (rigidbody2D == null)
            {
                Debug.LogError($"[{nameof(FixedShotEnemy)}] Rigidbody2D is required on {gameObject.name}.", this);
            }

            if (shotPosition == null)
            {
                shotPosition = transform;
            }

            if (enemyProjectilePrefab == null)
            {
                Debug.LogError($"[{nameof(FixedShotEnemy)}] enemyProjectilePrefab is not assigned on {gameObject.name}.", this);
            }

            if (projectilePool == null)
            {
                Debug.LogError($"[{nameof(FixedShotEnemy)}] projectilePool is not assigned on {gameObject.name}.", this);
            }
        }

        protected override void OnAttack()
        {
            TryShoot();
        }

        private void TryShoot()
        {
            if (enemyProjectilePrefab == null || projectilePool == null)
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

            Vector2 direction = shotDirection == FixedShotDirection.Right
                ? Vector2.right
                : Vector2.left;

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

            rigidbody2D.linearVelocity = Vector2.zero;
        }
    }
}
