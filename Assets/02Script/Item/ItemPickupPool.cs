using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class ItemPickupPool : MonoBehaviour
    {
        [Serializable]
        private class PoolEntry
        {
            [SerializeField] private GameObject prefab;
            [SerializeField] private int initialSize = 8;

            public GameObject Prefab => prefab;
            public int InitialSize => initialSize;
        }

        [SerializeField] private PoolEntry[] poolEntries;
        [SerializeField] private Transform poolRoot;

        private readonly Dictionary<GameObject, Queue<ItemPickup>> poolsByPrefab = new Dictionary<GameObject, Queue<ItemPickup>>();
        private Transform Root => poolRoot != null ? poolRoot : transform;

        private void Awake()
        {
            if (poolEntries == null)
            {
                return;
            }

            for (int i = 0; i < poolEntries.Length; i++)
            {
                PoolEntry entry = poolEntries[i];

                if (entry == null || entry.Prefab == null)
                {
                    continue;
                }

                WarmPool(entry.Prefab, entry.InitialSize);
            }
        }

        public ItemPickup Get(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError($"[{nameof(ItemPickupPool)}] Prefab is null.", this);
                return null;
            }

            if (!poolsByPrefab.ContainsKey(prefab))
            {
                WarmPool(prefab, 0);
            }

            Queue<ItemPickup> queue = poolsByPrefab[prefab];
            ItemPickup pickup;

            if (queue.Count > 0)
            {
                pickup = queue.Dequeue();
            }
            else
            {
                pickup = CreateInstance(prefab);
            }

            if (pickup == null)
            {
                return null;
            }

            pickup.gameObject.SetActive(true);
            return pickup;
        }

        public void Release(ItemPickup pickup)
        {
            if (pickup == null)
            {
                return;
            }

            GameObject prefab = pickup.SourcePrefab;

            if (prefab == null)
            {
                Debug.LogWarning($"[{nameof(ItemPickupPool)}] Pickup has no source prefab. Deactivating only.", pickup);
                pickup.gameObject.SetActive(false);
                return;
            }

            if (!poolsByPrefab.ContainsKey(prefab))
            {
                poolsByPrefab[prefab] = new Queue<ItemPickup>();
            }

            pickup.transform.SetParent(Root, false);
            pickup.gameObject.SetActive(false);
            poolsByPrefab[prefab].Enqueue(pickup);
        }

        private void WarmPool(GameObject prefab, int initialSize)
        {
            if (!poolsByPrefab.ContainsKey(prefab))
            {
                poolsByPrefab[prefab] = new Queue<ItemPickup>();
            }

            Queue<ItemPickup> queue = poolsByPrefab[prefab];

            for (int i = queue.Count; i < initialSize; i++)
            {
                ItemPickup pickup = CreateInstance(prefab);

                if (pickup != null)
                {
                    queue.Enqueue(pickup);
                }
            }
        }

        private ItemPickup CreateInstance(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, Root);
            ItemPickup pickup = instance.GetComponent<ItemPickup>();

            if (pickup == null)
            {
                Debug.LogError($"[{nameof(ItemPickupPool)}] Prefab {prefab.name} requires an {nameof(ItemPickup)} component.", this);
                Destroy(instance);
                return null;
            }

            pickup.ConfigureSourcePrefab(prefab);
            pickup.ConfigurePool(this);
            instance.SetActive(false);
            return pickup;
        }
    }
}
