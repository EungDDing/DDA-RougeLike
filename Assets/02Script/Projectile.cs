using UnityEngine;

namespace DDARoguelike
{
    public class Projectile : MonoBehaviour
    {
        private const string EnemyIgnoreTag = "Enemy";
        private const string ThornTag = "Thorn";
        private const string HoleTag = "Hole";
        private const string EnemyProjectileTag = "EnemyProjectile";
        private const string PlayerProjectileTag = "PlayerProjectile";

        [SerializeField] private float facingAngleOffsetDegrees = 90.0f;

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
        private bool hasDealtDamage;

        public GameObject SourcePrefab => sourcePrefab;
        public float Damage => damage;

        protected Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        protected bool IsActive => isActive;
        protected float Speed => speed;

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
            hasDealtDamage = false;
            ApplyFacingRotation(direction);
            OnLaunch();

            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = direction * speed;
            }
        }

        protected virtual void OnLaunch()
        {
        }

        protected void ApplyFacingRotation(Vector2 travelDirection)
        {
            float angleDegrees = Mathf.Atan2(travelDirection.y, travelDirection.x) * Mathf.Rad2Deg
                + facingAngleOffsetDegrees;
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, angleDegrees);
        }

        public void SetOwnerItemInventory(PlayerItemInventory inventory)
        {
            ownerItemInventory = inventory;
        }

        private void FixedUpdate()
        {
            if (!isActive)
            {
                return;
            }

            UpdateTravel();

            float traveledDistance = Vector2.Distance(spawnPosition, transform.position);

            if (traveledDistance >= maxRange)
            {
                Release();
            }
        }

        protected virtual void UpdateTravel()
        {
            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = direction * speed;
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive)
            {
                return;
            }

            if (ShouldIgnoreCollider(other))
            {
                return;
            }

            if (other.CompareTag(ThornTag) || other.CompareTag(HoleTag))
            {
                return;
            }

            if (TryResolveProjectileClash(other))
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

        private bool TryResolveProjectileClash(Collider2D other)
        {
            if (other == null)
            {
                return false;
            }

            bool thisIsPlayerShot = CompareTag(PlayerProjectileTag);
            bool thisIsEnemyShot = CompareTag(EnemyProjectileTag);
            bool otherIsPlayerShot = other.CompareTag(PlayerProjectileTag);
            bool otherIsEnemyShot = other.CompareTag(EnemyProjectileTag);

            if (!(thisIsPlayerShot && otherIsEnemyShot) && !(thisIsEnemyShot && otherIsPlayerShot))
            {
                return false;
            }

            // Destructible enemy projectiles use IDamaged / TakeDamage instead of instant clash.
            if (thisIsPlayerShot && other.GetComponent<IDamaged>() != null)
            {
                return false;
            }

            if (thisIsEnemyShot && GetComponent<IDamaged>() != null)
            {
                return false;
            }

            Projectile otherProjectile = other.GetComponent<Projectile>();

            if (otherProjectile != null)
            {
                otherProjectile.Release();
            }

            Release();
            return true;
        }

        private bool ShouldIgnoreCollider(Collider2D other)
        {
            if (other == null || string.IsNullOrEmpty(ignoreTag))
            {
                return false;
            }

            Transform current = other.transform;

            while (current != null)
            {
                if (current.CompareTag(ignoreTag))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private void ApplyDamage(Collider2D other)
        {
            if (hasDealtDamage)
            {
                return;
            }

            IDamaged damaged = other.GetComponent<IDamaged>();

            if (damaged == null)
            {
                damaged = other.GetComponentInParent<IDamaged>();
            }

            if (damaged == null)
            {
                return;
            }

            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy == null)
            {
                enemy = other.GetComponentInParent<Enemy>();
            }

            if (enemy != null && !enemy.TryAcceptProjectileHit(GetInstanceID()))
            {
                return;
            }

            hasDealtDamage = true;
            int appliedDamage = Mathf.RoundToInt(damage);
            damaged.TakeDamage(appliedDamage, attackerName);

            if (ownerItemInventory != null && appliedDamage > 0)
            {
                ownerItemInventory.NotifyDamageDealt(appliedDamage);
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
            hasDealtDamage = false;

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
