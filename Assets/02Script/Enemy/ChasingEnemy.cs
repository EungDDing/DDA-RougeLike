using UnityEngine;

namespace DDARoguelike
{
    public class ChasingEnemy : Enemy
    {
        [SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private float obstacleCastRadius = 0.35f;
        [SerializeField] private float obstacleLookAhead = 0.8f;
        [SerializeField] private float avoidanceStickBias = 0.35f;
        [SerializeField] private float turnRadiansPerSecond = 8.0f;

        private Transform playerTransform;
        private Vector2 smoothedMoveDirection;

        protected override void Awake()
        {
            if (maxHp <= 0.0f)
            {
                maxHp = 20.0f;
            }

            if (attackPower <= 0)
            {
                attackPower = 1;
            }

            base.Awake();
            SetState(AI_State.Chase);

            if (rigidbody2D == null)
            {
                Debug.LogError($"[{nameof(ChasingEnemy)}] Rigidbody2D is required on {gameObject.name}.", this);
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject == null)
            {
                Debug.LogError($"[{nameof(ChasingEnemy)}] Player with tag 'Player' was not found.", this);
            }
            else
            {
                playerTransform = playerObject.transform;
            }
        }

        protected override void OnPreparedFromPool()
        {
            SetState(AI_State.Chase);
            smoothedMoveDirection = Vector2.zero;

            if (playerTransform == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                {
                    playerTransform = playerObject.transform;
                }
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

            if (currentState != AI_State.Chase || playerTransform == null)
            {
                smoothedMoveDirection = Vector2.zero;
                rigidbody2D.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 direction = (Vector2)playerTransform.position - rigidbody2D.position;

            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
                Vector2 steered = ObstacleAvoidanceSteering.Resolve(
                    rigidbody2D.position,
                    direction,
                    playerTransform.position,
                    obstacleCastRadius,
                    obstacleLookAhead,
                    smoothedMoveDirection,
                    avoidanceStickBias);
                smoothedMoveDirection = ObstacleAvoidanceSteering.SmoothDirection(
                    smoothedMoveDirection,
                    steered,
                    turnRadiansPerSecond,
                    Time.fixedDeltaTime);
                rigidbody2D.linearVelocity = smoothedMoveDirection * moveSpeed;
            }
            else
            {
                smoothedMoveDirection = Vector2.zero;
                rigidbody2D.linearVelocity = Vector2.zero;
            }
        }
    }
}
