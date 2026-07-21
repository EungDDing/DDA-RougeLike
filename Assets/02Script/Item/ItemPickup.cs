using UnityEngine;

namespace DDARoguelike
{
    public abstract class ItemPickup : MonoBehaviour, IItemGet
    {
        [SerializeField] private ItemPickupPool itemPickupPool;

        private GameObject sourcePrefab;
        private bool isCollected;

        public GameObject SourcePrefab => sourcePrefab;

        public void ConfigureSourcePrefab(GameObject prefab)
        {
            sourcePrefab = prefab;
        }

        public void ConfigurePool(ItemPickupPool pool)
        {
            itemPickupPool = pool;
        }

        private void OnEnable()
        {
            isCollected = false;
        }

        public abstract void OnItemGet(GameObject collector);

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

            isCollected = true;
            OnItemGet(other.gameObject);

            if (itemPickupPool != null)
            {
                itemPickupPool.Release(this);
            }
            else
            {
                Debug.LogWarning($"[{nameof(ItemPickup)}] ItemPickupPool is not assigned on {gameObject.name}. Deactivating.", this);
                gameObject.SetActive(false);
            }
        }
    }
}
