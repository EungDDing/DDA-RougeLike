using UnityEngine;

namespace DDARoguelike
{
    public class RandomMovingEnemy : Enemy
    {
        private const string PlayerTag = "Player";

        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float directionChangeInterval = 1.5f;
        [SerializeField] private float pauseDuration = 0.5f;

        private Vector2 moveDirection;
        private float stateEndTime;
        private bool isPaused;

        protected override void Awake()
        {
            base.Awake();
            SetState(AI_State.Roaming);

            if (rigidbody2D == null)
            {
                Debug.LogError($"[{nameof(RandomMovingEnemy)}] Rigidbody2D is required on {gameObject.name}.", this);
            }

            BeginMoving();
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

        private void BeginMoving()
        {
            float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
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
