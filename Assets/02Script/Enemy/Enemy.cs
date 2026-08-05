using System;
using System.Collections;
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
        private const float ReleaseDelaySeconds = 0.5f;
        private const string PlayerTag = "Player";

        [SerializeField] protected float maxHp;
        [SerializeField] protected int attackPower;
        [SerializeField] private float knockbackForce = 2.0f;
        [SerializeField] private float knockbackDuration = 0.1f;
        [SerializeField] private float softPushSpeed = 5.0f;

        private float currentHp;
        private bool isDead;
        protected Rigidbody2D rigidbody2D;
        private Vector2 knockbackVelocity;
        private float knockbackTimer;
        protected AI_State currentState;
        private GameObject sourcePrefab;
        private EnemyPool ownerPool;
        private Coroutine releaseCoroutine;
        private Vector3 prefabLocalScale = Vector3.one;
        private int lastAcceptedProjectileId = int.MinValue;
        private int lastAcceptedProjectileFrame = -1;

        public float MaxHp => maxHp;
        public float CurrentHp => currentHp;
        public int AttackPower => attackPower;
        public AI_State CurrentState => currentState;
        public GameObject SourcePrefab => sourcePrefab;

        public event Action<float, string> Damaged;

        protected virtual bool ReceivesSoftPush => true;

        protected virtual void Awake()
        {
            currentHp = maxHp;
            rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void ConfigurePooling(GameObject prefab, EnemyPool pool)
        {
            sourcePrefab = prefab;
            ownerPool = pool;

            if (prefab != null)
            {
                prefabLocalScale = prefab.transform.localScale;
            }
        }

        public void PrepareFromPool()
        {
            if (releaseCoroutine != null)
            {
                StopCoroutine(releaseCoroutine);
                releaseCoroutine = null;
            }

            isDead = false;
            currentHp = maxHp;
            knockbackTimer = 0.0f;
            knockbackVelocity = Vector2.zero;
            lastAcceptedProjectileId = int.MinValue;
            lastAcceptedProjectileFrame = -1;
            transform.localScale = prefabLocalScale;

            SetCollidersEnabled(true);

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
            }

            OnPreparedFromPool();
        }

        protected virtual void OnPreparedFromPool()
        {
        }

        public bool TryAcceptProjectileHit(int projectileInstanceId)
        {
            int frame = Time.frameCount;

            if (projectileInstanceId == lastAcceptedProjectileId && frame == lastAcceptedProjectileFrame)
            {
                return false;
            }

            lastAcceptedProjectileId = projectileInstanceId;
            lastAcceptedProjectileFrame = frame;
            return true;
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
            if (isDead || damage <= 0)
            {
                return;
            }

            float hpBeforeDamage = currentHp;
            currentHp = Mathf.Max(0.0f, currentHp - damage);
            float appliedDamage = hpBeforeDamage - currentHp;
            Debug.Log($"{attackerName} dealt {damage} damage to {gameObject.name}. Remaining HP: {currentHp}");

            if (appliedDamage > 0.0f)
            {
                Damaged?.Invoke(appliedDamage, attackerName);
            }

            if (currentHp <= 0.0f)
            {
                BeginDeath();
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDead || currentState == AI_State.Die)
            {
                return;
            }

            if (!collision.collider.CompareTag(PlayerTag))
            {
                return;
            }

            IDamaged damaged = collision.collider.GetComponent<IDamaged>();

            if (damaged != null)
            {
                damaged.TakeDamage(attackPower, gameObject.name);
            }
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (!ReceivesSoftPush || isDead || currentState == AI_State.Die || rigidbody2D == null)
            {
                return;
            }

            if (collision == null || collision.collider == null)
            {
                return;
            }

            if (!IsSoftPushSource(collision.collider))
            {
                return;
            }

            Vector2 away = (Vector2)transform.position - (Vector2)collision.collider.transform.position;

            if (away.sqrMagnitude <= 0.0001f)
            {
                if (collision.contactCount > 0)
                {
                    away = collision.GetContact(0).normal;
                }
                else
                {
                    away = Vector2.right;
                }
            }

            Vector2 pushOffset = away.normalized * softPushSpeed * Time.fixedDeltaTime;
            rigidbody2D.MovePosition(rigidbody2D.position + pushOffset);
        }

        private static bool IsSoftPushSource(Collider2D other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.CompareTag(PlayerTag))
            {
                return true;
            }

            return other.GetComponentInParent<BossController>() != null;
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

            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);

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

            if (ownerPool != null)
            {
                releaseCoroutine = StartCoroutine(ReleaseAfterDelay());
            }
            else
            {
                Destroy(gameObject, ReleaseDelaySeconds);
            }
        }

        private void SetCollidersEnabled(bool enabled)
        {
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = enabled;
                }
            }
        }

        private IEnumerator ReleaseAfterDelay()
        {
            yield return new WaitForSeconds(ReleaseDelaySeconds);
            releaseCoroutine = null;

            if (ownerPool != null)
            {
                ownerPool.Release(this);
            }
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
