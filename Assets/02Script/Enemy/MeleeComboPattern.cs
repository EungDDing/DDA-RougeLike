using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    [CreateAssetMenu(fileName = "MeleeComboPattern", menuName = "DDA Roguelike/Boss Patterns/Melee Combo")]
    public sealed class MeleeComboPattern : BossPattern
    {
        [Header("Combo")]
        [SerializeField] private int hitCount = 3;
        [SerializeField] private float followUpTelegraphDuration = 0.16f;
        [SerializeField] private float intervalBetweenHits = 0.12f;

        [Header("Lunge")]
        [SerializeField] private float lungeSpeed = 4.0f;
        [SerializeField] private float lungeDuration = 0.08f;

        [Header("Attack")]
        [SerializeField] private float attackRange = 1.7f;
        [SerializeField] private float attackArc = 135.0f;
        [SerializeField] private float damageMultiplier = 1.0f;

        public override IEnumerator Execute(BossContext context)
        {
            int attackCount = Mathf.Max(1, hitCount);

            for (int i = 0; i < attackCount; i++)
            {
                if (!context.IsValid)
                {
                    yield break;
                }

                Vector2 attackDirection = context.DirectionToPlayer;
                float currentTelegraphDuration = i == 0
                    ? TelegraphDuration
                    : followUpTelegraphDuration;
                yield return context.Boss.PlayArcTelegraph(
                    attackDirection,
                    attackRange,
                    attackArc,
                    currentTelegraphDuration,
                    TelegraphColor);

                context.Boss.SetContactDamageEnabled(false);
                context.Boss.SetMovement(attackDirection * Mathf.Max(0.0f, lungeSpeed));
                yield return new WaitForSeconds(Mathf.Max(0.0f, lungeDuration));
                context.Boss.StopMovement();
                context.Boss.SetContactDamageEnabled(true);

                context.Boss.TryDamagePlayerInArc(
                    attackDirection,
                    attackRange,
                    attackArc,
                    damageMultiplier);
                context.Boss.ShowArcImpact(attackDirection, attackRange, attackArc);

                if (i < attackCount - 1)
                {
                    yield return new WaitForSeconds(Mathf.Max(0.0f, intervalBetweenHits));
                }
            }
        }
    }
}
