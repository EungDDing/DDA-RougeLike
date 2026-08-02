using UnityEngine;

namespace DDARoguelike
{
    public sealed class UpgradeApplier
    {
        public void Apply(
            UpgradeOffer offer,
            UpgradeCatalog catalog,
            PlayerAttack playerAttack,
            PlayerSkill playerSkill,
            PlayerMove playerMove,
            PlayerHealth playerHealth)
        {
            if (offer == null || offer.Definition == null)
            {
                Debug.LogError($"[{nameof(UpgradeApplier)}] Offer is null.");
                return;
            }

            if (catalog == null)
            {
                Debug.LogError($"[{nameof(UpgradeApplier)}] Catalog is null.");
                return;
            }

            float value = offer.Value;

            switch (offer.EffectType)
            {
                case UpgradeEffectType.AttackPower:
                    RequireAttack(playerAttack)?.MultiplyAttackPower(1.0f + value);
                    break;
                case UpgradeEffectType.FireInterval:
                    RequireAttack(playerAttack)?.MultiplyFireInterval(1.0f - value, catalog.MinFireIntervalSeconds);
                    break;
                case UpgradeEffectType.AttackRange:
                    RequireAttack(playerAttack)?.MultiplyAttackRange(1.0f + value);
                    break;
                case UpgradeEffectType.ShotSpeed:
                    RequireAttack(playerAttack)?.MultiplyShotSpeed(1.0f + value);
                    break;
                case UpgradeEffectType.CritChance:
                    ApplyCritChance(playerAttack, catalog, value);
                    break;
                case UpgradeEffectType.CritDamage:
                    RequireAttack(playerAttack)?.MultiplyCritDamage(1.0f + value);
                    break;
                case UpgradeEffectType.AttackProjectile:
                    RequireAttack(playerAttack)?.AddProjectileCount(Mathf.RoundToInt(value));
                    break;
                case UpgradeEffectType.SkillDamage:
                    RequireSkill(playerSkill)?.MultiplySkillDamage(1.0f + value);
                    break;
                case UpgradeEffectType.SkillCooldown:
                    RequireSkill(playerSkill)?.MultiplyCooldown(1.0f - value, catalog.MinSkillCooldownSeconds);
                    break;
                case UpgradeEffectType.SkillProjectile:
                    RequireSkill(playerSkill)?.AddSkillProjectileCount(Mathf.RoundToInt(value));
                    break;
                case UpgradeEffectType.MoveSpeed:
                    RequireMove(playerMove)?.MultiplyMoveSpeed(1.0f + value, catalog.MaxMoveSpeedMultiplier);
                    break;
                case UpgradeEffectType.MaxHp:
                    RequireHealth(playerHealth)?.AddMaxHp(Mathf.RoundToInt(value));
                    break;
                case UpgradeEffectType.Defense:
                    RequireHealth(playerHealth)?.AddDefense(value);
                    break;
                default:
                    Debug.LogError($"[{nameof(UpgradeApplier)}] Unsupported effect type {offer.EffectType}.");
                    break;
            }
        }

        private static void ApplyCritChance(PlayerAttack playerAttack, UpgradeCatalog catalog, float addedChance)
        {
            if (playerAttack == null)
            {
                Debug.LogError($"[{nameof(UpgradeApplier)}] PlayerAttack is required for CritChance.");
                return;
            }

            float maxChance = catalog.MaxCritChance;
            float nextChance = Mathf.Min(maxChance, playerAttack.CritChance + addedChance);
            float delta = nextChance - playerAttack.CritChance;

            if (delta != 0.0f)
            {
                playerAttack.AddCritChance(delta);
            }
        }

        private static PlayerAttack RequireAttack(PlayerAttack playerAttack)
        {
            if (playerAttack == null)
            {
                Debug.LogError($"[{nameof(UpgradeApplier)}] PlayerAttack is null.");
            }

            return playerAttack;
        }

        private static PlayerSkill RequireSkill(PlayerSkill playerSkill)
        {
            if (playerSkill == null)
            {
                Debug.LogError($"[{nameof(UpgradeApplier)}] PlayerSkill is null.");
            }

            return playerSkill;
        }

        private static PlayerMove RequireMove(PlayerMove playerMove)
        {
            if (playerMove == null)
            {
                Debug.LogError($"[{nameof(UpgradeApplier)}] PlayerMove is null.");
            }

            return playerMove;
        }

        private static PlayerHealth RequireHealth(PlayerHealth playerHealth)
        {
            if (playerHealth == null)
            {
                Debug.LogError($"[{nameof(UpgradeApplier)}] PlayerHealth is null.");
            }

            return playerHealth;
        }
    }
}
