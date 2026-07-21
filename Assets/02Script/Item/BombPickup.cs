using UnityEngine;

namespace DDARoguelike
{
    public class BombPickup : ItemPickup
    {
        public override void OnItemGet(GameObject collector)
        {
            PlayerBomb playerBomb = collector.GetComponent<PlayerBomb>();

            if (playerBomb == null)
            {
                playerBomb = collector.GetComponentInParent<PlayerBomb>();
            }

            if (playerBomb == null)
            {
                Debug.LogWarning($"[{nameof(BombPickup)}] {nameof(PlayerBomb)} was not found on collector.", this);
                return;
            }

            playerBomb.AddBomb(1);
        }
    }
}
