using UnityEngine;

namespace DDARoguelike
{
    public class Projectile : MonoBehaviour
    {
        private const string EnemyIgnoreTag = "Enemy";
        private const string ThornTag = "Thorn";
        private const string HoleTag = "Hole";

        private Rigidbody2D rigidbody2D;
        private Vector2 spawnPosition;
        private Vector2 direction;
        private float speed;
        private float maxRange;
        private float damage;
        private ProjectilePool ownerPool;
        private GameObject sourcePrefab;
        private string attackerName;
        private string ignoreTag;
        private PlayerItemInventory ownerItemInventory;
        private bool isActive;

        public GameObject SourcePrefab => sourcePrefab;

        private void Awake()
        {
            rigidbody2D = GetComponent<Rigidbody2D>();

            if (rigidbody2D == null)
            {
                Debug.LogError($"[{nameof(Projectile)}] Rigidbody2D is required on {gameObject.name}.", this);
            }
        }

        public void ConfigureSourcePrefab(GameObject prefab)
        {
            sourcePrefab = prefab;
        }

        public void Launch(
            Vector2 launchDirection,
            float launchSpeed,
            float launchMaxRange,
            float launchDamage,
            ProjectilePool pool,
            string launchAttackerName,
            string launchIgnoreTag)
        {
            ownerPool = pool;
            direction = launchDirection.sqrMagnitude > 0.0f ? launchDirection.normalized : Vector2.right;
            speed = launchSpeed;
            maxRange = launchMaxRange;
            damage = launchDamage;
            attackerName = launchAttackerName;
            ignoreTag = launchIgnoreTag;
            ownerItemInventory = null;
            spawnPosition = transform.position;
            isActive = true;
            ApplyFacingRotation(direction);

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = direction * speed;
            }
        }

        private void ApplyFacingRotation(Vector2 travelDirection)
        {
            // Sprite forward is down (-Y), so identity faces Vector2.down.
            float angleDegrees = Mathf.Atan2(travelDirection.y, travelDirection.x) * Mathf.Rad2Deg + 90.0f;
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, angleDegrees);
        }

        public void SetOwnerItemInventory(PlayerItemInventory inventory)
        {
            ownerItemInventory = inventory;
        }

        public float Damage => damage;

        private void FixedUpdate()
        {
            if (!isActive)
            {
                return;
            }

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = direction * speed;
            }

            float traveledDistance = Vector2.Distance(spawnPosition, transform.position);

            if (traveledDistance >= maxRange)
            {
                Release();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive)
            {
                return;
            }

            if (!string.IsNullOrEmpty(ignoreTag) && other.CompareTag(ignoreTag))
            {
                return;
            }

            if (other.CompareTag(ThornTag) || other.CompareTag(HoleTag))
            {
                return;
            }

            if (ignoreTag == EnemyIgnoreTag)
            {
                if (other.CompareTag("Player"))
                {
                    ApplyDamage(other);
                }

                Release();
                return;
            }

            ApplyDamage(other);
            ApplyKnockbackToEnemy(other);
            Release();
        }

        private void ApplyDamage(Collider2D other)
        {
            IDamaged damaged = other.GetComponent<IDamaged>();

            if (damaged == null)
            {
                damaged = other.GetComponentInParent<IDamaged>();
            }

            if (damaged != null)
            {
                int appliedDamage = Mathf.RoundToInt(damage);
                damaged.TakeDamage(appliedDamage, attackerName);

                if (ownerItemInventory != null && appliedDamage > 0)
                {
                    ownerItemInventory.NotifyDamageDealt(appliedDamage);
                }
            }
        }

        private void ApplyKnockbackToEnemy(Collider2D other)
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy == null)
            {
                enemy = other.GetComponentInParent<Enemy>();
            }

            if (enemy != null)
            {
                enemy.ApplyKnockback(direction);
            }
        }

        public void Release()
        {
            if (!isActive)
            {
                return;
            }

            isActive = false;

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
            }

            if (ownerPool != null)
            {
                ownerPool.Release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
