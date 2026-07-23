using UnityEngine;

namespace DDARoguelike
{
    public enum AI_State
    {
        Idle,
        Roaming,
        Return,
        Chase,
        Attack,
        Die,
    }

    public abstract class Enemy : MonoBehaviour, IDamaged
    {
        private const float DestroyDelaySeconds = 0.5f;

        [SerializeField] protected float maxHp;
        [SerializeField] protected int attackPower;
        [SerializeField] private float knockbackForce = 2.0f;
        [SerializeField] private float knockbackDuration = 0.1f;

        private float currentHp;
        private bool isDead;
        protected Rigidbody2D rigidbody2D;
        private Vector2 knockbackVelocity;
        private float knockbackTimer;
        protected AI_State currentState;

        public float MaxHp => maxHp;
        public float CurrentHp => currentHp;
        public int AttackPower => attackPower;
        public AI_State CurrentState => currentState;

        protected virtual void Awake()
        {
            currentHp = maxHp;
            rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void ApplyKnockback(Vector2 projectileDirection)
        {
            if (isDead || rigidbody2D == null)
            {
                return;
            }

            if (projectileDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            knockbackVelocity = projectileDirection.normalized * knockbackForce;
            knockbackTimer = knockbackDuration;
        }

        protected bool TryApplyKnockbackMovement()
        {
            if (knockbackTimer <= 0.0f || rigidbody2D == null)
            {
                return false;
            }

            knockbackTimer -= Time.fixedDeltaTime;
            rigidbody2D.linearVelocity = knockbackVelocity;
            return true;
        }

        private void Update()
        {
            TickAI();
        }

        private void TickAI()
        {
            switch (currentState)
            {
                case AI_State.Idle:
                    OnIdle();
                    break;
                case AI_State.Roaming:
                    OnRoaming();
                    break;
                case AI_State.Return:
                    OnReturn();
                    break;
                case AI_State.Chase:
                    OnChase();
                    break;
                case AI_State.Attack:
                    OnAttack();
                    break;
                case AI_State.Die:
                    OnDie();
                    break;
            }
        }

        protected void SetState(AI_State newState)
        {
            if (currentState == newState)
            {
                return;
            }

            currentState = newState;
        }

        public void TakeDamage(int damage, string attackerName)
        {
            if (isDead)
            {
                return;
            }

            currentHp = Mathf.Max(0.0f, currentHp - damage);
            Debug.Log($"{attackerName} dealt {damage} damage to {gameObject.name}. Remaining HP: {currentHp}");

            if (currentHp <= 0.0f)
            {
                BeginDeath();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDead || currentState == AI_State.Die)
            {
                return;
            }

            if (!collision.collider.CompareTag("Player"))
            {
                return;
            }

            IDamaged damaged = collision.collider.GetComponent<IDamaged>();

            if (damaged == null)
            {
                damaged = collision.collider.GetComponentInParent<IDamaged>();
            }

            if (damaged != null)
            {
                damaged.TakeDamage(attackPower, gameObject.name);
            }
        }

        private void BeginDeath()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            currentHp = 0.0f;
            knockbackTimer = 0.0f;
            SetState(AI_State.Die);

            RoomController roomController = GetComponentInParent<RoomController>();

            if (roomController != null)
            {
                roomController.NotifyEnemyDied();
            }

            Collider2D[] colliders = GetComponents<Collider2D>();

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            Rigidbody2D deathRigidbody = rigidbody2D;

            if (deathRigidbody == null)
            {
                deathRigidbody = GetComponent<Rigidbody2D>();
            }

            if (deathRigidbody != null)
            {
                deathRigidbody.linearVelocity = Vector2.zero;
            }

            Destroy(gameObject, DestroyDelaySeconds);
        }

        protected virtual void OnIdle()
        {
        }

        protected virtual void OnRoaming()
        {
        }

        protected virtual void OnReturn()
        {
        }

        protected virtual void OnChase()
        {
        }

        protected virtual void OnAttack()
        {
        }

        protected virtual void OnDie()
        {
        }
    }
}
