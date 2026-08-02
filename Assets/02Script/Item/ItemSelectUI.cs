using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class ItemSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private ItemSelectCardView[] cardViews;

        private Action<ItemDefinition> selectedCallback;
        private bool isOpen;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            if (cardViews == null || cardViews.Length == 0)
            {
                cardViews = panelRoot.GetComponentsInChildren<ItemSelectCardView>(true);
            }

            if (cardViews == null || cardViews.Length == 0)
            {
                EnsureCardViewsOnSelectionCards();
                cardViews = panelRoot.GetComponentsInChildren<ItemSelectCardView>(true);
            }

            HideImmediate();
        }

        private void EnsureCardViewsOnSelectionCards()
        {
            if (panelRoot == null)
            {
                return;
            }

            Transform selectGroup = panelRoot.transform.Find("SelectGroup");

            if (selectGroup == null)
            {
                return;
            }

            for (int i = 0; i < selectGroup.childCount; i++)
            {
                Transform cardTransform = selectGroup.GetChild(i);

                if (cardTransform.GetComponent<ItemSelectCardView>() == null)
                {
                    cardTransform.gameObject.AddComponent<ItemSelectCardView>();
                }
            }
        }

        public void Show(IReadOnlyList<ItemDefinition> definitions, Action<ItemDefinition> onSelected)
        {
            if (cardViews == null || cardViews.Length == 0)
            {
                Debug.LogError($"[{nameof(ItemSelectUI)}] cardViews is empty on {gameObject.name}.", this);
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
                ItemSelectCardView cardView = cardViews[i];

                if (cardView == null)
                {
                    continue;
                }

                if (definitions != null && i < definitions.Count)
                {
                    cardView.Bind(definitions[i], HandleCardSelected);
                }
                else
                {
                    cardView.Bind(null, null);
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

        private void HandleCardSelected(ItemDefinition definition)
        {
            if (!isOpen)
            {
                return;
            }

            Action<ItemDefinition> callback = selectedCallback;
            SetCardsInteractable(false);

            if (callback != null)
            {
                callback.Invoke(definition);
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
