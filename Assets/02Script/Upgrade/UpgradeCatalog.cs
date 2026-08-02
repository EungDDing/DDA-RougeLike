using UnityEngine;
using UnityEngine.Serialization;

namespace DDARoguelike
{
    [CreateAssetMenu(fileName = "UpgradeCatalog", menuName = "DDA Roguelike/Upgrade/Upgrade Catalog")]
    public class UpgradeCatalog : ScriptableObject
    {
        [SerializeField] private UpgradeDefinition[] definitions;
        [SerializeField] private int choiceCount = 3;
        [SerializeField] private float attackCategoryWeight = 0.40f;
        [SerializeField] private float skillCategoryWeight = 0.30f;
        [SerializeField] private float survivalCategoryWeight = 0.30f;
        [SerializeField] private float grade1Weight = 0.55f;
        [SerializeField] private float grade2Weight = 0.30f;
        [SerializeField] private float grade3Weight = 0.12f;
        [SerializeField] private float grade4Weight = 0.03f;
        [SerializeField] private Sprite attackCategoryImage;
        [SerializeField] private Sprite survivalCategoryImage;
        [SerializeField] private Sprite skillCategoryImage;
        [SerializeField] private string grade1DisplayName = "common";
        [SerializeField] private string grade2DisplayName = "rare";
        [SerializeField] private string grade3DisplayName = "unique";
        [SerializeField] private string grade4DisplayName = "legend";
        [FormerlySerializedAs("grade1Color")]
        [SerializeField] private Color grade1BackgroundColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);
        [FormerlySerializedAs("grade2Color")]
        [SerializeField] private Color grade2BackgroundColor = new Color(0.35f, 0.75f, 0.40f, 1.0f);
        [FormerlySerializedAs("grade3Color")]
        [SerializeField] private Color grade3BackgroundColor = new Color(0.35f, 0.55f, 0.95f, 1.0f);
        [FormerlySerializedAs("grade4Color")]
        [SerializeField] private Color grade4BackgroundColor = new Color(0.85f, 0.45f, 0.95f, 1.0f);
        [SerializeField] private Color grade1TextColor = Color.white;
        [SerializeField] private Color grade2TextColor = Color.white;
        [SerializeField] private Color grade3TextColor = Color.white;
        [SerializeField] private Color grade4TextColor = Color.white;
        [SerializeField] private float maxCritChance = 1.0f;
        [SerializeField] private float minFireIntervalSeconds = 0.05f;
        [SerializeField] private float minSkillCooldownSeconds = 0.25f;
        [SerializeField] private float maxMoveSpeedMultiplier = 3.0f;

        public UpgradeDefinition[] Definitions => definitions;
        public int ChoiceCount => Mathf.Max(1, choiceCount);
        public float AttackCategoryWeight => Mathf.Max(0.0f, attackCategoryWeight);
        public float SkillCategoryWeight => Mathf.Max(0.0f, skillCategoryWeight);
        public float SurvivalCategoryWeight => Mathf.Max(0.0f, survivalCategoryWeight);
        public Sprite AttackCategoryImage => attackCategoryImage;
        public Sprite SurvivalCategoryImage => survivalCategoryImage;
        public Sprite SkillCategoryImage => skillCategoryImage;
        public float MaxCritChance => Mathf.Clamp01(maxCritChance);
        public float MinFireIntervalSeconds => Mathf.Max(0.01f, minFireIntervalSeconds);
        public float MinSkillCooldownSeconds => Mathf.Max(0.01f, minSkillCooldownSeconds);
        public float MaxMoveSpeedMultiplier => Mathf.Max(1.0f, maxMoveSpeedMultiplier);

        public float GetGradeWeight(int grade)
        {
            switch (grade)
            {
                case 1:
                    return Mathf.Max(0.0f, grade1Weight);
                case 2:
                    return Mathf.Max(0.0f, grade2Weight);
                case 3:
                    return Mathf.Max(0.0f, grade3Weight);
                case 4:
                    return Mathf.Max(0.0f, grade4Weight);
                default:
                    return 0.0f;
            }
        }

        public string GetGradeDisplayName(int grade)
        {
            switch (grade)
            {
                case 1:
                    return string.IsNullOrEmpty(grade1DisplayName) ? "common" : grade1DisplayName;
                case 2:
                    return string.IsNullOrEmpty(grade2DisplayName) ? "rare" : grade2DisplayName;
                case 3:
                    return string.IsNullOrEmpty(grade3DisplayName) ? "unique" : grade3DisplayName;
                case 4:
                    return string.IsNullOrEmpty(grade4DisplayName) ? "legend" : grade4DisplayName;
                default:
                    return string.Empty;
            }
        }

        public Color GetGradeBackgroundColor(int grade)
        {
            switch (grade)
            {
                case 1:
                    return grade1BackgroundColor;
                case 2:
                    return grade2BackgroundColor;
                case 3:
                    return grade3BackgroundColor;
                case 4:
                    return grade4BackgroundColor;
                default:
                    return Color.white;
            }
        }

        public Color GetGradeTextColor(int grade)
        {
            switch (grade)
            {
                case 1:
                    return grade1TextColor;
                case 2:
                    return grade2TextColor;
                case 3:
                    return grade3TextColor;
                case 4:
                    return grade4TextColor;
                default:
                    return Color.white;
            }
        }

        public Sprite GetCategoryImage(UpgradeCategory category)
        {
            switch (category)
            {
                case UpgradeCategory.Attack:
                    return attackCategoryImage;
                case UpgradeCategory.Skill:
                    return skillCategoryImage;
                case UpgradeCategory.Survival:
                    return survivalCategoryImage;
                default:
                    return null;
            }
        }

        public float GetCategoryWeight(UpgradeCategory category)
        {
            switch (category)
            {
                case UpgradeCategory.Attack:
                    return AttackCategoryWeight;
                case UpgradeCategory.Skill:
                    return SkillCategoryWeight;
                case UpgradeCategory.Survival:
                    return SurvivalCategoryWeight;
                default:
                    return 0.0f;
            }
        }
    }
}
