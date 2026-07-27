using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    public class TemporaryBoss : Enemy
    {
        private const string EnemyTag = "Enemy";
        private const float MinimumDirectionMagnitude = 0.0001f;

        private enum BossPattern
        {
            AimedBurst,
            RadialBurst,
            Charge,
        }

        [Header("Activation")]
        [SerializeField] private float activationRange = 10.0f;

        [Header("Pattern Timing")]
        [SerializeField] private float telegraphDuration = 0.6f;
        [SerializeField] private float patternCooldown = 1.0f;
        [SerializeField] private Color telegraphColor = Color.red;

        [Header("Projectile Patterns")]
        [SerializeField] private Transform shotPosition;
        [SerializeField] private GameObject enemyProjectilePrefab;
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private float shotSpeed = 4.5f;
        [SerializeField] private float shotRange = 20.0f;
        [SerializeField] private int aimedBurstCount = 3;
        [SerializeField] private float aimedBurstInterval = 0.18f;
        [SerializeField] private int radialProjectileCount = 12;

        [Header("Charge Pattern")]
        [SerializeField] private float chargeSpeed = 8.0f;
        [SerializeField] private float chargeDuration = 0.6f;

        private Transform playerTransform;
        private SpriteRenderer spriteRenderer;
        private Color defaultColor = Color.white;
        private BossPattern previousPattern;
        private bool hasPreviousPattern;
        private bool isPatternRunning;
        private bool isCharging;
        private bool isDeathHandled;
        private Vector2 chargeVelocity;

        protected override void Awake()
        {
            maxHp = 30.0f;
            attackPower = 1;
            base.Awake();
            SetState(AI_State.Idle);

            if (rigidbody2D == null)
            {
                Debug.LogError($"[{nameof(TemporaryBoss)}] Rigidbody2D is required on {gameObject.name}.", this);
            }

            if (shotPosition == null)
            {
                shotPosition = transform;
            }

            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                defaultColor = spriteRenderer.color;
            }

            if (enemyProjectilePrefab == null)
            {
                Debug.LogError($"[{nameof(TemporaryBoss)}] enemyProjectilePrefab is not assigned on {gameObject.name}.", this);
            }

            if (projectilePool == null)
            {
                projectilePool = FindFirstObjectByType<ProjectilePool>();
            }

            if (projectilePool == null)
            {
                Debug.LogError($"[{nameof(TemporaryBoss)}] ProjectilePool was not found.", this);
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject == null)
            {
                Debug.LogError($"[{nameof(TemporaryBoss)}] Player with tag 'Player' was not found.", this);
            }
            else
            {
                playerTransform = playerObject.transform;
            }
        }

        protected override void OnIdle()
        {
            if (playerTransform == null)
            {
                return;
            }

            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance <= activationRange)
            {
                SetState(AI_State.Attack);
            }
        }

        protected override void OnAttack()
        {
            if (playerTransform == null || isPatternRunning)
            {
                return;
            }

            StartCoroutine(RunNextPattern());
        }

        protected override void OnDie()
        {
            if (isDeathHandled)
            {
                return;
            }

            isDeathHandled = true;
            isCharging = false;
            StopAllCoroutines();

            if (spriteRenderer != null)
            {
                spriteRenderer.color = defaultColor;
            }
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

            rigidbody2D.linearVelocity = isCharging ? chargeVelocity : Vector2.zero;
        }

        private IEnumerator RunNextPattern()
        {
            isPatternRunning = true;
            BossPattern pattern = SelectNextPattern();

            yield return Telegraph();

            switch (pattern)
            {
                case BossPattern.AimedBurst:
                    yield return FireAimedBurst();
                    break;
                case BossPattern.RadialBurst:
                    FireRadialBurst();
                    break;
                case BossPattern.Charge:
                    yield return ChargeAtPlayer();
                    break;
            }

            previousPattern = pattern;
            hasPreviousPattern = true;
            yield return new WaitForSeconds(patternCooldown);
            isPatternRunning = false;
        }

        private BossPattern SelectNextPattern()
        {
            int patternCount = System.Enum.GetValues(typeof(BossPattern)).Length;
            BossPattern selectedPattern = (BossPattern)Random.Range(0, patternCount);

            if (!hasPreviousPattern || selectedPattern != previousPattern)
            {
                return selectedPattern;
            }

            int offset = Random.Range(1, patternCount);
            return (BossPattern)(((int)previousPattern + offset) % patternCount);
        }

        private IEnumerator Telegraph()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = telegraphColor;
            }

            yield return new WaitForSeconds(telegraphDuration);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = defaultColor;
            }
        }

        private IEnumerator FireAimedBurst()
        {
            int shotCount = Mathf.Max(1, aimedBurstCount);

            for (int i = 0; i < shotCount; i++)
            {
                if (playerTransform == null)
                {
                    yield break;
                }

                Vector2 direction = (Vector2)playerTransform.position - (Vector2)shotPosition.position;
                FireProjectile(direction);

                if (i < shotCount - 1)
                {
                    yield return new WaitForSeconds(aimedBurstInterval);
                }
            }
        }

        private void FireRadialBurst()
        {
            int projectileCount = Mathf.Max(1, radialProjectileCount);
            float angleStep = 360.0f / projectileCount;

            for (int i = 0; i < projectileCount; i++)
            {
                float angleRadians = i * angleStep * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
                FireProjectile(direction);
            }
        }

        private IEnumerator ChargeAtPlayer()
        {
            if (playerTransform == null || rigidbody2D == null)
            {
                yield break;
            }

            Vector2 direction = (Vector2)playerTransform.position - rigidbody2D.position;

            if (direction.sqrMagnitude <= MinimumDirectionMagnitude)
            {
                yield break;
            }

            chargeVelocity = direction.normalized * chargeSpeed;
            isCharging = true;
            yield return new WaitForSeconds(chargeDuration);
            isCharging = false;
            rigidbody2D.linearVelocity = Vector2.zero;
        }

        private void FireProjectile(Vector2 direction)
        {
            if (enemyProjectilePrefab == null
                || projectilePool == null
                || direction.sqrMagnitude <= MinimumDirectionMagnitude)
            {
                return;
            }

            Projectile projectile = projectilePool.Get(enemyProjectilePrefab);

            if (projectile == null)
            {
                return;
            }

            projectile.transform.position = shotPosition.position;
            projectile.Launch(
                direction.normalized,
                shotSpeed,
                shotRange,
                attackPower,
                projectilePool,
                gameObject.name,
                EnemyTag);
        }
    }
}
