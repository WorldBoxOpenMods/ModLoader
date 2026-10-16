using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     The Mod Hub's look in one place: spacing, colours, font sizes and the game sprites it uses, and factories
///     that build laid-out panels, labels and buttons from them. Windows arrange what these return with layout
///     groups instead of fixed positions.
/// </summary>
internal static class ModHubStyle
{
    public const int SpaceS = 4;
    public const int SpaceM = 8;
    public const int SpaceL = 12;

    public const int FontTitle = 9;
    public const int FontBody = 7;
    public const int FontSmall = 6;

    public const float ButtonHeight = 16;
    public const float IconButtonSize = 16;
    public const float ToolbarButtonSize = 20;

    public static readonly Color TextBright = new(0.96f, 0.96f, 0.92f);
    public static readonly Color TextMuted = new(0.7f, 0.72f, 0.66f);
    public static readonly Color Positive = new(0.45f, 0.9f, 0.45f);
    public static readonly Color Negative = new(1f, 0.42f, 0.36f);
    public static readonly Color Warning = new(1f, 0.72f, 0.25f);
    public static readonly Color DividerColor = new(0f, 0f, 0f, 0.35f);

    // The room the stock frame's title and lower edge take inside the background.
    private const float frame_top = 32;
    private const float frame_bottom = 10;
    private const float side_tab_icon_size = 18;

    // The game's own sprites, so the Hub matches the window frame around it.
    public static Sprite Card => SpriteTextureLoader.getSprite("ui/special/windowInnerSlicedRound");
    public static Sprite Button => SpriteTextureLoader.getSprite("ui/special/special_buttonGray");
    public static Sprite PrimaryButton => SpriteTextureLoader.getSprite("ui/special/special_buttonRed");
    public static Sprite Pill => SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");

    /// <summary>
    ///     Lay out a stock window as a header strip, the window's scroll list, and a footer strip.
    /// </summary>
    /// <returns>The content area, holding the header, the list and the footer in that order</returns>
    public static RectTransform SetUpListWindow(Transform pBackground, out Transform pHeader, out Transform pFooter)
    {
        Transform gradient = pBackground.Find("Scrollgradient");
        if (gradient != null) gradient.gameObject.SetActive(false);
        RectTransform content = ContentArea(pBackground);
        Vertical(content.gameObject, SpaceM).childForceExpandWidth = true;

        pHeader = Strip(content, "Header", ToolbarButtonSize);
        Transform scroll_view = pBackground.Find("Scroll View");
        scroll_view.SetParent(content, false);
        Size(scroll_view.gameObject, pFlexibleHeight: 1);
        FitScrollParts(scroll_view);
        pFooter = Strip(content, "Footer", ButtonHeight);
        return content;
    }

    /// <summary>
    ///     Space the rows of a list window's content evenly, full width.
    /// </summary>
    public static void StyleList(Transform pListContent)
    {
        var layout = pListContent.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = SpaceS;
        layout.childAlignment = TextAnchor.UpperCenter;
    }

    /// <summary>
    ///     A muted message centred over a list, shown when it is empty.
    /// </summary>
    /// <param name="pListContent">The list window's content</param>
    /// <param name="pTextKey">Locale key of the message, or null to set the text in code</param>
    public static Text EmptyListLabel(Transform pListContent, string pTextKey)
    {
        Transform scroll_view = pListContent.parent.parent;
        Text label = Label(scroll_view, "EmptyMessage", FontBody, TextMuted, TextAnchor.MiddleCenter);
        Stretch(label.rectTransform, SpaceL);
        if (pTextKey != null) ModHubUI.SetLocaleKey(label, pTextKey);
        return label;
    }

    /// <summary>
    ///     One more toolbar strip like the header, at <paramref name="pIndex" /> in the content of
    ///     <see cref="SetUpListWindow" />.
    /// </summary>
    public static Transform InsertStrip(Transform pContent, int pIndex, string pName)
    {
        Transform strip = Strip(pContent, pName, ToolbarButtonSize);
        strip.SetSiblingIndex(pIndex);
        return strip;
    }

