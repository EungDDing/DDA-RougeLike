namespace DDARoguelike
{
    public sealed class UpgradeOffer
    {
        public UpgradeOffer(UpgradeDefinition definition, int grade, float value)
        {
            Definition = definition;
            Grade = grade;
            Value = value;
        }

        public UpgradeDefinition Definition { get; }
        public int Grade { get; }
        public float Value { get; }

        public UpgradeCategory Category => Definition != null ? Definition.Category : UpgradeCategory.Attack;
        public UpgradeEffectType EffectType => Definition != null ? Definition.EffectType : UpgradeEffectType.AttackPower;
        public string DisplayName => Definition != null ? Definition.DisplayName : string.Empty;

        public string BuildDescription()
        {
            if (Definition == null)
            {
                return string.Empty;
            }

            string template = Definition.DescriptionTemplate;

            if (string.IsNullOrEmpty(template))
            {
                return DisplayName;
            }

            string percentText = (Value * 100.0f).ToString("0");
            string valueText = Value.ToString("0.##");

            return template
                .Replace("{percent}", percentText)
                .Replace("{value}", valueText)
                .Replace("{grade}", Grade.ToString());
        }
    }
}
