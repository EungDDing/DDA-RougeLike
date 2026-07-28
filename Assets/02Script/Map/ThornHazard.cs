using UnityEngine;

namespace DDARoguelike
{
    public class ThornHazard : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        private const string AttackerName = "Thorn";

        [SerializeField] private int damage = 1;
        [SerializeField] private float hitCooldownSeconds = 0.5f;

        private float nextHitTime;

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamagePlayer(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamagePlayer(other);
        }

        private void TryDamagePlayer(Collider2D other)
        {
            if (damage <= 0)
            {
                return;
            }

            if (!other.CompareTag(PlayerTag))
            {
                return;
            }

            if (Time.time < nextHitTime)
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

            damaged.TakeDamage(damage, AttackerName);
            nextHitTime = Time.time + hitCooldownSeconds;
        }
    }
}
