using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public sealed class ItemRoller
    {
        public void Roll(
            ItemCatalog catalog,
            IReadOnlyList<ItemDefinition> excludedItems,
            List<ItemDefinition> results)
        {
            results.Clear();

            if (catalog == null || catalog.Definitions == null)
            {
                return;
            }

            List<ItemDefinition> candidates = new List<ItemDefinition>();
            ItemDefinition[] definitions = catalog.Definitions;

            for (int i = 0; i < definitions.Length; i++)
            {
                ItemDefinition definition = definitions[i];

                if (definition == null)
                {
                    continue;
                }

                if (IsExcluded(definition, excludedItems))
                {
                    continue;
                }

                candidates.Add(definition);
            }

            int pickCount = Mathf.Min(catalog.ChoiceCount, candidates.Count);

            for (int i = 0; i < pickCount; i++)
            {
                int swapIndex = Random.Range(i, candidates.Count);
                ItemDefinition temp = candidates[i];
                candidates[i] = candidates[swapIndex];
                candidates[swapIndex] = temp;
                results.Add(candidates[i]);
            }
        }

        private static bool IsExcluded(ItemDefinition definition, IReadOnlyList<ItemDefinition> excludedItems)
        {
            if (excludedItems == null)
            {
                return false;
            }

            for (int i = 0; i < excludedItems.Count; i++)
            {
                ItemDefinition owned = excludedItems[i];

                if (owned == null)
                {
                    continue;
                }

                if (owned == definition)
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(definition.ItemId)
                    && !string.IsNullOrEmpty(owned.ItemId)
                    && definition.ItemId == owned.ItemId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
