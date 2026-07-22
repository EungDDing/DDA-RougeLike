using UnityEngine;
using UnityEngine.UI;

namespace DDARoguelike
{
    public class PlayerStatusHud : MonoBehaviour
    {
        private const int StatRowCount = 6;
        private const float RowHeight = 44.0f;
        private const float IconSize = 40.0f;
        private const float Padding = 8.0f;
        private const float RowSpacing = 6.0f;
        private const float IconValueSpacing = 14.0f;

        [SerializeField] private PlayerAttack playerAttack;
        [SerializeField] private PlayerMove playerMove;
        [SerializeField] private RectTransform statusRoot;

        [SerializeField] private Sprite attackIcon;
        [SerializeField] private Sprite moveSpeedIcon;
        [SerializeField] private Sprite fireRateIcon;
        [SerializeField] private Sprite rangeIcon;
        [SerializeField] private Sprite shotSpeedIcon;
        [SerializeField] private Sprite projectileCountIcon;

        [SerializeField] private int fontSize = 22;
        [SerializeField] private Color valueColor = Color.white;

        private readonly Image[] iconImages = new Image[StatRowCount];
        private readonly Text[] valueTexts = new Text[StatRowCount];
        private bool rowsCreated;

        private void Awake()
        {
            ResolvePlayerReferences();

            if (statusRoot == null)
            {
                statusRoot = GetComponent<RectTransform>();
            }

            if (statusRoot == null)
            {
                Debug.LogError($"[{nameof(PlayerStatusHud)}] statusRoot is not assigned on {gameObject.name}.", this);
                return;
            }

            EnsureLayout();
            EnsureRows();
        }

        private void OnEnable()
        {
            if (playerAttack != null)
            {
                playerAttack.StatsChanged += Refresh;
            }

            if (playerMove != null)
            {
                playerMove.StatsChanged += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (playerAttack != null)
            {
                playerAttack.StatsChanged -= Refresh;
            }

            if (playerMove != null)
            {
                playerMove.StatsChanged -= Refresh;
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void ResolvePlayerReferences()
        {
            GameObject playerObject = null;

            if (playerAttack == null || playerMove == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }

            if (playerAttack == null && playerObject != null)
            {
                playerAttack = playerObject.GetComponent<PlayerAttack>();
            }

            if (playerMove == null && playerObject != null)
            {
                playerMove = playerObject.GetComponent<PlayerMove>();
            }

            if (playerAttack == null)
            {
                Debug.LogError($"[{nameof(PlayerStatusHud)}] {nameof(PlayerAttack)} is not assigned on {gameObject.name}.", this);
            }

            if (playerMove == null)
            {
                Debug.LogError($"[{nameof(PlayerStatusHud)}] {nameof(PlayerMove)} is not assigned on {gameObject.name}.", this);
            }
        }

        private void EnsureLayout()
        {
            VerticalLayoutGroup verticalLayout = statusRoot.GetComponent<VerticalLayoutGroup>();

            if (verticalLayout == null)
            {
                verticalLayout = statusRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            verticalLayout.padding = new RectOffset(
                Mathf.RoundToInt(Padding),
                Mathf.RoundToInt(Padding),
                Mathf.RoundToInt(Padding),
                Mathf.RoundToInt(Padding));
            verticalLayout.spacing = RowSpacing;
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = true;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
        }

        private void EnsureRows()
        {
            if (rowsCreated)
            {
                return;
            }

            Sprite[] icons = new Sprite[]
            {
                attackIcon,
                moveSpeedIcon,
                fireRateIcon,
                rangeIcon,
                shotSpeedIcon,
                projectileCountIcon,
            };

            for (int i = 0; i < StatRowCount; i++)
            {
                CreateRow(i, icons[i]);
            }

            rowsCreated = true;
        }

        private void CreateRow(int index, Sprite iconSprite)
        {
            GameObject rowObject = new GameObject(
                $"StatRow_{index}",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup),
                typeof(LayoutElement));
            RectTransform rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.SetParent(statusRoot, false);

            LayoutElement rowLayoutElement = rowObject.GetComponent<LayoutElement>();
            rowLayoutElement.minHeight = RowHeight;
            rowLayoutElement.preferredHeight = RowHeight;

            HorizontalLayoutGroup horizontalLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = IconValueSpacing;
            horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
            horizontalLayout.childControlWidth = true;
            horizontalLayout.childControlHeight = true;
            horizontalLayout.childForceExpandWidth = false;
            horizontalLayout.childForceExpandHeight = false;

            GameObject iconObject = new GameObject(
                "Icon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(LayoutElement));
            iconObject.transform.SetParent(rowObject.transform, false);

            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);

            LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
            iconLayout.minWidth = IconSize;
            iconLayout.preferredWidth = IconSize;
            iconLayout.flexibleWidth = 0.0f;
            iconLayout.minHeight = IconSize;
            iconLayout.preferredHeight = IconSize;
            iconLayout.flexibleHeight = 0.0f;

            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImages[index] = iconImage;

            GameObject valueObject = new GameObject(
                "Value",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text),
                typeof(LayoutElement));
            valueObject.transform.SetParent(rowObject.transform, false);

            LayoutElement valueLayout = valueObject.GetComponent<LayoutElement>();
            valueLayout.minWidth = 60.0f;
            valueLayout.preferredWidth = 60.0f;
            valueLayout.flexibleWidth = 1.0f;
            valueLayout.minHeight = RowHeight;
            valueLayout.preferredHeight = RowHeight;
            valueLayout.flexibleHeight = 0.0f;

            Text valueText = valueObject.GetComponent<Text>();
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valueText.fontSize = fontSize;
            valueText.color = valueColor;
            valueText.alignment = TextAnchor.MiddleLeft;
            valueText.horizontalOverflow = HorizontalWrapMode.Overflow;
            valueText.verticalOverflow = VerticalWrapMode.Overflow;
            valueText.raycastTarget = false;
            valueTexts[index] = valueText;
        }

        private void Refresh()
        {
            if (!rowsCreated)
            {
                return;
            }

            if (playerAttack != null)
            {
                valueTexts[0].text = FormatFloat(playerAttack.AttackPower);
                valueTexts[2].text = FormatFloat(playerAttack.FireRate);
                valueTexts[3].text = FormatFloat(playerAttack.AttackRange);
                valueTexts[4].text = FormatFloat(playerAttack.ShotSpeed);
                valueTexts[5].text = playerAttack.ProjectileCount.ToString();
            }

            if (playerMove != null)
            {
                valueTexts[1].text = FormatFloat(playerMove.MoveSpeed);
            }
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("F2");
        }
    }
}
