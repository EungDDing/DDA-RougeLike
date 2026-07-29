using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class EnemyPool : MonoBehaviour
    {
        [Serializable]
        private class PoolEntry
        {
            [SerializeField] private GameObject prefab;
            [SerializeField] private int initialSize = 4;

            public GameObject Prefab => prefab;
            public int InitialSize => initialSize;
        }

        [SerializeField] private PoolEntry[] poolEntries;
        [SerializeField] private Transform poolRoot;

        private readonly Dictionary<GameObject, Queue<Enemy>> poolsByPrefab = new Dictionary<GameObject, Queue<Enemy>>();
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

        public Enemy Get(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError($"[{nameof(EnemyPool)}] Prefab is null.", this);
                return null;
            }

            if (!poolsByPrefab.ContainsKey(prefab))
            {
                WarmPool(prefab, 0);
            }

            Queue<Enemy> queue = poolsByPrefab[prefab];
            Enemy enemy;

            if (queue.Count > 0)
            {
                enemy = queue.Dequeue();
            }
            else
            {
                enemy = CreateInstance(prefab);
            }

            if (enemy == null)
            {
                return null;
            }

            enemy.PrepareFromPool();
            enemy.gameObject.SetActive(true);
            return enemy;
        }

        public void Release(Enemy enemy)
        {
            if (enemy == null)
            {
                return;
            }

            GameObject prefab = enemy.SourcePrefab;

            if (prefab == null)
            {
                Debug.LogWarning($"[{nameof(EnemyPool)}] Enemy has no source prefab. Deactivating only.", enemy);
                enemy.gameObject.SetActive(false);
                return;
            }

            if (!poolsByPrefab.ContainsKey(prefab))
            {
                poolsByPrefab[prefab] = new Queue<Enemy>();
            }

            enemy.transform.SetParent(Root, false);
            enemy.gameObject.SetActive(false);
            poolsByPrefab[prefab].Enqueue(enemy);
        }

        private void WarmPool(GameObject prefab, int initialSize)
        {
            if (!poolsByPrefab.ContainsKey(prefab))
            {
                poolsByPrefab[prefab] = new Queue<Enemy>();
            }

            Queue<Enemy> queue = poolsByPrefab[prefab];

            for (int i = queue.Count; i < initialSize; i++)
            {
                Enemy enemy = CreateInstance(prefab);

                if (enemy != null)
                {
                    queue.Enqueue(enemy);
                }
            }
        }

        private Enemy CreateInstance(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, Root);
            Enemy enemy = instance.GetComponent<Enemy>();

            if (enemy == null)
            {
                Debug.LogError($"[{nameof(EnemyPool)}] Prefab {prefab.name} requires an {nameof(Enemy)} component.", this);
                Destroy(instance);
                return null;
            }

            enemy.ConfigurePooling(prefab, this);
            instance.SetActive(false);
            return enemy;
        }
    }
}
