using System.Collections;
using UnityEngine;

namespace DDARoguelike
{
    [CreateAssetMenu(fileName = "ChargeSlashPattern", menuName = "DDA Roguelike/Boss Patterns/Charge Slash")]
    public sealed class ChargeSlashPattern : BossPattern
    {
        [Header("Charge")]
        [SerializeField] private float chargeSpeed = 8.0f;
        [SerializeField] private float chargeDuration = 0.55f;
        [SerializeField] private float postChargeDelay = 0.12f;
        [SerializeField] private float chargeIndicatorWidth = 0.8f;

        [Header("Slash")]
        [SerializeField] private float slashRange = 2.2f;
        [SerializeField] private float slashArc = 160.0f;
        [SerializeField] private float damageMultiplier = 1.0f;

        public override IEnumerator Execute(BossContext context)
        {
            if (!context.IsValid)
            {
                yield break;
            }

            Vector2 chargeDirection = context.DirectionToPlayer;
            yield return context.Boss.PlayLineTelegraph(
                chargeDirection,
                chargeSpeed * chargeDuration,
                chargeIndicatorWidth,
                TelegraphDuration,
                TelegraphColor);

            context.Boss.SetContactDamageEnabled(false);
            context.Boss.SetMovement(chargeDirection * Mathf.Max(0.0f, chargeSpeed));

            float elapsedTime = 0.0f;

            while (elapsedTime < chargeDuration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            context.Boss.StopMovement();
            context.Boss.SetContactDamageEnabled(true);
            yield return new WaitForSeconds(Mathf.Max(0.0f, postChargeDelay));

            context.Boss.TryDamagePlayerInArc(
                chargeDirection,
                slashRange,
                slashArc,
                damageMultiplier);
            context.Boss.ShowArcImpact(chargeDirection, slashRange, slashArc);
        }
    }
}
