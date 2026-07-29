using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    [CreateAssetMenu(fileName = "GroundSlamPattern", menuName = "DDA Roguelike/Boss Patterns/Ground Slam")]
    public sealed class GroundSlamPattern : BossPattern
    {
        [Header("Slam")]
        [SerializeField] private float damageRadius = 2.5f;
        [SerializeField] private float damageMultiplier = 1.0f;

        public override IEnumerator Execute(BossContext context)
        {
            context.Boss.StopMovement();
            yield return context.Boss.PlayCircleTelegraph(
                damageRadius,
                TelegraphDuration,
                TelegraphColor);
            context.Boss.TryDamagePlayerInCircle(damageRadius, damageMultiplier);
            context.Boss.ShowCircleImpact(damageRadius);
        }
    }
}