    /// <summary>
    ///     A slim bar along the bottom edge of <paramref name="pParent" />. Returns the fill for
    ///     <see cref="SetProgress" />.
    /// </summary>
    public static RectTransform ProgressBar(Transform pParent)
    {
        var bar = new GameObject("Progress", typeof(Image));
        bar.transform.SetParent(pParent, false);
        bar.GetComponent<Image>().color = DividerColor;
        bar.GetComponent<Image>().raycastTarget = false;
        var rect = bar.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(0, 2);

        var fill = new GameObject("Fill", typeof(Image));
        fill.transform.SetParent(bar.transform, false);
        fill.GetComponent<Image>().color = Positive;
        fill.GetComponent<Image>().raycastTarget = false;
        var fill_rect = fill.GetComponent<RectTransform>();
        Stretch(fill_rect, 0);
        return fill_rect;
    }

    public static void SetProgress(RectTransform pFill, float pProgress)
    {
        pFill.anchorMax = new Vector2(Mathf.Clamp01(pProgress), 1);
    }

    /// <summary>
    ///     NML's text input, reporting every change so a list can filter while the player types.
    /// </summary>
    public static TextInput SearchBox(Transform pParent, string pText, float pWidth, UnityAction<string> pOnChanged)
    {
        TextInput search = TextInput.Instantiate(pParent, pName: "Search");
        search.Setup(pText, pOnChanged);
        search.SetSize(new Vector2(pWidth, ToolbarButtonSize));
        search.input.onValueChanged.AddListener(pOnChanged);
        Size(search.gameObject, pWidth, ToolbarButtonSize);
        return search;
    }

    public static VerticalLayoutGroup Vertical(GameObject pObject, float pSpacing, int pPadding = 0)
    {
        var layout = pObject.AddComponent<VerticalLayoutGroup>();
        SetUp(layout, pSpacing, pPadding, TextAnchor.UpperLeft);
        return layout;
    }

    public static HorizontalLayoutGroup Horizontal(GameObject pObject, float pSpacing, int pPadding = 0)
    {
        var layout = pObject.AddComponent<HorizontalLayoutGroup>();
        SetUp(layout, pSpacing, pPadding, TextAnchor.MiddleLeft);
        return layout;
    }

    /// <summary>
    ///     Fix the size a layout group gives <paramref name="pObject" />. Negative values leave it to the layout.
    /// </summary>
    public static void Size(GameObject pObject, float pWidth = -1, float pHeight = -1, float pFlexibleWidth = -1,
        float pFlexibleHeight = -1)
    {
        LayoutElement element = pObject.GetComponent<LayoutElement>();
        if (element == null) element = pObject.AddComponent<LayoutElement>();
        element.preferredWidth = pWidth;
        element.minWidth = pWidth;
        element.preferredHeight = pHeight;
        element.minHeight = pHeight;
        element.flexibleWidth = pFlexibleWidth;
        element.flexibleHeight = pFlexibleHeight;
    }

    /// <summary>
    ///     An empty object that takes the free space in a layout row, pushing what follows to the end.
    /// </summary>
    public static void Spacer(Transform pParent)
    {
        var spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(pParent, false);
        Size(spacer, pFlexibleWidth: 1);
    }

