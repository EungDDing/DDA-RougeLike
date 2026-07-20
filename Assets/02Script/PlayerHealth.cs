using UnityEngine;

namespace DDARoguelike
{
    public class PlayerHealth : MonoBehaviour, IDamaged
    {
        [SerializeField] private int maxHp = 6;

        private int currentHp;

        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;

        private void Awake()
        {
            currentHp = maxHp;
        }

        public void TakeDamage(int damage, string attackerName)
        {
            currentHp = Mathf.Max(0, currentHp - damage);
            Debug.Log($"{attackerName} dealt {damage} damage to {gameObject.name}. Remaining HP: {currentHp}");
        }
    }
}
