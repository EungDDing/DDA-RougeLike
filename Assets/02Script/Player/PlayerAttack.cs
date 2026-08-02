using System;
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
        [SerializeField] private float critChance = 0.0f;
        [SerializeField] private float critDamage = 1.0f;
        [SerializeField] private float multiShotAngleDegrees = 15.0f;
        [SerializeField] private Transform shotPosition;
        [SerializeField] private GameObject playerProjectilePrefab;
        [SerializeField] private ProjectilePool projectilePool;

        private float nextFireTime;
        private bool isCombatInputEnabled = true;

        public float AttackPower => attackPower;
        public float FireRate => fireRate;
        public float AttackRange => attackRange;
        public int ProjectileCount => projectileCount;
        public float ShotSpeed => shotSpeed;
        public float CritChance => critChance;
        public float CritDamage => critDamage;

        public event Action StatsChanged;
        public event Action<int> ShotsFired;

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

        public void SetCombatInputEnabled(bool isEnabled)
        {
            isCombatInputEnabled = isEnabled;
        }

        public void MultiplyAttackPower(float factor)
        {
            if (factor == 1.0f)
            {
                return;
            }

            attackPower = Mathf.Max(0.0f, attackPower * factor);
            Debug.Log($"AttackPower: {attackPower}");
            NotifyStatsChanged();
        }

        public void MultiplyFireInterval(float intervalFactor, float minIntervalSeconds)
        {
            if (fireRate <= 0.0f || intervalFactor == 1.0f)
            {
                return;
            }

            float interval = 1.0f / fireRate;
            float clampedFactor = Mathf.Max(0.01f, intervalFactor);
            interval = Mathf.Max(minIntervalSeconds, interval * clampedFactor);
            fireRate = 1.0f / interval;
            Debug.Log($"FireRate: {fireRate}");
            NotifyStatsChanged();
        }

        public void MultiplyAttackRange(float factor)
        {
            if (factor == 1.0f)
            {
                return;
            }

            attackRange = Mathf.Max(0.0f, attackRange * factor);
            Debug.Log($"AttackRange: {attackRange}");
            NotifyStatsChanged();
        }

        public void MultiplyShotSpeed(float factor)
        {
            if (factor == 1.0f)
            {
                return;
            }

            shotSpeed = Mathf.Max(0.0f, shotSpeed * factor);
            Debug.Log($"ShotSpeed: {shotSpeed}");
            NotifyStatsChanged();
        }

        public void MultiplyCritDamage(float factor)
        {
            if (factor == 1.0f)
            {
                return;
            }

            critDamage = Mathf.Max(0.0f, critDamage * factor);
            Debug.Log($"CritDamage: {critDamage}");
            NotifyStatsChanged();
        }

        private void Update()
        {
            if (!isCombatInputEnabled)
            {
                return;
            }

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
            NotifyStatsChanged();
        }

        public void AddAttackRange(float amount)
        {
            attackRange += amount;
            Debug.Log($"AttackRange: {attackRange}");
            NotifyStatsChanged();
        }

        public void AddFireRate(float amount)
        {
            fireRate += amount;
            Debug.Log($"FireRate: {fireRate}");
            NotifyStatsChanged();
        }

        public void AddShotSpeed(float amount)
        {
            shotSpeed += amount;
            Debug.Log($"ShotSpeed: {shotSpeed}");
            NotifyStatsChanged();
        }

        public void AddProjectileCount(int amount)
        {
            if (amount == 0)
            {
                return;
            }

            projectileCount = Mathf.Max(1, projectileCount + amount);
            Debug.Log($"ProjectileCount: {projectileCount}");
            NotifyStatsChanged();
        }

        public void AddCritChance(float amount)
        {
            if (amount == 0.0f)
            {
                return;
            }

            critChance = Mathf.Clamp01(critChance + amount);
            Debug.Log($"CritChance: {critChance}");
            NotifyStatsChanged();
        }

        public void AddCritDamage(float amount)
        {
            if (amount == 0.0f)
            {
                return;
            }

            critDamage = Mathf.Max(0.0f, critDamage + amount);
            Debug.Log($"CritDamage: {critDamage}");
            NotifyStatsChanged();
        }

        private void NotifyStatsChanged()
        {
            if (StatsChanged != null)
            {
                StatsChanged.Invoke();
            }
        }

        private void Fire(Vector2 direction)
        {
            int firedProjectileCount = 0;

            for (int i = 0; i < projectileCount; i++)
            {
                Projectile projectile = projectilePool.Get(playerProjectilePrefab);

                if (projectile == null)
                {
                    continue;
                }

                float angleOffset = (i - (projectileCount - 1) * 0.5f) * multiShotAngleDegrees;
                Vector2 shotDirection = RotateDirection(direction, angleOffset);
                float shotDamage = ResolveShotDamage();

                projectile.transform.position = shotPosition.position;
                projectile.Launch(shotDirection, shotSpeed, attackRange, shotDamage, projectilePool, "Player", "Player");
                firedProjectileCount++;
            }

            if (firedProjectileCount > 0)
            {
                ShotsFired?.Invoke(firedProjectileCount);
            }
        }

        private float ResolveShotDamage()
        {
            if (critChance > 0.0f && UnityEngine.Random.value < critChance)
            {
                return attackPower * critDamage;
            }

            return attackPower;
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
