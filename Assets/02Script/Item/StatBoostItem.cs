using UnityEngine;

namespace DDARoguelike
{
    public class StatBoostItem : MonoBehaviour
    {
        [SerializeField] private StatBoostItemData itemData;

        private bool isCollected;

        private void OnEnable()
        {
            isCollected = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (itemData == null)
            {
                Debug.LogError($"[{nameof(StatBoostItem)}] itemData is not assigned on {gameObject.name}.", this);
                return;
            }

            isCollected = true;
            ApplyBoosts(other.gameObject);
            Destroy(gameObject);
        }

        private void ApplyBoosts(GameObject collector)
        {
            StatBoostEntry[] boosts = itemData.Boosts;

            if (boosts == null || boosts.Length == 0)
            {
                Debug.LogWarning($"[{nameof(StatBoostItem)}] No boosts configured on {itemData.name}.", this);
                return;
            }

            for (int i = 0; i < boosts.Length; i++)
            {
                StatBoostEntry entry = boosts[i];

                if (entry == null)
                {
                    continue;
                }

                ApplyStat(collector, entry);
            }
        }

        private void ApplyStat(GameObject collector, StatBoostEntry entry)
        {
            switch (entry.StatType)
            {
                case PlayerStatType.AttackPower:
                case PlayerStatType.AttackRange:
                case PlayerStatType.FireRate:
                case PlayerStatType.ShotSpeed:
                case PlayerStatType.ProjectileCount:
                    ApplyAttackStat(collector, entry);
                    break;
                case PlayerStatType.MoveSpeed:
                    ApplyMoveStat(collector, entry);
                    break;
                case PlayerStatType.MaxHp:
                    ApplyHealthStat(collector, entry);
                    break;
                default:
                    Debug.LogWarning($"[{nameof(StatBoostItem)}] Unsupported stat type {entry.StatType}.", this);
                    break;
            }
        }

        private void ApplyAttackStat(GameObject collector, StatBoostEntry entry)
        {
            PlayerAttack playerAttack = collector.GetComponent<PlayerAttack>();

            if (playerAttack == null)
            {
                playerAttack = collector.GetComponentInParent<PlayerAttack>();
            }

            if (playerAttack == null)
            {
                Debug.LogWarning($"[{nameof(StatBoostItem)}] {nameof(PlayerAttack)} was not found on collector.", this);
                return;
            }

            switch (entry.StatType)
            {
                case PlayerStatType.AttackPower:
                    playerAttack.AddAttackPower(entry.Amount);
                    break;
                case PlayerStatType.AttackRange:
                    playerAttack.AddAttackRange(entry.Amount);
                    break;
                case PlayerStatType.FireRate:
                    playerAttack.AddFireRate(entry.Amount);
                    break;
                case PlayerStatType.ShotSpeed:
                    playerAttack.AddShotSpeed(entry.Amount);
                    break;
                case PlayerStatType.ProjectileCount:
                    playerAttack.AddProjectileCount(Mathf.RoundToInt(entry.Amount));
                    break;
            }
        }

        private void ApplyMoveStat(GameObject collector, StatBoostEntry entry)
        {
            PlayerMove playerMove = collector.GetComponent<PlayerMove>();

            if (playerMove == null)
            {
                playerMove = collector.GetComponentInParent<PlayerMove>();
            }

            if (playerMove == null)
            {
                Debug.LogWarning($"[{nameof(StatBoostItem)}] {nameof(PlayerMove)} was not found on collector.", this);
                return;
            }

            playerMove.AddMoveSpeed(entry.Amount);
        }

        private void ApplyHealthStat(GameObject collector, StatBoostEntry entry)
        {
            PlayerHealth playerHealth = collector.GetComponent<PlayerHealth>();

            if (playerHealth == null)
            {
                playerHealth = collector.GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth == null)
            {
                Debug.LogWarning($"[{nameof(StatBoostItem)}] {nameof(PlayerHealth)} was not found on collector.", this);
                return;
            }

            playerHealth.AddMaxHp(Mathf.RoundToInt(entry.Amount));
        }
    }
}
