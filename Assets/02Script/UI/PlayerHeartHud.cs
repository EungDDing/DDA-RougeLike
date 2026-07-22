using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DDARoguelike
{
    public class PlayerHeartHud : MonoBehaviour
    {
        private const int HpPerHeart = 2;
        private const int HeartsPerRow = 6;
        private const int MaxRows = 2;
        private const int MaxHeartSlots = HeartsPerRow * MaxRows;

        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private RectTransform heartContainer;
        [SerializeField] private Vector2 heartCellSize = new Vector2(48.0f, 44.0f);
        [SerializeField] private Vector2 heartSpacing = new Vector2(4.0f, 4.0f);

        [SerializeField] private Sprite hpFullSprite;
        [SerializeField] private Sprite hpHalfSprite;
        [SerializeField] private Sprite hpEmptySprite;
        [SerializeField] private Sprite shieldFullSprite;
        [SerializeField] private Sprite shieldHalfSprite;

        private readonly List<Image> heartImages = new List<Image>(MaxHeartSlots);
        private GridLayoutGroup gridLayoutGroup;

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
                Debug.LogError($"[{nameof(PlayerHeartHud)}] {nameof(PlayerHealth)} is not assigned on {gameObject.name}.", this);
            }

            EnsureHeartContainer();
            EnsureGridLayout();
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

        private void EnsureHeartContainer()
        {
            if (heartContainer != null)
            {
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                GameObject canvasObject = new GameObject(
                    "HeartHudCanvas",
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.transform.SetParent(transform, false);

                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920.0f, 1080.0f);
            }

            GameObject containerObject = new GameObject("HeartContainer", typeof(RectTransform));
            heartContainer = containerObject.GetComponent<RectTransform>();
            heartContainer.SetParent(canvas.transform, false);
            heartContainer.anchorMin = new Vector2(0.0f, 1.0f);
            heartContainer.anchorMax = new Vector2(0.0f, 1.0f);
            heartContainer.pivot = new Vector2(0.0f, 1.0f);
            heartContainer.anchoredPosition = new Vector2(24.0f, -24.0f);
            heartContainer.sizeDelta = new Vector2(
                HeartsPerRow * heartCellSize.x + (HeartsPerRow - 1) * heartSpacing.x,
                MaxRows * heartCellSize.y + (MaxRows - 1) * heartSpacing.y);
        }

        private void EnsureGridLayout()
        {
            gridLayoutGroup = heartContainer.GetComponent<GridLayoutGroup>();

            if (gridLayoutGroup == null)
            {
                gridLayoutGroup = heartContainer.gameObject.AddComponent<GridLayoutGroup>();
            }

            gridLayoutGroup.cellSize = heartCellSize;
            gridLayoutGroup.spacing = heartSpacing;
            gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = HeartsPerRow;
        }

        private void Refresh()
        {
            if (playerHealth == null || heartContainer == null)
            {
                return;
            }

            int hpHeartCount = Mathf.CeilToInt(playerHealth.MaxHp / (float)HpPerHeart);
            int shieldHeartCount = Mathf.CeilToInt(playerHealth.Shield / (float)HpPerHeart);
            int totalHeartCount = Mathf.Min(MaxHeartSlots, hpHeartCount + shieldHeartCount);

            EnsureHeartImageCount(totalHeartCount);

            int imageIndex = 0;

            for (int i = 0; i < hpHeartCount && imageIndex < totalHeartCount; i++)
            {
                int unitsInSlot = Mathf.Clamp(playerHealth.CurrentHp - i * HpPerHeart, 0, HpPerHeart);
                heartImages[imageIndex].sprite = GetHpSprite(unitsInSlot);
                heartImages[imageIndex].enabled = heartImages[imageIndex].sprite != null;
                imageIndex++;
            }

            for (int i = 0; i < shieldHeartCount && imageIndex < totalHeartCount; i++)
            {
                int unitsInSlot = Mathf.Clamp(playerHealth.Shield - i * HpPerHeart, 0, HpPerHeart);
                heartImages[imageIndex].sprite = GetShieldSprite(unitsInSlot);
                heartImages[imageIndex].enabled = heartImages[imageIndex].sprite != null;
                imageIndex++;
            }
        }

        private void EnsureHeartImageCount(int requiredCount)
        {
            while (heartImages.Count < requiredCount)
            {
                heartImages.Add(CreateHeartImage(heartImages.Count));
            }

            for (int i = 0; i < heartImages.Count; i++)
            {
                bool isActive = i < requiredCount;
                heartImages[i].gameObject.SetActive(isActive);
            }
        }

        private Image CreateHeartImage(int index)
        {
            GameObject heartObject = new GameObject(
                $"Heart_{index}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            RectTransform rectTransform = heartObject.GetComponent<RectTransform>();
            rectTransform.SetParent(heartContainer, false);

            Image image = heartObject.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private Sprite GetHpSprite(int unitsInSlot)
        {
            if (unitsInSlot >= HpPerHeart)
            {
                return hpFullSprite;
            }

            if (unitsInSlot == 1)
            {
                return hpHalfSprite;
            }

            return hpEmptySprite;
        }

        private Sprite GetShieldSprite(int unitsInSlot)
        {
            if (unitsInSlot >= HpPerHeart)
            {
                return shieldFullSprite;
            }

            if (unitsInSlot == 1)
            {
                return shieldHalfSprite;
            }

            return null;
        }
    }
}
