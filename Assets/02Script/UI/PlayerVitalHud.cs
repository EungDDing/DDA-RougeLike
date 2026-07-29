using UnityEngine;
using UnityEngine.UI;

namespace DDARoguelike
{
    public class PlayerVitalHud : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Image hpFill;
        [SerializeField] private Text hpText;
        [SerializeField] private Image shieldFrame;
        [SerializeField] private Text shieldText;

        private void Awake()
        {
            if (playerHealth == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                {
                    playerHealth = playerObject.GetComponent<PlayerHealth>();
                }
            }

            if (playerHealth == null)
            {
                Debug.LogError($"[{nameof(PlayerVitalHud)}] {nameof(PlayerHealth)} is not assigned on {gameObject.name}.", this);
            }
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= Refresh;
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (playerHealth == null)
            {
                return;
            }

            int currentHp = playerHealth.CurrentHp;
            int maxHp = playerHealth.MaxHp;
            int shield = playerHealth.Shield;

            if (hpFill != null)
            {
                float fillAmount = maxHp > 0 ? (float)currentHp / maxHp : 0.0f;
                hpFill.fillAmount = Mathf.Clamp01(fillAmount);
            }

            if (hpText != null)
            {
                hpText.text = $"{currentHp}/{maxHp}";
            }

            bool hasShield = shield > 0;

            if (shieldFrame != null)
            {
                shieldFrame.gameObject.SetActive(hasShield);
            }

            if (shieldText != null)
            {
                shieldText.gameObject.SetActive(hasShield);
                shieldText.text = shield.ToString();
            }
        }
    }
}
