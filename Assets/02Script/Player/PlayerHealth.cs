using UnityEngine;

namespace DDARoguelike
{
    public class PlayerHealth : MonoBehaviour, IDamaged
    {
        private const int MaxTotalDefense = 24;

        [SerializeField] private int maxHp = 6;

        private int currentHp;
        private int shield;
        private PlayerBomb playerBomb;

        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public int Shield => shield;

        public bool CanHeal()
        {
            return currentHp < maxHp;
        }

        public bool CanAddShield()
        {
            return maxHp + shield < MaxTotalDefense;
        }

        private void Awake()
        {
            currentHp = maxHp;
            shield = 0;
            playerBomb = GetComponent<PlayerBomb>();
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (!CanHeal())
            {
                return;
            }

            currentHp = Mathf.Min(maxHp, currentHp + amount);
            Debug.Log($"CurrentHp: {currentHp}  MaxHp: {maxHp}");
        }

        public void AddMaxHp(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            maxHp += amount;
            currentHp += amount;

            int maxShield = MaxTotalDefense - maxHp;

            if (maxShield < 0)
            {
                maxShield = 0;
            }

            if (shield > maxShield)
            {
                shield = maxShield;
            }

            Debug.Log($"MaxHp: {maxHp}  CurrentHp: {currentHp}  Shield: {shield}");
        }

        public void AddShield(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int remainingCapacity = MaxTotalDefense - maxHp - shield;

            if (remainingCapacity <= 0)
            {
                return;
            }

            int appliedAmount = Mathf.Min(amount, remainingCapacity);
            shield += appliedAmount;
            LogBombAndShield();
        }

        public void TakeDamage(int damage, string attackerName)
        {
            if (damage <= 0)
            {
                return;
            }

            int remainingDamage = damage;

            if (shield > 0)
            {
                int shieldAbsorbed = Mathf.Min(shield, remainingDamage);
                shield -= shieldAbsorbed;
                remainingDamage -= shieldAbsorbed;
            }

            if (remainingDamage > 0)
            {
                currentHp = Mathf.Max(0, currentHp - remainingDamage);
            }

            Debug.Log(
                $"{attackerName} dealt {damage} damage to {gameObject.name}. Remaining Shield: {shield}, Remaining HP: {currentHp}");
        }

        private void LogBombAndShield()
        {
            int bombCount = playerBomb != null ? playerBomb.BombCount : 0;
            Debug.Log($"Bomb: {bombCount}  Shield: {shield}");
        }
    }
}
