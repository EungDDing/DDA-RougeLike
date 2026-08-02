using UnityEngine;

namespace DDARoguelike
{
    public sealed class ItemEffectApplier
    {
        private const float MoveSpeedMaxMultiplierFromBase = 10.0f;

        public void ApplyImmediateEffects(
            ItemDefinition definition,
            PlayerAttack playerAttack,
            PlayerMove playerMove,
            PlayerHealth playerHealth,
            PlayerSkill playerSkill)
        {
            if (definition == null)
            {
                return;
            }

            ItemEffect[] effects = definition.Effects;

            if (effects == null)
            {
                return;
            }

            for (int i = 0; i < effects.Length; i++)
            {
                ItemEffect effect = effects[i];

                if (effect == null)
                {
                    continue;
                }

                if (effect.EffectType == ItemEffectType.ModifyStatPercent)
                {
                    ApplyPercent(effect, playerAttack, playerMove, playerHealth, playerSkill);
                }
                else if (effect.EffectType == ItemEffectType.ModifyStatFlat)
                {
                    ApplyFlat(effect, playerAttack, playerMove, playerHealth, playerSkill);
                }
            }
        }

        private void ApplyPercent(
            ItemEffect effect,
            PlayerAttack playerAttack,
            PlayerMove playerMove,
            PlayerHealth playerHealth,
            PlayerSkill playerSkill)
        {
            float factor = 1.0f + effect.Value;

            if (factor == 1.0f)
            {
                return;
            }

            switch (effect.StatType)
            {
                case ItemStatType.AttackPower:
                    RequireAttack(playerAttack)?.MultiplyAttackPower(factor);
                    break;
                case ItemStatType.AttackRange:
                    RequireAttack(playerAttack)?.MultiplyAttackRange(factor);
                    break;
                case ItemStatType.FireRate:
                    RequireAttack(playerAttack)?.MultiplyFireRate(factor);
                    break;
                case ItemStatType.ShotSpeed:
                    RequireAttack(playerAttack)?.MultiplyShotSpeed(factor);
                    break;
                case ItemStatType.CritDamage:
                    RequireAttack(playerAttack)?.MultiplyCritDamage(factor);
                    break;
                case ItemStatType.CritChance:
                    RequireAttack(playerAttack)?.MultiplyCritChance(factor);
                    break;
                case ItemStatType.MoveSpeed:
                    RequireMove(playerMove)?.MultiplyMoveSpeed(factor, MoveSpeedMaxMultiplierFromBase);
                    break;
                case ItemStatType.Defense:
                    RequireHealth(playerHealth)?.MultiplyDefense(factor);
                    break;
                case ItemStatType.SkillDamage:
                    RequireSkill(playerSkill)?.MultiplySkillDamage(factor);
                    break;
                case ItemStatType.MaxHp:
                case ItemStatType.ProjectileCount:
                case ItemStatType.SkillProjectileCount:
                    Debug.LogWarning(
                        $"[{nameof(ItemEffectApplier)}] Stat {effect.StatType} should use {nameof(ItemEffectType.ModifyStatFlat)}.");
                    break;
                default:
                    Debug.LogWarning($"[{nameof(ItemEffectApplier)}] Unsupported percent stat {effect.StatType}.");
                    break;
            }
        }

        private void ApplyFlat(
            ItemEffect effect,
            PlayerAttack playerAttack,
            PlayerMove playerMove,
            PlayerHealth playerHealth,
            PlayerSkill playerSkill)
        {
            if (effect.Value == 0.0f)
            {
                return;
            }

            switch (effect.StatType)
            {
                case ItemStatType.MaxHp:
                    RequireHealth(playerHealth)?.AddMaxHp(Mathf.RoundToInt(effect.Value));
                    break;
                case ItemStatType.ProjectileCount:
                    RequireAttack(playerAttack)?.AddProjectileCount(Mathf.RoundToInt(effect.Value));
                    break;
                case ItemStatType.SkillProjectileCount:
                    RequireSkill(playerSkill)?.AddSkillProjectileCount(Mathf.RoundToInt(effect.Value));
                    break;
                case ItemStatType.CritChance:
                    RequireAttack(playerAttack)?.AddCritChance(effect.Value);
                    break;
                case ItemStatType.AttackPower:
                    RequireAttack(playerAttack)?.AddAttackPower(effect.Value);
                    break;
                case ItemStatType.AttackRange:
                    RequireAttack(playerAttack)?.AddAttackRange(effect.Value);
                    break;
                case ItemStatType.FireRate:
                    RequireAttack(playerAttack)?.AddFireRate(effect.Value);
                    break;
                case ItemStatType.ShotSpeed:
                    RequireAttack(playerAttack)?.AddShotSpeed(effect.Value);
                    break;
                case ItemStatType.CritDamage:
                    RequireAttack(playerAttack)?.AddCritDamage(effect.Value);
                    break;
                case ItemStatType.MoveSpeed:
                    RequireMove(playerMove)?.AddMoveSpeed(effect.Value);
                    break;
                case ItemStatType.Defense:
                    RequireHealth(playerHealth)?.AddDefense(effect.Value);
                    break;
                case ItemStatType.SkillDamage:
                    RequireSkill(playerSkill)?.AddSkillDamage(effect.Value);
                    break;
                default:
                    Debug.LogWarning($"[{nameof(ItemEffectApplier)}] Unsupported flat stat {effect.StatType}.");
                    break;
            }
        }

        private static PlayerAttack RequireAttack(PlayerAttack playerAttack)
        {
            if (playerAttack == null)
            {
                Debug.LogError($"[{nameof(ItemEffectApplier)}] {nameof(PlayerAttack)} is null.");
            }

            return playerAttack;
        }

        private static PlayerMove RequireMove(PlayerMove playerMove)
        {
            if (playerMove == null)
            {
                Debug.LogError($"[{nameof(ItemEffectApplier)}] {nameof(PlayerMove)} is null.");
            }

            return playerMove;
        }

        private static PlayerHealth RequireHealth(PlayerHealth playerHealth)
        {
            if (playerHealth == null)
            {
                Debug.LogError($"[{nameof(ItemEffectApplier)}] {nameof(PlayerHealth)} is null.");
            }

            return playerHealth;
        }

        private static PlayerSkill RequireSkill(PlayerSkill playerSkill)
        {
            if (playerSkill == null)
            {
                Debug.LogError($"[{nameof(ItemEffectApplier)}] {nameof(PlayerSkill)} is null.");
            }

            return playerSkill;
        }
    }
}
