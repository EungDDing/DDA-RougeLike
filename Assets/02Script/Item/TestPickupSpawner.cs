using UnityEngine;
using UnityEngine.InputSystem;

namespace DDARoguelike
{
    public class TestPickupSpawner : MonoBehaviour
    {
        private const float SpawnOffsetX = 0.5f;

        [SerializeField] private ItemPickupPool itemPickupPool;
        [SerializeField] private GameObject bombPickupPrefab;
        [SerializeField] private GameObject shieldPickupPrefab;

        private void Awake()
        {
            if (itemPickupPool == null)
            {
                Debug.LogError($"[{nameof(TestPickupSpawner)}] itemPickupPool is not assigned on {gameObject.name}.", this);
            }

            if (bombPickupPrefab == null)
            {
                Debug.LogError($"[{nameof(TestPickupSpawner)}] bombPickupPrefab is not assigned on {gameObject.name}.", this);
            }

            if (shieldPickupPrefab == null)
            {
                Debug.LogError($"[{nameof(TestPickupSpawner)}] shieldPickupPrefab is not assigned on {gameObject.name}.", this);
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (!keyboard.spaceKey.wasPressedThisFrame)
            {
                return;
            }

            SpawnPickups();
        }

        private void SpawnPickups()
        {
            if (itemPickupPool == null || bombPickupPrefab == null || shieldPickupPrefab == null)
            {
                return;
            }

            Vector3 origin = transform.position;

            ItemPickup bombPickup = itemPickupPool.Get(bombPickupPrefab);

            if (bombPickup != null)
            {
                bombPickup.transform.position = origin + new Vector3(SpawnOffsetX, 0.0f, 0.0f);
            }

            ItemPickup shieldPickup = itemPickupPool.Get(shieldPickupPrefab);

            if (shieldPickup != null)
            {
                shieldPickup.transform.position = origin + new Vector3(-SpawnOffsetX, 0.0f, 0.0f);
            }
        }
    }
}
