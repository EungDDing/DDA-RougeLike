using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    [CreateAssetMenu(fileName = "MeleeSwingPattern", menuName = "DDA Roguelike/Boss Patterns/Melee Swing")]
    public sealed class MeleeSwingPattern : BossPattern
    {
        [Header("Chase")]
        [SerializeField] private float chaseSpeed = 3.0f;
        [SerializeField] private float maximumChaseDuration = 1.5f;

        [Header("Attack")]
        [SerializeField] private float attackRange = 1.8f;
        [SerializeField] private float attackArc = 120.0f;
        [SerializeField] private float damageMultiplier = 1.0f;

        public override IEnumerator Execute(BossContext context)
        {
            float elapsedTime = 0.0f;

            while (context.IsValid
                && context.DistanceToPlayer > attackRange
                && elapsedTime < maximumChaseDuration)
            {
                context.Boss.SetMovement(context.DirectionToPlayer * Mathf.Max(0.0f, chaseSpeed));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            context.Boss.StopMovement();

            if (!context.IsValid)
            {
                yield break;
            }

            Vector2 attackDirection = context.DirectionToPlayer;
            yield return context.Boss.PlayArcTelegraph(
                attackDirection,
                attackRange,
                attackArc,
                TelegraphDuration,
                TelegraphColor);

            context.Boss.TryDamagePlayerInArc(
                attackDirection,
                attackRange,
                attackArc,
                damageMultiplier);
            context.Boss.ShowArcImpact(attackDirection, attackRange, attackArc);
        }
    }
}
