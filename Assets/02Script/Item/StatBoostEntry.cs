using System;
using UnityEngine;

namespace DDARoguelike
{
    [Serializable]
    public class StatBoostEntry
    {
        [SerializeField] private PlayerStatType statType;
        [SerializeField] private float amount = 1.0f;

        public PlayerStatType StatType => statType;
        public float Amount => amount;
    }
}
