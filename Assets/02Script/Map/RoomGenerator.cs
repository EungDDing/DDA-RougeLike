using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class RoomGenerator : MonoBehaviour
    {
        private const int MinRoomCount = 5;
        private const int MaxRoomCountExclusive = 8;
        private const int MaxGenerationAttempts = 50;

        private static readonly Vector2Int[] CardinalDirections =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
        };

        [SerializeField] private GameObject startRoomPrefab;
        [SerializeField] private GameObject normalRoomPrefab;
        [SerializeField] private GameObject bossRoomPrefab;
        [SerializeField] private GameObject goldenRoomPrefab;
        [SerializeField] private Vector2 roomSpacing = new Vector2(20.0f, 10.0f);
        [SerializeField] private Transform roomsRoot;
        [SerializeField] private int seed;
        [SerializeField] private bool useRandomSeed = true;

        private void Start()
        {
            Generate();
        }

        public void Generate()
        {
            if (!ValidatePrefabs())
            {
                return;
            }

            if (roomsRoot == null)
            {
                GameObject rootObject = new GameObject("GeneratedRooms");
                roomsRoot = rootObject.transform;
            }

            ClearGeneratedRooms();

            int usedSeed = useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : seed;
            Random.InitState(usedSeed);
            Debug.Log($"[{nameof(RoomGenerator)}] Generating rooms with seed {usedSeed}.");

            Dictionary<Vector2Int, RoomType> layout = null;

            for (int attempt = 0; attempt < MaxGenerationAttempts; attempt++)
            {
                layout = TryBuildLayout();

                if (layout != null)
                {
                    break;
                }
            }

            if (layout == null)
            {
                Debug.LogError($"[{nameof(RoomGenerator)}] Failed to generate a valid layout after {MaxGenerationAttempts} attempts.", this);
                return;
            }

            SpawnRooms(layout);
        }

        private bool ValidatePrefabs()
        {
            bool isValid = true;

            if (startRoomPrefab == null)
            {
                Debug.LogError($"[{nameof(RoomGenerator)}] startRoomPrefab is not assigned.", this);
                isValid = false;
            }

            if (normalRoomPrefab == null)
            {
                Debug.LogError($"[{nameof(RoomGenerator)}] normalRoomPrefab is not assigned.", this);
                isValid = false;
            }

            if (bossRoomPrefab == null)
            {
                Debug.LogError($"[{nameof(RoomGenerator)}] bossRoomPrefab is not assigned.", this);
                isValid = false;
            }

            if (goldenRoomPrefab == null)
            {
                Debug.LogError($"[{nameof(RoomGenerator)}] goldenRoomPrefab is not assigned.", this);
                isValid = false;
            }

            return isValid;
        }

        private void ClearGeneratedRooms()
        {
            for (int i = roomsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(roomsRoot.GetChild(i).gameObject);
            }
        }

        private Dictionary<Vector2Int, RoomType> TryBuildLayout()
        {
            int targetCount = Random.Range(MinRoomCount, MaxRoomCountExclusive);
            HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
            Vector2Int startCell = Vector2Int.zero;
            occupied.Add(startCell);

            while (occupied.Count < targetCount)
            {
                List<Vector2Int> candidates = BuildCandidates(occupied);

                if (candidates.Count == 0)
                {
                    return null;
                }

                Vector2Int chosen = candidates[Random.Range(0, candidates.Count)];
                occupied.Add(chosen);
            }

            List<Vector2Int> leaves = CollectLeaves(occupied);

            if (leaves.Count < 2)
            {
                return null;
            }

            Vector2Int bossCell = FindFarthestLeaf(startCell, occupied, leaves);
            List<Vector2Int> goldenCandidates = new List<Vector2Int>();

            for (int i = 0; i < leaves.Count; i++)
            {
                Vector2Int leaf = leaves[i];

                if (leaf == bossCell)
                {
                    continue;
                }

                if (AreOrthogonallyAdjacent(leaf, bossCell))
                {
                    continue;
                }

                goldenCandidates.Add(leaf);
            }

            if (goldenCandidates.Count == 0)
            {
                return null;
            }

            Vector2Int goldenCell = goldenCandidates[Random.Range(0, goldenCandidates.Count)];

            Dictionary<Vector2Int, RoomType> layout = new Dictionary<Vector2Int, RoomType>();

            foreach (Vector2Int cell in occupied)
            {
                if (cell == startCell)
                {
                    layout[cell] = RoomType.Start;
                }
                else if (cell == bossCell)
                {
                    layout[cell] = RoomType.Boss;
                }
                else if (cell == goldenCell)
                {
                    layout[cell] = RoomType.Golden;
                }
                else
                {
                    layout[cell] = RoomType.Normal;
                }
            }

            return layout;
        }

        private static List<Vector2Int> BuildCandidates(HashSet<Vector2Int> occupied)
        {
            HashSet<Vector2Int> candidateSet = new HashSet<Vector2Int>();

            foreach (Vector2Int cell in occupied)
            {
                for (int i = 0; i < CardinalDirections.Length; i++)
                {
                    Vector2Int neighbor = cell + CardinalDirections[i];

                    if (occupied.Contains(neighbor))
                    {
                        continue;
                    }

                    if (WouldCreateTwoByTwo(occupied, neighbor))
                    {
                        continue;
                    }

                    candidateSet.Add(neighbor);
                }
            }

            return new List<Vector2Int>(candidateSet);
        }

        private static bool WouldCreateTwoByTwo(HashSet<Vector2Int> occupied, Vector2Int candidate)
        {
            for (int dx = -1; dx <= 0; dx++)
            {
                for (int dy = -1; dy <= 0; dy++)
                {
                    Vector2Int bottomLeft = new Vector2Int(candidate.x + dx, candidate.y + dy);
                    Vector2Int bottomRight = new Vector2Int(bottomLeft.x + 1, bottomLeft.y);
                    Vector2Int topLeft = new Vector2Int(bottomLeft.x, bottomLeft.y + 1);
                    Vector2Int topRight = new Vector2Int(bottomLeft.x + 1, bottomLeft.y + 1);

                    int filledCount = 0;

                    if (IsFilled(occupied, candidate, bottomLeft))
                    {
                        filledCount++;
                    }

                    if (IsFilled(occupied, candidate, bottomRight))
                    {
                        filledCount++;
                    }

                    if (IsFilled(occupied, candidate, topLeft))
                    {
                        filledCount++;
                    }

                    if (IsFilled(occupied, candidate, topRight))
                    {
                        filledCount++;
                    }

                    if (filledCount == 4)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsFilled(HashSet<Vector2Int> occupied, Vector2Int candidate, Vector2Int cell)
        {
            return cell == candidate || occupied.Contains(cell);
        }

        private static List<Vector2Int> CollectLeaves(HashSet<Vector2Int> occupied)
        {
            List<Vector2Int> leaves = new List<Vector2Int>();

            foreach (Vector2Int cell in occupied)
            {
                if (cell == Vector2Int.zero)
                {
                    continue;
                }

                if (CountNeighbors(occupied, cell) == 1)
                {
                    leaves.Add(cell);
                }
            }

            return leaves;
        }

        private static int CountNeighbors(HashSet<Vector2Int> occupied, Vector2Int cell)
        {
            int count = 0;

            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                if (occupied.Contains(cell + CardinalDirections[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool AreOrthogonallyAdjacent(Vector2Int a, Vector2Int b)
        {
            int manhattan = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
            return manhattan == 1;
        }

        private static Vector2Int FindFarthestLeaf(
            Vector2Int startCell,
            HashSet<Vector2Int> occupied,
            List<Vector2Int> leaves)
        {
            Dictionary<Vector2Int, int> distances = BreadthFirstDistances(startCell, occupied);
            Vector2Int farthest = leaves[0];
            int farthestDistance = -1;

            for (int i = 0; i < leaves.Count; i++)
            {
                Vector2Int leaf = leaves[i];
                int distance = 0;

                if (distances.ContainsKey(leaf))
                {
                    distance = distances[leaf];
                }

                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthest = leaf;
                }
            }

            return farthest;
        }

        private static Dictionary<Vector2Int, int> BreadthFirstDistances(
            Vector2Int startCell,
            HashSet<Vector2Int> occupied)
        {
            Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            distances[startCell] = 0;
            queue.Enqueue(startCell);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                int currentDistance = distances[current];

                for (int i = 0; i < CardinalDirections.Length; i++)
                {
                    Vector2Int neighbor = current + CardinalDirections[i];

                    if (!occupied.Contains(neighbor))
                    {
                        continue;
                    }

                    if (distances.ContainsKey(neighbor))
                    {
                        continue;
                    }

                    distances[neighbor] = currentDistance + 1;
                    queue.Enqueue(neighbor);
                }
            }

            return distances;
        }

        private void SpawnRooms(Dictionary<Vector2Int, RoomType> layout)
        {
            foreach (KeyValuePair<Vector2Int, RoomType> entry in layout)
            {
                GameObject prefab = GetPrefab(entry.Value);

                if (prefab == null)
                {
                    continue;
                }

                Vector3 worldPosition = new Vector3(
                    entry.Key.x * roomSpacing.x,
                    entry.Key.y * roomSpacing.y,
                    0.0f);

                GameObject instance = Instantiate(prefab, worldPosition, Quaternion.identity, roomsRoot);
                instance.name = $"{entry.Value}Room_{entry.Key.x}_{entry.Key.y}";
            }
        }

        private GameObject GetPrefab(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Start:
                    return startRoomPrefab;
                case RoomType.Normal:
                    return normalRoomPrefab;
                case RoomType.Boss:
                    return bossRoomPrefab;
                case RoomType.Golden:
                    return goldenRoomPrefab;
                default:
                    return null;
            }
        }
    }
}
