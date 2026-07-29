using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class RoomController : MonoBehaviour
    {
        private readonly Dictionary<Vector2Int, RoomDoor> doorsByDirection = new Dictionary<Vector2Int, RoomDoor>();

        private int aliveEnemyCount;
        private bool isInitialized;
        private bool hasSpawnPoints;
        private bool hasSpawnedEnemies;

        public bool IsCleared { get; private set; }
        public RoomType RoomType { get; private set; }

        public event Action ClearedChanged;

        public void Initialize(RoomType roomType)
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            RoomType = roomType;

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
        }
    }
}
