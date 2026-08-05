using UnityEngine;

namespace DDARoguelike
{
    public sealed class HomingProjectile : Projectile, IDamaged
    {
        private const string PlayerTag = "Player";
        private const string PlayerProjectileTag = "PlayerProjectile";
        private const string PlayerProjectileLayerName = "PlayerProjectile";
        private const int OverlapBufferSize = 8;

        private static readonly Collider2D[] OverlapBuffer = new Collider2D[OverlapBufferSize];
        private static bool isLayerMaskInitialized;
        private static int playerProjectileLayerMask;

        [SerializeField] private float turnRadiansPerSecond = 4.0f;
        [SerializeField] private int maxHp = 1;
        [SerializeField] private float hitOverlapPadding = 1.15f;

        private Transform playerTransform;
        private CircleCollider2D circleCollider;
        private int currentHp;
        private int lastAcceptedProjectileId = int.MinValue;
        private int lastAcceptedProjectileFrame = -1;

        protected override void OnLaunch()
        {
            EnsurePlayerProjectileLayerMask();

            if (circleCollider == null)
            {
                circleCollider = GetComponent<CircleCollider2D>();
            }

            currentHp = Mathf.Max(1, maxHp);
            lastAcceptedProjectileId = int.MinValue;
            lastAcceptedProjectileFrame = -1;
            ResolvePlayer();
        }

        public void TakeDamage(int damage, string attackerName)
        {
            TakeDamageFromProjectile(damage, attackerName, int.MinValue);
        }

        private void TakeDamageFromProjectile(int damage, string attackerName, int projectileInstanceId)
        {
            if (!IsActive || damage <= 0 || currentHp <= 0)
            {
                return;
            }

            if (projectileInstanceId != int.MinValue)
            {
                int frame = Time.frameCount;

                if (projectileInstanceId == lastAcceptedProjectileId && frame == lastAcceptedProjectileFrame)
                {
                    return;
                }

                lastAcceptedProjectileId = projectileInstanceId;
                lastAcceptedProjectileFrame = frame;
            }

            currentHp = Mathf.Max(0, currentHp - damage);

            if (currentHp <= 0)
            {
                Release();
            }
        }

        protected override void UpdateTravel()
        {
            if (playerTransform == null)
            {
                ResolvePlayer();
            }

            if (playerTransform != null)
            {
                Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;

                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    Vector2 desired = toPlayer.normalized;
                    float maxDelta = Mathf.Max(0.0f, turnRadiansPerSecond) * Time.fixedDeltaTime;
                    float angle = Vector2.SignedAngle(Direction, desired);
                    float step = Mathf.Clamp(angle, -maxDelta * Mathf.Rad2Deg, maxDelta * Mathf.Rad2Deg);
                    float radians = step * Mathf.Deg2Rad;
                    float cos = Mathf.Cos(radians);
                    float sin = Mathf.Sin(radians);
                    Vector2 current = Direction;
                    Direction = new Vector2(
                        current.x * cos - current.y * sin,
                        current.x * sin + current.y * cos).normalized;
                    ApplyFacingRotation(Direction);
                }
            }

            base.UpdateTravel();
            TryReceivePlayerProjectileHits();
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsActive || other == null)
            {
                return;
            }

            if (other.CompareTag(PlayerProjectileTag))
            {
                return;
            }

            if (ShouldIgnoreTaggedHierarchy(other, "Enemy"))
            {
                return;
            }

            if (other.CompareTag("Thorn") || other.CompareTag("Hole"))
            {
                return;
            }

            if (other.CompareTag(PlayerTag))
            {
                ApplyDamageToTarget(other);
                Release();
                return;
            }

            Release();
        }

        private void TryReceivePlayerProjectileHits()
        {
            if (!IsActive || currentHp <= 0 || playerProjectileLayerMask == 0)
            {
                return;
            }

            float radius = ResolveOverlapRadius();
            int hitCount = Physics2D.OverlapCircleNonAlloc(
                transform.position,
                radius,
                OverlapBuffer,
                playerProjectileLayerMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = OverlapBuffer[i];

                if (hitCollider == null)
                {
                    continue;
                }

                Projectile playerShot = hitCollider.GetComponent<Projectile>();

                if (playerShot == null || playerShot == this)
                {
                    continue;
                }

                int appliedDamage = Mathf.Max(1, Mathf.RoundToInt(playerShot.Damage));
                TakeDamageFromProjectile(appliedDamage, playerShot.gameObject.name, playerShot.GetInstanceID());
                playerShot.Release();

                if (!IsActive || currentHp <= 0)
                {
                    return;
                }
            }
        }

        private float ResolveOverlapRadius()
        {
            float padding = Mathf.Max(1.0f, hitOverlapPadding);

            if (circleCollider != null)
            {
                float scale = Mathf.Max(
                    Mathf.Abs(transform.lossyScale.x),
                    Mathf.Abs(transform.lossyScale.y));
                return Mathf.Max(0.15f, circleCollider.radius * scale * padding);
            }

            return 0.4f * padding;
        }

        private void ApplyDamageToTarget(Collider2D other)
        {
            IDamaged damaged = other.GetComponent<IDamaged>();

            if (damaged == null)
            {
                damaged = other.GetComponentInParent<IDamaged>();
            }

            if (damaged != null)
            {
                damaged.TakeDamage(Mathf.RoundToInt(Damage), gameObject.name);
            }
        }

        private static bool ShouldIgnoreTaggedHierarchy(Collider2D other, string tag)
        {
            Transform current = other.transform;

            while (current != null)
            {
                if (current.CompareTag(tag))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private void ResolvePlayer()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);

            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        private static void EnsurePlayerProjectileLayerMask()
        {
            if (isLayerMaskInitialized)
            {
                return;
            }

            isLayerMaskInitialized = true;
            int layer = LayerMask.NameToLayer(PlayerProjectileLayerName);

            if (layer < 0)
            {
                Debug.LogError(
                    $"[{nameof(HomingProjectile)}] Layer '{PlayerProjectileLayerName}' was not found.");
                playerProjectileLayerMask = 0;
                return;
            }

            playerProjectileLayerMask = 1 << layer;
        }
    }
}
