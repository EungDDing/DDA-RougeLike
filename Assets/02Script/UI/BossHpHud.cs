using UnityEngine;
using UnityEngine.UI;

namespace DDARoguelike
{
    public class BossHpHud : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image hpFill;

        private Enemy boundBoss;

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }

            if (hpFill == null)
            {
                Debug.LogError($"[{nameof(BossHpHud)}] hpFill is not assigned on {gameObject.name}.", this);
            }

            SetVisible(false);
        }

        private void OnDisable()
        {
            UnbindBoss();
        }

        public void BindBossRoom(RoomController room)
        {
            if (room == null || room.RoomType != RoomType.Boss)
            {
                Clear();
                return;
            }

            BossController bossController = room.GetComponentInChildren<BossController>(true);

            if (bossController == null)
            {
                Clear();
                return;
            }

            Bind(bossController);
        }

        public void Bind(Enemy boss)
        {
            if (boss == null)
            {
                Clear();
                return;
            }

            if (boundBoss == boss)
            {
                Refresh();
                SetVisible(true);
                return;
            }

            UnbindBoss();
            boundBoss = boss;
            boundBoss.Damaged += HandleBossDamaged;
            Refresh();
            SetVisible(true);
        }

        public void Clear()
        {
            UnbindBoss();
            SetVisible(false);
        }

        private void HandleBossDamaged(float appliedDamage, string attackerName)
        {
            Refresh();

            if (boundBoss == null || boundBoss.CurrentHp <= 0.0f || boundBoss.CurrentState == AI_State.Die)
            {
                Clear();
            }
        }

        private void Refresh()
        {
            if (hpFill == null)
            {
                return;
            }

            if (boundBoss == null)
            {
                hpFill.fillAmount = 0.0f;
                return;
            }

            float maxHp = boundBoss.MaxHp;
            float fillAmount = maxHp > 0.0f ? boundBoss.CurrentHp / maxHp : 0.0f;
            hpFill.fillAmount = Mathf.Clamp01(fillAmount);
        }

        private void UnbindBoss()
        {
            if (boundBoss == null)
            {
                return;
            }

            boundBoss.Damaged -= HandleBossDamaged;
            boundBoss = null;
        }

        private void SetVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }
    }
}
