using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Prefabs;

/// <summary>
///     This class is used to create a simple button with prefab.
/// </summary>
/// <inheritdoc cref="APrefab{T}" />
public class SimpleButton : APrefab<SimpleButton>
{
    [SerializeField] private Button button;

    [SerializeField] private TipButton tipButton;

    [SerializeField] private Image background;

    [SerializeField] private Image icon;

    [SerializeField] private Text text;

    /// <summary>
    ///     The <see cref="Button" /> component
    /// </summary>
    public Button Button => button;

    /// <summary>
    ///     The <see cref="TipButton" /> component
    /// </summary>
    public TipButton TipButton => tipButton;

    /// <summary>
    ///     The <see cref="Image" /> component of the background
    /// </summary>
    public Image Background => background;

    /// <summary>
    ///     The <see cref="Image" /> component of the button icon
    /// </summary>
    public Image Icon => icon;

    /// <summary>
    ///     The <see cref="Text" /> component of the button text
    /// </summary>
    public Text Text => text;

    private void Awake()
    {
        if (!Initialized) Init();
    }

    /// <summary>
    ///     Setup the button
    /// </summary>
    /// <param name="pClickAction">Action on button clicked</param>
    /// <param name="pIcon">The icon of button</param>
    /// <param name="pText">The text of button(When it is not null, <paramref name="pIcon" /> will be disabled)</param>
    /// <param name="pSize">The size of button rect</param>
    /// <param name="pTipType">When it is empty, <see cref="SimpleButton.TipButton" /> will be disabled</param>
    /// <param name="pTipData">TooltipData, it is available only when <paramref name="pTipType" /> is not null or empty</param>
    public void Setup(UnityAction pClickAction, Sprite pIcon, string pText = null, Vector2 pSize = default,
        string pTipType = null,
        TooltipData pTipData = default)
    {
        if (pSize == default)
        {
            pSize = new Vector2(32, 32);
        }

        SetSize(pSize);
        if (string.IsNullOrEmpty(pText))
        {
            Text.gameObject.SetActive(false);
            Icon.gameObject.SetActive(true);
        }
        else
        {
            Icon.gameObject.SetActive(false);
            Text.gameObject.SetActive(true);
        }

        Icon.sprite = pIcon;
        Text.text = pText;
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(pClickAction);

        if (string.IsNullOrEmpty(pTipType))
        {
            this.TipButton.enabled = false;
        }
        else
        {
            this.TipButton.enabled = true;
            this.TipButton.type = pTipType;
            if (string.IsNullOrEmpty(pTipData.tip_name))
            {
                TipButton.hoverAction = TipButton.showTooltipDefault;
            }
            else
            {
                TipButton.hoverAction = () =>
                {
                    Tooltip.show(gameObject, TipButton.type, pTipData);
                    transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                    transform.DOKill();
                    transform.DOScale(1f, 0.1f).SetEase(Ease.InBack);
                };
            }
        }
    }

    /// <inheritdoc cref="APrefab{T}.SetSize" />
    public override void SetSize(Vector2 pSize)
    {
        GetComponent<RectTransform>().sizeDelta = pSize;
        float min_edge = Mathf.Min(pSize.x, pSize.y);
        Icon.GetComponent<RectTransform>().sizeDelta = new Vector2(min_edge, min_edge) * 0.875f;
        Text.GetComponent<RectTransform>().sizeDelta = pSize * 0.875f;
    }

    internal static void _init()
    {
        GameObject obj = new GameObject(nameof(SimpleButton), typeof(Button), typeof(Image), typeof(TipButton));
        obj.transform.SetParent(WorldBoxMod.Transform);
        obj.GetComponent<TipButton>().enabled = false;
        obj.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonRed");
        obj.GetComponent<Image>().type = Image.Type.Sliced;

        GameObject icon = new GameObject("Icon", typeof(Image));
        icon.transform.SetParent(obj.transform);
        icon.transform.localPosition = Vector3.zero;
        icon.transform.localScale = Vector3.one;

        GameObject text = new GameObject("Text", typeof(Text));
        text.transform.SetParent(obj.transform);
        text.transform.localPosition = Vector3.zero;
        text.transform.localScale = Vector3.one;
        Text text_text = text.GetComponent<Text>();
        text_text.font = LocalizedTextManager.current_font;
        text_text.color = Color.white;
        text_text.resizeTextForBestFit = true;
        text_text.resizeTextMinSize = 1;
        text_text.resizeTextMaxSize = 10;
        text_text.alignment = TextAnchor.MiddleCenter;
        text.SetActive(false);

        Prefab = obj.AddComponent<SimpleButton>();
        Prefab.button = obj.GetComponent<Button>();
        Prefab.tipButton = obj.GetComponent<TipButton>();
        Prefab.background = obj.GetComponent<Image>();
        Prefab.icon = icon.GetComponent<Image>();
        Prefab.text = text_text;
    }
}