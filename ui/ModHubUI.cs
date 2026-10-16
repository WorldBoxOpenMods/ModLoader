using System.Collections;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Small helpers shared by the Mod Hub windows.
/// </summary>
internal static class ModHubUI
{
    private const string tooltip_title_key = "mod_hub_tooltip_title";
    private const string tooltip_text_key = "mod_hub_tooltip_text";

    public static readonly Color MutedTextColor = new(0.75f, 0.75f, 0.75f);

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
    ///     A plain text. Rich text is off because names, authors and descriptions come from mods.
    /// </summary>
    public static Text CreateText(Transform pParent, string pName, Vector2 pSize, Vector2 pPosition, int pFontSize,
        TextAnchor pAlignment)
    {
        var obj = new GameObject(pName, typeof(Text));
        obj.transform.SetParent(pParent, false);
        obj.transform.localPosition = pPosition;
        obj.GetComponent<RectTransform>().sizeDelta = pSize;

        Text text = obj.GetComponent<Text>();
        OT.InitializeCommonText(text);
        text.supportRichText = false;
        text.fontSize = pFontSize;
        text.alignment = pAlignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
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
    ///     An icon button with an optional tooltip, given as the locale key of its title.
    /// </summary>
    /// <remarks>
    ///     Pass a null click action when building a prefab: Unity does not copy listeners added in code to clones.
    /// </remarks>
    public static SimpleButton CreateIconButton(Transform pParent, string pName, Sprite pIcon, Vector2 pSize,
        Vector2 pPosition, UnityAction pOnClick, string pTooltipKey)
    {
        SimpleButton button = SimpleButton.Instantiate(pParent, pName: pName);
        button.Setup(pOnClick, pIcon, pSize: pSize);
        button.transform.localPosition = pPosition;
        if (pTooltipKey != null)
        {
            // Stored in serialized fields, so the tooltip also works on clones of a prefab.
            button.TipButton.enabled = true;
            button.TipButton.type = "normal";
            button.TipButton.textOnClick = pTooltipKey;
            button.TipButton.hoverAction = button.TipButton.showTooltipDefault;
        }

        return button;
    }

    /// <summary>
    ///     A text button whose label is the locale key <paramref name="pTextKey" />.
    /// </summary>
    public static SimpleButton CreateTextButton(Transform pParent, string pName, string pTextKey, Vector2 pSize,
        Vector2 pPosition, UnityAction pOnClick)
    {
        SimpleButton button = SimpleButton.Instantiate(pParent, pName: pName);
        button.Setup(pOnClick, null, LM.Get(pTextKey), pSize);
        SetLocaleKey(button.Text, pTextKey);
        button.transform.localPosition = pPosition;
        return button;
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

    /// <summary>
    ///     The game scrolls every window back to the top when it is shown. Coming back from a mod's page should
    ///     keep the player's place in a long list, so the list windows restore their position one frame later.
    /// </summary>
    public static IEnumerator RestoreScrollPosition(Transform pContent, float pY)
    {
        yield return null;
        Vector3 position = pContent.localPosition;
        position.y = pY;
        pContent.localPosition = position;
    }
}
