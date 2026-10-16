using System.Reflection;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     The Mod Hub's look in one place: spacing, colours, font sizes and sprites, and factories that build
///     laid-out panels, labels and buttons from them. Windows arrange what these return with layout groups
///     instead of fixed positions.
/// </summary>
internal static class ModHubStyle
{
    public const int SpaceS = 4;
    public const int SpaceM = 8;
    public const int SpaceL = 12;

    public const int FontTitle = 9;
    public const int FontBody = 7;
    public const int FontSmall = 6;

    public static readonly Color TextBright = new(0.96f, 0.96f, 0.92f);
    public static readonly Color TextMuted = new(0.7f, 0.72f, 0.66f);
    public static readonly Color Positive = new(0.45f, 0.9f, 0.45f);
    public static readonly Color Negative = new(1f, 0.42f, 0.36f);
    public static readonly Color Warning = new(1f, 0.72f, 0.25f);
    public static readonly Color DividerColor = new(0f, 0f, 0f, 0.35f);

    // The stock window frame: its width, and the room its title and lower edge take inside the background.
    private const float window_width = 240;
    private const float frame_top = 32;
    private const float frame_bottom = 20;

    private static readonly Dictionary<string, Sprite> sprites = new();

    public static Sprite Card => LoadSprite("card", 4);
    public static Sprite Panel => LoadSprite("panel", 4);
    public static Sprite Button => LoadSprite("button", 4);
    public static Sprite PrimaryButton => LoadSprite("button_primary", 4);
    public static Sprite Pill => LoadSprite("pill", 3);

    /// <summary>
    ///     Resize a stock window to fit <paramref name="pContentHeight" /> and return its content area: a rect
    ///     inside the frame with <see cref="SpaceL" /> margins, to fill with layout groups.
    /// </summary>
    public static RectTransform SetUpWindow(Transform pBackground, float pContentHeight)
    {
        pBackground.Find("Scroll View").gameObject.SetActive(false);
        ResizeFrame(pBackground, frame_top + frame_bottom + 2 * SpaceL + pContentHeight);

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(pBackground, false);
        var rect = content.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(SpaceL, frame_bottom + SpaceL);
        rect.offsetMax = new Vector2(-SpaceL, -(frame_top + SpaceL));
        return rect;
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
    public static void Size(GameObject pObject, float pWidth = -1, float pHeight = -1, float pFlexibleWidth = -1)
    {
        LayoutElement element = pObject.GetComponent<LayoutElement>();
        if (element == null) element = pObject.AddComponent<LayoutElement>();
        element.preferredWidth = pWidth;
        element.minWidth = pWidth;
        element.preferredHeight = pHeight;
        element.minHeight = pHeight;
        element.flexibleWidth = pFlexibleWidth;
    }

    /// <summary>
    ///     A 9-sliced image, such as <see cref="Card" /> or <see cref="Panel" />.
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
    ///     A box that brightens on hover and runs <paramref name="pOnClick" />.
    /// </summary>
    public static Button Clickable(GameObject pBox, Action pOnClick)
    {
        Button button = pBox.AddComponent<Button>();
        button.targetGraphic = pBox.GetComponent<Image>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f);
        button.colors = colors;
        if (pOnClick != null) button.onClick.AddListener(() => pOnClick());
        return button;
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

    /// <summary>
    ///     Change the height of a stock window. Parts placed from the window's centre (title, close and back
    ///     buttons) move with the top or bottom edge they belong to.
    /// </summary>
    private static void ResizeFrame(Transform pBackground, float pHeight)
    {
        var rect = pBackground.GetComponent<RectTransform>();
        float shift = (pHeight - rect.sizeDelta.y) / 2;
        rect.sizeDelta = new Vector2(window_width, pHeight);

        foreach (Transform part in pBackground) MoveWithEdge(part, shift, true);
        foreach (Transform part in pBackground.parent)
        {
            if (part != pBackground) MoveWithEdge(part, shift, false);
        }
    }

    private static void MoveWithEdge(Transform pPart, float pShift, bool pInsideBackground)
    {
        var rect = pPart as RectTransform;
        if (rect == null) return;
        // Inside the background, parts anchored to an edge follow it already.
        if (pInsideBackground && !Mathf.Approximately(rect.anchorMin.y, 0.5f)) return;
        rect.anchoredPosition += new Vector2(0, rect.anchoredPosition.y >= 0 ? pShift : -pShift);
    }

    private static Sprite LoadSprite(string pName, int pBorder)
    {
        if (sprites.TryGetValue(pName, out Sprite cached)) return cached;

        using Stream stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"NeoModLoader.resources.mod_hub.{pName}.png");
        var bytes = new byte[stream!.Length];
        stream.Read(bytes, 0, bytes.Length);
        var texture = new Texture2D(2, 2) { filterMode = FilterMode.Point };
        texture.LoadImage(bytes);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),
            1, 0, SpriteMeshType.FullRect, new Vector4(pBorder, pBorder, pBorder, pBorder));
        sprites[pName] = sprite;
        return sprite;
    }
}
