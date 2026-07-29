using UnityEngine;

namespace DDARoguelike
{
    [CreateAssetMenu(fileName = "BossData", menuName = "DDA Roguelike/Boss/Boss Data")]
    public sealed class BossData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string bossId = "melee-boss";

        [Header("Base Stats")]
        [SerializeField] private float maxHp = 30.0f;
        [SerializeField] private int attackPower = 1;
        [SerializeField] private float activationRange = 10.0f;
        [SerializeField] private float minimumPatternInterval = 0.35f;

        [Header("Pattern Visuals")]
        [SerializeField] private float indicatorLineWidth = 0.08f;
        [SerializeField] private float impactEffectDuration = 0.12f;
        [SerializeField] private Color impactEffectColor = Color.yellow;

        [Header("Patterns")]
        [SerializeField] private BossPattern[] patterns;

        public string BossId => bossId;
        public float MaxHp => Mathf.Max(1.0f, maxHp);
        public int AttackPower => Mathf.Max(1, attackPower);
        public float ActivationRange => Mathf.Max(0.0f, activationRange);
        public float MinimumPatternInterval => Mathf.Max(0.0f, minimumPatternInterval);
        public float IndicatorLineWidth => Mathf.Max(0.01f, indicatorLineWidth);
        public float ImpactEffectDuration => Mathf.Max(0.01f, impactEffectDuration);
        public Color ImpactEffectColor => impactEffectColor;
        public BossPattern[] Patterns => patterns;
    }
}
