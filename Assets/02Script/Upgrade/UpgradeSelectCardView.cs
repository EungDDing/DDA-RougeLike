using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DDARoguelike
{
    public class UpgradeSelectCardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI effectExplainText;
        [SerializeField] private Image categoryImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button selectButton;

        private UpgradeOffer boundOffer;
        private Action<UpgradeOffer> selectedCallback;

        public UpgradeOffer BoundOffer => boundOffer;

        private void Awake()
        {
            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
            }

            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }

            if (selectButton == null)
            {
                Debug.LogError($"[{nameof(UpgradeSelectCardView)}] Button is not assigned on {gameObject.name}.", this);
                return;
            }

            selectButton.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(HandleClicked);
            }
        }

        public void Bind(UpgradeOffer offer, UpgradeCatalog catalog, Action<UpgradeOffer> onSelected)
        {
            boundOffer = offer;
            selectedCallback = onSelected;

            if (offer == null || offer.Definition == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (nameText != null)
            {
                nameText.text = offer.DisplayName;
            }

            if (categoryText != null)
            {
                categoryText.text = GetCategoryDisplayName(offer.Category);
            }

            if (rankText != null)
            {
                if (catalog != null)
                {
                    rankText.text = catalog.GetGradeDisplayName(offer.Grade);
                }
                else
                {
                    rankText.text = offer.Grade.ToString();
                }
            }

            if (effectExplainText != null)
            {
                effectExplainText.text = offer.BuildDescription();
            }

            if (categoryImage != null)
            {
                Sprite displaySprite = ResolveDisplaySprite(offer, catalog);
                categoryImage.sprite = displaySprite;
                categoryImage.enabled = displaySprite != null;
            }

            if (catalog != null)
            {
                ApplyGradeColors(offer.Grade, catalog);
            }

            if (selectButton != null)
            {
                selectButton.interactable = true;
            }
        }

        public void SetInteractable(bool isInteractable)
        {
            if (selectButton != null)
            {
                selectButton.interactable = isInteractable;
            }
        }

        private void ApplyGradeColors(int grade, UpgradeCatalog catalog)
        {
            Color backgroundColor = catalog.GetGradeBackgroundColor(grade);
            Color textColor = catalog.GetGradeTextColor(grade);

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            if (rankText != null)
            {
                rankText.color = textColor;
            }

            if (nameText != null)
            {
                nameText.color = Color.white;
            }

            if (categoryText != null)
            {
                categoryText.color = Color.white;
            }

            if (effectExplainText != null)
            {
                effectExplainText.color = Color.white;
            }
        }

        private void HandleClicked()
        {
            if (boundOffer == null || selectedCallback == null)
            {
                return;
            }

            selectedCallback.Invoke(boundOffer);
        }

        private static Sprite ResolveDisplaySprite(UpgradeOffer offer, UpgradeCatalog catalog)
        {
            if (offer != null && offer.Definition != null && offer.Definition.IconOverride != null)
            {
                return offer.Definition.IconOverride;
            }

            if (catalog == null || offer == null)
            {
                return null;
            }

            return catalog.GetCategoryImage(offer.Category);
        }

        private static string GetCategoryDisplayName(UpgradeCategory category)
        {
            switch (category)
            {
                case UpgradeCategory.Attack:
                    return "일반 공격";
                case UpgradeCategory.Skill:
                    return "고유 스킬";
                case UpgradeCategory.Survival:
                    return "생존";
                default:
                    return category.ToString();
            }
        }
    }
}
