using System;
using UnityEngine;

namespace DDARoguelike
{
    [Serializable]
    public class UpgradeGradeValue
    {
        [SerializeField] private int grade = 1;
        [SerializeField] private float value;

        public int Grade => grade;
        public float Value => value;
    }

    [CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "DDA Roguelike/Upgrade/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string upgradeId;
        [SerializeField] private string displayName;
        [SerializeField] [TextArea] private string descriptionTemplate;
        [SerializeField] private UpgradeCategory category;
        [SerializeField] private UpgradeEffectType effectType;
        [SerializeField] private UpgradeGradeValue[] gradeValues;
        [SerializeField] private Sprite iconOverride;
        [SerializeField] private bool requiresSkillProjectileSupport;
        [SerializeField] private bool isEnabled = true;

        public string UpgradeId => upgradeId;
        public string DisplayName => displayName;
        public string DescriptionTemplate => descriptionTemplate;
        public UpgradeCategory Category => category;
        public UpgradeEffectType EffectType => effectType;
        public UpgradeGradeValue[] GradeValues => gradeValues;
        public Sprite IconOverride => iconOverride;
        public bool RequiresSkillProjectileSupport => requiresSkillProjectileSupport;
        public bool IsEnabled => isEnabled;

        public bool SupportsGrade(int grade)
        {
            if (gradeValues == null)
            {
                return false;
            }

            for (int i = 0; i < gradeValues.Length; i++)
            {
                if (gradeValues[i] != null && gradeValues[i].Grade == grade)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetValue(int grade, out float value)
        {
            value = 0.0f;

            if (gradeValues == null)
            {
                return false;
            }

            for (int i = 0; i < gradeValues.Length; i++)
            {
                UpgradeGradeValue entry = gradeValues[i];

                if (entry != null && entry.Grade == grade)
                {
                    value = entry.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
