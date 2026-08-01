using UnityEngine;

namespace DDARoguelike
{
    public class FireballSkill : PlayerSkill
    {
        private const int OverlapBufferSize = 32;
        private const string PlayerAttackerName = "Player";
        private const string PlayerIgnoreTag = "Player";

        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private Transform shotPosition;
        [SerializeField] private float detectRadius = 8.0f;
        [SerializeField] private float speed = 6.0f;
        [SerializeField] private float maxRange = 10.0f;
        [SerializeField] private float multiShotAngleDegrees = 15.0f;

        private readonly Collider2D[] overlapBuffer = new Collider2D[OverlapBufferSize];

        private void Awake()
        {
            if (fireballPrefab == null)
            {
                Debug.LogError($"[{nameof(FireballSkill)}] fireballPrefab is not assigned on {gameObject.name}.", this);
            }

            if (projectilePool == null)
            {
                Debug.LogError($"[{nameof(FireballSkill)}] projectilePool is not assigned on {gameObject.name}.", this);
            }

            if (shotPosition == null)
            {
                shotPosition = transform;
            }
        }

        protected override bool ActivateSkill()
        {
            if (fireballPrefab == null || projectilePool == null || shotPosition == null)
            {
                return false;
            }

            Vector2 fireDirection = ResolveFireDirection();
            int shotCount = SkillProjectileCount;
            int firedCount = 0;

            for (int i = 0; i < shotCount; i++)
            {
                Projectile projectile = projectilePool.Get(fireballPrefab);

                if (projectile == null)
                {
                    continue;
                }

                float angleOffset = (i - (shotCount - 1) * 0.5f) * multiShotAngleDegrees;
                Vector2 shotDirection = RotateDirection(fireDirection, angleOffset);

                projectile.transform.position = shotPosition.position;
                projectile.Launch(
                    shotDirection,
                    speed,
                    maxRange,
                    SkillDamage,
                    projectilePool,
                    PlayerAttackerName,
                    PlayerIgnoreTag);
                firedCount++;
            }

            return firedCount > 0;
        }

        private Vector2 ResolveFireDirection()
        {
            Vector2 origin = shotPosition.position;

            if (TryGetNearestEnemyDirection(origin, out Vector2 enemyDirection))
            {
                return enemyDirection;
            }

            return LastAimDirection;
        }

        private bool TryGetNearestEnemyDirection(Vector2 origin, out Vector2 direction)
        {
            direction = Vector2.zero;
            float radius = Mathf.Max(0.0f, detectRadius);

            if (radius <= 0.0f)
            {
                return false;
            }

            int hitCount = Physics2D.OverlapCircleNonAlloc(origin, radius, overlapBuffer);

            float nearestDistanceSquared = float.MaxValue;
            Vector2 nearestDirection = Vector2.zero;
            bool foundEnemy = false;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = overlapBuffer[i];

                if (hit == null)
                {
                    continue;
                }

                Enemy enemy = hit.GetComponent<Enemy>();

                if (enemy == null)
                {
                    enemy = hit.GetComponentInParent<Enemy>();
                }

                if (enemy == null || !enemy.isActiveAndEnabled || enemy.CurrentState == AI_State.Die)
                {
                    continue;
                }

                Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
                float distanceSquared = toEnemy.sqrMagnitude;

                if (distanceSquared <= 0.0001f || distanceSquared >= nearestDistanceSquared)
                {
                    continue;
                }

                nearestDistanceSquared = distanceSquared;
                nearestDirection = toEnemy.normalized;
                foundEnemy = true;
            }

            if (!foundEnemy)
            {
                return false;
            }

            direction = nearestDirection;
            return true;
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
    }
}
