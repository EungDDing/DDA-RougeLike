using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public sealed class UpgradeRoller
    {
        private static readonly UpgradeCategory[] Categories =
        {
            UpgradeCategory.Attack,
            UpgradeCategory.Skill,
            UpgradeCategory.Survival,
        };

        public List<UpgradeOffer> Roll(
            UpgradeCatalog catalog,
            bool isBossReward,
            bool supportsSkillProjectileCount)
        {
            List<UpgradeOffer> offers = new List<UpgradeOffer>();

            if (catalog == null || catalog.Definitions == null)
            {
                return offers;
            }

            HashSet<UpgradeDefinition> usedDefinitions = new HashSet<UpgradeDefinition>();
            int choiceCount = catalog.ChoiceCount;

            for (int i = 0; i < choiceCount; i++)
            {
                UpgradeOffer offer = RollOne(
                    catalog,
                    isBossReward,
                    supportsSkillProjectileCount,
                    usedDefinitions);

                if (offer == null)
                {
                    break;
                }

                offers.Add(offer);
                usedDefinitions.Add(offer.Definition);
            }

            return offers;
        }

        private static UpgradeOffer RollOne(
            UpgradeCatalog catalog,
            bool isBossReward,
            bool supportsSkillProjectileCount,
            HashSet<UpgradeDefinition> usedDefinitions)
        {
            UpgradeCategory category;

            if (!TryPickCategory(
                    catalog,
                    isBossReward,
                    supportsSkillProjectileCount,
                    usedDefinitions,
                    out category))
            {
                return null;
            }

            List<UpgradeDefinition> candidates = BuildCandidates(
                catalog,
                category,
                isBossReward,
                supportsSkillProjectileCount,
                usedDefinitions);

            if (candidates.Count == 0)
            {
                return null;
            }

            UpgradeDefinition definition = PickWeightedDefinition(catalog, candidates, isBossReward);

            if (definition == null)
            {
                return null;
            }

            int grade;

            if (!TryPickGrade(catalog, definition, isBossReward, out grade))
            {
                return null;
            }

            float value;

            if (!definition.TryGetValue(grade, out value))
            {
                return null;
            }

            return new UpgradeOffer(definition, grade, value);
        }

        private static bool TryPickCategory(
            UpgradeCatalog catalog,
            bool isBossReward,
            bool supportsSkillProjectileCount,
            HashSet<UpgradeDefinition> usedDefinitions,
            out UpgradeCategory selectedCategory)
        {
            selectedCategory = UpgradeCategory.Attack;
            float totalWeight = 0.0f;
            float[] weights = new float[Categories.Length];

            for (int i = 0; i < Categories.Length; i++)
            {
                UpgradeCategory category = Categories[i];
                List<UpgradeDefinition> candidates = BuildCandidates(
                    catalog,
                    category,
                    isBossReward,
                    supportsSkillProjectileCount,
                    usedDefinitions);

                if (candidates.Count == 0)
                {
                    weights[i] = 0.0f;
                    continue;
                }

                float weight = catalog.GetCategoryWeight(category);
                weights[i] = weight;
                totalWeight += weight;
            }

            if (totalWeight <= 0.0f)
            {
                return false;
            }

            float roll = Random.value * totalWeight;
            float cumulative = 0.0f;

            for (int i = 0; i < Categories.Length; i++)
            {
                cumulative += weights[i];

                if (roll <= cumulative)
                {
                    selectedCategory = Categories[i];
                    return true;
                }
            }

            selectedCategory = Categories[Categories.Length - 1];
            return true;
        }

        private static List<UpgradeDefinition> BuildCandidates(
            UpgradeCatalog catalog,
            UpgradeCategory category,
            bool isBossReward,
            bool supportsSkillProjectileCount,
            HashSet<UpgradeDefinition> usedDefinitions)
        {
            List<UpgradeDefinition> candidates = new List<UpgradeDefinition>();
            UpgradeDefinition[] definitions = catalog.Definitions;

            for (int i = 0; i < definitions.Length; i++)
            {
                UpgradeDefinition definition = definitions[i];

                if (definition == null || !definition.IsEnabled)
                {
                    continue;
                }

                if (definition.Category != category)
                {
                    continue;
                }

                if (usedDefinitions.Contains(definition))
                {
                    continue;
                }

                if (definition.RequiresSkillProjectileSupport && !supportsSkillProjectileCount)
                {
                    continue;
                }

                if (isBossReward)
                {
                    if (!definition.SupportsGrade(4))
                    {
                        continue;
                    }
                }
                else if (!HasAnyPositiveGradeWeight(catalog, definition))
                {
                    continue;
                }

                candidates.Add(definition);
            }

            return candidates;
        }

        private static bool HasAnyPositiveGradeWeight(UpgradeCatalog catalog, UpgradeDefinition definition)
        {
            UpgradeGradeValue[] gradeValues = definition.GradeValues;

            if (gradeValues == null)
            {
                return false;
            }

            for (int i = 0; i < gradeValues.Length; i++)
            {
                UpgradeGradeValue entry = gradeValues[i];

                if (entry == null)
                {
                    continue;
                }

                if (catalog.GetGradeWeight(entry.Grade) > 0.0f)
                {
                    return true;
                }
            }

            return false;
        }

        private static UpgradeDefinition PickWeightedDefinition(
            UpgradeCatalog catalog,
            List<UpgradeDefinition> candidates,
            bool isBossReward)
        {
            float totalWeight = 0.0f;
            float[] weights = new float[candidates.Count];

            for (int i = 0; i < candidates.Count; i++)
            {
                float weight = GetDefinitionWeight(catalog, candidates[i], isBossReward);
                weights[i] = weight;
                totalWeight += weight;
            }

            if (totalWeight <= 0.0f)
            {
                return null;
            }

            float roll = Random.value * totalWeight;
            float cumulative = 0.0f;

            for (int i = 0; i < candidates.Count; i++)
            {
                cumulative += weights[i];

                if (roll <= cumulative)
                {
                    return candidates[i];
                }
            }

            return candidates[candidates.Count - 1];
        }

        private static float GetDefinitionWeight(
            UpgradeCatalog catalog,
            UpgradeDefinition definition,
            bool isBossReward)
        {
            UpgradeGradeValue[] gradeValues = definition.GradeValues;

            if (gradeValues == null)
            {
                return 0.0f;
            }

            float sum = 0.0f;

            for (int i = 0; i < gradeValues.Length; i++)
            {
                UpgradeGradeValue entry = gradeValues[i];

                if (entry == null)
                {
                    continue;
                }

                if (isBossReward && entry.Grade != 4)
                {
                    continue;
                }

                sum += catalog.GetGradeWeight(entry.Grade);
            }

            return sum;
        }

        private static bool TryPickGrade(
            UpgradeCatalog catalog,
            UpgradeDefinition definition,
            bool isBossReward,
            out int grade)
        {
            grade = 0;

            if (isBossReward)
            {
                if (!definition.SupportsGrade(4))
                {
                    return false;
                }

                grade = 4;
                return true;
            }

            UpgradeGradeValue[] gradeValues = definition.GradeValues;

            if (gradeValues == null || gradeValues.Length == 0)
            {
                return false;
            }

            float totalWeight = 0.0f;
            float[] weights = new float[gradeValues.Length];

            for (int i = 0; i < gradeValues.Length; i++)
            {
                UpgradeGradeValue entry = gradeValues[i];

                if (entry == null)
                {
                    weights[i] = 0.0f;
                    continue;
                }

                float weight = catalog.GetGradeWeight(entry.Grade);
                weights[i] = weight;
                totalWeight += weight;
            }

            if (totalWeight <= 0.0f)
            {
                return false;
            }

            float roll = Random.value * totalWeight;
            float cumulative = 0.0f;

            for (int i = 0; i < gradeValues.Length; i++)
            {
                cumulative += weights[i];

                if (roll <= cumulative)
                {
                    grade = gradeValues[i].Grade;
                    return true;
                }
            }

            grade = gradeValues[gradeValues.Length - 1].Grade;
            return true;
        }
    }
}
