using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    public sealed class ThirdBoss : BossController
    {
        private const string WallTag = "Wall";
        private const int PatternCount = 2;
        private const float ActiveMass = 6.0f;
        private const float RestMass = 4.0f;

        private enum CombatPhase
        {
            BetweenPatterns,
            Crouch,
            Dash,
            Chase,
            Tired,
        }

        [SerializeField] private float betweenPatternDuration = 2.0f;
        [SerializeField] private float crouchDuration = 0.75f;
        [SerializeField] private float dashDuration = 2.0f;
        [SerializeField] private float chaseDuration = 3.0f;
        [SerializeField] private float tiredDuration = 1.0f;
        [SerializeField] private float dashSpeed = 10.0f;
        [SerializeField] private float chaseSpeed = 5.5f;
        [SerializeField] private float crouchScaleYMultiplier = 0.7f;

        private bool hasStartedCombatLoop;
        private Coroutine patternLoopCoroutine;
        private CombatPhase currentPhase = CombatPhase.BetweenPatterns;
        private Vector2 dashDirection = Vector2.left;
        private int previousPatternIndex = -1;
        private float baseScaleX = 1.0f;
        private float baseScaleY = 1.0f;
        private float baseScaleZ = 1.0f;
        private float facingSign = 1.0f;
        private bool isCrouching;

        protected override void Awake()
        {
            base.Awake();
            CacheBaseScale();
            ApplyMassForPhase(CombatPhase.BetweenPatterns);
            EnsurePlayerReference();
        }

        protected override void OnPreparedFromPool()
        {
            base.OnPreparedFromPool();
            hasStartedCombatLoop = false;
            previousPatternIndex = -1;
            patternLoopCoroutine = null;
            dashDirection = Vector2.left;
            isCrouching = false;
            CacheBaseScale();
            SetPhase(CombatPhase.BetweenPatterns);
            EnsurePlayerReference();
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
            UpdatePhaseMovement();
        }

        protected override void OnDie()
        {
            hasStartedCombatLoop = false;

            if (patternLoopCoroutine != null)
            {
                StopCoroutine(patternLoopCoroutine);
                patternLoopCoroutine = null;
            }

            isCrouching = false;
            ApplyCurrentScale();
            SetPhase(CombatPhase.BetweenPatterns);
            StopMovement();
            base.OnDie();
        }

        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            base.OnCollisionEnter2D(collision);
            TryBounceOffWall(collision);
        }

        protected override void OnCollisionStay2D(Collision2D collision)
        {
            base.OnCollisionStay2D(collision);
            TryBounceOffWall(collision);
        }

        private void TryBounceOffWall(Collision2D collision)
        {
            if (currentState == AI_State.Die || currentPhase != CombatPhase.Dash)
            {
                return;
            }

            if (collision == null || collision.collider == null)
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

            if (!TryGetMostOpposingWallNormal(collision, out Vector2 wallNormal))
            {
                return;
            }

            Vector2 reflected = Vector2.Reflect(dashDirection, wallNormal);

            if (reflected.sqrMagnitude <= 0.0001f)
            {
                reflected = -dashDirection;
            }

            dashDirection = reflected.normalized;

            // Leave the wall slightly so multi-body colliders do not keep scraping.
            if (rigidbody2D != null)
            {
                rigidbody2D.position += wallNormal * 0.05f;
            }

            FaceHorizontal(dashDirection.x);
            SetMovement(dashDirection * Mathf.Max(0.0f, dashSpeed));
        }

        private bool TryGetMostOpposingWallNormal(Collision2D collision, out Vector2 wallNormal)
        {
            wallNormal = Vector2.zero;
            float mostNegativeDot = 0.0f;
            bool found = false;

            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector2 normal = collision.GetContact(i).normal;

                if (normal.sqrMagnitude <= 0.0001f)
                {
                    continue;
                }

                normal.Normalize();
                float intoWallDot = Vector2.Dot(dashDirection, normal);

                // Only bounce when still moving into the wall (same idea as FirstStageBoss).
                if (intoWallDot >= -0.01f)
                {
                    continue;
                }

                if (!found || intoWallDot < mostNegativeDot)
                {
                    found = true;
                    mostNegativeDot = intoWallDot;
                    wallNormal = normal;
                }
            }

            return found;
        }

        private void BeginCombatLoop()
        {
            if (hasStartedCombatLoop || currentState == AI_State.Die)
            {
                return;
            }

            hasStartedCombatLoop = true;
            SetState(AI_State.Attack);
            EnsurePlayerReference();

            if (patternLoopCoroutine != null)
            {
                StopCoroutine(patternLoopCoroutine);
            }

            patternLoopCoroutine = StartCoroutine(PatternLoop());
        }

        private IEnumerator PatternLoop()
        {
            while (currentState != AI_State.Die)
            {
                yield return RunBetweenPatterns();

                if (currentState == AI_State.Die)
                {
                    yield break;
                }

                int patternIndex = SelectPatternIndex();
                previousPatternIndex = patternIndex;

                if (patternIndex == 0)
                {
                    yield return RunCrouchDashPattern();
                }
                else
                {
                    yield return RunChaseTiredPattern();
                }
            }
        }

        private IEnumerator RunBetweenPatterns()
        {
            SetPhase(CombatPhase.BetweenPatterns);
            isCrouching = false;
            ApplyCurrentScale();
            StopMovement();
            yield return new WaitForSeconds(Mathf.Max(0.0f, betweenPatternDuration));
        }

        private IEnumerator RunCrouchDashPattern()
        {
            SetPhase(CombatPhase.Crouch);
            StopMovement();
            isCrouching = true;
            ApplyCurrentScale();
            yield return new WaitForSeconds(Mathf.Max(0.0f, crouchDuration));

            if (currentState == AI_State.Die)
            {
                yield break;
            }

            isCrouching = false;
            ApplyCurrentScale();
            SetPhase(CombatPhase.Dash);
            dashDirection = GetDirectionToPlayer();
            FaceHorizontal(dashDirection.x);
            SetMovement(dashDirection * Mathf.Max(0.0f, dashSpeed));
            yield return new WaitForSeconds(Mathf.Max(0.0f, dashDuration));

            SetPhase(CombatPhase.BetweenPatterns);
            StopMovement();
        }

        private IEnumerator RunChaseTiredPattern()
        {
            SetPhase(CombatPhase.Chase);
            isCrouching = false;
            ApplyCurrentScale();
            yield return new WaitForSeconds(Mathf.Max(0.0f, chaseDuration));

            if (currentState == AI_State.Die)
            {
                yield break;
            }

            SetPhase(CombatPhase.Tired);
            StopMovement();
            yield return new WaitForSeconds(Mathf.Max(0.0f, tiredDuration));

            SetPhase(CombatPhase.BetweenPatterns);
            StopMovement();
        }

        private void SetPhase(CombatPhase phase)
        {
            currentPhase = phase;
            ApplyMassForPhase(phase);
        }

        private void ApplyMassForPhase(CombatPhase phase)
        {
            if (rigidbody2D == null)
            {
                return;
            }

            bool isActive = phase == CombatPhase.Dash || phase == CombatPhase.Chase;
            rigidbody2D.mass = isActive ? ActiveMass : RestMass;
        }

        private void UpdatePhaseMovement()
        {
            if (currentState == AI_State.Die)
            {
                StopMovement();
                return;
            }

            switch (currentPhase)
            {
                case CombatPhase.Dash:
                    SetMovement(dashDirection * Mathf.Max(0.0f, dashSpeed));
                    break;
                case CombatPhase.Chase:
                    Vector2 chaseDirection = GetDirectionToPlayer();
                    FaceHorizontal(chaseDirection.x);
                    SetMovement(chaseDirection * Mathf.Max(0.0f, chaseSpeed));
                    break;
                default:
                    StopMovement();
                    break;
            }
        }

        private void UpdateFacing()
        {
            if (currentState == AI_State.Die)
            {
                return;
            }

            if (currentPhase == CombatPhase.Dash)
            {
                FaceHorizontal(dashDirection.x);
                return;
            }

            if (playerTransform == null)
            {
                return;
            }

            float deltaX = playerTransform.position.x - transform.position.x;
            FaceHorizontal(deltaX);
        }

        private void FaceHorizontal(float directionX)
        {
            if (Mathf.Abs(directionX) <= 0.05f)
            {
                return;
            }

            // Default art faces -X (left). +X dash/look flips to face right.
            float targetSign = directionX > 0.0f ? -1.0f : 1.0f;

            if (Mathf.Abs(facingSign - targetSign) <= 0.0001f)
            {
                return;
            }

            facingSign = targetSign;
            ApplyCurrentScale();
        }

        private void CacheBaseScale()
        {
            transform.rotation = Quaternion.identity;
            Vector3 scale = transform.localScale;
            baseScaleX = Mathf.Abs(scale.x);

            if (baseScaleX <= 0.0001f)
            {
                baseScaleX = 1.0f;
            }

            baseScaleY = scale.y;
            baseScaleZ = scale.z;
            facingSign = scale.x < 0.0f ? -1.0f : 1.0f;
            ApplyCurrentScale();
        }

        private void ApplyCurrentScale()
        {
            float multiplier = Mathf.Clamp(crouchScaleYMultiplier, 0.2f, 1.0f);
            float scaleY = isCrouching ? baseScaleY * multiplier : baseScaleY;
            transform.localScale = new Vector3(baseScaleX * facingSign, scaleY, baseScaleZ);
        }

        private Vector2 GetDirectionToPlayer()
        {
            if (playerTransform == null)
            {
                EnsurePlayerReference();
            }

            if (playerTransform == null)
            {
                return Vector2.left;
            }

            Vector2 direction = (Vector2)playerTransform.position - (Vector2)transform.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.left;
            }

            return direction.normalized;
        }

        private int SelectPatternIndex()
        {
            int patternIndex = Random.Range(0, PatternCount);

            if (PatternCount > 1 && patternIndex == previousPatternIndex)
            {
                patternIndex = (patternIndex + 1) % PatternCount;
            }

            return patternIndex;
        }
    }
}
