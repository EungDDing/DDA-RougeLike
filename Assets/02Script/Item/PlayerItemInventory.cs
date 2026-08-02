using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class PlayerItemInventory : MonoBehaviour
    {
        private readonly List<ItemDefinition> ownedItems = new List<ItemDefinition>();
        private readonly ItemEffectApplier effectApplier = new ItemEffectApplier();

        private PlayerAttack playerAttack;
        private PlayerMove playerMove;
        private PlayerHealth playerHealth;
        private PlayerSkill playerSkill;
        private bool roomShieldGrantedThisRoom;
        private float pendingLifeStealHeal;

        private void Awake()
        {
            playerAttack = GetComponent<PlayerAttack>();
            playerMove = GetComponent<PlayerMove>();
            playerHealth = GetComponent<PlayerHealth>();
            playerSkill = GetComponent<PlayerSkill>();

            if (playerAttack == null)
            {
                Debug.LogError($"[{nameof(PlayerItemInventory)}] {nameof(PlayerAttack)} is required on {gameObject.name}.", this);
            }

            if (playerMove == null)
            {
                Debug.LogError($"[{nameof(PlayerItemInventory)}] {nameof(PlayerMove)} is required on {gameObject.name}.", this);
            }

            if (playerHealth == null)
            {
                Debug.LogError($"[{nameof(PlayerItemInventory)}] {nameof(PlayerHealth)} is required on {gameObject.name}.", this);
            }
        }

        public bool AddItem(ItemDefinition definition)
        {
            if (definition == null)
            {
                Debug.LogError($"[{nameof(PlayerItemInventory)}] Item definition is null.", this);
                return false;
            }

            ownedItems.Add(definition);
            effectApplier.ApplyImmediateEffects(definition, playerAttack, playerMove, playerHealth, playerSkill);
            TryGrantRoomShieldFromNewItem(definition);
            return true;
        }

        public bool Owns(ItemDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            for (int i = 0; i < ownedItems.Count; i++)
            {
                ItemDefinition owned = ownedItems[i];

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

        public IReadOnlyList<ItemDefinition> OwnedItems => ownedItems;

        public void NotifyDamageDealt(int damageDealt)
        {
            if (damageDealt <= 0 || playerHealth == null)
            {
                return;
            }

            float lifeStealRatio = GetTotalLifeStealRatio();

            if (lifeStealRatio <= 0.0f)
            {
                return;
            }

            if (!playerHealth.CanHeal())
            {
                return;
            }

            pendingLifeStealHeal += damageDealt * lifeStealRatio;
            int healAmount = Mathf.FloorToInt(pendingLifeStealHeal);

            if (healAmount <= 0)
            {
                return;
            }

            pendingLifeStealHeal -= healAmount;
            playerHealth.Heal(healAmount);
        }

        public void NotifyRoomEntered()
        {
            roomShieldGrantedThisRoom = false;
            GrantRoomEntryShield();
        }

        private void TryGrantRoomShieldFromNewItem(ItemDefinition definition)
        {
            if (playerHealth == null || definition == null)
            {
                return;
            }

            int shieldFromItem = GetRoomEntryShieldAmount(definition);

            if (shieldFromItem <= 0)
            {
                return;
            }

            playerHealth.AddShield(shieldFromItem);
            roomShieldGrantedThisRoom = true;
        }

        private void GrantRoomEntryShield()
        {
            if (roomShieldGrantedThisRoom || playerHealth == null)
            {
                return;
            }

            int totalShield = 0;

            for (int i = 0; i < ownedItems.Count; i++)
            {
                totalShield += GetRoomEntryShieldAmount(ownedItems[i]);
            }

            if (totalShield <= 0)
            {
                return;
            }

            playerHealth.AddShield(totalShield);
            roomShieldGrantedThisRoom = true;
        }

        private float GetTotalLifeStealRatio()
        {
            float total = 0.0f;

            for (int i = 0; i < ownedItems.Count; i++)
            {
                ItemDefinition definition = ownedItems[i];

                if (definition == null || definition.Effects == null)
                {
                    continue;
                }

                ItemEffect[] effects = definition.Effects;

                for (int effectIndex = 0; effectIndex < effects.Length; effectIndex++)
                {
                    ItemEffect effect = effects[effectIndex];

                    if (effect != null && effect.EffectType == ItemEffectType.LifeSteal)
                    {
                        total += effect.Value;
                    }
                }
            }

            return total;
        }

        private static int GetRoomEntryShieldAmount(ItemDefinition definition)
        {
            if (definition == null || definition.Effects == null)
            {
                return 0;
            }

            int total = 0;
            ItemEffect[] effects = definition.Effects;

            for (int i = 0; i < effects.Length; i++)
            {
                ItemEffect effect = effects[i];

                if (effect != null && effect.EffectType == ItemEffectType.RoomEntryShield)
                {
                    total += Mathf.RoundToInt(effect.Value);
                }
            }

            return total;
        }
    }
}
