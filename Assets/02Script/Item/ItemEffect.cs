using System;
using UnityEngine;

namespace DDARoguelike
{
    [Serializable]
    public class ItemEffect
    {
        [SerializeField] private ItemEffectType effectType;
        [SerializeField] private ItemStatType statType;
        [SerializeField] private float value;

        public ItemEffectType EffectType => effectType;
        public ItemStatType StatType => statType;
        public float Value => value;
    }
}
