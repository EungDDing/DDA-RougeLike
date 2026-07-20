using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class ProjectilePool : MonoBehaviour
    {
        [Serializable]
        private class PoolEntry
        {
            [SerializeField] private GameObject prefab;
            [SerializeField] private int initialSize = 16;

            public GameObject Prefab => prefab;
            public int InitialSize => initialSize;
        }

        [SerializeField] private PoolEntry[] poolEntries;
        [SerializeField] private Transform poolRoot;

        private readonly Dictionary<GameObject, Queue<Projectile>> poolsByPrefab = new Dictionary<GameObject, Queue<Projectile>>();
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

        public Projectile Get(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError($"[{nameof(ProjectilePool)}] Prefab is null.", this);
                return null;
            }

            if (!poolsByPrefab.ContainsKey(prefab))
            {
                WarmPool(prefab, 0);
            }

            Queue<Projectile> queue = poolsByPrefab[prefab];
            Projectile projectile;

            if (queue.Count > 0)
            {
                projectile = queue.Dequeue();
            }
            else
            {
                projectile = CreateInstance(prefab);
            }

            if (projectile == null)
            {
                return null;
            }

            projectile.gameObject.SetActive(true);
            return projectile;
        }

        public void Release(Projectile projectile)
        {
            if (projectile == null)
            {
                return;
            }

            GameObject prefab = projectile.SourcePrefab;

            if (prefab == null)
            {
                Debug.LogWarning($"[{nameof(ProjectilePool)}] Projectile has no source prefab. Deactivating only.", projectile);
                projectile.gameObject.SetActive(false);
                return;
            }

            if (!poolsByPrefab.ContainsKey(prefab))
            {
                poolsByPrefab[prefab] = new Queue<Projectile>();
            }

            projectile.transform.SetParent(Root, false);
            projectile.gameObject.SetActive(false);
            poolsByPrefab[prefab].Enqueue(projectile);
        }

        private void WarmPool(GameObject prefab, int initialSize)
        {
            if (!poolsByPrefab.ContainsKey(prefab))
            {
                poolsByPrefab[prefab] = new Queue<Projectile>();
            }

            Queue<Projectile> queue = poolsByPrefab[prefab];

            for (int i = queue.Count; i < initialSize; i++)
            {
                Projectile projectile = CreateInstance(prefab);

                if (projectile != null)
                {
                    queue.Enqueue(projectile);
                }
            }
        }

        private Projectile CreateInstance(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, Root);
            Projectile projectile = instance.GetComponent<Projectile>();

            if (projectile == null)
            {
                Debug.LogError($"[{nameof(ProjectilePool)}] Prefab {prefab.name} requires a {nameof(Projectile)} component.", this);
                Destroy(instance);
                return null;
            }

            projectile.ConfigureSourcePrefab(prefab);
            instance.SetActive(false);
            return projectile;
        }
    }
}
