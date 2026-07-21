using UnityEngine;

namespace DDARoguelike
{
    public class PlacedBomb : MonoBehaviour
    {
        private const float StopSpeedThreshold = 0.05f;

        [SerializeField] private float explosionRadius = 3.0f;
        [SerializeField] private int explosionDamage = 50;
        [SerializeField] private int playerExplosionDamage = 2;
        [SerializeField] private float fuseSeconds = 2.0f;
        [SerializeField] private float linearDamping = 3.0f;
        [SerializeField] private float pulseAmplitude = 0.2f;
        [SerializeField] private float pulseStartFrequency = 2.0f;
        [SerializeField] private float pulseEndFrequency = 12.0f;

        private Rigidbody2D rigidbody2D;
        private Vector3 baseScale;
        private float startTime;
        private float explodeAtTime;
        private float pulsePhase;
        private bool hasExploded;

        private void Awake()
        {
            rigidbody2D = GetComponent<Rigidbody2D>();
            baseScale = transform.localScale;
            startTime = Time.time;
            explodeAtTime = startTime + fuseSeconds;

            if (rigidbody2D != null)
            {
                rigidbody2D.linearDamping = linearDamping;
            }
        }

        private void Update()
        {
            if (hasExploded)
            {
                return;
            }

            UpdatePulseScale();

            if (Time.time >= explodeAtTime)
            {
                Explode();
            }
        }

        private void FixedUpdate()
        {
            if (hasExploded || rigidbody2D == null)
            {
                return;
            }

            if (rigidbody2D.linearVelocity.sqrMagnitude < StopSpeedThreshold * StopSpeedThreshold)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
            }
        }

        private void UpdatePulseScale()
        {
            float elapsed = Time.time - startTime;
            float t = fuseSeconds > 0.0f ? Mathf.Clamp01(elapsed / fuseSeconds) : 1.0f;
            float frequency = Mathf.Lerp(pulseStartFrequency, pulseEndFrequency, t);

            pulsePhase += frequency * Time.deltaTime;
            float pulse = 1.0f + pulseAmplitude * Mathf.Sin(pulsePhase * Mathf.PI * 2.0f);
            transform.localScale = baseScale * pulse;
        }

        private void Explode()
        {
            if (hasExploded)
            {
                return;
            }

            hasExploded = true;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];

                if (hit == null)
                {
                    continue;
                }

                if (hit.CompareTag("Player"))
                {
                    IDamaged playerDamaged = hit.GetComponent<IDamaged>();

                    if (playerDamaged == null)
                    {
                        playerDamaged = hit.GetComponentInParent<IDamaged>();
                    }

                    if (playerDamaged != null)
                    {
                        playerDamaged.TakeDamage(playerExplosionDamage, "Bomb");
                    }

                    continue;
                }

                Enemy enemy = hit.GetComponent<Enemy>();

                if (enemy == null)
                {
                    enemy = hit.GetComponentInParent<Enemy>();
                }

                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage, "Bomb");
                }
            }

            Destroy(gameObject);
        }
    }
}
