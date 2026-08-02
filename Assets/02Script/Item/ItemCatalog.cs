using UnityEngine;

namespace DDARoguelike
{
    [CreateAssetMenu(fileName = "ItemCatalog", menuName = "DDARoguelike/Item Catalog", order = 1)]
    public class ItemCatalog : ScriptableObject
    {
        [SerializeField] private ItemDefinition[] definitions = new ItemDefinition[0];
        [SerializeField] private int choiceCount = 3;

        public ItemDefinition[] Definitions => definitions;
        public int ChoiceCount => Mathf.Max(1, choiceCount);
    }
}
