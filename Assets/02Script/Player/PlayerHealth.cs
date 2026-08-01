using System;
using UnityEngine;

namespace DDARoguelike
{
    public class PlayerHealth : MonoBehaviour, IDamaged
    {
        private const int MaxTotalDefense = 24;
        private const float DefenseSoftCapConstant = 75.0f;

        [SerializeField] private int maxHp = 6;
        [SerializeField] private float defense = 0.0f;

        private int currentHp;
        private int shield;
        private PlayerBomb playerBomb;

        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public int Shield => shield;
        public float Defense => defense;

        public event Action HealthChanged;
        public event Action StatsChanged;
        public event Action<int, string> Damaged;

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
            NotifyHealthChanged();
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
            NotifyHealthChanged();
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
            NotifyHealthChanged();
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
            NotifyHealthChanged();
        }

        public void AddDefense(float amount)
        {
            if (amount == 0.0f)
            {
                return;
            }

            defense = Mathf.Max(0.0f, defense + amount);
            Debug.Log($"Defense: {defense}");
            NotifyStatsChanged();
        }

        public void TakeDamage(int damage, string attackerName)
        {
            if (damage <= 0)
            {
                return;
            }

            int mitigatedDamage = ApplyDefense(damage);
            int totalDefenseBeforeDamage = currentHp + shield;
            int remainingDamage = mitigatedDamage;

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
                $"{attackerName} dealt {mitigatedDamage} damage (raw {damage}) to {gameObject.name}. Remaining Shield: {shield}, Remaining HP: {currentHp}");

            int appliedDamage = totalDefenseBeforeDamage - currentHp - shield;

            if (appliedDamage > 0)
            {
                Damaged?.Invoke(appliedDamage, attackerName);
            }

            NotifyHealthChanged();
        }

        private int ApplyDefense(int rawDamage)
        {
            float reduced = rawDamage * DefenseSoftCapConstant / (DefenseSoftCapConstant + defense);
            int finalDamage = Mathf.RoundToInt(reduced);
            return Mathf.Max(1, finalDamage);
        }

        private void NotifyHealthChanged()
        {
            if (HealthChanged != null)
            {
                HealthChanged.Invoke();
            }
        }

        private void NotifyStatsChanged()
        {
            if (StatsChanged != null)
            {
                StatsChanged.Invoke();
            }
        }

        private void LogBombAndShield()
        {
            int bombCount = playerBomb != null ? playerBomb.BombCount : 0;
            Debug.Log($"Bomb: {bombCount}  Shield: {shield}");
        }
    }
}
