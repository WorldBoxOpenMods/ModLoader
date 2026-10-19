using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Small helpers shared by the Mod Hub windows.
/// </summary>
internal static class ModHubUI
{
    private const string tooltip_title_key = "mod_hub_tooltip_title";
    private const string tooltip_text_key = "mod_hub_tooltip_text";

    /// <summary>
    ///     The Mod Hub's windows, shown as tabs on the side of each of them. The title and description are the
    ///     locale keys "{id} Title" and "{id} Description".
    /// </summary>
    private static readonly (string WindowId, Func<Sprite> Icon)[] hub_windows =
    {
        ("ModManager", InternalResourcesGetter.GetIcon),
        ("ModBrowser", () => GameIcon("iconSteam"))
    };

    /// <summary>
    ///     Add a tab for each Mod Hub window to the side of <paramref name="pWindow" />.
    /// </summary>
    public static void AddHubTabs(Transform pWindow, string pCurrentId)
    {
        foreach ((string window_id, Func<Sprite> icon) in hub_windows)
        {
            bool current = window_id == pCurrentId;
            var tab = new GameObject(window_id + "Tab", typeof(Image), typeof(Button), typeof(TipButton));
            tab.transform.SetParent(pWindow.Find("Background/Tabs"), false);
            Image background = tab.GetComponent<Image>();
            background.sprite = pWindow.Find("BackButtonContainer/BackButtonBackground").GetComponent<Image>().sprite;
            background.type = Image.Type.Sliced;
            background.color = current ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            tab.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!current) SwitchTo(window_id);
            });
            TipButton tip = tab.GetComponent<TipButton>();
            tip.type = "normal";
            tip.override_click_scale_animation = true;
            tip.overridden_click_scale_animation = 1;
            var image = new GameObject("Icon", typeof(Image));
            image.transform.SetParent(tab.transform, false);
            image.GetComponent<Image>().sprite = icon();
            image.GetComponent<Image>().preserveAspect = true;
            image.GetComponent<Image>().raycastTarget = false;
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(18, 18);
            SetTextTooltip(tip, () => LM.Get(window_id + " Title"), () => LM.Get(window_id + " Description"));
        }
    }

    /// <summary>
    ///     Show another Hub window in place of the current one. The back button keeps going to the window before
    ///     the Hub, instead of through every tab clicked.
    /// </summary>
    private static void SwitchTo(string pWindowId)
    {
        // The game ignores window changes during its transition. Keep history intact in that case too.
        if (ScrollWindow.isAnimationActive()) return;
        List<WindowHistoryData> history = WindowHistory.list;
        if (history.Count > 0 && history[history.Count - 1].window == ScrollWindow.getCurrentWindow())
        {
            history.RemoveAt(history.Count - 1);
        }

        ScrollWindow.showWindow(pWindowId);
    }

    public static Sprite GameIcon(string pName)
    {
        return SpriteTextureLoader.getSprite("ui/icons/" + pName);
    }

    /// <summary>
    ///     Locale key for an enum value, such as "mod_hub_status_enabled".
    /// </summary>
    /// <remarks>
    ///     Keys are lower case because the game's tooltips convert keys with capitals to snake case before the lookup.
    /// </remarks>
    public static string LocaleKey(string pPrefix, Enum pValue)
    {
        return pPrefix + "_" + pValue.ToString().ToLowerInvariant();
    }

    /// <summary>
    ///     Show the locale key <paramref name="pKey" /> in <paramref name="pText" /> and follow language changes.
    /// </summary>
    public static void SetLocaleKey(Text pText, string pKey)
    {
        LocalizedText localized = pText.GetComponent<LocalizedText>();
        if (localized == null) localized = pText.gameObject.AddComponent<LocalizedText>();
        // It registers itself for language changes when it starts.
        localized.setKeyAndUpdate(pKey);
    }

    /// <summary>
    ///     Grey out a button that has nothing to do.
    /// </summary>
    public static void SetInteractable(SimpleButton pButton, bool pInteractable)
    {
        pButton.Button.interactable = pInteractable;
        pButton.Background.color = pInteractable ? Color.white : Color.gray;
    }

    /// <summary>
    ///     Show plain text when hovering <paramref name="pButton" />. The texts are read on every hover.
    /// </summary>
    public static void SetTextTooltip(SimpleButton pButton, Func<string> pTitle, Func<string> pText)
    {
        SetTextTooltip(pButton.TipButton, pTitle, pText);
    }

    /// <inheritdoc cref="SetTextTooltip(SimpleButton, Func{string}, Func{string})" />
    public static void SetTextTooltip(TipButton pTip, Func<string> pTitle, Func<string> pText)
    {
        pTip.enabled = true;
        pTip.setHoverAction(() => ShowTextTooltip(pTip.gameObject, pTitle(), pText()));
    }

    /// <summary>
    ///     "v1.0.0" for "1.0.0"; a version that already starts with "v" is kept as it is.
    /// </summary>
    public static string FormatVersion(string pVersion)
    {
        if (string.IsNullOrWhiteSpace(pVersion)) return "";
        pVersion = pVersion.Trim();
        return pVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? pVersion : "v" + pVersion;
    }

    /// <summary>
    ///     Show a tooltip with plain text, such as a mod name or an error message.
    /// </summary>
    /// <remarks>
    ///     The game's tooltips only display locale keys, so the text is passed through two reusable keys.
    /// </remarks>
    public static void ShowTextTooltip(GameObject pTarget, string pTitle, string pText)
    {
        LM.AddToCurrentLocale(tooltip_title_key, pTitle ?? "");
        LM.AddToCurrentLocale(tooltip_text_key, pText ?? "");
        Tooltip.show(pTarget, "normal", new TooltipData
        {
            tip_name = tooltip_title_key,
            tip_description = string.IsNullOrEmpty(pText) ? null : tooltip_text_key
        });
    }

    /// <summary>
    ///     Show the current choice of a toolbar button: its icon, and the tooltip line under its title.
    /// </summary>
    public static void ShowChoice(SimpleButton pButton, string pDescriptionKey, string pIconName)
    {
        pButton.Icon.sprite = GameIcon(pIconName);
        pButton.TipButton.textOnClickDescription = pDescriptionKey;
    }

    /// <summary>
    ///     The next value of an enum, back to the first after the last. For buttons that cycle through choices.
    /// </summary>
    public static T NextValue<T>(T pValue) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        return values[(Array.IndexOf(values, pValue) + 1) % values.Length];
    }

}
