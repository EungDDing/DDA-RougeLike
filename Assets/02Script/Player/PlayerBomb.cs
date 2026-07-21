using UnityEngine;
using UnityEngine.InputSystem;

namespace DDARoguelike
{
    public class PlayerBomb : MonoBehaviour
    {
        private const int MaxBombCount = 99;

        [SerializeField] private GameObject placedBombPrefab;

        private int bombCount;
        private PlayerHealth playerHealth;

        public int BombCount => bombCount;

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();

            if (placedBombPrefab == null)
            {
                Debug.LogError($"[{nameof(PlayerBomb)}] placedBombPrefab is not assigned on {gameObject.name}.", this);
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (!keyboard.eKey.wasPressedThisFrame)
            {
                return;
            }

            TryPlaceBomb();
        }

        public void AddBomb(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            bombCount = Mathf.Min(MaxBombCount, bombCount + amount);
            LogBombAndShield();
        }

        private void TryPlaceBomb()
        {
            if (bombCount <= 0)
            {
                return;
            }

            if (placedBombPrefab == null)
            {
                return;
            }

            bombCount -= 1;
            Instantiate(placedBombPrefab, transform.position, Quaternion.identity);
            LogBombAndShield();
        }

        private void LogBombAndShield()
        {
            int shield = playerHealth != null ? playerHealth.Shield : 0;
            Debug.Log($"Bomb: {bombCount}  Shield: {shield}");
        }
    }
}
