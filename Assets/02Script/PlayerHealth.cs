using UnityEngine;

namespace DDARoguelike
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHp = 3;

        private int currentHp;

        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;

        private void Awake()
        {
            currentHp = maxHp;
        }
    }
}
