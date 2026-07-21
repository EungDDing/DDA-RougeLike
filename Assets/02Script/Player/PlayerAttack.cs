using UnityEngine;
using UnityEngine.InputSystem;

namespace DDARoguelike
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private float attackPower = 3.0f;
        [SerializeField] private float fireRate = 1.0f;
        [SerializeField] private float attackRange = 6.0f;
        [SerializeField] private int projectileCount = 1;
        [SerializeField] private float shotSpeed = 3.0f;
        [SerializeField] private float multiShotAngleDegrees = 15.0f;
        [SerializeField] private Transform shotPosition;
        [SerializeField] private GameObject playerProjectilePrefab;
        [SerializeField] private ProjectilePool projectilePool;

        private float nextFireTime;

        public float AttackPower => attackPower;
        public float FireRate => fireRate;
        public float AttackRange => attackRange;
        public int ProjectileCount => projectileCount;
        public float ShotSpeed => shotSpeed;

        private void Awake()
        {
            if (shotPosition == null)
            {
                Debug.LogError($"[{nameof(PlayerAttack)}] shotPosition is not assigned on {gameObject.name}.", this);
            }

            if (playerProjectilePrefab == null)
            {
                Debug.LogError($"[{nameof(PlayerAttack)}] playerProjectilePrefab is not assigned on {gameObject.name}.", this);
            }

            if (projectilePool == null)
            {
                Debug.LogError($"[{nameof(PlayerAttack)}] projectilePool is not assigned on {gameObject.name}.", this);
            }
        }

        private void Update()
        {
            if (shotPosition == null || playerProjectilePrefab == null || projectilePool == null)
            {
                return;
            }

            if (fireRate <= 0.0f)
            {
                return;
            }

            Vector2 shootDirection = ReadShootDirection();

            if (shootDirection.sqrMagnitude <= 0.0f)
            {
                return;
            }

            if (Time.time < nextFireTime)
            {
                return;
            }

            nextFireTime = Time.time + 1.0f / fireRate;
            Fire(shootDirection.normalized);
        }

        public void AddAttackPower(float amount)
        {
            attackPower += amount;
            Debug.Log($"AttackPower: {attackPower}");
        }

        public void AddAttackRange(float amount)
        {
            attackRange += amount;
            Debug.Log($"AttackRange: {attackRange}");
        }

        public void AddFireRate(float amount)
        {
            fireRate += amount;
            Debug.Log($"FireRate: {fireRate}");
        }

        public void AddShotSpeed(float amount)
        {
            shotSpeed += amount;
            Debug.Log($"ShotSpeed: {shotSpeed}");
        }

        public void AddProjectileCount(int amount)
        {
            if (amount == 0)
            {
                return;
            }

            projectileCount = Mathf.Max(1, projectileCount + amount);
            Debug.Log($"ProjectileCount: {projectileCount}");
        }

        private void Fire(Vector2 direction)
        {
            for (int i = 0; i < projectileCount; i++)
            {
                Projectile projectile = projectilePool.Get(playerProjectilePrefab);

                if (projectile == null)
                {
                    continue;
                }

                float angleOffset = (i - (projectileCount - 1) * 0.5f) * multiShotAngleDegrees;
                Vector2 shotDirection = RotateDirection(direction, angleOffset);

                projectile.transform.position = shotPosition.position;
                projectile.Launch(shotDirection, shotSpeed, attackRange, attackPower, projectilePool, "Player", "Player");
            }
        }

        private static Vector2 RotateDirection(Vector2 direction, float angleDegrees)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos);
        }

        private Vector2 ReadShootDirection()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float horizontal = 0.0f;
            float vertical = 0.0f;

            if (keyboard.leftArrowKey.isPressed)
            {
                horizontal -= 1.0f;
            }

            if (keyboard.rightArrowKey.isPressed)
            {
                horizontal += 1.0f;
            }

            if (keyboard.downArrowKey.isPressed)
            {
                vertical -= 1.0f;
            }

            if (keyboard.upArrowKey.isPressed)
            {
                vertical += 1.0f;
            }

            return new Vector2(horizontal, vertical);
        }
    }
}
