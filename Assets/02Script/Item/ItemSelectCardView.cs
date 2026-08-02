using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DDARoguelike
{
    public class ItemSelectCardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button selectButton;

        private ItemDefinition boundDefinition;
        private Action<ItemDefinition> selectedCallback;

        private void Awake()
        {
            ResolveReferences();

            if (selectButton == null)
            {
                Debug.LogError($"[{nameof(ItemSelectCardView)}] Button is missing on {gameObject.name}.", this);
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

        public void Bind(ItemDefinition definition, Action<ItemDefinition> onSelected)
        {
            ResolveReferences();
            boundDefinition = definition;
            selectedCallback = onSelected;

            if (definition == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (nameText != null)
            {
                nameText.text = definition.DisplayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = definition.Description;
            }

            if (iconImage != null)
            {
                iconImage.sprite = definition.Icon;
                iconImage.enabled = definition.Icon != null;
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

        private void ResolveReferences()
        {
            if (nameText == null)
            {
                Transform nameTransform = transform.Find("ItemName");

                if (nameTransform != null)
                {
                    nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                }
            }

            if (descriptionText == null)
            {
                Transform effectTransform = transform.Find("ItemEffect");

                if (effectTransform != null)
                {
                    descriptionText = effectTransform.GetComponent<TextMeshProUGUI>();
                }
            }

            if (iconImage == null)
            {
                Transform iconTransform = transform.Find("ItemIconBackground/ItemIconImage");

                if (iconTransform != null)
                {
                    iconImage = iconTransform.GetComponent<Image>();
                }
            }

            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
            }

            if (selectButton == null)
            {
                selectButton = gameObject.AddComponent<Button>();
            }
        }

        private void HandleClicked()
        {
            if (boundDefinition == null || selectedCallback == null)
            {
                return;
            }

            selectedCallback.Invoke(boundDefinition);
        }
    }
}
