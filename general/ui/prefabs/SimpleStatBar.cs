using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Prefabs;

public class SimpleStatBar : APrefab<SimpleStatBar>
{
    private Image background;
    private Image bar;
    private Image icon;
    private StatBar stat_bar;

    private void Awake()
    {
        if (!Initialized) Init();
    }

    protected override void Init()
    {
        base.Init();
        stat_bar = GetComponent<StatBar>();
        background = GetComponent<Image>();
        icon = transform.Find("Icon").GetComponent<Image>();
        bar = transform.Find("Mask/Bar").GetComponent<Image>();
    }

    public virtual void Setup(float value, float max_value, string pEndText, Sprite pIcon, Sprite pBackground,
        Color pBarColor,
        Vector2 pSize, bool pReset = true, bool pFloat = false, bool pUpdateText = true, bool pWithoutTween = false)
    {
        if (!Initialized) Init();
        icon.sprite = pIcon;
        background.sprite = pBackground;

        if (pBackground == null) background.enabled = false;
        else background.enabled = true;

        GetComponent<RectTransform>().sizeDelta = pSize;

        var bar_size = pSize - new Vector2(pSize.y + 4, pSize.y * 0.3f);
        transform.Find("Background").GetComponent<RectTransform>().sizeDelta = bar_size;
        transform.Find("Background").localPosition =
            new Vector3((pSize.x - bar_size.x) / 2 - pSize.x * 0.02f, 0);

        transform.Find("Mask").GetComponent<RectTransform>().sizeDelta = bar_size;
        transform.Find("Mask").localPosition =
            new Vector3((pSize.x - bar_size.x) / 2 - pSize.x * 0.02f - bar_size.x / 2, 0);
        bar.GetComponent<RectTransform>().sizeDelta = bar_size;
        bar.transform.localPosition = new Vector3(bar_size.x / 2, 0);

        icon.transform.localPosition = new Vector3(-pSize.x / 2 + pSize.y / 2, 0, 0);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(pSize.y, pSize.y);

        transform.Find("Text").GetComponent<RectTransform>().sizeDelta = new Vector2(bar_size.x, bar_size.y);
        transform.Find("Text").localPosition =
            new Vector3((pSize.x - bar_size.x) / 2 - pSize.x * 0.02f, 0);

        UpdateBar(value, max_value, pEndText, pBarColor, pReset, pFloat, pUpdateText, pWithoutTween);
    }

    public void UpdateBar(float value, float max_value, string pEndText, Color pBarColor = default, bool pReset = true,
        bool pFloat = false, bool pUpdateText = true, bool pWithoutTween = false)
    {
        if (!Initialized) Init();
        if (pBarColor != default)
        {
            bar.color = pBarColor;
        }

        stat_bar.setBar(value, max_value, pEndText, pReset, pFloat, pUpdateText, pWithoutTween);
    }

    internal static void _init()
    {
        GameObject stat_bar_obj = new("SimpleStatBar", typeof(Button), typeof(TipButton),
            typeof(Image));
        stat_bar_obj.transform.SetParent(WorldBoxMod.Transform);
        stat_bar_obj.transform.localScale = Vector3.one;
        stat_bar_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 14f);
        stat_bar_obj.GetComponent<Image>().type = Image.Type.Sliced;

        GameObject background = new("Background", typeof(Image));
        background.transform.SetParent(stat_bar_obj.transform);
        Image image_background = background.GetComponent<Image>();
        image_background.sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        image_background.type = Image.Type.Sliced;
        image_background.color = new Color(0.49f, 0.49f, 0.49f);


        GameObject mask = new("Mask", typeof(Image), typeof(Mask));
        mask.transform.SetParent(stat_bar_obj.transform);
        Mask mask_mask = mask.GetComponent<Mask>();
        mask_mask.showMaskGraphic = false;
        mask.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
        mask.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
        mask.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);


        GameObject bar = new("Bar", typeof(Image));
        bar.transform.SetParent(mask.transform);
        Image image_bar = bar.GetComponent<Image>();
        image_bar.sprite = SpriteTextureLoader.getSprite("ui/special/windowBar");
        image_bar.type = Image.Type.Sliced;
        bar.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
        bar.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);

        GameObject icon = new("Icon", typeof(Image), typeof(Shadow));
        icon.transform.SetParent(stat_bar_obj.transform);
        Image image_icon = icon.GetComponent<Image>();
        image_icon.sprite = SpriteTextureLoader.getSprite("ui/icons/iconHealth");

        GameObject text = new("Text", typeof(Text), typeof(Shadow));
        text.transform.SetParent(stat_bar_obj.transform);
        Text text_text = text.GetComponent<Text>();
        text_text.text = "0/0";
        text_text.resizeTextForBestFit = true;
        text_text.resizeTextMaxSize = 10;
        text_text.resizeTextMinSize = 1;
        text_text.alignment = TextAnchor.UpperCenter;
        text_text.color = Color.white;
        text_text.font = LocalizedTextManager.currentFont;

        stat_bar_obj.SetActive(false);
        StatBar stat_bar = stat_bar_obj.AddComponent<StatBar>();
        stat_bar.textField = text_text;
        stat_bar.mask = mask.GetComponent<RectTransform>();
        stat_bar.bar = background.GetComponent<RectTransform>();
        stat_bar_obj.SetActive(true);
        Prefab = stat_bar_obj.AddComponent<SimpleStatBar>();
    }
}