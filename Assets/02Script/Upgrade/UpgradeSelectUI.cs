using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class UpgradeSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private UpgradeSelectCardView[] cardViews;

        private Action<UpgradeOffer> selectedCallback;
        private bool isOpen;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            HideImmediate();
        }

        public void Show(IReadOnlyList<UpgradeOffer> offers, UpgradeCatalog catalog, Action<UpgradeOffer> onSelected)
        {
            if (cardViews == null || cardViews.Length == 0)
            {
                Debug.LogError($"[{nameof(UpgradeSelectUI)}] cardViews is empty on {gameObject.name}.", this);
                return;
            }

            selectedCallback = onSelected;
            isOpen = true;

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            for (int i = 0; i < cardViews.Length; i++)
            {
                UpgradeSelectCardView cardView = cardViews[i];

                if (cardView == null)
                {
                    continue;
                }

                if (offers != null && i < offers.Count)
                {
                    cardView.Bind(offers[i], catalog, HandleCardSelected);
                }
                else
                {
                    cardView.Bind(null, catalog, null);
                }
            }
        }

        public void Hide()
        {
            HideImmediate();
        }

        private void HideImmediate()
        {
            isOpen = false;
            selectedCallback = null;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void HandleCardSelected(UpgradeOffer offer)
        {
            if (!isOpen)
            {
                return;
            }

            Action<UpgradeOffer> callback = selectedCallback;
            SetCardsInteractable(false);

            if (callback != null)
            {
                callback.Invoke(offer);
            }
        }

        private void SetCardsInteractable(bool isInteractable)
        {
            if (cardViews == null)
            {
                return;
            }

            for (int i = 0; i < cardViews.Length; i++)
            {
                if (cardViews[i] != null)
                {
                    cardViews[i].SetInteractable(isInteractable);
                }
            }
        }
    }
}
