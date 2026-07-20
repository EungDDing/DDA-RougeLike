using UnityEngine;

namespace DDARoguelike
{
    public class ChasingEnemy : Enemy
    {
        [SerializeField] private float moveSpeed = 3.0f;

        private Rigidbody2D rigidbody2D;
        private Transform playerTransform;

        protected override void Awake()
        {
            maxHp = 20.0f;
            attackPower = 0.5f;
            base.Awake();
            SetState(AI_State.Chase);

            rigidbody2D = GetComponent<Rigidbody2D>();

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

        private void FixedUpdate()
        {
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