    /// <summary>
    ///     A 9-sliced image, such as <see cref="Card" />.
    /// </summary>
    public static GameObject Box(Transform pParent, string pName, Sprite pSprite)
    {
        var obj = new GameObject(pName, typeof(Image));
        obj.transform.SetParent(pParent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = pSprite;
        image.type = Image.Type.Sliced;
        return obj;
    }

    /// <summary>
    ///     A thin line under a title.
    /// </summary>
    public static void Divider(Transform pParent)
    {
        var line = new GameObject("Divider", typeof(Image));
        line.transform.SetParent(pParent, false);
        line.GetComponent<Image>().color = DividerColor;
        line.GetComponent<Image>().raycastTarget = false;
        Size(line, pHeight: 1);
    }

    /// <summary>
    ///     A plain text sized by its layout group. Rich text is off: names and descriptions come from mods.
    /// </summary>
    public static Text Label(Transform pParent, string pName, int pFontSize, Color pColor,
        TextAnchor pAlignment = TextAnchor.MiddleLeft)
    {
        var obj = new GameObject(pName, typeof(Text));
        obj.transform.SetParent(pParent, false);
        Text text = obj.GetComponent<Text>();
        OT.InitializeCommonText(text);
        text.supportRichText = false;
        text.fontSize = pFontSize;
        text.color = pColor;
        text.alignment = pAlignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>
    ///     A tab on the left edge of a stock window, in the container the game uses for its own window tabs.
    /// </summary>
    /// <param name="pWindow">The window, parent of its background</param>
    /// <param name="pName">Name of the tab object</param>
    /// <param name="pIcon">Icon shown on the tab</param>
    /// <param name="pCurrent">Whether the tab is the window it is on; the others are dimmed</param>
    /// <param name="pOnClick">Run on click</param>
    public static TipButton SideTab(Transform pWindow, string pName, Sprite pIcon, bool pCurrent, Action pOnClick)
    {
        // The back button's tab, so these match the frame.
        Sprite sprite = pWindow.Find("BackButtonContainer/BackButtonBackground").GetComponent<Image>().sprite;
        GameObject tab = Box(pWindow.Find("Background/Tabs"), pName, sprite);
        tab.GetComponent<Image>().color = pCurrent ? Color.white : new Color(0.6f, 0.6f, 0.6f);
        tab.AddComponent<Button>().onClick.AddListener(() => pOnClick());
        var tip = tab.AddComponent<TipButton>();
        tip.type = "normal";

        Image icon = Icon(tab.transform, "Icon", pIcon, side_tab_icon_size);
        var icon_rect = icon.rectTransform;
        icon_rect.anchorMin = icon_rect.anchorMax = new Vector2(0.5f, 0.5f);
        icon_rect.sizeDelta = new Vector2(side_tab_icon_size, side_tab_icon_size);
        return tip;
    }

    /// <summary>
    ///     A short muted text in a dark pill, such as how long ago a mod was updated.
    /// </summary>
    public static Text PillLabel(Transform pParent, string pName, float pWidth)
    {
        GameObject pill = Box(pParent, pName, Pill);
        pill.GetComponent<Image>().raycastTarget = false;
        Size(pill, pWidth, IconButtonSize);
        Text text = Label(pill.transform, "Text", FontSmall, TextMuted, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform, 0);
        return text;
    }

    /// <summary>
    ///     A tooltip on hover. The game's click animation keeps its normal size (1), so text in a list does not
    ///     jump or blink.
    /// </summary>
    public static TipButton HoverTip(GameObject pObject)
    {
        var tip = pObject.AddComponent<TipButton>();
        tip.type = "normal";
        tip.override_click_scale_animation = true;
        tip.overridden_click_scale_animation = 1;
        return tip;
    }

    public static Image Icon(Transform pParent, string pName, Sprite pSprite, float pSize)
    {
        var obj = new GameObject(pName, typeof(Image));
        obj.transform.SetParent(pParent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = pSprite;
        image.preserveAspect = true;
        Size(obj, pSize, pSize);
        return image;
    }

    /// <summary>
    ///     A small flat icon button with a tooltip title from <paramref name="pTooltipKey" />.
    /// </summary>
    /// <remarks>Pass a null click action when building a prefab: Unity does not copy listeners to clones.</remarks>
    public static SimpleButton IconButton(Transform pParent, string pName, Sprite pIcon, UnityAction pOnClick,
        string pTooltipKey, float pSize = IconButtonSize)
    {
        SimpleButton button = ModHubUI.CreateIconButton(pParent, pName, pIcon, new Vector2(pSize, pSize),
            Vector2.zero, pOnClick, pTooltipKey);
        button.Background.sprite = Button;
        Size(button.gameObject, pSize, pSize);
        return button;
    }

    /// <summary>
    ///     A slightly larger <see cref="IconButton" /> for the strips above a list.
    /// </summary>
    public static SimpleButton ToolbarButton(Transform pParent, string pName, Sprite pIcon, UnityAction pOnClick,
        string pTooltipKey)
    {
        return IconButton(pParent, pName, pIcon, pOnClick, pTooltipKey, ToolbarButtonSize);
    }

    /// <summary>
    ///     A text button that shares the free width of its row. Red for the one primary action, grey otherwise.
    /// </summary>
    public static SimpleButton TextButton(Transform pParent, string pName, string pTextKey, UnityAction pOnClick,
        bool pPrimary = false)
    {
        SimpleButton button = ModHubUI.CreateTextButton(pParent, pName, pTextKey,
            new Vector2(60, ButtonHeight), Vector2.zero, pOnClick);
        button.Background.sprite = pPrimary ? PrimaryButton : Button;
        button.Text.resizeTextForBestFit = false;
        button.Text.fontSize = FontBody;
        Size(button.gameObject, pHeight: ButtonHeight, pFlexibleWidth: 1);
        return button;
    }

    private static void Stretch(RectTransform pRect, float pMargin)
    {
        pRect.anchorMin = Vector2.zero;
        pRect.anchorMax = Vector2.one;
        pRect.offsetMin = new Vector2(pMargin, 0);
        pRect.offsetMax = new Vector2(-pMargin, 0);
    }

    private static RectTransform ContentArea(Transform pBackground)
    {
        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(pBackground, false);
        var rect = content.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(SpaceL, frame_bottom + SpaceL);
        rect.offsetMax = new Vector2(-SpaceL, -(frame_top + SpaceL));
        return rect;
    }

    private static Transform Strip(Transform pParent, string pName, float pHeight)
    {
        var strip = new GameObject(pName, typeof(RectTransform));
        strip.transform.SetParent(pParent, false);
        Horizontal(strip, SpaceS);
        Size(strip, pHeight: pHeight);
        return strip.transform;
    }

    /// <summary>
    ///     The stock scroll view places its viewport and scrollbar for a fixed size; stretch them over whatever
    ///     size the layout gives it, with the scrollbar at the right edge.
    /// </summary>
    private static void FitScrollParts(Transform pScrollView)
    {
        var scrollbar = pScrollView.Find("Scrollbar Vertical Mask") as RectTransform;
        float scrollbar_width = scrollbar == null ? 0 : scrollbar.sizeDelta.x;
        if (scrollbar != null)
        {
            scrollbar.anchorMin = new Vector2(1, 0);
            scrollbar.anchorMax = Vector2.one;
            scrollbar.pivot = new Vector2(1, 0.5f);
            scrollbar.anchoredPosition = Vector2.zero;
            scrollbar.sizeDelta = new Vector2(scrollbar_width, 0);
        }

        var viewport = (RectTransform)pScrollView.Find("Viewport");
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.pivot = new Vector2(0, 1);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(-(scrollbar_width + SpaceS), 0);
    }

    private static void SetUp(HorizontalOrVerticalLayoutGroup pLayout, float pSpacing, int pPadding,
        TextAnchor pAlignment)
    {
        pLayout.spacing = pSpacing;
        pLayout.padding = new RectOffset(pPadding, pPadding, pPadding, pPadding);
        pLayout.childAlignment = pAlignment;
        pLayout.childControlWidth = true;
        pLayout.childControlHeight = true;
        pLayout.childForceExpandWidth = false;
        pLayout.childForceExpandHeight = false;
    }
}
