using UnityEngine;

namespace DDARoguelike
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "DDARoguelike/Item Definition", order = 0)]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private ItemEffect[] effects = new ItemEffect[0];

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemEffect[] Effects => effects;
    }
}
