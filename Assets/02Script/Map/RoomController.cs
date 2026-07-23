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

        public bool IsCleared { get; private set; }

        public event Action ClearedChanged;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            Enemy[] enemies = GetComponentsInChildren<Enemy>(true);
            aliveEnemyCount = enemies.Length;
            SetCleared(aliveEnemyCount == 0);
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
