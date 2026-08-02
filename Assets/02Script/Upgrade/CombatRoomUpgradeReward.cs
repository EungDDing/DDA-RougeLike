using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class CombatRoomUpgradeReward : MonoBehaviour
    {
        [SerializeField] private UpgradeCatalog catalog;
        [SerializeField] private UpgradeSelectUI selectUI;
        [SerializeField] private RoomGenerator roomGenerator;
        [SerializeField] private Transform roomsRoot;
        [SerializeField] private PlayerAttack playerAttack;
        [SerializeField] private PlayerSkill playerSkill;
        [SerializeField] private PlayerMove playerMove;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerBomb playerBomb;

        private readonly UpgradeRoller roller = new UpgradeRoller();
        private readonly UpgradeApplier applier = new UpgradeApplier();
        private readonly List<RoomController> subscribedRooms = new List<RoomController>();
        private readonly Dictionary<RoomController, System.Action> roomHandlers = new Dictionary<RoomController, System.Action>();
        private readonly HashSet<RoomController> rewardedRooms = new HashSet<RoomController>();

        private bool isRewardOpen;
        private float previousTimeScale = 1.0f;

        private void Awake()
        {
            if (catalog == null)
            {
                Debug.LogError($"[{nameof(CombatRoomUpgradeReward)}] catalog is not assigned on {gameObject.name}.", this);
            }

            if (selectUI == null)
            {
                Debug.LogError($"[{nameof(CombatRoomUpgradeReward)}] selectUI is not assigned on {gameObject.name}.", this);
            }

            if (playerAttack == null)
            {
                Debug.LogError($"[{nameof(CombatRoomUpgradeReward)}] playerAttack is not assigned on {gameObject.name}.", this);
            }

            if (playerSkill == null)
            {
                Debug.LogError($"[{nameof(CombatRoomUpgradeReward)}] playerSkill is not assigned on {gameObject.name}.", this);
            }

            if (playerMove == null)
            {
                Debug.LogError($"[{nameof(CombatRoomUpgradeReward)}] playerMove is not assigned on {gameObject.name}.", this);
            }

            if (playerHealth == null)
            {
                Debug.LogError($"[{nameof(CombatRoomUpgradeReward)}] playerHealth is not assigned on {gameObject.name}.", this);
            }
        }

        private void OnEnable()
        {
            if (roomGenerator != null)
            {
                roomGenerator.StageStarted += HandleStageStarted;
            }
        }

        private void OnDisable()
        {
            if (roomGenerator != null)
            {
                roomGenerator.StageStarted -= HandleStageStarted;
            }

            UnsubscribeAllRooms();
            ResumeGameplay();
        }

        private void HandleStageStarted(RoomController bossRoom)
        {
            rewardedRooms.Clear();
            SubscribeRooms();
        }

        private void SubscribeRooms()
        {
            UnsubscribeAllRooms();

            Transform root = roomsRoot;

            if (root == null && roomGenerator != null)
            {
                root = roomGenerator.RoomsRoot;
            }

            if (root == null)
            {
                Debug.LogError($"[{nameof(CombatRoomUpgradeReward)}] roomsRoot is not available.", this);
                return;
            }

            RoomController[] rooms = root.GetComponentsInChildren<RoomController>(true);

            for (int i = 0; i < rooms.Length; i++)
            {
                RoomController room = rooms[i];

                if (room == null)
                {
                    continue;
                }

                // Subscribe after generation so rooms that start already cleared do not grant rewards.
                System.Action handler = () => HandleRoomClearedChanged(room);
                room.ClearedChanged += handler;
                roomHandlers[room] = handler;
                subscribedRooms.Add(room);
            }
        }

        private void UnsubscribeAllRooms()
        {
            for (int i = 0; i < subscribedRooms.Count; i++)
            {
                RoomController room = subscribedRooms[i];

                if (room == null)
                {
                    continue;
                }

                System.Action handler;

                if (roomHandlers.TryGetValue(room, out handler))
                {
                    room.ClearedChanged -= handler;
                }
            }

            subscribedRooms.Clear();
            roomHandlers.Clear();
        }

        private void HandleRoomClearedChanged(RoomController room)
        {
            if (isRewardOpen || room == null || !room.IsCleared)
            {
                return;
            }

            if (room.RoomType != RoomType.Normal && room.RoomType != RoomType.Boss)
            {
                return;
            }

            if (rewardedRooms.Contains(room))
            {
                return;
            }

            if (TryOpenReward(room.RoomType == RoomType.Boss))
            {
                rewardedRooms.Add(room);
            }
        }

        private bool TryOpenReward(bool isBossReward)
        {
            if (catalog == null || selectUI == null)
            {
                return false;
            }

            bool supportsSkillProjectile = playerSkill != null && playerSkill.SupportsSkillProjectileCount;
            List<UpgradeOffer> offers = roller.Roll(catalog, isBossReward, supportsSkillProjectile);

            if (offers == null || offers.Count == 0)
            {
                Debug.LogWarning($"[{nameof(CombatRoomUpgradeReward)}] No upgrade offers were rolled.", this);
                return false;
            }

            isRewardOpen = true;
            PauseGameplay();
            selectUI.Show(offers, catalog, HandleOfferSelected);
            return true;
        }

        private void HandleOfferSelected(UpgradeOffer offer)
        {
            if (!isRewardOpen)
            {
                return;
            }

            applier.Apply(offer, catalog, playerAttack, playerSkill, playerMove, playerHealth);

            if (selectUI != null)
            {
                selectUI.Hide();
            }

            isRewardOpen = false;
            ResumeGameplay();
        }

        private void PauseGameplay()
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0.0f;
            SetPlayerInputEnabled(false);
        }

        private void ResumeGameplay()
        {
            Time.timeScale = previousTimeScale > 0.0f ? previousTimeScale : 1.0f;
            SetPlayerInputEnabled(true);
        }

        private void SetPlayerInputEnabled(bool isEnabled)
        {
            if (playerMove != null)
            {
                playerMove.SetMovementEnabled(isEnabled);
            }

            if (playerAttack != null)
            {
                playerAttack.SetCombatInputEnabled(isEnabled);
            }

            if (playerSkill != null)
            {
                playerSkill.SetSkillInputEnabled(isEnabled);
            }

            if (playerBomb != null)
            {
                playerBomb.SetBombInputEnabled(isEnabled);
            }
        }
    }
}
