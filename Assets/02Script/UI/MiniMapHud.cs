using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public sealed class MiniMapHud : MonoBehaviour
    {
        private enum MiniMapRoomState
        {
            Current = 0,
            Visited = 1,
            Discovered = 2,
        }

        [SerializeField] private RectTransform roomsRoot;
        [SerializeField] private Vector2 cellSpacing = new Vector2(90.0f, 60.0f);
        [SerializeField] private GameObject currentRoomPrefab;
        [SerializeField] private GameObject currentRoomBossPrefab;
        [SerializeField] private GameObject currentRoomGoldenPrefab;
        [SerializeField] private GameObject visitedRoomPrefab;
        [SerializeField] private GameObject visitedRoomBossPrefab;
        [SerializeField] private GameObject visitedRoomGoldenPrefab;
        [SerializeField] private GameObject discoveredRoomPrefab;
        [SerializeField] private GameObject discoveredRoomBossPrefab;
        [SerializeField] private GameObject discoveredRoomGoldenPrefab;

        private readonly HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> discoveredCells = new HashSet<Vector2Int>();
        private readonly List<Vector2Int> connectedDirections = new List<Vector2Int>();
        private readonly List<GameObject> spawnedIcons = new List<GameObject>();

        private Dictionary<Vector2Int, RoomController> roomsByCell;
        private Vector2Int currentCell;
        private bool hasCurrentCell;

        private void Awake()
        {
            ValidateReferences();
        }

        public void Bind(Dictionary<Vector2Int, RoomController> rooms)
        {
            ClearIcons();
            visitedCells.Clear();
            discoveredCells.Clear();
            roomsByCell = rooms;
            hasCurrentCell = false;
        }

        public void NotifyRoomEntered(RoomController room)
        {
            if (room == null)
            {
                return;
            }

            if (roomsByCell == null || roomsByCell.Count == 0)
            {
                Debug.LogError($"[{nameof(MiniMapHud)}] Map was not bound before NotifyRoomEntered.", this);
                return;
            }

            currentCell = room.Cell;
            hasCurrentCell = true;
            visitedCells.Add(currentCell);
            discoveredCells.Add(currentCell);
            DiscoverNeighbors(room);
            Rebuild();
        }

        private void DiscoverNeighbors(RoomController room)
        {
            if (room == null)
            {
                return;
            }

            room.GetConnectedDirections(connectedDirections);

            for (int i = 0; i < connectedDirections.Count; i++)
            {
                Vector2Int direction = connectedDirections[i];

                if (!room.TryGetDoor(direction, out RoomDoor door) || door == null)
                {
                    continue;
                }

                RoomController neighbor = door.TargetRoom;

                if (neighbor == null)
                {
                    continue;
                }

                discoveredCells.Add(neighbor.Cell);
            }
        }

        private void Rebuild()
        {
            ClearIcons();

            if (!hasCurrentCell || roomsByCell == null || roomsRoot == null)
            {
                return;
            }

            foreach (Vector2Int cell in discoveredCells)
            {
                if (!roomsByCell.TryGetValue(cell, out RoomController room) || room == null)
                {
                    continue;
                }

                MiniMapRoomState state = ResolveState(cell);
                GameObject prefab = ResolvePrefab(state, room.RoomType);

                if (prefab == null)
                {
                    Debug.LogError(
                        $"[{nameof(MiniMapHud)}] Prefab is missing for state {state} and type {room.RoomType}.",
                        this);
                    continue;
                }

                GameObject icon = Instantiate(prefab, roomsRoot, false);
                icon.name = $"{prefab.name}_{cell.x}_{cell.y}";

                RectTransform iconTransform = icon.transform as RectTransform;

                if (iconTransform != null)
                {
                    Vector2Int offset = cell - currentCell;
                    iconTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    iconTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    iconTransform.pivot = new Vector2(0.5f, 0.5f);
                    iconTransform.anchoredPosition = new Vector2(
                        offset.x * cellSpacing.x,
                        offset.y * cellSpacing.y);
                    iconTransform.localRotation = Quaternion.identity;
                    iconTransform.localScale = Vector3.one;
                }

                spawnedIcons.Add(icon);
            }
        }

        private MiniMapRoomState ResolveState(Vector2Int cell)
        {
            if (hasCurrentCell && cell == currentCell)
            {
                return MiniMapRoomState.Current;
            }

            if (visitedCells.Contains(cell))
            {
                return MiniMapRoomState.Visited;
            }

            return MiniMapRoomState.Discovered;
        }

        private GameObject ResolvePrefab(MiniMapRoomState state, RoomType roomType)
        {
            bool isBoss = roomType == RoomType.Boss;
            bool isGolden = roomType == RoomType.Golden;

            switch (state)
            {
                case MiniMapRoomState.Current:
                    if (isBoss)
                    {
                        return currentRoomBossPrefab;
                    }

                    if (isGolden)
                    {
                        return currentRoomGoldenPrefab;
                    }

                    return currentRoomPrefab;

                case MiniMapRoomState.Visited:
                    if (isBoss)
                    {
                        return visitedRoomBossPrefab;
                    }

                    if (isGolden)
                    {
                        return visitedRoomGoldenPrefab;
                    }

                    return visitedRoomPrefab;

                default:
                    if (isBoss)
                    {
                        return discoveredRoomBossPrefab;
                    }

                    if (isGolden)
                    {
                        return discoveredRoomGoldenPrefab;
                    }

                    return discoveredRoomPrefab;
            }
        }

        private void ClearIcons()
        {
            for (int i = 0; i < spawnedIcons.Count; i++)
            {
                GameObject icon = spawnedIcons[i];

                if (icon != null)
                {
                    Destroy(icon);
                }
            }

            spawnedIcons.Clear();
        }

        private void ValidateReferences()
        {
            if (roomsRoot == null)
            {
                Debug.LogError($"[{nameof(MiniMapHud)}] roomsRoot is not assigned on {gameObject.name}.", this);
            }

            if (currentRoomPrefab == null
                || currentRoomBossPrefab == null
                || currentRoomGoldenPrefab == null
                || visitedRoomPrefab == null
                || visitedRoomBossPrefab == null
                || visitedRoomGoldenPrefab == null
                || discoveredRoomPrefab == null
                || discoveredRoomBossPrefab == null
                || discoveredRoomGoldenPrefab == null)
            {
                Debug.LogError($"[{nameof(MiniMapHud)}] One or more room icon prefabs are missing on {gameObject.name}.", this);
            }
        }
    }
}
