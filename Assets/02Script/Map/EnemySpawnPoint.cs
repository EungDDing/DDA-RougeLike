using UnityEngine;

namespace DDARoguelike
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;

        public GameObject EnemyPrefab => enemyPrefab;

        public bool HasEnemyPrefab => enemyPrefab != null;
    }
}
