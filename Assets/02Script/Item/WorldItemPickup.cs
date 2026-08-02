using UnityEngine;

namespace DDARoguelike
{
    public class WorldItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemDefinition definition;

        private bool isCollected;

        private void OnEnable()
        {
            isCollected = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (definition == null)
            {
                Debug.LogError($"[{nameof(WorldItemPickup)}] definition is not assigned on {gameObject.name}.", this);
                return;
            }

            PlayerItemInventory inventory = other.GetComponent<PlayerItemInventory>();

            if (inventory == null)
            {
                inventory = other.GetComponentInParent<PlayerItemInventory>();
            }

            if (inventory == null)
            {
                Debug.LogError(
                    $"[{nameof(WorldItemPickup)}] {nameof(PlayerItemInventory)} was not found on collector.",
                    this);
                return;
            }

            if (!inventory.AddItem(definition))
            {
                return;
            }

            isCollected = true;
            Destroy(gameObject);
        }
    }
}
