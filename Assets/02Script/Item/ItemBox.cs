using System.Collections.Generic;
using UnityEngine;

namespace DDARoguelike
{
    public class ItemBox : MonoBehaviour
    {
        private const string ClosedChildName = "Closed";
        private const string OpenedChildName = "Opened";

        [SerializeField] private ItemCatalog catalog;
        [SerializeField] private ItemSelectUI selectUI;
        [SerializeField] private PlayerItemInventory playerInventory;
        [SerializeField] private PlayerMove playerMove;
        [SerializeField] private PlayerAttack playerAttack;
        [SerializeField] private PlayerSkill playerSkill;
        [SerializeField] private PlayerBomb playerBomb;

        private readonly ItemRoller roller = new ItemRoller();
        private readonly List<ItemDefinition> rolledItems = new List<ItemDefinition>();

        private GameObject closedObject;
        private GameObject openedObject;
        private bool isOpened;
        private bool isSelecting;
        private float previousTimeScale = 1.0f;

        private void Awake()
        {
            Transform closedTransform = transform.Find(ClosedChildName);
            Transform openedTransform = transform.Find(OpenedChildName);

            if (closedTransform == null)
            {
                Debug.LogError($"[{nameof(ItemBox)}] Child '{ClosedChildName}' was not found on {gameObject.name}.", this);
            }
            else
            {
                closedObject = closedTransform.gameObject;
            }

            if (openedTransform == null)
            {
                Debug.LogError($"[{nameof(ItemBox)}] Child '{OpenedChildName}' was not found on {gameObject.name}.", this);
            }
            else
            {
                openedObject = openedTransform.gameObject;
            }

            ResolvePlayerReferences();

            if (catalog == null)
            {
                Debug.LogError($"[{nameof(ItemBox)}] catalog is not assigned on {gameObject.name}.", this);
            }

            if (selectUI == null)
            {
                Debug.LogError($"[{nameof(ItemBox)}] selectUI is not assigned on {gameObject.name}.", this);
            }

            ApplyVisualState(false);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isOpened || isSelecting)
            {
                return;
            }

            if (collision == null || collision.collider == null)
            {
                return;
            }

            if (!collision.collider.CompareTag("Player"))
            {
                return;
            }

            TryOpen();
        }

        private void TryOpen()
        {
            ResolvePlayerReferences();

            if (playerInventory == null)
            {
                Debug.LogError($"[{nameof(ItemBox)}] {nameof(PlayerItemInventory)} was not found.", this);
                return;
            }

            if (catalog == null || selectUI == null)
            {
                return;
            }

            roller.Roll(catalog, playerInventory.OwnedItems, rolledItems);

            if (rolledItems.Count == 0)
            {
                CompleteOpenedState();
                return;
            }

            isSelecting = true;
            PauseGameplay();
            selectUI.Show(rolledItems, HandleItemSelected);
        }

        private void HandleItemSelected(ItemDefinition definition)
        {
            if (!isSelecting)
            {
                return;
            }

            if (definition != null && playerInventory != null)
            {
                playerInventory.AddItem(definition);
            }

            if (selectUI != null)
            {
                selectUI.Hide();
            }

            isSelecting = false;
            CompleteOpenedState();
            ResumeGameplay();
        }

        private void CompleteOpenedState()
        {
            isOpened = true;
            ApplyVisualState(true);
        }

        private void ApplyVisualState(bool opened)
        {
            if (closedObject != null)
            {
                closedObject.SetActive(!opened);
            }

            if (openedObject != null)
            {
                openedObject.SetActive(opened);
            }
        }

        private void ResolvePlayerReferences()
        {
            if (playerInventory != null
                && playerMove != null
                && playerAttack != null
                && playerSkill != null
                && playerBomb != null)
            {
                return;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject == null)
            {
                return;
            }

            if (playerInventory == null)
            {
                playerInventory = playerObject.GetComponent<PlayerItemInventory>();
            }

            if (playerMove == null)
            {
                playerMove = playerObject.GetComponent<PlayerMove>();
            }

            if (playerAttack == null)
            {
                playerAttack = playerObject.GetComponent<PlayerAttack>();
            }

            if (playerSkill == null)
            {
                playerSkill = playerObject.GetComponent<PlayerSkill>();
            }

            if (playerBomb == null)
            {
                playerBomb = playerObject.GetComponent<PlayerBomb>();
            }
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

        private void OnDisable()
        {
            if (isSelecting)
            {
                isSelecting = false;

                if (selectUI != null)
                {
                    selectUI.Hide();
                }

                ResumeGameplay();
            }
        }
    }
}
