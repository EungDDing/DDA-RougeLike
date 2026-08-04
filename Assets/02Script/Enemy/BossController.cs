using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    public abstract class BossController : Enemy
    {
        private const string PlayerTag = "Player";
        private const float PatternRetryDelay = 0.2f;

        [SerializeField] private BossData bossData;

        private readonly BossPatternSelector patternSelector = new BossPatternSelector();
        private Transform playerTransform;
        private IDamaged playerDamaged;
        private SpriteRenderer spriteRenderer;
        private Color defaultColor = Color.white;
        private BossContext context;
        private BossPattern previousPattern;
        private Vector2 movementVelocity;
        private Coroutine patternCoroutine;
        private bool isPatternRunning;
        private bool isDeathHandled;
        private bool contactDamageEnabled = true;

        public BossData Data => bossData;
        public string BossId => bossData != null ? bossData.BossId : gameObject.name;
        public Vector2 Position => rigidbody2D != null ? rigidbody2D.position : (Vector2)transform.position;

        protected override bool ReceivesSoftPush => false;

        protected override void Awake()
        {
            if (bossData != null)
            {
                maxHp = bossData.MaxHp;
                attackPower = bossData.AttackPower;
            }

            base.Awake();
            SetState(AI_State.Idle);

            if (bossData == null)
            {
                Debug.LogError($"[{nameof(BossController)}] BossData is not assigned on {gameObject.name}.", this);
            }

            if (rigidbody2D == null)
            {
                Debug.LogError($"[{nameof(BossController)}] Rigidbody2D is required on {gameObject.name}.", this);
            }

            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                defaultColor = spriteRenderer.color;
            }

            FindPlayer();
        }

        protected override void OnPreparedFromPool()
        {
            isDeathHandled = false;
            isPatternRunning = false;
            contactDamageEnabled = true;
            previousPattern = null;
            patternCoroutine = null;
            StopMovement();
            RestoreDefaultColor();
            SetState(AI_State.Idle);
        }

        protected override void OnIdle()
        {
            if (context == null || !context.IsValid || bossData == null)
            {
                return;
            }

            if (context.DistanceToPlayer <= bossData.ActivationRange)
            {
                SetState(AI_State.Attack);
            }
        }

        protected override void OnAttack()
        {
            if (context == null || !context.IsValid || bossData == null || isPatternRunning)
            {
                return;
            }

            patternCoroutine = StartCoroutine(RunNextPattern());
        }

        protected override void OnDie()
        {
            if (isDeathHandled)
            {
                return;
            }

            isDeathHandled = true;
            isPatternRunning = false;
            contactDamageEnabled = true;

            if (patternCoroutine != null)
            {
                StopCoroutine(patternCoroutine);
                patternCoroutine = null;
            }

            StopMovement();
            RestoreDefaultColor();
        }

        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            if (contactDamageEnabled)
            {
                base.OnCollisionEnter2D(collision);
            }
        }

        private void FixedUpdate()
        {
            if (TryApplyKnockbackMovement())
            {
                return;
            }

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = currentState == AI_State.Die
                    ? Vector2.zero
                    : movementVelocity;
            }
        }

        public void SetMovement(Vector2 velocity)
        {
            movementVelocity = velocity;
        }

        public void StopMovement()
        {
            movementVelocity = Vector2.zero;

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
            }
        }

        public void SetContactDamageEnabled(bool enabled)
        {
            contactDamageEnabled = enabled;
        }

        public IEnumerator PlayTelegraph(float duration, Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(Mathf.Max(0.0f, duration));
            RestoreDefaultColor();
        }

        public IEnumerator PlayArcTelegraph(
            Vector2 direction,
            float radius,
            float arcAngle,
            float duration,
            Color color)
        {
            BossAttackIndicator indicator = BossAttackIndicator.CreateArc(
                transform,
                direction,
                radius,
                arcAngle,
                color,
                bossData.IndicatorLineWidth);

            yield return PlayTelegraph(duration, color);
            DestroyIndicator(indicator);
        }

        public IEnumerator PlayCircleTelegraph(float radius, float duration, Color color)
        {
            BossAttackIndicator indicator = BossAttackIndicator.CreateCircle(
                transform,
                radius,
                color,
                bossData.IndicatorLineWidth);

            yield return PlayTelegraph(duration, color);
            DestroyIndicator(indicator);
        }

        public IEnumerator PlayLineTelegraph(
            Vector2 direction,
            float length,
            float lineWidth,
            float duration,
            Color color)
        {
            BossAttackIndicator indicator = BossAttackIndicator.CreateLine(
                transform,
                direction,
                length,
                color,
                lineWidth);

            yield return PlayTelegraph(duration, color);
            DestroyIndicator(indicator);
        }

        public void ShowArcImpact(Vector2 direction, float radius, float arcAngle)
        {
            BossAttackIndicator indicator = BossAttackIndicator.CreateArc(
                transform,
                direction,
                radius,
                arcAngle,
                bossData.ImpactEffectColor,
                bossData.IndicatorLineWidth * 2.5f);
            DestroyIndicatorAfterDelay(indicator);
        }

        public void ShowCircleImpact(float radius)
        {
            BossAttackIndicator indicator = BossAttackIndicator.CreateCircle(
                transform,
                radius,
                bossData.ImpactEffectColor,
                bossData.IndicatorLineWidth * 2.5f);
            DestroyIndicatorAfterDelay(indicator);
        }

        public bool TryDamagePlayerInCircle(float radius, float damageMultiplier)
        {
            if (context == null || !context.IsValid || playerDamaged == null)
            {
                return false;
            }

            if (context.DistanceToPlayer > Mathf.Max(0.0f, radius))
            {
                return false;
            }

            playerDamaged.TakeDamage(CalculatePatternDamage(damageMultiplier), gameObject.name);
            return true;
        }

        public bool TryDamagePlayerInArc(
            Vector2 direction,
            float radius,
            float arcAngle,
            float damageMultiplier)
        {
            if (context == null
                || !context.IsValid
                || playerDamaged == null
                || direction.sqrMagnitude <= 0.0001f
                || context.DistanceToPlayer > Mathf.Max(0.0f, radius))
            {
                return false;
            }

            Vector2 playerDirection = context.DirectionToPlayer;
            float halfAngle = Mathf.Clamp(arcAngle * 0.5f, 0.0f, 180.0f);
            float minimumDot = Mathf.Cos(halfAngle * Mathf.Deg2Rad);

            if (Vector2.Dot(direction.normalized, playerDirection) < minimumDot)
            {
                return false;
            }

            playerDamaged.TakeDamage(CalculatePatternDamage(damageMultiplier), gameObject.name);
            return true;
        }

        private IEnumerator RunNextPattern()
        {
            isPatternRunning = true;
            BossPattern pattern = patternSelector.Select(bossData.Patterns, context, previousPattern);

            if (pattern == null)
            {
                yield return new WaitForSeconds(PatternRetryDelay);
                isPatternRunning = false;
                patternCoroutine = null;
                yield break;
            }

            yield return pattern.Execute(context);

            StopMovement();
            SetContactDamageEnabled(true);
            RestoreDefaultColor();
            previousPattern = pattern;

            float interval = bossData.MinimumPatternInterval + pattern.RecoveryDuration;
            yield return new WaitForSeconds(interval);
            isPatternRunning = false;
            patternCoroutine = null;
        }

        private void FindPlayer()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);

            if (playerObject == null)
            {
                Debug.LogError($"[{nameof(BossController)}] Player with tag '{PlayerTag}' was not found.", this);
                return;
            }

            playerTransform = playerObject.transform;
            playerDamaged = playerObject.GetComponent<IDamaged>();

            if (playerDamaged == null)
            {
                playerDamaged = playerObject.GetComponentInParent<IDamaged>();
            }

            if (playerDamaged == null)
            {
                Debug.LogError($"[{nameof(BossController)}] Player requires an {nameof(IDamaged)} component.", this);
            }

            context = new BossContext(this, playerTransform);
        }

        private int CalculatePatternDamage(float damageMultiplier)
        {
            return Mathf.Max(1, Mathf.RoundToInt(attackPower * Mathf.Max(0.0f, damageMultiplier)));
        }

        private void DestroyIndicator(BossAttackIndicator indicator)
        {
            if (indicator != null)
            {
                Destroy(indicator.gameObject);
            }
        }

        private void DestroyIndicatorAfterDelay(BossAttackIndicator indicator)
        {
            if (indicator != null)
            {
                Destroy(indicator.gameObject, bossData.ImpactEffectDuration);
            }
        }

        private void RestoreDefaultColor()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = defaultColor;
            }
        }
    }
}
