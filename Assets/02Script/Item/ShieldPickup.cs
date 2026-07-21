using UnityEngine;

namespace DDARoguelike
{
    public class ShieldPickup : ItemPickup
    {
        private const int ShieldAmount = 2;

        public override void OnItemGet(GameObject collector)
        {
            PlayerHealth playerHealth = collector.GetComponent<PlayerHealth>();

            if (playerHealth == null)
            {
                playerHealth = collector.GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth == null)
            {
                Debug.LogWarning($"[{nameof(ShieldPickup)}] {nameof(PlayerHealth)} was not found on collector.", this);
                return;
            }

            playerHealth.AddShield(ShieldAmount);
        }
    }
}
