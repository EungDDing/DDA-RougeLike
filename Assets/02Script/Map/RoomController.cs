using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class RoomController : MonoBehaviour
    {
        private const string ItemPositionChildName = "ItemPosition";

        private readonly Dictionary<Vector2Int, RoomDoor> doorsByDirection = new Dictionary<Vector2Int, RoomDoor>();

        private int aliveEnemyCount;
        private bool isInitialized;
        private bool hasSpawnPoints;
        private bool hasSpawnedEnemies;
        private bool hasSpawnedItemBox;
        private GameObject itemBoxPrefab;

        public bool IsCleared { get; private set; }
        public RoomType RoomType { get; private set; }
        public Vector2Int Cell { get; private set; }

        public event Action ClearedChanged;

        public void Initialize(RoomType roomType, Vector2Int cell)
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            RoomType = roomType;
            Cell = cell;

            EnemySpawnPoint[] spawnPoints = GetComponentsInChildren<EnemySpawnPoint>(true);
            hasSpawnPoints = spawnPoints != null && spawnPoints.Length > 0;

            if (hasSpawnPoints)
            {
                aliveEnemyCount = 0;
                hasSpawnedEnemies = false;
                SetCleared(false);
                return;
            }

            Enemy[] enemies = GetComponentsInChildren<Enemy>(true);
            aliveEnemyCount = enemies.Length;
            SetCleared(aliveEnemyCount == 0);
        }

        public void SetItemBoxPrefab(GameObject prefab)
        {
            itemBoxPrefab = prefab;
        }

        public void TrySpawnEnemies(EnemyPool enemyPool)
        {
            if (!isInitialized || hasSpawnedEnemies || IsCleared)
            {
                return;
            }

            if (!hasSpawnPoints)
            {
                return;
            }

            hasSpawnedEnemies = true;

            if (enemyPool == null)
            {
                Debug.LogError($"[{nameof(RoomController)}] {nameof(EnemyPool)} is null when spawning enemies in {gameObject.name}.", this);
                SetCleared(true);
                return;
            }

            EnemySpawnPoint[] spawnPoints = GetComponentsInChildren<EnemySpawnPoint>(true);
            int spawnedCount = 0;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                EnemySpawnPoint spawnPoint = spawnPoints[i];

                if (spawnPoint == null || !spawnPoint.HasEnemyPrefab)
                {
                    continue;
                }

                Enemy enemy = enemyPool.Get(spawnPoint.EnemyPrefab);

                if (enemy == null)
                {
                    continue;
                }

                Vector3 prefabScale = spawnPoint.EnemyPrefab.transform.localScale;
                enemy.transform.SetParent(transform, false);
                enemy.transform.position = spawnPoint.transform.position;
                enemy.transform.rotation = Quaternion.identity;
                ApplyWorldScale(enemy.transform, prefabScale);
                spawnedCount++;
            }

            aliveEnemyCount = spawnedCount;

            if (aliveEnemyCount <= 0)
            {
                SetCleared(true);
            }
        }

        public void TrySpawnItemBoxOnEnter()
        {
            if (RoomType != RoomType.Golden)
            {
                return;
            }

            TrySpawnItemBox();
        }

        private void TrySpawnItemBoxOnClear()
        {
            if (RoomType != RoomType.Boss)
            {
                return;
            }

            if (!IsCleared)
            {
                return;
            }

            TrySpawnItemBox();
        }

        private void TrySpawnItemBox()
        {
            if (!isInitialized || hasSpawnedItemBox)
            {
                return;
            }

            if (itemBoxPrefab == null)
            {
                Debug.LogError($"[{nameof(RoomController)}] ItemBox prefab is not assigned for {gameObject.name}.", this);
                return;
            }

            Transform itemPosition = FindItemPosition();

            if (itemPosition == null)
            {
                Debug.LogError(
                    $"[{nameof(RoomController)}] Child '{ItemPositionChildName}' was not found on {gameObject.name}.",
                    this);
                return;
            }

            hasSpawnedItemBox = true;

            GameObject itemBoxInstance = Instantiate(itemBoxPrefab);
            itemBoxInstance.name = itemBoxPrefab.name;
            itemBoxInstance.transform.SetParent(transform, false);
            itemBoxInstance.transform.position = itemPosition.position;
            itemBoxInstance.transform.rotation = Quaternion.identity;
            ApplyWorldScale(itemBoxInstance.transform, itemBoxPrefab.transform.localScale);
        }

        private Transform FindItemPosition()
        {
            Transform directChild = transform.Find(ItemPositionChildName);

            if (directChild != null)
            {
                return directChild;
            }

            Transform[] children = GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];

                if (child != null && child.name == ItemPositionChildName)
                {
                    return child;
                }
            }

            return null;
        }

        private static void ApplyWorldScale(Transform target, Vector3 worldScale)
        {
            if (target == null)
            {
                return;
            }

            Transform parent = target.parent;

            if (parent == null)
            {
                target.localScale = worldScale;
                return;
            }

            Vector3 parentScale = parent.lossyScale;
            float scaleX = Mathf.Approximately(parentScale.x, 0.0f) ? worldScale.x : worldScale.x / parentScale.x;
            float scaleY = Mathf.Approximately(parentScale.y, 0.0f) ? worldScale.y : worldScale.y / parentScale.y;
            float scaleZ = Mathf.Approximately(parentScale.z, 0.0f) ? worldScale.z : worldScale.z / parentScale.z;
            target.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }

        public void RegisterSpawnedEnemies(int count)
        {
            if (!isInitialized || IsCleared || count <= 0)
            {
                return;
            }

            aliveEnemyCount += count;
        }

        public void NotifyEnemyDied()
        {
            if (!isInitialized || IsCleared)
            {
                return;
            }

            aliveEnemyCount = Mathf.Max(0, aliveEnemyCount - 1);

            if (aliveEnemyCount == 0)
            {
                SetCleared(true);
            }
        }

        public void RegisterDoor(Vector2Int direction, RoomDoor door)
        {
            if (door == null)
            {
                Debug.LogError($"[{nameof(RoomController)}] Door is null on {gameObject.name}.", this);
                return;
            }

            doorsByDirection[direction] = door;
        }

        public bool TryGetDoor(Vector2Int direction, out RoomDoor door)
        {
            return doorsByDirection.TryGetValue(direction, out door);
        }

        public void GetConnectedDirections(List<Vector2Int> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();

            foreach (KeyValuePair<Vector2Int, RoomDoor> entry in doorsByDirection)
            {
                if (entry.Value != null)
                {
                    results.Add(entry.Key);
                }
            }
        }

        private void SetCleared(bool cleared)
        {
            if (IsCleared == cleared)
            {
                return;
            }

            if (IsCleared && !cleared)
            {
                return;
            }

            IsCleared = cleared;
            ClearedChanged?.Invoke();

            if (IsCleared)
            {
                TrySpawnItemBoxOnClear();
            }
        }
    }
}
