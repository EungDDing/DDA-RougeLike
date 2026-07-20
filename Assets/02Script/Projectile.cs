using UnityEngine;

namespace DDARoguelike
{
    public class Projectile : MonoBehaviour
    {
        private const string EnemyIgnoreTag = "Enemy";

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
            spawnPosition = transform.position;
            isActive = true;

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = direction * speed;
            }
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
                damaged.TakeDamage(Mathf.RoundToInt(damage), attackerName);
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
