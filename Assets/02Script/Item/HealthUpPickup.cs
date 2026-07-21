using UnityEngine;

namespace DDARoguelike
{
    public class HealthUpPickup : ItemPickup
    {
        private const int HealAmount = 2;

        protected override bool CanCollect(GameObject collector)
        {
            PlayerHealth playerHealth = ResolvePlayerHealth(collector);

            if (playerHealth == null)
            {
                Debug.LogWarning($"[{nameof(HealthUpPickup)}] {nameof(PlayerHealth)} was not found on collector.", this);
                return false;
            }

            return playerHealth.CanHeal();
        }

        public override void OnItemGet(GameObject collector)
        {
            PlayerHealth playerHealth = ResolvePlayerHealth(collector);

            if (playerHealth == null)
            {
                Debug.LogWarning($"[{nameof(HealthUpPickup)}] {nameof(PlayerHealth)} was not found on collector.", this);
                return;
            }

            playerHealth.Heal(HealAmount);
        }

        private static PlayerHealth ResolvePlayerHealth(GameObject collector)
        {
            PlayerHealth playerHealth = collector.GetComponent<PlayerHealth>();

            if (playerHealth == null)
            {
                playerHealth = collector.GetComponentInParent<PlayerHealth>();
            }

            return playerHealth;
        }
    }
}
