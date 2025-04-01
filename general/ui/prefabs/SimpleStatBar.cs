using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Prefabs;

/// <summary>
///     This class is used to create a simple stat bar(like health bar) with prefab.
/// </summary>
/// <inheritdoc cref="APrefab{T}" />
public class SimpleStatBar : APrefab<SimpleStatBar>
{
    [SerializeField] private Image _background;

    [SerializeField] private Image _bar;

    [SerializeField] private Image _icon;

    [SerializeField] private StatBar _stat_bar;

    public Image background => _background;
    public Image bar => _bar;
    public Image icon => _icon;
    public StatBar stat_bar => _stat_bar;

    /// <summary>
    /// </summary>
    /// <param name="value">Bar's value and displayed text</param>
    /// <param name="max_value">Bar's max value, not displayed</param>
    /// <param name="pEndText">
    ///     The text append to the end of <paramref name="value" />. If you want to display max value,
    ///     append it here
    /// </param>
    /// <param name="pIcon">The icon at the left</param>
    /// <param name="pBackground">The background of bar</param>
    /// <param name="pBarColor">The color of bar</param>
    /// <param name="pSize">Size of background, bar's size will be changed automatically</param>
    /// <param name="pReset"></param>
    /// <param name="pFloat"></param>
    /// <param name="pUpdateText"></param>
    /// <param name="pWithoutTween"></param>
    public virtual void Setup(float value, float max_value, string pEndText, Sprite pIcon, Sprite pBackground,
        Color pBarColor,
        Vector2 pSize, bool pReset = true, bool pFloat = false, bool pUpdateText = true,
        float pSpeed = 0.3f)
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

        UpdateBar(value, max_value, pEndText, pBarColor, pReset, pFloat, pUpdateText, pSpeed);
    }

    /// <summary>
    ///     Update Bar and Bar Text
    /// </summary>
    /// <param name="value"></param>
    /// <param name="max_value"></param>
    /// <param name="pEndText"></param>
    /// <param name="pBarColor"></param>
    /// <param name="pReset"></param>
    /// <param name="pFloat"></param>
    /// <param name="pUpdateText"></param>
    /// <param name="pSpeed"></param>
    public void UpdateBar(float value, float max_value, string pEndText, Color pBarColor = default, bool pReset = true,
        bool pFloat = false, bool pUpdateText = true, float pSpeed = 0.3f)
    {
        if (!Initialized) Init();
        if (pBarColor != default)
        {
            bar.color = pBarColor;
        }

        stat_bar.setBar(value, max_value, pEndText, pReset, pFloat, pUpdateText, pSpeed);
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
        text_text.font = LocalizedTextManager.current_font;

        stat_bar_obj.SetActive(false);
        StatBar stat_bar = stat_bar_obj.AddComponent<StatBar>();
        stat_bar.textField = text_text;
        stat_bar.mask = mask.GetComponent<RectTransform>();
        stat_bar.bar = background.GetComponent<RectTransform>();
        stat_bar_obj.SetActive(true);
        Prefab = stat_bar_obj.AddComponent<SimpleStatBar>();
        Prefab._background = image_background;
        Prefab._bar = image_bar;
        Prefab._icon = image_icon;
        Prefab._stat_bar = stat_bar;
    }
}