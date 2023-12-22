using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Prefabs;

public class SimpleButton : APrefab<SimpleButton>
{
    public Button Button { get; private set; }
    public TipButton TipButton { get; private set; }
    public Image Background { get; private set; }
    public Image Icon { get; private set; }
    public Text Text { get; private set; }

    private void Awake()
    {
        if(!Initialized) Init();
    }

    protected override void Init()
    {
        base.Init();
        Button = GetComponent<Button>();
        Background = GetComponent<Image>();
        Icon = transform.Find("Icon").GetComponent<Image>();
        Text = transform.Find("Text").GetComponent<Text>();
        TipButton = GetComponent<TipButton>();
    }

    public void Setup(UnityAction pClickAction, Sprite pIcon, string pText = null, Vector2 pSize = default, string pTipType = null,
        TooltipData pTipData = default)
    {
        if(!Initialized) Init();
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

    private void SetSize(Vector2 pSize)
    {
        GetComponent<RectTransform>().sizeDelta = pSize;
        Icon.GetComponent<RectTransform>().sizeDelta = pSize * 0.875f;
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
        text_text.font = LocalizedTextManager.currentFont;
        text_text.color = Color.white;
        text_text.resizeTextForBestFit = true;
        text_text.resizeTextMinSize = 1;
        text_text.resizeTextMaxSize = 10;
        text_text.alignment = TextAnchor.MiddleCenter;
        text.SetActive(false);

        Prefab = obj.AddComponent<SimpleButton>();
    }
}