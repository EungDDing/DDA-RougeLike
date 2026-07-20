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

        private float currentHp;
        private bool isDead;
        protected AI_State currentState;

        public float MaxHp => maxHp;
        public float CurrentHp => currentHp;
        public int AttackPower => attackPower;
        public AI_State CurrentState => currentState;

        protected virtual void Awake()
        {
            currentHp = maxHp;
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
            SetState(AI_State.Die);

            Collider2D[] colliders = GetComponents<Collider2D>();

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
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
