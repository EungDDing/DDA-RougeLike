using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DDARoguelike
{
    public class PlayerSkillHud : MonoBehaviour
    {
        private const float CompleteFlashSeconds = 0.12f;

        [SerializeField] private PlayerSkill playerSkill;
        [SerializeField] private Image coolTimeCover;
        [SerializeField] private TextMeshProUGUI coolTimeText;
        [SerializeField] private GameObject coolTimeComplete;

        private Coroutine completeBlinkCoroutine;

        private void Awake()
        {
            if (playerSkill == null)
            {
                playerSkill = FindFirstObjectByType<PlayerSkill>();
            }

            if (playerSkill == null)
            {
                Debug.LogError($"[{nameof(PlayerSkillHud)}] {nameof(PlayerSkill)} is not assigned on {gameObject.name}.", this);
            }

            ConfigureCoverImage();
            SetCooldownVisualActive(false);
            SetCompleteActive(false);
        }

        private void OnEnable()
        {
            if (playerSkill == null)
            {
                return;
            }

            playerSkill.CooldownStarted += HandleCooldownStarted;
            playerSkill.CooldownReady += HandleCooldownReady;
            RefreshCooldownVisual();
        }

        private void OnDisable()
        {
            if (playerSkill != null)
            {
                playerSkill.CooldownStarted -= HandleCooldownStarted;
                playerSkill.CooldownReady -= HandleCooldownReady;
            }

            if (completeBlinkCoroutine != null)
            {
                StopCoroutine(completeBlinkCoroutine);
                completeBlinkCoroutine = null;
            }
        }

        private void Update()
        {
            if (playerSkill == null || !playerSkill.IsOnCooldown)
            {
                return;
            }

            RefreshCooldownVisual();
        }

        private void HandleCooldownStarted()
        {
            if (completeBlinkCoroutine != null)
            {
                StopCoroutine(completeBlinkCoroutine);
                completeBlinkCoroutine = null;
            }

            SetCompleteActive(false);
            SetCooldownVisualActive(true);
            RefreshCooldownVisual();
        }

        private void HandleCooldownReady()
        {
            SetCooldownVisualActive(false);

            if (completeBlinkCoroutine != null)
            {
                StopCoroutine(completeBlinkCoroutine);
            }

            completeBlinkCoroutine = StartCoroutine(BlinkCompleteRoutine());
        }

        private void RefreshCooldownVisual()
        {
            if (playerSkill == null)
            {
                return;
            }

            float duration = playerSkill.CooldownDuration;
            float remaining = playerSkill.RemainingCooldown;
            bool onCooldown = remaining > 0.0f && duration > 0.0f;

            if (!onCooldown)
            {
                return;
            }

            if (coolTimeCover != null)
            {
                coolTimeCover.fillAmount = Mathf.Clamp01(remaining / duration);
            }

            if (coolTimeText != null)
            {
                int displaySeconds = Mathf.CeilToInt(remaining);
                coolTimeText.text = displaySeconds.ToString();
            }
        }

        private void ConfigureCoverImage()
        {
            if (coolTimeCover == null)
            {
                return;
            }

            coolTimeCover.type = Image.Type.Filled;
            coolTimeCover.fillMethod = Image.FillMethod.Radial360;
            coolTimeCover.fillClockwise = true;
            coolTimeCover.fillOrigin = (int)Image.Origin360.Top;
        }

        private void SetCooldownVisualActive(bool isActive)
        {
            if (coolTimeCover != null)
            {
                coolTimeCover.gameObject.SetActive(isActive);

                if (isActive)
                {
                    coolTimeCover.fillAmount = 1.0f;
                }
            }

            if (coolTimeText != null)
            {
                coolTimeText.gameObject.SetActive(isActive);
            }
        }

        private void SetCompleteActive(bool isActive)
        {
            if (coolTimeComplete != null)
            {
                coolTimeComplete.SetActive(isActive);
            }
        }

        private IEnumerator BlinkCompleteRoutine()
        {
            SetCompleteActive(true);
            yield return new WaitForSecondsRealtime(CompleteFlashSeconds);
            SetCompleteActive(false);
            completeBlinkCoroutine = null;
        }
    }
}
