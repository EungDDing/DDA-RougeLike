using UnityEngine;

namespace DDARoguelike
{
    [CreateAssetMenu(fileName = "StatBoostItem", menuName = "DDARoguelike/Stat Boost Item", order = 0)]
    public class StatBoostItemData : ScriptableObject
    {
        [SerializeField] private StatBoostEntry[] boosts = new StatBoostEntry[0];

        public StatBoostEntry[] Boosts => boosts;
    }
}
